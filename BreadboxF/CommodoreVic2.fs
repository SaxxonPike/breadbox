namespace BreadboxF

type CommodoreVic2Configuration(vBlankSet, cyclesPerRasterLine, rasterLinesPerFrame) =
    let rasterWidth = (cyclesPerRasterLine * 8)
    let rasterOffset = ((65 - cyclesPerRasterLine) * 8)
    let hBlankSet = 0x18C - rasterOffset
    let hBlankClear = 0x1E8 - (System.Math.Max(0, 64 - cyclesPerRasterLine) * 8)
    let vBlankClear = (vBlankSet + 28) % rasterLinesPerFrame
    let rasterOpsX = hBlankClear - 0x094 // 0x15C - rasterOffset 
    let rasterIncrement = hBlankClear - 0x04C
    let visibleRasterLines = rasterLinesPerFrame - 28
    let visiblePixelsPerRasterLine = rasterWidth - 100

    member val CyclesPerRasterLine = cyclesPerRasterLine
    member val HBlankSet = hBlankSet
    member val HBlankClear = hBlankClear
    member val VBlankSet = vBlankSet
    member val VBlankClear = vBlankClear
    member val RasterLinesPerFrame = rasterLinesPerFrame
    member val RasterOpsX = rasterOpsX
    member val RasterWidth = rasterWidth
    member val RasterIncrement = rasterIncrement
    member val VisibleRasterLines = visibleRasterLines
    member val VisiblePixelsPerRasterLine = visiblePixelsPerRasterLine

type CommodoreVic2ConfigurationFactory() =
    member this.CreateOldNtscConfiguration() = new CommodoreVic2Configuration(13, 64, 262)
    member this.CreateNewNtscConfiguration() = new CommodoreVic2Configuration(13, 65, 263)
    member this.CreatePalBConfiguration() = new CommodoreVic2Configuration(300, 63, 312)
    member this.CreatePalNConfiguration() = new CommodoreVic2Configuration(300, 65, 312)
    member this.CreatePalMConfiguration() = this.CreateNewNtscConfiguration()

type CommodoreVic2VideoOutput =
    struct
        val Pixel: int
        val VBlank: bool
        val HBlank: bool
        new (pixel, vblank, hblank) = {
            Pixel = pixel;
            VBlank = vblank;
            HBlank = hblank;
        }
    end

type CommodoreVic2VideoInterface =
    abstract member Output: CommodoreVic2VideoOutput -> unit

type CommodoreVic2ClockInterface =
    abstract member ClockPhi1: unit -> unit
    abstract member ClockPhi2: unit -> unit

type CommodoreVic2Chip(config:CommodoreVic2Configuration, memory:MemoryInterface, video:CommodoreVic2VideoInterface, clock:CommodoreVic2ClockInterface) = 


    // ========================================================================
    // Utility
    // ========================================================================

    let getValueFromBit index value =
        if value then 1 <<< index else 0

    let getValueFromBits(arr:bool[]) =
        (getValueFromBit 0 arr.[0]) |||
        (getValueFromBit 1 arr.[1]) |||
        (getValueFromBit 2 arr.[2]) |||
        (getValueFromBit 3 arr.[3]) |||
        (getValueFromBit 4 arr.[4]) |||
        (getValueFromBit 5 arr.[5]) |||
        (getValueFromBit 6 arr.[6]) |||
        (getValueFromBit 7 arr.[7])

    let setBitsFromValue(arr:bool[], value) =
        arr.[0] <- (value &&& 0x01) <> 0
        arr.[1] <- (value &&& 0x02) <> 0
        arr.[2] <- (value &&& 0x04) <> 0
        arr.[3] <- (value &&& 0x08) <> 0
        arr.[4] <- (value &&& 0x10) <> 0
        arr.[5] <- (value &&& 0x20) <> 0
        arr.[6] <- (value &&& 0x40) <> 0
        arr.[7] <- (value &&& 0x80) <> 0

    let clearBits(arr:bool[]) =
        arr.[0] <- false
        arr.[1] <- false
        arr.[2] <- false
        arr.[3] <- false
        arr.[4] <- false
        arr.[5] <- false
        arr.[6] <- false
        arr.[7] <- false
    

    // ========================================================================
    // Registers
    // ========================================================================


    // MnX (00, 02, 04, 06, 08, 0A, 0C, 0E, 10)
    let mobX = Array.zeroCreate 8
    let GetLowMobX index =
        mobX.[index] &&& 0xFF
    let GetHighMobX () =
        ((mobX.[0] &&& 0x100) >>> 8) |||
        ((mobX.[1] &&& 0x100) >>> 7) |||
        ((mobX.[2] &&& 0x100) >>> 6) |||
        ((mobX.[3] &&& 0x100) >>> 5) |||
        ((mobX.[4] &&& 0x100) >>> 4) |||
        ((mobX.[5] &&& 0x100) >>> 3) |||
        ((mobX.[6] &&& 0x100) >>> 2) |||
        ((mobX.[7] &&& 0x100) >>> 1)
    let SetLowMobX index value =
        mobX.[index] <- (mobX.[index] &&& 0x100) ||| (value &&& 0xFF)
    let SetHighMobX value =
        mobX.[0] <- (mobX.[0] &&& 0x0FF) ||| (if value &&& 0x01 <> 0 then 0x100 else 0x000)
        mobX.[1] <- (mobX.[1] &&& 0x0FF) ||| (if value &&& 0x02 <> 0 then 0x100 else 0x000)
        mobX.[2] <- (mobX.[2] &&& 0x0FF) ||| (if value &&& 0x04 <> 0 then 0x100 else 0x000)
        mobX.[3] <- (mobX.[3] &&& 0x0FF) ||| (if value &&& 0x08 <> 0 then 0x100 else 0x000)
        mobX.[4] <- (mobX.[4] &&& 0x0FF) ||| (if value &&& 0x10 <> 0 then 0x100 else 0x000)
        mobX.[5] <- (mobX.[5] &&& 0x0FF) ||| (if value &&& 0x20 <> 0 then 0x100 else 0x000)
        mobX.[6] <- (mobX.[6] &&& 0x0FF) ||| (if value &&& 0x40 <> 0 then 0x100 else 0x000)
        mobX.[7] <- (mobX.[7] &&& 0x0FF) ||| (if value &&& 0x80 <> 0 then 0x100 else 0x000)

    // MxY (01, 03, 05, 07, 09, 0B, 0D, 0F)
    let mobY = Array.zeroCreate 8
    let GetMobY index =
        mobY.[index]
    let SetMobY index value =
        mobY.[index] <- value &&& 0xFF

    // RASTER/RST8 (11, 12) (high expects high bit in position 7 as if you wrote to the reg)
    let mutable rasterYCompareValue = 0
    let SetLowRasterYCompareValue value =
        rasterYCompareValue <- (rasterYCompareValue &&& 0x100) ||| value
    let mutable rasterY =
        config.RasterLinesPerFrame - 1
    let nextRasterY =
        Array.init config.RasterLinesPerFrame (fun y -> if y >= (config.RasterLinesPerFrame - 1) then 0 else (y + 1))
    let GetLowRasterY () =
        rasterY &&& 0x0FF

    // Control register 1 (11) (RST8/ECM/BMM/DEN/RSEL/YSCROLL)
    let mutable extraColorMode = false
    let mutable bitmapMode = false
    let mutable displayEnabled = false
    let mutable rowSelect = false
    let mutable yScroll = 0
    let GetControlRegister1 () =
        ((rasterY &&& 0x100) >>> 1) |||
        (if extraColorMode then 0x40 else 0x00) |||
        (if bitmapMode then 0x20 else 0x00) |||
        (if displayEnabled then 0x10 else 0x00) |||
        (if rowSelect then 0x08 else 0x00) |||
        yScroll
    let SetControlRegister1 value =
        rasterYCompareValue <- (rasterYCompareValue &&& 0x0FF) ||| ((value &&& 0x80) <<< 1)
        extraColorMode <- (value &&& 0x40 <> 0x00)
        bitmapMode <- (value &&& 0x20 <> 0x00)
        displayEnabled <- (value &&& 0x10 <> 0x00)
        rowSelect <- (value &&& 0x08 <> 0x00)
        yScroll <- (value &&& 0x07)

    // LPX (13) (it's a 9 bit register but we only keep the upper 8)
    let mutable lightPenX = 0
    let GetLightPenX () =
        lightPenX
    let SetLightPenX value =
        lightPenX <- value >>> 1

    // LPY (14)
    let mutable lightPenY = 0
    let GetLightPenY () =
        lightPenY
    let SetLightPenY value =
        lightPenY <- value

    // MnE (15)
    let mobEnabled = Array.zeroCreate 8
    let GetMobEnable () =
        getValueFromBits(mobEnabled)
    let SetMobEnable value =
        setBitsFromValue(mobEnabled, value)
        
    // Control Register 2 (16)
    let mutable res = false
    let mutable multiColorMode = false
    let mutable columnSelect = false
    let mutable xScroll = 0
    let GetControlRegister2 () =
        0xC0 |||
        (if res then 0x20 else 0x00) |||
        (if multiColorMode then 0x10 else 0x00) |||
        (if columnSelect then 0x08 else 0x00) |||
        xScroll
    let SetControlRegister2 value =
        res <- (value &&& 0x20) <> 0x00
        multiColorMode <- (value &&& 0x10) <> 0x00
        columnSelect <- (value &&& 0x08) <> 0x00
        xScroll <- (value &&& 0x07)

    // Mob Y Expansion (17)
    let mobYExpansionEnabled = Array.zeroCreate 8
    let mobYExpansionToggle = Array.zeroCreate 8
    let GetMobYExpansionEnable () =
        getValueFromBits(mobYExpansionEnabled)
    let SetMobYExpansionEnable value =
        setBitsFromValue(mobYExpansionEnabled, value)

    // Memory Pointers (18) (VM and CB are pre-shifted on store for speed)
    let mutable videoMemoryPointer = 0
    let mutable characterBankPointer = 0
    let GetMemoryPointers () =
        (videoMemoryPointer >>> 6) |||
        (characterBankPointer >>> 10) |||
        0x01
    let SetMemoryPointers value =
        videoMemoryPointer <- (value &&& 0xF0) <<< 6
        characterBankPointer <- (value &&& 0x0E) <<< 10

    // Interrupt Register (19) and Interrupt Enable Register (1A)
    let mutable irq = false
    let mutable lightPenIrq = false
    let mutable mobMobCollisionIrq = false
    let mutable mobBackgroundCollisionIrq = false
    let mutable rasterIrq = false
    let mutable lightPenIrqEnabled = false
    let mutable mobMobCollisionIrqEnabled = false
    let mutable mobBackgroundCollisionIrqEnabled = false
    let mutable rasterIrqEnabled = false
    let GetInterruptRegister () =
        (if irq then 0x80 else 0x00) |||
        (if lightPenIrq then 0x08 else 0x00) |||
        (if mobMobCollisionIrq then 0x04 else 0x00) |||
        (if mobBackgroundCollisionIrq then 0x02 else 0x00) |||
        (if rasterIrq then 0x01 else 0x00)
    let GetInterruptEnable () =
        (if lightPenIrqEnabled then 0x08 else 0x00) |||
        (if mobMobCollisionIrqEnabled then 0x04 else 0x00) |||
        (if mobBackgroundCollisionIrqEnabled then 0x02 else 0x00) |||
        (if rasterIrqEnabled then 0x01 else 0x00)
    let SetInterruptRegister value =
        lightPenIrq <- (lightPenIrq && ((value &&& 0x08) = 0))
        mobMobCollisionIrq <- (mobMobCollisionIrq && ((value &&& 0x04) = 0))
        mobBackgroundCollisionIrq <- (mobBackgroundCollisionIrq && ((value &&& 0x02) = 0))
        rasterIrq <- (rasterIrq && ((value &&& 0x01) = 0))
    let SetInterruptEnable value =
        lightPenIrqEnabled <- (value &&& 0x08) <> 0
        mobMobCollisionIrqEnabled <- (value &&& 0x04) <> 0
        mobBackgroundCollisionIrq <- (value &&& 0x02) <> 0
        rasterIrqEnabled <- (value &&& 0x01) <> 0
    let UpdateIrq () =
        irq <- (lightPenIrq && lightPenIrqEnabled) ||
            (mobMobCollisionIrq && mobMobCollisionIrqEnabled) ||
            (mobBackgroundCollisionIrq && mobBackgroundCollisionIrqEnabled) ||
            (rasterIrq && rasterIrqEnabled)

    // Sprite Data Priority (1B)
    let mobDataPriority = Array.zeroCreate 8
    let GetMobDataPriority () =
        getValueFromBits(mobDataPriority)
    let SetMobDataPriority value =
        setBitsFromValue(mobDataPriority, value)

    // Sprite Multicolor Enable (1C)
    let mobMultiColorEnabled = Array.zeroCreate 8
    let mobMultiColorToggle = Array.zeroCreate 8
    let GetMobMultiColorEnable () =
        getValueFromBits(mobMultiColorEnabled)
    let SetMobMultiColorEnable value =
        setBitsFromValue(mobMultiColorEnabled, value)
    
    // Sprite X Expansion Enable (1D)
    let mobXExpansionEnabled = Array.zeroCreate 8
    let mobXExpansionToggle = Array.zeroCreate 8
    let GetMobXExpansionEnable () =
        getValueFromBits(mobXExpansionEnabled)
    let SetMobXExpansionEnable value =
        setBitsFromValue(mobXExpansionEnabled, value)

    // Sprite-sprite Collision (1E)
    let mutable mobMobFirstCollidedIndex = -1
    let mutable mobMobCollisionOccurred = false
    let mobMobCollision = Array.zeroCreate 8
    let GetMobMobCollision () =
        getValueFromBits(mobMobCollision)
    let ClearAndGetMobMobCollision () =
        let oldValue = GetMobMobCollision()
        clearBits(mobMobCollision)
        mobMobFirstCollidedIndex <- -1
        oldValue

    // Sprite-background Collision (1F)
    let mobDataCollisionOccurred = false
    let mobDataCollision = Array.zeroCreate 8
    let GetMobDataCollision () =
        getValueFromBits(mobDataCollision)
    let ClearAndGetMobDataCollision () =
        let oldValue = GetMobDataCollision()
        clearBits(mobDataCollision)
        oldValue

    // Border Color (20)
    let mutable borderColor = 0
    let GetBorderColor () =
        borderColor ||| 0xF0
    let SetBorderColor value =
        borderColor <- value &&& 0x0F

    // Background Colors (21, 22, 23, 24)
    let backgroundColor = Array.zeroCreate 4
    let GetBackgroundColor index =
        backgroundColor.[index] ||| 0xF0
    let SetBackgroundColor index value =
        backgroundColor.[index] <- value &&& 0x0F

    // Sprite Multicolors (25, 26)
    let mobMultiColor = Array.zeroCreate 2
    let GetMobMultiColor index =
        mobMultiColor.[index] ||| 0xF0
    let SetMobMultiColor index value =
        mobMultiColor.[index] <- value &&& 0x0F

    // Sprite Colors (27, 28, 29, 2A, 2B, 2C, 2D, 2E)
    let mobColor = Array.zeroCreate 8
    let GetMobColor index =
        mobColor.[index] ||| 0xF0
    let SetMobColor index value =
        mobColor.[index] <- value &&& 0x0F


    // ========================================================================
    // Internals
    // ========================================================================


    // Raster Line Counter
    // - This determines the Raster X position as well as all horizontal timed operations.
    let rasterX = Array.init config.RasterWidth (fun counter ->
        if config.RasterWidth <= 0x200 || counter <= 0x18C then
            counter
        else
            let extraCycles = config.RasterWidth - 0x200
            let adjustedCycle = counter - extraCycles
            if adjustedCycle < 0x18C then
                0x18C
            else
                adjustedCycle
        )
    let nextRasterLineCounter = Array.init config.RasterWidth (fun x -> if x >= (config.RasterWidth - 1) then 0 else (x + 1))

    let mutable rasterLineCounter = config.RasterIncrement - 1
    let GetRasterX () =
        rasterX.[rasterLineCounter]

    // Blanking
    // - The circuitry outputs black when blanked, so there's no need to render pixels.
    let vBlankY = Array.init config.RasterLinesPerFrame (fun y ->
        if config.VBlankSet < config.VBlankClear then
            y >= config.VBlankSet && y < config.VBlankClear
        else
            y >= config.VBlankSet || y < config.VBlankClear
        )
    let hBlankX = Array.init config.RasterWidth (fun x -> rasterX.[x] >= config.HBlankSet && rasterX.[x] < config.HBlankClear)

    // Clocks
    let IsPhi0 () =
        rasterLineCounter &&& 0x4 <> 0
    let IsPhi1 () =
        rasterLineCounter &&& 0x4 = 0

    // Memory Access Cycles (BA for sprite 0 begins the sequence)
    // - This determines which memory accesses need to happen.
    //   0  First sprite BA
    //   6  First sprite fetch
    //  38  First R fetch
    //  40  Character BA
    //  46  Last R fetch
    //  47  First C fetch
    //  48  First G fetch
    // 125  Last C fetch
    // 126  Last G fetch (NOTE: this overlaps 0 on PAL systems)
    let memoryAccessCycle = Array.init config.RasterWidth (fun counter ->
        if (counter % 4 <> 0) then
            -1
        else
            (((config.RasterWidth - config.RasterOpsX) + counter) % config.RasterWidth) / 4
        )
    let GetMemoryAccessCycle () =
        memoryAccessCycle.[rasterLineCounter]

    // RC
    let mutable rowCounter = 0

    let IncrementRowCounter () =
        rowCounter <- (rowCounter + 1) &&& 0x7
    let ResetRowCounter () =
        rowCounter <- 0

    // VC, VCBASE, VMLI, VMLM
    let mutable videoCounter = 0
    let mutable videoCounterBase = 0
    let mutable videoMatrixLineIndex = 0
    let videoMatrixLineMemory = Array.zeroCreate 40

    // Graphics shift registers
    let mutable graphicsShiftRegister = 0
    let mutable graphicsShiftRegisterColor = 0
    let mutable graphicsShiftRegisterOutput = 0
    let mutable graphicsShiftRegisterMultiColorToggle = false
    let mutable graphicsReadC = 0
    let mutable graphicsReadG = 0
    let mutable graphicsPendingC = 0
    let mutable graphicsPendingG = 0

    // MC, MCBASE, MDMA, MDC, MP
    let mobCounter = Array.zeroCreate 8
    let mobCounterBase = Array.zeroCreate 8
    let mobDma = Array.zeroCreate 8
    let mobDataCrunch = Array.zeroCreate 8
    let mobPointer = Array.zeroCreate 8
    let mobDisplay = Array.zeroCreate 8

    // Sprite shift registers
    let mobShiftRegister = Array.zeroCreate 8
    let mobShiftRegisterEnable = Array.zeroCreate 8
    let mobShiftRegisterOutput = Array.zeroCreate 8

    // Display/Idle state
    let mutable displayState = false

    // Refresh counter
    let mutable refreshCounter = 0

    // Bad lines
    let mutable badLinesEnabled = false
    let mutable badLine = false

    // AEC and BA
    let mutable aec = false
    let mutable ba = true
    let mutable baCounter = 24

    // Border Unit
    let mutable borderVerticalEnabled = true
    let mutable borderMainEnabled = true
    let mutable borderEnableDelay = -1

    // Light Pen
    let mutable lightPenTriggeredThisFrame = false


    // ========================================================================
    // Internal Units
    // ========================================================================


    // Raster Counter
    // (PAL cycle to MAC table)
    // 55: 0   G-   63: 16  SS  8:  32  SS  16: 48  GC  24: 64  GC  32: 80  GC  40: 96  GC  48: 112 GC
    // 56: 2   I-   1:  18  PS  9:  34  PS  17: 50  GC  25: 66  GC  33: 82  GC  41: 98  GC  49: 114 GC
    // 57: 4   I-   2:  20  SS  10: 36  SS  18: 52  GC  26: 68  GC  34: 84  GC  42: 100 GC  50: 116 GC
    // 58: 6   PS   3:  22  PS  11: 38  R-  19: 54  GC  27: 70  GC  35: 86  GC  43: 102 GC  51: 118 GC
    // 59: 8   SS   4:  24  SS  12: 40  R-  20: 56  GC  28: 72  GC  36: 88  GC  44: 104 GC  52: 120 GC
    // 60: 10  PS   5:  26  PS  13: 42  R-  21: 58  GC  29: 74  GC  37: 90  GC  45: 106 GC  53: 122 GC
    // 61: 12  SS   6:  28  SS  14: 44  R-  22: 60  GC  30: 76  GC  38: 92  GC  46: 108 GC  54: 124 GC
    // 62: 14  PS   7:  30  PS  15: 46  RC  23: 62  GC  31: 78  GC  39: 94  GC  47: 110 GC
    let ClockRasterCounter mac rasterX =
        if rasterLineCounter = config.RasterIncrement then
            rasterY <- nextRasterY.[rasterY]
            if rasterY = 0x0F8 then
                badLinesEnabled <- false
                badLine <- false
            if rasterY = 0x000 then
                videoCounterBase <- 0
                lightPenTriggeredThisFrame <- false

        if (rasterY = 0x030) && displayEnabled then
            badLinesEnabled <- true

        let lastBadLine = badLine
        badLine <- (badLinesEnabled) && (yScroll = (rasterY &&& 0x7))
        if badLine && (not lastBadLine) then
            displayState <- true

        match mac with
            | 0 | 2 ->
                // cycle 55-56
                if mac = 0 then
                    let toggleExpansionIfEnabled index =
                        mobYExpansionToggle.[index] <- mobYExpansionToggle.[index] <> mobYExpansionEnabled.[index]
                    toggleExpansionIfEnabled 0
                    toggleExpansionIfEnabled 1
                    toggleExpansionIfEnabled 2
                    toggleExpansionIfEnabled 3
                    toggleExpansionIfEnabled 4
                    toggleExpansionIfEnabled 5
                    toggleExpansionIfEnabled 6
                    toggleExpansionIfEnabled 7
                let checkSpriteEnable (index) =
                    if mobEnabled.[index] && (mobY.[index] &&& 0x7) = (rasterY &&& 0x7) then
                        if not mobDma.[index] then
                            mobDma.[index] <- true
                            mobCounterBase.[index] <- 0
                            mobYExpansionToggle.[index] <- mobYExpansionToggle.[index] && (not mobYExpansionEnabled.[index])
                checkSpriteEnable 0
                checkSpriteEnable 1
                checkSpriteEnable 2
                checkSpriteEnable 3
                checkSpriteEnable 4
                checkSpriteEnable 5
                checkSpriteEnable 6
                checkSpriteEnable 7
            | 6 ->
                // cycle 58
                if rowCounter = 7 then
                    displayState <- false
                    videoCounterBase <- videoCounter
                if displayState then
                    IncrementRowCounter()
                let reloadMobCounter (index) =
                    mobShiftRegisterEnable.[index] <- false
                    mobCounter.[index] <- mobCounterBase.[index]
                    if mobDma.[index] then
                        if (mobY.[index] &&& 0x7) = (rasterY &&& 0x7) then
                            mobDisplay.[index] <- true
                    else
                        mobDisplay.[index] <- false
                reloadMobCounter(0)
                reloadMobCounter(1)
                reloadMobCounter(2)
                reloadMobCounter(3)
                reloadMobCounter(4)
                reloadMobCounter(5)
                reloadMobCounter(6)
                reloadMobCounter(7)
            | 44 ->
                // cycle 14
                videoCounter <- videoCounterBase
                videoMatrixLineIndex <- 0
                if badLine then
                    rowCounter <- 0
            | 48 ->
                // cycle 16
                let checkSpriteCrunch (index) =
                    if mobYExpansionToggle.[index] then
                        mobCounterBase.[index] <-
                            if mobDataCrunch.[index] then
                                (0x2A &&& (mobCounterBase.[index] &&& mobCounter.[index])) ||| (0x15 &&& (mobCounterBase.[index] ||| mobCounter.[index]))
                            else
                                mobCounter.[index]
                        if mobCounterBase.[index] = 63 then
                            mobDma.[index] <- false
                checkSpriteCrunch(0)
                checkSpriteCrunch(1)
                checkSpriteCrunch(2)
                checkSpriteCrunch(3)
                checkSpriteCrunch(4)
                checkSpriteCrunch(5)
                checkSpriteCrunch(6)
                checkSpriteCrunch(7)
            | _ -> ()

    // Memory Interface, Address Generator, Refresh Counter
    let ClockMemoryInterface (mac) =
        let readP (index) =
            mobPointer.[index] <- (memory.Read(videoMemoryPointer ||| 0x3F8 ||| index) &&& 0xFF) <<< 6

        let readS (index, counter) =
            if mobDma.[index] then
                mobShiftRegister.[index] <- (mobShiftRegister.[index] <<< 8) ||| (memory.Read(mobCounter.[index] ||| mobPointer.[index]) &&& 0xFF)
                mobCounter.[index] <- mobCounter.[index] + 1
            else
                if counter = 1 then
                    memory.Read(0x3FFF) |> ignore

        let readC () =
            if badLine then
                graphicsReadC <- memory.Read(videoMemoryPointer ||| videoCounter)
                if videoMatrixLineIndex < 40 then
                    videoMatrixLineMemory.[videoMatrixLineIndex] <- graphicsReadC
            else
                memory.Read(0x3FFF) |> ignore

        let readG () =
            graphicsReadG <-
                memory.Read((if extraColorMode then 0x39FF else 0x3FFF) &&&
                    if (displayState) then
                        if (bitmapMode) then
                            (characterBankPointer &&& 0x2000) ||| (videoCounter <<< 3) ||| rowCounter
                        else
                            let videoMatrixLineMemory =
                                if videoMatrixLineIndex < 40 then
                                    videoMatrixLineMemory.[videoMatrixLineIndex] else 0
                            characterBankPointer ||| ((videoMatrixLineMemory &&& 0xFF) <<< 3) ||| rowCounter
                    else
                        0x3FFF
                ) &&& 0xFF
            videoCounter <- (videoCounter + 1) &&& 0x3FF
            videoMatrixLineIndex <- (videoMatrixLineIndex + 1) &&& 0x3F

        let readI () =
            memory.Read(0x3FFF) |> ignore

        let readR () =
            refreshCounter <- (refreshCounter - 1) &&& 0xFF
            memory.Read(0x3F00 ||| refreshCounter) |> ignore
        match mac with
            | -1 -> ()
            |  0 -> if (config.CyclesPerRasterLine < 64) then readG() else readI()
            |  2 |  4 -> readI()
            |  6 -> readP(0)
            |  7 -> readS(0, 0)
            |  8 -> readS(0, 1)
            |  9 -> readS(0, 2)
            | 10 -> readP(1)
            | 11 -> readS(1, 0)
            | 12 -> readS(1, 1)
            | 13 -> readS(1, 2)
            | 14 -> readP(2)
            | 15 -> readS(2, 0)
            | 16 -> readS(2, 1)
            | 17 -> readS(2, 2)
            | 18 -> readP(3)
            | 19 -> readS(3, 0)
            | 20 -> readS(3, 1)
            | 21 -> readS(3, 2)
            | 22 -> readP(4)
            | 23 -> readS(4, 0)
            | 24 -> readS(4, 1)
            | 25 -> readS(4, 2)
            | 26 -> readP(5)
            | 27 -> readS(5, 0)
            | 28 -> readS(5, 1)
            | 29 -> readS(5, 2)
            | 30 -> readP(6)
            | 31 -> readS(6, 0)
            | 32 -> readS(6, 1)
            | 33 -> readS(6, 2)
            | 34 -> readP(7)
            | 35 -> readS(7, 0)
            | 36 -> readS(7, 1)
            | 37 -> readS(7, 2)
            | 38 | 40 | 42 | 44 | 46 -> readR()
            | x ->
                if (x &&& 1 = 0) then
                    if (x >= 48 && x < 128) then
                        readG()
                    else
                        readI()
                else
                    if (x >= 47 && x < 127) then
                        readC()

    let ClockBaAec(mac) =
        ba <-
            if mac = -1 then
                ba
            else
                let IsBadLineBa =
                    badLine && mac >= 40 && mac < 126
                let IsSpriteBa (index) =
                    let lowerBound = index * 4
                    let upperBound = lowerBound + 10
                    mac >= lowerBound && mac < upperBound && mobDma.[index]
                let IsAnySpriteBa () =
                    IsSpriteBa(0) ||
                    IsSpriteBa(1) ||
                    IsSpriteBa(2) ||
                    IsSpriteBa(3) ||
                    IsSpriteBa(4) ||
                    IsSpriteBa(5) ||
                    IsSpriteBa(6) ||
                    IsSpriteBa(7)
                not (IsBadLineBa || IsAnySpriteBa())
        baCounter <-
            if ba then
                24
            else
                if (baCounter > 0) then
                    baCounter - 1
                else
                    0
        aec <- baCounter > 0 && IsPhi0()

    let ClockIrq() =
        UpdateIrq()

    let ClockSprite (index, rasterX) =
        let display = mobDisplay.[index]
        if display then
            let expandX = mobXExpansionEnabled.[index]
            if not mobShiftRegisterEnable.[index] then
                if mobX.[index] = rasterX then
                    mobShiftRegisterEnable.[index] <- true
            if mobShiftRegisterEnable.[index] then
                let multiColor = mobMultiColorEnabled.[index]
                if mobXExpansionToggle.[index] then
                    if mobMultiColorToggle.[index] then
                        mobShiftRegisterOutput.[index] <- (if multiColor then 0xC00000 else 0x800000)
                        mobShiftRegister.[index] <- mobShiftRegister.[index] <<< (if multiColor then 2 else 1)
                    mobMultiColorToggle.[index] <- (not multiColor) || (mobMultiColorToggle.[index] <> multiColor)
                mobXExpansionToggle.[index] <- (not expandX) || (mobXExpansionToggle.[index] <> expandX)

    let ClockSprites (rasterX) =
        ClockSprite(0, rasterX)
        ClockSprite(1, rasterX)
        ClockSprite(2, rasterX)
        ClockSprite(3, rasterX)
        ClockSprite(4, rasterX)
        ClockSprite(5, rasterX)
        ClockSprite(6, rasterX)
        ClockSprite(7, rasterX)

    let ClockGraphics (mac, rasterX) =
        graphicsShiftRegisterMultiColorToggle <- not graphicsShiftRegisterMultiColorToggle
        if (mac >= 0) && (mac &&& 1 = 1) then
            graphicsPendingC <- graphicsReadC
            graphicsPendingG <- graphicsReadG
            graphicsReadC <- 0
            graphicsReadG <- 0
        if xScroll = (rasterX &&& 0x7) then
            graphicsShiftRegister <- graphicsPendingG
            graphicsShiftRegisterColor <- graphicsPendingC
            graphicsShiftRegisterMultiColorToggle <- false
        let multiColor =
            multiColorMode && (bitmapMode || (graphicsShiftRegisterColor &&& 0x800 <> 0))
        if (not multiColor) || graphicsShiftRegisterMultiColorToggle then
            graphicsShiftRegisterOutput <- graphicsShiftRegister &&& (if multiColor then 0xC0 else 0x80)
            graphicsShiftRegister <- graphicsShiftRegister <<< (if multiColor then 2 else 1)

    let ClockBorder (mac) =
        let leftCompare = if columnSelect then 0x018 else 0x01F
        let rightCompare = if columnSelect then 0x158 else 0x14F
        let topCompare = if rowSelect then 0x033 else 0x037
        let bottomCompare = if rowSelect then 0x0FB else 0x0F7
        let isLastCycle = (mac = 16)

        if (rasterLineCounter = rightCompare) then
            borderMainEnabled <- true
        if (rasterY = bottomCompare && isLastCycle) then
            borderVerticalEnabled <- true
        else
            if (displayEnabled && rasterY = topCompare && isLastCycle) then
                borderVerticalEnabled <- false
            else
                if (rasterLineCounter = leftCompare && rasterY = bottomCompare) then
                    borderVerticalEnabled <- true
                else
                    if (displayEnabled && rasterY = topCompare && rasterLineCounter = leftCompare) then
                        borderVerticalEnabled <- false
        if ((not borderVerticalEnabled) && rasterLineCounter = leftCompare) then
            borderMainEnabled <- false
        borderEnableDelay <- (borderEnableDelay <<< 1) ||| (if (borderMainEnabled || borderVerticalEnabled) then 1 else 0)

    let ClockPixel () =
        if (borderEnableDelay &&& 0x100) <> 0 then
            borderColor
        else
            mobMobFirstCollidedIndex <- -1
            let outputForSprite(index) =
                match mobShiftRegisterOutput.[index] with
                    | 0x000000 -> -1
                    | bits ->
                        if mobMobFirstCollidedIndex = -1 then
                            mobMobFirstCollidedIndex <- index
                        else
                            mobMobCollision.[index] <- true
                            mobMobCollision.[mobMobFirstCollidedIndex] <- true
                            if not mobMobCollisionOccurred then
                                mobMobCollisionIrq <- true
                                mobMobCollisionOccurred <- true
                        if (not borderVerticalEnabled) && (graphicsShiftRegisterOutput >= 0x80) then
                            mobDataCollision.[index] <- true
                        match bits with
                            | 0x400000 -> mobMultiColor.[0]
                            | 0xC00000 -> mobMultiColor.[1]
                            | _ -> mobColor.[index]
            let spriteOutput0 = outputForSprite(0)
            let spriteOutput1 = outputForSprite(1)
            let spriteOutput2 = outputForSprite(2)
            let spriteOutput3 = outputForSprite(3)
            let spriteOutput4 = outputForSprite(4)
            let spriteOutput5 = outputForSprite(5)
            let spriteOutput6 = outputForSprite(6)
            let spriteOutput7 = outputForSprite(7)
            let graphicsOutput =
                if (extraColorMode && (bitmapMode || multiColorMode)) then
                    0
                else
                    match graphicsShiftRegisterOutput with
                        | 0x40 ->
                            if bitmapMode then
                                (graphicsShiftRegisterColor >>> 4) &&& 0x00F
                            else
                                backgroundColor.[1]
                        | 0x80 ->
                            if bitmapMode then
                                if multiColorMode then
                                    graphicsShiftRegisterColor &&& 0x00F
                                else
                                    (graphicsShiftRegisterColor >>> 4) &&& 0x00F
                            else
                                if multiColorMode then
                                    backgroundColor.[2]
                                else
                                    (graphicsShiftRegisterColor >>> 8) &&& 0x00F
                        | 0xC0 ->
                            (graphicsShiftRegisterColor >>> 8) &&& (if bitmapMode then 0x00F else 0x007)
                        | _ ->
                            if extraColorMode then
                                backgroundColor.[(graphicsShiftRegisterColor >>> 6) &&& 0x003]
                            else
                                if bitmapMode && not multiColorMode then
                                    graphicsShiftRegisterColor &&& 0x00F
                                else
                                    backgroundColor.[0]
            let spriteOutput =
                if spriteOutput0 = -1 then
                    if spriteOutput1 = -1 then
                        if spriteOutput2 = -1 then
                            if spriteOutput3 = -1 then
                                if spriteOutput4 = -1 then
                                    if spriteOutput5 = -1 then
                                        if spriteOutput6 = -1 then
                                            spriteOutput7
                                        else
                                            spriteOutput6
                                    else
                                        spriteOutput5
                                else
                                    spriteOutput4
                            else
                                spriteOutput3
                        else
                            spriteOutput2
                    else
                        spriteOutput1
                else
                    spriteOutput0
            if mobMobFirstCollidedIndex >= 0 then
                if (graphicsShiftRegisterOutput >= 0x80) || (not mobDataPriority.[mobMobFirstCollidedIndex]) then
                    spriteOutput
                else
                    graphicsOutput
            else
                graphicsOutput

    // ========================================================================
    // Process
    // ========================================================================


    member this.Clock () =
        rasterLineCounter <- nextRasterLineCounter.[rasterLineCounter]

        let mac = GetMemoryAccessCycle()
        let rasterX = GetRasterX()

        ClockRasterCounter mac rasterX
        ClockBorder(mac)
        ClockBaAec(mac)
        ClockMemoryInterface(mac)
        ClockSprites(rasterX)
        ClockGraphics(mac, rasterX)
        video.Output(new CommodoreVic2VideoOutput(ClockPixel(), vBlankY.[rasterY], hBlankX.[rasterLineCounter]))
        ClockIrq()

        match (rasterLineCounter &&& 0x7) with
            | 0 -> clock.ClockPhi1()
            | 4 -> clock.ClockPhi2()
            | _ -> ()

    member this.ClockMultiple (count) =
        let mutable i = count
        while i > 0 do
            this.Clock()
            i <- i - 1

    member this.ClockRaster() =
        this.ClockMultiple(config.RasterWidth)

    member this.ClockToCounterX(counter) =
        let actualCounter = counter % config.RasterWidth
        while rasterLineCounter <> actualCounter do
            this.Clock()

    member this.ClockToRasterY(raster) =
        let actualRaster = raster % config.RasterLinesPerFrame
        while rasterY <> actualRaster do
            this.Clock()

    member this.ClockTo(counter, raster) =
        this.ClockToRasterY(raster)
        this.ClockToCounterX(counter)

    member this.ClockFrame () =
        this.ClockMultiple(config.RasterWidth * config.RasterLinesPerFrame)


    // ========================================================================
    // Interface
    // ========================================================================


    // Register Access
    member this.PeekRegister address =
        match (address &&& 0x3F) with
            | 0x00 -> GetLowMobX(0)
            | 0x01 -> GetMobY(0)
            | 0x02 -> GetLowMobX(1)
            | 0x03 -> GetMobY(1)
            | 0x04 -> GetLowMobX(2)
            | 0x05 -> GetMobY(2)
            | 0x06 -> GetLowMobX(3)
            | 0x07 -> GetMobY(3)
            | 0x08 -> GetLowMobX(4)
            | 0x09 -> GetMobY(4)
            | 0x0A -> GetLowMobX(5)
            | 0x0B -> GetMobY(5)
            | 0x0C -> GetLowMobX(6)
            | 0x0D -> GetMobY(6)
            | 0x0E -> GetLowMobX(7)
            | 0x0F -> GetMobY(7)
            | 0x10 -> GetHighMobX()
            | 0x11 -> GetControlRegister1()
            | 0x12 -> GetLowRasterY()
            | 0x13 -> GetLightPenX()
            | 0x14 -> GetLightPenY()
            | 0x15 -> GetMobEnable()
            | 0x16 -> GetControlRegister2()
            | 0x17 -> GetMobYExpansionEnable()
            | 0x18 -> GetMemoryPointers()
            | 0x19 -> GetInterruptRegister()
            | 0x1A -> GetInterruptEnable()
            | 0x1B -> GetMobDataPriority()
            | 0x1C -> GetMobMultiColorEnable()
            | 0x1D -> GetMobXExpansionEnable()
            | 0x1E -> GetMobMobCollision()
            | 0x1F -> GetMobDataCollision()
            | 0x20 -> GetBorderColor()
            | 0x21 -> GetBackgroundColor(0)
            | 0x22 -> GetBackgroundColor(1)
            | 0x23 -> GetBackgroundColor(2)
            | 0x24 -> GetBackgroundColor(3)
            | 0x25 -> GetMobMultiColor(0)
            | 0x26 -> GetMobMultiColor(1)
            | 0x27 -> GetMobColor(0)
            | 0x28 -> GetMobColor(1)
            | 0x29 -> GetMobColor(2)
            | 0x2A -> GetMobColor(3)
            | 0x2B -> GetMobColor(4)
            | 0x2C -> GetMobColor(5)
            | 0x2D -> GetMobColor(6)
            | 0x2E -> GetMobColor(7)
            | _ -> 0xFF

    member this.PokeRegister address value =
        match address &&& 0x3F with
            | 0x00 -> SetLowMobX 0 value
            | 0x01 -> SetMobY 0 value
            | 0x02 -> SetLowMobX 1 value
            | 0x03 -> SetMobY 1 value
            | 0x04 -> SetLowMobX 2 value
            | 0x05 -> SetMobY 2 value
            | 0x06 -> SetLowMobX 3 value
            | 0x07 -> SetMobY 3 value
            | 0x08 -> SetLowMobX 4 value
            | 0x09 -> SetMobY 4 value
            | 0x0A -> SetLowMobX 5 value
            | 0x0B -> SetMobY 5 value
            | 0x0C -> SetLowMobX 6 value
            | 0x0D -> SetMobY 6 value
            | 0x0E -> SetLowMobX 7 value
            | 0x0F -> SetMobY 7 value
            | 0x10 -> SetHighMobX value
            | 0x11 -> SetControlRegister1 value
            | 0x12 -> SetLowRasterYCompareValue value
            | 0x15 -> SetMobEnable value
            | 0x16 -> SetControlRegister2 value
            | 0x17 -> SetMobYExpansionEnable value
            | 0x18 -> SetMemoryPointers value
            | 0x19 -> SetInterruptRegister value
            | 0x1A -> SetInterruptEnable value
            | 0x1B -> SetMobDataPriority value
            | 0x1C -> SetMobMultiColorEnable value
            | 0x1D -> SetMobXExpansionEnable value
            | 0x20 -> SetBorderColor value
            | 0x21 -> SetBackgroundColor 0 value
            | 0x22 -> SetBackgroundColor 1 value
            | 0x23 -> SetBackgroundColor 2 value
            | 0x24 -> SetBackgroundColor 3 value
            | 0x25 -> SetMobMultiColor 0 value
            | 0x26 -> SetMobMultiColor 1 value
            | 0x27 -> SetMobColor 0 value
            | 0x28 -> SetMobColor 1 value
            | 0x29 -> SetMobColor 2 value
            | 0x2A -> SetMobColor 3 value
            | 0x2B -> SetMobColor 4 value
            | 0x2C -> SetMobColor 5 value
            | 0x2D -> SetMobColor 6 value
            | 0x2E -> SetMobColor 7 value
            | _ -> ()

    member this.ReadRegister address =
        match address &&& 0x3F with
            | 0x1E -> ClearAndGetMobMobCollision()
            | 0x1F -> ClearAndGetMobDataCollision()
            | _ -> this.PeekRegister address

    member this.WriteRegister address value =
        match address &&& 0x3F with
            | 0x13 | 0x14 | 0x1E | 0x1F -> ()
            | _ -> this.PokeRegister address value

    member this.TriggerLightPen() =
        if not lightPenTriggeredThisFrame then
            SetLightPenX (GetRasterX())
            SetLightPenY rasterY
            lightPenTriggeredThisFrame <- true
            lightPenIrq <- true

    member this.OutputIrq = not irq
    member this.OutputBa = ba
    member this.OutputAec = aec
    
    member this.RasterLineCounter = rasterLineCounter
    member this.RasterX = GetRasterX()
    member this.RasterYCompareValue = rasterYCompareValue
    member this.SpriteCrunched(index) = mobDataCrunch.[index]
    member this.SpriteDisplay(index) = mobDisplay.[index]
    member this.SpriteDma(index) = mobDma.[index]
    member this.SpriteCounter(index) = mobCounter.[index]
    member this.SpriteCounterBase(index) = mobCounterBase.[index]
    member this.SpriteMultiColorToggle(index) = mobMultiColorToggle.[index]
    member this.GraphicsMultiColorToggle = graphicsShiftRegisterMultiColorToggle
    member this.BorderMainEnabled = borderMainEnabled
    member this.BorderVerticalEnabled = borderVerticalEnabled

    member this.SpriteX(index) = mobX.[index]
    member this.SpriteY(index) = mobY.[index]
    member this.ExtraColorMode = extraColorMode
    member this.BitmapMode = bitmapMode
    member this.DisplayEnabled = displayEnabled
    member this.RowSelect = rowSelect
    member this.YScroll = yScroll
    member this.RasterY = rasterY
    member this.LightPenX = lightPenX
    member this.LightPenY = lightPenY
    member this.SpriteEnabled(index) = mobEnabled.[index]
    member this.Res = res
    member this.MultiColorMode = multiColorMode
    member this.ColumnSelect = columnSelect
    member this.XScroll = xScroll
    member this.SpriteYExpansionEnabled(index) = mobYExpansionEnabled.[index]
    member this.SpriteYExpansionToggle(index) = mobYExpansionToggle.[index]
    member this.VideoMatrixPointer = videoMemoryPointer >>> 10
    member this.CharacterBankPointer = characterBankPointer >>> 11
    member this.LightPenIrq = lightPenIrq
    member this.SpriteSpriteCollisionIrq = mobMobCollisionIrq
    member this.SpriteBackgroundCollisionIrq = mobBackgroundCollisionIrq
    member this.RasterIrq = rasterIrq
    member this.LightPenIrqEnabled = lightPenIrqEnabled
    member this.SpriteSpriteCollisionIrqEnabled = mobMobCollisionIrqEnabled
    member this.SpriteBackgroundCollisionIrqEnabled = mobBackgroundCollisionIrqEnabled
    member this.RasterIrqEnabled = rasterIrqEnabled
    member this.SpriteDataPriority(index) = mobDataPriority.[index]
    member this.SpriteMultiColorEnabled(index) = mobMultiColorEnabled.[index]
    member this.SpriteXExpansionEnabled(index) = mobXExpansionEnabled.[index]
    member this.SpriteXExpansionToggle(index) = mobXExpansionToggle.[index]
    member this.SpriteSpriteCollision(index) = mobMobCollision.[index]
    member this.SpriteBackgroundCollision(index) = mobDataCollision.[index]
    member this.BorderColor = borderColor
    member this.BackgroundColor(index) = backgroundColor.[index]
    member this.SpriteMultiColor(index) = mobMultiColor.[index]
    member this.SpriteColor(index) = mobColor.[index]
    member this.MemoryAccessCycle = GetMemoryAccessCycle()
    member this.BadLine = badLine
    member this.BadLinesEnabled = badLinesEnabled
    member this.DisplayState = displayState
    member this.VideoMatrix (index) = videoMatrixLineMemory.[index]

namespace Breadbox

    

type CommodoreVic2Configuration(vBlankSet, cyclesPerRasterLine, rasterLinesPerFrame, clockNumerator, clockDenominator) =
    let rasterWidth = (cyclesPerRasterLine * 8)
    let rasterOffset = ((65 - cyclesPerRasterLine) * 8)
    let hBlankSet = 0x18C - rasterOffset
    let hBlankClear = 0x1E8 - (System.Math.Max(0, 64 - cyclesPerRasterLine) * 8)
    let vBlankClear = (vBlankSet + 28) % rasterLinesPerFrame
    let rasterOpsX = hBlankClear - 0x094 // 0x15C - rasterOffsret 
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
    member val ClockNumerator = clockNumerator
    member val ClockDenominator = clockDenominator

type CommodoreVic2ConfigurationFactory() =
    member this.CreateOldNtscConfiguration() = new CommodoreVic2Configuration(13, 64, 262, 14318181, 14)
    member this.CreateNewNtscConfiguration() = new CommodoreVic2Configuration(13, 65, 263, 14318181, 14)
    member this.CreatePalBConfiguration() = new CommodoreVic2Configuration(300, 63, 312, 17734475, 18)
    member this.CreatePalNConfiguration() = new CommodoreVic2Configuration(300, 65, 312, 17734475, 18)
    member this.CreatePalMConfiguration() = this.CreateNewNtscConfiguration()

type CommodoreVic2Chip(config:CommodoreVic2Configuration, memory:IMemory, clockPhi1:IClock, clockPhi2:IClock) = 
    
    let cyclesPerRasterLine = config.CyclesPerRasterLine
    let hBlankSet = config.HBlankSet
    let hBlankClear = config.HBlankClear
    let vBlankSet = config.VBlankSet
    let vBlankClear = config.VBlankClear
    let rasterLinesPerFrame = config.RasterLinesPerFrame
    let rasterOpsX = config.RasterOpsX
    let rasterWidth = config.RasterWidth
    let rasterIncrement = config.RasterIncrement
    let visibleRasterLines = config.VisibleRasterLines
    let visiblePixelsPerRasterLine = config.VisiblePixelsPerRasterLine
    let clockNumerator = config.ClockNumerator
    let clockDenominator = config.ClockDenominator

    let read = memory.Read
    let write = memory.Write

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


    // Framebuffer
    let frameBufferSize = (visiblePixelsPerRasterLine * visibleRasterLines)
    let frameBuffer = Array.zeroCreate frameBufferSize
    let mutable frameBufferIndex = -1

    // MnX (00, 02, 04, 06, 08, 0A, 0C, 0E, 10)
    let mobX = Array.zeroCreate 8

    // MxY (01, 03, 05, 07, 09, 0B, 0D, 0F)
    let mobY = Array.zeroCreate 8

    // RASTER/RST8 (11, 12) (high expects high bit in position 7 as if you wrote to the reg)
    let mutable rasterYCompareValue = 0
    let mutable rasterY =
        rasterLinesPerFrame - 1
    let nextRasterY =
        Array.init rasterLinesPerFrame (fun y -> if y >= (rasterLinesPerFrame - 1) then 0 else (y + 1))

    // Control register 1 (11) (RST8/ECM/BMM/DEN/RSEL/YSCROLL)
    let mutable extraColorMode = false
    let mutable bitmapMode = false
    let mutable displayEnabled = false
    let mutable rowSelect = false
    let mutable yScroll = 0

    // LPX (13) (it's a 9 bit register but we only keep the upper 8)
    let mutable lightPenX = 0

    // LPY (14)
    let mutable lightPenY = 0

    // MnE (15)
    let mobEnabled = Array.zeroCreate 8
        
    // Control Register 2 (16)
    let mutable res = false
    let mutable multiColorMode = false
    let mutable columnSelect = false
    let mutable xScroll = 0

    // Mob Y Expansion (17)
    let mobYExpansionEnabled = Array.zeroCreate 8
    let mobYExpansionToggle = Array.zeroCreate 8

    // Memory Pointers (18) (VM and CB are pre-shifted on store for speed)
    let mutable videoMemoryPointer = 0
    let mutable characterBankPointer = 0

    // Interrupt Register (19) and Interrupt Enable Register (1A)
    let mutable irq = false
    let mutable lightPenIrq = false
    let mutable mobMobCollisionIrq = false
    let mutable mobBackgroundCollisionIrq = false
    let mutable rasterIrq = false
    let mutable rasterIrqDelay = -1
    let mutable lightPenIrqEnabled = false
    let mutable mobMobCollisionIrqEnabled = false
    let mutable mobBackgroundCollisionIrqEnabled = false
    let mutable rasterIrqEnabled = false

    // Sprite Data Priority (1B)
    let mobDataPriority = Array.zeroCreate 8

    // Sprite Multicolor Enable (1C)
    let mobMultiColorEnabled = Array.zeroCreate 8
    let mobMultiColorToggle = Array.zeroCreate 8
    
    // Sprite X Expansion Enable (1D)
    let mobXExpansionEnabled = Array.zeroCreate 8
    let mobXExpansionToggle = Array.zeroCreate 8

    // Sprite-sprite Collision (1E)
    let mutable mobMobFirstCollidedIndex = -1
    let mutable mobMobCollisionOccurred = false
    let mobMobCollision = Array.zeroCreate 8

    // Sprite-background Collision (1F)
    let mobDataCollisionOccurred = false
    let mobDataCollision = Array.zeroCreate 8

    // Border Color (20)
    let mutable borderColor = 0

    // Background Colors (21, 22, 23, 24)
    let backgroundColor = Array.zeroCreate 4

    // Sprite Multicolors (25, 26)
    let mobMultiColor = Array.zeroCreate 2

    // Sprite Colors (27, 28, 29, 2A, 2B, 2C, 2D, 2E)
    let mobColor = Array.zeroCreate 8


    // ========================================================================
    // Internals
    // ========================================================================


    // Raster Line Counter
    // - This determines the Raster X position as well as all horizontal timed operations.
    let rasterXMap = Array.init rasterWidth (fun counter ->
        if rasterWidth <= 0x200 || counter <= 0x18C then
            counter
        else
            let extraCycles = rasterWidth - 0x200
            let adjustedCycle = counter - extraCycles
            if adjustedCycle < 0x18C then
                0x18C
            else
                adjustedCycle
        )
    let nextRasterLineCounter = Array.init rasterWidth (fun x -> if x >= (rasterWidth - 1) then 0 else (x + 1))

    let mutable rasterLineCounter = rasterIncrement - 1

    // Blanking
    let vBlankY = Array.init rasterLinesPerFrame (fun y ->
        if vBlankSet < vBlankClear then
            y >= vBlankSet && y < vBlankClear
        else
            y >= vBlankSet || y < vBlankClear
        )
    let hBlankX = Array.init rasterWidth (fun x -> rasterXMap.[x] >= hBlankSet && rasterXMap.[x] < hBlankClear)

    // Memory Access Cycles (BA for sprite 0 begins the sequence)
    let memoryAccessCycle = Array.init rasterWidth (fun counter ->
        if (counter % 4 <> 0) then
            -1
        else
            (((rasterWidth - rasterOpsX) + counter) % rasterWidth) / 4
        )

    // RC
    let mutable rowCounter = 0

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
    // Process
    // ========================================================================


    let mutable mac = memoryAccessCycle.[rasterLineCounter]
    let mutable rasterX = rasterXMap.[rasterLineCounter]

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

    let ToggleExpansionIfEnabled () =
        for index = 0 to 7 do
            mobYExpansionToggle.[index] <- mobYExpansionToggle.[index] <> mobYExpansionEnabled.[index]

    let CheckSpriteEnable () =
        for index = 0 to 7 do
            if mobEnabled.[index] && (mobY.[index] &&& 0x7) = (rasterY &&& 0x7) && (not mobDma.[index]) then
                mobDma.[index] <- true
                mobCounterBase.[index] <- 0
                mobYExpansionToggle.[index] <- mobYExpansionToggle.[index] && (not mobYExpansionEnabled.[index])

    let CheckSpriteDma () =
        for index = 0 to 7 do
            mobShiftRegisterEnable.[index] <- false
            mobCounter.[index] <- mobCounterBase.[index]
            if mobDma.[index] then
                if (mobY.[index] &&& 0x7) = (rasterY &&& 0x7) then
                    mobDisplay.[index] <- true
            else
                mobDisplay.[index] <- false

    let CheckSpriteCrunch () =
        for index = 0 to 7 do
            if mobYExpansionToggle.[index] then
                mobCounterBase.[index] <-
                    if mobDataCrunch.[index] then
                        (0x2A &&& (mobCounterBase.[index] &&& mobCounter.[index])) ||| (0x15 &&& (mobCounterBase.[index] ||| mobCounter.[index]))
                    else
                        mobCounter.[index]
                if mobCounterBase.[index] = 63 then
                    mobDma.[index] <- false

    let AdvanceRasterY () =
        rasterY <-
            match rasterY with
                | 0x0F8 ->
                    badLinesEnabled <- false
                    badLine <- false
                    0x0F9
                | 0x000 ->
                    videoCounterBase <- 0
                    lightPenTriggeredThisFrame <- false
                    0x001
                | _ ->
                    nextRasterY.[rasterY]
        match rasterY with
            | y when y = rasterYCompareValue ->
                rasterIrqDelay <- (if rasterY = 0 then 8 else 0)
            | _ -> ()

    let ClockRasterCounter () =
        match rasterLineCounter with
            | x when x = rasterIncrement ->
                AdvanceRasterY()
            | _ -> ()

        match rasterLineCounter, rasterY with
            | x,y when y = 0x030 && (x &&& 7) = 4 ->
                badLinesEnabled <- badLinesEnabled || displayEnabled
            | _ -> ()

        match rasterLineCounter, rasterY with
            | x,y when (y &&& 7) = yScroll && (x &&& 7) = 4 ->
                badLine <- badLinesEnabled
                displayState <- displayState || badLine
            | _ -> ()

        match rasterIrqDelay with
            | -1 -> ()
            | x when x > 0 ->
                rasterIrqDelay <- x - 1
            | x when x = 0 -> 
                rasterIrqDelay <- x - 1
                rasterIrq <- true
            | _ -> ()

        match mac with
            | 0 ->
                // cycle 55
                ToggleExpansionIfEnabled()
                CheckSpriteEnable()
            | 2 ->
                // cycle 56
                CheckSpriteEnable()
            | 6 ->
                // cycle 58
                if rowCounter = 7 then
                    displayState <- false
                    videoCounterBase <- videoCounter
                if displayState then
                    rowCounter <- (rowCounter + 1) &&& 0x7
                CheckSpriteDma()
            | 44 ->
                // cycle 14
                videoCounter <- videoCounterBase
                videoMatrixLineIndex <- 0
                if badLine then
                    rowCounter <- 0
            | 48 ->
                // cycle 16
                CheckSpriteCrunch()
            | _ -> ()

    // Memory Interface, Address Generator, Refresh Counter
    let GetReadP index =
        fun _ ->
            mobPointer.[index] <- (read(videoMemoryPointer ||| 0x3F8 ||| index) &&& 0xFF) <<< 6
    let ReadP0 = GetReadP 0
    let ReadP1 = GetReadP 1
    let ReadP2 = GetReadP 2
    let ReadP3 = GetReadP 3
    let ReadP4 = GetReadP 4
    let ReadP5 = GetReadP 5
    let ReadP6 = GetReadP 6
    let ReadP7 = GetReadP 7

    let GetReadS index counter =
        fun _ ->
            match mobDma.[index], counter with
                | true,_ ->
                    mobShiftRegister.[index] <- (mobShiftRegister.[index] <<< 8) ||| (read(mobCounter.[index] ||| mobPointer.[index]) &&& 0xFF)
                    mobCounter.[index] <- mobCounter.[index] + 1
                | _,1 ->
                    read(0x3FFF) |> ignore
                | _ -> ()

    let ReadS00 = GetReadS 0 0
    let ReadS01 = GetReadS 0 1
    let ReadS02 = GetReadS 0 2
    let ReadS10 = GetReadS 1 0
    let ReadS11 = GetReadS 1 1
    let ReadS12 = GetReadS 1 2
    let ReadS20 = GetReadS 2 0
    let ReadS21 = GetReadS 2 1
    let ReadS22 = GetReadS 2 2
    let ReadS30 = GetReadS 3 0
    let ReadS31 = GetReadS 3 1
    let ReadS32 = GetReadS 3 2
    let ReadS40 = GetReadS 4 0
    let ReadS41 = GetReadS 4 1
    let ReadS42 = GetReadS 4 2
    let ReadS50 = GetReadS 5 0
    let ReadS51 = GetReadS 5 1
    let ReadS52 = GetReadS 5 2
    let ReadS60 = GetReadS 6 0
    let ReadS61 = GetReadS 6 1
    let ReadS62 = GetReadS 6 2
    let ReadS70 = GetReadS 7 0
    let ReadS71 = GetReadS 7 1
    let ReadS72 = GetReadS 7 2

    let ReadC () =
        if badLine then
            graphicsReadC <- read(videoMemoryPointer ||| videoCounter)
            if videoMatrixLineIndex < 40 then
                videoMatrixLineMemory.[videoMatrixLineIndex] <- graphicsReadC

    let ReadG () =
        graphicsReadG <-
            read((if extraColorMode then 0x39FF else 0x3FFF) &&&
                if (displayState) then
                    if (bitmapMode) then
                        (characterBankPointer &&& 0x2000) ||| (videoCounter <<< 3) ||| rowCounter
                    else
                        characterBankPointer |||
                        (((if videoMatrixLineIndex < 40 then videoMatrixLineMemory.[videoMatrixLineIndex] else 0) &&& 0xFF) <<< 3) |||
                        rowCounter
                else
                    0x3FFF
            ) &&& 0xFF
        videoCounter <- (videoCounter + 1) &&& 0x3FF
        videoMatrixLineIndex <- (videoMatrixLineIndex + 1) &&& 0x3F

    let ReadI () =
        read(0x3FFF) |> ignore

    let ReadR () =
        refreshCounter <- (refreshCounter - 1) &&& 0xFF
        read(0x3F00 ||| refreshCounter) |> ignore

    let ReadNone () = ()

    let MacMap = [|
        // 000
        (if (cyclesPerRasterLine < 64) then ReadG else ReadI);
        ReadNone;
        (if (cyclesPerRasterLine < 63) then ReadG else ReadI);
        ReadNone;
        (if (cyclesPerRasterLine < 62) then ReadG else ReadI);
        ReadNone;
        ReadP0;
        ReadS00;
        ReadS01;
        ReadS02;
        // 010
        ReadP1;
        ReadS10;
        ReadS11;
        ReadS12;
        ReadP2;
        ReadS20;
        ReadS21;
        ReadS22;
        ReadP3;
        ReadS30;
        // 020
        ReadS31;
        ReadS32;
        ReadP4;
        ReadS40;
        ReadS41;
        ReadS42;
        ReadP5;
        ReadS50;
        ReadS51;
        ReadS52;
        // 030
        ReadP6;
        ReadS60;
        ReadS61;
        ReadS62;
        ReadP7;
        ReadS70;
        ReadS71;
        ReadS72;
        ReadR;
        ReadNone;
        // 040
        ReadR;
        ReadNone;
        ReadR;
        ReadNone;
        ReadR;
        ReadNone;
        ReadR;
        ReadC;
        ReadG;
        ReadC;
        // 050
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        // 060
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        // 070
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        // 080
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        // 090
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        // 100
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        // 110
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        // 120
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
        ReadC;
        ReadG;
    |]

    let ClockMemoryInterface () =
        () |>
            match mac with
                | x when x >= 0 && x < 127 -> MacMap.[x]
                | x when (x &&& 1) = 0 -> ReadI
                | _ -> ReadNone

    let ClockBaAec () =
        ba <-
            match mac with
                | -1 -> ba
                | x when x >= 40 && x < 126 -> not badLine
                | x when x >= 0 && x < 10 && mobDma.[0] -> false
                | x when x >= 4 && x < 14 && mobDma.[1] -> false
                | x when x >= 8 && x < 18 && mobDma.[2] -> false
                | x when x >= 12 && x < 22 && mobDma.[3] -> false
                | x when x >= 16 && x < 26 && mobDma.[4] -> false
                | x when x >= 20 && x < 30 && mobDma.[5] -> false
                | x when x >= 24 && x < 34 && mobDma.[6] -> false
                | x when x >= 28 && x < 38 && mobDma.[7] -> false
                | _ -> true
        baCounter <-
            match ba,baCounter with
                | true,_ -> 24
                | _,c when c > 0 -> c - 1
                | _ -> 0
        aec <-
            match baCounter,rasterLineCounter with
                | c,x when c > 0 && (x &&& 0x4) <> 0 -> true
                | _ -> false

    let ClockIrq () =
        irq <-
            match lightPenIrq,lightPenIrqEnabled,mobMobCollisionIrq,mobMobCollisionIrqEnabled,mobBackgroundCollisionIrq,mobBackgroundCollisionIrqEnabled,rasterIrq,rasterIrqEnabled with
                | true,true,_,_,_,_,_,_ -> true
                | _,_,true,true,_,_,_,_ -> true
                | _,_,_,_,true,true,_,_ -> true
                | _,_,_,_,_,_,true,true -> true
                | _ -> false

    let ClockSprites () =
        for index = 0 to 7 do
            if mobDisplay.[index] then
                if not mobShiftRegisterEnable.[index] && mobX.[index] = rasterX then
                    mobShiftRegisterEnable.[index] <- true
                if mobShiftRegisterEnable.[index] then
                    match mobMultiColorEnabled.[index], mobMultiColorToggle.[index], mobXExpansionEnabled.[index], mobXExpansionToggle.[index] with
                        | multiColor, multiColorToggle, xExpansion, xExpansionToggle ->
                            if xExpansionToggle then
                                if multiColorToggle then
                                    mobShiftRegisterOutput.[index] <- (if multiColor then 0xC00000 else 0x800000)
                                    mobShiftRegister.[index] <- mobShiftRegister.[index] <<< (if multiColor then 2 else 1)
                                mobMultiColorToggle.[index] <- (not multiColor) || (multiColorToggle <> multiColor)
                            mobXExpansionToggle.[index] <- (not xExpansion) || (xExpansionToggle <> xExpansion)

    let ClockGraphics () =
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
        match multiColorMode && (bitmapMode || (graphicsShiftRegisterColor &&& 0x800 <> 0)), graphicsShiftRegisterMultiColorToggle, graphicsShiftRegister with
            | true, true, gsr ->
                graphicsShiftRegisterOutput <- gsr &&& 0xC0
                graphicsShiftRegister <- gsr <<< 2
            | false, _, gsr ->
                graphicsShiftRegisterOutput <- gsr &&& 0x80
                graphicsShiftRegister <- gsr <<< 1
            | _ -> ()

    let ClockBorder () =
        match
            rasterLineCounter,
            rasterY,
            (mac = 16),
            displayEnabled,
            (if columnSelect then 0x018 else 0x01F),
            (if rowSelect then 0x033 else 0x037),
            (if columnSelect then 0x158 else 0x14F),
            (if rowSelect then 0x0FB else 0x0F7)
            with
                | x,_,_,_,_,_,right,_ when x = right ->
                    borderMainEnabled <- true
                | _,y,true,_,_,_,_,bottom when y = bottom ->
                    borderVerticalEnabled <- true
                | _,y,true,true,_,top,_,_ when y = top ->
                    borderVerticalEnabled <- false
                | x,y,_,_,left,_,_,bottom when x = left && y = bottom ->
                    borderVerticalEnabled <- true
                | x,y,_,true,left,top,_,_ when x = left && y = top ->
                    borderVerticalEnabled <- false
                    borderMainEnabled <- false
                | x,_,_,_,left,_,_,_ when x = left ->
                    borderMainEnabled <- false
                | _ -> ()
        borderEnableDelay <- (borderEnableDelay <<< 1) ||| (if (borderMainEnabled || borderVerticalEnabled) then 1 else 0)

    let ClockPixel () =
        if (borderEnableDelay &&& 0x100) <> 0 then
            borderColor
        else
            mobMobFirstCollidedIndex <- -1
            let mutable spriteOutput = -1
            for index = 0 to 7 do
                match mobShiftRegisterOutput.[index], mobMobFirstCollidedIndex with
                    | 0x000000, _ -> ()
                    | bits, collided ->
                        match collided with
                            | -1 ->
                                mobMobFirstCollidedIndex <- index
                                spriteOutput <-
                                    match bits with
                                        | 0x400000 -> mobMultiColor.[0]
                                        | 0xC00000 -> mobMultiColor.[1]
                                        | _ -> mobColor.[index]
                            | _ ->
                                mobMobCollision.[index] <- true
                                mobMobCollision.[collided] <- true
                                if not mobMobCollisionOccurred then
                                    mobMobCollisionIrq <- true
                                    mobMobCollisionOccurred <- true
                        if (not borderVerticalEnabled) && (graphicsShiftRegisterOutput >= 0x80) then
                            mobDataCollision.[index] <- true
            if (spriteOutput >= 0) && ((graphicsShiftRegisterOutput >= 0x80) || (not mobDataPriority.[mobMobFirstCollidedIndex])) then
                spriteOutput
            else
                match graphicsShiftRegisterOutput, bitmapMode, multiColorMode, extraColorMode with
                    | _, true, _, true -> 0
                    | _, _, true, true -> 0
                    | 0x40, true, _, _ -> (graphicsShiftRegisterColor >>> 4) &&& 0x00F
                    | 0x40, false, _, _ -> backgroundColor.[1]
                    | 0x80, true, true, _ -> graphicsShiftRegisterColor &&& 0x00F
                    | 0x80, true, false, _ -> (graphicsShiftRegisterColor >>> 4) &&& 0x00F
                    | 0x80, false, true, _ -> backgroundColor.[2]
                    | 0x80, false, false, _ -> (graphicsShiftRegisterColor >>> 8) &&& 0x00F
                    | 0xC0, true, _, _ -> (graphicsShiftRegisterColor >>> 8) &&& 0x00F
                    | 0xC0, false, _, _ -> (graphicsShiftRegisterColor >>> 8) &&& 0x007
                    | _, false, false, true -> backgroundColor.[(graphicsShiftRegisterColor >>> 6) &&& 0x003]
                    | _, true, false, false -> graphicsShiftRegisterColor &&& 0x00F
                    | _ -> backgroundColor.[0]

    let clock count =
        let mutable i = count
        while i > 0 do
            i <- i - 1
            rasterLineCounter <- nextRasterLineCounter.[rasterLineCounter]
            mac <- memoryAccessCycle.[rasterLineCounter]
            rasterX <- rasterXMap.[rasterLineCounter]

            ClockRasterCounter()
            ClockBorder()
            ClockBaAec()
            ClockMemoryInterface()
            ClockSprites()
            ClockGraphics()
            ClockIrq()

            match (rasterLineCounter &&& 0x7) with
                | 0 -> clockPhi1.Clock
                | 4 -> clockPhi2.Clock
                | _ -> ignore
            <| ()

            let pixel = ClockPixel()
            let hBlank = hBlankX.[rasterLineCounter]
            let vBlank = vBlankY.[rasterY]
            if not (hBlank || vBlank) then
                frameBufferIndex <- if frameBufferIndex >= frameBufferSize then 0 else frameBufferIndex + 1

    member this.Clock () =
        clock 1

    member this.ClockMultiple (count) =
        clock count

    member this.ClockRaster() =
        clock rasterWidth

    member this.ClockToCounterX(counter) =
        let actualCounter = counter % rasterWidth
        while rasterLineCounter <> actualCounter do
            clock 1

    member this.ClockToRasterY(raster) =
        let actualRaster = raster % rasterLinesPerFrame
        while rasterY <> actualRaster do
            clock 1

    member this.ClockTo(counter, raster) =
        this.ClockToRasterY(raster)
        this.ClockToCounterX(counter)

    member this.ClockFrame () =
        clock (rasterWidth * rasterLinesPerFrame)

    member this.ClockSecond () =
        clock (clockNumerator / clockDenominator)


    // ========================================================================
    // Interface
    // ========================================================================


    // Register Access
    member this.PeekRegister address =
        let inline GetLowMobX index =
            mobX.[index] &&& 0xFF
        let inline GetHighMobX () =
            ((mobX.[0] &&& 0x100) >>> 8) |||
            ((mobX.[1] &&& 0x100) >>> 7) |||
            ((mobX.[2] &&& 0x100) >>> 6) |||
            ((mobX.[3] &&& 0x100) >>> 5) |||
            ((mobX.[4] &&& 0x100) >>> 4) |||
            ((mobX.[5] &&& 0x100) >>> 3) |||
            ((mobX.[6] &&& 0x100) >>> 2) |||
            ((mobX.[7] &&& 0x100) >>> 1)
        let inline GetMobY index =
            mobY.[index]
        let inline GetLowRasterY () =
            rasterY &&& 0x0FF
        let inline GetControlRegister1 () =
            ((rasterY &&& 0x100) >>> 1) |||
            (if extraColorMode then 0x40 else 0x00) |||
            (if bitmapMode then 0x20 else 0x00) |||
            (if displayEnabled then 0x10 else 0x00) |||
            (if rowSelect then 0x08 else 0x00) |||
            yScroll
        let inline GetLightPenX () =
            lightPenX
        let inline GetLightPenY () =
            lightPenY
        let inline GetMobEnable () =
            getValueFromBits(mobEnabled)
        let inline GetControlRegister2 () =
            0xC0 |||
            (if res then 0x20 else 0x00) |||
            (if multiColorMode then 0x10 else 0x00) |||
            (if columnSelect then 0x08 else 0x00) |||
            xScroll
        let inline GetMobYExpansionEnable () =
            getValueFromBits(mobYExpansionEnabled)
        let inline GetMemoryPointers () =
            (videoMemoryPointer >>> 6) |||
            (characterBankPointer >>> 10) |||
            0x01
        let inline GetInterruptRegister () =
            (if irq then 0x80 else 0x00) |||
            (if lightPenIrq then 0x08 else 0x00) |||
            (if mobMobCollisionIrq then 0x04 else 0x00) |||
            (if mobBackgroundCollisionIrq then 0x02 else 0x00) |||
            (if rasterIrq then 0x01 else 0x00)
        let inline GetInterruptEnable () =
            (if lightPenIrqEnabled then 0x08 else 0x00) |||
            (if mobMobCollisionIrqEnabled then 0x04 else 0x00) |||
            (if mobBackgroundCollisionIrqEnabled then 0x02 else 0x00) |||
            (if rasterIrqEnabled then 0x01 else 0x00)
        let inline GetMobDataPriority () =
            getValueFromBits(mobDataPriority)
        let inline GetMobMultiColorEnable () =
            getValueFromBits(mobMultiColorEnabled)
        let inline GetMobXExpansionEnable () =
            getValueFromBits(mobXExpansionEnabled)
        let inline GetMobMobCollision () =
            getValueFromBits(mobMobCollision)
        let inline GetMobDataCollision () =
            getValueFromBits(mobDataCollision)
        let inline GetBorderColor () =
            borderColor ||| 0xF0
        let inline GetBackgroundColor index =
            backgroundColor.[index] ||| 0xF0
        let inline GetMobMultiColor index =
            mobMultiColor.[index] ||| 0xF0
        let inline GetMobColor index =
            mobColor.[index] ||| 0xF0

        match (address &&& 0x3F) with
            | 0x00 -> GetLowMobX 0
            | 0x01 -> GetMobY 0
            | 0x02 -> GetLowMobX 1
            | 0x03 -> GetMobY 1
            | 0x04 -> GetLowMobX 2
            | 0x05 -> GetMobY 2
            | 0x06 -> GetLowMobX 3
            | 0x07 -> GetMobY 3
            | 0x08 -> GetLowMobX 4
            | 0x09 -> GetMobY 4
            | 0x0A -> GetLowMobX 5
            | 0x0B -> GetMobY 5
            | 0x0C -> GetLowMobX 6
            | 0x0D -> GetMobY 6
            | 0x0E -> GetLowMobX 7
            | 0x0F -> GetMobY 7
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
            | 0x21 -> GetBackgroundColor 0
            | 0x22 -> GetBackgroundColor 1
            | 0x23 -> GetBackgroundColor 2
            | 0x24 -> GetBackgroundColor 3
            | 0x25 -> GetMobMultiColor 0
            | 0x26 -> GetMobMultiColor 1
            | 0x27 -> GetMobColor 0
            | 0x28 -> GetMobColor 1
            | 0x29 -> GetMobColor 2
            | 0x2A -> GetMobColor 3
            | 0x2B -> GetMobColor 4
            | 0x2C -> GetMobColor 5
            | 0x2D -> GetMobColor 6
            | 0x2E -> GetMobColor 7
            | _ -> 0xFF

    member this.PokeRegister address value =
        let inline SetLowMobX index value =
            mobX.[index] <- (mobX.[index] &&& 0x100) ||| (value &&& 0xFF)
        let inline SetHighMobX value =
            mobX.[0] <- (mobX.[0] &&& 0x0FF) ||| (if value &&& 0x01 <> 0 then 0x100 else 0x000)
            mobX.[1] <- (mobX.[1] &&& 0x0FF) ||| (if value &&& 0x02 <> 0 then 0x100 else 0x000)
            mobX.[2] <- (mobX.[2] &&& 0x0FF) ||| (if value &&& 0x04 <> 0 then 0x100 else 0x000)
            mobX.[3] <- (mobX.[3] &&& 0x0FF) ||| (if value &&& 0x08 <> 0 then 0x100 else 0x000)
            mobX.[4] <- (mobX.[4] &&& 0x0FF) ||| (if value &&& 0x10 <> 0 then 0x100 else 0x000)
            mobX.[5] <- (mobX.[5] &&& 0x0FF) ||| (if value &&& 0x20 <> 0 then 0x100 else 0x000)
            mobX.[6] <- (mobX.[6] &&& 0x0FF) ||| (if value &&& 0x40 <> 0 then 0x100 else 0x000)
            mobX.[7] <- (mobX.[7] &&& 0x0FF) ||| (if value &&& 0x80 <> 0 then 0x100 else 0x000)
        let inline SetMobY index value =
            mobY.[index] <- value &&& 0xFF
        let inline SetLowRasterYCompareValue value =
            rasterYCompareValue <- (rasterYCompareValue &&& 0x100) ||| value
        let inline SetControlRegister1 value =
            rasterYCompareValue <- (rasterYCompareValue &&& 0x0FF) ||| ((value &&& 0x80) <<< 1)
            extraColorMode <- (value &&& 0x40 <> 0x00)
            bitmapMode <- (value &&& 0x20 <> 0x00)
            displayEnabled <- (value &&& 0x10 <> 0x00)
            rowSelect <- (value &&& 0x08 <> 0x00)
            yScroll <- (value &&& 0x07)
        let inline SetMobEnable value =
            setBitsFromValue(mobEnabled, value)
        let inline SetControlRegister2 value =
            res <- (value &&& 0x20) <> 0x00
            multiColorMode <- (value &&& 0x10) <> 0x00
            columnSelect <- (value &&& 0x08) <> 0x00
            xScroll <- (value &&& 0x07)
        let inline SetMobYExpansionEnable value =
            setBitsFromValue(mobYExpansionEnabled, value)
        let inline SetMemoryPointers value =
            videoMemoryPointer <- (value &&& 0xF0) <<< 6
            characterBankPointer <- (value &&& 0x0E) <<< 10
        let inline SetInterruptRegister value =
            lightPenIrq <- (lightPenIrq && ((value &&& 0x08) = 0))
            mobMobCollisionIrq <- (mobMobCollisionIrq && ((value &&& 0x04) = 0))
            mobBackgroundCollisionIrq <- (mobBackgroundCollisionIrq && ((value &&& 0x02) = 0))
            rasterIrq <- (rasterIrq && ((value &&& 0x01) = 0))
        let inline SetInterruptEnable value =
            lightPenIrqEnabled <- (value &&& 0x08) <> 0
            mobMobCollisionIrqEnabled <- (value &&& 0x04) <> 0
            mobBackgroundCollisionIrq <- (value &&& 0x02) <> 0
            rasterIrqEnabled <- (value &&& 0x01) <> 0
        let inline SetMobDataPriority value =
            setBitsFromValue(mobDataPriority, value)
        let inline SetMobMultiColorEnable value =
            setBitsFromValue(mobMultiColorEnabled, value)
        let inline SetMobXExpansionEnable value =
            setBitsFromValue(mobXExpansionEnabled, value)
        let inline SetBorderColor value =
            borderColor <- value &&& 0x0F
        let inline SetBackgroundColor index value =
            backgroundColor.[index] <- value &&& 0x0F
        let inline SetMobMultiColor index value =
            mobMultiColor.[index] <- value &&& 0x0F
        let inline SetMobColor index value =
            mobColor.[index] <- value &&& 0x0F

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
        let ClearAndGetMobMobCollision () =
            let oldValue = this.PeekRegister 0x1E
            clearBits(mobMobCollision)
            mobMobFirstCollidedIndex <- -1
            oldValue
        let ClearAndGetMobDataCollision () =
            let oldValue = this.PeekRegister 0x1F
            clearBits(mobDataCollision)
            oldValue

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
            lightPenX <- rasterX >>> 1
            lightPenY <- rasterY
            lightPenTriggeredThisFrame <- true
            lightPenIrq <- true

    member this.OutputIrq = not irq
    member this.OutputBa = ba
    member this.OutputAec = aec
    
    member this.RasterLineCounter = rasterLineCounter
    member this.RasterX = rasterX
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
    member this.MemoryAccessCycle = mac
    member this.BadLine = badLine
    member this.BadLinesEnabled = badLinesEnabled
    member this.DisplayState = displayState
    member this.VideoMatrix (index) = videoMatrixLineMemory.[index]
    member this.FrameBuffer = frameBuffer
    member this.FrameBufferIndex = frameBufferIndex

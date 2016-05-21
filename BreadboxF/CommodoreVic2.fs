namespace BreadboxF

type CommodoreVic2Configuration(vBlankSet:int, cyclesPerRasterLine:int, rasterLinesPerFrame:int) =
    let rasterWidth = (cyclesPerRasterLine * 8)
    member val CyclesPerRasterLine = cyclesPerRasterLine
    member val HBlankSet = 0x18C - ((65 - cyclesPerRasterLine) * 8)
    member val HBlankClear = 0x1E8 - (System.Math.Max(0, 64 - cyclesPerRasterLine) * 8)
    member val VBlankSet = vBlankSet
    member val VBlankClear = (vBlankSet + 28) % rasterLinesPerFrame
    member val RasterLinesPerFrame = rasterLinesPerFrame
    member val RasterOpsX = 0x15C - ((65 - cyclesPerRasterLine) * 8)
    member val RasterWidth = rasterWidth

type CommodoreVic2Chip(config:CommodoreVic2Configuration, readBus:System.Func<int, int>, writeVideo:System.Action<int, bool>, clockPhi1, clockPhi2) = 


    // ========================================================================
    // Registers
    // ========================================================================


    // MnX (00, 02, 04, 06, 08, 0A, 0C, 0E, 10)
    let mobX = Array.create 8 0
    let GetLowMobX (index:int) =
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
    let SetLowMobX (index:int, value:int) =
        mobX.[index] <- (mobX.[index] &&& 0x100) ||| value
    let SetHighMobX (value:int) =
        mobX.[0] <- (mobX.[0] &&& 0x0FF) ||| (if value &&& 0x01 <> 0 then 0x100 else 0x000)
        mobX.[1] <- (mobX.[1] &&& 0x0FF) ||| (if value &&& 0x02 <> 0 then 0x100 else 0x000)
        mobX.[2] <- (mobX.[2] &&& 0x0FF) ||| (if value &&& 0x04 <> 0 then 0x100 else 0x000)
        mobX.[3] <- (mobX.[3] &&& 0x0FF) ||| (if value &&& 0x08 <> 0 then 0x100 else 0x000)
        mobX.[4] <- (mobX.[4] &&& 0x0FF) ||| (if value &&& 0x10 <> 0 then 0x100 else 0x000)
        mobX.[5] <- (mobX.[5] &&& 0x0FF) ||| (if value &&& 0x20 <> 0 then 0x100 else 0x000)
        mobX.[6] <- (mobX.[6] &&& 0x0FF) ||| (if value &&& 0x40 <> 0 then 0x100 else 0x000)
        mobX.[7] <- (mobX.[7] &&& 0x0FF) ||| (if value &&& 0x80 <> 0 then 0x100 else 0x000)

    // MxY (01, 03, 05, 07, 09, 0B, 0D, 0F)
    let mobY = Array.create 8 0
    let GetMobY (index:int) =
        mobY.[index]
    let SetMobY (index:int, value:int) =
        mobY.[index] <- value

    // RASTER/RST8 (11, 12) (high expects high bit in position 7 as if you wrote to the reg)
    let mutable rasterY = 0
    let mutable rasterYCompareValue = 0
    let IncrementRasterY () =
        rasterY <- if rasterY >= (config.CyclesPerRasterLine - 1) then 0 else (rasterY + 1)
    let GetLowRasterY () =
        rasterY &&& 0x0FF
    let SetLowRasterYCompareValue (value:int) =
        rasterYCompareValue <- (rasterYCompareValue &&& 0x100) ||| value

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
    let SetControlRegister1 (value:int) =
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
    let SetLightPenX (value:int) =
        lightPenX <- value >>> 1

    // LPY (14)
    let mutable lightPenY = 0
    let GetLightPenY () =
        lightPenY
    let SetLightPenY (value:int) =
        lightPenY <- value

    // MnE (15)
    let mobEnabled = Array.create 8 false
    let GetMobEnable () =
        (if mobEnabled.[0] then 0x01 else 0x00) |||
        (if mobEnabled.[1] then 0x02 else 0x00) |||
        (if mobEnabled.[2] then 0x04 else 0x00) |||
        (if mobEnabled.[3] then 0x08 else 0x00) |||
        (if mobEnabled.[4] then 0x10 else 0x00) |||
        (if mobEnabled.[5] then 0x20 else 0x00) |||
        (if mobEnabled.[6] then 0x40 else 0x00) |||
        (if mobEnabled.[7] then 0x80 else 0x00)
    let SetMobEnable (value:int) =
        mobEnabled.[0] <- (value &&& 0x01) <> 0
        mobEnabled.[1] <- (value &&& 0x02) <> 0
        mobEnabled.[2] <- (value &&& 0x04) <> 0
        mobEnabled.[3] <- (value &&& 0x08) <> 0
        mobEnabled.[4] <- (value &&& 0x10) <> 0
        mobEnabled.[5] <- (value &&& 0x20) <> 0
        mobEnabled.[6] <- (value &&& 0x40) <> 0
        mobEnabled.[7] <- (value &&& 0x80) <> 0
        
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
    let SetControlRegister2 (value:int) =
        res <- (value &&& 0x20) <> 0x00
        multiColorMode <- (value &&& 0x10) <> 0x00
        columnSelect <- (value &&& 0x08) <> 0x00
        xScroll <- (value &&& 0x07)

    // Mob Y Expansion (17)
    let mobYExpansionEnabled = Array.create 8 false
    let mobYExpansionToggle = Array.create 8 false
    let GetMobYExpansionEnable () =
        (if mobYExpansionEnabled.[0] then 0x01 else 0x00) |||
        (if mobYExpansionEnabled.[1] then 0x02 else 0x00) |||
        (if mobYExpansionEnabled.[2] then 0x04 else 0x00) |||
        (if mobYExpansionEnabled.[3] then 0x08 else 0x00) |||
        (if mobYExpansionEnabled.[4] then 0x10 else 0x00) |||
        (if mobYExpansionEnabled.[5] then 0x20 else 0x00) |||
        (if mobYExpansionEnabled.[6] then 0x40 else 0x00) |||
        (if mobYExpansionEnabled.[7] then 0x80 else 0x00)
    let SetMobYExpansionEnable (value:int) =
        mobYExpansionEnabled.[0] <- (value &&& 0x01) <> 0
        mobYExpansionEnabled.[1] <- (value &&& 0x02) <> 0
        mobYExpansionEnabled.[2] <- (value &&& 0x04) <> 0
        mobYExpansionEnabled.[3] <- (value &&& 0x08) <> 0
        mobYExpansionEnabled.[4] <- (value &&& 0x10) <> 0
        mobYExpansionEnabled.[5] <- (value &&& 0x20) <> 0
        mobYExpansionEnabled.[6] <- (value &&& 0x40) <> 0
        mobYExpansionEnabled.[7] <- (value &&& 0x80) <> 0
    let SetMobYExpansionToggle (index:int) =
        mobYExpansionToggle.[index] <- true
    let ClearMobYExpansionToggle (index:int) =
        mobYExpansionToggle.[index] <- false
    let InvertMobYExpansionToggle (index:int) =
        mobYExpansionToggle.[index] <- not mobYExpansionToggle.[index]

    // Memory Pointers (18) (VM and CB are pre-shifted on store for speed)
    let mutable videoMemoryPointer = 0
    let mutable characterBankPointer = 0
    let GetMemoryPointers () =
        (videoMemoryPointer >>> 6) |||
        (characterBankPointer >>> 10) |||
        0x01
    let SetMemoryPointers (value:int) =
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
    let SetInterruptRegister (value:int) =
        lightPenIrq <- (lightPenIrq && ((value &&& 0x08) = 0))
        mobMobCollisionIrq <- (mobMobCollisionIrq && ((value &&& 0x04) = 0))
        mobBackgroundCollisionIrq <- (mobBackgroundCollisionIrq && ((value &&& 0x02) = 0))
        rasterIrq <- (rasterIrq && ((value &&& 0x01) = 0))
    let SetInterruptEnable (value:int) =
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
    let mobDataPriority = Array.create 8 false
    let GetMobDataPriority () =
        (if mobDataPriority.[0] then 0x01 else 0x00) |||
        (if mobDataPriority.[1] then 0x02 else 0x00) |||
        (if mobDataPriority.[2] then 0x04 else 0x00) |||
        (if mobDataPriority.[3] then 0x08 else 0x00) |||
        (if mobDataPriority.[4] then 0x10 else 0x00) |||
        (if mobDataPriority.[5] then 0x20 else 0x00) |||
        (if mobDataPriority.[6] then 0x40 else 0x00) |||
        (if mobDataPriority.[7] then 0x80 else 0x00)
    let SetMobDataPriority (value:int) =
        mobDataPriority.[0] <- (value &&& 0x01) <> 0
        mobDataPriority.[1] <- (value &&& 0x02) <> 0
        mobDataPriority.[2] <- (value &&& 0x04) <> 0
        mobDataPriority.[3] <- (value &&& 0x08) <> 0
        mobDataPriority.[4] <- (value &&& 0x10) <> 0
        mobDataPriority.[5] <- (value &&& 0x20) <> 0
        mobDataPriority.[6] <- (value &&& 0x40) <> 0
        mobDataPriority.[7] <- (value &&& 0x80) <> 0

    // Sprite Multicolor Enable (1C)
    let mobMultiColorEnabled = Array.create 8 false
    let mobMultiColorToggle = Array.create 8 false
    let GetMobMultiColorEnable () =
        (if mobMultiColorEnabled.[0] then 0x01 else 0x00) |||
        (if mobMultiColorEnabled.[1] then 0x02 else 0x00) |||
        (if mobMultiColorEnabled.[2] then 0x04 else 0x00) |||
        (if mobMultiColorEnabled.[3] then 0x08 else 0x00) |||
        (if mobMultiColorEnabled.[4] then 0x10 else 0x00) |||
        (if mobMultiColorEnabled.[5] then 0x20 else 0x00) |||
        (if mobMultiColorEnabled.[6] then 0x40 else 0x00) |||
        (if mobMultiColorEnabled.[7] then 0x80 else 0x00)
    let SetMobMultiColorEnable (value:int) =
        mobMultiColorEnabled.[0] <- (value &&& 0x01) <> 0
        mobMultiColorEnabled.[1] <- (value &&& 0x02) <> 0
        mobMultiColorEnabled.[2] <- (value &&& 0x04) <> 0
        mobMultiColorEnabled.[3] <- (value &&& 0x08) <> 0
        mobMultiColorEnabled.[4] <- (value &&& 0x10) <> 0
        mobMultiColorEnabled.[5] <- (value &&& 0x20) <> 0
        mobMultiColorEnabled.[6] <- (value &&& 0x40) <> 0
        mobMultiColorEnabled.[7] <- (value &&& 0x80) <> 0
    
    // Sprite X Expansion Enable (1D)
    let mobXExpansionEnabled = Array.create 8 false
    let mobXExpansionToggle = Array.create 8 false
    let GetMobXExpansionEnable () =
        (if mobXExpansionEnabled.[0] then 0x01 else 0x00) |||
        (if mobXExpansionEnabled.[1] then 0x02 else 0x00) |||
        (if mobXExpansionEnabled.[2] then 0x04 else 0x00) |||
        (if mobXExpansionEnabled.[3] then 0x08 else 0x00) |||
        (if mobXExpansionEnabled.[4] then 0x10 else 0x00) |||
        (if mobXExpansionEnabled.[5] then 0x20 else 0x00) |||
        (if mobXExpansionEnabled.[6] then 0x40 else 0x00) |||
        (if mobXExpansionEnabled.[7] then 0x80 else 0x00)
    let SetMobXExpansionEnable (value:int) =
        mobXExpansionEnabled.[0] <- (value &&& 0x01) <> 0
        mobXExpansionEnabled.[1] <- (value &&& 0x02) <> 0
        mobXExpansionEnabled.[2] <- (value &&& 0x04) <> 0
        mobXExpansionEnabled.[3] <- (value &&& 0x08) <> 0
        mobXExpansionEnabled.[4] <- (value &&& 0x10) <> 0
        mobXExpansionEnabled.[5] <- (value &&& 0x20) <> 0
        mobXExpansionEnabled.[6] <- (value &&& 0x40) <> 0
        mobXExpansionEnabled.[7] <- (value &&& 0x80) <> 0
    let SetMobXExpansionToggle (index:int) =
        mobXExpansionToggle.[index] <- true
    let ClearMobXExpansionToggle (index:int) =
        mobXExpansionToggle.[index] <- false
    let InvertMobXExpansionToggle (index:int) =
        mobXExpansionToggle.[index] <- not mobXExpansionToggle.[index]

    // Sprite-sprite Collision (1E)
    let mutable mobMobFirstCollidedIndex = -1
    let mutable mobMobCollisionOccurred = false
    let mobMobCollision = Array.create 8 false
    let GetMobMobCollision () =
        (if mobMobCollision.[0] then 0x01 else 0x00) |||
        (if mobMobCollision.[1] then 0x02 else 0x00) |||
        (if mobMobCollision.[2] then 0x04 else 0x00) |||
        (if mobMobCollision.[3] then 0x08 else 0x00) |||
        (if mobMobCollision.[4] then 0x10 else 0x00) |||
        (if mobMobCollision.[5] then 0x20 else 0x00) |||
        (if mobMobCollision.[6] then 0x40 else 0x00) |||
        (if mobMobCollision.[7] then 0x80 else 0x00)
    let ClearAndGetMobMobCollision () =
        let oldValue = GetMobMobCollision()
        mobMobCollision.[0] <- false
        mobMobCollision.[1] <- false
        mobMobCollision.[2] <- false
        mobMobCollision.[3] <- false
        mobMobCollision.[4] <- false
        mobMobCollision.[5] <- false
        mobMobCollision.[6] <- false
        mobMobCollision.[7] <- false
        mobMobFirstCollidedIndex <- -1
        oldValue

    // Sprite-background Collision (1F)
    let mobDataCollisionOccurred = false
    let mobDataCollision = Array.create 8 false
    let GetMobDataCollision () =
        (if mobDataCollision.[0] then 0x01 else 0x00) |||
        (if mobDataCollision.[1] then 0x02 else 0x00) |||
        (if mobDataCollision.[2] then 0x04 else 0x00) |||
        (if mobDataCollision.[3] then 0x08 else 0x00) |||
        (if mobDataCollision.[4] then 0x10 else 0x00) |||
        (if mobDataCollision.[5] then 0x20 else 0x00) |||
        (if mobDataCollision.[6] then 0x40 else 0x00) |||
        (if mobDataCollision.[7] then 0x80 else 0x00)
    let ClearAndGetMobDataCollision () =
        let oldValue = GetMobDataCollision()
        mobDataCollision.[0] <- false
        mobDataCollision.[1] <- false
        mobDataCollision.[2] <- false
        mobDataCollision.[3] <- false
        mobDataCollision.[4] <- false
        mobDataCollision.[5] <- false
        mobDataCollision.[6] <- false
        mobDataCollision.[7] <- false
        oldValue

    // Border Color (20)
    let mutable borderColor = 0
    let GetBorderColor () =
        borderColor ||| 0xF0
    let SetBorderColor (value:int) =
        borderColor <- value &&& 0x0F

    // Background Colors (21, 22, 23, 24)
    let backgroundColor = Array.create 4 0
    let GetBackgroundColor (index:int) =
        backgroundColor.[index] ||| 0xF0
    let SetBackgroundColor (index:int, value:int) =
        backgroundColor.[index] <- value &&& 0x0F

    // Sprite Multicolors (25, 26)
    let mobMultiColor = Array.create 2 0
    let GetMobMultiColor (index:int) =
        mobMultiColor.[index] ||| 0xF0
    let SetMobMultiColor (index:int, value:int) =
        mobMultiColor.[index] <- value &&& 0x0F

    // Sprite Colors (27, 28, 29, 2A, 2B, 2C, 2D, 2E)
    let mobColor = Array.create 8 0
    let GetMobColor (index:int) =
        mobColor.[index] ||| 0xF0
    let SetMobColor (index:int, value:int) =
        mobColor.[index] <- value &&& 0x0F


    // ========================================================================
    // Internals
    // ========================================================================


    // Blanking
    // - The circuitry outputs black when blanked, so there's no need to render pixels.
    let mutable vBlank = true
    let mutable hBlank = true
    let ClearVBlank () =
        vBlank <- false
    let ClearHBlank () =
        hBlank <- false
    let SetVBlank () =
        vBlank <- true
    let SetHBlank () =
        hBlank <- true

    // Raster Line Counter
    // - This determines the Raster X position as well as all horizontal timed operations.
    let rasterX = Array.init (config.RasterWidth) (fun counter ->
        if (config.RasterWidth <= 0x200 || counter <= 0x18C) then
            counter
        else
            let extraCycles = config.RasterWidth - 0x200
            let adjustedCycle = counter - extraCycles
            if (adjustedCycle < 0x18C) then
                0x18C
            else
                adjustedCycle
        )

    let mutable rasterLineCounter = 0
    let IncrementRasterLineCounter () =
        rasterLineCounter <- if rasterLineCounter >= (config.CyclesPerRasterLine - 1) then 0 else rasterLineCounter + 1
    let GetRasterX () =
        rasterX.[rasterLineCounter]

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
    let memoryAccessCycle = Array.init (config.RasterWidth) (fun counter ->
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
    let videoMatrixLineMemory = Array.create 40 0
    let IncrementVideoCounter () =
        videoCounter <- (videoCounter + 1) &&& 0x3FF
        videoMatrixLineIndex <- (videoMatrixLineIndex + 1) &&& 0x3F
    let IncrementVideoCounterBase () =
        videoCounterBase <- videoCounter
    let ResetVideoCounter () =
        videoCounter <- videoCounterBase
        videoMatrixLineIndex <- 0
    let ResetVideoCounterBase () =
        videoCounterBase <- 0
    let GetVideoMatrixLineMemory () =
        if videoMatrixLineIndex < 40 then videoMatrixLineMemory.[videoMatrixLineIndex] else 0
    let SetVideoMatrixLineMemory (value:int) =
        if videoMatrixLineIndex < 40 then videoMatrixLineMemory.[videoMatrixLineIndex] <- value

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
    let mobCounter = Array.create 8 0
    let mobCounterBase = Array.create 8 0
    let mobDma = Array.create 8 false
    let mobDataCrunch = Array.create 8 false
    let mobPointer = Array.create 8 0
    let mobDisplay = Array.create 8 false

    // Sprite shift registers
    let mobShiftRegister = Array.create 8 0
    let mobShiftRegisterEnable = Array.create 8 false
    let mobShiftRegisterOutput = Array.create 8 0

    // Display/Idle state
    let mutable displayState = false
    let GoToDisplayState () =
        displayState <- true
    let GoToIdleState () =
        displayState <- false

    // Refresh counter
    let mutable refreshCounter = 0
    let DecrementRefreshCounter () =
        refreshCounter <- (refreshCounter - 1) &&& 0xFF

    // Bad lines
    let mutable badLinesEnabled = false
    let mutable badLine = false

    // AEC and BA
    let mutable aec = false
    let mutable ba = false
    let mutable baCounter = 24
    let IsSpriteBa (counter:int, index:int) =
        let lowerBound = index * 4
        let upperBound = lowerBound + 10
        counter >= lowerBound && counter < upperBound && mobDma.[index]
    let IsAnySpriteBa (counter:int) =
        IsSpriteBa(counter, 0) ||
        IsSpriteBa(counter, 1) ||
        IsSpriteBa(counter, 2) ||
        IsSpriteBa(counter, 3) ||
        IsSpriteBa(counter, 4) ||
        IsSpriteBa(counter, 5) ||
        IsSpriteBa(counter, 6) ||
        IsSpriteBa(counter, 7)
    let IsBadLineBa (counter:int) =
        badLine && counter >= 40 && counter < 126

    // Border Unit
    let mutable borderVerticalEnabled = false
    let mutable borderMainEnabled = false
    let mutable borderEnableDelay = 0
    let GetBorderOutput () =
        (borderEnableDelay &&& 0x100 = 1)


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
    let ClockRasterCounter (mac:int) =
        match mac with
            | 0 | 2 ->
                // cycle 55-56
                if mac = 0 then
                    let toggleExpansionIfEnabled (index:int) =
                        mobYExpansionToggle.[index] <- mobYExpansionToggle.[index] <> mobYExpansionEnabled.[index]
                    toggleExpansionIfEnabled(0)
                    toggleExpansionIfEnabled(1)
                    toggleExpansionIfEnabled(2)
                    toggleExpansionIfEnabled(3)
                    toggleExpansionIfEnabled(4)
                    toggleExpansionIfEnabled(5)
                    toggleExpansionIfEnabled(6)
                    toggleExpansionIfEnabled(7)
                let checkSpriteEnable (index:int) =
                    if mobEnabled.[index] && (mobY.[index] &&& 0x7) = (rasterY &&& 0x7) then
                        if not mobDma.[index] then
                            mobDma.[index] <- true
                            mobCounterBase.[index] <- 0
                            mobYExpansionToggle.[index] <- mobYExpansionToggle.[index] && (not mobYExpansionEnabled.[index])
                checkSpriteEnable(0)
                checkSpriteEnable(1)
                checkSpriteEnable(2)
                checkSpriteEnable(3)
                checkSpriteEnable(4)
                checkSpriteEnable(5)
                checkSpriteEnable(6)
                checkSpriteEnable(7)
            | 6 ->
                // cycle 58
                if rowCounter = 7 then
                    GoToIdleState()
                    videoCounterBase <- videoCounter
                if displayState then
                    IncrementRowCounter()
                let reloadMobCounter (index:int) =
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
                let checkSpriteCrunch (index:int) =
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
    let ClockMemoryInterface (mac:int) =
        let readP (index:int) =
            mobPointer.[index] <- (readBus.Invoke(videoMemoryPointer ||| 0x3F8 ||| index) &&& 0xFF) <<< 6
        let readS (index:int, counter:int) =
            if mobDma.[index] then
                mobShiftRegister.[index] <- (mobShiftRegister.[index] <<< 8) ||| (readBus.Invoke(mobCounter.[index] ||| mobPointer.[index]) &&& 0xFF)
                mobCounter.[index] <- mobCounter.[index] + 1
            else
                if counter = 1 then
                    readBus.Invoke(0x3FFF) |> ignore
        let readC () =
            if badLine then
                graphicsReadC <- readBus.Invoke(videoMemoryPointer ||| videoCounter)
                SetVideoMatrixLineMemory(graphicsReadC)
            else
                readBus.Invoke(0x3FFF) |> ignore
        let readG () =
            graphicsReadG <-
                readBus.Invoke((if extraColorMode then 0x39FF else 0x3FFF) &&&
                    if (displayState) then
                        if (bitmapMode) then
                            (characterBankPointer &&& 0x2000) ||| (videoCounter <<< 3) ||| rowCounter
                        else
                            characterBankPointer ||| (GetVideoMatrixLineMemory() &&& 0xFF) ||| rowCounter
                    else
                        0x3FFF
                ) &&& 0xFF
            IncrementVideoCounter()
        let readI () =
            readBus.Invoke(0x3FFF) |> ignore
        let readR () =
            DecrementRefreshCounter()
            readBus.Invoke(0x3F00 ||| refreshCounter) |> ignore
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

    let ClockBaAec(mac:int) =
        ba <-
            if mac = -1 then
                ba
            else
                not (IsBadLineBa(mac) || IsAnySpriteBa(mac))
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

    let ClockSprite (index:int, rasterX:int) =
        let display = mobDisplay.[index]
        let multiColor = mobMultiColorEnabled.[index]
        let expandX = mobXExpansionEnabled.[index]
        let mobX = mobX.[index]
        if display then
            if not mobShiftRegisterEnable.[index] then
                if mobX = rasterX then
                    mobShiftRegisterEnable.[index] <- true
            if mobShiftRegisterEnable.[index] then
                if mobXExpansionToggle.[index] then
                    if mobMultiColorToggle.[index] then
                        mobShiftRegisterOutput.[index] <- (if multiColor then 0xC00000 else 0x800000)
                        mobShiftRegister.[index] <- mobShiftRegister.[index] <<< (if multiColor then 2 else 1)
                    mobMultiColorToggle.[index] <- (not multiColor) || (mobMultiColorToggle.[index] <> multiColor)
                mobXExpansionToggle.[index] <- (not expandX) || (mobXExpansionToggle.[index] <> expandX)

    let ClockSprites (rasterX:int) =
        ClockSprite(0, rasterX)
        ClockSprite(1, rasterX)
        ClockSprite(2, rasterX)
        ClockSprite(3, rasterX)
        ClockSprite(4, rasterX)
        ClockSprite(5, rasterX)
        ClockSprite(6, rasterX)
        ClockSprite(7, rasterX)

    let ClockGraphics (mac:int, rasterX:int) =
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

    let ClockBorder () =
        borderEnableDelay <- (borderEnableDelay <<< 1) ||| (if borderMainEnabled || borderVerticalEnabled then 1 else 0)

    let ClockPixel () =
        mobMobFirstCollidedIndex <- -1
        let outputForSprite(index:int) =
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

        if GetBorderOutput() then
            borderColor
        else
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
        IncrementRasterLineCounter()
        let mac = GetMemoryAccessCycle()
        let rasterX = GetRasterX()

        ClockRasterCounter(mac)
        ClockBaAec(mac)
        ClockMemoryInterface(mac)
        ClockSprites(rasterX)
        ClockGraphics(mac, rasterX)
        writeVideo.Invoke(ClockPixel(), hBlank || vBlank)
        ClockIrq()

    member this.ClockFrame () =
        let mutable i = config.RasterWidth * config.RasterLinesPerFrame
        while i > 0 do
            this.Clock()
            i <- i - 1


    // ========================================================================
    // Interface
    // ========================================================================


    // Register Access
    member this.PeekRegister (address:int) =
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

    member this.PokeRegister (address:int, value:int) =
        match (address &&& 0x3F) with
            | 0x00 -> SetLowMobX(0, value)
            | 0x01 -> SetMobY(0, value)
            | 0x02 -> SetLowMobX(1, value)
            | 0x03 -> SetMobY(1, value)
            | 0x04 -> SetLowMobX(2, value)
            | 0x05 -> SetMobY(2, value)
            | 0x06 -> SetLowMobX(3, value)
            | 0x07 -> SetMobY(3, value)
            | 0x08 -> SetLowMobX(4, value)
            | 0x09 -> SetMobY(4, value)
            | 0x0A -> SetLowMobX(5, value)
            | 0x0B -> SetMobY(5, value)
            | 0x0C -> SetLowMobX(6, value)
            | 0x0D -> SetMobY(6, value)
            | 0x0E -> SetLowMobX(7, value)
            | 0x0F -> SetMobY(7, value)
            | 0x10 -> SetHighMobX(value)
            | 0x11 -> SetControlRegister1(value)
            | 0x12 -> SetLowRasterYCompareValue(value)
            | 0x15 -> SetMobEnable(value)
            | 0x16 -> SetControlRegister2(value)
            | 0x17 -> SetMobYExpansionEnable(value)
            | 0x18 -> SetMemoryPointers(value)
            | 0x19 -> SetInterruptRegister(value)
            | 0x1A -> SetInterruptEnable(value)
            | 0x1B -> SetMobDataPriority(value)
            | 0x1C -> SetMobMultiColorEnable(value)
            | 0x1D -> SetMobXExpansionEnable(value)
            | 0x20 -> SetBorderColor(value)
            | 0x21 -> SetBackgroundColor(0, value)
            | 0x22 -> SetBackgroundColor(1, value)
            | 0x23 -> SetBackgroundColor(2, value)
            | 0x24 -> SetBackgroundColor(3, value)
            | 0x25 -> SetMobMultiColor(0, value)
            | 0x26 -> SetMobMultiColor(1, value)
            | 0x27 -> SetMobColor(0, value)
            | 0x28 -> SetMobColor(1, value)
            | 0x29 -> SetMobColor(2, value)
            | 0x2A -> SetMobColor(3, value)
            | 0x2B -> SetMobColor(4, value)
            | 0x2C -> SetMobColor(5, value)
            | 0x2D -> SetMobColor(6, value)
            | 0x2E -> SetMobColor(7, value)
            | _ -> ()

    member this.ReadRegister(address:int) =
        match (address &&& 0x3F) with
            | 0x1E -> ClearAndGetMobMobCollision()
            | 0x1F -> ClearAndGetMobDataCollision()
            | _ -> this.PeekRegister(address)

    member this.WriteRegister(address:int, value:int) =
        match (address &&& 0x3F) with
            | 0x13 | 0x14 | 0x1E | 0x1F -> ()
            | _ -> this.PokeRegister(address, value)

    member this.OutputIrq = not irq
    member this.OutputBa = ba
    member this.OutputAec = aec
    member this.OutputPhi1 = IsPhi1()
    member this.OutputPhi2 = IsPhi0()
    



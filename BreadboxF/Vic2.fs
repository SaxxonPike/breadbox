namespace BreadboxF

type Vic2Configuration(vBlankSet:int, cyclesPerRasterLine:int, rasterLinesPerFrame:int) =
    member val CyclesPerRasterLine = cyclesPerRasterLine
    member val HBlankSet = 0x18C - ((65 - cyclesPerRasterLine) * 8)
    member val HBlankClear = 0x1E8 - (System.Math.Max(0, 64 - cyclesPerRasterLine) * 8)
    member val VBlankSet = vBlankSet
    member val VBlankClear = (vBlankSet + 28) % rasterLinesPerFrame
    member val RasterLinesPerFrame = rasterLinesPerFrame
    member val RasterOpsX = 0x15C - ((65 - cyclesPerRasterLine) * 8)

type Vic2State(config:Vic2Configuration) = 
    // Constants
    let bitMasks = [0x01; 0x02; 0x04; 0x08; 0x10; 0x20; 0x40; 0x80]

    // MnX (00, 02, 04, 06, 08, 0A, 0C, 0E, 10)
    let mobX = [|0;0;0;0;0;0;0;0|]
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
        value
    let SetHighMobX (value:int) =
        mobX.[0] <- (mobX.[0] &&& 0x0FF) ||| (if value &&& 0x01 <> 0 then 0x100 else 0x000)
        mobX.[1] <- (mobX.[1] &&& 0x0FF) ||| (if value &&& 0x02 <> 0 then 0x100 else 0x000)
        mobX.[2] <- (mobX.[2] &&& 0x0FF) ||| (if value &&& 0x04 <> 0 then 0x100 else 0x000)
        mobX.[3] <- (mobX.[3] &&& 0x0FF) ||| (if value &&& 0x08 <> 0 then 0x100 else 0x000)
        mobX.[4] <- (mobX.[4] &&& 0x0FF) ||| (if value &&& 0x10 <> 0 then 0x100 else 0x000)
        mobX.[5] <- (mobX.[5] &&& 0x0FF) ||| (if value &&& 0x20 <> 0 then 0x100 else 0x000)
        mobX.[6] <- (mobX.[6] &&& 0x0FF) ||| (if value &&& 0x40 <> 0 then 0x100 else 0x000)
        mobX.[7] <- (mobX.[7] &&& 0x0FF) ||| (if value &&& 0x80 <> 0 then 0x100 else 0x000)
        value

    // MxY (01, 03, 05, 07, 09, 0B, 0D, 0F)
    let mobY = [|0;0;0;0;0;0;0;0|]
    let GetMobY (index:int) =
        mobY.[index]
    let SetMobY (index:int, value:int) =
        mobY.[index] <- value
        value

    // RASTER/RST8 (11, 12) (high expects high bit in position 7 as if you wrote to the reg)
    let mutable rasterY = 0
    let mutable rasterYCompareValue = 0
    let IncrementRasterY () =
        rasterY <- if rasterY >= (config.CyclesPerRasterLine - 1) then 0 else (rasterY + 1)
        rasterY
    let GetLowRasterY () =
        rasterY &&& 0x0FF
    let SetLowRasterYCompareValue (value:int) =
        rasterYCompareValue <- (rasterYCompareValue &&& 0x100) ||| value
        value
    let SetHighRasterYCompareValue (value:int) =
        rasterYCompareValue <- (rasterYCompareValue &&& 0x0FF) ||| ((value &&& 0x80) <<< 1)
        value

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
        SetHighRasterYCompareValue(value) |> ignore
        extraColorMode <- (value &&& 0x40 <> 0x00)
        bitmapMode <- (value &&& 0x20 <> 0x00)
        displayEnabled <- (value &&& 0x10 <> 0x00)
        rowSelect <- (value &&& 0x08 <> 0x00)
        yScroll <- (value &&& 0x07)
        value

    // LPX (13) (it's a 9 bit register but we only keep the upper 8)
    let mutable lightPenX = 0
    let GetLightPenX () =
        lightPenX
    let SetLightPenX (value:int) =
        lightPenX <- value >>> 1
        value

    // LPY (14)
    let mutable lightPenY = 0
    let GetLightPenY () =
        lightPenY
    let SetLightPenY (value:int) =
        lightPenY <- value
        value

    // MnE (15)
    let mobEnabled = [|false;false;false;false;false;false;false;false|]
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
        value
        
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
        value

    // Mob Y Expansion (17)
    let mobYExpansionEnabled = [|false;false;false;false;false;false;false;false|]
    let mobYExpansionToggle = [|false;false;false;false;false;false;false;false|]
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
        value

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
        value

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
        value
    let SetInterruptEnable (value:int) =
        lightPenIrqEnabled <- (value &&& 0x08) <> 0
        mobMobCollisionIrqEnabled <- (value &&& 0x04) <> 0
        mobBackgroundCollisionIrq <- (value &&& 0x02) <> 0
        rasterIrqEnabled <- (value &&& 0x01) <> 0
        value
    let UpdateIrq () =
        irq <- (lightPenIrq && lightPenIrqEnabled) ||
            (mobMobCollisionIrq && mobMobCollisionIrqEnabled) ||
            (mobBackgroundCollisionIrq && mobBackgroundCollisionIrqEnabled) ||
            (rasterIrq && rasterIrqEnabled)

    // Sprite Data Priority (1B)
    let mobDataPriority = [|false;false;false;false;false;false;false;false|]
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
        value

    // Sprite Multicolor Enable (1C)
    let mobMultiColorEnabled = [|false;false;false;false;false;false;false;false|]
    let mobMultiColorToggle = [|false;false;false;false;false;false;false;false|]
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
        value
    
    // Sprite X Expansion Enable (1D)
    let mobXExpansionEnabled = [|false;false;false;false;false;false;false;false|]
    let mobXExpansionToggle = [|false;false;false;false;false;false;false;false|]
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
        value

    // Sprite-sprite Collision (1E)
    let mobMobCollision = [|false;false;false;false;false;false;false;false|]
    let GetMobMobCollision () =
        (if mobMobCollision.[0] then 0x01 else 0x00) |||
        (if mobMobCollision.[1] then 0x02 else 0x00) |||
        (if mobMobCollision.[2] then 0x04 else 0x00) |||
        (if mobMobCollision.[3] then 0x08 else 0x00) |||
        (if mobMobCollision.[4] then 0x10 else 0x00) |||
        (if mobMobCollision.[5] then 0x20 else 0x00) |||
        (if mobMobCollision.[6] then 0x40 else 0x00) |||
        (if mobMobCollision.[7] then 0x80 else 0x00)
    let ClearMobMobCollision () =
        mobMobCollision.[0] <- false
        mobMobCollision.[1] <- false
        mobMobCollision.[2] <- false
        mobMobCollision.[3] <- false
        mobMobCollision.[4] <- false
        mobMobCollision.[5] <- false
        mobMobCollision.[6] <- false
        mobMobCollision.[7] <- false
        0

    // Sprite-background Collision (1F)
    let mobDataCollision = [|false;false;false;false;false;false;false;false|]
    let GetMobDataCollision () =
        (if mobDataCollision.[0] then 0x01 else 0x00) |||
        (if mobDataCollision.[1] then 0x02 else 0x00) |||
        (if mobDataCollision.[2] then 0x04 else 0x00) |||
        (if mobDataCollision.[3] then 0x08 else 0x00) |||
        (if mobDataCollision.[4] then 0x10 else 0x00) |||
        (if mobDataCollision.[5] then 0x20 else 0x00) |||
        (if mobDataCollision.[6] then 0x40 else 0x00) |||
        (if mobDataCollision.[7] then 0x80 else 0x00)
    let ClearMobDataCollision () =
        mobDataCollision.[0] <- false
        mobDataCollision.[1] <- false
        mobDataCollision.[2] <- false
        mobDataCollision.[3] <- false
        mobDataCollision.[4] <- false
        mobDataCollision.[5] <- false
        mobDataCollision.[6] <- false
        mobDataCollision.[7] <- false
        0

    // Border Color (20)
    let mutable borderColor = 0
    let GetBorderColor () =
        borderColor ||| 0xF0
    let SetBorderColor (value:int) =
        borderColor <- value &&& 0x0F
        value

    // Background Colors (21, 22, 23, 24)
    let backgroundColor = [|0;0;0;0|]
    let GetBackgroundColor (index:int) =
        backgroundColor.[index] ||| 0xF0
    let SetBackgroundColor (index:int, value:int) =
        backgroundColor.[index] <- value &&& 0x0F

    // Sprite Multicolors (25, 26)
    let mobMultiColor = [|0;0|]
    let GetMobMultiColor (index:int) =
        mobMultiColor.[index] ||| 0xF0
    let SetMobMultiColor (index:int, value:int) =
        mobMultiColor.[index] <- value &&& 0x0F

    // Sprite Colors (27, 28, 29, 2A, 2B, 2C, 2D, 2E)
    let mobColor = [|0;0;0;0;0;0;0;0|]
    let GetMobColor (index:int) =
        mobColor.[index] ||| 0xF0
    let SetMobColor (index:int, value:int) =
        mobColor.[index] <- value &&& 0x0F

    // Raster X Counter (for sprites)
    let mutable rasterX = 0


    let mutable vBlank = true
    let mutable hBlank = true

    // Raster Line Counter (not identical to X)
    let mutable rasterLineCounter = 0

    let IncrementRasterLineCounter () =
        rasterLineCounter <- if rasterLineCounter >= (config.CyclesPerRasterLine - 1) then 0 else rasterLineCounter + 1
        rasterLineCounter



    // RC
    let mutable rowCounter = 0

    let IncrementRowCounter () =
        rowCounter <- (rowCounter + 1) &&& 0x7
        rowCounter
    let ResetRowCounter () =
        rowCounter <- 0
        0

    // VC, VCBASE, VMLI
    let mutable videoCounter = 0
    let mutable videoCounterBase = 0
    let mutable videoMatrixLineIndex = 0

    let IncrementVideoCounter () =
        videoCounter <- (videoCounter + 1) &&& 0x3FF
        videoMatrixLineIndex <- (videoMatrixLineIndex + 1) &&& 0x3F
        videoMatrixLineIndex
    let IncrementVideoCounterBase () =
        videoCounterBase <- videoCounter
        videoCounterBase
    let ResetVideoCounter () =
        videoCounter <- videoCounterBase
        videoMatrixLineIndex <- 0
        0
    let ResetVideoCounterBase () =
        videoCounterBase <- 0
        0

    // Display/Idle state
    let mutable displayState = false

    let GoToDisplayState () =
        displayState <- true
        displayState
    let GoToIdleState () =
        displayState <- false
        displayState





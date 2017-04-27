namespace Breadbox

// 6567 core.

[<Sealed>]
type Mos6567Configuration(read:System.Func<int, int>, lines:int, cyclesPerLine:int, vBlankStart:int, vBlankEnd:int, xStart:int, yStart:int) =
    member val Read = read.Invoke
    member val Lines = lines
    member val VBlankStart = vBlankStart
    member val VBlankEnd = vBlankEnd
    member val CyclesPerLine = cyclesPerLine
    member val XStart = xStart
    member val YStart = yStart

[<Sealed>]
type Mos6567(config:Mos6567Configuration) =
    let ReadMemory = config.Read
    let clocksPerLine = config.CyclesPerLine * 8
    let holdClocks = System.Math.Min(0x200 - clocksPerLine, 0)
    let hBlankStart = 0x184 - ((0x200 - clocksPerLine) * 8)
    let hBlankEnd = 0x19C - ((0x200 - System.Math.Min(0x200, clocksPerLine)))
    let fetchStart = clocksPerLine - 0x094
    let baStart = fetchStart - 0x018

    let bitFlags = Array.init 8 <| fun i -> 1 <<< i

    let mutable irq = false
    let mutable ba = true
    let mutable aec = true

    let mutable vblank = true
    let mutable hblank = true
    let mutable vc = 0x00
    let mutable vcbase = 0x00
    let mutable rc = 0x0
    let mutable vmli = 0x00

    let mnx = Array.create 8 0x000
    let mny = Array.create 8 0x00
    let mutable den = false
    let mutable rsel = 0x0
    let mutable yscroll = 0x0
    let mutable raster = 0x000
    let mutable lpx = 0x00
    let mutable lpy = 0x00
    let mne = Array.create 8 false
    let mutable res = false
    let mutable csel = 0x0
    let mutable xscroll = 0x0
    let mnye = Array.create 8 false
    let mutable vm = 0x0000
    let mutable cb = 0x0000
    let mutable ilp = false
    let mutable immc = false
    let mutable imbc = false
    let mutable irst = false
    let mutable elp = false
    let mutable emmc = false
    let mutable embc = false
    let mutable erst = false
    let mndp = Array.create 8 0x0
    let mnmc = Array.create 8 0x0
    let mnxe = Array.create 8 false
    let mnm = Array.create 8 false
    let mnd = Array.create 8 false
    let mutable ec = 0x0
    let bnc = Array.create 4 0x0
    let mmn = Array.create 2 0x0
    let mnc = Array.create 8 0x0

    // Border unit compare values
    let rselTop = [| 0x037, 0x033 |]
    let rselBottom = [| 0x0F7, 0x0FB |]
    let cselLeft = [| 0x01F, 0x018 |]
    let cselRight = [| 0x14F, 0x158 |]

    // Raster interrupt compare value
    let mutable rasterI = 0x000

    // Sprite pointer
    let mpn = Array.create 8 0x00

    // Sprite fetch index
    let mcn = Array.create 8 0x00

    // Sprite data shift register + enable
    let msr = Array.create 8 0x000000
    let msre = Array.create 8 0x0

    // Refresh fetch address
    let mutable refresh = 0x00
    
    // Video matrix
    let matrix = Array.create 40 0x000

    // Graphics data fetch buffer
    let mutable gbuffer = 0x00

    // Color matrix data fetch buffer
    let mutable cbuffer = 0x000

    // Color matrix data latch
    let mutable coutput = 0x000

    // Graphics data shift register
    let mutable gsr = 0x00

    // Badline status
    let mutable badline = false

    // Graphics data fetch modes (bitfield: 1=MCM, 2=BMM, 4=ECM, 8=idle state)
    let graphicsModeCount = 16
    let mutable graphicsMode = 0
    let fetchGAddressModes = 
        let ec address = address &&& 0x39FF
        let text index = (((matrix.[index] &&& 0xFF) <<< 3) ||| cb ||| rc)
        let bitmap _ = ((vc <<< 3) ||| (cb &&& 0x2000) ||| rc)
        let idle _ = 0x3FFF
        Array.init graphicsModeCount <| fun i ->
            match (i &&& 0x2) <> 0, (i &&& 0x4) <> 0, (i &&& 0x8) <> 0 with
                | false, false, false -> text
                | false, true,  false -> text >> ec
                | true,  false, false -> bitmap
                | true,  true,  false -> bitmap >> ec
                | _,     false, true  -> idle
                | _,     true,  true  -> idle >> ec

    // Foreground/Background data bit modes
    let synthGDataModes =
        let oneBit g _ = (g &&& 1) ||| ((g &&& 1) <<< 1)
        let twoBits g _ = (g &&& 3)
        let multiColorTextModes = [| oneBit; twoBits |]
        let multiColorText g c = multiColorTextModes.[c >>> 11] g c
        Array.init graphicsModeCount <| fun i ->
            match (i &&& 0x1) <> 0, (i &&& 0x2) <> 0 with
                | false, false -> oneBit
                | false, true  -> oneBit
                | true,  false -> multiColorText
                | true,  true  -> twoBits

    // Graphics color modes
    let synthGColorModes = 
        let upperColor c = (c >>> 8)
        let upperColorMulti c = (c >>> 8) &&& 0x7
        let midColor c = (c >>> 4) &&& 0xF
        let lowColor c = (c &&& 0xF)
        let backgroundColor _ = bnc.[0]
        let color1 _ = bnc.[1]
        let color2 _ = bnc.[2]
        let extraColor c = bnc.[(c >>> 6) &&& 0x3]
        let black _ = 0
        let singleColorTextMode = [| backgroundColor, backgroundColor, upperColor, upperColor |]
        let multiColorTextMode = [| backgroundColor, color1, color2, upperColorMulti |]
        let singleColorBitmapMode = [| lowColor, lowColor, midColor, midColor |]
        let multiColorBitmapMode = [| backgroundColor, midColor, lowColor, upperColor |]
        let extraColorMode = [| extraColor, extraColor, upperColor, upperColor |]
        let idleSingleColorTextMode = [| backgroundColor, backgroundColor, black, black |]
        let idleMultiColorTextMode = [| backgroundColor, color1, color2, black |]
        let idleSingleColorBitmapMode = [| black, black, black, black |]
        let idleMultiColorBitmapMode = [| backgroundColor, black, black, black |]
        let idleExtraColorMode = [| backgroundColor, backgroundColor, black, black |]
        let allBlack = [| black, black, black, black |]
        Array.init graphicsModeCount <| fun i ->
            match (i &&& 0x1) <> 0, (i &&& 0x2) <> 0, (i &&& 0x4) <> 0, (i &&& 0x8) <> 0 with
                | false, false, false, false -> singleColorTextMode
                | true,  false, false, false -> multiColorTextMode
                | false, true,  false, false -> singleColorBitmapMode
                | true,  true,  false, false -> multiColorBitmapMode
                | false, false, true,  false -> extraColorMode
                | false, false, false, true  -> idleSingleColorTextMode
                | true,  false, false, true  -> idleMultiColorTextMode
                | false, true,  false, true  -> idleSingleColorBitmapMode
                | true,  true,  false, true  -> idleMultiColorBitmapMode
                | false, false, true,  true  -> idleExtraColorMode
                | _,     _,     _,     _     -> allBlack
            
    // Sprite data bit modes (1=MC, 2=DP)
    let spriteModeCount = 4
    let spriteMode = Array.zeroCreate 8
    let synthSDataModes =
        let oneBit s = (s &&& 1)
        let twoBits s = (s &&& 3)
        Array.init spriteModeCount <| fun i ->
            match (i &&& 0x1) <> 0 with
                | false -> oneBit
                | true  -> twoBits

    // Sprite color modes
    let synthSColorModes =
        let noColor _ = 0
        let multi0 _ = mmn.[0]
        let multi1 _ = mmn.[1]
        let mobColor = id
        let anyColorMode = [| noColor, multi0, mobColor, multi1 |]
        Array.init spriteModeCount <| fun _ ->
            anyColorMode
    
    // Sprite priority modes
    let synthSPriorityModes =
        let showSpriteColor spriteColor _ _ = spriteColor()
        let showGraphicsColor _ _ graphicsColor = graphicsColor()
        let backgroundModes = [| showSpriteColor; showGraphicsColor |]
        let background spriteColor graphicsData graphicsColor = backgroundModes.[graphicsData >>> 1] spriteColor graphicsData graphicsColor
        Array.init spriteModeCount <| fun i ->
            match (i &&& 0x2) <> 0 with
                | false -> showSpriteColor
                | true  -> background

    // Sprite + Graphics mux
    let muxSG =
        // Lookup table for sprite/sprite collisions
        let spriteCollisionMap = Array.init 0x100 <| fun i _ ->
            match i with
                | 0x01 | 0x02 | 0x04 | 0x08 | 0x10 | 0x20 | 0x40 | 0x80 -> 0x00
                | _ -> i
        // Bits in the collision mask per sprite
        let spriteCollisionOutput = Array.init 8 <| fun i -> 1 <<< i
        // Sprite data source depending if shift register is enabled
        let shiftRegisterSource = [|
            fun _ -> 0;
            fun i -> synthSDataModes.[spriteMode.[i]] <| msr.[i]
        |]
        // Determine sprite/sprite priority
        let prioritySort = Array.init 8 <| fun s ->
            Array.init 4 <| fun i ->
                match i with
                    | 0 -> id
                    | _ ->
                        let result = s ||| (i <<< 3)
                        fun _ -> result
        // Determine sprite collision
        let collisionSort = Array.init 8 <| fun s ->
            Array.init 4 <| fun i ->
                let bit = spriteCollisionOutput.[s]
                match i with
                    | 0 -> id
                    | _ -> fun j -> j ||| s
        // Get actual sprite color output, index 8 is no sprite
        let spriteColorOut = Array.init 9 <| fun s ->
            match s with
                | s when s >= 0 && s < 8 ->
                    synthSPriorityModes.[spriteMode.[s]]
                | _ ->
                    fun _ _ graphicsColor -> graphicsColor()
        let graphicsOutput = fun _ ->
            synthGDataModes.[graphicsMode] gsr coutput
        
        fun _ -> 0

    let test1 = muxSG ()
        
    // Fetch idle
    let fetchI () =
        ReadMemory 0x3FFF |> ignore

    // Fetch sprite pointer
    let fetchP index () =
        mpn.[index] <- (ReadMemory (0x03F8 ||| index)) <<< 6

    // Fetch upper 8 bits of sprite data
    let fetchS0 index () =
        msr.[index] <- msr.[index] ||| ((ReadMemory (mpn.[index] ||| mcn.[index])) <<< 16)

    // Fetch middle 8 bits of sprite data
    let fetchS1 index () =
        msr.[index] <- msr.[index] ||| ((ReadMemory (mpn.[index] ||| mcn.[index])) <<< 8)

    // Fetch lower 8 bits of sprite data
    let fetchS2 index () =
        msr.[index] <- msr.[index] ||| (ReadMemory (mpn.[index] ||| mcn.[index]))
    
    // Fetch refresh
    let fetchR () =
        let newRefresh = (refresh - 1) &&& 0xFF
        ReadMemory 0x3F00 ||| newRefresh |> ignore
        refresh <- newRefresh

    // Current color matrix fetch mode
    let mutable fetchCInternal = fun _ -> 0x000

    // Fetch color matrix data
    let fetchC index () =
        cbuffer <- fetchCInternal index

    // Fetch graphics data
    let fetchG index () =
        gbuffer <- (ReadMemory <| fetchGAddressModes.[graphicsMode] index)

    // Change the badline status
    let setBadline newBadline =
        badline <- newBadline

        fetchCInternal <-
            if newBadline then
                fun index ->
                    let data = ReadMemory (vm ||| vc)
                    matrix.[index] <- data
                    cbuffer <- data
                    data
            else
                fun index ->
                    let data = matrix.[index]
                    cbuffer <- data
                    data
    
    // Change the graphics mode
    let setGraphicsMode newMode =
        graphicsMode <- (graphicsMode &&& (~~~0x07)) ||| (newMode &&& 0x07)

    // Change the idle state
    let setIdleState newIdleState =
        graphicsMode <- (graphicsMode &&& (~~~0x08)) ||| (if newIdleState then 0x08 else 0x00)

    // Border status
    let mutable borderMode = 0x0
    let borderModeCount = 4

    // Change the vertical border state
    let setVerticalBorder newState =
        borderMode <- (borderMode &&& (~~~0x01)) ||| (if newState then 0x01 else 0x00)

    // Change the main border state
    let setMainBorder newState =
        borderMode <- (borderMode &&& (~~~0x02)) ||| (if newState then 0x02 else 0x00)








    let getBitmask (arr:bool[]) =
        (if arr.[0] then 0x01 else 0x00) |||
        (if arr.[1] then 0x02 else 0x00) |||
        (if arr.[2] then 0x04 else 0x00) |||
        (if arr.[3] then 0x08 else 0x00) |||
        (if arr.[4] then 0x10 else 0x00) |||
        (if arr.[5] then 0x20 else 0x00) |||
        (if arr.[6] then 0x40 else 0x00) |||
        (if arr.[7] then 0x80 else 0x00)

    let setBitmask (arr:bool[]) value =
        arr.[0] <- (value &&& 0x01) <> 0
        arr.[1] <- (value &&& 0x02) <> 0
        arr.[2] <- (value &&& 0x04) <> 0
        arr.[3] <- (value &&& 0x08) <> 0
        arr.[4] <- (value &&& 0x10) <> 0
        arr.[5] <- (value &&& 0x20) <> 0
        arr.[6] <- (value &&& 0x40) <> 0
        arr.[7] <- (value &&& 0x80) <> 0

    let getValueMask (arr:int[]) =
        arr.[0] |||
        (arr.[1] <<< 1) |||
        (arr.[2] <<< 2) |||
        (arr.[3] <<< 3) |||
        (arr.[4] <<< 4) |||
        (arr.[5] <<< 5) |||
        (arr.[6] <<< 6) |||
        (arr.[7] <<< 7)

    let setValueMask (arr:int[]) value =
        arr.[0] <- (value &&& 0x01)
        arr.[1] <- (value &&& 0x02)
        arr.[2] <- (value &&& 0x04)
        arr.[3] <- (value &&& 0x08)
        arr.[4] <- (value &&& 0x10)
        arr.[5] <- (value &&& 0x20)
        arr.[6] <- (value &&& 0x40)
        arr.[7] <- (value &&& 0x80)

    let setMnxLow index value =
        mnx.[index] <- mnx.[index] &&& 0x100 ||| value
    
    let setMnxHigh value =
        mnx.[0] <- mnx.[0] &&& 0x0FF ||| ((value &&& 0x01) <<< 8)
        mnx.[1] <- mnx.[1] &&& 0x0FF ||| ((value &&& 0x02) <<< 7)
        mnx.[2] <- mnx.[2] &&& 0x0FF ||| ((value &&& 0x04) <<< 6)
        mnx.[3] <- mnx.[3] &&& 0x0FF ||| ((value &&& 0x08) <<< 5)
        mnx.[4] <- mnx.[4] &&& 0x0FF ||| ((value &&& 0x10) <<< 4)
        mnx.[5] <- mnx.[5] &&& 0x0FF ||| ((value &&& 0x20) <<< 3)
        mnx.[6] <- mnx.[6] &&& 0x0FF ||| ((value &&& 0x40) <<< 2)
        mnx.[7] <- mnx.[7] &&& 0x0FF ||| ((value &&& 0x80) <<< 1)

    let setMny index value =
        mny.[index] <- value

    let setControl1 value =
        rasterI <- rasterI &&& 0x0FF ||| ((value &&& 0x80) <<< 1)
        setGraphicsMode (((value &&& 0x60) >>> 4) ||| (graphicsMode &&& 0x1))
        den <- (value &&& 0x10) <> 0
        rsel <- (value &&& 0x08) >>> 3
        yscroll <- (value &&& 0x07)

    let setRaster value =
        rasterI <- rasterI &&& 0x100 ||| value

    let setSpriteEnable = setBitmask mne

    let setControl2 value =
        res <- (value &&& 0x20) <> 0
        setGraphicsMode (((value &&& 0x10) >>> 4) ||| (graphicsMode &&& 0x6))
        csel <- (value &&& 0x08) >>> 3
        xscroll <- (value &&& 0x7)

    let setSpriteYExpansion = setBitmask mnye
    
    let setMemoryPointers value =
        vm <- (value &&& 0xF0) <<< 6
        cb <- (value &&& 0x0E) <<< 10

    let setInterruptEnable value =
        elp <- (value &&& 0x08) <> 0
        emmc <- (value &&& 0x04) <> 0
        embc <- (value &&& 0x02) <> 0
        erst <- (value &&& 0x01) <> 0

    let setSpritePriority = setValueMask mndp

    let setSpriteMulticolorEnable = setValueMask mnmc
    
    let setSpriteXExpansion = setBitmask mnxe

    let setBorderColor value =
        ec <- value &&& 0xF

    let setBackgroundColor index value =
        bnc.[index] <- value &&& 0xF
    
    let setSpriteMulticolor index value =
        mmn.[index] <- value &&& 0xF

    let setSpriteColor index value =
        mnc.[index] <- value &&& 0xF

    let getMnxLow index () =
        mnx.[index] &&& 0xFF

    let getMnxHigh () =
        ((mnx.[0] &&& 0x100) >>> 8) |||
        ((mnx.[1] &&& 0x100) >>> 7) |||
        ((mnx.[2] &&& 0x100) >>> 6) |||
        ((mnx.[3] &&& 0x100) >>> 5) |||
        ((mnx.[4] &&& 0x100) >>> 4) |||
        ((mnx.[5] &&& 0x100) >>> 3) |||
        ((mnx.[6] &&& 0x100) >>> 2) |||
        ((mnx.[7] &&& 0x100) >>> 1)

    let getMny index () =
        mny.[index]

    let getControl1 () =
        ((raster &&& 0x100) >>> 1) |||
        ((graphicsMode &&& 0x6) <<< 4) |||
        (if den then 0x10 else 0x00) |||
        (rsel <<< 3) |||
        yscroll

    let getRaster () =
        raster &&& 0xFF

    let getLpx () =
        lpx

    let getLpy () =
        lpy

    let getSpriteEnable () = getBitmask mne

    let getControl2 () =
        0xE0 |||
        (if res then 0x20 else 0x00) |||
        ((graphicsMode &&& 0x1) <<< 4) |||
        (csel <<< 3) |||
        xscroll

    let getSpriteYExpansion () = getBitmask mnye
    
    let getMemoryPointers () =
        (vm >>> 6) |||
        (cb >>> 10) |||
        0x01

    let getInterruptRegisters () =
        (if irq then 0x80 else 0x00) |||
        (if ilp then 0x08 else 0x00) |||
        (if immc then 0x04 else 0x00) |||
        (if imbc then 0x02 else 0x00) |||
        (if irst then 0x01 else 0x00) |||
        0x70

    let getInterruptEnable () =
        (if elp then 0x08 else 0x00) |||
        (if emmc then 0x04 else 0x00) |||
        (if embc then 0x02 else 0x00) |||
        (if erst then 0x01 else 0x00) |||
        0xF0

    let getSpritePriority () = getValueMask mndp

    let getSpriteMulticolorEnable () = getValueMask mnmc

    let getSpriteXExpansion () = getBitmask mnxe

    let getSpriteSpriteCollision () = getBitmask mnm

    let getSpriteDataCollision () = getBitmask mnd

    let getBorderColor () = ec ||| 0xF0

    let getBackgroundColor index () = bnc.[index] ||| 0xF0

    let getSpriteMulticolor index () = mmn.[index] ||| 0xF0

    let getSpriteColor index () = mnc.[index] ||| 0xF0

    let getUnconnected () = 0xFF

    let clearSpriteSpriteCollision () = setBitmask mnm 0

    let clearSpriteDataCollision () = setBitmask mnd 0

    // Read a register without side effects
    let peekRegister = [|
        // 0x00
        getMnxLow 0;
        getMny 0;
        getMnxLow 1;
        getMny 1;
        getMnxLow 2;
        getMny 2;
        getMnxLow 3;
        getMny 3;
        // 0x08
        getMnxLow 4;
        getMny 4;
        getMnxLow 5;
        getMny 5;
        getMnxLow 6;
        getMny 6;
        getMnxLow 7;
        getMny 7;
        // 0x10
        getMnxHigh;
        getControl1;
        getRaster;
        getLpx;
        getLpy;
        getSpriteEnable;
        getControl2;
        getSpriteYExpansion;
        // 0x18
        getMemoryPointers;
        getInterruptRegisters;
        getInterruptEnable;
        getSpritePriority;
        getSpriteMulticolorEnable;
        getSpriteXExpansion;
        getSpriteSpriteCollision;
        getSpriteDataCollision;
        // 0x20
        getBorderColor;
        getBackgroundColor 0;
        getBackgroundColor 1;
        getBackgroundColor 2;
        getBackgroundColor 3;
        getSpriteMulticolor 0;
        getSpriteMulticolor 1;
        getSpriteColor 0;
        // 0x28
        getSpriteColor 1;
        getSpriteColor 2;
        getSpriteColor 3;
        getSpriteColor 4;
        getSpriteColor 5;
        getSpriteColor 6;
        getSpriteColor 7;
        getUnconnected;
        // 0x30
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        // 0x38
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
    |]

    // Read a register considering side effects
    let readRegister = Array.init peekRegister.Length <| fun i ->
        match i with
            | 0x1E -> clearSpriteSpriteCollision >> peekRegister.[i]
            | 0x1F -> clearSpriteDataCollision >> peekRegister.[i]
            | _ -> peekRegister.[i]
    
    // Write a register without side effects
    let pokeRegister = [|
        // 0x00
        setMnxLow 0;
        setMny 0;
        setMnxLow 1;
        setMny 1;
        setMnxLow 2;
        setMny 2;
        setMnxLow 3;
        setMny 3;
        // 0x08
        setMnxLow 4;
        setMny 4;
        setMnxLow 5;
        setMny 5;
        setMnxLow 6;
        setMny 6;
        setMnxLow 7;
        setMny 7;
        // 0x10
        setMnxHigh;
        setControl1;
        setRaster;
        ignore;
        ignore;
        setSpriteEnable;
        setControl2;
        setSpriteYExpansion;
        // 0x18
        setMemoryPointers;
        ignore;
        setInterruptEnable;
        setSpritePriority;
        setSpriteMulticolorEnable;
        setSpriteXExpansion;
        ignore;
        ignore;
        // 0x20
        setBorderColor;
        setBackgroundColor 0;
        setBackgroundColor 1;
        setBackgroundColor 2;
        setBackgroundColor 3;
        setSpriteMulticolor 0;
        setSpriteMulticolor 1;
        setSpriteColor 0;
        // 0x28
        setSpriteColor 1;
        setSpriteColor 2;
        setSpriteColor 3;
        setSpriteColor 4;
        setSpriteColor 5;
        setSpriteColor 6;
        setSpriteColor 7;
        ignore;
        // 0x30
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        // 0x38
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
    |]

    // Write a register considering side effects
    let writeRegister = Array.init pokeRegister.Length <| fun i ->
        match i with
            | _ -> pokeRegister.[i]

    member this.Irq = irq
    member this.Ba = ba
    member this.Aec = aec

    member this.Bmm
        with get () = (graphicsMode &&& 0x2) <> 0
        and set (value) = setGraphicsMode ((graphicsMode &&& 0x5) ||| (if value then 0x2 else 0x0))

    member this.Ecm
        with get () = (graphicsMode &&& 0x4) <> 0
        and set (value) = setGraphicsMode ((graphicsMode &&& 0x3) ||| (if value then 0x4 else 0x0))

    member this.Mcm
        with get () = (graphicsMode &&& 0x1) <> 0
        and set (value) = setGraphicsMode ((graphicsMode &&& 0x6) ||| (if value then 0x1 else 0x0))

    member this.Peek address =
        peekRegister.[address &&& 0x3F] <| ()

    member this.Read address =
        readRegister.[address &&& 0x3F] <| ()

    member this.Poke address value =
        pokeRegister.[address &&& 0x3F] <| (value &&& 0xFF)

    member this.Write address value =
        writeRegister.[address &&& 0x3F] <| (value &&& 0xFF)

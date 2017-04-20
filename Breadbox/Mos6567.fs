﻿namespace Breadbox

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
    let mutable ecm = false
    let mutable bmm = false
    let mutable den = false
    let mutable rsel = false
    let mutable yscroll = 0x0
    let mutable raster = 0x000
    let mutable lpx = 0x00
    let mutable lpy = 0x00
    let mne = Array.create 8 false
    let mutable res = false
    let mutable mcm = false
    let mutable csel = false
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
    let mndp = Array.create 8 false
    let mnmc = Array.create 8 false
    let mnxe = Array.create 8 false
    let mnm = Array.create 8 false
    let mnd = Array.create 8 false
    let mutable ec = 0x0
    let bnc = Array.create 4 0x0
    let mmn = Array.create 2 0x0
    let mnc = Array.create 8 0x0
    let mutable rasterI = 0x000

    let mpn = Array.create 8 0x00
    let mcn = Array.create 8 0x00
    let msr = Array.create 8 0x000000
    let mutable refresh = 0x00
    
    let matrix = Array.create 40 0x000
    let mutable gbuffer = 0x00
    let mutable cbuffer = 0x000
    let mutable coutput = 0x000
    let mutable gsr = 0x00
    let mutable badline = false

    let fetchI () =
        ReadMemory 0x3FFF |> ignore

    let fetchP index () =
        mpn.[index] <- (ReadMemory (0x03F8 ||| index)) <<< 6

    let fetchS0 index () =
        msr.[index] <- msr.[index] ||| ((ReadMemory (mpn.[index] ||| mcn.[index])) <<< 16)

    let fetchS1 index () =
        msr.[index] <- msr.[index] ||| ((ReadMemory (mpn.[index] ||| mcn.[index])) <<< 8)

    let fetchS2 index () =
        msr.[index] <- msr.[index] ||| (ReadMemory (mpn.[index] ||| mcn.[index]))

    let fetchR () =
        let newRefresh = (refresh - 1) &&& 0xFF
        ReadMemory 0x3F00 ||| newRefresh |> ignore
        refresh <- newRefresh

    let fetchC index () =
        if badline then
            let data = ReadMemory (vm ||| vc)
            matrix.[index] <- data
            cbuffer <- data
        else
            cbuffer <- matrix.[index]

    let getFetchGInternal () =
        let inline ec address = address &&& 0x39FF
        let inline text index = ((matrix.[index] &&& 0xFF) <<< 3) ||| cb ||| rc
        let inline bitmap index = (vc <<< 3) ||| (cb &&& 0x2000) ||| rc
        match bmm, ecm with
            | false, false -> text
            | false, _     -> text >> ec
            | true, false  -> bitmap
            | _,    _      -> bitmap >> ec

    let mutable fetchGInternal = getFetchGInternal()

    let fetchG index () =
        gbuffer <- (ReadMemory <| fetchGInternal index)

    let getClockMux mcm bmm ecm =
        let getDataOutput =
            let inline singleColor c g = ((g &&& 0x1) <<< 1) ||| (g &&& 0x1)
            let inline multiColor c g = g &&& 0x3
            let inline multiColorText c g = if (c &&& 0x800) <> 0 then multiColor c g else singleColor c g
            match mcm,   bmm with
                | false, _     -> singleColor
                | _,     false -> multiColorText
                | _,     _     -> multiColor
        
        let getDataColorOutput =
            let inline invalid c d = 0
            let inline extraColorText c d = if (d <> 0) then (c >>> 8) else bnc.[(c >>> 6) &&& 0x3]
            let inline singleColorText c d = if (d <> 0) then (c >>> 8) else bnc.[0]
            let inline multiColorText c d = if (d < 3) then bnc.[d] else (c >>> 8)
            let inline singleColorBitmap c d = (if (d <> 0) then c else (c >>> 4)) &&& 0xF
            let inline multiColorBitmap c d =
                match d with
                    | 0b01 -> (c >>> 4) &&& 0xF 
                    | 0b10 -> c &&& 0xF
                    | 0b11 -> c >>> 8
                    | _ -> bnc.[0]
            match mcm,   bmm,   ecm with
                | false, false, true  -> extraColorText
                | _,     _,     true  -> invalid
                | false, false, _     -> singleColorText
                | false, true,  _     -> singleColorBitmap
                | _,     false, _     -> multiColorText
                | _,     _,     _     -> multiColorBitmap
    
        let getDataColorIdleOutput =
            let inline black c d = 0
            let inline blackPlusBackground c d = if (d <> 0) then 0 else bnc.[0]
            match mcm,   bmm,   ecm with
                | false, false, true  -> blackPlusBackground
                | _,     _,     true
                | false, true,  false -> black
                | _,     _,     _     -> blackPlusBackground

        let inline getSpriteOutput index =
            if mnmc.[index] then
                msr.[index] &&& 0x3
            else
                (msr.[index] &&& 0x1) <<< 1

        let inline getSpriteColorOutput index d =
            if d = 0b10 then mnc.[index] else mmn.[d >>> 1]

        let inline collideSprites d =
            let mutable frontMost = -1
            let mutable spriteCollision = 0x00
            let mutable dataCollision = 0x00
            for sprite = 0 to 7 do
                let output = getSpriteOutput sprite
                if (output <> 0) then
                    if (frontMost >= 0) then
                        mnm.[sprite] <- true
                        mnm.[frontMost] <- true
                    else
                        frontMost <- sprite
                    if (d >= 0b10) then
                        mnd.[sprite] <- true
            frontMost

        fun _ ->
            let graphicsDataOutput = getDataOutput coutput gsr
            let visibleSpriteNumber = collideSprites graphicsDataOutput
            if visibleSpriteNumber < 0 || mndp.[visibleSpriteNumber] then
                getDataColorOutput coutput graphicsDataOutput
            else
                getSpriteColorOutput visibleSpriteNumber (getSpriteOutput visibleSpriteNumber)

    let timingX =
        Array.init clocksPerLine <| fun x ->
            match x with
                | x when x <= hBlankStart -> x
                | x when x - hBlankStart < holdClocks -> hBlankStart
                | _ -> x - holdClocks
            
    let timingFetch =
        let noop = fun () -> ()
        let result = Array.init clocksPerLine <| fun x ->
            match x with
                | x when (x &&& 0x7) = 0x4 -> fetchI
                | _ -> noop
        for x = 0 to 125 do
            result.[((x * 4) + fetchStart) % clocksPerLine] <-
                match x with
                    | x when x < 32 && (x &&& 3) = 0 ->  fetchP  (x >>> 2)
                    | x when x < 32 && (x &&& 3) = 1 ->  fetchS0 (x >>> 2)
                    | x when x < 32 && (x &&& 3) = 2 ->  fetchS1 (x >>> 2)
                    | x when x < 32 && (x &&& 3) = 3 ->  fetchS2 (x >>> 2)
                    | x when x < 41 && (x &&& 1) = 0 ->  fetchR
                    | x when x < 121 && (x &&& 1) = 1 -> fetchC  ((x - 41) >>> 1)
                    | x when x < 121 && (x &&& 1) = 0 -> fetchG  ((x - 41) >>> 1)
                    | x when x &&& 0x1 = 0            -> fetchI
                    | _ ->                                 noop

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
        ecm <- (value &&& 0x40) <> 0
        bmm <- (value &&& 0x20) <> 0
        den <- (value &&& 0x10) <> 0
        rsel <- (value &&& 0x08) <> 0
        yscroll <- (value &&& 0x07)

    let setRaster value =
        rasterI <- rasterI &&& 0x100 ||| value

    let setSpriteEnable = setBitmask mne

    let setControl2 value =
        res <- (value &&& 0x20) <> 0
        mcm <- (value &&& 0x10) <> 0
        csel <- (value &&& 0x08) <> 0
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

    let setSpritePriority = setBitmask mndp

    let setSpriteMulticolorEnable = setBitmask mnmc
    
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
        (if ecm then 0x40 else 0x00) |||
        (if bmm then 0x20 else 0x00) |||
        (if den then 0x10 else 0x00) |||
        (if rsel then 0x08 else 0x00) |||
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
        (if mcm then 0x10 else 0x00) |||
        (if csel then 0x08 else 0x00) |||
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

    let getSpritePriority () = getBitmask mndp

    let getSpriteMulticolorEnable () = getBitmask mnmc

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

    let peekRegister = [|
        getMnxLow 0;
        getMny 0;
        getMnxLow 1;
        getMny 1;
        getMnxLow 2;
        getMny 2;
        getMnxLow 3;
        getMny 3;
        getMnxLow 4;
        getMny 4;
        getMnxLow 5;
        getMny 5;
        getMnxLow 6;
        getMny 6;
        getMnxLow 7;
        getMny 7;

        getMnxHigh;
        getControl1;
        getRaster;
        getLpx;
        getLpy;
        getSpriteEnable;
        getControl2;
        getSpriteYExpansion;
        getMemoryPointers;
        getInterruptRegisters;
        getInterruptEnable;
        getSpritePriority;
        getSpriteMulticolorEnable;
        getSpriteXExpansion;
        getSpriteSpriteCollision;
        getSpriteDataCollision;

        getBorderColor;
        getBackgroundColor 0;
        getBackgroundColor 1;
        getBackgroundColor 2;
        getBackgroundColor 3;
        getSpriteMulticolor 0;
        getSpriteMulticolor 1;
        getSpriteColor 0;
        getSpriteColor 1;
        getSpriteColor 2;
        getSpriteColor 3;
        getSpriteColor 4;
        getSpriteColor 5;
        getSpriteColor 6;
        getSpriteColor 7;
        getUnconnected;

        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
        getUnconnected;
    |]

    let readRegister = Array.init peekRegister.Length <| fun i ->
        match i with
            | 0x1E -> clearSpriteSpriteCollision >> peekRegister.[i]
            | 0x1F -> clearSpriteDataCollision >> peekRegister.[i]
            | _ -> peekRegister.[i]

    let pokeRegister = [|
        setMnxLow 0;
        setMny 0;
        setMnxLow 1;
        setMny 1;
        setMnxLow 2;
        setMny 2;
        setMnxLow 3;
        setMny 3;
        setMnxLow 4;
        setMny 4;
        setMnxLow 5;
        setMny 5;
        setMnxLow 6;
        setMny 6;
        setMnxLow 7;
        setMny 7;

        setMnxHigh;
        setControl1;
        setRaster;
        ignore;
        ignore;
        setSpriteEnable;
        setControl2;
        setSpriteYExpansion;
        setMemoryPointers;
        ignore;
        setInterruptEnable;
        setSpritePriority;
        setSpriteMulticolorEnable;
        setSpriteXExpansion;
        ignore;
        ignore;

        setBorderColor;
        setBackgroundColor 0;
        setBackgroundColor 1;
        setBackgroundColor 2;
        setBackgroundColor 3;
        setSpriteMulticolor 0;
        setSpriteMulticolor 1;
        setSpriteColor 0;
        setSpriteColor 1;
        setSpriteColor 2;
        setSpriteColor 3;
        setSpriteColor 4;
        setSpriteColor 5;
        setSpriteColor 6;
        setSpriteColor 7;
        ignore;

        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
        ignore;
    |]

    let writeRegister = Array.init pokeRegister.Length <| fun i ->
        match i with
            | _ -> pokeRegister.[i]

    member this.Irq = irq
    member this.Ba = ba
    member this.Aec = aec

    member this.Bmm with get () = bmm and set (value) = bmm <- value
    member this.Ecm with get () = ecm and set (value) = ecm <- value
    member this.Mcm with get () = mcm and set (value) = mcm <- value

    member this.Peek address =
        peekRegister.[address &&& 0x3F] <| ()

    member this.Read address =
        readRegister.[address &&& 0x3F] <| ()

    member this.Poke address value =
        pokeRegister.[address &&& 0x3F] <| (value &&& 0xFF)

    member this.Write address value =
        writeRegister.[address &&& 0x3F] <| (value &&& 0xFF)

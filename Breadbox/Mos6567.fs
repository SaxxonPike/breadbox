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

    let halfCycles = config.CyclesPerLine * 2

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
    let vmatrix = Array.create 40 0x000
    let msr = Array.create 8 0x000000
    let mcn = Array.create 8 0x00
    let mpn = Array.create 8 0x00
    let mutable gbuffer = 0x00
    let mutable gsr = 0x00
    let mutable cbuffer = 0x000
    let mutable idle = true
    let mutable raster = config.YStart
    let mutable rasterX = config.XStart
    let mutable rasterI = 0x000
    let outBuffer = Array.create 8 0x0
    let mdma = Array.create 8 false
    let mutable badlineEnable = false
    let mutable refresh = 0x00
    let mutable halfCycle = 0
    let lastHalfCycle = halfCycles - 1
    let lastRaster = config.Lines - 1
    let topBorder = 0x037
    let bottomBorder = 0x033
    let leftBorder = 0x01F
    let rightBorder = 0x14E

    // Notes on hTiming
    //
    // - Sprite fetch 3 always happens on HBlank start
    //   - We can infer all other functions based on this
    // - Sprite fetch 0 all the way through the last G access is
    //   identical on all platforms, all that differs is the spacing
    //   - Sprite fetch 0 happens six cycles before HBlank
    //   - Sprite fetch 0 BA happens three cycles prior to that

    let noOp () =
        ()

    let colorBa () =
        not ((badlineEnable) &&
            (raster >= 0x30) &&
            (raster < 0xF8) &&
            (raster &&& 0x7 = yscroll))

    let noBa () =
        true

    let spriteBa1 index0 () =
        ba && (not mdma.[index0])

    let spriteBa2 index0 index1 () =
        ba && (not (mdma.[index0] || mdma.[index1]))

    let spriteBa3 index0 index1 index2 () =
        ba && (not (mdma.[index0] || mdma.[index1] || mdma.[index2]))

    let fetchS0 index () =
        let ptr = mpn.[index]
        msr.[index] <- (msr.[index] &&& 0x00FFFF) ||| (((ReadMemory (mcn.[index] ||| ptr)) &&& 0xFF) <<< 16)
        mpn.[index] <- (ptr + 1) &&& 0x3F

    let fetchS1 index () =
        let ptr = mpn.[index]
        msr.[index] <- (msr.[index] &&& 0xFF00FF) ||| (((ReadMemory (mcn.[index] ||| ptr)) &&& 0xFF) <<< 8)
        mpn.[index] <- (ptr + 1) &&& 0x3F

    let fetchS2 index () =
        let ptr = mpn.[index]
        msr.[index] <- (msr.[index] &&& 0xFFFF00) ||| ((ReadMemory (mcn.[index] ||| ptr)) &&& 0xFF)
        mpn.[index] <- (ptr + 1) &&& 0x3F

    let fetchP index () =
        mpn.[index] <- (ReadMemory (vm ||| 0x3F0 ||| index)) &&& 0xFF

    let fetchC index () =
        vmatrix.[index] <- ReadMemory (vm ||| vc)

    let fetchG index () =
        gbuffer <- 0xFF &&& (ReadMemory <|
            (if ecm then 0x39FF else 0x3FFF) &&&
                if idle then
                     0x3FFF
                else
                    rc |||
                        if mcm then
                            ((vmatrix.[index] &&& 0xFF) <<< 3) ||| cb
                        else
                            (vc <<< 3) ||| (cb &&& 0x2000))

    let fetchI () =
        ignore <| ReadMemory 0x3FFF

    let fetchR () =
        ignore <| ReadMemory (0x3F00 ||| refresh)
        refresh <- ((refresh - 1) &&& 0xFF)

    let decodeWidePixel () =
        mcm && (bmm || (cbuffer &&& 0x800 <> 0))

    let decodeBufferColor () =
        match ecm, bmm, mcm with
            | true, false, false ->
                // ECM
                if gbuffer &&& 1 = 0 then
                    bnc.[(cbuffer >>> 6) &&& 0x3]
                else
                    cbuffer >>> 8
            | false, false, b ->
                // (multicolor/) text
                if b && cbuffer &&& 0x800 <> 0 then
                    match gbuffer &&& 0x3 with
                        | 3 -> (cbuffer >>> 8) &&& 0x7
                        | c -> bnc.[c]
                else
                    if gbuffer &&& 1 = 0 then
                        bnc.[0]
                    else
                        cbuffer >>> 8
            | false, true, true ->
                // multicolor bitmap
                match gbuffer &&& 0x3 with
                    | 1 -> (cbuffer >>> 4) &&& 0xF
                    | 2 -> cbuffer &&& 0xF
                    | 3 -> (cbuffer >>> 8)
                    | _ -> bnc.[0]
            | false, true, false ->
                // bitmap
                if gbuffer &&& 1 = 0 then
                    cbuffer &&& 0xF
                else
                    (cbuffer >>> 4) &&& 0xF
            | _ ->
                // invalid
                0

    let resetRx () =
        rasterX <- 0

    let masterFetchTiming = [|
        fetchP  0;
        fetchS0 0;
        fetchS1 0;
        fetchS2 0;
        fetchP  1;
        fetchS0 1;
        fetchS1 1;
        fetchS2 1;
        fetchP  2;
        fetchS0 2;
        fetchS1 2;
        fetchS2 2;
        fetchP  3;
        fetchS0 3;
        fetchS1 3;
        fetchS2 3;
        fetchP  4;
        fetchS0 4;
        fetchS1 4;
        fetchS2 4;
        fetchP  5;
        fetchS0 5;
        fetchS1 5;
        fetchS2 5;
        fetchP  6;
        fetchS0 6;
        fetchS1 6;
        fetchS2 6;
        fetchP  7;
        fetchS0 7;
        fetchS1 7;
        fetchS2 7;
        fetchR   ;
        noOp     ;
        fetchR   ;
        noOp     ;
        fetchR   ;
        resetRx  ;
        fetchR   ;
        noOp     ;
        fetchR   ;
        fetchC  0;
        fetchG  0;
        fetchC  1;
        fetchG  1;
        fetchC  2;
        fetchG  2;
        fetchC  3;
        fetchG  3;
        fetchC  4;
        fetchG  4;
        fetchC  5;
        fetchG  5;
        fetchC  6;
        fetchG  6;
        fetchC  7;
        fetchG  7;
        fetchC  8;
        fetchG  8;
        fetchC  9;
        fetchG  9;
        fetchC 10;
        fetchG 10;
        fetchC 11;
        fetchG 11;
        fetchC 12;
        fetchG 12;
        fetchC 13;
        fetchG 13;
        fetchC 14;
        fetchG 14;
        fetchC 15;
        fetchG 15;
        fetchC 16;
        fetchG 16;
        fetchC 17;
        fetchG 17;
        fetchC 18;
        fetchG 18;
        fetchC 19;
        fetchG 19;
        fetchC 20;
        fetchG 20;
        fetchC 21;
        fetchG 21;
        fetchC 22;
        fetchG 22;
        fetchC 23;
        fetchG 23;
        fetchC 24;
        fetchG 24;
        fetchC 25;
        fetchG 25;
        fetchC 26;
        fetchG 26;
        fetchC 27;
        fetchG 27;
        fetchC 28;
        fetchG 28;
        fetchC 29;
        fetchG 29;
        fetchC 30;
        fetchG 30;
        fetchC 31;
        fetchG 31;
        fetchC 32;
        fetchG 32;
        fetchC 33;
        fetchG 33;
        fetchC 34;
        fetchG 34;
        fetchC 35;
        fetchG 35;
        fetchC 36;
        fetchG 36;
        fetchC 37;
        fetchG 37;
        fetchC 38;
        fetchG 38;
        fetchC 39;
        fetchG 39;
    |]

    let masterBaTiming = [|
        spriteBa1 0;
        spriteBa1 0;
        spriteBa1 0;
        spriteBa1 0;
        spriteBa2 0 1;
        spriteBa2 0 1;
        spriteBa2 0 1;
        spriteBa2 0 1;
        spriteBa3 0 1 2;
        spriteBa3 0 1 2;
        spriteBa2 1 2;
        spriteBa2 1 2;
        spriteBa3 1 2 3;
        spriteBa3 1 2 3;
        spriteBa2 2 3;
        spriteBa2 2 3;
        spriteBa3 2 3 4;
        spriteBa3 2 3 4;
        spriteBa2 3 4;
        spriteBa2 3 4;
        spriteBa3 3 4 5;
        spriteBa3 3 4 5;
        spriteBa2 4 5;
        spriteBa2 4 5;
        spriteBa3 4 5 6;
        spriteBa3 4 5 6;
        spriteBa2 5 6;
        spriteBa2 5 6;
        spriteBa3 5 6 7;
        spriteBa3 5 6 7;
        spriteBa2 6 7;
        spriteBa2 6 7;
        spriteBa2 6 7;
        spriteBa2 6 7;
        spriteBa1 7;
        spriteBa1 7;
        spriteBa1 7;
        spriteBa1 7;
        noBa;
        noBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
        colorBa;
    |]

    let hTiming =
        // fetch
        let fetchOp = Array.init halfCycles <| fun i ->
            if (i % 1) = 0 then
                fetchI
            else
                noOp
        for i = 0 to halfCycles - 1 do
            let timingIndex = (i + 12) % halfCycles
            if (timingIndex < masterFetchTiming.Length) then
                fetchOp.[i] <- masterFetchTiming.[timingIndex]
        // ba
        let baOp = Array.init (config.CyclesPerLine * 2) <| fun i ->
            noBa
        for i = 0 to halfCycles - 1 do
            let timingIndex = (i + 18) % halfCycles
            if (timingIndex < masterBaTiming.Length) then
                baOp.[i] <- masterBaTiming.[timingIndex]
        // composition
        let output = Array.init halfCycles <| fun i ->
            fetchOp.[i] >> baOp.[i]
        output

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

    let rec ClockInternal index =
        if (index &&& 0x3 = 0) then
            let thisHalfCycle = halfCycle
            ba <- hTiming.[thisHalfCycle]()
            halfCycle <-
                if thisHalfCycle = lastHalfCycle then
                    let thisRaster = raster
                    raster <-
                        if thisRaster = lastRaster then
                            0
                        else
                            thisRaster + 1
                    0
                else
                    halfCycle + 1

        if index < 8 then
            ClockInternal <| index + 1

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

    member this.Output = outBuffer
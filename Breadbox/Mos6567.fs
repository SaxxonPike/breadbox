namespace Breadbox

// 6567 core.

[<Sealed>]
type Mos6567Configuration(read:System.Func<int, int>, lines:int, cyclesPerLine:int, hBlankStart:int, hBlankEnd:int, vBlankStart:int, vBlankEnd:int) =
    member val Read = read.Invoke
    member val Lines = lines
    member val HBlankStart = hBlankStart
    member val HBlankEnd = hBlankEnd
    member val VBlankStart = vBlankStart
    member val VBlankEnd = vBlankEnd
    member val CyclesPerLine = cyclesPerLine

[<Sealed>]
type Mos6567(config:Mos6567Configuration) =
    let Read = config.Read

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
    let mutable bnc = Array.create 4 0x0
    let mutable mm0 = 0x0
    let mutable mm1 = 0x0
    let mnc = Array.create 8 0x0
    let vmatrix = Array.create 40 0x000
    let msr = Array.create 8 0x000000
    let mcn = Array.create 8 0x00
    let mpn = Array.create 8 0x00
    let mutable gbuffer = 0x00
    let mutable gsr = 0x00
    let mutable cbuffer = 0x000
    let mutable idle = true

    // Notes on hTiming
    //
    // - Sprite fetch 3 always happens on HBlank start
    //   - We can infer all other functions based on this
    // - Sprite fetch 0 all the way through the last G access is
    //   identical on all platforms, all that differs is the spacing
    //   - Sprite fetch 0 happens six cycles before HBlank
    //   - Sprite fetch 0 BA happens three cycles prior to that

    let fetchS0 index =
        let ptr = mpn.[index]
        msr.[index] <- (msr.[index] &&& 0x00FFFF) ||| (((Read (mcn.[index] ||| ptr)) &&& 0xFF) <<< 16)
        mpn.[index] <- (ptr + 1) &&& 0x3F

    let fetchS1 index =
        let ptr = mpn.[index]
        msr.[index] <- (msr.[index] &&& 0xFF00FF) ||| (((Read (mcn.[index] ||| ptr)) &&& 0xFF) <<< 8)
        mpn.[index] <- (ptr + 1) &&& 0x3F

    let fetchS2 index =
        let ptr = mpn.[index]
        msr.[index] <- (msr.[index] &&& 0xFFFF00) ||| ((Read (mcn.[index] ||| ptr)) &&& 0xFF)
        mpn.[index] <- (ptr + 1) &&& 0x3F

    let fetchP index =
        mpn.[index] <- (Read (vm ||| 0x3F0 ||| index)) &&& 0xFF

    let fetchC index =
        vmatrix.[index] <- Read (vm ||| vc)

    let fetchG index =
        gbuffer <- 0xFF &&& (Read <|
            (if ecm then 0x39FF else 0x3FFF) &&&
                if idle then
                     0x3FFF
                else
                    rc |||
                        if mcm then
                            ((vmatrix.[index] &&& 0xFF) <<< 3) ||| cb
                        else
                            (vc <<< 3) ||| (cb &&& 0x2000))

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

    let masterTiming = [|
        0
    |]

    let hTiming = Array.init <| config.CyclesPerLine * 2 <| fun i ->
        0

    member this.Irq = irq
    member this.Ba = ba
    member this.Aec = aec



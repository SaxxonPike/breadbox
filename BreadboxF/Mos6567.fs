namespace Breadbox

type private SpriteUop =
    | None
    | InvertYExpansionToggle
    | EnableDma
    | EnableDisplay
    | PerformCrunch

type private GraphicsUop =
    | None
    | LoadVcBase
    | IncrementRc

type private FetchUop =
    | None
    | Idle
    | SpritePointer
    | SpriteData
    | Graphics
    | Character
    | Refresh

type private BaUop =
    | None
    | Idle
    | Sprite0
    | Sprite01
    | Sprite012
    | Sprite12
    | Sprite123
    | Sprite23
    | Sprite234
    | Sprite34
    | Sprite345
    | Sprite45
    | Sprite456
    | Sprite56
    | Sprite567
    | Sprite67
    | Sprite7
    | Character

type Mos6567Configuration (cyclesPerRasterLine, rasterLinesPerFrame) =
    member val CyclesPerRasterLine = cyclesPerRasterLine
    member val RasterLinesPerFrame = rasterLinesPerFrame
    member val PixelsPerRasterLine = cyclesPerRasterLine * 8












type Mos6567Chip (config:Mos6567Configuration) =

    // Interface
    let ReadMemory address:int = 0
    
    // Timing Information
    let CyclesPerRasterLine = config.CyclesPerRasterLine
    let RasterLinesPerFrame = config.RasterLinesPerFrame
    let PixelsPerRasterLine = config.PixelsPerRasterLine

    // Determines phase transitions based on raster counter
    let IsFirstPhaseEdge x =
        (x &&& 0x7) = 4
    let IsSecondPhaseEdge x =
        (x &&& 0x7) = 0
    let IsPhaseEdge x =
        (x &&& 0x3) = 0

    // Determines raster counter for system-agnostic cycle (PAL 55 = old NTSC 56 = new NTSC 57)
    let RasterCounterForPalCycle cycle =
        match cycle with
            | c when c >= 1 && c <= 54 -> (((c - 1) <<< 3) + (PixelsPerRasterLine - 0x064)) % PixelsPerRasterLine
            | c when c >= 55 && c <= 63 -> (((c - 55) <<< 3) + (PixelsPerRasterLine - 0x0AC)) % PixelsPerRasterLine
            | _ -> raise (System.Exception("Cycle must be in the range 1-63."))

    // Determines raster counter for fetch index (0 = PAL 55, first BA)
    let RasterCounterForFetchIndex index =
        let indexPixel = (index % (CyclesPerRasterLine <<< 1)) <<< 2
        let start = RasterCounterForPalCycle 55
        (start + indexPixel) % PixelsPerRasterLine

    // Determines fetch index for raster counter
    let FetchIndexForRasterCounter counter =
        match counter with
            | c when not (IsPhaseEdge c) ->
                -1
            | c ->
                match (((PixelsPerRasterLine + c) - RasterCounterForPalCycle(55)) % PixelsPerRasterLine) >>> 2 with
                    | i when i >= 0 && i <= 126 -> i
                    | _ -> -1

    // Next value for raster counter
    let NextRasterCounter counter =
        match counter with
            | c when c = PixelsPerRasterLine - 1 -> 0x000
            | c -> c + 1

    // X coordinate for raster counter
    let RasterCounterX =
        let freeze = max 0 (PixelsPerRasterLine - 0x200)
        Array.init <|
            PixelsPerRasterLine <|
            fun x ->
                match x with
                    | x when x < 0x18C -> x
                    | x when x < 0x18C + freeze -> 0x18C
                    | _ -> x - freeze

    // Fetches for raster counter
    let RasterCounterFetch =
        let fetchOps = [|
            (if CyclesPerRasterLine < 64 then FetchUop.Graphics else FetchUop.Idle);
            FetchUop.Idle;
            FetchUop.Idle;
            FetchUop.Idle;
            FetchUop.Idle;
            FetchUop.Idle;
            FetchUop.SpritePointer; FetchUop.SpriteData; FetchUop.SpriteData; FetchUop.SpriteData;
            FetchUop.SpritePointer; FetchUop.SpriteData; FetchUop.SpriteData; FetchUop.SpriteData;
            FetchUop.SpritePointer; FetchUop.SpriteData; FetchUop.SpriteData; FetchUop.SpriteData;
            FetchUop.SpritePointer; FetchUop.SpriteData; FetchUop.SpriteData; FetchUop.SpriteData;
            FetchUop.SpritePointer; FetchUop.SpriteData; FetchUop.SpriteData; FetchUop.SpriteData;
            FetchUop.SpritePointer; FetchUop.SpriteData; FetchUop.SpriteData; FetchUop.SpriteData;
            FetchUop.SpritePointer; FetchUop.SpriteData; FetchUop.SpriteData; FetchUop.SpriteData;
            FetchUop.SpritePointer; FetchUop.SpriteData; FetchUop.SpriteData; FetchUop.SpriteData;
            FetchUop.Refresh; FetchUop.Idle; FetchUop.Refresh; FetchUop.Idle;
            FetchUop.Refresh; FetchUop.Idle; FetchUop.Refresh; FetchUop.Idle; FetchUop.Refresh;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
            FetchUop.Character; FetchUop.Graphics; FetchUop.Character; FetchUop.Graphics;
        |]
        Array.init <|
            PixelsPerRasterLine <|
            fun x ->
                match (FetchIndexForRasterCounter x), (IsPhaseEdge x) with
                    | _, false -> FetchUop.None
                    | -1, true -> FetchUop.Idle
                    | c, _ -> fetchOps.[c]

    // Sprite index for fetches above
    let RasterCounterFetchSprite =
        Array.init <|
            PixelsPerRasterLine <|
            fun x ->
                match (FetchIndexForRasterCounter x), (IsPhaseEdge x) with
                    | c, true when c >= 6 && c < 38 -> (c - 6) >>> 2
                    | _ -> -1

    // BA checks for raster counter
    let RasterCounterBa =
        let baOps = [|
            BaUop.Sprite0; BaUop.Sprite0; BaUop.Sprite01; BaUop.Sprite01;
            BaUop.Sprite012; BaUop.Sprite12; BaUop.Sprite123; BaUop.Sprite23;
            BaUop.Sprite234; BaUop.Sprite34; BaUop.Sprite345; BaUop.Sprite45;
            BaUop.Sprite456; BaUop.Sprite56; BaUop.Sprite567; BaUop.Sprite67;
            BaUop.Sprite67; BaUop.Sprite7; BaUop.Sprite7; BaUop.Idle;
            BaUop.Character; BaUop.Character; BaUop.Character;
            BaUop.Character; BaUop.Character; BaUop.Character; BaUop.Character;
            BaUop.Character; BaUop.Character; BaUop.Character; BaUop.Character;
            BaUop.Character; BaUop.Character; BaUop.Character; BaUop.Character;
            BaUop.Character; BaUop.Character; BaUop.Character; BaUop.Character;
            BaUop.Character; BaUop.Character; BaUop.Character; BaUop.Character;
            BaUop.Character; BaUop.Character; BaUop.Character; BaUop.Character;
            BaUop.Character; BaUop.Character; BaUop.Character; BaUop.Character;
            BaUop.Character; BaUop.Character; BaUop.Character; BaUop.Character;
            BaUop.Character; BaUop.Character; BaUop.Character; BaUop.Character;
            BaUop.Character; BaUop.Character; BaUop.Character; BaUop.Character
        |]
        Array.init <|
            PixelsPerRasterLine <|
            fun x ->
                match (FetchIndexForRasterCounter x), (IsSecondPhaseEdge x) with
                    | _, false -> BaUop.None
                    | -1, true -> BaUop.Idle
                    | c, _ -> baOps.[c >>> 1]

    // Sprite ops for raster counter
    let SpriteUops =
        Array.init <|
            PixelsPerRasterLine <|
            fun x ->
                match x with
                    | x when x = RasterCounterForPalCycle 16 -> SpriteUop.PerformCrunch
                    | x when x = RasterCounterForPalCycle 55 -> SpriteUop.InvertYExpansionToggle
                    | x when x = RasterCounterForPalCycle 56 -> SpriteUop.EnableDma
                    | x when x = RasterCounterForPalCycle 58 -> SpriteUop.EnableDisplay
                    | _ -> SpriteUop.None

    // Graphics ops for raster counter
    let GraphicsUops =
        Array.init <|
            PixelsPerRasterLine <|
            fun x ->
                match x with
                    | x when x = RasterCounterForPalCycle 14 -> GraphicsUop.LoadVcBase
                    | x when x = RasterCounterForPalCycle 58 -> GraphicsUop.IncrementRc
                    | _ -> GraphicsUop.None

    // In cycle 14, Mc <- f(Mc)
    let SpriteMcAdd = [|
        0x03; 0x04; 0x05; 0x06;
        0x07; 0x08; 0x09; 0x0A;
        0x0B; 0x0C; 0x0D; 0x0E;
        0x0F; 0x10; 0x11; 0x12;
        0x13; 0x14; 0x15; 0x16;
        0x17; 0x18; 0x19; 0x1A;
        0x1B; 0x1C; 0x1D; 0x1E;
        0x1F; 0x20; 0x21; 0x22;
        0x23; 0x24; 0x25; 0x26;
        0x27; 0x28; 0x29; 0x2A;
        0x2B; 0x2C; 0x2D; 0x2E;
        0x2F; 0x30; 0x31; 0x32;
        0x33; 0x34; 0x35; 0x36;
        0x37; 0x38; 0x39; 0x3A;
        0x3B; 0x3C; 0x3D; 0x3E;
        0x3F; 0x00; 0x01; 0x3F;
    |]

    // If Y-flag and Y-expand are both off in cycle 16, McBase <- f(McBase)
    let SpriteMcBaseCrunch = [|
        0x01; 0x05; 0x05; 0x07;
        0x05; 0x05; 0x05; 0x07;
        0x09; 0x0D; 0x0D; 0x0F;
        0x0D; 0x15; 0x15; 0x17;
        0x11; 0x15; 0x15; 0x17;
        0x15; 0x15; 0x15; 0x17;
        0x19; 0x1D; 0x1D; 0x1F;
        0x1D; 0x15; 0x15; 0x17;
        0x21; 0x25; 0x25; 0x27;
        0x25; 0x25; 0x25; 0x27;
        0x29; 0x2D; 0x2D; 0x2F;
        0x2D; 0x35; 0x35; 0x37;
        0x31; 0x35; 0x35; 0x37;
        0x35; 0x35; 0x35; 0x37;
        0x39; 0x3D; 0x3D; 0x3F;
        0x3D; 0x15; 0x15; 0x3F;
    |]









    // Determine I fetch address.
    let FetchIAddress =
        0x3FFF

    // Determine P fetch address. VM must be pre-shifted.
    let FetchPAddress vm index =
        vm ||| 0x03F8 ||| index

    // Determine S fetch address. MP must be pre-shifted.
    let FetchSAddress mp mc =
        mc ||| mp

    // Determine R fetch address. REF must be pre-truncated.
    let FetchRAddress ref =
        0x3F00 ||| ref

    // Determine G fetch address. CB must be pre-shifted.
    let FetchGAddress idle ecm bmm vc cb c rc =
        (if idle then 0x3FFF elif bmm then ((cb &&& 0x2000) ||| (vc <<< 3) ||| rc) else (cb ||| ((c &&& 0xFF) <<< 3) ||| rc)) &&& (if ecm then 0x39FF else 0x3FFF)

    // Determine C fetch address. VM must be pre-shifted.
    let FetchCAddress vm vc =
        (vm ||| vc)
    
    // Determine fetch operation. (operation:int, index:int)
    let Fetch cycle =
        match RasterCounterFetch.[cycle] with
            | FetchUop.None           -> FetchUop.None, 0
            | FetchUop.Idle           -> FetchUop.Idle, 0
            | FetchUop.Graphics       -> FetchUop.Graphics, 0
            | FetchUop.Character      -> FetchUop.Character, 0
            | FetchUop.SpriteData     -> FetchUop.SpriteData, RasterCounterFetchSprite.[cycle]
            | FetchUop.SpritePointer  -> FetchUop.SpritePointer, RasterCounterFetchSprite.[cycle]
            | FetchUop.Refresh        -> FetchUop.Refresh, 0
    
    // Determine graphics output color and data [000] (color:int, foreground:bool)
    let RawGraphicsOutputStandardTextMode b0c color sr =
        match (sr &&& 0x80) with
            | 0x80 -> (color >>> 8, true)
            | _    -> (b0c, false)

    // Determine graphics output color and data [001] (color:int, foreground:bool)
    let RawGraphicsOutputMulticolorTextMode b0c b1c b2c color sr =
        match (sr &&& 0xC0), (color &&& 0x800) <> 0 with
            | 0x40         , true                     -> (b1c, false)
            | 0x80         , true                     -> (b2c, true)
            | 0x80         , _
            | 0xC0         , _                        -> ((color >>> 8) &&& 0x7, true)
            | _                                       -> (b0c, false)

    // Determine graphics output color and data [010] (color:int, foreground:bool)
    let RawGraphicsOutputStandardBitmapMode color sr =
        match (sr &&& 0x80) with
            | 0x80            -> ((color >>> 4) &&& 0xF, true)
            | _               -> (color &&& 0xF, false)

    // Determine graphics output color and data [011] (color:int, foreground:bool)
    let RawGraphicsOutputMulticolorBitmapMode b0c color sr =
        match (sr &&& 0xC0) with
            | 0x40            -> ((color >>> 4) &&& 0xF, false)
            | 0x80            -> (color &&& 0xF, true)
            | 0xC0            -> ((color >>> 8), true)
            | _               -> (b0c, false)

    // Determine graphics output color and data [100] (color:int, foreground:bool)
    let RawGraphicsOutputExtraColorMode b0c b1c b2c b3c color sr =
        match (sr &&& 0x80), (color &&& 0xC0) with
            | 0x80         , _                  -> ((color >>> 8), true)
            | _            , 0x40               -> (b1c, false)
            | _            , 0x80               -> (b2c, false)
            | _            , 0xC0               -> (b3c, false)
            | _                                 -> (b0c, false)

    // Determine graphics output color and data [101] (color:int, foreground:bool)
    let RawGraphicsOutputInvalidExtraColorMode sr =
        match (sr &&& 0x80) with
            | 0x80            -> (0, true)
            | _               -> (0, false)

    // Determine graphics output color and data (color:int, foreground:bool)
    let RawGraphicsOutput ecm bmm mcm b0c b1c b2c b3c color sr =
        match ecm,   bmm,   mcm   with
            | false, false, false   -> RawGraphicsOutputStandardTextMode b0c color sr
            | false, false, true    -> RawGraphicsOutputMulticolorTextMode b0c b1c b2c color sr
            | false, true,  false   -> RawGraphicsOutputStandardBitmapMode color sr
            | false, true,  true    -> RawGraphicsOutputMulticolorBitmapMode b0c color sr
            | true,  false, false   -> RawGraphicsOutputExtraColorMode b0c b1c b2c b3c color sr
            | _                     -> RawGraphicsOutputInvalidExtraColorMode sr

    // Determine sprite output color and data (color:int, output:bool, priority:bool)
    let RawSpriteOutput mmc0 mmc1 color multicolor sr dp disp =
        match disp with
            | false -> 0, false, dp
            | _ ->
                match sr &&& 0x800000, multicolor, sr &&& 0xC00000 with
                    | 0x800000       , false     , _
                    | _              , true      , 0x800000          -> color, true, dp
                    | _              , true      , 0x400000          -> mmc0, true, dp
                    | _              , true      , 0xC00000          -> mmc1, true, dp
                    | _                                              -> 0, false, dp

    // Determine border output color and data (color:int, output:bool)
    let RawBorderOutput ec mborder vborder =
        match mborder, vborder with
            | true   , _
            | _      , true      -> ec, true
            | _                  -> 0, false

    // Determine which sprites are outputting (register:int)
    let RawMuxSprites s0 s1 s2 s3 s4 s5 s6 s7 =
        (match s0 with | (_, true, _) -> 0x01 | _ -> 0x00) |||
        (match s1 with | (_, true, _) -> 0x02 | _ -> 0x00) |||
        (match s2 with | (_, true, _) -> 0x04 | _ -> 0x00) |||
        (match s3 with | (_, true, _) -> 0x08 | _ -> 0x00) |||
        (match s4 with | (_, true, _) -> 0x10 | _ -> 0x00) |||
        (match s5 with | (_, true, _) -> 0x20 | _ -> 0x00) |||
        (match s6 with | (_, true, _) -> 0x40 | _ -> 0x00) |||
        (match s7 with | (_, true, _) -> 0x80 | _ -> 0x00)

    // Determine shifted graphics state (mcToggle:bool, sr:int)
    let ClockedGraphics bmm mcm mct c sr =
        match bmm, mcm, mct, (c &&& 0x800 <> 0) with
            | false, true, _, false
            | _, false, _, _ -> true, sr <<< 1
            | _, _, true, _ -> false, sr
            | _, _, false, _ -> true, sr <<< 2

    // Determine shifted sprite state (srEnabled:bool, mcToggle:bool, xeToggle:bool, sr:int)
    let ClockedSprite rasterx x sre disp sr mc mct xe xet =
        match disp, sre || (rasterx = x), mc, xe, mct, xet with
            | false, _    , _    , _    , _    , _
            | _    , false, _    , _    , _    , _     -> false, true, true, sr
            | _    , _    , false, false, _    , _
            | _    , _    , false, true , _    , false -> true, true, true, sr <<< 1
            | _    , _    , true , false, false, _
            | _    , _    , true , true , false, false -> true, true, true, sr <<< 2
            | _    , _    , false, true , _    , true
            | _    , _    , true , true , false, true  -> true, true, false, sr
            | _    , _    , true , false, true , _
            | _    , _    , true , true , true , true  -> true, false, true, sr
            | _    , _    , true , true , true , false -> true, false, false, sr

    // Determine clocked raster position. (counterX:int, rasterY:int, rasterX:int)
    let ClockedRaster rasterCounter rasterY =
        match (rasterCounter + 1) with
            | newCounter when newCounter >= PixelsPerRasterLine ->
                match (rasterY + 1) with
                    | newRasterY when newRasterY >= RasterLinesPerFrame -> 0, 0, 0
                    | newRasterY -> 0, newRasterY, 0
            | newCounter -> newCounter, rasterY, RasterCounterX.[newCounter]

    // Determine frontmost sprite to render (color:int, output:bool, priority:bool)
    let MuxSpritesForeground s0 s1 s2 s3 s4 s5 s6 s7 =
        match s0, s1, s2, s3, s4, s5, s6, s7 with
            | (_, true, _), _, _, _, _, _, _, _ -> s0
            | _, (_, true, _), _, _, _, _, _, _ -> s1
            | _, _, (_, true, _), _, _, _, _, _ -> s2
            | _, _, _, (_, true, _), _, _, _, _ -> s3
            | _, _, _, _, (_, true, _), _, _, _ -> s4
            | _, _, _, _, _, (_, true, _), _, _ -> s5
            | _, _, _, _, _, _, (_, true, _), _ -> s6
            | _, _, _, _, _, _, _, (_, true, _) -> s7
            | _                                 -> (0, false, true)

    // Determine sprite-sprite collision register result (register:int)
    let MuxSpriteSpriteCollision rawmux =
        match rawmux with
            | 0x00 | 0x01 | 0x02 | 0x04 | 0x08 | 0x10 | 0x20 | 0x40 | 0x80 -> 0x00
            | _ -> rawmux

    // Determine sprite-data collision register result (register:int)
    let MuxSpriteBackgroundCollision g rawmux =
        match g with
            | (_, false) -> 0x00
            | _ -> rawmux
    
    // Determine output sprite color, data, priority and collision (color:int, output:bool, priority:bool, spriteCollisions:int, dataCollisions:int)
    let MuxSprites graphicsOutput s0 s1 s2 s3 s4 s5 s6 s7 =
        match MuxSpritesForeground s0 s1 s2 s3 s4 s5 s6 s7, RawMuxSprites s0 s1 s2 s3 s4 s5 s6 s7 with
            | (color, output, priority), rawmux ->
                (color, output, priority, MuxSpriteSpriteCollision rawmux, MuxSpriteBackgroundCollision graphicsOutput rawmux) 

    // Determine graphics unit output (color:int, spriteCollisions:int, dataCollisions:int)
    let Mux s0 s1 s2 s3 s4 s5 s6 s7 ec vborder ecm bmm mcm b0c b1c b2c b3c gc gsr =
        match vborder with
            | true -> ec, 0x00, 0x00
            | _ ->
                match RawGraphicsOutput ecm bmm mcm b0c b1c b2c b3c gc gsr with
                    | (graphicsColor, graphicsForeground) ->
                        match MuxSprites (graphicsColor, graphicsForeground) s0 s1 s2 s3 s4 s5 s6 s7 with
                            | (spriteColor, spriteData, spritePriority, spriteSpriteCollisions, spriteDataCollisions) ->
                                match graphicsForeground, spriteData, spritePriority with
                                    | _, false, _ | true, _, true -> (graphicsColor, spriteSpriteCollisions, spriteDataCollisions)
                                    | _ -> (spriteColor, spriteSpriteCollisions, spriteDataCollisions)

    // Determine video output (color:int, spriteCollisions:int, dataCollisions:int)
    let Output s0 s1 s2 s3 s4 s5 s6 s7 ec vborder mborder ecm bmm mcm b0c b1c b2c b3c gc gsr =
        match mborder, Mux s0 s1 s2 s3 s4 s5 s6 s7 ec vborder ecm bmm mcm b0c b1c b2c b3c gc gsr with
            | true, (_, spriteSpriteCollisions, spriteDataCollisions) -> (ec, spriteSpriteCollisions, spriteDataCollisions)
            | _, (color, spriteSpriteCollisions, spriteDataCollisions) -> (color, spriteSpriteCollisions, spriteDataCollisions)


    member this.TestFetchIAddress () = FetchIAddress
    member this.TestFetchRAddress ref = FetchRAddress ref
    member this.TestFetchGAddress ecm bmm vc cb c rc = FetchGAddress ecm bmm vc cb c rc
    member this.TestFetchCAddress vm vc = FetchCAddress vm vc
    member this.TestFetchPAddress vm index = FetchPAddress vm index
    member this.TestFetchSAddress mp mc = FetchSAddress mp mc

    member this.TestRawGraphicsOutputStandardTextMode b0c color sr = RawGraphicsOutputStandardTextMode b0c color sr
    member this.TestRawGraphicsOutputMulticolorTextMode b0c b1c b2c color sr = RawGraphicsOutputMulticolorTextMode b0c b1c b2c color sr
    member this.TestRawGraphicsOutputStandardBitmapMode color sr = RawGraphicsOutputStandardBitmapMode color sr
    member this.TestRawGraphicsOutputMulticolorBitmapMode b0c color sr = RawGraphicsOutputMulticolorBitmapMode b0c color sr
    member this.TestRawGraphicsOutputExtraColorMode b0c b1c b2c b3c color sr = RawGraphicsOutputExtraColorMode b0c b1c b2c b3c color sr
    member this.TestRawGraphicsOutputInvalidExtraColorMode sr = RawGraphicsOutputInvalidExtraColorMode sr

    member this.TestRawSpriteOutput mmc0 mmc1 color multicolor sr dp disp = RawSpriteOutput mmc0 mmc1 color multicolor sr dp disp
    member this.TestRawBorderOutput ec mborder vborder = RawBorderOutput ec mborder vborder
    member this.TestRawMuxSprites s0 s1 s2 s3 s4 s5 s6 s7 = RawMuxSprites s0 s1 s2 s3 s4 s5 s6 s7
    member this.TestClockedGraphics bmm mcm mct c sr = ClockedGraphics bmm mcm mct c sr
    member this.TestClockedSprite rasterx x sre disp sr mc mct xe xet = ClockedSprite rasterx x sre disp sr mc mct xe xet
    member this.TestMuxSpritesForeground s0 s1 s2 s3 s4 s5 s6 s7 = MuxSpritesForeground s0 s1 s2 s3 s4 s5 s6 s7
    member this.TestMuxSpriteSpriteCollision rawmux = MuxSpriteSpriteCollision rawmux
    member this.TestMuxSpriteBackgroundCollision g rawmux = MuxSpriteBackgroundCollision g rawmux
    member this.TestMuxSprites graphicsOutput s0 s1 s2 s3 s4 s5 s6 s7 = MuxSprites graphicsOutput s0 s1 s2 s3 s4 s5 s6 s7
    member this.TestMux s0 s1 s2 s3 s4 s5 s6 s7 ec vborder ecm bmm mcm b0c b1c b2c b3c gc gsr = Mux s0 s1 s2 s3 s4 s5 s6 s7 ec vborder ecm bmm mcm b0c b1c b2c b3c gc gsr
    member this.TestOutput s0 s1 s2 s3 s4 s5 s6 s7 ec vborder mborder ecm bmm mcm b0c b1c b2c b3c gc gsr = Output s0 s1 s2 s3 s4 s5 s6 s7 ec vborder mborder ecm bmm mcm b0c b1c b2c b3c gc gsr
    member this.TestClockedRaster rasterCounter rasterY = ClockedRaster rasterCounter rasterY
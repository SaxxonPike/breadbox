namespace Breadbox

type Mos6502(memory:IMemory) =
    let mutable pc = 0
    let mutable a = 0
    let mutable x = 0
    let mutable y = 0
    let mutable sp = 0
    let mutable p = 0x20

    let setSign value =
        p <- if value then p ||| 0x80 else p &&& 0x7F

    let setOverflow value =
        p <- if value then p ||| 0x40 else p &&& 0xBF

    let setBreak value =
        p <- if value then p ||| 0x10 else p &&& 0xEF

    let setDecimal value =
        p <- if value then p ||| 0x08 else p &&& 0xF7

    let setInterrupt value =
        p <- if value then p ||| 0x04 else p &&& 0xFB

    let setZero value =
        p <- if value = 0 then p ||| 0x02 else p &&& 0xFD

    let setCarry value =
        p <- if value then p ||| 0x01 else p &&& 0xFE

    let setStatus value =
        p <- value
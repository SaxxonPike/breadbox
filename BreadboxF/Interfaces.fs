namespace Breadbox

type IMemory =
    abstract member Read: int -> int
    abstract member Write: int * int -> unit

type IClock =
    abstract member Clock: unit -> unit

type MemoryNull () =
    interface IMemory with
        member this.Read (address) = 0
        member this.Write (address, value) = ()

type ClockNull () =
    interface IClock with
        member this.Clock () = ()

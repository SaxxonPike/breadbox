namespace Breadbox

type IMemory =
    abstract member Read: int -> int
    abstract member Write: int * int -> unit

type IClock =
    abstract member Clock: unit -> unit

type IReadySignal =
    abstract member Rdy: unit -> bool

type MemoryNull () =
    interface IMemory with
        member this.Read (address) = 0
        member this.Write (address, value) = ()

type ClockNull () =
    interface IClock with
        member this.Clock () = ()

type ReadySignalNull () =
    interface IReadySignal with
        member this.Rdy () = true
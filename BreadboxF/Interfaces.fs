namespace Breadbox

type IMemory =
    abstract member Read: int -> int
    abstract member Write: int * int -> unit
    abstract member Peek: int -> int
    abstract member Poke: int * int -> unit

type IClock =
    abstract member Clock: unit -> unit

type IReadySignal =
    abstract member Rdy: unit -> bool

type MemoryNull () =
    interface IMemory with
        member this.Read (address) = 0
        member this.Write (address, value) = ()
        member this.Peek (address) = 0
        member this.Poke (address, value) = ()

type ClockNull () =
    interface IClock with
        member this.Clock () = ()

type ReadySignalNull () =
    interface IReadySignal with
        member this.Rdy () = true

type MemoryTrace (memory:IMemory, onRead:System.Action<int>, onWrite:System.Action<int,int>) =
    interface IMemory with
        member this.Read (address) =
            onRead.Invoke(address)
            memory.Read address
        member this.Write (address, value) =
            onWrite.Invoke(address, value)
            memory.Write(address, value)
        member this.Peek (address) =
            memory.Peek address
        member this.Poke (address, value) =
            memory.Poke(address, value)

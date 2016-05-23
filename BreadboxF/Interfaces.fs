namespace BreadboxF

type MemoryInterface =
    abstract member Read: int -> int
    abstract member Write: int * int -> unit

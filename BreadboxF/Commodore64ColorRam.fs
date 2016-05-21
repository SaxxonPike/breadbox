namespace BreadboxF

type Commodore64ColorRam() =
    let ram = Array.create 1024 0

    member this.Read(address:int) =
        ram.[address &&& 0x3FF]
    member this.Write(address:int, value:int) =
        ram.[address &&& 0x3FF] <- (value &&& 0xF)
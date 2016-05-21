namespace BreadboxF

type Commodore64SystemRam() =
    let ram = Array.create 65536 0

    member this.Read(address:int) =
        ram.[address &&& 0xFFFF]
    member this.Write(address:int, value:int) =
        ram.[address &&& 0xFFFF] <- (value &&& 0xFF)
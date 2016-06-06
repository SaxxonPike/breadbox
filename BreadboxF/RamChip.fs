﻿namespace Breadbox

type RamChip (sizeBits, dataBits) =
    let capacity = 1 <<< sizeBits
    let addressMask = capacity - 1
    let dataMask = (1 <<< dataBits) - 1
    let data = Array.zeroCreate capacity

    let read address = data.[address &&& addressMask]
    let write address value = data.[address &&& addressMask] <- value &&& dataMask

    interface IMemory with
        member this.Read(address) = read address
        member this.Write(address, value) = write address value
        member this.Peek(address) = read address
        member this.Poke(address, value) = write address value

    member this.Patch (binary:int[], startAddress:int) =
        let mutable address = startAddress
        for value in binary do
            write address value
            address <- address + 1


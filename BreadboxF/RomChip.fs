namespace Breadbox

type RomChip (sizeBits, dataBits) =
    let capacity = 1 <<< sizeBits
    let addressMask = capacity - 1
    let dataMask = (1 <<< dataBits) - 1
    let data = Array.zeroCreate capacity

    interface IMemory with
        member this.Read(address) = data.[address &&& addressMask]
        member this.Write(address, value) = ()
        member this.Peek(address) = data.[address &&& addressMask]
        member this.Poke(address, value) = data.[address &&& addressMask] <- value &&& dataMask

    member this.Flash (binary:int[]) =
        if binary.Length <> data.Length then
            raise (System.Exception("Binary length must match rom capacity exactly."))
        else
            binary.CopyTo(data, 0)


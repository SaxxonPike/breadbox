namespace BreadboxF

type Memory(addressBits:int, dataBits:int) =
    let memory = Array.zeroCreate (1 <<< addressBits)
    let addressMask = (1 <<< addressBits) - 1
    let dataMask = (1 <<< dataBits) - 1
    member this.Read (address:int) =
        memory.[address &&& addressMask]
    member this.ReadUnchecked (address:int) =
        memory.[address]
    member this.Write (address:int, value:int) =
        memory.[address &&& addressMask] <- value &&& dataMask
    member this.WriteUnchecked (address:int, value:int) =
        memory.[address] <- value
    member this.Initialize (startAddress:int, data:int array) =
        let mutable sourceOffset = 0
        let mutable targetOffset = startAddress
        let mutable remainingData = Array.length(data)
        while (remainingData > 0) do
            this.Write(targetOffset, data.[sourceOffset])
            sourceOffset <- sourceOffset + 1
            targetOffset <- targetOffset + 1
            remainingData <- remainingData - 1
                
type MemoryAdapter(readFunction:System.Func<int, int>, writeFunction:System.Action<int, int>) =
    member this.Read (address:int) =
        readFunction.Invoke(address)
    member this.Write (address:int, value:int) =
        writeFunction.Invoke(address, value)

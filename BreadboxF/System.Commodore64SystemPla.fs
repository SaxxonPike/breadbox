namespace Breadbox.System
open Breadbox

// Data source:
// http://www.zimmers.net/anonftp/pub/cbm/firmware/computers/c64/C64_PLA_Dissected.pdf

type Commodore64SystemPlaConfiguration(loRamInput:ILoRamSignal, hiRamInput:IHiRamSignal, charenInput:ICharenSignal, gameInput:IGameSignal, exromInput:IExRomSignal, ram:IMemory, color:IMemory, basic:IMemory, kernal:IMemory, char:IMemory, roml:IMemory, romh:IMemory, io:IMemory, vicBank:IVicBank) =
    member val LoRam = loRamInput
    member val HiRam = hiRamInput
    member val Charen = charenInput
    member val Game = gameInput
    member val ExRom = exromInput
    member val Ram = ram
    member val Kernal = kernal
    member val Basic = basic
    member val Char = char
    member val RomL = roml
    member val RomH = romh
    member val Io = io
    member val Color = color
    member val VicBank = vicBank

type Commodore64SystemPlaVoidMemory (getLastData:unit->int) =
    interface IMemory with
        member this.Read (address) = getLastData()
        member this.Write (address, value) = ()
        member this.Peek (address) = getLastData()
        member this.Poke (address, value) = ()

type Commodore64SystemPla (config:Commodore64SystemPlaConfiguration) =
    let mutable lastAddress = 0xFFFF
    let mutable lastData = 0xFF

    let readLoRam = config.LoRam.ReadLoRam
    let readHiRam = config.HiRam.ReadHiRam
    let readGame = config.Game.ReadGame
    let readExRom = config.ExRom.ReadExRom
    let readCharen = config.Charen.ReadCharen
    let readVicBank = config.VicBank.ReadVicBank

    let ram = config.Ram
    let kernal = config.Kernal
    let basic = config.Basic
    let char = config.Char
    let romL = config.RomL
    let romH = config.RomH
    let io = config.Io
    let color = config.Color
    let none = new Commodore64SystemPlaVoidMemory(fun _ -> lastData) :> IMemory

    let vicReadTarget address =
        match address &&& 0x3000, readGame(), readExRom() with
            | 0x3000, false, true -> romH
            | 0x1000, true, _ -> char
            | 0x1000, _, false -> char
            | _ -> ram

    let getMode () =
        match readLoRam(), readHiRam(), readGame(), readExRom() with
            | false, false, false, false -> 0xb0000
            | false, false, false, true -> 0xb0001
            | false, false, true, false -> 0xb0010
            | false, false, true, true -> 0xb0011
            | false, true, false, false -> 0xb0100
            | false, true, false, true -> 0xb0101
            | false, true, true, false -> 0xb0110
            | false, true, true, true -> 0xb0111
            | true, false, false, false -> 0xb1000
            | true, false, false, true -> 0xb1001
            | true, false, true, false -> 0xb1010
            | true, false, true, true -> 0xb1011
            | true, true, false, false -> 0xb1100
            | true, true, false, true -> 0xb1101
            | true, true, true, false -> 0xb1110
            | _ -> 0xb1111

    let readTarget address =
        match getMode() with
            | 0b1111 ->
                match address >>> 12 with
                    | 0xA | 0xB -> basic
                    | 0xD -> if readCharen() then io else char
                    | 0xE | 0xF -> kernal
                    | _ -> ram
            | 0b0110 | 0b0111 ->
                match address >>> 12 with
                    | 0xD -> if readCharen() then io else char
                    | 0xE | 0xF -> kernal
                    | _ -> ram
            | 0b1000 ->
                if (address >>> 12 = 0xD && readCharen()) then io else ram
            | 0b1010 | 0b1011 ->
                if (address >>> 12 = 0xD && readCharen()) then io else char
            | 0b0000 | 0b0010 | 0b0011 ->
                ram
            | 0b1100 ->
                match address >>> 12 with
                    | 0x8 | 0x9 -> romL
                    | 0xA | 0xB -> romH
                    | 0xD -> if readCharen() then io else char
                    | 0xE | 0xF -> kernal
                    | _ -> ram
            | 0b0100 ->
                match address >>> 12 with
                    | 0xA | 0xB -> romH
                    | 0xD -> if readCharen() then io else char
                    | 0xE | 0xF -> kernal
                    | _ -> ram
            | 0b1110 ->
                match address >>> 12 with
                    | 0x8 | 0x9 -> romL
                    | 0xA | 0xB -> basic
                    | 0xD -> if readCharen() then io else char
                    | 0xE | 0xF -> kernal
                    | _ -> ram
            | _ ->
                match address >>> 12 with
                    | 0x0 -> ram
                    | 0x8 | 0x9 -> romL
                    | 0xD -> io
                    | 0xE | 0xF -> romH
                    | _ -> none
                    
    let writeTarget address =
        match getMode() with
            | 0b0000 | 0b0010 | 0b0011 ->
                ram
            | 0b0001 | 0b0101 | 0b1001 | 0b1101 ->
                match address >>> 12 with
                    | 0x0 -> ram
                    | 0x8 | 0x9 -> romL
                    | 0xD -> io
                    | 0xE | 0xF -> romH
                    | _ -> none
            | _ ->
                if (address >>> 12 = 0xD && readCharen()) then io else ram
    
    let vicRead (address) = vicReadTarget(address).Read(address)
    let vicPeek (address) = vicReadTarget(address).Peek(address)

    let read (address) = readTarget(address).Read(address)
    let write (address, value) = writeTarget(address).Write(address, value)
    let peek (address) = readTarget(address).Peek(address)
    let poke (address, value) = writeTarget(address).Write(address, value)

    member this.VicBus = new MemoryMap(vicRead, ignore, vicPeek, ignore) :> IMemory
    member this.SystemBus = new MemoryMap(read, write, peek, poke) :> IMemory

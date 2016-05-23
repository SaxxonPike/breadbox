namespace BreadboxF

type CommodoreSidFilterInterface =
    abstract member Sample: int -> int
    abstract member SetFrequency: int -> unit
    abstract member SetResonance: int -> unit

type CommodoreSidWaveTableInterface =
    abstract member Waveforms: int[][]

type CommodoreSidFilterNull () =
    interface CommodoreSidFilterInterface with
        member this.Sample (sample) = sample
        member this.SetFrequency (frequency) = ()
        member this.SetResonance (resonance) = ()

type CommodoreSidWaveTableNull () =
    interface CommodoreSidWaveTableInterface with
        member this.Waveforms = Array.init 8 (fun _ -> Array.zeroCreate 4096)

type CommodoreSidConfiguration(waveTable:CommodoreSidWaveTableInterface, filter:CommodoreSidFilterInterface) =
    member val Filter = filter
    member val WaveTable = waveTable

type CommodoreSidConfigurationFactory() =
    member this.CreateOldSidConfiguration() = new CommodoreSidConfiguration(new CommodoreSidWaveTableNull(), new CommodoreSidFilterNull())
    member this.CreateNewSidConfiguration() = new CommodoreSidConfiguration(new CommodoreSidWaveTableNull(), new CommodoreSidFilterNull())

type CommodoreSidAudioOutput =
    struct
        val Sample : int
        new (sample) = {
            Sample = sample
        }
    end

type CommodoreSidAudioInterface =
    abstract member Output: CommodoreSidAudioOutput -> unit

type CommodoreSidChip(config:CommodoreSidConfiguration, audio:CommodoreSidAudioInterface) =
    

    // ========================================================================
    // Interface
    // ========================================================================


    member this.Clock() =
        audio.Output(new CommodoreSidAudioOutput(0))

    member this.ClockMultiple (count) =
        let mutable i = count
        while i > 0 do
            this.Clock()
            i <- i - 1

namespace Breadbox

type private Uop =
    | Unsupported
    | Fetch1
    | Fetch1Real
    | Fetch2
    | Fetch3
    | FetchDummy
    | Nop
    | Jsr
    | IncPc
    | AbsWriteSta
    | AbsWriteStx
    | AbsWriteSty
    | AbsWriteSax
    | AbsReadBit
    | AbsReadLda
    | AbsReadLdy
    | AbsReadOra
    | AbsReadLdx
    | AbsReadCmp
    | AbsReadAdc
    | AbsReadCpx
    | AbsReadSbc
    | AbsReadAnd
    | AbsReadEor
    | AbsReadCpy
    | AbsReadNop
    | AbsReadLax
    | AbsRmwStage4
    | AbsRmwStage6
    | AbsRmwStage5Inc
    | AbsRmwStage5Dec
    | AbsRmwStage5Lsr
    | AbsRmwStage5Rol
    | AbsRmwStage5Asl
    | AbsRmwStage5Ror
    | AbsRmwStage5Slo
    | AbsRmwStage5Rla
    | AbsRmwStage5Sre
    | AbsRmwStage5Rra
    | AbsRmwStage5Dcp
    | AbsRmwStage5Isc
    | JmpAbs
    | ZpIdxStage3X
    | ZpIdxStage3Y
    | ZpIdxRmwStage4
    | ZpIdxRmwStage6
    | ZpWriteSta
    | ZpWriteStx
    | ZpWriteSty
    | ZpWriteSax
    | ZpRmwStage3
    | ZpRmwStage5
    | ZpRmwDec
    | ZpRmwInc
    | ZpRmwAsl
    | ZpRmwLsr
    | ZpRmwRor
    | ZpRmwRol
    | ZpRmwSlo
    | ZpRmwRla
    | ZpRmwSre
    | ZpRmwRra
    | ZpRmwDcp
    | ZpRmwIsc
    | ZpReadEor
    | ZpReadBit
    | ZpReadOra
    | ZpReadLda
    | ZpReadLdy
    | ZpReadLdx
    | ZpReadCpx
    | ZpReadSbc
    | ZpReadCpy
    | ZpReadNop
    | ZpReadAdc
    | ZpReadAnd
    | ZpReadCmp
    | ZpReadLax
    | IdxIndStage3
    | IdxIndStage4
    | IdxIndStage5
    | IdxIndReadStage6Ora
    | IdxIndReadStage6Sbc
    | IdxIndReadStage6Lda
    | IdxIndReadStage6Eor
    | IdxIndReadStage6Cmp
    | IdxIndReadStage6Adc
    | IdxIndReadStage6And
    | IdxIndReadStage6Lax
    | IdxIndWriteStage6Sta
    | IdxIndWriteStage6Sax
    | IdxIndRmwStage6
    | IdxIndRmwStage7Slo
    | IdxIndRmwStage7Rla
    | IdxIndRmwStage7Sre
    | IdxIndRmwStage7Rra
    | IdxIndRmwStage7Isc
    | IdxIndRmwStage7Dcp
    | IdxIndStage8Rmw
    | AbsIdxStage3X
    | AbsIdxStage3Y
    | AbsIdxStage4
    | AbsIdxWriteStage5Sta
    | AbsIdxWriteStage5Shy
    | AbsIdxWriteStage5Shx
    | AbsIdxWriteStage5Error
    | AbsIdxReadStage4
    | AbsIdxReadStage5Lda
    | AbsIdxReadStage5Cmp
    | AbsIdxReadStage5Sbc
    | AbsIdxReadStage5Adc
    | AbsIdxReadStage5Eor
    | AbsIdxReadStage5Ldx
    | AbsIdxReadStage5And
    | AbsIdxReadStage5Ora
    | AbsIdxReadStage5Ldy
    | AbsIdxReadStage5Nop
    | AbsIdxReadStage5Lax
    | AbsIdxReadStage5Error
    | AbsIdxRmwStage5
    | AbsIdxRmwStage7
    | AbsIdxRmwStage6Ror
    | AbsIdxRmwStage6Dec
    | AbsIdxRmwStage6Inc
    | AbsIdxRmwStage6Asl
    | AbsIdxRmwStage6Lsr
    | AbsIdxRmwStage6Rol
    | AbsIdxRmwStage6Slo
    | AbsIdxRmwStage6Rla
    | AbsIdxRmwStage6Sre
    | AbsIdxRmwStage6Rra
    | AbsIdxRmwStage6Dcp
    | AbsIdxRmwStage6Isc
    | IncS
    | DecS
    | PushPcl
    | PushPch
    | PushP
    | PullP
    | PullPcl
    | PullPchNoInc
    | PushA
    | PullANoInc
    | PullPNoInc
    | PushPBrk
    | PushPNmi
    | PushPIrq
    | PushPReset
    | PushDummy
    | FetchPclVector
    | FetchPchVector
    | ImpAslA
    | ImpRolA
    | ImpRorA
    | ImpLsrA
    | ImpSec
    | ImpCli
    | ImpSei
    | ImpCld
    | ImpClc
    | ImpClv
    | ImpSed
    | ImpIny
    | ImpDey
    | ImpInx
    | ImpDex
    | ImpTsx
    | ImpTxs
    | ImpTax
    | ImpTay
    | ImpTya
    | ImpTxa
    | ImmCmp
    | ImmAdc
    | ImmAnd
    | ImmSbc
    | ImmOra
    | ImmEor
    | ImmCpy
    | ImmCpx
    | ImmAnc
    | ImmAsr
    | ImmArr
    | ImmLxa
    | ImmAxs
    | ImmLda
    | ImmLdx
    | ImmLdy
    | ImmUnsupported
    | NzX
    | NzY
    | NzA
    | RelBranchStage2Bne
    | RelBranchStage2Bpl
    | RelBranchStage2Bcc
    | RelBranchStage2Bcs
    | RelBranchStage2Beq
    | RelBranchStage2Bmi
    | RelBranchStage2Bvc
    | RelBranchStage2Bvs
    | RelBranchStage2
    | RelBranchStage3
    | RelBranchStage4
    | AluEor
    | AluBit
    | AluCpx
    | AluCpy
    | AluCmp
    | AluAdc
    | AluSbc
    | AluOra
    | AluAnd
    | AluAnc
    | AluAsr
    | AluArr
    | AluLxa
    | AluAxs
    | AbsIndJmpStage4
    | AbsIndJmpStage5
    | IndIdxStage3
    | IndIdxStage4
    | IndIdxReadStage5
    | IndIdxWriteStage5
    | IndIdxWriteStage6Sta
    | IndIdxWriteStage6Sha
    | IndIdxReadStage6Lda
    | IndIdxReadStage6Cmp
    | IndIdxReadStage6Ora
    | IndIdxReadStage6Sbc
    | IndIdxReadStage6Adc
    | IndIdxReadStage6And
    | IndIdxReadStage6Eor
    | IndIdxReadStage6Lax
    | IndIdxRmwStage5
    | IndIdxRmwStage6
    | IndIdxRmwStage7Slo
    | IndIdxRmwStage7Rla
    | IndIdxRmwStage7Sre
    | IndIdxRmwStage7Rra
    | IndIdxRmwStage7Isc
    | IndIdxRmwStage7Dcp
    | IndIdxStage8Rmw
    | End
    | EndISpecial
    | EndBranchSpecial
    | EndSuppressInterrupt
    | Jam

type Mos6502Configuration(lxaConstant:int, hasDecimalMode:bool) =
    member val LxaConstant = lxaConstant
    member val HasDecimalMode = hasDecimalMode

type Mos6502(config:Mos6502Configuration, memory:IMemory) =
    let microCode =
        [|
            // 00
            [| Uop.Fetch2; Uop.PushPch; Uop.PushPcl; Uop.PushPBrk; Uop.FetchPclVector; Uop.FetchPchVector; Uop.EndSuppressInterrupt |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndReadStage6Ora; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndRmwStage6; Uop.IdxIndRmwStage7Slo; Uop.IdxIndStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadNop; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadOra; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwAsl; Uop.ZpRmwStage5; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwSlo; Uop.ZpRmwStage5; Uop.End |];

            // 08
            [| Uop.FetchDummy; Uop.PushP; Uop.End |];
            [| Uop.ImmOra; Uop.End |];
            [| Uop.ImpAslA; Uop.End |];
            [| Uop.ImmAnc; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadNop; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadOra; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Asl; Uop.AbsRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Slo; Uop.AbsRmwStage6; Uop.End |];

            // 10
            [| Uop.RelBranchStage2Bpl; Uop.End |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxReadStage5; Uop.IndIdxReadStage6Ora; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxRmwStage5; Uop.IndIdxRmwStage6; Uop.IndIdxRmwStage7Slo; Uop.IndIdxStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadNop; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadOra; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwAsl; Uop.ZpIdxRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwSlo; Uop.ZpIdxRmwStage6; Uop.End |];

            // 18
            [| Uop.ImpClc; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Ora; Uop.End |];
            [| Uop.FetchDummy; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Slo; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Nop; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Ora; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Asl; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Slo; Uop.AbsIdxRmwStage7; Uop.End |];

            // 20
            [| Uop.Fetch2; Uop.Nop; Uop.PushPch; Uop.PushPcl; Uop.Jsr; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndReadStage6And; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndRmwStage6; Uop.IdxIndRmwStage7Rla; Uop.IdxIndStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadBit; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadAnd; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwRol; Uop.ZpRmwStage5; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwRla; Uop.ZpRmwStage5; Uop.End |];

            // 28
            [| Uop.FetchDummy;  Uop.IncS; Uop.PullPNoInc; Uop.EndISpecial |];
            [| Uop.ImmAnd; Uop.End |];
            [| Uop.ImpRolA; Uop.End |];
            [| Uop.ImmAnc; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadBit; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadAnd; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Rol; Uop.AbsRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Rla; Uop.AbsRmwStage6; Uop.End |];

            // 30
            [| Uop.RelBranchStage2Bmi; Uop.End |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxReadStage5; Uop.IndIdxReadStage6And; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxRmwStage5; Uop.IndIdxRmwStage6; Uop.IndIdxRmwStage7Rla; Uop.IndIdxStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadNop; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadAnd; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwRol; Uop.ZpIdxRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwRla; Uop.ZpIdxRmwStage6; Uop.End |];

            // 38
            [| Uop.ImpSec; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5And; Uop.End |];
            [| Uop.FetchDummy; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Rla; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Nop; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5And; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Rol; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Rla; Uop.AbsIdxRmwStage7; Uop.End |];

            // 40
            [| Uop.FetchDummy; Uop.IncS; Uop.PullP; Uop.PullPcl; Uop.PullPchNoInc; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndReadStage6Eor; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndRmwStage6; Uop.IdxIndRmwStage7Sre; Uop.IdxIndStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadNop; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadEor; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwLsr; Uop.ZpRmwStage5; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwSre; Uop.ZpRmwStage5; Uop.End |];

            // 48
            [| Uop.FetchDummy; Uop.PushA; Uop.End |];
            [| Uop.ImmEor; Uop.End |];
            [| Uop.ImpLsrA; Uop.End |];
            [| Uop.ImmAsr; Uop.End |];
            [| Uop.Fetch2; Uop.JmpAbs; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadEor; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Lsr; Uop.AbsRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Sre; Uop.AbsRmwStage6; Uop.End |];

            // 50
            [| Uop.RelBranchStage2Bvc; Uop.End |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxReadStage5; Uop.IndIdxReadStage6Eor; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxRmwStage5; Uop.IndIdxRmwStage6; Uop.IndIdxRmwStage7Sre; Uop.IndIdxStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadNop; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadEor; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwLsr; Uop.ZpIdxRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwSre; Uop.ZpIdxRmwStage6; Uop.End |];

            // 58
            [| Uop.ImpCli; Uop.EndISpecial |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Eor; Uop.End |];
            [| Uop.FetchDummy; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Sre; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Nop; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Eor; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Lsr; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Sre; Uop.AbsIdxRmwStage7; Uop.End |];

            // 60
            [| Uop.FetchDummy; Uop.IncS; Uop.PullPcl; Uop.PullPchNoInc; Uop.IncPc; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndReadStage6Adc; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndRmwStage6; Uop.IdxIndRmwStage7Rra; Uop.IdxIndStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadNop; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadAdc; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwRor; Uop.ZpRmwStage5; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwRra; Uop.ZpRmwStage5; Uop.End |];

            // 68
            [| Uop.FetchDummy; Uop.IncS; Uop.PullANoInc; Uop.End |];
            [| Uop.ImmAdc; Uop.End |];
            [| Uop.ImpRorA; Uop.End |];
            [| Uop.ImmArr; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsIndJmpStage4; Uop.AbsIndJmpStage5; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadAdc; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Ror; Uop.AbsRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Rra; Uop.AbsRmwStage6; Uop.End |];

            // 70
            [| Uop.RelBranchStage2Bvs; Uop.End |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxReadStage5; Uop.IndIdxReadStage6Adc; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxRmwStage5; Uop.IndIdxRmwStage6; Uop.IndIdxRmwStage7Rra; Uop.IndIdxStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadNop; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadAdc; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwRor; Uop.ZpIdxRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwRra; Uop.ZpIdxRmwStage6; Uop.End |];

            // 78
            [| Uop.ImpSei; Uop.EndISpecial |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Adc; Uop.End |];
            [| Uop.FetchDummy; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Rra; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Nop; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Adc; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Ror; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Rra; Uop.AbsIdxRmwStage7; Uop.End |];

            // 80
            [| Uop.ImmUnsupported; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndWriteStage6Sta; Uop.End |];
            [| Uop.ImmUnsupported; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndWriteStage6Sax; Uop.End |];
            [| Uop.Fetch2; Uop.ZpWriteSty; Uop.End |];
            [| Uop.Fetch2; Uop.ZpWriteSta; Uop.End |];
            [| Uop.Fetch2; Uop.ZpWriteStx; Uop.End |];
            [| Uop.Fetch2; Uop.ZpWriteSax; Uop.End |];

            // 88
            [| Uop.ImpDey; Uop.End |];
            [| Uop.ImmUnsupported; Uop.End |];
            [| Uop.ImpTxa; Uop.End |];
            [| Uop.ImmUnsupported; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsWriteSty; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsWriteSta; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsWriteStx; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsWriteSax; Uop.End |];

            // 90
            [| Uop.RelBranchStage2Bcc; Uop.End |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxWriteStage5; Uop.IndIdxWriteStage6Sta; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxWriteStage5; Uop.IndIdxWriteStage6Sha; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpWriteSty; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpWriteSta; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3Y; Uop.ZpWriteStx; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3Y; Uop.ZpWriteSax; Uop.End |];

            // 98
            [| Uop.ImpTya; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxStage4; Uop.AbsIdxWriteStage5Sta; Uop.End |];
            [| Uop.ImpTxs; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxStage4; Uop.AbsIdxWriteStage5Error; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxStage4; Uop.AbsIdxWriteStage5Shy; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxStage4; Uop.AbsIdxWriteStage5Sta; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxStage4; Uop.AbsIdxWriteStage5Shx; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxStage4; Uop.AbsIdxWriteStage5Shx; Uop.End |];

            // A0
            [| Uop.ImmLdy; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndReadStage6Lda; Uop.End |];
            [| Uop.ImmLdx; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndReadStage6Lax; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadLdy; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadLda; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadLdx; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadLax; Uop.End |];

            // A8
            [| Uop.ImpTay; Uop.End |];
            [| Uop.ImmLda; Uop.End |];
            [| Uop.ImpTax; Uop.End |];
            [| Uop.ImmLxa; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadLdy; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadLda; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadLdx; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadLax; Uop.End |];

            // B0
            [| Uop.RelBranchStage2Bcs; Uop.End |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxReadStage5; Uop.IndIdxReadStage6Lda; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxReadStage5; Uop.IndIdxReadStage6Lax; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadLdy; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadLda; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3Y; Uop.ZpReadLdx; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3Y; Uop.ZpReadLax; Uop.End |];

            // B8
            [| Uop.ImpClv; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Lda; Uop.End |];
            [| Uop.ImpTsx; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Error; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Ldy; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Lda; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Ldx; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Lax; Uop.End |];

            // C0
            [| Uop.ImmCpy; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndReadStage6Cmp; Uop.End |];
            [| Uop.ImmUnsupported; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndRmwStage6; Uop.IdxIndRmwStage7Dcp; Uop.IdxIndStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadCpy; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadCmp; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwDec; Uop.ZpRmwStage5; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwDcp; Uop.ZpRmwStage5; Uop.End |];

            // C8
            [| Uop.ImpIny; Uop.End |];
            [| Uop.ImmCmp; Uop.End |];
            [| Uop.ImpDex; Uop.End |];
            [| Uop.ImmAxs; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadCpy; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadCmp; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Dec; Uop.AbsRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Dcp; Uop.AbsRmwStage6; Uop.End |];

            // D0
            [| Uop.RelBranchStage2Bne; Uop.End |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxReadStage5; Uop.IndIdxReadStage6Cmp; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxRmwStage5; Uop.IndIdxRmwStage6; Uop.IndIdxRmwStage7Dcp; Uop.IndIdxStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadNop; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadCmp; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwDec; Uop.ZpIdxRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwDcp; Uop.ZpIdxRmwStage6; Uop.End |];

            // D8
            [| Uop.ImpCld; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Cmp; Uop.End |];
            [| Uop.FetchDummy; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Dcp; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Nop; Uop.End|];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Cmp; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Dec; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Dcp; Uop.AbsIdxRmwStage7; Uop.End |];

            // E0
            [| Uop.ImmCpx; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndReadStage6Sbc; Uop.End |];
            [| Uop.ImmUnsupported; Uop.End |];
            [| Uop.Fetch2; Uop.IdxIndStage3; Uop.IdxIndStage4; Uop.IdxIndStage5; Uop.IdxIndRmwStage6; Uop.IdxIndRmwStage7Isc; Uop.IdxIndStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadCpx; Uop.End |];
            [| Uop.Fetch2; Uop.ZpReadSbc; Uop.End|];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwInc; Uop.ZpRmwStage5; Uop.End |];
            [| Uop.Fetch2; Uop.ZpRmwStage3; Uop.ZpRmwIsc; Uop.ZpRmwStage5; Uop.End |];

            // E8
            [| Uop.ImpInx; Uop.End |];
            [| Uop.ImmSbc; Uop.End |];
            [| Uop.FetchDummy; Uop.End |];
            [| Uop.ImmSbc; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadCpx; Uop.End|];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsReadSbc; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Inc; Uop.AbsRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.Fetch3; Uop.AbsRmwStage4; Uop.AbsRmwStage5Isc; Uop.AbsRmwStage6; Uop.End |];

            // F0
            [| Uop.RelBranchStage2Beq; Uop.End |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxReadStage5; Uop.IndIdxReadStage6Sbc; Uop.End |];
            [| Uop.Jam |];
            [| Uop.Fetch2; Uop.IndIdxStage3; Uop.IndIdxStage4; Uop.IndIdxRmwStage5; Uop.IndIdxRmwStage6; Uop.IndIdxRmwStage7Isc; Uop.IndIdxStage8Rmw; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadNop; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpReadSbc; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwInc; Uop.ZpIdxRmwStage6; Uop.End |];
            [| Uop.Fetch2; Uop.ZpIdxStage3X; Uop.ZpIdxRmwStage4; Uop.ZpRmwIsc; Uop.ZpIdxRmwStage6; Uop.End |];

            // F8
            [| Uop.ImpSed; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Sbc; Uop.End |];
            [| Uop.FetchDummy; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3Y;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Isc; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Nop; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X; Uop.AbsIdxReadStage4; Uop.AbsIdxReadStage5Sbc; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Inc; Uop.AbsIdxRmwStage7; Uop.End |];
            [| Uop.Fetch2; Uop.AbsIdxStage3X;  Uop.AbsIdxStage4; Uop.AbsIdxRmwStage5; Uop.AbsIdxRmwStage6Isc; Uop.AbsIdxRmwStage7; Uop.End |];

            // 100 (VOP_Fetch1)
            [| Uop.Fetch1 |];
            // 101 (VOP_RelativeStuff)
            [| Uop.RelBranchStage3; Uop.EndBranchSpecial |];
            // 102 (VOP_RelativeStuff2)
            [| Uop.RelBranchStage4; Uop.End |];
            // 103 (VOP_RelativeStuff3)
            [| Uop.EndSuppressInterrupt |]
            // 104 (VOP_NMI)
            [| Uop.FetchDummy; Uop.FetchDummy; Uop.PushPch; Uop.PushPcl; Uop.PushPNmi; Uop.FetchPclVector; Uop.FetchPchVector; Uop.EndSuppressInterrupt |];
            // 105 (VOP_IRQ)
            [| Uop.FetchDummy; Uop.FetchDummy; Uop.PushPch; Uop.PushPcl; Uop.PushPIrq; Uop.FetchPclVector; Uop.FetchPchVector; Uop.EndSuppressInterrupt |];
            // 106 (VOP_RESET)
            [| Uop.FetchDummy; Uop.FetchDummy; Uop.PushDummy; Uop.PushDummy; Uop.PushPReset; Uop.FetchPclVector; Uop.FetchPchVector; Uop.EndSuppressInterrupt |];
            // 107 (VOP_Fetch1_NoInterrupt)
            [| Uop.Fetch1Real |];
        |]

    [<Literal>]
    let vopFetch1 = 0x100
    [<Literal>]
    let vopRelativeStuff = 0x101
    [<Literal>]
    let vopRelativeStuff2 = 0x102
    [<Literal>]
    let vopRelativeStuff3 = 0x103
    [<Literal>]
    let vopNmi = 0x104
    [<Literal>]
    let vopIrq = 0x105
    [<Literal>]
    let vopReset = 0x106
    [<Literal>]
    let vopFetch1NoInterrupt = 0x107
    [<Literal>]
    let vopNum = 0x108

    [<Literal>]
    let nmiVector = 0xFFFA
    [<Literal>]
    let resetVector = 0xFFFC
    [<Literal>]
    let irqVector = 0xFFFE
    [<Literal>]
    let brkVector = 0xFFFE

    let mutable opcode = vopFetch1
    let mutable opcode2 = 0
    let mutable opcode3 = 0
    let mutable ea = 0
    let mutable aluTemp = 0
    let mutable mi = 0
    let mutable myIFlag = false
    let mutable iFlagPending = false
    let mutable rdyFreeze = false
    let mutable interruptPending = false
    let mutable branchIrqHack = false
    let mutable irq = false
    let mutable nmi = false
    let mutable rdy = false
    let mutable value8 = 0
    let mutable value16 = 0

    let mutable pc = 0
    let mutable a = 0
    let mutable x = 0
    let mutable y = 0
    let mutable s = 0

    let mutable isDecimalMode = false

    let mutable n = false
    let mutable v = false
    let mutable b = false
    let mutable d = false
    let mutable i = false
    let mutable z = false
    let mutable c = false

    let rec ExecuteOneRetry () =
        let NZ value =
            z <- (value &&& 0xFF) = 0
            n <- (value &&& 0x80) <> 0

        let ReadMemory address =
            memory.Read(address)

        let WriteMemory address value =
            memory.Write(address, value)
            value

        let ReadMemoryPcIncrement () =
            let result = ReadMemory pc
            pc <- pc + 1
            result

        let ReadMemoryS () =
            ReadMemory (0x100 ||| s)

        let WriteMemoryS value =
            value |> WriteMemory (0x100 ||| s) |> ignore

        let DecrementS () =
            s <- (s - 1 &&& 0xFF)

        let IncrementS () =
            s <- (s + 1 &&& 0xFF)

        let Push value =
            value |> WriteMemoryS
            DecrementS()

        let Pull () =
            IncrementS()
            ReadMemoryS()

        let GetP () =
            0x20 |||
            (if n then 0x80 else 0x00) |||
            (if v then 0x40 else 0x00) |||
            (if b then 0x10 else 0x00) |||
            (if d then 0x08 else 0x00) |||
            (if i then 0x04 else 0x00) |||
            (if z then 0x02 else 0x00) |||
            (if c then 0x01 else 0x00)

        let SetP value =
            n <- (value &&& 0x80) <> 0
            v <- (value &&& 0x40) <> 0
            b <- (value &&& 0x10) <> 0
            d <- (value &&& 0x08) <> 0
            i <- (value &&& 0x04) <> 0
            z <- (value &&& 0x02) <> 0
            c <- (value &&& 0x01) <> 0

        let IfReady action =
            if rdy then
                action
            rdy


        // ----- ALU -----


        let Cmp register value =
            aluTemp <- (register - value) &&& 0xFF
            c <- register >= aluTemp
            NZ aluTemp

        let And value =
            aluTemp <- a &&& value
            a <- aluTemp
            NZ a

        let Bit value =
            aluTemp <- value
            n <- (aluTemp &&& 0x80) <> 0
            v <- (aluTemp &&& 0x40) <> 0
            z <- (a &&& aluTemp) = 0

        let Eor value =
            aluTemp <- a ^^^ value
            a <- aluTemp
            NZ a

        let Ora value =
            aluTemp <- a ||| value
            a <- aluTemp
            NZ a

        let Anc value =
            aluTemp <- a &&& value
            c <- (aluTemp &&& 0x80) <> 0
            a <- aluTemp
            NZ a

        let Asr value =
            aluTemp <- value
            let anded = a &&& aluTemp
            c <- (anded &&& 0x01) <> 0
            aluTemp <- anded >>> 1
            a <- aluTemp
            NZ a

        let Axs value =
            aluTemp <- value
            x <- x &&& a
            aluTemp <- (x - aluTemp)
            x <- aluTemp &&& 0xFF
            c <- (aluTemp &&& 0x100) = 0
            NZ x

        let Arr value =
            aluTemp <-
                let initialMask = a &&& value
                let binaryResult = (initialMask >>> 1) ||| (if c then 0x80 else 0x00)
                if isDecimalMode then
                    n <- (a &&& 0x80) <> 0
                    z <- binaryResult = 0
                    v <- (binaryResult ^^^ initialMask) &&& 0x40 <> 0
                    let lowResult =
                        if ((initialMask &&& 0xf) + (initialMask &&& 0x1)) > 0x5 then
                            (binaryResult &&& 0xf0) ||| ((binaryResult + 0x6) &&& 0xf)
                        else
                            binaryResult
                    if ((initialMask &&& 0xf0) + (initialMask &&& 0x10)) > 0x50 then
                        c <- true
                        (lowResult &&& 0x0f) ||| ((lowResult + 0x60) &&& 0xf0)
                    else
                        c <- false
                        lowResult
                else
                    NZ binaryResult
                    c <- (binaryResult &&& 0x40) <> 0
                    v <- (binaryResult &&& 0x40) <> ((binaryResult &&& 0x20) <<< 1)
                    binaryResult
            a <- aluTemp

        let Lxa value =
            aluTemp <- (a ||| config.LxaConstant) &&& value
            a <- aluTemp
            x <- aluTemp
            NZ a

        let Sbc value =
            let inline setV input result =
                v <- ((a ^^^ input) &&& 0x80) &&& ((a ^^^ result) &&& 0x80) <> 0
            aluTemp <-
                let binaryResult = a - value - (if c then 0 else 1)
                if isDecimalMode then
                    let initialSub = (a &&& 0x0F) - (value &&& 0x0F) - (if c then 0 else 1)
                    let adjustedSub =
                        if (initialSub &&& 0x10) <> 0 then
                            ((initialSub - 6) &&& 0x0F) ||| ((a &&& 0xF0) - (value &&& 0xF0) - 0x10)
                        else
                            (initialSub &&& 0x0F) ||| ((a &&& 0xF0) - (value &&& 0xF0))
                    let result = (if (adjustedSub &&& 0x100) <> 0 then (adjustedSub - 0x060) else adjustedSub)
                    z <- (binaryResult &&& 0xFF) = 0
                    n <- (binaryResult &&& 0x80) <> 0
                    c <- binaryResult < 0x100
                    setV value binaryResult
                    result
                else
                    NZ binaryResult
                    c <- binaryResult > 0xFF
                    setV value binaryResult
                    binaryResult
            a <- aluTemp

        let Adc value =
            let inline setV input result =
                v <- (((a ^^^ input) &&& 0x80) ^^^ 0x80) &&& ((a ^^^ result) &&& 0x80) <> 0
            aluTemp <-
                let binaryResult = value + a + (if c then 1 else 0)
                if isDecimalMode then
                    let initialAdd = (a &&& 0x0F) + (value &&& 0x0F) + (if c then 1 else 0)
                    let adjustedAdd = initialAdd + (if initialAdd > 9 then 6 else 0)
                    let result = (initialAdd &&& 0x0F) + (a &&& 0xF0) + (value &&& 0xF0) + (if adjustedAdd > 0x0F then 0x10 else 0x00)
                    z <- (binaryResult &&& 0xFF) = 0
                    n <- (result &&& 0x80) <> 0
                    c <- (result &&& 0x1FF) > 0x0F0
                    setV value result
                    result
                else
                    NZ binaryResult
                    c <- binaryResult > 0xFF
                    setV aluTemp binaryResult
                    binaryResult
            a <- aluTemp

        let Slo value =
            c <- (value &&& 0x80) <> 0
            aluTemp <- ((value <<< 1) &&& 0xFF) ||| a
            a <- aluTemp
            NZ a

        let Isc value =
            (value + 1) &&& 0xFF |> Sbc

        let Dcp value =
            c <- (value &&& 0x01) <> 0
            (value - 1) &&& 0xFF |> Cmp a

        let Sre value =
            c <- (value &&& 0x01) <> 0
            aluTemp <- ((value >>> 1) &&& 0xFF) ^^^ a
            a <- aluTemp
            NZ a

        let Rra value =
            let oldC = c
            c <- (value &&& 0x01) <> 0
            (value >>> 1) ||| (if oldC then 0x80 else 0x00) |> Adc

        let Rla value =
            let oldC = c
            c <- (value &&& 0x80) <> 0
            aluTemp <- (((value <<< 1) &&& 0xFF) ||| (if oldC then 0x01 else 0x00)) &&& a
            a <- aluTemp
            NZ aluTemp

        let Lsr value =
            aluTemp <- value
            c <- (aluTemp &&& 0x01) <> 0
            aluTemp <- aluTemp >>> 1
            NZ aluTemp
            aluTemp

        let Asl value =
            aluTemp <- value
            c <- (aluTemp &&& 0x80) <> 0
            aluTemp <- (aluTemp <<< 1) &&& 0xFF
            NZ aluTemp
            aluTemp

        let Inc value =
            aluTemp <- (value + 1) &&& 0xFF
            NZ aluTemp
            aluTemp

        let Dec value =
            aluTemp <- (value - 1) &&& 0xFF
            NZ aluTemp
            aluTemp

        let Lda value =
            a <- value
            NZ a

        let Ldx value =
            x <- value
            NZ x

        let Ldy value =
            y <- value
            NZ y

        let Lax value =
            x <- value
            a <- value
            NZ a

        let Rol value =
            let oldC = c
            c <- (aluTemp &&& 0x80) <> 0
            aluTemp <- ((a <<< 1) &&& 0xFF) ||| (if oldC then 0x01 else 0x00)
            NZ aluTemp
            aluTemp

        let Ror value =
            let oldC = c
            c <- (aluTemp &&& 0x01) <> 0
            aluTemp <- (a >>> 1) ||| (if oldC then 0x80 else 0x00)
            NZ aluTemp
            aluTemp


        // ----- uOPS -----


        let FetchDummy () =
            ReadMemory pc |> ignore

        let Fetch1Real () =
            IfReady <| (
                branchIrqHack <- false
                opcode <- ReadMemoryPcIncrement()
                pc <- pc + 1
                mi <- -1)

        let Fetch1 () =
            IfReady <| (
                myIFlag <- i
                i <- iFlagPending
                if not branchIrqHack then
                    interruptPending <- false
                    if nmi then
                        ea <- nmiVector
                        opcode <- vopNmi
                        nmi <- false
                        mi <- 0
                        ExecuteOneRetry()
                    else
                        if irq && (not myIFlag) then
                            ea <- irqVector
                            opcode <- vopIrq
                            mi <- 0
                            ExecuteOneRetry()
                else
                    Fetch1Real |> ignore
            )

        let Fetch2 () =
            if rdy then
                opcode2 <- ReadMemoryPcIncrement()

        let Fetch3 () =
            if rdy then
                opcode3 <- ReadMemoryPcIncrement()

        let PushPch () =
            pc >>> 8 |> WriteMemoryS
            DecrementS()

        let PushPcl () =
            WriteMemoryS pc
            DecrementS()

        let PushPBrk () =
            b <- true
            GetP() |> WriteMemoryS
            i <- true
            ea <- brkVector

        let PushPIrq () =
            b <- false
            GetP() |> WriteMemoryS
            i <- true
            ea <- irqVector
            
        let PushPNmi () =
            b <- false
            GetP() |> WriteMemoryS
            i <- true
            ea <- nmiVector

        let PushPReset () =
            ea <- resetVector
            DecrementS()
            i <- true

        let PushDummy () =
            DecrementS()

        let FetchPclVector () =
            if rdy then
                if nmi && ((ea = brkVector && b) || (ea = irqVector && (not b))) then
                    nmi <- false
                    ea <- nmiVector
                aluTemp <- ReadMemory ea
            rdy
            
        let FetchPchVector () =
            if rdy then
                aluTemp <- aluTemp ||| (ReadMemory(ea) <<< 8)
                pc <- aluTemp
            rdy

        let ImpIny () =
            if rdy then
                FetchDummy()
                y <- y + 1
                NZ y
            rdy

        let ImpDey () =
            if rdy then
                FetchDummy()
                y <- y - 1
                NZ y
            rdy
            
        let ImpInx () =
            if rdy then
                FetchDummy()
                x <- x + 1
                NZ x
            rdy

        let ImpDex () =
            if rdy then
                FetchDummy()
                x <- x - 1
                NZ x
            rdy

        let ImpTsx () =
            if rdy then
                FetchDummy()
                x <- s
                NZ x
            rdy

        let ImpTxs () =
            if rdy then
                FetchDummy()
                s <- x
            rdy

        let ImpTax () =
            if rdy then
                FetchDummy()
                x <- a
                NZ x
            rdy

        let ImpTay () =
            if rdy then
                FetchDummy()
                y <- a
                NZ y
            rdy
            
        let ImpTya () =
            if rdy then
                FetchDummy()
                a <- y
                NZ a
            rdy
        
        let ImpTxa () =
            if rdy then
                FetchDummy()
                a <- x
                NZ a
            rdy

        let ImpSei () =
            if rdy then
                FetchDummy()
                iFlagPending <- true
            rdy

        let ImpCli () =
            if rdy then
                FetchDummy()
                iFlagPending <- false
            rdy

        let ImpSec () =
            if rdy then
                FetchDummy()
                c <- true
            rdy

        let ImpClc () =
            if rdy then
                FetchDummy()
                c <- false
            rdy

        let ImpSed () =
            if rdy then
                FetchDummy()
                d <- true
                isDecimalMode <- config.HasDecimalMode
            rdy

        let ImpCld () =
            if rdy then
                FetchDummy()
                d <- false
                isDecimalMode <- false
            rdy

        let ImpClv () =
            if rdy then
                FetchDummy()
                v <- false
            rdy

        let AbsWrite value =
            (opcode3 <<< 8) ||| opcode2 |> WriteMemory value |> ignore
            true

        let AbsWriteSta () =
            AbsWrite a

        let AbsWriteStx () =
            AbsWrite x
            
        let AbsWriteSty () =
            AbsWrite y

        let AbsWriteSax () =
            AbsWrite (x &&& a)

        let ZpWrite value =
            value |> WriteMemory opcode2 |> ignore
            true

        let ZpWriteSta () =
            ZpWrite a

        let ZpWriteStx () =
            ZpWrite x

        let ZpWriteSty () =
            ZpWrite y

        let ZpWriteSax () =
            ZpWrite (x &&& a)

        let IndIdxStage3 () =
            if rdy then
                ea <- ReadMemory opcode2
            rdy

        let IndIdxStage4 () =
            if rdy then
                aluTemp <- ea + y
                ea <- (((opcode2 + 1) &&& 0xFF) <<< 8) ||| (aluTemp &&& 0xFF) |> ReadMemory
            rdy

        let IndIdxWriteStage5 () =
            if rdy then
                ReadMemory ea |> ignore
                ea <- ea + (aluTemp &&& 0xFF00)
            rdy

        let IndIdxReadStage5 () =
            if rdy then
                if aluTemp >= 0x100 then
                    mi <- mi + 1
                    ExecuteOneRetry |> ignore
                else
                    ReadMemory ea |> ignore
                    ea <- (ea + 0x100) &&& 0xFFFF
            rdy

        let IndIdxRmwStage5 () =
            if rdy then
                if aluTemp >= 0x100 then
                    ea <- (ea + 0x100) &&& 0xFFFF
                ReadMemory ea |> ignore
            rdy

        let IndIdxWriteStage6 value =
            value |> WriteMemory ea |> ignore
            true

        let IndIdxWriteStage6Sta () =
            IndIdxWriteStage6 a

        let IndIdxWriteStage6Sha () =
            IndIdxWriteStage6 (a &&& x &&& 7)

        let IndIdxReadStage6 operation =
            IfReady <| (ReadMemory ea |> operation)

        let IndIdxReadStage6Lda () =
            IndIdxReadStage6 <| Lda

        let IndIdxReadStage6Cmp () =
            IndIdxReadStage6 <| Cmp a

        let IndIdxReadStage6And () =
            IndIdxReadStage6 <| And

        let IndIdxReadStage6Eor () =
            IndIdxReadStage6 <| Eor

        let IndIdxReadStage6Lax () =
            IndIdxReadStage6 <| Lax

        let IndIdxReadStage6Adc () =
            IndIdxReadStage6 <| Adc

        let IndIdxReadStage6Sbc () =
            IndIdxReadStage6 <| Sbc

        let IndIdxReadStage6Ora () =
            IndIdxReadStage6 <| Ora

        let IndIdxRmwStage6 () =
            IfReady <| (aluTemp <- ReadMemory ea)

        let IndIdxRmwStage7 operation =
            aluTemp |> WriteMemory ea |> operation |> ignore
            true

        let IndIdxRmwStage7Slo () =
            IndIdxRmwStage7 <| Slo

        let IndIdxRmwStage7Sre () =
            IndIdxRmwStage7 <| Sre

        let IndIdxRmwStage7Rra () =
            IndIdxRmwStage7 <| Rra

        let IndIdxRmwStage7Isc () =
            IndIdxRmwStage7 <| Isc

        let IndIdxRmwStage7Dcp () =
            IndIdxRmwStage7 <| Dcp

        let IndIdxRmwStage7Rla () =
            IndIdxRmwStage7 <| Rla

        let IndIdxRmwStage8 () =
            aluTemp |> WriteMemory ea |> ignore
            true

        let RelBranchStage2 branchTaken =
            if rdy then
                opcode2 <- ReadMemoryPcIncrement()
                if branchTaken then
                    opcode <- vopRelativeStuff
                    mi <- -1
            rdy
            
        let RelBranchStage2Bvs () =
            RelBranchStage2 v

        let RelBranchStage2Bvc () =
            RelBranchStage2 (not v)

        let RelBranchStage2Bmi () =
            RelBranchStage2 n

        let RelBranchStage2Bpl () =
            RelBranchStage2 (not n)

        let RelBranchStage2Bcs () =
            RelBranchStage2 c

        let RelBranchStage2Bcc () =
            RelBranchStage2 (not c)

        let RelBranchStage2Beq () =
            RelBranchStage2 z

        let RelBranchStage2Bne () =
            RelBranchStage2 (not z)

        let RelBranchStage3 () =
            if rdy then
                FetchDummy()
                aluTemp <- (pc &&& 0xFF) + opcode2
                pc <- (pc &&& 0xFF00) ||| (aluTemp &&& 0xFF)
                if aluTemp >= 0x100 then
                    opcode <- vopRelativeStuff2
                    mi <- -1
                else
                    if interruptPending then
                        branchIrqHack <- true
            rdy

        let RelBranchStage4 () =
            if rdy then
                FetchDummy()
                pc <- (pc + (if aluTemp < 0 then -256 else 256)) &&& 0xFFFF
            rdy

        let Nop () =
            if rdy then
                FetchDummy()
            rdy

        let DecS () =
            if rdy then
                FetchDummy()
                DecrementS()
            rdy

        let IncS () =
            if rdy then
                FetchDummy()
                IncrementS()
            rdy

        let Jsr () =
            if rdy then
                pc <- (ReadMemory(pc) <<< 8) ||| opcode2
            rdy

        let PullP () =
            if rdy then
                ReadMemoryS() |> SetP
                IncrementS()
            rdy

        let PullPcl () =
            if rdy then
                pc <- (pc &&& 0xFF00) ||| ReadMemoryS()
                IncrementS()
            rdy

        let PullPchNoInc () =
            if rdy then
                pc <- (pc &&& 0x00FF) ||| (ReadMemoryS() <<< 8)
            rdy

        let AbsReadLda () =
            if rdy then
                a <- ReadMemory((opcode3 <<< 8) ||| opcode2)
                NZ a
            rdy

        let AbsReadLdy () =
            if rdy then
                y <- ReadMemory((opcode3 <<< 8) ||| opcode2)
                NZ y
            rdy

        let AbsReadLdx () =
            if rdy then
                x <- ReadMemory((opcode3 <<< 8) ||| opcode2)
                NZ x
            rdy

        let AbsReadBit () =
            if rdy then
                ReadMemory((opcode3 <<< 8) ||| opcode2) |> Bit
            rdy

        let AbsReadLax () =
            if rdy then
                aluTemp <- ReadMemory((opcode3 <<< 8) ||| opcode2)
                a <- aluTemp
                x <- a
                NZ a
            rdy

        let AbsReadAnd () =
            if rdy then
                ReadMemory((opcode3 <<< 8) ||| opcode2) |> And
            rdy

        let AbsReadEor () =
            if rdy then
                ReadMemory((opcode3 <<< 8) ||| opcode2) |> Eor
            rdy

        let AbsReadOra () =
            if rdy then
                ReadMemory((opcode3 <<< 8) ||| opcode2) |> Ora
            rdy

        let AbsReadAdc () =
            if rdy then
                ReadMemory((opcode3 <<< 8) ||| opcode2) |> Adc
            rdy

        let AbsReadCmp () =
            if rdy then
                ReadMemory((opcode3 <<< 8) ||| opcode2) |> Cmp a
            rdy

        let AbsReadCpy () =
            if rdy then
                ReadMemory((opcode3 <<< 8) ||| opcode2) |> Cmp y
            rdy

        let AbsReadNop () =
            if rdy then
                ReadMemory((opcode3 <<< 8) ||| opcode2) |> ignore
            rdy

        let AbsReadCpx () =
            if rdy then
                ReadMemory((opcode3 <<< 8) ||| opcode2) |> Cmp x
            rdy

        let AbsReadSbc () =
            if rdy then
                ReadMemory((opcode3 <<< 8) ||| opcode2) |> Sbc
            rdy

        let ZpIdxStage3X () =
            if rdy then
                ReadMemory opcode2 |> ignore
                opcode2 <- (opcode2 + x) &&& 0xFF
            rdy

        let ZpIdxStage3Y () =
            IfReady <| (
                ReadMemory opcode2 |> ignore
                opcode2 <- (opcode2 + y) &&& 0xFF)

        let ZpIdxRmwStage4 () =
            IfReady <| (aluTemp <- ReadMemory opcode2)

        let ZpIdxRmwStage6 () =
            aluTemp |> WriteMemory opcode2 |> ignore
            true

        let ZpReadEor () =
            if rdy then
                ReadMemory opcode2 |> Eor
            rdy

        let ZpReadBit () =
            if rdy then
                ReadMemory opcode2 |> Bit
            rdy

        let ZpReadLda () =
            if rdy then
                ReadMemory opcode2 |> Lda
            rdy

        let ZpReadLdy () =
            if rdy then
                ReadMemory opcode2 |> Ldy
            rdy

        let ZpReadLdx () =
            if rdy then
                ReadMemory opcode2 |> Ldx
            rdy

        let ZpReadLax () =
            if rdy then
                ReadMemory opcode2 |> Lax
            rdy

        let ZpReadCpy () =
            if rdy then
                ReadMemory opcode2 |> Cmp y
            rdy

        let ZpReadCmp () =
            if rdy then
                ReadMemory opcode2 |> Cmp a
            rdy

        let ZpReadCpx () =
            if rdy then
                ReadMemory opcode2 |> Cmp x
            rdy

        let ZpReadOra () =
            if rdy then
                ReadMemory opcode2 |> Ora
            rdy

        let ZpReadNop () =
            if rdy then
                ReadMemory opcode2 |> ignore
            rdy

        let ZpReadSbc () =
            if rdy then
                ReadMemory opcode2 |> Sbc
            rdy

        let ZpReadAdc () =
            if rdy then
                ReadMemory opcode2 |> Adc
            rdy

        let ZpReadAnd () =
            if rdy then
                ReadMemory opcode2 |> And
            rdy

        let ImmEor () =
            if rdy then
                ReadMemoryPcIncrement() |> Eor
            rdy

        let ImmAnc () =
            if rdy then
                ReadMemoryPcIncrement() |> Anc
            rdy

        let ImmAsr () =
            if rdy then
                ReadMemoryPcIncrement() |> Asr
            rdy

        let ImmAxs () =
            if rdy then
                ReadMemoryPcIncrement() |> Axs
            rdy

        let ImmArr () =
            if rdy then
                ReadMemoryPcIncrement() |> Arr
            rdy

        let ImmLxa () =
            if rdy then
                ReadMemoryPcIncrement() |> Lxa
            rdy

        let ImmOra () =
            if rdy then
                ReadMemoryPcIncrement() |> Ora
            rdy

        let ImmCpy () =
            if rdy then
                ReadMemoryPcIncrement() |> Cmp y
            rdy

        let ImmCpx () =
            if rdy then
                ReadMemoryPcIncrement() |> Cmp x
            rdy

        let ImmCmp () =
            if rdy then
                ReadMemoryPcIncrement() |> Cmp a
            rdy

        let ImmSbc () =
            if rdy then
                ReadMemoryPcIncrement() |> Sbc
            rdy

        let ImmAnd () =
            if rdy then
                ReadMemoryPcIncrement() |> And
            rdy

        let ImmAdc () =
            if rdy then
                ReadMemoryPcIncrement() |> Adc
            rdy

        let ImmLda () =
            if rdy then
                ReadMemoryPcIncrement() |> Lda
            rdy

        let ImmLdx () =
            if rdy then
                ReadMemoryPcIncrement() |> Ldx
            rdy

        let ImmLdy () =
            if rdy then
                ReadMemoryPcIncrement() |> Ldy
            rdy

        let ImmUnsupported () =
            if rdy then
                ReadMemoryPcIncrement |> ignore
            rdy

        let IdxIndStage3 () =
            if rdy then
                ReadMemory opcode2 |> ignore
                aluTemp <- (opcode2 + x) &&& 0xFF
            rdy

        let IdxIndStage4 () =
            if rdy then
                ea <- ReadMemory aluTemp
            rdy

        let IdxIndStage5 () =
            if rdy then
                ea <- ea ||| (ReadMemory(aluTemp + 1) <<< 8)
            rdy

        let IdxIndReadStage6 operation =
            IfReady <| (ReadMemory ea |> operation)

        let IdxIndReadStage6Lda () = IdxIndReadStage6 <| Lda
        let IdxIndReadStage6Ora () = IdxIndReadStage6 <| Ora
        let IdxIndReadStage6Lax () = IdxIndReadStage6 <| Lax
        let IdxIndReadStage6Cmp () = IdxIndReadStage6 <| Cmp a
        let IdxIndReadStage6Adc () = IdxIndReadStage6 <| Adc
        let IdxIndReadStage6And () = IdxIndReadStage6 <| And
        let IdxIndReadStage6Eor () = IdxIndReadStage6 <| Eor
        let IdxIndReadStage6Sbc () = IdxIndReadStage6 <| Sbc

        let IdxIndWriteStage6 value =
            value |> WriteMemory ea |> ignore
            true

        let IdxIndWriteStage6Sta () = IdxIndWriteStage6 <| a
        let IdxIndWriteStage6Sax () = IdxIndWriteStage6 <| (a &&& x)

        let IdxIndRmwStage6 () =
            IfReady <| aluTemp <- ReadMemory ea

        let IdxIndRmwStage7 operation =
            aluTemp |> WriteMemory ea |> operation |> ignore
            true

        let IdxIndRmwStage7Slo () = IdxIndRmwStage7 <| Slo
        let IdxIndRmwStage7Sre () = IdxIndRmwStage7 <| Sre
        let IdxIndRmwStage7Rra () = IdxIndRmwStage7 <| Rra
        let IdxIndRmwStage7Isc () = IdxIndRmwStage7 <| Isc

        let IdxIndRmwStage8 () =
            aluTemp |> WriteMemory ea |> ignore
            true

        let PushP () =
            b <- true
            GetP() |> Push
            true

        let PushA () =
            Push a
            true

        let PullPNoInc () =
            if rdy then
                myIFlag <- i
                ReadMemoryS() |> SetP
                iFlagPending <- i
                i <- myIFlag
            rdy

        let ImpAslA () =
            if rdy then
                FetchDummy()
                a <- Asl a
            rdy

        let ImpRolA () =
            if rdy then
                FetchDummy()
                a <- Rol a
            rdy

        let ImpRorA () =
            if rdy then
                FetchDummy()
                a <- Ror a
            rdy

        let ImpLsrA () =
            if rdy then
                FetchDummy()
                a <- Lsr a
            rdy

        let JmpAbs () =
            if rdy then
                pc <- ((ReadMemory pc) <<< 8) ||| opcode2
            rdy
            
        let IncPc () =
            if rdy then
                FetchDummy()
                pc <- pc + 1
            rdy

        let ZpRmwStage3 () =
            IfReady <| aluTemp <- ReadMemory opcode2

        let ZpRmwStage5 () =
            aluTemp |> WriteMemory opcode2 |> ignore
            true

        let ZpRmw operation =
            aluTemp |> WriteMemory opcode2 |> operation |> ignore
            true

        let ZpRmwInc () = ZpRmw Inc
        let ZpRmwDec () = ZpRmw Dec
        let ZpRmwAsl () = ZpRmw Asl
        let ZpRmwSre () = ZpRmw Sre
        let ZpRmwRra () = ZpRmw Rra
        let ZpRmwDcp () = ZpRmw Dcp
        let ZpRmwLsr () = ZpRmw Lsr
        let ZpRmwRor () = ZpRmw Ror
        let ZpRmwRol () = ZpRmw Rol
        let ZpRmwSlo () = ZpRmw Slo
        let ZpRmwIsc () = ZpRmw Isc
        let ZpRmwRla () = ZpRmw Rla
            

            


            

        




            










        ()

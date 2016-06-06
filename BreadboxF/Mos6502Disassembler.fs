namespace Breadbox

type Mos6502Disassembler (memory:IMemory) =
    let getOpcode pc =
        let acc opcode = System.String.Format("{0} A", opcode), 1
        let imp opcode = opcode, 1
        let imm opcode = System.String.Format("{0} #${1:x2}", opcode, (memory.Peek(pc + 1))), 2
        let zp opcode = System.String.Format("{0} ${1:x2}", opcode, (memory.Peek(pc + 1))), 2
        let zpx opcode = System.String.Format("{0} ${1:x2},X", opcode, (memory.Peek(pc + 1))), 2
        let zpy opcode = System.String.Format("{0} ${1:x2},Y", opcode, (memory.Peek(pc + 1))), 2
        let izx opcode = System.String.Format("{0} (${1:x2},X)", opcode, (memory.Peek(pc + 1))), 2
        let izy opcode = System.String.Format("{0} (${1:x2}),Y", opcode, (memory.Peek(pc + 1))), 2
        let abs opcode = System.String.Format("{0} ${2:x2}{1:x2}", opcode, (memory.Peek(pc + 1)), (memory.Peek(pc + 2))), 3
        let abx opcode = System.String.Format("{0} ${2:x2}{1:x2},X", opcode, (memory.Peek(pc + 1)), (memory.Peek(pc + 2))), 3
        let aby opcode = System.String.Format("{0} ${2:x2}{1:x2},Y", opcode, (memory.Peek(pc + 1)), (memory.Peek(pc + 2))), 3
        let ind opcode = System.String.Format("{0} (${2:x2}{1:x2})", opcode, (memory.Peek(pc + 1)), (memory.Peek(pc + 2))), 3
        let rel opcode =
            let offset = (memory.Peek(pc + 1))
            System.String.Format("{0} ${1:x4}", opcode, (pc + 2 - (if offset < 0x80 then 0 else 256))), 2
        let jam = "JAM", 1

        let src = memory.Peek(pc)
        let group = src &&& 0x3
        let addressing = (src >>> 2) &&& 0x7
        let operation = (src >>> 5) &&& 0x7

        match src with
            | 0x00 -> imp "BRK"
            | 0x01 -> izx "ORA"
            | 0x02 -> jam
            | 0x03 -> izx "SLO"
            | 0x04 -> zp "NOP"
            | 0x05 -> zp "ORA"
            | 0x06 -> zp "ASL"
            | 0x07 -> zp "SLO"
            | 0x08 -> imp "PHP"
            | 0x09 -> imm "ORA"
            | 0x0A -> imp "ASL"
            | 0x0B -> imm "ANC"
            | 0x0C -> abs "NOP"
            | 0x0D -> abs "ORA"
            | 0x0E -> abs "ASL"
            | 0x0F -> abs "SLO"

            | 0x10 -> rel "BPL"
            | 0x11 -> izy "ORA"
            | 0x12 -> jam
            | 0x13 -> izy "SLO"
            | 0x14 -> zpx "NOP"
            | 0x15 -> zpx "ORA"
            | 0x16 -> zpx "ASL"
            | 0x17 -> zpx "SLO"
            | 0x18 -> imp "CLC"
            | 0x19 -> aby "ORA"
            | 0x1A -> imp "NOP"
            | 0x1B -> aby "SLO"
            | 0x1C -> abx "NOP"
            | 0x1D -> abx "ORA"
            | 0x1E -> abx "ASL"
            | 0x1F -> abx "SLO"

            | 0x20 -> abs "JSR"
            | 0x21 -> izx "AND"
            | 0x22 -> jam
            | 0x23 -> izx "RLA"
            | 0x24 -> zp "BIT"
            | 0x25 -> zp "AND"
            | 0x26 -> zp "ROL"
            | 0x27 -> zp "RLA"
            | 0x28 -> imp "PLP"
            | 0x29 -> imm "AND"
            | 0x2A -> imp "ROL"
            | 0x2B -> imm "ANC"
            | 0x2C -> abs "BIT"
            | 0x2D -> abs "AND"
            | 0x2E -> abs "ROL"
            | 0x2F -> abs "RLA"

            | 0x30 -> rel "BMI"
            | 0x31 -> izy "AND"
            | 0x32 -> jam
            | 0x33 -> izy "RLA"
            | 0x34 -> zpx "NOP"
            | 0x35 -> zpx "AND"
            | 0x36 -> zpx "ROL"
            | 0x37 -> zpx "RLA"
            | 0x38 -> imp "SEC"
            | 0x39 -> aby "AND"
            | 0x3A -> imp "NOP"
            | 0x3B -> aby "RLA"
            | 0x3C -> abx "NOP"
            | 0x3D -> abx "AND"
            | 0x3E -> abx "ROL"
            | 0x3F -> abx "RLA"

            | 0x40 -> imp "RTI"
            | 0x41 -> izx "EOR"
            | 0x42 -> jam
            | 0x43 -> izx "SRE"
            | 0x44 -> zp "NOP"
            | 0x45 -> zp "EOR"
            | 0x46 -> zp "LSR"
            | 0x47 -> zp "SRE"
            | 0x48 -> imp "PHA"
            | 0x49 -> imm "EOR"
            | 0x4A -> imp "LSR"
            | 0x4B -> imm "ALR"
            | 0x4C -> abs "JMP"
            | 0x4D -> abs "EOR"
            | 0x4E -> abs "LSR"
            | 0x4F -> abs "SRE"

            | 0x50 -> rel "BVC"
            | 0x51 -> izy "EOR"
            | 0x52 -> jam
            | 0x53 -> izy "SRE"
            | 0x54 -> zpx "NOP"
            | 0x55 -> zpx "EOR"
            | 0x56 -> zpx "LSR"
            | 0x57 -> zpx "SRE"
            | 0x58 -> imp "CLI"
            | 0x59 -> aby "EOR"
            | 0x5A -> imp "NOP"
            | 0x5B -> aby "SRE"
            | 0x5C -> abx "NOP"
            | 0x5D -> abx "EOR"
            | 0x5E -> abx "LSR"
            | 0x5F -> abx "SRE"

            | 0x60 -> imp "RTS"
            | 0x61 -> izx "ADC"
            | 0x62 -> jam
            | 0x63 -> izx "RRA"
            | 0x64 -> zp "NOP"
            | 0x65 -> zp "ADC"
            | 0x66 -> zp "ROR"
            | 0x67 -> zp "RRA"
            | 0x68 -> imp "PLA"
            | 0x69 -> imm "ADC"
            | 0x6A -> imp "ROR"
            | 0x6B -> imm "ARR"
            | 0x6C -> ind "JMP"
            | 0x6D -> abs "ADC"
            | 0x6E -> abs "ROR"
            | 0x6F -> abs "RRA"

            | 0x70 -> rel "BVS"
            | 0x71 -> izy "ADC"
            | 0x72 -> jam
            | 0x73 -> izy "RRA"
            | 0x74 -> zpx "NOP"
            | 0x75 -> zpx "ADC"
            | 0x76 -> zpx "ROR"
            | 0x77 -> zpx "RRA"
            | 0x78 -> imp "SEI"
            | 0x79 -> aby "ADC"
            | 0x7A -> imp "NOP"
            | 0x7B -> aby "RRA"
            | 0x7C -> abx "NOP"
            | 0x7D -> abx "ADC"
            | 0x7E -> abx "ROR"
            | 0x7F -> abx "RRA"

            | 0x80 -> imm "NOP"
            | 0x81 -> izx "STA"
            | 0x82 -> imm "NOP"
            | 0x83 -> izx "SAX"
            | 0x84 -> zp "STY"
            | 0x85 -> zp "STA"
            | 0x86 -> zp "STX"
            | 0x87 -> zp "SAX"
            | 0x88 -> imp "DEY"
            | 0x89 -> imm "NOP"
            | 0x8A -> imp "TXA"
            | 0x8B -> imm "XAA"
            | 0x8C -> abs "STY"
            | 0x8D -> abs "STA"
            | 0x8E -> abs "STX"
            | 0x8F -> abs "SAX"

            | 0x90 -> rel "BCC"
            | 0x91 -> izy "STA"
            | 0x92 -> jam
            | 0x93 -> izy "AHX"
            | 0x94 -> zpx "STY"
            | 0x95 -> zpx "STA"
            | 0x96 -> zpy "STX"
            | 0x97 -> zpy "SAX"
            | 0x98 -> imp "TYA"
            | 0x99 -> aby "STA"
            | 0x9A -> imp "TXS"
            | 0x9B -> aby "TAS"
            | 0x9C -> abx "SHY"
            | 0x9D -> abx "STA"
            | 0x9E -> aby "SHX"
            | 0x9F -> aby "AHX"



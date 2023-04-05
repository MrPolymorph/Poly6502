using System;
using System.Collections.Generic;
using Poly6502.Microprocessor.Attributes;
using Poly6502.Microprocessor.Flags;
using Poly6502.Microprocessor.Models;
using Poly6502.Microprocessor.Utilities;
using Poly6502.Microprocessor.Utilities.Arguments;

namespace Poly6502.Microprocessor
{
    /// <summary>
    /// The 6502 is a little-endian 8-bit processor with a 16-bit address bus.
    ///51187
    /// The 6502 typically runs at 1 to 2Mhz
    /// </summary>
    public class M6502 : AbstractAddressDataBus
    {
        private ushort _offset;

        /// <summary>
        /// 8-bit A register
        /// </summary>
        private byte _a;

        /// <summary>
        /// 8-bit X register
        /// </summary>
        private byte _x;

        /// <summary>
        /// 8-bit Y register
        /// </summary>
        private byte _y;

        /// <summary>
        /// 8-bit stack pointer
        /// </summary>
        private byte _sp;

        /// <summary>
        /// operand byte
        /// </summary>
        private byte _operand;

        /// <summary>
        /// current instruction cycles
        /// </summary>
        private int _instructionCycles;

        /// <summary>
        /// current addressing mode cycles.
        /// </summary>
        private int _addressingModeCycles;

        /// <summary>
        /// 8-bit status register
        /// </summary>
        private StatusRegister _p;

        /// <summary>
        /// Current op 
        /// </summary>
        private Operation _instructionRegister;

        /// <summary>
        /// Set if the microprocessor is currently fetching the instruction to run.
        /// </summary>
        public bool FetchInstruction { get; private set; }

        public event EventHandler ClockComplete;
        public event EventHandler OpComplete;
        public event EventHandler FetchComplete;
        public event ProcessorEventHandler ProcessorStateChange;

        public delegate void ProcessorEventHandler(object? sender, ProcessorStateChangedEventArgs args);

        /// <summary>
        /// Op Code lookup table.
        /// </summary>
        public Dictionary<byte, Operation> OpCodeLookupTable { get; private set; }


        /// <summary>
        /// 8-Bit Accumulator
        /// </summary>
        public byte A
        {
            get => _a;
            private set
            {
                ProcessorStateChange?.Invoke(this, new ProcessorStateChangedEventArgs(StateChangeType.ARegister,
                    _a, value));

                _a = value;
            }
        }

        /// <summary>
        /// 8-Bit X Register
        /// </summary>
        public byte X
        {
            get => _x;
            private set
            {
                ProcessorStateChange?.Invoke(this, new ProcessorStateChangedEventArgs(StateChangeType.XRegister,
                    _x, value));

                _x = value;
            }
        }

        /// <summary>
        /// 8-Bit Y Register
        /// </summary>
        public byte Y
        {
            get => _y;
            private set
            {
                ProcessorStateChange?.Invoke(this, new ProcessorStateChangedEventArgs(StateChangeType.YRegister,
                    _y, value));

                _y = value;
            }
        }

        /// <summary>
        /// 8-Bit Stack Pointer
        /// </summary>
        public byte SP
        {
            get => _sp;
            private set
            {
                ProcessorStateChange?.Invoke(this, new ProcessorStateChangedEventArgs(StateChangeType.StackPointer,
                    _sp, value));

                _sp = value;
            }
        }

        /// <summary>
        /// 16-Bit Program Counter
        /// </summary>
        public ushort Pc { get; set; }

        /// <summary>
        /// The current instruction Lo Byte 
        /// </summary>
        public byte InstructionLoByte { get; set; }

        /// <summary>
        /// The current instruction Hi Byte
        /// </summary>
        public byte InstructionHiByte { get; set; }


        /// <summary>
        /// 8-Bit Status Register / Processor Flags
        /// </summary>
        public StatusRegister P
        {
            get => _p;
            private set
            {
                ProcessorStateChange?.Invoke(this, new ProcessorStateChangedEventArgs(StateChangeType.StatusRegister,
                    _p, value));

                _p = value;
            }
        }

        /// <summary>
        /// The operation function currently being run.
        /// </summary>
        public Operation InstructionRegister
        {
            get => _instructionRegister;
            private set
            {
                ProcessorStateChange?.Invoke(this, new ProcessorStateChangedEventArgs(StateChangeType.Op,
                    _instructionRegister, value));

                _instructionRegister = value;
            }
        }

        /// <summary>
        /// The op code currently being run.
        /// </summary>
        public byte OpCode { get; private set; }

        /// <summary>
        /// Voltage from the input pin.
        /// </summary>
        public float InputVoltage { get; private set; }

        /// <summary>
        /// ???
        /// </summary>
        public ushort TempAddress { get; private set; }

        /// <summary>
        /// True if an instruction is being executed.
        /// </summary>
        public bool OpCodeInProgress { get; private set; }

        /// <summary>
        /// True if the addressing mode is being processed.
        /// </summary>
        public bool AddressingModeInProgress { get; private set; }

        /// <summary>
        /// +1 is because a fetch is always 1 cycle.
        /// </summary>
        public int CurrentTotalCyclesTaken => _instructionCycles + _addressingModeCycles + 1;

        /// <summary>
        /// constructor.
        /// </summary>
        public M6502()
        {
            IgnorePropagation(true);
            P = new StatusRegister();

            OpCodeLookupTable = new Dictionary<byte, Operation>()
            {
                /* 0 Row */
                { 0x00, new Operation(BRK, IMP, 1, 7) },
                { 0x01, new Operation(ORA, IZX, 2, 6) },
                { 0x05, new Operation(ORA, ZPA, 2, 3) },
                { 0x06, new Operation(ASL, ZPA, 2, 5) },
                { 0x08, new Operation(PHP, IMP, 1, 3) },
                { 0x09, new Operation(ORA, IMM, 2, 2) },
                { 0x0A, new Operation(ASL, ACC, 1, 2) },
                { 0x0D, new Operation(ORA, ABS, 3, 4) },
                { 0x0E, new Operation(ASL, ABS, 3, 6) },

                /* 1 Row */
                { 0x10, new Operation(BPL, REL, 2, 2) },
                { 0x11, new Operation(ORA, IZY, 2, 5) },
                { 0x15, new Operation(ORA, ZPX, 2, 4) },
                { 0x16, new Operation(ASL, ZPX, 2, 6) },
                { 0x18, new Operation(CLC, IMP, 1, 2) },
                { 0x19, new Operation(ORA, ABY, 3, 4) },
                { 0x1D, new Operation(ORA, ABX, 3, 4) },
                { 0x1E, new Operation(ASL, ABX, 3, 7) },

                /* 2 Row */
                { 0x20, new Operation(JSR, ABS, 3, 6) },
                { 0x21, new Operation(AND, IZX, 2, 6) },
                { 0x24, new Operation(BIT, ZPA, 2, 3) },
                { 0x25, new Operation(AND, ZPA, 2, 3) },
                { 0x26, new Operation(ROL, ZPA, 2, 5) },
                { 0x28, new Operation(PLP, IMP, 1, 4) },
                { 0x29, new Operation(AND, IMM, 2, 2) },
                { 0x2A, new Operation(ROL, ACC, 1, 2) },
                { 0x2C, new Operation(BIT, ABS, 3, 4) },
                { 0x2D, new Operation(AND, ABS, 3, 4) },
                { 0x2E, new Operation(ROL, ABS, 3, 6) },

                /* 3 Row */
                { 0x30, new Operation(BMI, REL, 2, 2) },
                { 0x31, new Operation(AND, IZY, 2, 5) },
                { 0x35, new Operation(AND, ZPX, 2, 4) },
                { 0x36, new Operation(ROL, ZPX, 2, 6) },
                { 0x38, new Operation(SEC, IMP, 1, 2) },
                { 0x39, new Operation(AND, ABY, 3, 4) },
                { 0x3D, new Operation(AND, ABX, 3, 4) },
                { 0x3E, new Operation(ROL, ABX, 3, 7) },

                /* 4 Row */
                { 0x40, new Operation(RTI, IMP, 1, 6) },
                { 0x41, new Operation(EOR, IZX, 2, 6) },
                { 0x45, new Operation(EOR, ZPA, 2, 3) },
                { 0x46, new Operation(LSR, ZPA, 2, 5) },
                { 0x48, new Operation(PHA, IMP, 1, 3) },
                { 0x49, new Operation(EOR, IMM, 2, 2) },
                { 0x4A, new Operation(LSR, ACC, 1, 2) },
                { 0x4C, new Operation(JMP, ABS, 3, 3) },
                { 0x4D, new Operation(EOR, ABS, 3, 4) },
                { 0x4E, new Operation(LSR, ABS, 3, 6) },

                /* 5 Row */
                { 0x50, new Operation(BVC, REL, 2, 2) },
                { 0x51, new Operation(EOR, IZY, 2, 5) },
                { 0x55, new Operation(EOR, ZPX, 2, 4) },
                { 0x56, new Operation(LSR, ZPX, 2, 6) },
                { 0x58, new Operation(CLI, IMP, 1, 2) },
                { 0x59, new Operation(EOR, ABY, 3, 4) },
                { 0x5D, new Operation(EOR, ABX, 3, 4) },
                { 0x5E, new Operation(LSR, ABX, 3, 7) },

                /* 6 Row */
                { 0x60, new Operation(RTS, IMP, 1, 6) },
                { 0x61, new Operation(ADC, IZX, 2, 6) },
                { 0x65, new Operation(ADC, ZPA, 2, 3) },
                { 0x66, new Operation(ROR, ZPA, 2, 5) },
                { 0x68, new Operation(PLA, IMP, 1, 4) },
                { 0x69, new Operation(ADC, IMM, 2, 2) },
                { 0x6A, new Operation(ROR, ACC, 1, 2) },
                { 0x6C, new Operation(JMP, IND, 3, 5) },
                { 0x6D, new Operation(ADC, ABS, 3, 4) },
                { 0x6E, new Operation(ROR, ABS, 3, 6) },

                /* 7 Row */
                { 0x70, new Operation(BVS, REL, 2, 2) },
                { 0x71, new Operation(ADC, IZY, 2, 5) },
                { 0x75, new Operation(ADC, ZPX, 2, 4) },
                { 0x76, new Operation(ROR, ZPX, 2, 6) },
                { 0x78, new Operation(SEI, IMP, 1, 2) },
                { 0x79, new Operation(ADC, ABY, 3, 4) },
                { 0x7D, new Operation(ADC, ABX, 3, 4) },
                { 0x7E, new Operation(ROR, ABX, 3, 7) },

                /* 8 Row */
                { 0x81, new Operation(STA, IZX, 2, 6) },
                { 0x84, new Operation(STY, ZPA, 2, 3) },
                { 0x85, new Operation(STA, ZPA, 2, 3) },
                { 0x86, new Operation(STX, ZPA, 2, 3) },
                { 0x88, new Operation(DEY, IMP, 1, 2) },
                { 0x8A, new Operation(TXA, IMP, 1, 2) },
                { 0x8C, new Operation(STY, ABS, 3, 4) },
                { 0x8D, new Operation(STA, ABS, 3, 4) },
                { 0x8E, new Operation(STX, ABS, 3, 4) },

                /* 9 Row */
                { 0x90, new Operation(BCC, REL, 2, 2) },
                { 0x91, new Operation(STA, IZY, 2, 6) },
                { 0x94, new Operation(STY, ZPX, 2, 4) },
                { 0x95, new Operation(STA, ZPX, 2, 4) },
                { 0x96, new Operation(STX, ZPY, 2, 4) },
                { 0x98, new Operation(TYA, IMP, 1, 2) },
                { 0x99, new Operation(STA, ABY, 3, 5) },
                { 0x9A, new Operation(TXS, IMP, 1, 2) },
                { 0x9D, new Operation(STA, ABX, 3, 5) },

                /* A Row */
                { 0xA0, new Operation(LDY, IMM, 2, 2) },
                { 0xA1, new Operation(LDA, IZX, 2, 6) },
                { 0xA2, new Operation(LDX, IMM, 2, 2) },
                { 0xA4, new Operation(LDY, ZPA, 2, 3) },
                { 0xA5, new Operation(LDA, ZPA, 2, 3) },
                { 0xA6, new Operation(LDX, ZPA, 2, 3) },
                { 0xA8, new Operation(TAY, IMP, 1, 2) },
                { 0xA9, new Operation(LDA, IMM, 2, 2) },
                { 0xAA, new Operation(TAX, IMP, 1, 2) },
                { 0xAC, new Operation(LDY, ABS, 3, 4) },
                { 0xAD, new Operation(LDA, ABS, 3, 4) },
                { 0xAE, new Operation(LDX, ABS, 3, 4) },

                /* B Row */
                { 0xB0, new Operation(BCS, REL, 2, 2) },
                { 0xB1, new Operation(LDA, IZY, 2, 5) },
                { 0xB4, new Operation(LDY, ZPX, 2, 4) },
                { 0xB5, new Operation(LDA, ZPX, 2, 4) },
                { 0xB6, new Operation(LDX, ZPY, 2, 4) },
                { 0xB8, new Operation(CLV, IMP, 1, 2) },
                { 0xB9, new Operation(LDA, ABY, 3, 4) },
                { 0xBA, new Operation(TSX, IMP, 1, 2) },
                { 0xBC, new Operation(LDY, ABX, 3, 4) },
                { 0xBD, new Operation(LDA, ABX, 3, 4) },
                { 0xBE, new Operation(LDX, ABY, 3, 4) },

                /* C Row */
                { 0xC0, new Operation(CPY, IMM, 2, 2) },
                { 0xC1, new Operation(CMP, IZX, 2, 6) },
                { 0xC4, new Operation(CPY, ZPA, 2, 3) },
                { 0xC5, new Operation(CMP, ZPA, 2, 3) },
                { 0xC6, new Operation(DEC, ZPA, 2, 5) },
                { 0xC8, new Operation(INY, IMP, 1, 2) },
                { 0xC9, new Operation(CMP, IMM, 2, 2) },
                { 0xCA, new Operation(DEX, IMP, 1, 2) },
                { 0xCC, new Operation(CPY, ABS, 3, 4) },
                { 0xCD, new Operation(CMP, ABS, 3, 4) },
                { 0xCE, new Operation(DEC, ABS, 3, 6) },

                /* D Row */
                { 0xD0, new Operation(BNE, REL, 2, 2) },
                { 0xD1, new Operation(CMP, IZY, 2, 5) },
                { 0xD5, new Operation(CMP, ZPX, 2, 4) },
                { 0xD6, new Operation(DEC, ZPX, 2, 6) },
                { 0xD8, new Operation(CLD, IMP, 1, 2) },
                { 0xD9, new Operation(CMP, ABY, 3, 4) },
                { 0xDD, new Operation(CMP, ABX, 3, 4) },
                { 0xDE, new Operation(DEC, ABX, 3, 7) },

                /* E Row */
                { 0xE0, new Operation(CPX, IMM, 2, 2) },
                { 0xE1, new Operation(SBC, IZX, 2, 6) },
                { 0xE4, new Operation(CPX, ZPA, 2, 3) },
                { 0xE5, new Operation(SBC, ZPA, 2, 3) },
                { 0xE6, new Operation(INC, ZPA, 2, 5) },
                { 0xE8, new Operation(INX, IMP, 1, 2) },
                { 0xE9, new Operation(SBC, IMM, 2, 2) },
                { 0xEA, new Operation(NOP, IMP, 1, 2) },
                { 0xEC, new Operation(CPX, ABS, 3, 4) },
                { 0xED, new Operation(SBC, ABS, 3, 4) },
                { 0xEE, new Operation(INC, ABS, 3, 6) },

                /* F Row */
                { 0xF0, new Operation(BEQ, REL, 2, 2) },
                { 0xF1, new Operation(SBC, IZY, 2, 5) },
                { 0xF5, new Operation(SBC, ZPX, 2, 4) },
                { 0xF6, new Operation(INC, ZPX, 2, 6) },
                { 0xF8, new Operation(SED, IMP, 1, 2) },
                { 0xF9, new Operation(SBC, ABY, 3, 4) },
                { 0xFD, new Operation(SBC, ABX, 3, 4) },
                { 0xFE, new Operation(INC, ABX, 3, 7) },

                /* NOP Illegal OpCodes */
                { 0x1A, new Operation(NOP, IMP, 1, 2) },
                { 0x3A, new Operation(NOP, IMP, 1, 2) },
                { 0x5A, new Operation(NOP, IMP, 1, 2) },
                { 0x7A, new Operation(NOP, IMP, 1, 2) },
                { 0xDA, new Operation(NOP, IMP, 1, 2) },
                { 0xFA, new Operation(NOP, IMP, 1, 2) },
                { 0x80, new Operation(NOP, IMM, 2, 2) },
                { 0x82, new Operation(NOP, IMM, 2, 2) },
                { 0x89, new Operation(NOP, IMM, 2, 2) },
                { 0xC2, new Operation(NOP, IMM, 2, 2) },
                { 0xE2, new Operation(NOP, IMM, 2, 2) },
                { 0x04, new Operation(NOP, ZPA, 2, 3) },
                { 0x44, new Operation(NOP, ZPA, 2, 3) },
                { 0x64, new Operation(NOP, ZPA, 2, 3) },
                { 0x14, new Operation(NOP, ZPX, 2, 4) },
                { 0x34, new Operation(NOP, ZPX, 2, 4) },
                { 0x54, new Operation(NOP, ZPX, 2, 4) },
                { 0x74, new Operation(NOP, ZPX, 2, 4) },
                { 0xD4, new Operation(NOP, ZPX, 2, 4) },
                { 0xF4, new Operation(NOP, ZPX, 2, 4) },
                { 0x0C, new Operation(NOP, ABS, 3, 4) },
                { 0x1C, new Operation(NOP, ABX, 3, 4) },
                { 0x3C, new Operation(NOP, ABX, 3, 4) },
                { 0x5C, new Operation(NOP, ABX, 3, 4) },
                { 0x7C, new Operation(NOP, ABX, 3, 4) },
                { 0xDC, new Operation(NOP, ABX, 3, 4) },
                { 0xFC, new Operation(NOP, ABX, 3, 4) },

                /* LAX Illegal OpCodes */
                { 0xA7, new Operation(LAX, ZPA, 2, 3) },
                { 0xB7, new Operation(LAX, ZPY, 2, 4) },
                { 0xAF, new Operation(LAX, ABS, 3, 4) },
                { 0xBF, new Operation(LAX, ABY, 3, 4) },
                { 0xA3, new Operation(LAX, IZX, 2, 6) },
                { 0xB3, new Operation(LAX, IZY, 2, 5) },

                /* SAX Illegal Opcodes */
                { 0x87, new Operation(SAX, ZPA, 2, 3) },
                { 0x97, new Operation(SAX, ZPY, 2, 4) },
                { 0x8F, new Operation(SAX, ABS, 3, 4) },
                { 0x83, new Operation(SAX, IZX, 2, 6) },

                /* USBC Illegal OpCodes */
                { 0xEB, new Operation(SBC, IMM, 2, 2) },

                /* DCP Illegal OpCodes */
                { 0xC7, new Operation(DCP, ZPA, 2, 5) },
                { 0xD7, new Operation(DCP, ZPX, 2, 6) },
                { 0xCF, new Operation(DCP, ABS, 3, 6) },
                { 0xDF, new Operation(DCP, ABX, 3, 7) },
                { 0xDB, new Operation(DCP, ABY, 3, 7) },
                { 0xC3, new Operation(DCP, IZX, 2, 8) },
                { 0xD3, new Operation(DCP, IZY, 2, 8) },

                /* ISC Illegal OpCodes */
                { 0xE7, new Operation(ISC, ZPA, 2, 5) },
                { 0xF7, new Operation(ISC, ZPX, 2, 6) },
                { 0xEF, new Operation(ISC, ABS, 3, 6) },
                { 0xFF, new Operation(ISC, ABX, 3, 7) },
                { 0xFB, new Operation(ISC, ABY, 3, 7) },
                { 0xE3, new Operation(ISC, IZX, 2, 8) },
                { 0xF3, new Operation(ISC, IZY, 2, 4) },

                /* SLO Illegal OpCodes */
                { 0x07, new Operation(SLO, ZPA, 2, 5) },
                { 0x17, new Operation(SLO, ZPX, 2, 6) },
                { 0x0F, new Operation(SLO, ABS, 3, 6) },
                { 0x1F, new Operation(SLO, ABX, 3, 7) },
                { 0x1B, new Operation(SLO, ABY, 3, 7) },
                { 0x03, new Operation(SLO, IZX, 2, 8) },
                { 0x13, new Operation(SLO, IZY, 2, 8) },

                /* RLA Illegal OpCodes */
                { 0x27, new Operation(RLA, ZPA, 2, 5) },
                { 0x37, new Operation(RLA, ZPX, 2, 6) },
                { 0x2F, new Operation(RLA, ABS, 3, 6) },
                { 0x3F, new Operation(RLA, ABX, 3, 7) },
                { 0x3B, new Operation(RLA, ABY, 3, 7) },
                { 0x23, new Operation(RLA, IZX, 2, 8) },
                { 0x33, new Operation(RLA, IZY, 2, 8) },

                /* SRE Illegal OpCodes */
                { 0x47, new Operation(SRE, ZPA, 2, 5) },
                { 0x57, new Operation(SRE, ZPX, 2, 6) },
                { 0x4F, new Operation(SRE, ABS, 3, 6) },
                { 0x5F, new Operation(SRE, ABX, 3, 7) },
                { 0x5B, new Operation(SRE, ABY, 3, 7) },
                { 0x43, new Operation(SRE, IZX, 2, 8) },
                { 0x53, new Operation(SRE, IZY, 2, 8) },

                /* RRA Illegal OpCodes */
                { 0x67, new Operation(RRA, ZPA, 2, 5) },
                { 0x77, new Operation(RRA, ZPX, 2, 6) },
                { 0x6F, new Operation(RRA, ABS, 3, 6) },
                { 0x7F, new Operation(RRA, ABX, 3, 7) },
                { 0x7B, new Operation(RRA, ABY, 3, 7) },
                { 0x63, new Operation(RRA, IZX, 2, 8) },
                { 0x73, new Operation(RRA, IZY, 2, 8) },

                /* JAM Illegal Opcodes */
                { 0x02, new Operation(JAM, IMP, 1, 2) },
                { 0x12, new Operation(JAM, IMP, 1, 2) },
                { 0x22, new Operation(JAM, IMP, 1, 2) },
                { 0x32, new Operation(JAM, IMP, 1, 2) },
                { 0x42, new Operation(JAM, IMP, 1, 2) },
                { 0x52, new Operation(JAM, IMP, 1, 2) },
                { 0x62, new Operation(JAM, IMP, 1, 2) },
                { 0x72, new Operation(JAM, IMP, 1, 2) },
                { 0x92, new Operation(JAM, IMP, 1, 2) },
                { 0xB2, new Operation(JAM, IMP, 1, 2) },
                { 0xD2, new Operation(JAM, IMP, 1, 2) },
                { 0xF2, new Operation(JAM, IMP, 1, 2) },

                /* ANC Illegal Opcodes */
                { 0x0B, new Operation(ANC, IMM, 2, 2) },
                { 0x2B, new Operation(ANC2, IMM, 2, 2) },

                /* ALR Illegal Opcodes */
                { 0x4B, new Operation(ALR, IMM, 2, 2) },

                /* ARR Illegal Opcodes */
                { 0x6B, new Operation(ARR, IMM, 2, 2) },

                /* ANE Illegal Opcodes*/
                { 0x8B, new Operation(ANE, IMM, 2, 2) },

                /* SHA Illegal Opcodes */
                { 0x9F, new Operation(SHA, ABY, 3, 5) },
                { 0x93, new Operation(SHA, IZY, 2, 6) },

                /* TAS Illegal Opcodes */
                { 0x9B, new Operation(TAS, ABY, 3, 5) },

                /* SHY Illegal Opcodes */
                { 0x9C, new Operation(SHY, ABX, 3, 5) },

                /* SHX Illegal Opcodes */
                { 0x9E, new Operation(SHX, ABY, 3, 5) },

                /* LXA Illegal Opcodes */
                { 0xAB, new Operation(LXA, IMM, 2, 2) },

                /* LAS Illegal Opcodes */
                { 0xBB, new Operation(LAS, ABY, 3, 4) },
            };

            AddressChanged += (sender, args) =>
            {
                if (args is AddressChangedEventArgs addressArgs)
                {
                    ProcessorStateChange?.Invoke(this, new ProcessorStateChangedEventArgs(
                        StateChangeType.ProgramCounter, addressArgs.OldAddress, addressArgs.NewAddress
                    ));
                }
            };

            RES();
        }


        #region Addressing Modes

        /// <summary>
        /// Accumulator Addressing
        ///
        /// This form of addressing is represented with a 1 byte instruction,
        /// implying an operation on the accumulator
        /// </summary>
        public void ACC()
        {
            _operand = A;
            AddressingModeInProgress = false;
        }

        /// <summary>
        /// Implied Addressing
        ///
        /// The data is implied as part of the op
        /// </summary>
        public void IMP()
        {
            ACC();
            AddressingModeInProgress = false;
        }

        /// <summary>
        /// Immediate Addressing
        ///
        /// In Immediate addressing, the operand
        /// is contained in the 2nd byte of the instruction, with no
        /// further memory addressing required.
        /// </summary>
        public void IMM()
        {
            _operand = Read(Pc);
            Pc++;
            AddressingModeInProgress = false;
        }

        /// <summary>
        /// Absolute Addressing
        ///
        /// In Absolute Addressing, the 2nd byte of the instruction specifies
        /// the eight low order bits of the effective address while the 3rd byte
        /// specifies the eight high order bits. Thus, the absolute addressing mode
        /// allows access to the entire 65K bytes of addressable memory
        /// 
        /// </summary>
        public void ABS()
        {
            switch (_addressingModeCycles)
            {
                case 0: //Cycle 1 perform a read, getting instruction lo.
                    AddressingModeInProgress = true;
                    InstructionLoByte = Read(Pc);
                    Pc++;
                    break;
                case 1: //Cycle 2 perform a read, getting instruction hi
                    InstructionHiByte = Read(Pc);
                    Pc++;
                    AddressBusAddress = (ushort)(InstructionHiByte << 8 | InstructionLoByte);
                    break;
                case 2:
                    _operand = Read(AddressBusAddress);
                    AddressingModeInProgress = false;
                    break;
            }
            
            _addressingModeCycles++;
        }

        /// <summary>
        /// Zero Page Addressing
        ///
        /// The Zero Page instructions allow for shorter
        /// code and execution times by only fetching the second
        /// byte of the instruction and assuming a 0 high address byte.
        ///
        /// Careful use of the Zero page can result in significant increase in
        /// code efficiency
        /// </summary>
        public void ZPA()
        {
            switch (_addressingModeCycles)
            {
                case 0:
                    AddressBusAddress = Read(Pc++);
                    break;
                case 1:
                    _operand = Read(AddressBusAddress);
                    AddressingModeInProgress = false;
                    break;
            }
            
            _addressingModeCycles++;
        }

        /// <summary>
        /// Zero Page Addressing with X offset.
        ///
        ///  Should take 4 cycles https://www.nesdev.org/wiki/CPU_addressing_modes
        /// </summary>
        public void ZPX()
        {
            switch (_addressingModeCycles)
            {
                case 0: //Cycle 1 Read 
                    AddressBusAddress = Read((ushort)(Pc + X));
                    Pc++;
                    AddressBusAddress &= 0xFF;
                    break;
                case 1:
                    _operand = Read(AddressBusAddress);
                    break;
                case 2:
                    AddressingModeInProgress = false;
                    break;
            }
            
            _addressingModeCycles++;
        }

        /// <summary>
        /// operand is zero page address; effective address is address incremented by Y without carry
        ///
        /// The available 16-bit address space is conceived as consisting of pages of 256 bytes each, with
        /// address hi-bytes representing the page index. An increment with carry may affect the hi-byte
        /// and may thus result in a crossing of page boundaries, adding an extra cycle to the execution.
         /// Increments without carry do not affect the hi-byte of an address and no page transitions do occur.
         /// Generally, increments of 16-bit addresses include a carry, increments of zeropage addresses don't.
         /// Notably this is not related in any way to the state of the carry bit of the accumulator.
         ///
         /// Should Take 4 Cycles.
         /// </summary>
         public void ZPY()
         {
             switch (_addressingModeCycles)
             {
                 case (0): //Cycle 1 Read Lo Byte
                     AddressBusAddress = (ushort)(Read(Pc) + Y);
                     AddressBusAddress &= 0xFF;
                     _operand = DataBusData;
                     Pc++;
                     AddressingModeInProgress = false;
                     break;
             }

            _addressingModeCycles++;
        }

        /// <summary>
        /// Index Absolute Addressing
        ///
        /// (X, Y indexing) — This form of addressing  is used  in  conjunction with
        /// X and Y index register and is  referred to as "Absolute, X,"
        /// and "Absolute, Y." The effective address  is formed  by adding the
        /// contents of X and Y to the address  contained  in  the second and third
        /// bytes of the instruction. This mode allows the index register to contain the
        /// index or count value and the instruction to contain  the base address.
        ///
        /// This type of indexing  allows any location  referencing and the index to
        /// modify multiple fields  resulting  in  reduced coding and execution time.
        ///
        /// Should take 4+ (boundary crossing) Cycles https://www.nesdev.org/wiki/CPU_addressing_modes
        /// </summary>
        public void ABY()
        {
            switch (_addressingModeCycles)
            {
                case 0: //set the address bus to get the lo byte of the address
                    InstructionLoByte = Read(Pc);
                    Pc++;
                    break;
                case 1:
                    InstructionHiByte = Read(Pc);
                    Pc++;
                    AddressBusAddress = (ushort)(InstructionHiByte << 8 | InstructionLoByte);
                    AddressBusAddress += Y;
                    break;
                case 2:
                    _operand = Read(AddressBusAddress);
                    
                    if (!BoundaryCrossed())
                    {
                        AddressingModeInProgress = false;
                    }
                    
                    break;
                case 3: //boundary crossing penalty
                    AddressingModeInProgress = false;
                    break;
            }
            
            _addressingModeCycles++;
        }

        /// <summary>
        /// Index Absolute Addressing
        ///
        /// (X, Y indexing) — This form of addressing  is used  in  conjunction with
        /// X and Y index register and is  referred to as "Absolute, X,"
        /// and "Absolute, Y." The effective address  is formed  by adding the
        /// contents of X and Y to the address  contained  in  the second and third
        /// bytes of the instruction. This mode allows the index register to contain the
        /// index or count value and the instruction to contain  the base address.
        ///
        /// This type of indexing  allows any location  referencing and the index to
        /// modify multiple fields  resulting  in  reduced coding and execution time.
        ///
        /// Should take 4+ (boundary crossing) cycles. https://www.nesdev.org/wiki/CPU_addressing_modes
        /// </summary>
        public void ABX()
        {
            switch (_addressingModeCycles)
            {
                case 0: //set the address bus to get the lo byte of the address
                    InstructionLoByte = Read(Pc);
                    Pc++;
                    break;
                case 1:
                    InstructionHiByte = Read(Pc);
                    Pc++;
                    AddressBusAddress = (ushort)((ushort)(InstructionHiByte << 8 | InstructionLoByte) + X);
                    break;
                case 2:
                    if (!BoundaryCrossed())
                    {
                        _operand = Read(AddressBusAddress);
                        AddressingModeInProgress = false;
                    }
                    
                    break;
                case 3: //boundary crossing penalty
                    AddressingModeInProgress = false;
                    break;
            }
            
            _addressingModeCycles++;
        }

        /// <summary>
        /// Relative Addressing
        ///
        /// Relative addressing  is  used only with  branch  instructions and establishes a
        /// destination for the conditional branch.The second byte of the instruction  becomes
        /// the operand which  is an “Offset"  added to the contents of the lower eight bits
        /// of the program counter when  the counter is set at the next instruction. T
        ///
        /// he range of the offset is —  128 to +  127 bytes from  the next instruction
        /// </summary>
        public void REL()
        {
            switch (_addressingModeCycles)
            {
                case 0:
                    RelativeAddress = Read(Pc);
                    Pc++;

                    if ((RelativeAddress & 0x80) != 0)
                    {
                        RelativeAddress |= 0xFF00;
                    }

                    AddressingModeInProgress = false;
                    break;
            }
        }

        /// <summary>
        /// Indexed Indirect Addressing
        ///
        /// n  indexed  indirect addressing (referred to as [Indirect, X]),
        /// the second byte of the instruction  is added to the contents of the X
        /// index register, discarding the carry. The result of this addition
        /// points to a memory location on  page zero whose contents is the low order eight
        /// bits of the effective address.
        ///
        /// The next memory location  in  page zero contains the high order eight bits of the
        /// effective address. Both memory locations specifying the  high and  low order
        /// bytes of the effective address  must be in  page zero.
        ///
        /// Should take 6 Cycles. https://www.nesdev.org/wiki/CPU_addressing_modes
        /// </summary>
        public void IZX()
        {
            switch (_addressingModeCycles)
            {
                case (0):
                {
                    _offset = Read(Pc);
                    Pc++;
                    break;
                }
                case (1):
                {
                    InstructionLoByte = Read((ushort)((_offset + (X)) & 0xFF));
                    break;
                }
                case (2):
                {
                    InstructionHiByte = Read((ushort)((_offset + X + 1) & 0xFF));
                    AddressBusAddress = (ushort)(InstructionHiByte << 8 | InstructionLoByte);
                    break;
                }
                case (3):
                {
                    _operand = Read(AddressBusAddress);
                    break;
                }
                case (4):
                {
                    AddressingModeInProgress = false;
                    break;
                }
            }

            _addressingModeCycles++;
        }

        /// <summary>
        /// Indirect Indexed Addressing
        ///
        /// n  indirect indexed addressing (referred to as [Indirect. Y]).
        /// the second byte of the instruction  points to a  memory location  in page zero.
        /// The contents of this  memory location  is added to the contents of the Y index
        /// register, the result being the low order eight bits of the effective address.
        ///
        /// The carry from this addition  is added  to the contents of the next page zero
        /// memory location, the result being the high order eight bits of the effective address.
        ///
        /// Should take 5+ Cycles
        /// </summary>
        public void IZY()
        {
            switch (_addressingModeCycles)
            {
                case (0):
                    TempAddress = Read(Pc);
                    Pc++;
                    break;
                
                case (1):
                    InstructionLoByte = Read((ushort)(TempAddress & 0xFF));
                    break;
                case 2:
                    InstructionHiByte = Read((ushort)((TempAddress + 1) & 0xFF));
                    
                    AddressBusAddress = (ushort)((ushort)(InstructionHiByte << 8 | InstructionLoByte) + Y);
                    
                    break;
                case 3: //boundary crossing penalty
                    _operand = Read(AddressBusAddress);
                    
                    if (!BoundaryCrossed())
                    {
                        AddressingModeInProgress = false;
                    }
                    break;
                case 4:
                    AddressingModeInProgress = false;
                    break;
            }

            _addressingModeCycles++;
        }


        /// <summary>
        /// Indirect Addressing
        ///
        /// This addressing mode reads from the address supplied after the opcode.
        /// this address is then read from to retrieve the opeand
        ///
        /// </summary>
        public void IND()
        {
            switch (_addressingModeCycles)
            {
                case 0:
                    InstructionLoByte = Read(Pc);
                    Pc++;
                    break;
                case 1:
                    InstructionHiByte = Read(Pc);
                    Pc++;
                    break;
                case 2:
                    AddressBusAddress = (ushort)(InstructionHiByte << 8 | InstructionLoByte);
                    break;
                case 3:
                    if (InstructionLoByte == 0x00FF)
                    {
                        ushort temp = (ushort) ((Read((ushort)(AddressBusAddress & 0xFF00)) << 8) 
                                              | Read((ushort)(AddressBusAddress + 0)));
                        
                        AddressBusAddress = temp;
                    }
                    else
                    {
                        ushort temp = (ushort) ((Read((ushort)(AddressBusAddress + 1)) << 8) 
                                                | Read((ushort)(AddressBusAddress + 0)));
                        
                        AddressBusAddress = temp;
                    }

                    AddressingModeInProgress = false;
                    break;
            }

            _addressingModeCycles++;
        }

        #endregion

        #region Instruction Set

        /// <summary>
        /// freeze the CPU.
        /// The processor will be trapped infinitely in
        /// T1 phase with $FF on the data bus.
        ///
        /// call to <see cref="RES"/> required.
        /// </summary>
        public void JAM()
        {
            OpCodeInProgress = true;
            AddressingModeInProgress = false;
        }

        /// <summary>
        /// Stores A AND X AND (high-byte of addr. + 1) at addr.
        /// unstable: sometimes 'AND (H+1)' is dropped, page boundary crossings may not work
        /// (with the high-byte of the value used as the high-byte of the address)
        ///
        /// </summary>
        [Unstable]
        public void SHA()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Puts A AND X in SP and stores A AND X AND (high-byte of addr. + 1) at addr.
        /// unstable: sometimes 'AND (H+1)' is dropped, page boundary crossings may not work
        /// (with the high-byte of the value used as the high-byte of the address)
        /// </summary>
        [Unstable]
        public void TAS()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// LDA/TSX oper
        /// </summary>
        public void LAS()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Store * AND oper in A and X
        /// Highly unstable, involves a 'magic' constant, <see cref="ANE"/>
        /// </summary>
        [HighlyUnstable]
        public void LXA()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stores X AND (high-byte of addr. + 1) at addr.
        /// unstable: sometimes 'AND (H+1)' is dropped, page boundary crossings may not work
        /// (with the high-byte of the value used as the high-byte of the address)
        /// </summary>
        [Unstable]
        public void SHX()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stores Y AND (high-byte of addr. + 1) at addr.
        /// unstable: sometimes 'AND (H+1)' is dropped, page boundary crossings may not work
        /// (with the high-byte of the value used as the high-byte of the address)
        /// </summary>
        [Unstable]
        public void SHY()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// AND operation + set C as ASL
        /// </summary>
        public void ANC()
        {
            BeginOpCode();

            switch (_instructionCycles)
            {
                case (0):
                {
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    A = (byte)(A & DataBusData);
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.C, P.HasFlag(StatusRegisterFlags.N));
                    AddressBusAddress++;
                    break;
                }
                case (2):
                {
                    EndOpCode();
                    break;
                }
            }
        }

        public void ANC2()
        {
            BeginOpCode();

            switch (_instructionCycles)
            {
                case (0):
                {
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    A = (byte)(A & DataBusData);
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.C, P.HasFlag(StatusRegisterFlags.N));
                    AddressBusAddress++;
                    break;
                }
                case (2):
                {
                    EndOpCode();
                    break;
                }
            }
        }


        /// <summary>
        /// AND (bitwise AND with accumulator)
        /// 
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// </remarks>
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, Y: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, Y: </term>
        ///         <description>5+ Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void AND()
        {
            A = (byte)(A & _operand);
            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
            
            EndOpCode();
        }

        /// <summary>
        /// ASL (Arithmetic Shift Left)
        ///
        /// ASL shifts all the bits left one position. 0 is shifted into bit 0
        /// and the original bit 7 is shifted into <see cref="StatusRegisterFlags.C"/>
        /// 
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// <see cref="StatusRegisterFlags.C"/>
        ///
        /// </remarks>
        /// <list type="bullet">
        ///     <item>
        ///         <term>Accumulator: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>5 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>7 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void ASL()
        {
            var data = _operand;
            int result = (data << 1);

            P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, (result & 0x00FF) == 0);
            P.SetFlag(StatusRegisterFlags.C, (result & 0xFF00) > 0);

            if (OpCode == 0x0A) //Hacky McHack
            {
                A = (byte)result;
            }
            else
            {
                switch (_instructionCycles)
                {
                    case 0:
                        UpdateRw(false);
                        SetData((byte)result); //this is an extra instruction cycle.
                        return;
                    case 1:
                        OutputDataToDatabus();
                        _instructionCycles++;
                        return;
                }
            }
            
            EndOpCode();
        }

        /// <summary>
        /// This operation involves the adder:
        /// V-flag is set according to (A AND oper) + oper
        /// The carry is not set, but bit 7 (sign) is exchanged with the carry
        /// </summary>
        public void ARR()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    A = (byte)(A & DataBusData);

                    var result = (P.HasFlag(StatusRegisterFlags.C) ? 1 : 0) << 7 | A >> 1;

                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, result == 0);
                    P.SetFlag(StatusRegisterFlags.C, (DataBusData & (1 << 5)) != 0);
                    P.SetFlag(StatusRegisterFlags.V, ((DataBusData & (1 << 5)) ^ ((DataBusData & (1 << 4)))) != 0);

                    if (OpCode == 0x6A)
                        A = (byte)result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte)result;
                        OutputDataToDatabus();
                    }

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// ANE - AND X + AND oper.
        /// 
        /// Highly unstable, do not use.
        /// A base value in A is determined based on the contents of A and a constant,
        /// which may be typically $00, $ff, $ee, etc. The value of this constant depends on
        /// temperature, the chip series, and maybe other factors, as well.
        /// In order to eliminate these uncertainties from the equation,
        /// use either 0 as the operand or a value of $FF in the accumulator.
        /// </summary>
        [HighlyUnstable]
        public void ANE()
        {
            throw new NotImplementedException();
        }

        public void ALR()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    A = (byte)(A & DataBusData);
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    AddressBusAddress++;
                    _instructionCycles++;
                    break;
                }
                case (3):
                {
                    P.SetFlag(StatusRegisterFlags.C, (DataBusData & 0x01) != 0);
                    var result = DataBusData >> 1;
                    P.SetFlag(StatusRegisterFlags.Z, result == 0);
                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);

                    if (OpCode == 0x4A)
                        A = (byte)result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte)result;
                        OutputDataToDatabus();
                    }

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// ADC (ADd with Carry)
        /// 
        /// ADC results are depending on the settings of the <see cref="StatusRegisterFlags.D"/> flag.
        ///
        /// In decimal mode, addition is carried out on the assumption that the values involved are packed BCD
        /// (Binary Coded Decimal).
        ///
        /// There is no way to add without carry.
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.V"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// <see cref="StatusRegisterFlags.C"/>
        ///
        /// </remarks>
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, Y: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, Y: </term>
        ///         <description>5+ Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void ADC()
        {
            BeginOpCode();

            byte data = _operand;

            var result = A + data + (P.HasFlag(StatusRegisterFlags.C) ? 1 : 0);
            var overflow = (~(A ^ data) & (data ^ result) & 0x80) != 0;
            A = (byte)result;

            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.C, result > byte.MaxValue);
            P.SetFlag(StatusRegisterFlags.V, overflow);

            EndOpCode();
        }

        /// <summary>
        /// BCC - Branch on Carry Clear
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Relative Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void BCC()
        {
            BeginOpCode();

            if (!P.HasFlag(StatusRegisterFlags.C))
            {
                AddressBusAddress = (ushort) (Pc + DataBusData);
                Pc = AddressBusAddress;
                Read(AddressBusAddress);
            }

            AddressBusAddress++;

            EndOpCode();
        }

        /// <summary>
        /// BCS - Branch on Carry Set
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Relative Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void BCS()
        {
            BeginOpCode();

            if (P.HasFlag(StatusRegisterFlags.C))
            {
                Pc += DataBusData;
                Read(AddressBusAddress);
            }
            else
            {
                AddressBusAddress++;
            }

            EndOpCode();
        }

        /// <summary>
        /// BEQ - Branch on Result Zero
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Relative Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void BEQ()
        {
            BeginOpCode();

            if (P.HasFlag(StatusRegisterFlags.Z))
            {
                AddressBusAddress = (ushort) (Pc + DataBusData);
                Pc = AddressBusAddress;
                Read(AddressBusAddress);
            }

            AddressBusAddress++;


            EndOpCode();
        }

        /// <summary>
        /// BIT (test BITs).
        ///
        /// BIT sets the <see cref="StatusRegisterFlags.Z"/> flag as through the value
        /// in the address tested were <see cref="AND"/> with the accumulator.
        ///
        /// The <see cref="StatusRegisterFlags.N"/> and <see cref="StatusRegisterFlags.V"/> flags are
        /// set to match bits 7 and 6 respectively in the value stored at the tested address.
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.V"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// </remarks>
        /// <list type="bullet">
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void BIT()
        {
            BeginOpCode();
            Read(AddressBusAddress); //there a problem here???

            byte temp = (byte)(A & DataBusData);
            P.SetFlag(StatusRegisterFlags.Z, (temp & 0x00FF) == 0);
            P.SetFlag(StatusRegisterFlags.N, (DataBusData & (1 << 7)) != 0);
            P.SetFlag(StatusRegisterFlags.V, (DataBusData & (1 << 6)) != 0);

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// BMI - Branch on Result Minus
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Relative Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void BMI()
        {
            BeginOpCode();

            if (P.HasFlag(StatusRegisterFlags.N))
            {
                var temp = Pc + RelativeAddress;

                if ((temp & 0xFF00) != (Pc & 0xFF00)) ;
                    //this should cause an extra cycle?

                Pc = (ushort) temp;
            }
            
            EndOpCode();
        }

        /// <summary>
        /// BNE - Branch on Result not Zero
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Relative Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void BNE()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();

                    if (!P.HasFlag(StatusRegisterFlags.Z))
                    {
                        var absolute = AddressBusAddress + RelativeAddress;

                        if ((absolute & 0xFF00) != (AddressBusAddress & 0xFF00))
                        {
                            _instructionCycles++;
                        }
                        else
                        {
                            AddressBusAddress = (ushort)absolute;
                            AddressBusAddress++;
                            Pc = AddressBusAddress;
                            EndOpCode();
                        }
                    }
                    else
                    {
                        AddressBusAddress++;
                        EndOpCode();
                    }

                    break;
                }
                case (1):
                {
                    AddressBusAddress += InstructionLoByte;
                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// BPL - Branch on Result Plus
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Relative Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void BPL()
        {
            BeginOpCode();

            if (!P.HasFlag(StatusRegisterFlags.N))
            {
                AddressBusAddress = (ushort) (Pc + RelativeAddress);
                Pc = AddressBusAddress;
            }

            EndOpCode();
        }

        /// <Summary>
        /// BRK (BReaK)
        ///
        ///
        /// BRK causes a non-maskable interrupt and increments the program counter
        /// by 1.
        /// 
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.B"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>7 Cycles</description>
        ///     </item>
        /// </list>
        /// 
        /// </Summary>
        public void BRK()
        {
            switch (_instructionCycles)
            {
                case (0):
                    BeginOpCode();
                    UpdateRw(false);
                    AddressBusAddress = (ushort)(0x0100 + SP);
                    Read(AddressBusAddress);
                    DataBusData = (byte)((AddressBusAddress >> 8) & 0x00FF);
                    SP--;

                    //enable interrupt because we are in a software break.
                    P.SetFlag(StatusRegisterFlags.I, true);

                    _instructionCycles++;
                    break;
                case (1):
                    AddressBusAddress = (ushort)(0x0100 + SP);
                    Read(AddressBusAddress);
                    DataBusData = (byte)(AddressBusAddress & 0x00FF);
                    SP--;

                    _instructionCycles++;
                    break;
                case (2):
                    AddressBusAddress = (ushort)(0x0100 + SP);
                    Read(AddressBusAddress);
                    DataBusData = (byte)P.Register;
                    P.SetFlag(StatusRegisterFlags.B, true);
                    SP--;

                    _instructionCycles++;
                    break;
                case (3):
                    UpdateRw(true);
                    AddressBusAddress = 0xFFFE;
                    Read(AddressBusAddress);

                    _instructionCycles++;
                    break;
                case (4):
                    AddressBusAddress = 0x0000;
                    AddressBusAddress = DataBusData;
                    AddressBusAddress = 0xFFFF;
                    Read(AddressBusAddress);

                    _instructionCycles++;
                    break;
                case (5):
                    AddressBusAddress = (ushort)(DataBusData << 8);
                    P.SetFlag(StatusRegisterFlags.B, false);

                    _instructionCycles++;

                    EndOpCode();
                    break;
            }
        }

        /// <summary>
        /// BVC - Branch on Overflow Clear
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Relative Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void BVC()
        {
            BeginOpCode();

            if (!P.HasFlag(StatusRegisterFlags.V))
            {
                AddressBusAddress = (ushort) (Pc + DataBusData);
                Pc = AddressBusAddress;
            }
            
            Read(AddressBusAddress);
            EndOpCode();
        }

        /// <summary>
        /// BVS - Branch on Overflow Set
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Relative Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void BVS()
        {
            BeginOpCode();

            if (P.HasFlag(StatusRegisterFlags.V))
            {
                AddressBusAddress = (ushort) (Pc + DataBusData);
                Pc = AddressBusAddress;
            }

            AddressBusAddress++;

            Read(AddressBusAddress);
            EndOpCode();
        }

        /// <summary>
        /// CLC (CLear Carry)
        /// 
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.C"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void CLC()
        {
            BeginOpCode();
            P.SetFlag(StatusRegisterFlags.C, false);
            AddressBusAddress++;
            OpCodeInProgress = false;
            EndOpCode();
        }

        /// <summary>
        /// CLD (CLear Decimal)
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.D"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void CLD()
        {
            BeginOpCode();
            P.SetFlag(StatusRegisterFlags.D, false);
            AddressBusAddress++;
            Read(AddressBusAddress);
            EndOpCode();
        }

        /// <summary>
        /// CLI (CLear Interrupt)
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.I"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void CLI()
        {
            BeginOpCode();
            P.SetFlag(StatusRegisterFlags.I, false);
            EndOpCode();
        }

        /// <summary>
        /// CLV (CLear oVerflow)
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.V"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void CLV()
        {
            BeginOpCode();
            P.SetFlag(StatusRegisterFlags.V, false);
            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// CMP (CoMPare accumulator)
        ///
        /// Compare sets flags as if a subtraction had been carried out. If the value in the
        /// accumulator is >= the compared value, the <see cref="StatusRegisterFlags.C"/> flag will be set.
        ///
        /// The <see cref="StatusRegisterFlags.Z"/> and <see cref="StatusRegisterFlags.N"/> flags will be set based
        /// on equality or lack thereof and the sign of the accumulator.
        ///  
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// <see cref="StatusRegisterFlags.C"/>
        ///
        /// </remarks>
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, Y: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, Y: </term>
        ///         <description>5+ Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void CMP()
        {

            int comparison = (byte)(A - DataBusData);
            if (comparison is < 0 or > byte.MaxValue)
                comparison = 0;

            P.SetFlag(StatusRegisterFlags.C, A >= DataBusData);
            P.SetFlag(StatusRegisterFlags.Z, (comparison & 0x00FF) == 0);
            P.SetFlag(StatusRegisterFlags.N, (comparison & 0x0080) != 0);
            AddressBusAddress++;
            Read(AddressBusAddress);
            EndOpCode();
        }


        /// <summary>
        /// CPX (ComPare X register)
        ///
        /// The compare X register operation and flag results
        /// are identical to the equivalent <see cref="CMP"/> operations
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// <see cref="StatusRegisterFlags.C"/>
        ///
        /// </remarks>
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void CPX()
        {
            var result = X - DataBusData;
            P.SetFlag(StatusRegisterFlags.C, X >= DataBusData);
            P.SetFlag(StatusRegisterFlags.Z, (result & 0x00FF) == 0);
            P.SetFlag(StatusRegisterFlags.N, (result & 0x0080) != 0);

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// CPY (ComPare Y register)
        ///
        /// The compare Y register operation and flag results
        /// are identical to the equivalent <see cref="CMP"/> operations
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// <see cref="StatusRegisterFlags.C"/>
        ///
        /// </remarks>
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void CPY()
        {
            var result = Y - DataBusData;

            P.SetFlag(StatusRegisterFlags.Z, result == 0);
            P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.C, Y >= DataBusData);

            AddressBusAddress++;

            EndOpCode();
        }

        /// <summary>
        /// DEC (DECrement memory)
        ///
        /// The compare X register operation and flag results
        /// are identical to the equivalent <see cref="CMP"/> operations
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>5 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>7 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void DEC()
        {
            switch (_instructionCycles)
            {
                case 0:
                    var data = (byte)(DataBusData - 1);
                    DataBusData = data;
                    P.SetFlag(StatusRegisterFlags.N, (data & 0x0080) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, (data & 0x00FF) == 0);
                    UpdateRw(false);
                    _instructionCycles++;
                    break;
                case 1:
                    DataBusData = (byte)(DataBusData & 0x00FF);
                    OutputDataToDatabus();
                    _instructionCycles++;
                    break;
                case 2:
                    AddressBusAddress++;
                    EndOpCode();
                    break;
            }
        }

        /// <summary>
        /// DEX (DEcrement X)
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void DEX()
        {
            X--;
            P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, X == 0);

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// DEY (DEcrement Y)
        ///
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void DEY()
        {
            Y--;
            P.SetFlag(StatusRegisterFlags.N, (Y & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, Y == 0);

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// EOR (bitwise Exclusive OR)
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, Y: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, Y: </term>
        ///         <description>5+ Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void EOR()
        {
            A ^= DataBusData;

            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

            AddressBusAddress++;

            EndOpCode();
        }

        /// <summary>
        /// INC (INCrement memory)
        /// 
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>5 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>7 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void INC()
        {
            var data = (DataBusData + 1);
            P.SetFlag(StatusRegisterFlags.N, (data & 0x0080) != 0);
            P.SetFlag(StatusRegisterFlags.Z, (data & 0x00FF) == 0);

            UpdateRw(false);
            DataBusData = (byte)(data & 0x00FF);
            OutputDataToDatabus();

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// INX (INcrement X)
        ///
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void INX()
        {
            X++;
            P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, X == 0);

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// INY (INcrement Y)
        ///
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void INY()
        {
            Y++;

            P.SetFlag(StatusRegisterFlags.N, (Y & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, Y == 0);

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// JMP (JuMP)
        ///
        /// JMP transfers program execution to the following address (absolute) or to the
        /// location contained in the following address (indirect).
        ///
        /// Note that there is no carry associated with the indirect jump.
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect: </term>
        ///         <description>5 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void JMP()
        {
            BeginOpCode();
            Pc = AddressBusAddress;
            EndOpCode();
        }

        /// <summary>
        /// JSR (Jump to SubRoutine)
        /// 
        /// JSR pushes (address - 1) of the next operation on to the stack before
        /// transferring program control to the following address.
        ///
        /// Subroutines are normally terminated by the <see cref="RTS"/> opcode.
        /// <list type="bullet">
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void JSR()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();
                    _instructionCycles++;
                    break;
                }
                case (1): //store the hi byte
                    UpdateRw(false);
                    Pc--;
                    DataBusData = (byte) ((Pc >> 8) & 0x00FF);
                    OutputDataToDatabus((ushort)(0x0100 | SP));
                    SP--;
                    _instructionCycles++;
                    break;
                case (2): //store the lo byte
                    UpdateRw(false);
                    Pc--;
                    DataBusData = (byte) (Pc + 1 & 0x00FF);
                    OutputDataToDatabus((ushort)(0x0100 | SP));
                    SP--;
                    _instructionCycles++;
                    break;
                case (3):
                    Pc = AddressBusAddress;
                    Read(AddressBusAddress);
                    EndOpCode();
                    break;
            }
        }

        /// <summary>
        /// LDA (LoaD Accumulator)
        /// 
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term> 
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, Y: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, Y: </term>
        ///         <description>5+ Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void LDA()
        {
            BeginOpCode();
            
            A = _operand;
            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

            EndOpCode();
        }

        /// <summary>
        /// LDX (LoaD X register)
        /// 
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, Y: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, Y: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void LDX()
        {
            X = DataBusData;
            AddressBusAddress++;

            P.SetFlag(StatusRegisterFlags.Z, X == 0);
            P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);

            EndOpCode();
        }

        /// <summary>
        /// LDY (Load Y register)
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void LDY()
        {
            Y = DataBusData;
            AddressBusAddress++;

            P.SetFlag(StatusRegisterFlags.Z, Y == 0);
            P.SetFlag(StatusRegisterFlags.N, (Y & 0x80) != 0);

            EndOpCode();
        }

        /// <summary>
        /// LSR (Logical Shift Right)
        /// 
        /// LSR shifts all bits right 1 position.
        /// 0 is shifted into bit 7 and the original bit 0 is shifted
        /// into <see cref="StatusRegisterFlags.C"/>
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// <see cref="StatusRegisterFlags.C"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Accumulator: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>5 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>7 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void LSR()
        {
            P.SetFlag(StatusRegisterFlags.C, (DataBusData & 0x01) != 0);
            var result = DataBusData >> 1;
            P.SetFlag(StatusRegisterFlags.Z, result == 0);
            P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);

            if (OpCode == 0x4A)
                A = (byte)result;
            else
            {
                UpdateRw(false);
                DataBusData = (byte)result;
                OutputDataToDatabus();
            }

            AddressBusAddress++;
            
            EndOpCode();
        }


        /// <summary>
        /// NOP (No Operation)
        /// 
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void NOP()
        {
            OpCodeInProgress = false;
            AddressBusAddress++;

            Read(AddressBusAddress);

            EndOpCode();
        }

        /// <summary>
        /// ORA (bitwise OR with Accumulator)
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, Y: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, Y: </term>
        ///         <description>5+ Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void ORA()
        {
            A = (byte)(A | DataBusData);

            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

            AddressBusAddress++;

            EndOpCode();
        }


        /// <summary>
        /// Push Accumulator on Stack
        /// </summary>
        public void PHA()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();
                    _instructionCycles++;
                    break;
                }
                case (1):
                    BeginOpCode();
                    UpdateRw(false);
                    DataBusData = A;
                    OutputDataToDatabus((ushort)(SP | 0x0100));
                    _instructionCycles++;
                    break;
                case (2):
                    SP--;
                    AddressBusAddress++;
                    Read(AddressBusAddress);
                    EndOpCode();
                    break;
            }
        }

        /// <summary>
        /// Push Processor Status on Stack
        ///
        /// Implied Addressing
        /// </summary>
        public void PHP()
        {
            switch (_instructionCycles)
            {
                case 0:
                {
                    BeginOpCode();
                    UpdateRw(false);
                    P.SetFlag(StatusRegisterFlags.B, true); //PHP pushes with the B flag as 1, no matter what
                    DataBusData = P.Register;
                    OutputDataToDatabus((ushort)(0x0100 | SP));
                    P.SetFlag(StatusRegisterFlags.B);
                    SP--;
                    break;
                }
                case 1:
                {
                    P.SetFlag(StatusRegisterFlags.B, false);
                    AddressBusAddress++;
                    Read(AddressBusAddress);
                    EndOpCode();
                    break;
                }
            }

            _instructionCycles++;
        }

        /// <summary>
        /// Pull Accumulator from Stack
        /// </summary>
        public void PLA()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    SP++;
                    Read((ushort)(0x0100 | SP));
                    _instructionCycles++;
                    break;
                }
                case (2):
                {
                    A = DataBusData;
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    Read(AddressBusAddress);
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// Pull Processor Status from Stack
        /// </summary>
        public void PLP()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();
                    SP++;
                    Read((ushort)(SP | 0x0100));
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    P.SetFlag(DataBusData);
                    P.SetFlag(StatusRegisterFlags.B, false);
                    P.SetFlag(StatusRegisterFlags.Reserved, true);
                    P.SetFlag(StatusRegisterFlags.I, true);
                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// ROL (ROtate Left)
        ///
        /// ROL shifts all the bits left 1 position.
        ///
        /// The <see cref="StatusRegisterFlags.C"/> is shifted into bit 0 and
        /// the original bit 7 is shifted into the <see cref="StatusRegisterFlags.C"/>
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// <see cref="StatusRegisterFlags.C"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Accumulator </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>5 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>7 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void ROL()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    var result = DataBusData << 1 | (P.HasFlag(StatusRegisterFlags.C) ? 1 : 0);

                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, result == 0);
                    P.SetFlag(StatusRegisterFlags.C, (result & 0xFF00) != 0);

                    if (OpCode == 0x2A)
                        A = (byte)result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte)result;
                        OutputDataToDatabus();
                    }

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// ROR (ROtate Right)
        ///
        /// ROR shifts all the bits right one position. The <see cref="StatusRegisterFlags.C"/> is shifted into bit 7 and the original
        /// bit 0 is shifted into the <see cref="StatusRegisterFlags.C"/>
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// <see cref="StatusRegisterFlags.C"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Accumulator </term>
        ///         <description>2 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>5 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>7 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void ROR()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    var result = (P.HasFlag(StatusRegisterFlags.C) ? 1 : 0) << 7 | DataBusData >> 1;

                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, result == 0);
                    P.SetFlag(StatusRegisterFlags.C, (DataBusData & 0x01) != 0);

                    if (OpCode == 0x6A)
                        A = (byte)result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte)result;
                        OutputDataToDatabus();
                    }

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// RTI (ReTurn from Interrupt
        ///
        /// RTI retrieves the <see cref="P"/> and the program counter form the stack
        /// in that order.
        /// <remarks>
        /// Affects the following Flags
        /// ALL
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void RTI()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();
                    SP++;
                    AddressBusAddress = (ushort)(0x0100 + SP);
                    Read(AddressBusAddress);
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    P.SetFlag(DataBusData);
                    P.SetFlag(StatusRegisterFlags.B, P.HasFlag(StatusRegisterFlags.B));
                    P.SetFlag(StatusRegisterFlags.Reserved, P.HasFlag(StatusRegisterFlags.Reserved));
                    SP++;
                    AddressBusAddress = (ushort)(0x0100 + SP);
                    Read(AddressBusAddress);
                    _instructionCycles++;
                    break;
                }
                case (2):
                {
                    InstructionLoByte = DataBusData;
                    SP++;
                    AddressBusAddress = (ushort)(0x0100 + SP);
                    Read(AddressBusAddress);
                    _instructionCycles++;
                    break;
                }
                case (3):
                {
                    InstructionHiByte = DataBusData;
                    AddressBusAddress = (ushort)(InstructionHiByte << 8 | InstructionLoByte);
                    Read(AddressBusAddress);
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// RTS (ReTurn from Subroutine)
        ///
        /// 
        /// RTS pulls the top 2 bytes off the stack (little endian) and xfers
        /// program control to (address + 1).
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void RTS()
        {
            switch (_instructionCycles)
            {
                case (0):
                {
                    BeginOpCode();
                    SP++;
                    Read((ushort)(0x0100 | SP));
                    AddressBusAddress = DataBusData;
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    SP++;
                    Read((ushort) (0x0100 + SP));
                    AddressBusAddress |= (ushort)(DataBusData << 8);
                    _instructionCycles++;
                    break;
                }
                case (2):
                {
                    AddressBusAddress++;
                    Pc = AddressBusAddress;
                    Read(AddressBusAddress);
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// SBC (SuBtract with Carry)
        ///
        /// SBC results are dependant on the setting of the <see cref="StatusRegisterFlags.D"/> flag.
        ///
        /// In decimal mode, subtraction is carried out on the assumption thgat values involved are packed BCD.
        ///
        /// There is no way to subtract without carry.
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.N"/>
        /// <see cref="StatusRegisterFlags.Z"/>
        /// <see cref="StatusRegisterFlags.V"/>
        /// <see cref="StatusRegisterFlags.C"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Immediate Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, Y: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, Y: </term>
        ///         <description>5+ Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void SBC()
        {
            int result = A - DataBusData - (P.HasFlag(StatusRegisterFlags.C) ? 0 : 1);
            var overflow = ((A ^ result) & (~DataBusData ^ result) & 0x80) != 0;

            P.SetFlag(StatusRegisterFlags.C, (result & 0x100) == 0);
            P.SetFlag(StatusRegisterFlags.Z, result == 0);
            P.SetFlag(StatusRegisterFlags.V, overflow);
            P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
            A = (byte)result;

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// SEC (SEt Carry)
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.C"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void SEC()
        {
            P.SetFlag(StatusRegisterFlags.C);
            AddressBusAddress++;
            OpCodeInProgress = false;
            EndOpCode();
        }

        /// <summary>
        /// SED (SEt Decimal)
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.D"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void SED()
        {
            P.SetFlag(StatusRegisterFlags.D);
            AddressBusAddress++;
            Read(AddressBusAddress);
            EndOpCode();
        }

        /// <summary>
        /// SEI (SEt Interrupt)
        ///
        /// <remarks>
        /// Affects the following Flags
        /// <see cref="StatusRegisterFlags.I"/>
        /// </remarks>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        ///
        /// </summary>
        public void SEI()
        {
            P.SetFlag(StatusRegisterFlags.I, true);
            AddressBusAddress++;
            Read(AddressBusAddress);
            EndOpCode();
        }

        /// <summary>
        /// STA (STore Accumulator)
        /// 
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Zero Page: </term>
        ///         <description>3 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero Page, X: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute: </term>
        ///         <description>4 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, X: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Absolute, Y: </term>
        ///         <description>4+ Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, X: </term>
        ///         <description>6 Cycles</description>
        ///     </item>
        ///     <item>
        ///         <term>Indirect, Y: </term>
        ///         <description>5+ Cycles</description>
        ///     </item>
        /// </list>
        /// </summary>
        public void STA()
        {
            UpdateRw(false);
            DataBusData = A;
            OutputDataToDatabus(AddressBusAddress);
            OpCodeInProgress = false;
            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// Store Index X in Memory
        /// </summary>
        public void STX()
        {
            UpdateRw(false);
            DataBusData = X;
            OutputDataToDatabus();
            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// Store Index Y in Memory
        /// </summary>
        public void STY()
        {
            UpdateRw(false);
            DataBusData = Y;
            OutputDataToDatabus();
            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// TAX (Transfer A to X)
        /// 
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void TAX()
        {
            X = A;
            P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, X == 0);

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// TAY (Transfer A to Y)
        ///
        /// <list type="bullet">
        ///     <item>
        ///         <term>Implied Mode: </term>
        ///         <description>2 Cycles: </description>
        ///     </item>
        /// </list>
        /// </summary>
        public void TAY()
        {
            Y = A;
            P.SetFlag(StatusRegisterFlags.N, (Y & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, Y == 0);

            AddressBusAddress++;

            EndOpCode();
        }

        /// <summary>
        /// Transfer Stack Pointer to Index X
        /// </summary>
        public void TSX()
        {
            X = SP;
            P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, X == 0);

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// TXA (Transfer X to A)
        /// </summary>
        public void TXA()
        {
            A = X;
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, A == 0);

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// Transfer  Index X to Stack Register
        /// </summary>
        public void TXS()
        {
            SP = X;
            AddressBusAddress++;
            _instructionCycles++;
            EndOpCode();
        }

        /// <summary>
        /// TYA (Transfer Y to A)
        /// </summary>
        public void TYA()
        {
            A = Y;
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            _instructionCycles++;
            AddressBusAddress++;
            EndOpCode();
        }

        public void SLO()
        {
            var result = DataBusData << 1;
            P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, (result & 0x00FF) == 0);
            P.SetFlag(StatusRegisterFlags.C, (result & 0xFF00) > 0);

            if (OpCode == 0x0A)
                A = (byte)result;
            else
            {
                UpdateRw(false);
                DataBusData = (byte)result;
                OutputDataToDatabus();
            }

            A = (byte)(A | DataBusData);

            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

            AddressBusAddress++;
            _instructionCycles++;
            EndOpCode();
        }

        public void RLA()
        {
            var result = DataBusData << 1 | (P.HasFlag(StatusRegisterFlags.C) ? 1 : 0);

            P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, result == 0);
            P.SetFlag(StatusRegisterFlags.C, (result & 0xFF00) != 0);

            if (OpCode == 0x2A)
                A = (byte)result;
            else
            {
                if (_instructionCycles == 0)
                {
                    UpdateRw(false);
                    DataBusData = (byte)result;
                    OutputDataToDatabus();
                    _instructionCycles++;
                    return;
                }
            }

            A = (byte)(A & DataBusData);
            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
            AddressBusAddress++;
            EndOpCode();
        }

        public void SRE()
        {
            P.SetFlag(StatusRegisterFlags.C, (DataBusData & 0x01) != 0);
            var result = DataBusData >> 1;
            P.SetFlag(StatusRegisterFlags.Z, result == 0);
            P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);

            if (OpCode == 0x4A)
                A = (byte)result;
            else
            {
            UpdateRw(false);
            DataBusData = (byte)result;
            OutputDataToDatabus();
            }

            A ^= DataBusData;

            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

            AddressBusAddress++;

            EndOpCode();
        }

        public void RRA()
        {
            var result = (P.HasFlag(StatusRegisterFlags.C) ? 1 : 0) << 7 | DataBusData >> 1;

            P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.Z, result == 0);
            P.SetFlag(StatusRegisterFlags.C, (DataBusData & 0x01) != 0);

            if (OpCode == 0x6A)
                A = (byte)result;
            else
            {
                UpdateRw(false);
                DataBusData = (byte)result;
                OutputDataToDatabus();
            }

            result = A + DataBusData + (P.HasFlag(StatusRegisterFlags.C) ? 1 : 0);
            var overflow = ((A ^ result) & (DataBusData ^ result) & 0x80) != 0;
            A = (byte)result;

            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
            P.SetFlag(StatusRegisterFlags.C, result > byte.MaxValue);
            P.SetFlag(StatusRegisterFlags.V, overflow);

            AddressBusAddress++;
            EndOpCode();
        }

        /// <summary>
        /// Shortcut for LDA then TAX
        /// </summary>
        public void LAX()
        {
            A = DataBusData;
            P.SetFlag(StatusRegisterFlags.Z, A == 0);
            P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

            X = A;
            P.SetFlag(StatusRegisterFlags.Z, X == 0);
            P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);

            AddressBusAddress++;
            Read(AddressBusAddress);

            EndOpCode();
        }

        public void SAX()
        {
            var temp = (byte)(A & X);
            UpdateRw(false);
            DataBusData = temp;
            OutputDataToDatabus();
            AddressBusAddress++;
            EndOpCode();
        }

        public void DCP()
        {
            var data = DataBusData - 1;

            P.SetFlag(StatusRegisterFlags.N, (data & 0x0080) != 0);
            P.SetFlag(StatusRegisterFlags.Z, (data & 0x00FF) == 0);

            UpdateRw(false);
            DataBusData = (byte)(data & 0x00FF);
            OutputDataToDatabus();

            int comparison = (byte)(A - DataBusData);
            if (comparison is < 0 or > byte.MaxValue)
                comparison = 0;

            P.SetFlag(StatusRegisterFlags.C, A >= DataBusData);
            P.SetFlag(StatusRegisterFlags.Z, (comparison & 0x00FF) == 0);
            P.SetFlag(StatusRegisterFlags.N, (comparison & 0x0080) != 0);

            AddressBusAddress++;
            EndOpCode();
        }

        /*
         * Equivalent to INC value then SBC value, except supporting more addressing modes.
         */
        public void ISC()
        {
            var data = (DataBusData + 1);
            P.SetFlag(StatusRegisterFlags.N, (data & 0x0080) != 0);
            P.SetFlag(StatusRegisterFlags.Z, (data & 0x00FF) == 0);

            UpdateRw(false);
            DataBusData = (byte)(data & 0x00FF);
            OutputDataToDatabus();

            int result = A - DataBusData - (P.HasFlag(StatusRegisterFlags.C) ? 0 : 1);
            var overflow = ((A ^ result) & (~DataBusData ^ result) & 0x80) != 0;

            P.SetFlag(StatusRegisterFlags.C, (result & 0x100) == 0);
            P.SetFlag(StatusRegisterFlags.Z, result == 0);
            P.SetFlag(StatusRegisterFlags.V, overflow);
            P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
            A = (byte)result;

            AddressBusAddress++;
            EndOpCode();
        }

        #endregion

        #region 6502 Pins

        /// <summary>
        /// Pin 1 - VSS (Power)
        /// </summary>
        public void GRD()
        {
        }

        /// <summary>
        /// Pin 2 - RDY: Memory ready signal
        ///
        /// Ready. When 0, the cpu waits after the next ready cycle
        /// for this line going to 1
        ///
        /// For the cpu to run, this should be kept hi
        /// </summary>
        public void RDY(int inputVoltage)
        {
        }

        /// <summary>
        /// Pin 3 - Clock Out
        /// </summary>
        public void Ø1()
        {
        }

        /// <summary>
        /// Pin 4 - Interrupt Request
        ///
        /// When 0 the program counter is set to the value
        /// stored at 0xFFFE / 0xFFFF after the current command.
        ///
        /// The <see cref="StatusRegister.I"/> flag must be set.
        ///
        /// For the cpu to run, this should be kept hi
        /// </summary>
        public void IRQ()
        {
        }

        /// <summary>
        /// Pin 5 - Not Connected.
        /// </summary>
        public void NC()
        {
        }

        /// <summary>
        /// Pin 6 - Non Maskable Interrupt.
        ///
        /// When 0 the program counter is set to the value stored in
        /// 0xFFFA/0xFFFB after processing the current command.
        ///
        /// For the cpu to run, this should be kept hi
        /// </summary>
        public void NMI(float inputVoltage)
        {
        }

        /// <summary>
        /// Pin 7 - Synchronization.
        ///
        /// outputs 1 when 
        /// </summary>
        public void SYNC()
        {
        }

        /// <summary>
        /// Pin 8 - VCC (Power)
        ///
        /// Should be a clean +5V
        ///
        /// <remarks>
        /// Electrical characteristics of the 6502 determine that the input voltage should be
        /// +5.0v ± 5%
        ///
        /// VCC - 0.2 Min voltage
        /// </remarks>
        /// </summary>
        public void PWR(int voltage)
        {
            InputVoltage = voltage;
        }

        /// <summary>
        /// Pin 21 - Ground Pin
        /// </summary>
        public void GRD21()
        {
        }

        /// <summary>
        /// Pin 34OpCode
        ///
        /// <remarks>
        /// 0 = Write
        /// 1 = Read
        /// </remarks>
        /// </summary>
        public void RW(float inputVoltage)
        {
            CpuRead = inputVoltage >= 3.1;
        }

        /// <summary>
        /// Pin 35
        /// </summary>
        public void NC35()
        {
        }

        /// <summary>
        /// Pin 36
        /// </summary>
        public void NC36()
        {
        }

        public override void SetRW(bool rw)
        {
            throw new NotImplementedException();
        }

        public override byte Read(ushort address, bool ronly = false)
        {
            throw new NotImplementedException();
        }

        public override void Write(ushort address, byte data)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Pin 37
        ///
        /// External clock signal input
        ///
        /// Used to clock the CPU
        /// </summary>
        public override void Clock()
        {
            //Set the unused Flag
            P.SetFlag(StatusRegisterFlags.Reserved);
            P.SetFlag(StatusRegisterFlags.I);

            if (FetchInstruction)
            {
                Fetch();
                return; //fetching is always 1 cycle.
            }

            Execute();

            ClockComplete?.Invoke(this, null);
        }

        public void Fetch()
        {
            if (FetchInstruction)
            {
                _addressingModeCycles = 0;
                _instructionCycles = 0;

                AddressingModeInProgress = true;

                foreach (var kvp in _dataCompatibleDevices)
                {
                    if (!kvp.Value.PropagationOverridden)
                    {
                        OpCode = kvp.Value.Read(Pc); //Read the opcode from the databus
                    }
                }

                InstructionRegister = OpCodeLookupTable[OpCode]; //move the opcode to the IR

                Pc++; //Increment the program counter
                
                FetchInstruction = false;

                FetchComplete?.Invoke(this, null);
            }
        }

        public void Execute()
        {
            if (AddressingModeInProgress)
            {
                InstructionRegister.AddressingModeMethod();
            }

            if (!AddressingModeInProgress)
            {
                OpCodeInProgress = true;
                InstructionRegister.OpCodeMethod();
            }
        }

        /// <summary>
        /// Pin 38
        /// </summary>
        public void S0()
        {
        }

        /// <summary>
        /// Pin 39
        /// </summary>
        public void Ø2()
        {
        }

        public void ProgramCounterInitialisation()
        {
            foreach (var kvp in _dataCompatibleDevices)
            {
                if (!kvp.Value.PropagationOverridden)
                {
                    InstructionLoByte = kvp.Value.Read(0xFFFC);
                    InstructionHiByte = kvp.Value.Read(0xFFFD);

                    Pc = (ushort)(InstructionHiByte << 8 | InstructionLoByte);
                    AddressBusAddress = Pc;
                }
            }
        }

        /// <summary>
        /// Pin 40
        /// RESET
        /// </summary>
        public void RES(ushort address = 0x0000)
        {
            _instructionCycles = 0;
            _addressingModeCycles = 0;

            SP = 0xFD;
            AddressBusAddress = 0x0000;
            Pc = AddressBusAddress;
            AddressingModeInProgress = true;
            FetchInstruction = true;
            CpuRead = true;
            P.SetFlag(StatusRegisterFlags.Reserved);
            P.SetFlag(StatusRegisterFlags.I);

            if (address == 0x0000)
                ProgramCounterInitialisation();
            else
            {
                Pc = address;
                AddressBusAddress = address;
            }

            UpdateRw(true);
        }

        private void BeginOpCode()
        {
            if (!OpCodeInProgress)
            {
                OpCodeInProgress = true;
                AddressingModeInProgress = true;
                _addressingModeCycles = 0;
                InstructionLoByte = 0;
                InstructionHiByte = 0;
                FetchInstruction = false;
            }
        }

        private void EndOpCode()
        {
            OpCodeInProgress = false;
            AddressingModeInProgress = true;
            FetchInstruction = true;
            CpuRead = true;
            OpComplete?.Invoke(this, null);
            AddressBusAddress++;
        }

        private void SetData(byte databusData)
        {
            _instructionCycles++;
            DataBusData = databusData;
        }

        private byte Read(ushort address)
        {
#if EMULATE_PIN_OUTPUT
            //Address bus is uni directional.
            //Tell everything connected to the address bus the address
            foreach (var device in _addressCompatibleDevices)
            {
                for (var i = 0; i < device.AddressBusLines.Count; i++)
                {
                    var pw = Math.Pow(2, i);
                    var data = ((address & (ushort) pw) >> i);
                    device.AddressBusLines[i](data);
                }
            }
#else
            foreach (var kvp in _dataCompatibleDevices)
            {
                if (!kvp.Value.PropagationOverridden)
                {
                    DataBusData = kvp.Value.Read(address);
                }
            }

            return DataBusData;
#endif
        }

        private void UpdateRw(bool cpuRead)
        {
            CpuRead = cpuRead;
            foreach (var kvp in _dataCompatibleDevices)
            {
                kvp.Value.SetRW(CpuRead);
            }
        }

        #endregion

        private bool BoundaryCrossed()
        {
            return (AddressBusAddress & 0xFF00) != (InstructionHiByte << 8);
        }
    }
}
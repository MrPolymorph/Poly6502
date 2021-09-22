using System;
using System.Collections.Generic;
using System.Linq;
using Poly6502.Microprocessor.Attributes;
using Poly6502.Microprocessor.Flags;
using Poly6502.Utilities;

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

        public event EventHandler ClockComplete;
        public event EventHandler OpComplete;
        public event EventHandler AddressChanged;
        public event EventHandler FetchComplete;

        public Dictionary<byte, Operation> OpCodeLookupTable { get; private set; }

        /// <summary>
        /// Accumulator
        /// </summary>
        public byte A { get; private set; }

        /// <summary>
        /// X Register
        /// </summary>
        public byte X { get; private set; }

        /// <summary>
        /// Y Register
        /// </summary>
        public byte Y { get; private set; }

        /// <summary>
        /// Stack Pointer
        /// </summary>
        public byte SP { get; private set; }

        public ushort PC { get; set; }

        public byte InstructionLoByte { get; set; }
        public byte InstructionHiByte { get; set; }

        private int _instructionCycles;
        private int _addressingModeCycles;

        /// <summary>
        /// Status Register / Processor Flags
        /// </summary>
        public StatusRegister P { get; private set; }

        public Operation CurrentOperation { get; private set; }

        public byte OpCode { get; private set; }

        public float InputVoltage { get; private set; }

        public ushort TempAddress { get; private set; }
        public bool OpCodeInProgress { get; private set; }
        public bool AddressingModeInProgress { get; private set; }

        public int PreviousInstructionCycleLength { get; private set; }
        public int PreviousAddressingModeCycleLength { get; private set; }

        public M6502()
        {
            IgnorePropagation(true);
            P = new StatusRegister();

            OpCodeLookupTable = new Dictionary<byte, Operation>()
            {
                /* 0 Row */
                {0x00, new Operation(BRK, IMP)},
                {0x01, new Operation(ORA, IZX)},
                {0x05, new Operation(ORA, ZPA)},
                {0x06, new Operation(ASL, ZPA)},
                {0x08, new Operation(PHP, IMP)},
                {0x09, new Operation(ORA, IMM)},
                {0x0A, new Operation(ASL, ACC)},
                {0x0D, new Operation(ORA, ABS)},
                {0x0E, new Operation(ASL, ABS)},

                /* 1 Row */
                {0x10, new Operation(BPL, REL)},
                {0x11, new Operation(ORA, IZY)},
                {0x15, new Operation(ORA, ZPX)},
                {0x16, new Operation(ASL, ZPX)},
                {0x18, new Operation(CLC, IMP)},
                {0x19, new Operation(ORA, ABY)},
                {0x1D, new Operation(ORA, ABX)},
                {0x1E, new Operation(ASL, ABX)},

                /* 2 Row */
                {0x20, new Operation(JSR, ABS)},
                {0x21, new Operation(AND, IZX)},
                {0x24, new Operation(BIT, ZPA)},
                {0x25, new Operation(AND, ZPA)},
                {0x26, new Operation(ROL, ZPA)},
                {0x28, new Operation(PLP, IMP)},
                {0x29, new Operation(AND, IMM)},
                {0x2A, new Operation(ROL, ACC)},
                {0x2C, new Operation(BIT, ABS)},
                {0x2D, new Operation(AND, ABS)},
                {0x2E, new Operation(ROL, ABS)},

                /* 3 Row */
                {0x30, new Operation(BMI, REL)},
                {0x31, new Operation(AND, IZY)},
                {0x35, new Operation(AND, ZPX)},
                {0x36, new Operation(ROL, ZPX)},
                {0x38, new Operation(SEC, IMP)},
                {0x39, new Operation(AND, ABY)},
                {0x3D, new Operation(AND, ABX)},
                {0x3E, new Operation(ROL, ABX)},

                /* 4 Row */
                {0x40, new Operation(RTI, IMP)},
                {0x41, new Operation(EOR, IZX)},
                {0x45, new Operation(EOR, ZPA)},
                {0x46, new Operation(LSR, ZPA)},
                {0x48, new Operation(PHA, IMP)},
                {0x49, new Operation(EOR, IMM)},
                {0x4A, new Operation(LSR, ACC)},
                {0x4C, new Operation(JMP, ABS)},
                {0x4D, new Operation(EOR, ABS)},
                {0x4E, new Operation(LSR, ABS)},

                /* 5 Row */
                {0x50, new Operation(BVC, REL)},
                {0x51, new Operation(EOR, IZY)},
                {0x55, new Operation(EOR, ZPX)},
                {0x56, new Operation(LSR, ZPX)},
                {0x58, new Operation(CLI, IMP)},
                {0x59, new Operation(EOR, ABY)},
                {0x5D, new Operation(EOR, ABX)},
                {0x5E, new Operation(LSR, ABX)},

                /* 6 Row */
                {0x60, new Operation(RTS, IMP)},
                {0x61, new Operation(ADC, IZX)},
                {0x65, new Operation(ADC, ZPA)},
                {0x66, new Operation(ROR, ZPA)},
                {0x68, new Operation(PLA, IMP)},
                {0x69, new Operation(ADC, IMM)},
                {0x6A, new Operation(ROR, ACC)},
                {0x6C, new Operation(JMP, IND)},
                {0x6D, new Operation(ADC, ABS)},
                {0x6E, new Operation(ROR, ABS)},

                /* 7 Row */
                {0x70, new Operation(BVS, REL)},
                {0x71, new Operation(ADC, IZY)},
                {0x75, new Operation(ADC, ZPX)},
                {0x76, new Operation(ROR, ZPX)},
                {0x78, new Operation(SEI, IMP)},
                {0x79, new Operation(ADC, ABY)},
                {0x7D, new Operation(ADC, ABX)},
                {0x7E, new Operation(ROR, ABX)},

                /* 8 Row */
                {0x81, new Operation(STA, IZX)},
                {0x84, new Operation(STY, ZPA)},
                {0x85, new Operation(STA, ZPA)},
                {0x86, new Operation(STX, ZPA)},
                {0x88, new Operation(DEY, IMP)},
                {0x8A, new Operation(TXA, IMP)},
                {0x8C, new Operation(STY, ABS)},
                {0x8D, new Operation(STA, ABS)},
                {0x8E, new Operation(STX, ABS)},

                /* 9 Row */
                {0x90, new Operation(BCC, REL)},
                {0x91, new Operation(STA, IZY)},
                {0x94, new Operation(STY, ZPX)},
                {0x95, new Operation(STA, ZPX)},
                {0x96, new Operation(STX, ZPY)},
                {0x98, new Operation(TYA, IMP)},
                {0x99, new Operation(STA, ABY)},
                {0x9A, new Operation(TXS, IMP)},
                {0x9D, new Operation(STA, ABX)},

                /* A Row */
                {0xA0, new Operation(LDY, IMM)},
                {0xA1, new Operation(LDA, IZX)},
                {0xA2, new Operation(LDX, IMM)},
                {0xA4, new Operation(LDY, ZPA)},
                {0xA5, new Operation(LDA, ZPA)},
                {0xA6, new Operation(LDX, ZPA)},
                {0xA8, new Operation(TAY, IMP)},
                {0xA9, new Operation(LDA, IMM)},
                {0xAA, new Operation(TAX, IMP)},
                {0xAC, new Operation(LDY, ABS)},
                {0xAD, new Operation(LDA, ABS)},
                {0xAE, new Operation(LDX, ABS)},

                /* B Row */
                {0xB0, new Operation(BCS, REL)},
                {0xB1, new Operation(LDA, IZY)},
                {0xB4, new Operation(LDY, ZPX)},
                {0xB5, new Operation(LDA, ZPX)},
                {0xB6, new Operation(LDX, ZPY)},
                {0xB8, new Operation(CLV, IMP)},
                {0xB9, new Operation(LDA, ABY)},
                {0xBA, new Operation(TSX, IMP)},
                {0xBC, new Operation(LDY, ABX)},
                {0xBD, new Operation(LDA, ABX)},
                {0xBE, new Operation(LDX, ABY)},

                /* C Row */
                {0xC0, new Operation(CPY, IMM)},
                {0xC1, new Operation(CMP, IZX)},
                {0xC4, new Operation(CPY, ZPA)},
                {0xC5, new Operation(CMP, ZPA)},
                {0xC6, new Operation(DEC, ZPA)},
                {0xC8, new Operation(INY, IMP)},
                {0xC9, new Operation(CMP, IMM)},
                {0xCA, new Operation(DEX, IMP)},
                {0xCC, new Operation(CPY, ABS)},
                {0xCD, new Operation(CMP, ABS)},
                {0xCE, new Operation(DEC, ABS)},

                /* D Row */
                {0xD0, new Operation(BNE, REL)},
                {0xD1, new Operation(CMP, IZY)},
                {0xD5, new Operation(CMP, ZPX)},
                {0xD6, new Operation(DEC, ZPX)},
                {0xD8, new Operation(CLD, IMP)},
                {0xD9, new Operation(CMP, ABY)},
                {0xDD, new Operation(CMP, ABX)},
                {0xDE, new Operation(DEC, ABX)},

                /* E Row */
                {0xE0, new Operation(CPX, IMM)},
                {0xE1, new Operation(SBC, IZX)},
                {0xE4, new Operation(CPX, ZPA)},
                {0xE5, new Operation(SBC, ZPA)},
                {0xE6, new Operation(INC, ZPA)},
                {0xE8, new Operation(INX, IMP)},
                {0xE9, new Operation(SBC, IMM)},
                {0xEA, new Operation(NOP, IMP)},
                {0xEC, new Operation(CPX, ABS)},
                {0xED, new Operation(SBC, ABS)},
                {0xEE, new Operation(INC, ABS)},

                /* F Row */
                {0xF0, new Operation(BEQ, REL)},
                {0xF1, new Operation(SBC, IZY)},
                {0xF5, new Operation(SBC, ZPX)},
                {0xF6, new Operation(INC, ZPX)},
                {0xF8, new Operation(SED, IMP)},
                {0xF9, new Operation(SBC, ABY)},
                {0xFD, new Operation(SBC, ABX)},
                {0xFE, new Operation(INC, ABX)},

                /* NOP Illegal OpCodes */
                {0x1A, new Operation(NOP, IMP)},
                {0x3A, new Operation(NOP, IMP)},
                {0x5A, new Operation(NOP, IMP)},
                {0x7A, new Operation(NOP, IMP)},
                {0xDA, new Operation(NOP, IMP)},
                {0xFA, new Operation(NOP, IMP)},
                {0x80, new Operation(NOP, IMM)},
                {0x82, new Operation(NOP, IMM)},
                {0x89, new Operation(NOP, IMM)},
                {0xC2, new Operation(NOP, IMM)},
                {0xE2, new Operation(NOP, IMM)},
                {0x04, new Operation(NOP, ZPA)},
                {0x44, new Operation(NOP, ZPA)},
                {0x64, new Operation(NOP, ZPA)},
                {0x14, new Operation(NOP, ZPX)},
                {0x34, new Operation(NOP, ZPX)},
                {0x54, new Operation(NOP, ZPX)},
                {0x74, new Operation(NOP, ZPX)},
                {0xD4, new Operation(NOP, ZPX)},
                {0xF4, new Operation(NOP, ZPX)},
                {0x0C, new Operation(NOP, ABS)},
                {0x1C, new Operation(NOP, ABX)},
                {0x3C, new Operation(NOP, ABX)},
                {0x5C, new Operation(NOP, ABX)},
                {0x7C, new Operation(NOP, ABX)},
                {0xDC, new Operation(NOP, ABX)},
                {0xFC, new Operation(NOP, ABX)},

                /* LAX Illegal OpCodes */
                {0xA7, new Operation(LAX, ZPA)},
                {0xB7, new Operation(LAX, ZPY)},
                {0xAF, new Operation(LAX, ABS)},
                {0xBF, new Operation(LAX, ABY)},
                {0xA3, new Operation(LAX, IZX)},
                {0xB3, new Operation(LAX, IZY)},

                /* SAX Illegal Opcodes */
                {0x87, new Operation(SAX, ZPA)},
                {0x97, new Operation(SAX, ZPY)},
                {0x8F, new Operation(SAX, ABS)},
                {0x83, new Operation(SAX, IZX)},

                /* USBC Illegal OpCodes */
                {0xEB, new Operation(SBC, IMM)},

                /* DCP Illegal OpCodes */
                {0xC7, new Operation(DCP, ZPA)},
                {0xD7, new Operation(DCP, ZPX)},
                {0xCF, new Operation(DCP, ABS)},
                {0xDF, new Operation(DCP, ABX)},
                {0xDB, new Operation(DCP, ABY)},
                {0xC3, new Operation(DCP, IZX)},
                {0xD3, new Operation(DCP, IZY)},

                /* ISC Illegal OpCodes */
                {0xE7, new Operation(ISC, ZPA)},
                {0xF7, new Operation(ISC, ZPX)},
                {0xEF, new Operation(ISC, ABS)},
                {0xFF, new Operation(ISC, ABX)},
                {0xFB, new Operation(ISC, ABY)},
                {0xE3, new Operation(ISC, IZX)},
                {0xF3, new Operation(ISC, IZY)},

                /* SLO Illegal OpCodes */
                {0x07, new Operation(SLO, ZPA)},
                {0x17, new Operation(SLO, ZPX)},
                {0x0F, new Operation(SLO, ABS)},
                {0x1F, new Operation(SLO, ABX)},
                {0x1B, new Operation(SLO, ABY)},
                {0x03, new Operation(SLO, IZX)},
                {0x13, new Operation(SLO, IZY)},

                /* RLA Illegal OpCodes */
                {0x27, new Operation(RLA, ZPA)},
                {0x37, new Operation(RLA, ZPX)},
                {0x2F, new Operation(RLA, ABS)},
                {0x3F, new Operation(RLA, ABX)},
                {0x3B, new Operation(RLA, ABY)},
                {0x23, new Operation(RLA, IZX)},
                {0x33, new Operation(RLA, IZY)},

                /* SRE Illegal OpCodes */
                {0x47, new Operation(SRE, ZPA)},
                {0x57, new Operation(SRE, ZPX)},
                {0x4F, new Operation(SRE, ABS)},
                {0x5F, new Operation(SRE, ABX)},
                {0x5B, new Operation(SRE, ABY)},
                {0x43, new Operation(SRE, IZX)},
                {0x53, new Operation(SRE, IZY)},

                /* SRE Illegal OpCodes */
                {0x67, new Operation(RRA, ZPA)},
                {0x77, new Operation(RRA, ZPX)},
                {0x6F, new Operation(RRA, ABS)},
                {0x7F, new Operation(RRA, ABX)},
                {0x7B, new Operation(RRA, ABY)},
                {0x63, new Operation(RRA, IZX)},
                {0x73, new Operation(RRA, IZY)},

                /* JAM Illegal Opcodes */
                {0x02, new Operation(JAM, IMP)},
                {0x12, new Operation(JAM, IMP)},
                {0x22, new Operation(JAM, IMP)},
                {0x32, new Operation(JAM, IMP)},
                {0x42, new Operation(JAM, IMP)},
                {0x52, new Operation(JAM, IMP)},
                {0x62, new Operation(JAM, IMP)},
                {0x72, new Operation(JAM, IMP)},
                {0x92, new Operation(JAM, IMP)},
                {0xB2, new Operation(JAM, IMP)},
                {0xD2, new Operation(JAM, IMP)},
                {0xF2, new Operation(JAM, IMP)},

                /* ANC Illegal Opcodes */
                {0x0B, new Operation(ANC, IMM)},
                {0x2B, new Operation(ANC, IMM)},

                /* ALR Illegal Opcodes */
                {0x4B, new Operation(ALR, IMM)},

                /* ARR Illegal Opcodes */
                {0x6B, new Operation(ARR, IMM)},

                /* ANE Illegal Opcodes*/
                {0x8B, new Operation(ANE, IMM)},

                /* SHA Illegal Opcodes */
                {0x9F, new Operation(SHA, ABY)},
                {0x93, new Operation(SHA, IZY)},

                /* TAS Illegal Opcodes */
                {0x9B, new Operation(TAS, ABY)},

                /* SHY Illegal Opcodes */
                {0x9C, new Operation(SHY, ABX)},

                /* SHX Illegal Opcodes */
                {0x9E, new Operation(SHX, ABY)},

                /* LXA Illegal Opcodes */
                {0xAB, new Operation(LXA, IMM)},

                /* LAS Illegal Opcodes */
                {0xBB, new Operation(LAS, ABY)},
            };

            RES();
        }


        #region Addressing Modes

        /*
         * http://www.emulator101.com/6502-addressing-modes.html :
         *
         * When the 6502 refers to addressing modes, it really means "What is the source of the data used in this instruction?" 
         * The 6502's data book divides the addressing modes into 2 groups, indexed and non-indexed.
         * 
         */


        /// <summary>
        /// Accumulator Addressing
        ///
        /// This form of addressing is represented with a 1 byte instruction,
        /// implying an operation on the accumulator
        /// </summary>
        public void ACC()
        {
            DataBusData = A;
            AddressingModeInProgress = false;
        }

        /// <summary>
        /// Implied Addressing
        ///
        /// The data is implied as part of the op
        /// </summary>
        public void IMP()
        {
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
            switch (_addressingModeCycles)
            {
                case 0:
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    InstructionLoByte = DataBusData;
                    AddressingModeInProgress = false;
                    break;
            }
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
                case 0: 
                {
                    AddressingModeInProgress = true;
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    _addressingModeCycles++;
                    break;
                }
                case 1:
                    InstructionLoByte = DataBusData;
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    _addressingModeCycles++;
                    break;
                case 2:
                    InstructionHiByte = DataBusData;
                    TempAddress = AddressBusAddress;
                    AddressBusAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
                    OutputAddressToPins(AddressBusAddress);
                    AddressBusAddress = TempAddress;
                    AddressingModeInProgress = false;
                    break;
            }
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
                case (0):
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    _addressingModeCycles++;
                    break;
                case (1):
                    InstructionLoByte = DataBusData;
                    OutputAddressToPins(InstructionLoByte);
                    AddressingModeInProgress = false;
                    break;
            }
        }

        /// <summary>
        /// Zero Page Addressing with X offset.
        ///
        /// 
        /// </summary>
        public void ZPX()
        {
            switch (_addressingModeCycles)
            {
                case (0):
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    _addressingModeCycles++;
                    break;
                case (1):
                    InstructionLoByte = (byte) (DataBusData + X);
                    OutputAddressToPins((ushort) ((InstructionLoByte) & 0x00FF));
                    AddressingModeInProgress = false;
                    _addressingModeCycles++;
                    break;
            }
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
        /// </summary>
        public void ZPY()
        {
            switch (_addressingModeCycles)
            {
                case (0):
                {
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                }
                case (1):
                {
                    OutputAddressToPins((ushort) ((DataBusData + Y) & 0x00FF));
                    AddressingModeInProgress = false;
                    break;
                }
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
        /// </summary>
        public void ABY()
        {
            switch (_addressingModeCycles)
            {
                case 0: //set the address bus to get the lo byte of the address
                {
                    BeginOpCode();
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                }
                case 1:
                    InstructionLoByte = DataBusData;
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                case 2:
                    InstructionHiByte = DataBusData;
                    ushort address = (ushort) ((ushort) ((InstructionHiByte << 8 | InstructionLoByte)) + Y);
                    OutputAddressToPins(address);
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
        /// </summary>
        public void ABX()
        {
            switch (_addressingModeCycles)
            {
                case 0: //set the address bus to get the lo byte of the address
                {
                    BeginOpCode();
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                }
                case 1:
                    InstructionLoByte = DataBusData;
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                case 2:
                    InstructionHiByte = DataBusData;
                    ushort address = (ushort) ((ushort) ((InstructionHiByte << 8 | InstructionLoByte)) + X);
                    OutputAddressToPins(address);
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
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    RelativeAddress = DataBusData;

                    if ((RelativeAddress & 0x80) != 0)
                    {
                        RelativeAddress = (ushort) (DataBusData | 0xFF00);
                    }

                    AddressingModeInProgress = false;
                    break;
            }

            _addressingModeCycles++;
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
        /// </summary>
        public void IZX()
        {
            switch (_addressingModeCycles)
            {
                case (0):
                {
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                }
                case (1):
                {
                    _offset = DataBusData;
                    TempAddress = AddressBusAddress;
                    AddressBusAddress = (ushort) ((_offset + (X)) & 0x00FF);
                    OutputAddressToPins(AddressBusAddress);
                    break;
                }
                case (2):
                {
                    InstructionLoByte = DataBusData;
                    AddressBusAddress = (ushort) ((_offset + (X + 1)) & 0x00FF);
                    OutputAddressToPins(AddressBusAddress);
                    break;
                }
                case (3):
                {
                    InstructionHiByte = DataBusData;
                    AddressBusAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
                    OutputAddressToPins(AddressBusAddress);
                    AddressBusAddress = TempAddress;
                    AddressingModeInProgress = false;
                    break;
                }
            }

            _addressingModeCycles++;
        }

        /// <summary>
        /// Indirect Indexed Addressing
        ///
        /// n  indirect indexed addressing (referred to as [Indirect. Y[).
        /// the second byte of the instruction  points to a  memory location  in page zero.
        /// The contents of this  memory location  is added to the contents of the Y index
        /// register, the result being the low order eight bits of the effective address.
        ///
        /// The carry from this addition  is added  to the contents of the next page zero
        /// memory location, the result being the high order eight bits of the effective address.
        /// </summary>
        public void IZY()
        {
            switch (_addressingModeCycles)
            {
                case (0):
                {
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                }
                case (1):
                {
                    _offset = DataBusData;
                    TempAddress = AddressBusAddress;
                    AddressBusAddress = (ushort) ((_offset) & 0x00FF);
                    OutputAddressToPins(AddressBusAddress);
                    break;
                }
                case (2):
                {
                    InstructionLoByte = DataBusData;
                    AddressBusAddress = (ushort) ((_offset + 1) & 0x00FF);
                    OutputAddressToPins(AddressBusAddress);
                    break;
                }
                case (3):
                {
                    InstructionHiByte = DataBusData;
                    AddressBusAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
                    OutputAddressToPins((ushort) (AddressBusAddress + Y));
                    AddressBusAddress = TempAddress;
                    AddressingModeInProgress = false;
                    break;
                }
            }

            _addressingModeCycles++;
        }


        /// <summary>
        /// Indirect Addressing
        ///
        /// This addressing mode reads from the address supplied after the opcode.
        /// this address is then read from to retrieve the opeand
        ///
        /// Cycle 0 - Send out address
        /// Cycle 1 - Read the lo byte
        /// Cycle 2 - Read the hi byte / Send combined address (hi sl 8 or lo)
        /// Cycle 3 - Read new lo byte / page boundary check. Either output the hi byte of the new address or the address +1
        /// Cycle 4 - Read the new hi byte
        /// </summary>
        public void IND()
        {
            switch (_addressingModeCycles)
            {
                case 0:
                    TempAddress = AddressBusAddress;
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    _addressingModeCycles++;
                    break;
                case 1:
                    InstructionLoByte = DataBusData;
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    _addressingModeCycles++;
                    break;
                case 2:
                    InstructionHiByte = DataBusData;
                    TempAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
                    OutputAddressToPins(TempAddress);
                    _addressingModeCycles++;
                    break;
                case 3:
                    InstructionLoByte = DataBusData;
                    
                    if (InstructionLoByte == 0x00FF)
                        OutputAddressToPins((ushort) (TempAddress & 0xFF00));
                    else
                        OutputAddressToPins((ushort) (TempAddress + 1));
                    
                    _addressingModeCycles++;
                    break;
                case 4:
                    InstructionHiByte = DataBusData;
                    AddressingModeInProgress = false;
                    break;
            }
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
                    A = (byte) (A & DataBusData);
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
                    A = (byte) (A & DataBusData);
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
                    var result = DataBusData << 1;
                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, (result & 0x00FF) == 0);
                    P.SetFlag(StatusRegisterFlags.C, (result & 0xFF00) > 0);

                    if (OpCode == 0x0A)
                        A = (byte) result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte) result;
                        OutputDataToDatabus();
                    }


                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// This operation involves the adder:
        /// V-flag is set according to (A AND oper) + oper
        /// The carry is not set, but bit 7 (sign) is exchanged with the carry
        /// </summary>
        public void ARR()
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
                    A = (byte) (A & DataBusData);

                    var result = (P.HasFlag(StatusRegisterFlags.C) ? 1 : 0) << 7 | A >> 1;

                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, result == 0);
                    P.SetFlag(StatusRegisterFlags.C, (DataBusData & (1 << 5)) != 0);
                    P.SetFlag(StatusRegisterFlags.V, ((DataBusData & (1 << 5)) ^ ((DataBusData & (1 << 4)))) != 0);

                    if (OpCode == 0x6A)
                        A = (byte) result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte) result;
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
                    A = (byte) (A & DataBusData);
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
                        A = (byte) result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte) result;
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
                    var result = A + DataBusData + (P.HasFlag(StatusRegisterFlags.C) ? 1 : 0);
                    var overflow = ((A ^ result) & (DataBusData ^ result) & 0x80) != 0;
                    A = (byte) result;

                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.C, result > byte.MaxValue);
                    P.SetFlag(StatusRegisterFlags.V, overflow);

                    AddressBusAddress++;
                    EndOpCode();

                    break;
                }
            }
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
            if (!P.HasFlag(StatusRegisterFlags.C))
            {
                AddressBusAddress += DataBusData;
                OutputAddressToPins(AddressBusAddress);
            }

            AddressBusAddress++;

            OpCodeInProgress = false;
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
            if (P.HasFlag(StatusRegisterFlags.C))
            {
                AddressBusAddress++;
                AddressBusAddress += DataBusData;
                OutputAddressToPins(AddressBusAddress);
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
            if (P.HasFlag(StatusRegisterFlags.Z))
            {
                AddressBusAddress += DataBusData;
                OutputAddressToPins(AddressBusAddress);
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
            switch (_instructionCycles)
            {
                case (0):
                {
                    OutputAddressToPins((ushort) (InstructionHiByte << 8 | InstructionLoByte));
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    byte temp = (byte) (A & DataBusData);
                    P.SetFlag(StatusRegisterFlags.Z, (temp & 0x00FF) == 0);
                    P.SetFlag(StatusRegisterFlags.N, (DataBusData & (1 << 7)) != 0);
                    P.SetFlag(StatusRegisterFlags.V, (DataBusData & (1 << 6)) != 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
                AddressBusAddress += DataBusData;
            }

            AddressBusAddress++;

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
                    if (!P.HasFlag(StatusRegisterFlags.Z))
                    {
                        var absolute = AddressBusAddress + RelativeAddress;

                        if ((absolute & 0xFF00) != (AddressBusAddress & 0xFF00))
                        {
                            _instructionCycles++;
                        }
                        else
                        {
                            AddressBusAddress = (ushort) absolute;
                            AddressBusAddress++;
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
                AddressBusAddress += DataBusData;
            }

            AddressBusAddress++;
            OutputAddressToPins(AddressBusAddress);
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
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    DataBusData = (byte) ((AddressBusAddress >> 8) & 0x00FF);
                    SP--;

                    //enable interrupt because we are in a software break.
                    P.SetFlag(StatusRegisterFlags.I, true);

                    _instructionCycles++;
                    break;
                case (1):
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    DataBusData = (byte) (AddressBusAddress & 0x00FF);
                    SP--;

                    _instructionCycles++;
                    break;
                case (2):
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    DataBusData = (byte) P.Register;
                    P.SetFlag(StatusRegisterFlags.B, true);
                    SP--;

                    _instructionCycles++;
                    break;
                case (3):
                    UpdateRw(true);
                    AddressBusAddress = 0xFFFE;
                    OutputAddressToPins(AddressBusAddress);

                    _instructionCycles++;
                    break;
                case (4):
                    AddressBusAddress = 0x0000;
                    AddressBusAddress = DataBusData;
                    AddressBusAddress = 0xFFFF;
                    OutputAddressToPins(AddressBusAddress);

                    _instructionCycles++;
                    break;
                case (5):
                    AddressBusAddress = (ushort) (DataBusData << 8);
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
            if (!P.HasFlag(StatusRegisterFlags.V))
            {
                AddressBusAddress += DataBusData;
            }

            AddressBusAddress++;


            OutputAddressToPins(AddressBusAddress);
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
            if (P.HasFlag(StatusRegisterFlags.V))
            {
                AddressBusAddress += DataBusData;
            }

            AddressBusAddress++;

            OutputAddressToPins(AddressBusAddress);
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
            P.SetFlag(StatusRegisterFlags.C, false);
            AddressBusAddress++;
            OpCodeInProgress = false;
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
            P.SetFlag(StatusRegisterFlags.D, false);
            AddressBusAddress++;
            OutputAddressToPins(AddressBusAddress);
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
            P.SetFlag(StatusRegisterFlags.I, false);
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
                    int comparison = (byte) (A - DataBusData);
                    if (comparison is < 0 or > byte.MaxValue)
                        comparison = 0;

                    P.SetFlag(StatusRegisterFlags.C, A >= DataBusData);
                    P.SetFlag(StatusRegisterFlags.Z, (comparison & 0x00FF) == 0);
                    P.SetFlag(StatusRegisterFlags.N, (comparison & 0x0080) != 0);
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    EndOpCode();
                    break;
                }
            }
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
                    var result = X - DataBusData;
                    P.SetFlag(StatusRegisterFlags.C, X >= DataBusData);
                    P.SetFlag(StatusRegisterFlags.Z, (result & 0x00FF) == 0);
                    P.SetFlag(StatusRegisterFlags.N, (result & 0x0080) != 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
            switch (_instructionCycles)
            {
                case (0):
                {
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    BeginOpCode();

                    var result = Y - DataBusData;

                    P.SetFlag(StatusRegisterFlags.Z, result == 0);
                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.C, Y >= DataBusData);

                    AddressBusAddress++;

                    EndOpCode();
                    break;
                }
            }
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
                case (0):
                {
                    BeginOpCode();
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    var data = DataBusData - 1;

                    P.SetFlag(StatusRegisterFlags.N, (data & 0x0080) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, (data & 0x00FF) == 0);

                    UpdateRw(false);
                    DataBusData = (byte) (data & 0x00FF);
                    OutputDataToDatabus();

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
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
                    X--;
                    P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, X == 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
                    Y--;
                    P.SetFlag(StatusRegisterFlags.N, (Y & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, Y == 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
                    A ^= DataBusData;

                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

                    AddressBusAddress++;

                    EndOpCode();
                    break;
                }
            }
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
                    var data = (DataBusData + 1);
                    P.SetFlag(StatusRegisterFlags.N, (data & 0x0080) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, (data & 0x00FF) == 0);

                    UpdateRw(false);
                    DataBusData = (byte) (data & 0x00FF);
                    OutputDataToDatabus();

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
                    X++;
                    P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, X == 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
                    Y++;

                    P.SetFlag(StatusRegisterFlags.N, (Y & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, Y == 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
            AddressBusAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
            OutputAddressToPins(AddressBusAddress);
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
                    OutputAddressToPins((ushort) (0x0100 + SP));
                    DataBusData = (byte) ((AddressBusAddress >> 8) & 0x00FF);
                    OutputDataToDatabus();
                    SP--;
                    _instructionCycles++;
                    break;
                case (2): //store the lo byte
                    UpdateRw(false);
                    OutputAddressToPins((ushort) (0x0100 + SP));
                    DataBusData = (byte) ((AddressBusAddress & 0x00FF));
                    SP--;
                    _instructionCycles++;
                    break;
                case (3):
                    AddressBusAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
                    OutputAddressToPins(AddressBusAddress);
                    OpCodeInProgress = false;
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
                    A = DataBusData;
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

                    // TODO something else should incrementing this
                    // AddressBusAddress++;
                    // OutputAddressToPins(AddressBusAddress);

                    EndOpCode();
                    break;
                }
            }
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
                    X = DataBusData;
                    AddressBusAddress++;

                    P.SetFlag(StatusRegisterFlags.Z, X == 0);
                    P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);

                    EndOpCode();
                    break;
                }
            }
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
                    Y = DataBusData;
                    AddressBusAddress++;

                    P.SetFlag(StatusRegisterFlags.Z, Y == 0);
                    P.SetFlag(StatusRegisterFlags.N, (Y & 0x80) != 0);

                    EndOpCode();
                    break;
                }
            }
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
                    P.SetFlag(StatusRegisterFlags.C, (DataBusData & 0x01) != 0);
                    var result = DataBusData >> 1;
                    P.SetFlag(StatusRegisterFlags.Z, result == 0);
                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);

                    if (OpCode == 0x4A)
                        A = (byte) result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte) result;
                        OutputDataToDatabus();
                    }

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
                    OpCodeInProgress = false;
                    AddressBusAddress++;

                    OutputAddressToPins(AddressBusAddress);

                    EndOpCode();
                    break;
                }
            }
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
            switch (_instructionCycles)
            {
                case (0):
                {
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    A = (byte) (A | DataBusData);

                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

                    AddressBusAddress++;

                    EndOpCode();
                    break;
                }
            }
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
                    OutputAddressToPins((ushort) (SP + 0x0100));
                    DataBusData = A;
                    OutputDataToDatabus();
                    _instructionCycles++;
                    break;
                case (2):
                    SP--;
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
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
            BeginOpCode();

            switch (_instructionCycles)
            {
                case 0:
                {
                    TempAddress = AddressBusAddress;
                    AddressBusAddress = (ushort) (0x0100 | SP);
                    P.SetFlag(StatusRegisterFlags.B);
                    UpdateRw(false);
                    OutputAddressToPins(AddressBusAddress);
                    DataBusData = P.Register;
                    OutputDataToDatabus();
                    SP--;
                    break;
                }
                case 1:
                {
                    P.SetFlag(StatusRegisterFlags.B, false);
                    AddressBusAddress = TempAddress;
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
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
                    TempAddress = AddressBusAddress;
                    AddressBusAddress = (ushort) (0x0100 | SP);
                    OutputAddressToPins(AddressBusAddress);
                    _instructionCycles++;
                    break;
                }
                case (2):
                {
                    A = DataBusData;
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    AddressBusAddress = TempAddress;
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
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
            BeginOpCode();

            switch (_instructionCycles)
            {
                case (0):
                {
                    SP++;
                    OutputAddressToPins((ushort) (SP | 0x0100));
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    P.SetFlag(DataBusData);
                    P.SetFlag(StatusRegisterFlags.B, false);
                    P.SetFlag(StatusRegisterFlags.Reserved, true);
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
                        A = (byte) result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte) result;
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
                        A = (byte) result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte) result;
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
            BeginOpCode();

            switch (_instructionCycles)
            {
                case (0):
                {
                    SP++;
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    P.SetFlag(DataBusData);
                    P.SetFlag(StatusRegisterFlags.B, P.HasFlag(StatusRegisterFlags.B));
                    P.SetFlag(StatusRegisterFlags.Reserved, P.HasFlag(StatusRegisterFlags.Reserved));
                    SP++;
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    _instructionCycles++;
                    break;
                }
                case (2):
                {
                    InstructionLoByte = DataBusData;
                    SP++;
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    _instructionCycles++;
                    break;
                }
                case (3):
                {
                    InstructionHiByte = DataBusData;
                    AddressBusAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
                    OutputAddressToPins(AddressBusAddress);
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// RTS (ReTurn from Subroutine
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
            BeginOpCode();

            switch (_instructionCycles)
            {
                case (0):
                {
                    SP++;
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    _instructionCycles++;
                    break;
                }
                case (1):
                {
                    InstructionLoByte = DataBusData;
                    SP++;
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    _instructionCycles++;
                    break;
                }
                case (2):
                {
                    InstructionHiByte = DataBusData;
                    AddressBusAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
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
                    int result = A - DataBusData - (P.HasFlag(StatusRegisterFlags.C) ? 0 : 1);
                    var overflow = ((A ^ result) & (~DataBusData ^ result) & 0x80) != 0;

                    P.SetFlag(StatusRegisterFlags.C, (result & 0x100) == 0);
                    P.SetFlag(StatusRegisterFlags.Z, result == 0);
                    P.SetFlag(StatusRegisterFlags.V, overflow);
                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
                    A = (byte) result;

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
                    P.SetFlag(StatusRegisterFlags.C);
                    AddressBusAddress++;
                    OpCodeInProgress = false;
                    break;
                }
            }
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
                    P.SetFlag(StatusRegisterFlags.D);
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    EndOpCode();
                    break;
                }
            }
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
                    P.SetFlag(StatusRegisterFlags.I, true);
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    EndOpCode();
                    break;
                }
            }
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
                    UpdateRw(false);
                    DataBusData = A;
                    OutputDataToDatabus();
                    OpCodeInProgress = false;
                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// Store Index X in Memory
        /// </summary>
        public void STX()
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
                    UpdateRw(false);
                    DataBusData = X;
                    OutputDataToDatabus();
                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// Store Index Y in Memory
        /// </summary>
        public void STY()
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
                    UpdateRw(false);
                    DataBusData = Y;
                    OutputDataToDatabus();
                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
                    X = A;
                    P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, X == 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
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
                    Y = A;
                    P.SetFlag(StatusRegisterFlags.N, (Y & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, Y == 0);

                    AddressBusAddress++;

                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// Transfer Stack Pointer to Index X
        /// </summary>
        public void TSX()
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
                    X = SP;
                    P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, X == 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// TXA (Transfer X to A)
        /// </summary>
        public void TXA()
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
                    A = X;
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// Transfer  Index X to Stack Register
        /// </summary>
        public void TXS()
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
                    SP = X;
                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /// <summary>
        /// TYA (Transfer Y to A)
        /// </summary>
        public void TYA()
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
                    A = Y;
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        public void SLO()
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
                    var result = DataBusData << 1;
                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, (result & 0x00FF) == 0);
                    P.SetFlag(StatusRegisterFlags.C, (result & 0xFF00) > 0);

                    if (OpCode == 0x0A)
                        A = (byte) result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte) result;
                        OutputDataToDatabus();
                    }

                    A = (byte) (A | DataBusData);

                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

                    AddressBusAddress++;

                    EndOpCode();
                    break;
                }
            }
        }

        public void RLA()
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
                        A = (byte) result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte) result;
                        OutputDataToDatabus();
                    }

                    A = (byte) (A & DataBusData);
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        public void SRE()
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
                    P.SetFlag(StatusRegisterFlags.C, (DataBusData & 0x01) != 0);
                    var result = DataBusData >> 1;
                    P.SetFlag(StatusRegisterFlags.Z, result == 0);
                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);

                    if (OpCode == 0x4A)
                        A = (byte) result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte) result;
                        OutputDataToDatabus();
                    }

                    A ^= DataBusData;

                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

                    AddressBusAddress++;

                    EndOpCode();
                    break;
                }
            }
        }

        public void RRA()
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
                        A = (byte) result;
                    else
                    {
                        UpdateRw(false);
                        DataBusData = (byte) result;
                        OutputDataToDatabus();
                    }

                    result = A + DataBusData + (P.HasFlag(StatusRegisterFlags.C) ? 1 : 0);
                    var overflow = ((A ^ result) & (DataBusData ^ result) & 0x80) != 0;
                    A = (byte) result;

                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);
                    P.SetFlag(StatusRegisterFlags.C, result > byte.MaxValue);
                    P.SetFlag(StatusRegisterFlags.V, overflow);

                    AddressBusAddress++;
                    EndOpCode();

                    break;
                }
            }
        }

        /// <summary>
        /// Shortcut for LDA then TAX
        /// </summary>
        public void LAX()
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
                    A = DataBusData;
                    P.SetFlag(StatusRegisterFlags.Z, A == 0);
                    P.SetFlag(StatusRegisterFlags.N, (A & 0x80) != 0);

                    X = A;
                    P.SetFlag(StatusRegisterFlags.Z, X == 0);
                    P.SetFlag(StatusRegisterFlags.N, (X & 0x80) != 0);

                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);

                    EndOpCode();
                    break;
                }
            }
        }

        public void SAX()
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
                    var temp = (byte) (A & X);
                    UpdateRw(false);
                    DataBusData = temp;
                    OutputDataToDatabus();
                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        public void DCP()
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
                    var data = DataBusData - 1;

                    P.SetFlag(StatusRegisterFlags.N, (data & 0x0080) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, (data & 0x00FF) == 0);

                    UpdateRw(false);
                    DataBusData = (byte) (data & 0x00FF);
                    OutputDataToDatabus();

                    int comparison = (byte) (A - DataBusData);
                    if (comparison is < 0 or > byte.MaxValue)
                        comparison = 0;

                    P.SetFlag(StatusRegisterFlags.C, A >= DataBusData);
                    P.SetFlag(StatusRegisterFlags.Z, (comparison & 0x00FF) == 0);
                    P.SetFlag(StatusRegisterFlags.N, (comparison & 0x0080) != 0);

                    AddressBusAddress++;
                    EndOpCode();
                    break;
                }
            }
        }

        /*
         * Equivalent to INC value then SBC value, except supporting more addressing modes.
         */
        public void ISC()
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
                    var data = (DataBusData + 1);
                    P.SetFlag(StatusRegisterFlags.N, (data & 0x0080) != 0);
                    P.SetFlag(StatusRegisterFlags.Z, (data & 0x00FF) == 0);

                    UpdateRw(false);
                    DataBusData = (byte) (data & 0x00FF);
                    OutputDataToDatabus();

                    int result = A - DataBusData - (P.HasFlag(StatusRegisterFlags.C) ? 0 : 1);
                    var overflow = ((A ^ result) & (~DataBusData ^ result) & 0x80) != 0;

                    P.SetFlag(StatusRegisterFlags.C, (result & 0x100) == 0);
                    P.SetFlag(StatusRegisterFlags.Z, result == 0);
                    P.SetFlag(StatusRegisterFlags.V, overflow);
                    P.SetFlag(StatusRegisterFlags.N, (result & 0x80) != 0);
                    A = (byte) result;

                    AddressBusAddress++;
                    EndOpCode();

                    break;
                }
            }
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

        public override byte Read(ushort address)
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

            if (!OpCodeInProgress)
                Fetch();
            
            Execute();
            
            ClockComplete?.Invoke(this, null);
        }

        public void Fetch()
<<<<<<< HEAD
        {
            if (!AddressingModeInProgress && !OpCodeInProgress)
            {
                PC = AddressBusAddress;
                AddressingModeInProgress = true;

                foreach (var kvp in _dataCompatibleDevices.OrderBy(x => x.Key))
                {
                    if (!kvp.Value.PropagationOverridden)
                    {
                        OpCode = kvp.Value.Read(AddressBusAddress);
                    }
                }

                CurrentOperation = OpCodeLookupTable[OpCode];

                FetchComplete?.Invoke(this, null);
            }
=======
        {
            if (!AddressingModeInProgress && !OpCodeInProgress)
            {
                AddressingModeInProgress = true;

                foreach (var kvp in _dataCompatibleDevices)
                {
                    if (!kvp.Value.PropagationOverridden)
                    {
                        OpCode = kvp.Value.Read(AddressBusAddress);
                    }
                }
                CurrentOperation = OpCodeLookupTable[OpCode];
            }

            CurrentOperation.AddressingModeMethod();

            FetchComplete?.Invoke(this, null);
            
>>>>>>> dfda4d279541fe14c8abfdd78e07481b62e9642e
        }

        public void Execute()
        {
            if (AddressingModeInProgress)
            {
                CurrentOperation.AddressingModeMethod();
            }
            
            if(!AddressingModeInProgress)
            {
                OpCodeInProgress = true;
                CurrentOperation.OpCodeMethod();
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

                    PC = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
                    AddressBusAddress = PC;
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
            PC = AddressBusAddress;
            AddressingModeInProgress = false;
            CpuRead = true;
            P.SetFlag(StatusRegisterFlags.Reserved);
            P.SetFlag(StatusRegisterFlags.I);
            
            if(address == 0x0000)
                ProgramCounterInitialisation();
            else
            {
                PC = address;
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
                _instructionCycles = 0;
                _addressingModeCycles = 0;
                InstructionLoByte = 0;
                InstructionHiByte = 0;
            }
        }

        private void EndOpCode()
        {
            OpCodeInProgress = false;
            AddressingModeInProgress = false;
            PreviousInstructionCycleLength = _instructionCycles;
            PreviousAddressingModeCycleLength = _addressingModeCycles;
            _addressingModeCycles = 0;
            _instructionCycles = 0;
            CpuRead = true;
            OpComplete?.Invoke(this, null);
        }

        private void OutputAddressToPins(ushort address)
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
                    DataBusData = kvp.Value.Read(AddressBusAddress);
                }
            }
            AddressChanged?.Invoke(this, null);
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
    }
}
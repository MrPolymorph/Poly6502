using System;
using System.Collections.Generic;
using Poly6502.Microprocessor.Flags;
using Poly6502.Utilities;

namespace Poly6502.Microprocessor
{
    /// <summary>
    /// The 6502 is a little-endian 8-bit processor with a 16-bit address bus.
    ///
    /// The 6502 typically runs at 1 to 2Mhz
    /// </summary>
    public class M6502 : AbstractAddressDataBus
    {
        private bool _pcFetchComplete;
        private bool _addressingModeInProgress;

        private int _instructionCycles;
        private int _addressingModeCycles;
        private int _pcCurrentFetchCycle;
        private ushort _indirectAddress;

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


        public ushort PC { get; private set; }

        public byte InstructionLoByte { get; set; }
        public byte InstructionHiByte { get; set; }

        /// <summary>
        /// Status Register / Processor Flags
        /// </summary>
        public StatusRegister P { get; private set; }

        public byte OpCode { get; private set; }


        public float InputVoltage { get; private set; }

        public ushort AbsoluteAddress { get; private set; }
        public bool OpCodeInProgress { get; private set; }


        public M6502()
        {
            OpCodeLookupTable = new Dictionary<byte, Operation>()
            {
                /* 0 Row */
                {0x00, new Operation(BRK, IMP)},
                {0x01, new Operation(ORA, IND)},
                {0x05, new Operation(ORA, ZPA)},
                {0x06, new Operation(ASL, ZPA)},
                {0x07, new Operation(SLO, ZPA)},
                {0x08, new Operation(PHP, IMP)},
                {0x09, new Operation(ORA, IMM)},
                {0x0A, new Operation(ASL, ACC)},
                {0x0D, new Operation(ORA, ABS)},
                {0x0E, new Operation(ASL, ABS)},

                /* 1 Row */
                {0x10, new Operation(BPL, REL)},
                {0x11, new Operation(ORA, IND)},
                {0x15, new Operation(ORA, ZPX)},
                {0x16, new Operation(ASL, ZPX)},
                {0x18, new Operation(CLC, IMP)},
                {0x19, new Operation(ORA, ABS)},
                {0x1D, new Operation(ORA, ABS)},
                {0x1E, new Operation(ASL, ABS)},

                /* 2 Row */
                {0x20, new Operation(JSR, ABS)},
                {0x21, new Operation(AND, IND)},
                {0x24, new Operation(BIT, ZPA)},
                {0x25, new Operation(AND, ZPA)},
                {0x26, new Operation(ROL, ZPA)},
                {0x28, new Operation(PLP, IMP)},
                {0x29, new Operation(AND, IMM)},
                {0x2A, new Operation(ROL, IMP)},
                {0x2C, new Operation(BIT, ABS)},
                {0x2D, new Operation(AND, ABS)},
                {0x2E, new Operation(ROL, ABS)},

                /* 3 Row */
                {0x30, new Operation(BMI, REL)},
                {0x31, new Operation(AND, IND)},
                {0x35, new Operation(AND, ZPX)},
                {0x36, new Operation(AND, ZPX)},
                {0x38, new Operation(SEC, IMP)},
                {0x39, new Operation(AND, ABS)},
                {0x3D, new Operation(AND, ABS)},
                {0x3E, new Operation(ROL, ABS)},

                /* 4 Row */
                {0x40, new Operation(RTI, IMP)},
                {0x41, new Operation(EOR, IND)},
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
                {0x51, new Operation(EOR, IND)},
                {0x55, new Operation(EOR, ZPX)},
                {0x56, new Operation(LSR, ZPX)},
                {0x58, new Operation(CLI, IMP)},
                {0x59, new Operation(EOR, ABS)},
                {0x5D, new Operation(EOR, ABS)},
                {0x5E, new Operation(LSR, ABS)},

                /* 6 Row */
                {0x60, new Operation(RTS, IMP)},
                {0x61, new Operation(ADC, IND)},
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
                {0x71, new Operation(ADC, IND)},
                {0x75, new Operation(ADC, ZPX)},
                {0x76, new Operation(ROR, ZPX)},
                {0x78, new Operation(SEI, IMP)},
                {0x79, new Operation(ADC, ABS)},
                {0x7D, new Operation(ADC, ABS)},
                {0x7E, new Operation(ROR, ABS)},

                /* 8 Row */
                {0x81, new Operation(STA, IND)},
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
                {0x91, new Operation(STA, IND)},
                {0x94, new Operation(STY, ZPX)},
                {0x95, new Operation(STA, ZPX)},
                {0x96, new Operation(STX, ZPY)},
                {0x98, new Operation(TYA, IMP)},
                {0x99, new Operation(STA, ABS)},
                {0x9A, new Operation(TXS, ABS)},
                {0x9D, new Operation(STA, ABS)},

                /* A Row */
                {0xA0, new Operation(LDY, IMM)},
                {0xA1, new Operation(LDA, IND)},
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
                {0xB1, new Operation(LDA, IND)},
                {0xB4, new Operation(LDY, ZPX)},
                {0xB5, new Operation(LDA, ZPX)},
                {0xB6, new Operation(LDX, ZPY)},
                {0xB8, new Operation(CLV, IMP)},
                {0xB9, new Operation(LDA, ABS)},
                {0xBA, new Operation(TSX, IMP)},
                {0xBC, new Operation(LDY, ABS)},
                {0xBD, new Operation(LDA, ABS)},
                {0xBE, new Operation(LDX, ABS)},

                /* C Row */
                {0xC0, new Operation(CPY, IMM)},
                {0xC1, new Operation(CMP, IND)},
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
                {0xD1, new Operation(CMP, IND)},
                {0xD5, new Operation(CMP, ZPX)},
                {0xD6, new Operation(DEC, ZPX)},
                {0xD8, new Operation(CLD, ZPY)},
                {0xD9, new Operation(CMP, IMP)},
                {0xDD, new Operation(CMP, ABS)},
                {0xDE, new Operation(DEC, ABS)},

                /* E Row */
                {0xE0, new Operation(CPX, IMM)},
                {0xE1, new Operation(SBC, IND)},
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
                {0xF1, new Operation(SBC, IND)},
                {0xF5, new Operation(SBC, ZPX)},
                {0xF6, new Operation(INC, ZPX)},
                {0xF8, new Operation(SED, IMP)},
                {0xF9, new Operation(SBC, ABS)},
                {0xFD, new Operation(SBC, ABS)},
                {0xFE, new Operation(INC, ABS)},
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
        }

        public void IMP()
        {
            _addressingModeInProgress = false;
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
                    break;
                case 1:
                    InstructionLoByte = DataBusData;
                    _addressingModeInProgress = false;
                    break;
            }

            _addressingModeCycles++;
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
                case 0: //set the address bus to get the lo byte of the address
                {
                    _addressingModeInProgress = true;
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
                    _addressingModeInProgress = false;
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
            if (!_addressingModeInProgress)
            {
                _addressingModeInProgress = true;
            }

            switch (_addressingModeCycles)
            {
                case (0):
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                case (1):
                    InstructionLoByte = DataBusData &= 0x00FF;
                    _addressingModeInProgress = false;
                    AddressBusAddress++;
                    break;
            }

            _addressingModeCycles++;
        }

        /// <summary>
        /// Zero Page Addressing with X offset.
        ///
        /// 
        /// </summary>
        public void ZPX()
        {
            if (!_addressingModeInProgress)
            {
                _addressingModeInProgress = true;
            }

            switch (_addressingModeCycles)
            {
                case (0):
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                case (1):
                    AddressBusAddress = (ushort) ((DataBusData + X) & 0x00FF);
                    OutputAddressToPins(AddressBusAddress);
                    _addressingModeInProgress = false;
                    break;
            }

            _addressingModeCycles++;
        }

        public void ZPY()
        {
            switch (_addressingModeCycles)
            {
            }
        }

        /// <summary>
        /// Indexed Zero Page Addressing
        ///
        /// This form of addressing is used in conjunction with the index register
        /// and is referred to as Zero Page X or Zero Page Y. The effective address
        /// is calculated by adding the second byte to the contents of the index register.
        ///  Since this is a form of "Zero Page" addressing, the content of the 2nd byte
        /// references a location in page zero. Additionally, due to the Zero Page addressing
        /// nature of this mode. no carry is added to the high order 8 bits of memory and
        /// crossing of page boundaries does not occur.
        /// </summary>
        public void IZP()
        {
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
        public void IAB()
        {
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
                    break;
                case 1:
                    InstructionLoByte = DataBusData;
                    _addressingModeInProgress = false;
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
        public void IIND()
        {
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
        public void INDI()
        {
        }

        /// <summary>
        /// Absolute Indirect
        ///
        /// The second byte of the instruction contains the low order eight bits of a memory location.
        /// The  high order eight bits of that memory location  is contained  in  the third byte of the instruction.
        /// The contents of the fully specified  memory location  is the low order byte of the effective address.
        /// The  next memory location contains the high  order byte of the effective address which
        /// is loaded  into the sixteen  bits of the program counter.
        /// </summary>
        public void ABI()
        {
        }

        /// <summary>
        /// Indirect Addressing
        ///
        /// Indirect addressing take 5 cycles
        ///
        /// cycle 1 - Setup Address
        /// cycle 2 - Read, setup address
        /// cycle 3 - Read, setup address
        /// cycle 4 - Read, setup address
        /// cycle 5 - Finalisation
        /// </summary>
        public void IND()
        {
            switch (_addressingModeCycles)
            {
                /*
                 * Setup address to read from memory at location PC
                 */
                case 0:
                    OutputAddressToPins(AddressBusAddress);
                    break;
                /*
                 * Save data from the data bus
                 * increment PC
                 * setup address to read from memory at PC
                 */
                case 1:
                    InstructionLoByte = DataBusData;
                    AddressBusAddress++;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                /*
                 * Save data from the data bus
                 * save the pointer to (_indirectAddress)
                 * increment PC
                 * setup address to read from _indirectAddress
                 */
                case 2:
                    InstructionHiByte = DataBusData;
                    _indirectAddress = (ushort) ((InstructionHiByte << 8) | InstructionLoByte);

                    AddressBusAddress++;
                    if (InstructionLoByte == 0x00FF)
                        AddressBusAddress = (ushort) (_indirectAddress & 0xFF00);
                    else
                        AddressBusAddress = (ushort) (_indirectAddress + 1);

                    OutputAddressToPins(AddressBusAddress);
                    break;
                /*
                 * Save data from the data bus
                 * Setup address to read from _indirectAddress
                 */
                case 3:
                    AbsoluteAddress = (ushort) (DataBusData << 8);
                    AddressBusAddress = _indirectAddress;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                /*
                 * Combine all answers to give final absolute address.
                 * Setup the address to read from that location.
                 */
                case 4:
                    AddressBusAddress = (ushort) (AbsoluteAddress | DataBusData);
                    _addressingModeInProgress = false;
                    OutputDataToDatabus();
                    break;
            }

            _addressingModeCycles++;
        }

        #endregion

        #region Instruction Set

        /// <summary>
        /// Add Memory to Accumulator with carry
        /// </summary>
        public void ADS()
        {
        }

        /// <summary>
        /// AND Memory with Accumulator
        /// </summary>
        public void AND()
        {
        }

        /// <summary>
        /// Shift left One Bit (Memory or Accumulator)
        /// </summary>
        public void ASL()
        {
            SetFlag(StatusRegister.C, (DataBusData & 0xFF00) > 0);
            SetFlag(StatusRegister.Z, (DataBusData & 0x00FF) == 0);
            SetFlag(StatusRegister.N, (DataBusData & 0x80) == 1);

            switch (_instructionCycles)
            {
                case 0:
                    CpuRead = false;
                    UpdateRw();
                    OutputAddressToPins(AddressBusAddress);
                    DataBusData &= 0x00FF;
                    break;
                case 1:
                    EndOpCode();
                    break;
            }


            _instructionCycles++;
        }

        public void ADC()
        {
        }

        /// <summary>
        /// Branch on Carry Clear
        /// </summary>
        public void BCC()
        {
            if ((P & StatusRegister.C) == 0)
            {
                AddressBusAddress++;
                AddressBusAddress += InstructionLoByte;
                OutputAddressToPins(AddressBusAddress);
            }
            else
            {
                AddressBusAddress++;
                
            }
            
            OpCodeInProgress = false;
        }

        /// <summary>
        /// Branch on Carry Set
        /// </summary>
        public void BCS()
        {
            if ((P & StatusRegister.C) != 0)
            {
                AddressBusAddress++;
                AddressBusAddress += InstructionLoByte;
                OutputAddressToPins(AddressBusAddress);
            }
            else
            {
                AddressBusAddress++;
                
            }
            
            EndOpCode();
        }

        /// <summary>
        /// Branch on Result Zero
        /// </summary>
        public void BEQ()
        {
            if ((P & StatusRegister.Z) != 0)
            {
                AddressBusAddress++;
                AddressBusAddress += InstructionLoByte;
                OutputAddressToPins(AddressBusAddress);
            }
            else
            {
                AddressBusAddress++;
            }
            
            EndOpCode();
        }

        /// <summary>
        /// Test Bits in Memory with Accumulator
        /// </summary>
        public void BIT()
        {
            int temp = (A & DataBusData);
            SetFlag(StatusRegister.Z, (temp & 0x00FF) == 0);
            SetFlag(StatusRegister.N, (DataBusData & (1 << 7)) == 0);
            SetFlag(StatusRegister.V, (DataBusData & (1 << 6)) == 0);
            
            OutputAddressToPins(AddressBusAddress);
            EndOpCode();
        }

        /// <summary>
        /// Branch on Result Minus
        /// </summary>
        public void BMI()
        {
        }

        /// <summary>
        /// Branch on Result Result not Zero
        /// </summary>
        public void BNE()
        {
            if ((P & StatusRegister.Z) == 0)
            {
                //crossing page boundary check
                AddressBusAddress += DataBusData;
            }
            
            AddressBusAddress++;

            OutputAddressToPins(AddressBusAddress);

            EndOpCode();
        }

        /// <summary>
        /// Branch on Result Plus
        /// </summary>
        public void BPL()
        {
        }

        /// <summary>
        /// Force Break
        /// </summary>
        public void BRK()
        {
            BeginOpCode();

            switch (_instructionCycles)
            {
                /*
                 * Tell everyone we are writing to the data bus
                 * set the address bus to address on the stack.
                 * Write the hi byte of the program counter to the data bus
                 * decrement stack pointer for next operation
                 */
                case (0):
                    CpuRead = false;
                    UpdateRw();
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    DataBusData = (byte) ((AddressBusAddress >> 8) & 0x00FF);
                    SP--;

                    //enable interrupt because we are in a software break.
                    SetFlag(StatusRegister.I, true);
                    break;
                /*
                 * Set the address bus to the address on the stack.
                 * Write the lo byte of the program counter to the data bus
                 * decrement the stack pointer for next operation
                 * 
                 */
                case (1):
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    DataBusData = (byte) (AddressBusAddress & 0x00FF);
                    SP--;
                    break;
                /*
                 * Set the address bus to the address on the stack
                 * Write the current status register to the data bus
                 * decrement the stack pointer for next operation
                 */
                case (2):
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins(AddressBusAddress);
                    DataBusData = (byte) P;
                    SetFlag(StatusRegister.B, true);
                    SP--;
                    break;
                /*
                 * Tell everyone we now want to read
                 * Set the address bus to address 0xFFFE
                 */
                case (3):
                    CpuRead = true;
                    UpdateRw();
                    AddressBusAddress = 0xFFFE;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                /*
                 * read the data from the databus into the PC.
                 * Set the address bus to address 0xFFFF
                 */
                case (4):
                    AddressBusAddress = 0x0000;
                    AddressBusAddress = DataBusData;
                    AddressBusAddress = 0xFFFF;
                    OutputAddressToPins(AddressBusAddress);
                    break;
                /*
                 * Read the data bus into the hi byte of the PC
                 * Opcode Complete
                 */
                case (5):
                    AddressBusAddress = (ushort) (DataBusData << 8);
                    SetFlag(StatusRegister.B, false);
                    EndOpCode();
                    break;
            }

            _instructionCycles++;
        }

        /// <summary>
        /// Branch on Overflow Clear
        /// </summary>
        public void BVC()
        {
            if ((P & StatusRegister.V) == 0)
            {
                AddressBusAddress += DataBusData;
                AddressBusAddress++;
            }
            else
            {
                AddressBusAddress++;
            }
            
            OutputAddressToPins(AddressBusAddress);
            EndOpCode();
        }

        /// <summary>
        /// Branch on Overflow Set
        /// </summary>
        public void BVS()
        {
            if ((P & StatusRegister.V) != 0)
            {
                AddressBusAddress += DataBusData;
                AddressBusAddress++;
            }
            
            OutputAddressToPins(AddressBusAddress);
            EndOpCode();
        }

        /// <summary>
        /// Clear Carry Flag
        /// </summary>
        public void CLC()
        {
            SetFlag(StatusRegister.C, false);
            AddressBusAddress++;
            OpCodeInProgress = false;
        }

        /// <summary>
        /// Clear Decimal Mode
        /// </summary>
        public void CLD()
        {
            P &= ~StatusRegister.D;
        }

        /// <summary>
        /// Clear Interrupt Disable Bit
        /// </summary>
        public void CLI()
        {
            P &= ~StatusRegister.D;
        }

        /// <summary>
        /// Clear Overflow Flag
        /// </summary>
        public void CLV()
        {
        }

        /// <summary>
        /// Compare Memory and Accumulator 
        /// </summary>
        public void CMP()
        {
            switch (_instructionCycles)
            {
                case (0):
                    var comparison = A - DataBusData;
                    SetFlag(StatusRegister.C, A >= DataBusData);
                    SetFlag(StatusRegister.Z, (comparison & 0x00FF) == 0);
                    SetFlag(StatusRegister.N, (comparison & 0x0080) == 0);
                    EndOpCode();
                    break;
            }
        }


        /// <summary>
        /// Compare Memory and Index X
        /// </summary>
        public void CPX()
        {
            var comparison = X - InstructionLoByte;
            SetFlag(StatusRegister.C, X >= InstructionLoByte);
            SetFlag(StatusRegister.Z, (comparison & 0x00FF) == 0);
            SetFlag(StatusRegister.N, (comparison & 0x0080) == 0);
        }

        /// <summary>
        /// Compare Memory and Index Y
        /// </summary>
        public void CPY()
        {
        }

        /// <summary>
        /// Decrement Memory by 1
        /// </summary>
        public void DEC()
        {
        }

        /// <summary>
        /// Decrement Index X by 1
        /// </summary>
        public void DEX()
        {
        }

        /// <summary>
        /// Decrement Index Y by 1
        /// </summary>
        public void DEY()
        {
        }

        /// <summary>
        /// Exclusive OR Memory with Accumulator
        /// </summary>
        public void EOR()
        {
        }

        /// <summary>
        /// Increment Memory by 1
        /// </summary>
        public void INC()
        {
        }

        /// <summary>
        /// Increment Index X by 1
        /// </summary>
        public void INX()
        {
        }

        /// <summary>
        /// Increment Index Y by 1
        /// </summary>
        public void INY()
        {
        }

        /// <summary>
        /// Jump to New Location
        ///
        /// 3 cycles
        /// </summary>
        public void JMP()
        {
            AddressBusAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
            PC = AddressBusAddress;
            OutputAddressToPins(AddressBusAddress);
            EndOpCode();
        }

        /// <summary>
        /// Jump to New Location
        /// Saving Return Address
        /// </summary>
        public void JSR()
        {
            switch (_instructionCycles)
            {
                case 0: //store the hi byte
                    OutputAddressToPins((ushort) (0x0100 + SP));
                    DataBusData = (byte) ((AddressBusAddress >> 8) & 0x00FF);
                    OutputDataToDatabus();
                    SP--;
                    break;
                case 1: //store the lo byte
                    OutputAddressToPins((ushort) (0x0100 + SP));
                    DataBusData = (byte) ((AddressBusAddress & 0x00FF));
                    SP--;
                    break;
                case 2:
                    AddressBusAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
                    OutputAddressToPins(AddressBusAddress);
                    OpCodeInProgress = false;
                    break;
            }

            _instructionCycles++;
        }

        /// <summary>
        /// Load the Accumulator with Memory
        /// </summary>
        public void LDA()
        {
            A = DataBusData;
            SetFlag(StatusRegister.Z, A == 0);
            SetFlag(StatusRegister.N, (A & 0x80) != 0);

            AddressBusAddress++;
            OutputAddressToPins(AddressBusAddress);
            
            EndOpCode();
        }

        /// <summary>
        /// Load Index X with Memory
        /// </summary>
        public void LDX()
        {
            X = DataBusData;
            AddressBusAddress++;

            SetFlag(StatusRegister.Z, X == 0);
            SetFlag(StatusRegister.N, (X & 0x80) != 0);

            EndOpCode();
        }

        /// <summary>
        /// Load Index Y with Memory
        /// </summary>
        public void LDY()
        {
        }

        /// <summary>
        /// Shift 1 bit right (Memory or Accumulation)
        /// </summary>
        public void LSR()
        {
        }

        /// <summary>
        /// No Operation
        /// </summary>
        public void NOP()
        {
            OpCodeInProgress = false;
            AddressBusAddress++;
            
            EndOpCode();
        }

        /// <summary>
        /// OR Memory with Accumulator
        /// </summary>
        public void ORA()
        {
            var data = InstructionLoByte;
            A = (byte) (A | data);

            SetFlag(StatusRegister.Z, A == 0);
            SetFlag(StatusRegister.N, (A & 0x80) == 0);

            EndOpCode();
        }


        /// <summary>
        /// Push Accumulator on Stack
        /// </summary>
        public void PHA()
        {
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
                    CpuRead = false;
                    UpdateRw();
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    SetFlag(StatusRegister.B);
                    DataBusData = (byte) P;
                    OutputAddressToPins(AddressBusAddress);
                    SP--;
                    break;
                }
                case 1:
                {
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
        }

        /// <summary>
        /// Pull Processor Status from Stack
        /// </summary>
        public void PLP()
        {
        }

        /// <summary>
        /// Rotate 1 bit left (Memory or Accumulator)
        /// </summary>
        public void ROL()
        {
        }

        /// <summary>
        /// Rotate 1 bit right (Memory of Accumulator)
        /// </summary>
        public void ROR()
        {
        }

        /// <summary>
        /// Return from Interrupt
        /// </summary>
        public void RTI()
        {
        }

        /// <summary>
        /// Return from Subroutine
        /// </summary>
        public void RTS()
        {
        }

        /// <summary>
        /// Subtract Memory from Accumulator with Borrow
        /// </summary>
        public void SBC()
        {
        }

        /// <summary>
        /// Set Carry Flag
        /// </summary>
        public void SEC()
        {
            SetFlag(StatusRegister.C);
            AddressBusAddress++;
            OpCodeInProgress = false;
        }

        /// <summary>
        /// Set Decimal Mode
        /// </summary>
        public void SED()
        {
        }

        /// <summary>
        /// Set Interrupt Disable Status
        /// </summary>
        public void SEI()
        {
        }

        /// <summary>
        /// Store Accumulator in Memory
        /// </summary>
        public void STA()
        {
            CpuRead = false;
            UpdateRw();
            DataBusData = A;
            OutputDataToDatabus();
            OpCodeInProgress = false;
            EndOpCode();
        }

        /// <summary>
        /// Store Index X in Memory
        /// </summary>
        public void STX()
        {
            CpuRead = false;
            UpdateRw();
            OutputAddressToPins(InstructionLoByte);
            DataBusData = X;
            OutputDataToDatabus();
            EndOpCode();
        }

        /// <summary>
        /// Store Index Y in Memory
        /// </summary>
        public void STY()
        {
        }

        /// <summary>
        /// Transfer Accumulator to Index X
        /// </summary>
        public void TAX()
        {
        }

        /// <summary>
        /// Transfer Accumulator to IndexY
        /// </summary>
        public void TAY()
        {
        }

        /// <summary>
        /// Transfer Stack Pointer to Index X
        /// </summary>
        public void TSX()
        {
        }

        /// <summary>
        /// Transfer  Index X to Accumulator
        /// </summary>
        public void TXA()
        {
        }

        /// <summary>
        /// Transfer  Index X to Stack Register
        /// </summary>
        public void TXS()
        {
        }

        /// <summary>
        /// Transfer Index Y to Accumulator
        /// </summary>
        public void TYA()
        {
        }

        public void SLO()
        {
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
            Clock();
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

        /// <summary>
        /// Pin 37
        ///
        /// External clock signal input
        ///
        /// Used to clock the CPU
        /// </summary>
        public override void Clock()
        {
            CpuRead = true;
            UpdateRw();

            OutputAddressToPins(AddressBusAddress);
            
            //Set the unused Flag
            SetFlag(StatusRegister.Reserved);

            Fetch();
            Execute();

            if (!CpuRead) //Write any data required to the address bus
            {
                OutputDataToDatabus();
            }
            
            if (!OpCodeInProgress)
            {
                PC = AddressBusAddress;
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

        /// <summary>
        /// Pin 40
        /// RESET
        /// </summary>
        public void RES()
        {
            //On a reset, the cpu will look at
            //memory location 0xFFFC for an opcode to run
            //this will take two cycles
            //one for the lo byte, one for the hi byte.            
            SP = 0xFD;

            AddressBusAddress = 0xC000;
            PC = 0xBFFF;
            _instructionCycles = 2;
            _pcCurrentFetchCycle = 2;
            _indirectAddress = 0;
            _addressingModeCycles = 0;
            _addressingModeInProgress = false;
            _pcFetchComplete = false;
            CpuRead = true;
            SetFlag(StatusRegister.Reserved);



            //output the address to the address bus
            //so that, on the next cycle, data can be picked up
            //outputted from ram/rom
            OutputAddressToPins(AddressBusAddress);
            UpdateRw();
        }
        
        public override byte DirectRead(ushort address)
        {
            throw new NotImplementedException();
        }

        private void Fetch()
        {
            if (!OpCodeInProgress)
                OpCode = DataBusData;
        }

        private void DecodePc()
        {
        }


        private void Execute()
        {
            var operation = OpCodeLookupTable[OpCode];

            BeginOpCode();

            operation.AddressingModeMethod();

            if (!_addressingModeInProgress)
                operation.OpCodeMethod();
        }

        private void BeginOpCode()
        {
            if (!OpCodeInProgress)
            {
                OpCodeInProgress = true;
                _addressingModeInProgress = true;
                _addressingModeCycles = 0;
                InstructionLoByte = 0;
                InstructionHiByte = 0;
            }
        }

        private void EndOpCode()
        {
            OpCodeInProgress = false;
            _addressingModeInProgress = false;
            _addressingModeCycles = 0;
            _instructionCycles = 0;
            CpuRead = true;
            UpdateRw();
        }

        private void OutputAddressToPins(ushort address)
        {
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
        }

        private void UpdateRw()
        {
            foreach (var busDevices in _dataCompatibleDevices)
            {
                busDevices.SetRW(CpuRead);
            }
        }

        private void SetFlag(StatusRegister flag, bool set = true)
        {
            if (set)
                P |= flag;
            else
                P &= ~flag;
        }

        #endregion
    }
}
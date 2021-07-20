using System;
using System.Collections.Generic;
using Poly6502.Microprocessor.Extensions;
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
        private bool _opCodeInProgress;
        private int _instructionCycles;
        private int _addressingModeCycles;
        private int _pcCurrentFetchCycle;
        private ushort _indirectAddress;

        private Dictionary<AddressingMode, Action> _addressingMethodLookup;

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

        public byte OpCodeData { get; private set; }


        public float InputVoltage { get; private set; }

        public ushort AbsoluteAddress { get; private set; }


        public M6502()
        {
            _addressingMethodLookup = new Dictionary<AddressingMode, Action>()
            {
                {AddressingMode.Absolute, ABS},
                {AddressingMode.Accumulator, ACC},
                {AddressingMode.ZeroPage, ZPA},
                {AddressingMode.ZeroPageX, ZPX},
                {AddressingMode.Indirect, IND},
                {AddressingMode.Relative, REL},
                {AddressingMode.Immediate, IMM},
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
                    OpCodeData = DataBusData;
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
            if (!_addressingModeInProgress)
            {
                _addressingModeInProgress = true;
            }

            switch (_addressingModeCycles)
            {
                case 0: //set the address bus to get the lo byte of the address
                {
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
                    OpCodeData = DataBusData &= 0x00FF;
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
                    OutputDataToDatabus();
                    break;
                case 5:
                    OpCodeData = DataBusData;
                    _addressingModeInProgress = false;
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
        public void ASL(AddressingMode mode)
        {
            BeginOpCode();

            if (_addressingModeInProgress)
            {
                _addressingMethodLookup[mode]();
            }
            else
            {
                P.Set(StatusRegister.C, (DataBusData & 0xFF00) > 0);
                P.Set(StatusRegister.Z, (DataBusData & 0x00FF) == 0);
                P.Set(StatusRegister.N, (DataBusData & 0x80) == 1);

                if (mode == AddressingMode.Implicit)
                    A = (byte) (DataBusData & 0x00FF);
                else
                    _instructionCycles++;

                switch (_instructionCycles)
                {
                    case 0:
                        CpuRead = false;
                        UpdateRw();
                        OutputAddressToPins(AddressBusAddress);
                        DataBusData &= 0x00FF;
                        break;
                    case 1:
                        _opCodeInProgress = false;
                        CpuRead = true;
                        UpdateRw();
                        break;
                }
            }

            _instructionCycles++;
        }

        /// <summary>
        /// Branch on Carry Clear
        /// </summary>
        public void BCC()
        {
        }

        /// <summary>
        /// Branch on Carry Set
        /// </summary>
        public void BCS()
        {
        }

        /// <summary>
        /// Branch on Result Zero
        /// </summary>
        public void BEQ()
        {
        }

        /// <summary>
        /// Test Bits in Memory with Accumulator
        /// </summary>
        public void BIT()
        {
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
            BeginOpCode();
            
            if ((P & StatusRegister.Z) == 0)
            {
                _instructionCycles++;


                //crossing page boundary check
                if ((AbsoluteAddress & 0xFF00) != (AddressBusAddress & 0xFF00))

                    AddressBusAddress = AbsoluteAddress;
                OutputAddressToPins(AddressBusAddress);
            }
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

                    P |= StatusRegister.I;
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
                    P |= StatusRegister.B;
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
                    _opCodeInProgress = false;
                    P &= ~StatusRegister.B;
                    break;
            }

            _instructionCycles++;
        }

        /// <summary>
        /// Branch on Overflow Clear
        /// </summary>
        public void BVC()
        {
        }

        /// <summary>
        /// Branch on Overflow Set
        /// </summary>
        public void BVS()
        {
        }

        /// <summary>
        /// Clear Carry Flag
        /// </summary>
        public void CLC()
        {
            P &= ~StatusRegister.C;
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
        public void CMP(AddressingMode mode)
        {
            BeginOpCode();

            if (_addressingModeInProgress)
            {
                _addressingMethodLookup[mode]();
            }
            else
            {
                switch (_instructionCycles)
                {
                    case (0):
                        var comparison = A - DataBusData;
                        P.Set(StatusRegister.C, A >= DataBusData);
                        P.Set(StatusRegister.Z, (comparison & 0x00FF) == 0);
                        P.Set(StatusRegister.N, (comparison & 0x0080) == 0);
                        _opCodeInProgress = false;
                        break;
                }
            }
        }


        /// <summary>
        /// Compare Memory and Index X
        /// </summary>
        public void CPX()
        {
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
        public void JMP(AddressingMode mode)
        {
            BeginOpCode();

            if (_addressingModeInProgress)
            {
                _addressingMethodLookup[mode]();
            }

            if (_addressingModeInProgress) return;

            AddressBusAddress = (ushort) (InstructionHiByte << 8 | InstructionLoByte);
            PC = AddressBusAddress;
            OutputAddressToPins(AddressBusAddress);
            _opCodeInProgress = false;
        }

        /// <summary>
        /// Jump to New Location
        /// Saving Return Address
        /// </summary>
        public void JSR()
        {
        }

        /// <summary>
        /// Load the Accumulator with Memory
        /// </summary>
        public void LDA()
        {
        }

        /// <summary>
        /// Load Index X with Memory
        /// </summary>
        public void LDX(AddressingMode mode)
        {
            BeginOpCode();

            if (_addressingModeInProgress)
            {
                _addressingMethodLookup[mode]();
            }

            if (_addressingModeInProgress) return;

            X = DataBusData;
            _opCodeInProgress = false;
            AddressBusAddress++;
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
        }

        /// <summary>
        /// OR Memory with Accumulator
        /// </summary>
        public void ORA(AddressingMode mode)
        {
            BeginOpCode();

            if (_addressingModeInProgress)
            {
                _addressingMethodLookup[mode]();
            }
            else
            {
                var data = OpCodeData;
                A = (byte) (A | data);

                if (A == 0x00)
                    P |= StatusRegister.Z;
                else
                    P &= ~StatusRegister.Z;

                if ((A & 0x80) == 1)
                    P |= StatusRegister.N;
                else
                    P &= ~StatusRegister.N;

                _opCodeInProgress = false;
            }
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
            BeginOpCode();

            switch (_instructionCycles)
            {
                case 0:
                {
                    CpuRead = false;
                    UpdateRw();
                    AddressBusAddress = (ushort) (0x0100 + SP);
                    DataBusData = (byte) P.Set(StatusRegister.B, true);
                    OutputAddressToPins(AddressBusAddress);
                    SP--;
                    break;
                }
                case 1:
                {
                    CpuRead = true;
                    UpdateRw();
                    _opCodeInProgress = false;
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
        public void STA(AddressingMode mode)
        {
            BeginOpCode();

            if (_addressingModeInProgress)
            {
                _addressingMethodLookup[mode]();
            }
            else
            {
                CpuRead = false;
                UpdateRw();
                DataBusData = A;
                OutputDataToDatabus();
                _opCodeInProgress = false;
            }
        }

        /// <summary>
        /// Store Index X in Memory
        /// </summary>
        public void STX(AddressingMode mode)
        {
            BeginOpCode();

            if (_addressingModeInProgress)
                _addressingMethodLookup[mode]();
            
            if(_addressingModeInProgress) return;
            
            CpuRead = false;
            UpdateRw();
            OutputAddressToPins(OpCodeData);
            DataBusData = X;
            OutputDataToDatabus();
            _opCodeInProgress = false;
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

        public void SLO(AddressingMode mode)
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
        public void RDY()
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
        public void PWR()
        {
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

            if (!_opCodeInProgress)
            {
                PC = AddressBusAddress;
            }

            //Set the unused Flag
            P.Set(StatusRegister.Reserved, true);

            Fetch();
            Execute();

            if (!CpuRead) //Write any data required to the address bus
            {
                OutputDataToDatabus();
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
            P = StatusRegister.Reserved;


            //output the address to the address bus
            //so that, on the next cycle, data can be picked up
            //outputted from ram/rom
            OutputAddressToPins(AddressBusAddress);
            UpdateRw();
        }

        private void Fetch()
        {
            if (!_opCodeInProgress)
                OpCode = DataBusData;
        }

        private void DecodePc()
        {
        }


        private void Execute()
        {
            switch (OpCode)
            {
                case 0x00: //BRK 5 cycles
                {
                    BRK();
                    break;
                }
                case 0x01: // ORA X, Indirect
                {
                    ORA(AddressingMode.Indirect);
                    break;
                }
                case 0x05: // ORA, Zero Page
                    ORA(AddressingMode.ZeroPage);
                    break;
                case 0x06:
                    ASL(AddressingMode.ZeroPage);
                    break;
                case 0x07: //Illegal Opcode
                    SLO(AddressingMode.ZeroPage);
                    break;
                case 0x08: //PHP Implied
                    PHP();
                    break;
                case 0x09: //ORA, Immediate
                    ORA(AddressingMode.Immediate);
                    break;
                case 0x0A: //ASL Accumulator
                    ASL(AddressingMode.Accumulator);
                    break;
                case 0x0D: //ORA Absolute
                    ORA(AddressingMode.Absolute);
                    break;
                case 0xC5: //CMP ZP
                    CMP(AddressingMode.ZeroPage);
                    break;
                case 0xF5: //SBC ZeroPage X:
                    CMP(AddressingMode.ZeroPageX);
                    break;
                case 0x85:
                    STA(AddressingMode.ZeroPage);
                    break;
                case 0x4C:
                    JMP(AddressingMode.Absolute);
                    break;
                case 0xA2:
                    LDX(AddressingMode.Immediate);
                    break;
                case 0x86:
                    STX(AddressingMode.ZeroPage);
                    break;
            }
        }

        private void BeginOpCode()
        {
            if (!_opCodeInProgress)
            {
                _opCodeInProgress = true;
                _addressingModeInProgress = true;
                _addressingModeCycles = 0;
            }
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

        #endregion
    }
}
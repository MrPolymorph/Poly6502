using System;
using Poly6502.DataDelivery;
using Poly6502.Extensions;
using Poly6502.Flags;
using Poly6502.Utilities;

namespace Poly6502
{
    /// <summary>
    /// The 6502 is a little-endian 8-bit processor with a 16-bit address bus.
    ///
    /// The 6502 typically runs at 1 to 2Mhz
    /// </summary>
    public class Processor : AbstractAddressDataBus
    {
        #region Main Registers

        /// <summary>
        /// Accumulator
        /// </summary>
        public byte A { get; private set; }

        #endregion

        #region IndexRegisters

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

        #endregion

        #region ProgramCounter

        /// <summary>
        /// Program Counter
        /// </summary>
        public ushort Pc { get; private set; }

        #endregion


        #region StatusRegister

        /// <summary>
        /// Status Register / Processor Flags
        /// </summary>
        public StatusRegister P { get; private set; }

        #endregion

        private byte _opCode;
        private ushort _indirectAddress;
        private ushort _absoluteAddress;

        #region Hardware Emulation

        private bool _pcFetchComplete;
        private bool _addressingModeInProgress;
        private bool _opCodeInProgress;
        private byte _instructionLoByte;
        private byte _instructionHiByte;
        private int _instructionCycles;
        private int _addressingModeCycles;
        private int _pcCurrentFetchCycle;

        public float InputVoltage { get; private set; }

        #endregion

        public Processor()
        {
            _pcCurrentFetchCycle = 2;
            _indirectAddress = 0;
            _addressingModeCycles = 0;
            _addressingModeInProgress = false;
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
                /*
                 * Set address bus to the program counter
                 */
                case 2:
                    _addressBusAddress = Pc;
                    Pc++;
                    break;
                /*
                 * Read the absolute address from the
                 * data bus
                 *
                 * complete address mode
                 */
                case 1:
                    _addressBusAddress = (ushort) (_dataBusData & 0x00FF);
                    _addressingModeInProgress = false;
                    break;
            }

            _addressingModeCycles--;
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
                case 5:
                    _addressBusAddress = Pc;
                    OutputAddressToPins();
                    break;
                /*
                 * Save data from the data bus
                 * increment PC
                 * setup address to read from memory at PC
                 */
                case 4:
                    _instructionLoByte = _dataBusData;
                    Pc++;
                    _addressBusAddress = Pc;
                    OutputAddressToPins();
                    break;
                /*
                 * Save data from the data bus
                 * save the pointer to (_indirectAddress)
                 * increment PC
                 * setup address to read from _indirectAddress
                 */
                case 3:
                    _instructionHiByte = _dataBusData;
                    _indirectAddress = (ushort) ((_instructionHiByte << 8) | _instructionLoByte);

                    Pc++;
                    if (_instructionLoByte == 0x00FF)
                        _addressBusAddress = (ushort) (_indirectAddress & 0xFF00);
                    else
                        _addressBusAddress = (ushort) (_indirectAddress + 1);

                    OutputAddressToPins();
                    break;
                /*
                 * Save data from the data bus
                 * Setup address to read from _indirectAddress
                 */
                case 2:
                    _absoluteAddress = (ushort) (_dataBusData << 8);
                    _addressBusAddress = _indirectAddress;
                    OutputAddressToPins();
                    break;
                /*
                 * Combine all answers to give final absolute address.
                 * Setup the address to read from that location.
                 */
                case 1:
                    _addressBusAddress = (ushort) (_absoluteAddress | _dataBusData);
                    OutputDataBus();
                    _addressingModeInProgress = false;
                    break;
            }

            _addressingModeCycles--;
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
        /// Shift left One Bit(Memory or Accumulator)
        /// </summary>
        public void ASL(AddressingMode mode)
        {
            if (!_opCodeInProgress)
            {
                _opCodeInProgress = true;
                _instructionCycles = 0;

                switch (mode)
                {
                    case AddressingMode.ZeroPage:
                        _addressingModeInProgress = true;
                        _addressingModeCycles = 2;
                        break;
                }
            }

            if (_addressingModeInProgress)
            {
                switch (mode)
                {
                    case AddressingMode.ZeroPage:
                        ZPA();
                        break;
                }
            }
            else
            {
                P.Set(StatusRegister.C, (_dataBusData & 0xFF00) > 0);
                P.Set(StatusRegister.Z, (_dataBusData & 0x00FF) == 0);
                P.Set(StatusRegister.N, (_dataBusData & 0x80) == 1);

                if (mode == AddressingMode.Implied)
                    A = (byte) (_dataBusData & 0x00FF);
                else
                    _instructionCycles++;

                switch (_instructionCycles)
                {
                    case 1:
                        _cpuRead = false;
                        UpdateRw();
                        OutputAddressToPins();
                        _dataBusData &= 0x00FF;
                        break;
                    case 0:
                        _opCodeInProgress = false;
                        _cpuRead = true;
                        UpdateRw();
                        break;
                }
                
            }

            _instructionCycles--;
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
            if (!_opCodeInProgress)
            {
                _opCodeInProgress = true;
                _instructionCycles = 5;
            }
            
            switch (_instructionCycles)
            {
                /*
                 * Tell everyone we are writing to the data bus
                 * set the address bus to address on the stack.
                 * Write the hi byte of the program counter to the data bus
                 * decrement stack pointer for next operation
                 */
                case(5):
                    _cpuRead = false;
                    UpdateRw();
                    _addressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins();
                    _dataBusData = (byte) ((Pc >> 8) & 0x00FF);
                    SP--;

                    P |= StatusRegister.I;
                    break;
                /*
                 * Set the address bus to the address on the stack.
                 * Write the lo byte of the program counter to the data bus
                 * decrement the stack pointer for next operation
                 * 
                 */
                case(4):
                    _addressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins();
                    _dataBusData = (byte) (Pc & 0x00FF);
                    SP--;
                    break;
                /*
                 * Set the address bus to the address on the stack
                 * Write the current status register to the data bus
                 * decrement the stack pointer for next operation
                 */
                case(3):
                    _addressBusAddress = (ushort) (0x0100 + SP);
                    OutputAddressToPins();
                    _dataBusData = (byte) P;
                    P |= StatusRegister.B;
                    SP--;
                    break;
                /*
                 * Tell everyone we now want to read
                 * Set the address bus to address 0xFFFE
                 */
                case(2):
                    _cpuRead = true;
                    UpdateRw();
                    _addressBusAddress = 0xFFFE;
                    OutputAddressToPins();
                    break;
                /*
                 * read the data from the databus into the PC.
                 * Set the address bus to address 0xFFFF
                 */
                case(1):
                    Pc = 0x0000;
                    Pc = _dataBusData;
                    _addressBusAddress = 0xFFFF;
                    OutputAddressToPins();
                    break;
                /*
                 * Read the data bus into the hi byte of the PC
                 * Opcode Complete
                 */
                case(0):
                    Pc = (ushort) (_dataBusData << 8);
                    _opCodeInProgress = false;
                    P &= ~StatusRegister.B;
                    break;
            }

            _instructionCycles--;
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
        public void CMP()
        {
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
        public void JMP(AddressingMode addressMode)
        {
            if (!_opCodeInProgress)
            {
                _opCodeInProgress = true;
                
            }
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
        public void LDX()
        {
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
            if (!_opCodeInProgress)
            {
                _opCodeInProgress = true;
                _addressingModeInProgress = true;
                _instructionCycles = 2;

                switch (mode)
                {
                    case AddressingMode.Indirect:
                        _addressingModeCycles = 5;
                        break;
                    case AddressingMode.ZeroPage:
                        _addressingModeCycles = 2;
                        break;
                }

            }

            if (_addressingModeInProgress)
            {
                switch (mode)
                {
                    case AddressingMode.Indirect:
                        IND();
                        break;
                    case AddressingMode.ZeroPage:
                        ZPA();
                        break;
                }
            }
            else
            {
                var data = _dataBusData;
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
            if (!_opCodeInProgress)
            {
                _opCodeInProgress = true;
                _instructionCycles = 2;
            }

            switch (_instructionCycles)
            {
                case 2:
                {
                    _cpuRead = false;
                    UpdateRw();
                    _addressBusAddress = (ushort) (0x0100 + SP);
                    _dataBusData = (byte) P.Set(StatusRegister.B, true);
                    OutputAddressToPins();
                    SP--;
                    break;
                }
                case 1:
                {
                    _cpuRead = true;
                    UpdateRw();
                    _opCodeInProgress = false;
                    break;
                }
            }

            _instructionCycles--;
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
        public void STA()
        {
        }

        /// <summary>
        /// Store Index X in Memory
        /// </summary>
        public void STX()
        {
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
        /// Pin 34
        ///
        /// <remarks>
        /// 0 = Write
        /// 1 = Read
        /// </remarks>
        /// </summary>
        public void RW(float inputVoltage)
        {
            _cpuRead = inputVoltage >= 3.1;
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
            if (!_opCodeInProgress)
            {
                Pc++;
            }
            
            //Set the unused Flag
            P |= StatusRegister.Reserved;

            Fetch();
            Execute();
            
            OutputAddressToPins();
            
            if (!_cpuRead) //Write any data required to the address bus
            {
                OutputDataBus();
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
            _cpuRead = true;
            Pc = 0xFFFC;
            SP = 0xFD;
            //On a reset, the cpu will look at
            //memory location 0xFFFC for an opcode to run
            //this will take two cycles
            //one for the lo byte, one for the hi byte.
            _addressBusAddress = 0xFFFC;
            _pcFetchComplete = false;
            _instructionCycles = 2;

            //output the address to the address bus
            //so that, on the next cycle, data can be picked up
            //outputted from ram/rom
            OutputAddressToPins();
            UpdateRw();
        }

        private void Fetch()
        {
            //we must decode the opcode by fetching
            //the data at pc and pc+1 which gives us our 16 bit address
            //this takes 2 cycles.

            if (_pcCurrentFetchCycle == 2 && !_pcFetchComplete) // our first cycle is the lo byte
            {
                _instructionLoByte = _dataBusData;
                _pcCurrentFetchCycle--;
            }

            if (_pcCurrentFetchCycle == 1 && !_pcFetchComplete) // our second cycle is the hi byte
            {
                _instructionHiByte = _dataBusData;
                //We can construct our program counter with this data now.
                //the 6502 is little endian, so we need to swap the 
                //bytes around.
                Pc = (ushort) (_instructionHiByte << 8 | _instructionLoByte);
                _pcFetchComplete = true;
                _pcCurrentFetchCycle--;
            }

            if (_pcFetchComplete && !_opCodeInProgress)
                _opCode = _dataBusData;
        }

        private void DecodePc()
        {
        }


        private void Execute()
        {
            if (!_pcFetchComplete) //the program has not finished getting the pc
                return;

            switch (_opCode)
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
                case 0x08:
                    PHP();
                    break;

            }
        }

        private void OutputAddressToPins()
        {
            //Address bus is uni directional.
            //Tell everything connected to the address bus the address
            foreach (var device in _addressCompatibleDevices)
            {
                for (var i = 0; i < device.AddressBusLines.Count; i++)
                {
                    var pw = Math.Pow(2, i);
                    var data = ((_addressBusAddress & (ushort) pw) >> i);
                    device.AddressBusLines[i](data);
                }
            }
        }

        private void OutputDataBus()
        {
            for (int i = 0; i < DataBusLines.Count; i++)
            {
                var pw = Math.Pow(2, i);
                var data = ((_dataBusData & (byte) pw) >> i);
                DataBusLines[i](data);
            }
        }

        private void UpdateRw()
        {
            foreach (var busDevices in _dataCompatibleDevices)
            {
                busDevices.SetRW(_cpuRead);
            }
        }

        #endregion
    }
}
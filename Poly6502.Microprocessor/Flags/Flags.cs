using System;

namespace Poly6502.Microprocessor.Flags
{
    public class StatusRegister
    {
        /// <summary>
        /// Carry Flag: Enables numbers larger than a single word to be
        /// added/subtracted by carrying a binary digit from a less significant
        /// word to the least significant bit of a more significant word ad needed.
        ///
        /// It is also used to extend bit shifts and rotates in a similar manner on many
        /// processors
        /// <remarks>
        /// 1 = True
        /// </remarks>
        /// </summary>
        private bool C;

        /// <summary>
        /// Zero Flag: Indicates that the result of the arithmetic or logical operation
        /// (or, sometimes, a load) was zero.
        ///
        /// <remarks>
        /// 1 = result zero
        /// </remarks>
        /// </summary>
        private bool Z;

        /// <summary>
        /// Interrupt Flag: This bit indicates if interrupts are enabled or masked.
        ///
        /// <remarks>
        /// 1 = Disabled.
        /// 
        /// The interrupt flag is used to prevent <see cref="M6502.SEI"/> or enable <see cref="M6502.CLI"/>
        /// maskable interrupts (aka IRQ's). It does not signal the presence or absence of an interrupt condition.
        ///
        /// The 6502 will set this flag automatically in response to an interrupt and restore it to its prior status
        /// on completion of the interrupt service routine. If you want your interrupt service routine to permit other
        /// maskable interrupts, you must clear the <see cref="I"/> flag in your code.
        /// </remarks>
        /// </summary>
        private bool I;

        /// <summary>
        /// Decimal Flag:
        ///
        /// <remarks>
        /// 1 = true.
        ///
        /// The decimal flag controls how the 6502 adds and subtracts. If set, arithmetic is carried out
        /// in BCD. This flag is unchanged by interrupts and is unknown on power-up. The implication is that a CLD should
        /// be included in boot or interrupt coding.
        /// </remarks>
        /// </summary>
        private bool D;

        /// <summary>
        /// Break Flag:
        /// </summary>
        private bool B;

        private bool Reserved;

        /// <summary>
        /// Overflow Flag: Indicates that the signed result of an operation is too large
        /// to fit in the register width using two's compliment representation.
        ///
        /// <remarks>
        /// 1 = true
        /// </remarks>
        /// </summary>
        private bool V;

        /// <summary>
        /// Negative Flag: Indicates that the result of a mathematical operation
        /// is negative.
        ///
        ///  <remarks>
        /// 1 = negative
        /// </remarks>
        /// </summary>
        private bool N;
        
        public byte Register
        {
            get
            {
                byte register = 0;

                register |= (byte) (N ? (1 << 7) : (0 << 7));
                register |= (byte) (V ? (1 << 6) : (0 << 6));
                register |= (byte) (Reserved ? (1 << 5) : (0 << 5));
                register |= (byte) (B ? (1 << 4) : (0 << 4));
                register |= (byte) (D ? (1 << 3) : (0 << 3));
                register |= (byte) (I ? (1 << 2) : (0 << 2));
                register |= (byte) (Z ? (1 << 1) : (0 << 1));
                register |= (byte)(C ? 1 : 0);
                return register;
            }
        }

        public bool HasFlag(StatusRegisterFlags flag)
        {
            switch (flag)
            {
                case StatusRegisterFlags.C:
                    return C;
                case StatusRegisterFlags.Z:
                    return Z;
                case StatusRegisterFlags.I:
                    return I;
                case StatusRegisterFlags.D:
                    return D;
                case StatusRegisterFlags.B:
                    return B;
                case StatusRegisterFlags.Reserved:
                    return Reserved;
                case StatusRegisterFlags.V:
                    return V;
                case StatusRegisterFlags.N:
                    return N;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
            }
        }

        public void SetFlag(byte b)
        {
            C = (b & (1 << 0)) != 0;
            Z = (b & (1 << 1)) != 0;
            I = (b & (1 << 2)) != 0;
            D = (b & (1 << 3)) != 0;
            B = (b & (1 << 4)) != 0;
            Reserved = (b & (1 << 5)) != 0;
            V = (b & (1 << 6)) != 0;
            N = (b & (1 << 7)) != 0;
        }
        
        public void SetFlag(StatusRegisterFlags flag, bool set = true)
        {
            switch (flag)
            {
                case StatusRegisterFlags.C:
                    C = set;
                    break;
                case StatusRegisterFlags.Z:
                    Z = set;
                    break;
                case StatusRegisterFlags.I:
                    I = set;
                    break;
                case StatusRegisterFlags.D:
                    D = set;
                    break;
                case StatusRegisterFlags.B:
                    B = set;
                    break;
                case StatusRegisterFlags.Reserved:
                    Reserved = set;
                    break;
                case StatusRegisterFlags.V:
                    V = set;
                    break;
                case StatusRegisterFlags.N:
                    N = set;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
            }
        }

        public override string ToString()
        {
            return $"C {(C ? 1:0)} | Z {(Z ? 1:0)} | I {(I ? 1:0)} | D {(D ? 1:0)} | B {(B ? 1:0)} | R 0 | V {(V ? 1:0)} | N {(N ? 1:0)} | Register: 0x{Register:X2}";
        }
    }

    public enum StatusRegisterFlags
    {
        /// <summary>
        /// Carry Flag: Enables numbers larger than a single word to be
        /// added/subtracted by carrying a binary digit from a less significant
        /// word to the least significant bit of a more significant word ad needed.
        ///
        /// It is also used to extend bit shifts and rotates in a similar manner on many
        /// processors
        /// <remarks>
        /// 1 = True
        /// </remarks>
        /// </summary>
        C,

        /// <summary>
        /// Zero Flag: Indicates that the result of the arithmetic or logical operation
        /// (or, sometimes, a load) was zero.
        ///
        /// <remarks>
        /// 1 = result zero
        /// </remarks>
        /// </summary>
        Z,

        /// <summary>
        /// Interrupt Flag: This bit indicates if interrupts are enabled or masked.
        ///
        /// <remarks>
        /// 1 = Disabled.
        /// 
        /// The interrupt flag is used to prevent <see cref="M6502.SEI"/> or enable <see cref="M6502.CLI"/>
        /// maskable interrupts (aka IRQ's). It does not signal the presence or absence of an interrupt condition.
        ///
        /// The 6502 will set this flag automatically in response to an interrupt and restore it to its prior status
        /// on completion of the interrupt service routine. If you want your interrupt service routine to permit other
        /// maskable interrupts, you must clear the <see cref="I"/> flag in your code.
        /// </remarks>
        /// </summary>
        I,

        /// <summary>
        /// Decimal Flag:
        ///
        /// <remarks>
        /// 1 = true.
        ///
        /// The decimal flag controls how the 6502 adds and subtracts. If set, arithmetic is carried out
        /// in BCD. This flag is unchanged by interrupts and is unknown on power-up. The implication is that a CLD should
        /// be included in boot or interrupt coding.
        /// </remarks>
        /// </summary>
        D,

        /// <summary>
        /// Break Flag:
        /// </summary>
        B,

        Reserved,

        /// <summary>
        /// Overflow Flag: Indicates that the signed result of an operation is too large
        /// to fit in the register width using two's compliment representation.
        ///
        /// <remarks>
        /// 1 = true.
        ///
        /// After an <see cref="M6502.ADC"/> or <see cref="M6502.SBC"/> instruction, the overflow flag will be set if
        /// the 2's compliment result is less than -128 or greater than +127, and it will be cleared otherwise.
        ///
        /// </remarks>
        /// </summary>
        V,

        /// <summary>
        /// Negative Flag: Indicates that the result of a mathematical operation
        /// is negative.
        ///
        ///  <remarks>
        /// 1 = negative
        /// </remarks>
        /// </summary>
        N,
    }
}
using System;

namespace Poly6502.Microprocessor.Flags
{
    [Flags]
    public enum StatusRegister
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
        C = 1 << 0,
        /// <summary>
        /// Zero Flag: Indicates that the result of the arithmetic or logical operation
        /// (or, sometimes, a load) was zero.
        ///
        /// <remarks>
        /// 1 = result zero
        /// </remarks>
        /// </summary>
        Z = 1 << 1,
        /// <summary>
        /// Interrupt Flag: This bit indicates if interrupts are enabled or masked.
        ///
        /// <remarks>
        /// 1 = Disabled
        /// </remarks>
        /// </summary>
        I = 1 << 2,
        /// <summary>
        /// Decimal Flag:
        ///
        /// <remarks>
        /// 1 = true
        /// </remarks>
        /// </summary>
        D = 1 << 3,
        /// <summary>
        /// Break Flag:
        /// </summary>
        B = 1 << 4,
        Reserved = 1 << 5,
        /// <summary>
        /// Overflow Flag: Indicates that the signed result of an operation is too large
        /// to fit in the register width using two's compliment representation.
        ///
        /// <remarks>
        /// 1 = true
        /// </remarks>
        /// </summary>
        V = 1 << 6,
        /// <summary>
        /// Negative Flag: Indicates that the result of a mathematical operation
        /// is negative.
        ///
        ///  <remarks>
        /// 1 = negative
        /// </remarks>
        /// </summary>
        N = 1 << 7
    }
}
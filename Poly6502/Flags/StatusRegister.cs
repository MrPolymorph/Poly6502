using System;

namespace Poly6502.Flags
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
        C = 0x0,
        /// <summary>
        /// Zero Flag: Indicates that the result of the arithmetic or logical operation
        /// (or, sometimes, a load) was zero.
        ///
        /// <remarks>
        /// 1 = result zero
        /// </remarks>
        /// </summary>
        Z = 0x1,
        /// <summary>
        /// Interrupt Flag: This bit indicates if interrupts are enabled or masked.
        ///
        /// <remarks>
        /// 1 = Disabled
        /// </remarks>
        /// </summary>
        I = 0x2,
        /// <summary>
        /// Decimal Flag:
        ///
        /// <remarks>
        /// 1 = true
        /// </remarks>
        /// </summary>
        D = 0x3,
        /// <summary>
        /// Break Flag:
        /// </summary>
        B = 0x4,
        Reserved = 0x5,
        /// <summary>
        /// Overflow Flag: Indicates that the signed result of an operation is too large
        /// to fit in the register width using two's compliment representation.
        ///
        /// <remarks>
        /// 1 = true
        /// </remarks>
        /// </summary>
        V = 0x6,
        /// <summary>
        /// Negative Flag: Indicates that the result of a mathematical operation
        /// is negative.
        ///
        ///  <remarks>
        /// 1 = negative
        /// </remarks>
        /// </summary>
        N = 0x7
    }
}
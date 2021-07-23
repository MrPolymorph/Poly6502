using Poly6502.Microprocessor.Flags;

namespace Poly6502.Microprocessor.Extensions
{
    public static class FlagExtensions
    {
        public static StatusRegister Set(this StatusRegister flag, StatusRegister flagToSet, bool setFlag)
        {
            if (setFlag)
                flag |= flagToSet;
            else
                flag &= ~flagToSet;

            return flag;
        }
    }
}
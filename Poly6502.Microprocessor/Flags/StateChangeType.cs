using System.ComponentModel;
using System.Reflection;

namespace Poly6502.Microprocessor.Flags
{
    public enum StateChangeType
    {
        [Description("Program Counter")]
        ProgramCounter = 0x1,
        [Description("Stack Pointer")]
        StackPointer = 0x2,
        [Description("A Register")]
        ARegister = 0x3,
        [Description("X Register")]
        XRegister = 0x4,
        [Description("YRegister")]
        YRegister = 0x5,
        [Description("Status Register")]
        StatusRegister = 0x6,
        [Description("Op Code")]
        Op = 0x7,
    }
}
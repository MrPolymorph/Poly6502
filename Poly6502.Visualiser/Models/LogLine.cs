using Poly6502.Microprocessor.Flags;

namespace Poly6502.Visualiser.Models
{
    public class LogLine
    {
        public ushort ProgramCounter { get; set; }
        public byte OpCode { get; set; }
        public byte LoByte { get; set; }
        public byte HiByte { get; set; }
        public string OpCodeName { get; set; }
        public StatusRegister Flags { get; set; }

        public bool Compare(byte opCode, byte data1, byte data2, StatusRegister P)
        {
            return OpCode == opCode && data1 == LoByte && data2 == HiByte && P == Flags;
        }
    }
}
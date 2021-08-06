using Poly6502.Microprocessor.Flags;

namespace Poly6502.Visualiser.Models
{
    public class LogLine
    {
        public ushort ProgramCounter { get; set; }
        public byte OpCode { get; set; }
        public byte Data1 { get; set; }
        public byte Data2 { get; set; }
        public string OpCodeName { get; set; }
        public StatusRegister Flags { get; set; }

        public bool Compare(byte opCode, byte data1, byte data2)
        {
            return OpCode == opCode && Data1 == data1 && Data2 == data2;
        }
    }
}
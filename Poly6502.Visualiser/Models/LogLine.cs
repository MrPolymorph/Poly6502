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
        public byte Flags { get; set; }
        public byte A { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }

        public bool Compare(LogLine log)
        {
            return ProgramCounter == log.ProgramCounter &&
                   OpCode == log.OpCode &&
                   // LoByte == log.LoByte &&
                   // HiByte == log.HiByte &&
                   OpCodeName == log.OpCodeName &&
                   Flags == log.Flags &&
                   A == log.A &&
                   X == log.X &&
                   Y == log.Y;
        }
    }
}
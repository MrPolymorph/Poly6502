namespace Poly6502.Microprocessor.Tests.Models
{
    public class CycleTruthData
    {
        public bool BoundaryCrossable { get; }
        public byte OpCode { get; }
        public int Cycles { get; }
        public int MaxPotentialCycles { get; }

        public CycleTruthData(byte opCode, int cycles, bool boundaryCrossable = false)
        {
            OpCode = opCode;
            Cycles = cycles;
            BoundaryCrossable = boundaryCrossable;
            
            MaxPotentialCycles = Cycles + (boundaryCrossable ? 1 : 0);
        }
    }
}
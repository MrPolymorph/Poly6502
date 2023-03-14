namespace Poly6502.Microprocessor.Flags;

public class ProcessorRegisters
{
    public byte A { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte SP { get; set; }
    public ushort PC { get; set; }
    public StatusRegister P { get; set; }

    public void Update(byte a, byte x, byte y, byte sp, ushort pc, StatusRegister p)
    {
        A = a;
        X = x;
        Y = y;
        SP = sp;
        PC = pc;
        P = p;
    }
}
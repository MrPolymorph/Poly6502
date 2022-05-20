using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;

namespace Poly6502.Microprocessor.Tests.CycleTiming;

public static class CycleTimingTester
{
    public static void TestOpcode(M6502 m6502, Mock<IDataBusCompatible> mockRam, byte opcode, Operation op)
    {
        int clocked = 0;
        
        m6502.Pc = 0xC000;

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(), false))
            .Returns(opcode)
            .Returns(0x05);

        do
        {
            m6502.Clock();    
            
            clocked++;
            
            if(clocked > op.MachineCycles)
                Assert.Fail($"op 0x{opcode:x2} failed as it took {clocked} and was expected to take {op.MachineCycles}");
            
        } while (clocked != op.MachineCycles);
            
        Assert.IsTrue(m6502.FetchInstruction, $"opcode 0x{opcode:x2} did not finish completing. Cycles Taken : {m6502.CurrentTotalCyclesTaken}, Expected to Take {op.MachineCycles}");
    }
}
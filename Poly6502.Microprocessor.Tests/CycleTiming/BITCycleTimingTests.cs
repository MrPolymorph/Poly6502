using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class BitCycleTimingTests
    {
        [Test]
        [TestCase((byte)0x24)]
        [TestCase((byte)0x2C)]
        public void Test_BIT_Cycle_Timing(byte opcode)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);
            
            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.BIT));

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
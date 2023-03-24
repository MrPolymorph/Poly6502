using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class AndCycleTimingTests
    {
        [Test]
        [TestCase((byte)0x29)]
        [TestCase((byte)0x25)]
        [TestCase((byte)0x35)]
        [TestCase((byte)0x2D)]
        [TestCase((byte)0x3D, true)]
        [TestCase((byte)0x39, true)]
        [TestCase((byte)0x21)]
        [TestCase((byte)0x31, true)]
        public void Test_AND_Cycle_Timing(byte opcode, bool crossesBoundary = false)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);
            
            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.AND));

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
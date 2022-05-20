using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class AslCycleTimingTests
    {
        [Test]
        [TestCase((byte)0x0A)]
        [TestCase((byte)0x06)]
        [TestCase((byte)0x16)]
        [TestCase((byte)0x0E)]
        [TestCase((byte)0x1E)]
        public void Test_ASL_Cycle_Timing(byte opcode)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);
            
            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.ASL));
            
            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
}
}
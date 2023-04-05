using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Microprocessor.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class LDYCycleTimingTests
    {
        [Test]
        [TestCase((byte)0xA0)]
        [TestCase((byte)0xA4)]
        [TestCase((byte)0xB4)]
        [TestCase((byte)0xAC)]
        [TestCase((byte)0xBC, true)]
        public void Test_LDY_Cycle_Timing(byte opcode, bool boundaryCrossable = false)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);

            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.LDY));

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Microprocessor.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class ORACycleTimingTests
    {
        [Test]
        [TestCase((byte)0x09)]
        [TestCase((byte)0x05)]
        [TestCase((byte)0x15)]
        [TestCase((byte)0x0D)]
        [TestCase((byte)0x1D, true)]
        [TestCase((byte)0x19, true)]
        [TestCase((byte)0x01)]
        [TestCase((byte)0x11, true)]
        public void Test_ORA_Cycle_Timing(byte opcode, bool boundaryCrossable = false)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);

            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.ORA));

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
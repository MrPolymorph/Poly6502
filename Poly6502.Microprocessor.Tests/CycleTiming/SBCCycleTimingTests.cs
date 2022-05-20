using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class SBCCycleTimingTests
    {
        [Test]
        [TestCase((byte)0xE9)]
        [TestCase((byte)0xE5)]
        [TestCase((byte)0xF5)]
        [TestCase((byte)0xED)]
        [TestCase((byte)0xFD, true)]
        [TestCase((byte)0xF9, true)]
        [TestCase((byte)0xE1)]
        [TestCase((byte)0xF1, true)]
        public void Test_SBC_Cycle_Timing(byte opcode, bool boundaryCrossable = false)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);

            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.SBC));

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class RORCycleTimingTests
    {
        [Test]
        [TestCase((byte)0x6A)]
        [TestCase((byte)0x66)]
        [TestCase((byte)0x76)]
        [TestCase((byte)0x6E)]
        [TestCase((byte)0x7E)]
        public void Test_ROR_Cycle_Timing(byte opcode)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);

            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.ROR));

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
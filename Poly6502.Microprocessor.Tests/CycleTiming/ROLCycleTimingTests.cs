using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;
using static NUnit.Framework.Assert;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class ROLCycleTimingTests
    {
        [Test]
        [TestCase((byte)0x2A)]
        [TestCase((byte)0x26)]
        [TestCase((byte)0x36)]
        [TestCase((byte)0x2E)]
        [TestCase((byte)0x3E)]
        public void Test_ROL_Cycle_Timing(byte opcode)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);

            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.ROL));

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
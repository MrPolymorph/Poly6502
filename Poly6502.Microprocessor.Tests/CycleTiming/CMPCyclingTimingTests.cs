using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class CMPCyclingTimingTests
    {
        [Test]
        [TestCase((byte)0xC9)]
        [TestCase((byte)0xC5)]
        [TestCase((byte)0xD5)]
        [TestCase((byte)0xCD)]
        [TestCase((byte)0xDD, true)]
        [TestCase((byte)0xD9, true)]
        [TestCase((byte)0xC1)]
        [TestCase((byte)0xD1, true)]
        public void Test_CMP_Cycle_Timing(byte opcode, bool boundaryCrossable = false)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);
            
            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.CMP));

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
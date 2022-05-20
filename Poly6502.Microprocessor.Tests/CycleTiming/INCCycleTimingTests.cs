using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class INCCycleTimingTests
    {
        [Test]
        [TestCase((byte)0xE6)]
        [TestCase((byte)0xF6)]
        [TestCase((byte)0xEE)]
        [TestCase((byte)0xFE)]
        public void Test_INC_Cycle_Timing(byte opcode)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);
            
            Operation op = m6502.OpCodeLookupTable[opcode];
            
            Assert.IsTrue(op.OpCodeCompare(m6502.INC));
            
            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
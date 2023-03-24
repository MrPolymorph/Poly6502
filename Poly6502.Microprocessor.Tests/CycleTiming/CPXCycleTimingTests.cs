using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class CPXCycleTimingTests
    {
        [Test]
        [TestCase((byte)0xE0)]
        [TestCase((byte)0xE4)]
        [TestCase((byte)0xEC)]
        public void Test_CPX_Cycle_Timing(byte opcode)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);
            
            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.CPX));
            
            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
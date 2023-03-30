using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Microprocessor.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class EORCycleTimingTests
    {
        [Test]
        [TestCase((byte)0x49)]
        [TestCase((byte)0x45)]
        [TestCase((byte)0x55)]
        [TestCase((byte)0x4D)]
        [TestCase((byte)0x5D, true)]
        [TestCase((byte)0x59, true)]
        [TestCase((byte)0x41)]
        [TestCase((byte)0x51, true)]
        public void Test_EOR_Cycle_Timing(byte opcode, bool boundaryCrossable = false)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);
            
            Operation op = m6502.OpCodeLookupTable[opcode];
            
            Assert.IsTrue(op.OpCodeCompare(m6502.EOR));
            
            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class STACycleTimingTests
    {
        [Test]
        [TestCase((byte)0x85)]
        [TestCase((byte)0x95)]
        [TestCase((byte)0x8D)]
        [TestCase((byte)0x9D, true)]
        [TestCase((byte)0x99, true)]
        [TestCase((byte)0x81)]
        [TestCase((byte)0x91, true)]
        
        public void Test_STA_Cycle_Timing(byte opcode, bool boundaryCrossable = false)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);

            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.STA));

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
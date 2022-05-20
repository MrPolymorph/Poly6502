using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class ADCCycleTimingTests
    {
        [Test]
        [TestCase((byte)0x69)]
        [TestCase((byte)0x65)]
        [TestCase((byte)0x75)]
        [TestCase((byte)0x6D)]
        [TestCase((byte)0x7D, true)]
        [TestCase((byte)0x79, true)]
        [TestCase((byte)0x61)]
        [TestCase((byte)0x71, true)]
        public void Test_ADC_Cycle_Timing(byte opcode, bool crossesBoundary = false)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);
            
            Operation op = m6502.OpCodeLookupTable[opcode];
            
            Assert.IsTrue(op.OpCodeCompare(m6502.ADC));
            
            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
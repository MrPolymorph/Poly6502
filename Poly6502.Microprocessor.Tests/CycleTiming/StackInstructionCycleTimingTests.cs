using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class StackInstructionCycleTimingTests
    {
        [Test]
        [TestCase((byte)0x9A)]
        [TestCase((byte)0xBA)]
        [TestCase((byte)0x68)]
        [TestCase((byte)0x08)]

        public void Test_StackInstructions_Cycle_Timing(byte opcode)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);

            Operation op = m6502.OpCodeLookupTable[opcode];

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class RegisterInstructionCycleTimingTests
    {
        [Test]
        [TestCase((byte)0xAA)]
        [TestCase((byte)0x8A)]
        [TestCase((byte)0xCA)]
        [TestCase((byte)0xE8)]
        [TestCase((byte)0xA8)]
        [TestCase((byte)0x98)]
        [TestCase((byte)0x88)]
        [TestCase((byte)0xC8)]
        public void Test_RegisterInstruction_Cycle_Timing(byte opcode)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);

            Operation op = m6502.OpCodeLookupTable[opcode];

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
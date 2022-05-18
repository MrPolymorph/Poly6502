using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class ASLCycleTimingTests
    {

        private M6502 _m6502;
        private Mock<IDataBusCompatible> _mockRam;

        [SetUp]
        public void Setup()
        {

            _m6502 = new M6502();
            _mockRam = new Mock<IDataBusCompatible>();

            _m6502.RegisterDevice(_mockRam.Object, 1);
        }

        [Test]
        [TestCase((byte)0x0A)]
        [TestCase((byte)0x06)]
        [TestCase((byte)0x16)]
        [TestCase((byte)0x0E)]
        [TestCase((byte)0x1E)]
        public void Test_ASL_Cycle_Timing(byte opcode)
        {

            Operation op = _m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(_m6502.ASL));

            _m6502.Pc = 0xC000;

            _mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(), false))
                .Returns(opcode)
                .Returns(0x05);


            _m6502.Fetch();
            
            do
            {
                _m6502.Execute();
            } while (_m6502.AddressingModeInProgress);

            do
            {
                _m6502.Execute();
            } while (_m6502.OpCodeInProgress);

            var takenCycles =
                (_m6502.PreviousInstructionCycleLength + _m6502.PreviousAddressingModeCycleLength);
            
            if (takenCycles != op.MachineCycles)
                Assert.Fail($"0x{opcode:x2} Expected {op.MachineCycles}. Actual : {takenCycles}");
            
            Console.WriteLine($"OpCode 0x{opcode:x2} Passed");
        }
}
}
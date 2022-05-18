using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class ANDCycleTimingTests
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
        [TestCase((byte)0x29)]
        [TestCase((byte)0x25)]
        [TestCase((byte)0x35)]
        [TestCase((byte)0x2D)]
        [TestCase((byte)0x3D, true)]
        [TestCase((byte)0x39, true)]
        [TestCase((byte)0x21)]
        [TestCase((byte)0x31, true)]
        public void Test_AND_Cycle_Timing(byte opcode, bool crossesBoundary = false)
        {
            Operation op = _m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(_m6502.AND));
            
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

            var takenCycles = (_m6502.PreviousInstructionCycleLength + _m6502.PreviousAddressingModeCycleLength);

            if (!crossesBoundary)
            {
                Assert.AreEqual(op.MachineCycles, takenCycles, $"opcode 0x{opcode:X2}", $"OpCode 0x{opcode:x2} Passed");
                Console.WriteLine($"OpCode {op.OpCodeMethod.Method.Name} 0x{opcode:x2} Passed");
            }
            else
            {
                if(takenCycles != op.MachineCycles && takenCycles != op.MachineCycles + 1)
                    Assert.Fail($"Expected {op.MachineCycles} or {op.MachineCycles + 1} cycles. Actual : {takenCycles}");
                else if (takenCycles == op.MachineCycles || takenCycles == op.MachineCycles + 1)
                {
                    Console.WriteLine($"OpCode 0x{opcode:x2} Passed");
                }
            }
        }
    }
}
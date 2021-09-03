using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class BRKCycleTimingTests
    { 
        private List<CycleTruthData> _truthData;

        private M6502 _m6502;
        private Mock<IDataBusCompatible> _mockRam;

        private void SetupTruthTable()
        {
            _truthData = new List<CycleTruthData>()
            {
                /* AND */
                /* Implied */ new CycleTruthData(0x00, 7),
            };
        }

        [SetUp]
        public void Setup()
        {
            SetupTruthTable();

            _m6502 = new M6502();
            _mockRam = new Mock<IDataBusCompatible>();

            _m6502.RegisterDevice(_mockRam.Object);
        }

        [Test]
        public void Test_BRK_Cycle_Timing()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var truth in _truthData)
            {
                Operation op = _m6502.OpCodeLookupTable[truth.OpCode];

                Assert.IsTrue(op.OpCodeCompare(_m6502.BRK));

                _m6502.PC = 0xC000;

                _mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>()))
                    .Returns(truth.OpCode)
                    .Returns(0x05);
                
                do
                {
                    _m6502.Fetch();
                } while (_m6502.AddressingModeInProgress);

                do
                {
                    _m6502.Execute();
                } while (_m6502.OpCodeInProgress);

                var takenCycles =
                    (_m6502.PreviousInstructionCycleLength + 1 + _m6502.PreviousAddressingModeCycleLength);

                if (!truth.BoundaryCrossable)
                {
                    Assert.AreEqual(truth.Cycles, takenCycles, $"opcode 0x{truth.OpCode:X2}",
                        $"OpCode 0x{truth.OpCode:x2} Passed");
                    Console.WriteLine($"OpCode 0x{truth.OpCode:x2} Passed");
                }
                else
                {
                    if (takenCycles != truth.Cycles && takenCycles != truth.MaxPotentialCycles)
                        Assert.Fail(
                            $"Expected {truth.Cycles} or {truth.MaxPotentialCycles} cycles. Actual : {takenCycles}");
                    else if (takenCycles == truth.Cycles || takenCycles == truth.MaxPotentialCycles)
                    {
                        Console.WriteLine($"OpCode 0x{truth.OpCode:x2} Passed");
                    }
                }
            }
        }
        
    }
}
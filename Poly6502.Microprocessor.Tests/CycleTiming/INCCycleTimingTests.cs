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
        private List<CycleTruthData> _truthData;

        private M6502 _m6502;
        private Mock<IDataBusCompatible> _mockRam;

        private void SetupTruthTable()
        {
            _truthData = new List<CycleTruthData>()
            {
                /* INC */
                /* Zero Page */   new CycleTruthData(0xE6, 5),
                /* Zero Page X */ new CycleTruthData(0xF6, 6),
                /* Absolute */    new CycleTruthData(0xEE, 6),
                /* Absolute X */  new CycleTruthData(0xFE, 7),
            };
        }

        [SetUp]
        public void Setup()
        {
            SetupTruthTable();

            _m6502 = new M6502();
            _mockRam = new Mock<IDataBusCompatible>();

            _m6502.RegisterDevice(_mockRam.Object, 1);
        }

        [Test]
        public void Test_INC_Cycle_Timing()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var truth in _truthData)
            {
                TestINC(truth.OpCode);

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


        public void TestINC(byte opcode)
        {
            Operation op = _m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(_m6502.INC));

            _m6502.Pc = 0xC000;

            _mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(), false))
                .Returns(opcode)
                .Returns(0x05);



        }
    }
}
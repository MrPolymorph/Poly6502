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
        private List<CycleTruthData> _truthData;
        
        private M6502 _m6502;
        private Mock<IDataBusCompatible> _mockRam;
        
        private void SetupTruthTable()
        {
            _truthData = new List<CycleTruthData>()
            {
                /* AND */
                /* Immediate */   new CycleTruthData(0x29, 2),
                /* Zero Page */   new CycleTruthData(0x25, 3),
                /* Zero Page X */ new CycleTruthData(0x35, 4),
                /* Absolute */    new CycleTruthData(0x2D, 4),
                /* Absolute X */  new CycleTruthData(0x3D, 4, true),
                /* Absolute Y */  new CycleTruthData(0x39, 4, true),
                /* Indirect X */  new CycleTruthData(0x21, 6),
                /* Indirect Y */  new CycleTruthData(0x31, 5, true),
                
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
        public void Test_AND_Cycle_Timing()
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (var truth in _truthData)
            {
                TestAND(truth.OpCode);

                var takenCycles = (_m6502.PreviousInstructionCycleLength + 1 + _m6502.PreviousAddressingModeCycleLength);

                if (!truth.BoundaryCrossable)
                {
                    Assert.AreEqual(truth.Cycles, takenCycles, $"opcode 0x{truth.OpCode:X2}", $"OpCode 0x{truth.OpCode:x2} Passed");
                    Console.WriteLine($"OpCode 0x{truth.OpCode:x2} Passed");
                }
                else
                {
                    if(takenCycles != truth.Cycles && takenCycles != truth.MaxPotentialCycles)
                        Assert.Fail($"Expected {truth.Cycles} or {truth.MaxPotentialCycles} cycles. Actual : {takenCycles}");
                    else if (takenCycles == truth.Cycles || takenCycles == truth.MaxPotentialCycles)
                    {
                        Console.WriteLine($"OpCode 0x{truth.OpCode:x2} Passed");
                    }
                }
            }
        }
        
        
        public void TestAND(byte opcode)
        {
            Operation op = _m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(_m6502.AND));
            
            _m6502.PC = 0xC000;

            _mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>()))
                .Returns(opcode)
                .Returns(0x05);
            
            
            do
            {
                _m6502.Fetch();
            } while (_m6502.AddressingModeInProgress);

            do
            {
                _m6502.Execute();
            } while (_m6502.OpCodeInProgress);
            
        }
    }
}
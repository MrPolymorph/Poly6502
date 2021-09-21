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
        private List<CycleTruthData> _truthData;
        
        private M6502 _m6502;
        private Mock<IDataBusCompatible> _mockRam;
        
        private void SetupTruthTable()
        {
            _truthData = new List<CycleTruthData>()
            {
                /* Register Instructions */
                /* Implied */   new CycleTruthData(0xAA, 2),
                /* Implied */   new CycleTruthData(0x8A, 2),
                /* Implied */   new CycleTruthData(0xCA, 2),
                /* Implied */   new CycleTruthData(0xE8, 2),
                /* Implied */   new CycleTruthData(0xA8, 2),
                /* Implied */   new CycleTruthData(0x98, 2),
                /* Implied */   new CycleTruthData(0x88, 2),
                /* Implied */   new CycleTruthData(0xC8, 2),
                
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
        public void Test_RegisterInstruction_Cycle_Timing()
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (var truth in _truthData)
            {
                TestRegisterInstruction(truth.OpCode);

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
        
        
        public void TestRegisterInstruction(byte opcode)
        {
            Operation op = _m6502.OpCodeLookupTable[opcode];
            
            
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
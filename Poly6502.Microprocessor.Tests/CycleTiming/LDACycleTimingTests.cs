using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Microprocessor.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class LDACycleTimingTests
    {
        private List<CycleTruthData> _truthData;
        
        private M6502 _m6502;
        private Mock<IDataBusCompatible> _mockRam;
        
        private void SetupTruthTable()
        {
            _truthData = new List<CycleTruthData>()
            {
                /* LDA */
                /* Immediate */   new CycleTruthData(0xA9, 2),
                /* Zero Page */   new CycleTruthData(0xA5, 3),
                /* Zero Page X */ new CycleTruthData(0xB5, 4),
                /* Absolute */    new CycleTruthData(0xAD, 4),
                /* Absolute X */  new CycleTruthData(0xBD, 4, true),
                /* Absolute Y */  new CycleTruthData(0xB9, 4, true),
                /* Indirect X */  new CycleTruthData(0xA1, 6),
                /* Indirect Y */  new CycleTruthData(0xB1, 5, true),
                
            };
        }

        [Test]
        [TestCase((byte)0xA9)]
        [TestCase((byte)0xA5)]
        [TestCase((byte)0xB5)]
        [TestCase((byte)0xAD)]
        [TestCase((byte)0xBD, true)]
        [TestCase((byte)0xB9, true)]
        [TestCase((byte)0xA1)]
        [TestCase((byte)0xB1, true)]
        
        public void Test_LDA_Cycle_Timing(byte opcode, bool boundaryCrossable = false)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);
            
            Operation op = m6502.OpCodeLookupTable[opcode];
            
            Assert.IsTrue(op.OpCodeCompare(m6502.LDA));
            
            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }

    }
}
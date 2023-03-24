using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Tests.Models;

namespace Poly6502.Microprocessor.Tests.CycleTiming
{
    public class LDXCycleTimingTests
    {
        private List<CycleTruthData> _truthData;
        
        private M6502 _m6502;
        private Mock<IDataBusCompatible> _mockRam;
        
        private void SetupTruthTable()
        {
            _truthData = new List<CycleTruthData>()
            {
                /* LDX */
                /* Immediate */   new CycleTruthData(0xA2, 2),
                /* Zero Page */   new CycleTruthData(0xA6, 3),
                /* Zero Page Y */ new CycleTruthData(0xB6, 4),
                /* Absolute */    new CycleTruthData(0xAE, 4),
                /* Absolute Y */  new CycleTruthData(0xBE, 4, true),

            };
        }

        [Test]
        [TestCase((byte)0xA2)]
        [TestCase((byte)0xA6)]
        [TestCase((byte)0xAE)]
        [TestCase((byte)0xBE, true)]
        public void Test_LDX_Cycle_Timing(byte opcode, bool boundaryCrossable = false)
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();
            m6502.RegisterDevice(mockRam.Object, 1);

            Operation op = m6502.OpCodeLookupTable[opcode];

            Assert.IsTrue(op.OpCodeCompare(m6502.LDX));

            CycleTimingTester.TestOpcode(m6502, mockRam, opcode, op);
        }
    }
}
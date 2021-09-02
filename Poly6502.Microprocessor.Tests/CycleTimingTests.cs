using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;

namespace Poly6502.Microprocessor.Tests
{
    public class AddressingModeCycleTests
    {
        private M6502 _m6502;
        private Mock<IDataBusCompatible> _mockRam;
        
        [SetUp]
        public void Setup()
        {
            _m6502 = new M6502();
            _mockRam = new Mock<IDataBusCompatible>();
            
            _m6502.RegisterDevice(_mockRam.Object);
        }

        [Test]
        public void ADC_IMM_Should_Take_2_Cycles()
        {
            Operation op = _m6502.OpCodeLookupTable[0x69];
            
            Assert.IsTrue(op.CompareInstruction(_m6502.ADC, _m6502.IMM));

            _m6502.PC = 0xC000;

            _mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>()))
                .Returns(0x69)
                .Returns(0x05);

            int totalCycles = 0;
            
            do
            {
                _m6502.Fetch();
                totalCycles++;
            } while (_m6502.AddressingModeInProgress);

            do
            {
                _m6502.Execute();
                totalCycles++;
            } while (_m6502.OpCodeInProgress);
            
            Assert.AreEqual(4, totalCycles);
        }
    }
}
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Flags;

namespace Poly6502.Microprocessor.Tests.CorrectnessTests
{
    public class ADCCorrectnessTests
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
        public void ADC_SimpleAddition_ImmediateMode()
        {
            // LDA IMM
            Operation lda = _m6502.OpCodeLookupTable[0xA9];
            
            //ADC IMM 
            Operation adc = _m6502.OpCodeLookupTable[0x69];

            Assert.IsTrue(lda.OpCodeCompare(_m6502.LDA));
            Assert.IsTrue(adc.OpCodeCompare(_m6502.ADC));

            _m6502.PC = 0xC000;

            _mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>()))
                .Returns(0x0A)
                .Returns(0x19);

            // Load the accumulator with 0x0A (10)
            do
            {
                lda.AddressingModeMethod();
            } while (_m6502.AddressingModeInProgress);

            do
            {
                lda.OpCodeMethod();
            } while (_m6502.OpCodeInProgress);
            
            //Call ADC to add 25
            do
            {
                adc.AddressingModeMethod();
            } while (_m6502.AddressingModeInProgress);

            do
            {
                adc.OpCodeMethod();
            } while (_m6502.OpCodeInProgress);
            
            //test result
            Assert.AreEqual(35, _m6502.A);
            
            //test processor status
            Assert.AreEqual(0x24, _m6502.P.Register);
        }
        
        [Test]
        public void ADC_SetCarry_ImmediateMode()
        {
            //CLC Implied
            Operation clc = _m6502.OpCodeLookupTable[0x18];
            
            //Store the Accumulator - Zero Page
            Operation sta = _m6502.OpCodeLookupTable[0x85];
            
            // LDA IMM
            Operation lda = _m6502.OpCodeLookupTable[0xA9];
            
            //ADC IMM 
            Operation adc = _m6502.OpCodeLookupTable[0x69];

            Assert.IsTrue(lda.OpCodeCompare(_m6502.LDA));
            Assert.IsTrue(adc.OpCodeCompare(_m6502.ADC));

            _m6502.PC = 0xC000;

            _mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>()))
                .Returns(0xF0)
                .Returns(0x14)
                .Returns(0x01)
                .Returns(0x00)
                .Returns(0x00)
                .Returns(0x02)
                .Returns(0x00);
            
                

            //Clear the carry bit
            do
            {
                clc.AddressingModeMethod();
                
            } while (_m6502.AddressingModeInProgress);

            do
            {
                clc.OpCodeMethod();
            } while (_m6502.OpCodeInProgress);
            
            Assert.IsTrue(!_m6502.P.HasFlag(StatusRegisterFlags.C));
            
            do
            {
                lda.AddressingModeMethod();
            } while (_m6502.AddressingModeInProgress);

            do
            {
                lda.OpCodeMethod();
            } while (_m6502.OpCodeInProgress);
            
            Assert.AreEqual(0xF0, _m6502.A);
            
            //Call ADC to add 240
            do
            {
                adc.AddressingModeMethod();
            } while (_m6502.AddressingModeInProgress);

            do
            {
                adc.OpCodeMethod();
            } while (_m6502.OpCodeInProgress);
            
            Assert.AreEqual(4, _m6502.A);
            Assert.IsTrue(_m6502.P.HasFlag(StatusRegisterFlags.C), _m6502.P.ToString());
            
            //store the result lo byte
            do
            {
                sta.AddressingModeMethod();
            } while (_m6502.AddressingModeInProgress);
            
            do
            {
                sta.OpCodeMethod();
            } while (_m6502.OpCodeInProgress);
            
            
            
            // Load the accumulator with 0x00 (00)
            do
            {
                lda.AddressingModeMethod();
            } while (_m6502.AddressingModeInProgress);

            do
            {
                lda.OpCodeMethod();
            } while (_m6502.OpCodeInProgress);
            
            Assert.AreEqual(0, _m6502.A);
            
            //Call ADC 
            do
            {
                adc.AddressingModeMethod();
            } while (_m6502.AddressingModeInProgress);

            do
            {
                adc.OpCodeMethod();
            } while (_m6502.OpCodeInProgress);
            
            
            //test result
            Assert.AreEqual(1, _m6502.A);
            
            //test processor status
            Assert.AreEqual(0x24, _m6502.P.Register);
        }
    }
}
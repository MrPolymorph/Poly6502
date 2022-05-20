using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Flags;

namespace Poly6502.Microprocessor.Tests.CorrectnessTests
{
    public class ADCCorrectnessTests
    {
        [Test]
        public void ADC_0x69_ImmediateMode_Correctness_Test_No_Carry()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0x69) //fetch should return opcode ADC IMM;
                .Returns(0x0A); //data fetched should be immediately after op code.
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //ADC IMM takes 2 cycles exactly.
            m6502.Clock();
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(2));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);

            Assert.AreEqual(0x0A, m6502.A);
            
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }        
        
        [Test]
        public void ADC_0x69_ImmediateMode_Correctness_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //Load the accumulator IMM
                .Returns(0xFF) //Accumulator should load with 0x0F
                .Returns(0x69) //fetch should return opcode ADC IMM;
                .Returns(0xFF); //data fetched should be immediately after op code.
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //LDA IMM takes 2 cycles exactly.
            m6502.Clock();
            m6502.Clock(); //ADC IMM takes 2 cycles exactly.
            m6502.Clock();
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(4));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);

            Assert.AreEqual(0xFE, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        }

        [Test] public void ADC_0x69_ImmediateMode_Correctness_Test_Should_Enable_Zero_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //Load the accumulator IMM
                .Returns(0xFF) //Accumulator should load with 0x0F
                .Returns(0x69) //fetch should return opcode ADC IMM;
                .Returns(0x01); //data fetched should be immediately after op code.
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //LDA IMM takes 2 cycles exactly.
            m6502.Clock();
            m6502.Clock(); //ADC IMM takes 2 cycles exactly.
            m6502.Clock();
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(4));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);

            Assert.AreEqual(0, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }
    }
}
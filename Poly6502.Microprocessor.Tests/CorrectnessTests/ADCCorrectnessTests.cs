using System;
using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Flags;

namespace Poly6502.Microprocessor.Tests.CorrectnessTests
{
    public class ADCCorrectnessTests
    {
        #region 0x69 Immediate Mode
        [Test]
        public void ADC_ImmediateMode_Correctness_Test_No_Carry()
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
        public void ADC_ImmediateMode_Correctness_Test_Should_Enable_Carry_Flag()
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

        [Test] public void ADC_ImmediateMode_Correctness_Test_Should_Enable_Zero_Flag()
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
        
        #endregion
        
        #region 0x65 ZeroPage
        [Test]
        public void ADC_ZeroPage_Correctness_Test_No_Carry()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0x65) //fetch should return opcode ADC IMM;
                .Returns(0x0A); //data fetched should be immediately after op code.
            
            m6502.RegisterDevice(mockRam.Object, 1);

            m6502.Clock(); //ADC IMM takes 2 cycles exactly.
            m6502.Clock();
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
        public void ADC_ZeroPage_Correctness_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //Load the accumulator IMM
                .Returns(0xFF) //Accumulator should load with 0x0F
                .Returns(0x65) //fetch should return opcode ADC IMM;
                .Returns(0xFF); //data fetched should be immediately after op code.
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //LDA IMM takes 2 cycles exactly.
            m6502.Clock();
            m6502.Clock();
            m6502.Clock();
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

        [Test] public void ADC_ZeroPage_Correctness_Test_Should_Enable_Zero_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //Load the accumulator IMM
                .Returns(0xFF) //Accumulator should load with 0xFF
                .Returns(0x65) //fetch should return opcode ADC ZPA;
                .Returns(0x01); //data fetched should be immediately after op code.
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //LDA IMM takes 2 cycles exactly.
            m6502.Clock();
            m6502.Clock();
            m6502.Clock();
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
        
        #endregion 0x65 Zero Page
        
        #region 0x75 Zero Page, X
                [Test]
        public void ADC_ZeroPageX_Correctness_Test_No_Carry()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA2)  //LDX
                .Returns(0x05)  //DATA
                .Returns(0x75)  //fetch should return opcode ADC;
                .Returns(0x05)  //Lo Byte
                .Returns(0x0A); //DATA
            
            m6502.RegisterDevice(mockRam.Object, 1);

            m6502.Clock(); //Fetch LDX
            m6502.Clock(); //Execute LDX
            m6502.Clock(); //Fetch ADC ZPX
            m6502.Clock(); //Read Lo Byte
            m6502.Clock(); //Read Lo Byte + X Offset.
            m6502.Clock(); //Execute ADC

            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(5));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);

            Assert.AreEqual(0x0A, m6502.A);
            
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }        
        
        [Test]
        public void ADC_ZeroPageX_Correctness_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //Load the accumulator IMM
                .Returns(0xFF) //Accumulator should load with 0xFF
                .Returns(0xA2) //Load the X Register IMM
                .Returns(0x05) //X should load with 0x05
                .Returns(0x75) //fetch should return opcode ADC ZPX;
                .Returns(0x0A) //get the address byte to be offset by the X register 
                .Returns(0xFF); //data fetched should be from operand + x
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); 
            m6502.Clock(); 
            m6502.Clock(); 
            m6502.Clock();
            m6502.Clock();
            m6502.Clock();
            m6502.Clock();
            m6502.Clock();

            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(7));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);

            Assert.AreEqual(0xFE, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        }

        [Test] public void ADC_ZeroPageX_Correctness_Test_Should_Enable_Zero_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA Opcode
                .Returns(0xFF) //LDA Operand
                .Returns(0xA2) //LDX Opcode
                .Returns(0x05) //LDX Operand
                .Returns(0x75) //ADC Opcode
                .Returns(0xAB) //ADC Address (address + x) 
                .Returns(0x01); //ADC Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch LDX
            m6502.Clock(); //Execute LDX
            m6502.Clock(); //Fetch ADC
            m6502.Clock(); //ZPX Execute
            m6502.Clock(); //ZPA Execute
            m6502.Clock(); //ADC Execute
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(7));

            
            Assert.AreEqual(0, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }
        #endregion
    }
}
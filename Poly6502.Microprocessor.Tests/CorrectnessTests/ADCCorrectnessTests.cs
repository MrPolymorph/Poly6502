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
        public void ADC_ImmediateMode_Test_No_Carry()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0x69) //ADC
                .Returns(0x0A); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);

            m6502.Clock(); //Fetch ADC
            m6502.Clock(); //Execute ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
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
        public void ADC_ImmediateMode_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0xFF) //Operand
                .Returns(0x69) //ADC
                .Returns(0xFF); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            m6502.Clock(); //Execute ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(4));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);

            Assert.AreEqual(0xFE, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        }

        [Test] public void ADC_ImmediateMode_Test_Should_Enable_Zero_Flag()
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
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            m6502.Clock(); //Execute ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(4));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);

            Assert.AreEqual(0, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }
        
        #endregion
        
        #region 0x65 ZeroPage
        [Test]
        public void ADC_ZeroPage_Test_No_Carry()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0x05) //LDA Operand
                .Returns(0x65) //ADC
                .Returns(0x0A) //ZPA address
                .Returns(0x05); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);

            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Fetch LDA Operand
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Fetch Operand
            m6502.Clock(); //Execute ADC
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(5));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(0xA, false), Times.Once);

            Assert.AreEqual(0x0A, m6502.A);
            
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }        
        
        [Test]
        public void ADC_ZeroPage_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //Load the accumulator IMM
                .Returns(0xFF) //Accumulator should load with 0x0F
                .Returns(0x65) //ADC
                .Returns(0x0A) //ZPA address
                .Returns(0x0FF); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Fetch Operand
            m6502.Clock(); //Execute ADC
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(5));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0A, false), Times.Once);

            Assert.AreEqual(0xFE, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        }

        [Test] public void ADC_ZeroPage_Test_Should_Enable_Zero_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0xFF) //LDA Operand
                .Returns(0x65) //ADC
                .Returns(0x0A) //ZPA address
                .Returns(0x01); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Fetch Operand
            m6502.Clock(); //Execute ADC
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(5));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0A, false), Times.Once);

            Assert.AreEqual(0, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }
        
        #endregion 0x65 Zero Page
        
        #region 0x75 Zero Page, X
        [Test]
        public void ADC_ZeroPageX_Test_No_Carry()
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
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
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
        public void ADC_ZeroPageX_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0xFF) //Operand
                .Returns(0xA2) //LDX
                .Returns(0x0A) //Operand
                .Returns(0x75) //ADC
                .Returns(0x0A) //ADC Offset 
                .Returns(0xFF); //ADC Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch LDX
            m6502.Clock(); //Execute LDX
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Fetch ADC offset
            m6502.Clock(); //Fetch ADC Operand
            m6502.Clock(); //Execute ADC

            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(7));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(4, false), Times.Once);
            mockRam.Verify(x => x.Read(0xF, false), Times.Once);
            mockRam.Verify(x => x.Read(0xA, false), Times.Once);

            Assert.AreEqual(0xFE, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        }

        [Test] public void ADC_ZeroPageX_Test_Should_Enable_Zero_Flag()
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
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
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
        
        #region 0x6D Absolute
        [Test]
        public void ADC_Absolute_Test_No_Carry()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0x6D) //ADC ABS
                .Returns(0x0F) //Lo Byte
                .Returns(0x0F) //HiByte
                .Returns(0x70);  //Operand

            m6502.RegisterDevice(mockRam.Object, 1);

            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Absolute Read Lo Byte
            m6502.Clock(); //Absolute Read Hi Byte
            m6502.Clock(); //Execute ADC
            

            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(4));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F0F, false), Times.Once);

            Assert.AreEqual(0x70, m6502.A);
            
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }        
        
        [Test]
        public void ADC_Absolute_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0xFF) //LDA operand.
                .Returns(0x6D) //ADC ABS
                .Returns(0x0F) //Lo Byte
                .Returns(0x0F) //HiByte
                .Returns(0xFF);  //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Absolute Read Lo Byte
            m6502.Clock(); //Absolute Read Hi Byte
            m6502.Clock(); //Execute ADC

            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(6));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(4, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F0F, false), Times.Once);

            Assert.AreEqual(0xFE, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        }

        [Test] 
        public void DCADC_Absolute_Test_Should_Enable_Zero_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //Load the accumulator IMM
                .Returns(0xFF) //Accumulator should load with 0x0F
                .Returns(0x6D) //ADC ABS
                .Returns(0x0F) //Lo Byte
                .Returns(0x0F) //HiByte
                .Returns(0x01); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Absolute Read Lo Byte
            m6502.Clock(); //Absolute Read Hi Byte
            m6502.Clock(); //Execute ADC
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(6));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(4, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F0F, false), Times.Once);
            
            Assert.AreEqual(0, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }
        #endregion
        
        #region 0x7D Absolute,X
        [Test]
        public void ADC_AbsoluteX_Test_No_Carry()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0x0A) //LDA Operand
                .Returns(0x7D) //ADC ABS X
                .Returns(0x0F) //Lo Byte
                .Returns(0x0F) //HiByte
                .Returns(0x70);  //Operand

            m6502.RegisterDevice(mockRam.Object, 1);

            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Absolute Read Lo Byte
            m6502.Clock(); //Absolute Read Hi Byte
            m6502.Clock(); //Execute ADC


            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(6));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(4, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F0F, false), Times.Once);
            

            Assert.AreEqual(0x7A, m6502.A);
            
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }        
        
        [Test]
        public void ADC_AbsoluteX_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0xFF) //LDA operand.
                .Returns(0x7D) //ADC ABS
                .Returns(0x0F) //Lo Byte
                .Returns(0x0F) //HiByte
                .Returns(0xFF); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC X
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Absolute Read Lo Byte
            m6502.Clock(); //Absolute Read Hi Byte
            m6502.Clock(); //Execute ADC

            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(6));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(4, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F0F, false), Times.Once);

            Assert.AreEqual(0xFE, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        }

        [Test] 
        public void ADC_AbsoluteX_Test_Should_Enable_Zero_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //Load the accumulator IMM
                .Returns(0xFF) //Accumulator should load with 0x0F
                .Returns(0x7D) //ADC ABS X
                .Returns(0x0F) //Lo Byte
                .Returns(0x0F) //HiByte
                .Returns(0x01); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Absolute Read Lo Byte
            m6502.Clock(); //Absolute Read Hi Byte
            m6502.Clock(); //Execute ADC
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(6));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(4, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F0F, false), Times.Once);
            
            Assert.AreEqual(0, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }
        #endregion
        
        #region 0x79 Absolute,Y
        [Test]
        public void ADC_AbsoluteY_Test_No_Carry()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0x0A) //LDA Operand
                .Returns(0x79) //ADC ABS Y
                .Returns(0x0F) //Lo Byte
                .Returns(0x0F) //HiByte
                .Returns(0x70);  //Operand

            m6502.RegisterDevice(mockRam.Object, 1);

            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Absolute Read Lo Byte
            m6502.Clock(); //Absolute Read Hi Byte
            m6502.Clock(); //Execute ADC


            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(6));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(4, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F0F, false), Times.Once);
            

            Assert.AreEqual(0x7A, m6502.A);
            
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }        
        
        [Test]
        public void ADC_AbsoluteY_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0xFF) //LDA operand.
                .Returns(0x79) //ADC ABS
                .Returns(0x0F) //Lo Byte
                .Returns(0x0F) //HiByte
                .Returns(0xFF); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC X
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Absolute Read Lo Byte
            m6502.Clock(); //Absolute Read Hi Byte
            m6502.Clock(); //Execute ADC

            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(6));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(4, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F0F, false), Times.Once);

            Assert.AreEqual(0xFE, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        }

        [Test] 
        public void ADC_AbsoluteY_Test_Should_Enable_Zero_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //Load the accumulator IMM
                .Returns(0xFF) //Accumulator should load with 0x0F
                .Returns(0x79) //ADC ABS X
                .Returns(0x0F) //Lo Byte
                .Returns(0x0F) //HiByte
                .Returns(0x01); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Absolute Read Lo Byte
            m6502.Clock(); //Absolute Read Hi Byte
            m6502.Clock(); //Execute ADC
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(6));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(4, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F0F, false), Times.Once);
            
            Assert.AreEqual(0, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }
        #endregion
        
        #region 0x61 (Indirect,X)
        [Test]
        public void ADC_IndirectX_Test_No_Carry()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0x0A) //LDA Operand
                .Returns(0x61) //ADC ABS Y
                .Returns(0x0F) //Offset
                .Returns(0x0F) //LoByte
                .Returns(0x70) //HiByte
                .Returns(0x0A); //Operand

            m6502.RegisterDevice(mockRam.Object, 1);

            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Read Offset Byte
            m6502.Clock(); //Read LoByte
            m6502.Clock(); //Read HiByte
            m6502.Clock(); //Read Operand
            m6502.Clock(); //Execute ADC


            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(7));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F, false), Times.Once);
            mockRam.Verify(x => x.Read(0x10, false), Times.Once);
            mockRam.Verify(x => x.Read(0x700F, false), Times.Once);
            

            Assert.AreEqual(0x14, m6502.A);
            
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }        
        
        [Test]
        public void ADC_IndirectX_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0xFF) //Operand
                .Returns(0x61) //ADC ABS Y
                .Returns(0x0F) //Offset
                .Returns(0x0F) //LoByte
                .Returns(0x70) //HiByte
                .Returns(0xFF); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Read Offset Byte
            m6502.Clock(); //Read LoByte
            m6502.Clock(); //Read HiByte
            m6502.Clock(); //Read Operand
            m6502.Clock(); //Execute ADC

            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(7));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F, false), Times.Once);
            mockRam.Verify(x => x.Read(0x10, false), Times.Once);
            mockRam.Verify(x => x.Read(0x700F, false), Times.Once);

            Assert.AreEqual(0xFE, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        }

        [Test] 
        public void ADC_IndirectX_Test_Should_Enable_Zero_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0xFF) //LDA Operand
                .Returns(0x61) //ADC ABS Y
                .Returns(0x0F) //Offset
                .Returns(0x0F) //LoByte
                .Returns(0x70) //HiByte
                .Returns(0x01); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Read Offset Byte
            m6502.Clock(); //Read LoByte
            m6502.Clock(); //Read HiByte
            m6502.Clock(); //Read Operand
            m6502.Clock(); //Execute ADC
            
            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(7));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F, false), Times.Once);
            mockRam.Verify(x => x.Read(0x10, false), Times.Once);
            mockRam.Verify(x => x.Read(0x700F, false), Times.Once);
            
            Assert.AreEqual(0, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }
        #endregion
        
        #region 0x71 (Indirect), Y
        [Test]
        public void ADC_Indirect_Y_Test_No_Carry()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0x0A) //LDA Operand
                .Returns(0x71) //ADC ABS Y
                .Returns(0x0F) //Offset
                .Returns(0x0F) //LoByte
                .Returns(0x70) //HiByte
                .Returns(0x0A); //Operand

            m6502.RegisterDevice(mockRam.Object, 1);

            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Read Offset Byte
            m6502.Clock(); //Read LoByte
            m6502.Clock(); //Read HiByte
            m6502.Clock(); //Execute


            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(7));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F, false), Times.Once);
            mockRam.Verify(x => x.Read(0x10, false), Times.Once);
            mockRam.Verify(x => x.Read(0x700F, false), Times.Once);
            

            Assert.AreEqual(0x14, m6502.A);
            
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }        
        
        [Test]
        public void ADC_Indirect_Y_Test_Should_Enable_Carry_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0xFF) //Operand
                .Returns(0x71) //ADC ABS Y
                .Returns(0x0F) //Offset
                .Returns(0x0F) //LoByte
                .Returns(0x70) //HiByte
                .Returns(0xFF); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Read Offset Byte
            m6502.Clock(); //Read LoByte
            m6502.Clock(); //Read HiByte
            m6502.Clock(); //Execute

            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(7));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F, false), Times.Once);
            mockRam.Verify(x => x.Read(0x10, false), Times.Once);
            mockRam.Verify(x => x.Read(0x700F, false), Times.Once);

            Assert.AreEqual(0xFE, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        }

        [Test] 
        public void ADC_Indirect_Y_Test_Should_Enable_Zero_Flag()
        {
            var m6502 = new M6502();
            var mockRam = new Mock<IDataBusCompatible>();

            mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                    It.IsAny<bool>()))
                .Returns(0xA9) //LDA
                .Returns(0xFF) //LDA Operand
                .Returns(0x71) //ADC ABS Y
                .Returns(0x0F) //Offset
                .Returns(0x0F) //LoByte
                .Returns(0x70) //HiByte
                .Returns(0x01); //Operand
            
            m6502.RegisterDevice(mockRam.Object, 1);
            
            m6502.Clock(); //Fetch LDA
            m6502.Clock(); //Execute LDA
            m6502.Clock(); //Fetch ADC
            
            Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ADC);
            
            m6502.Clock(); //Read Offset Byte
            m6502.Clock(); //Read LoByte
            m6502.Clock(); //Read HiByte
            m6502.Clock(); //Execute

            mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(7));
            mockRam.Verify(x => x.Read(0, false), Times.Once);
            mockRam.Verify(x => x.Read(1, false), Times.Once);
            mockRam.Verify(x => x.Read(2, false), Times.Once);
            mockRam.Verify(x => x.Read(3, false), Times.Once);
            mockRam.Verify(x => x.Read(0x0F, false), Times.Once);
            mockRam.Verify(x => x.Read(0x10, false), Times.Once);
            mockRam.Verify(x => x.Read(0x700F, false), Times.Once);
            
            Assert.AreEqual(0, m6502.A);
            
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
            Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
            Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        }        
        #endregion
    }
}
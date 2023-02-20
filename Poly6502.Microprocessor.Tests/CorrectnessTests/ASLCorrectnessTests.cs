using Moq;
using NUnit.Framework;
using Poly6502.Interfaces;
using Poly6502.Microprocessor.Flags;

namespace Poly6502.Microprocessor.Tests.CorrectnessTests;

public class ASLCorrectnessTests
{
    #region 0x0A Accumulator Mode

    [Test]
    public void ASL_Accumulator_Should_Shift()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0x04) //Operand
            .Returns(0x0A); //ASL

        m6502.RegisterDevice(mockRam.Object, 1);
        
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch ASL
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ASL);
        
        m6502.Clock(); //Execute ASL
        
        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        
        Assert.AreEqual(8, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }
    
    [Test]
    public void ASL_Accumulator_Should_Shift_And_Set_Negative_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0x40) //Operand
            .Returns(0x0A); //ASL

        m6502.RegisterDevice(mockRam.Object, 1);
        
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch ASL
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ASL);
        
        m6502.Clock(); //Execute ASL
        
        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        
        
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
        Assert.AreEqual(0x80, m6502.A);
    }
    
    [Test]
    public void ASL_Accumulator_Should_Shift_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0) //Operand
            .Returns(0x0A); //ASL

        m6502.RegisterDevice(mockRam.Object, 1);
        
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch ASL
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ASL);
        
        m6502.Clock(); //Execute ASL
        
        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        
        
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        Assert.AreEqual(0, m6502.A);
    }
    
    [Test]
    public void ASL_Accumulator_Should_Shift_And_Set_Carry_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0x80) //Operand
            .Returns(0x0A); //ASL

        m6502.RegisterDevice(mockRam.Object, 1);
        
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch ASL
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ASL);
        
        m6502.Clock(); //Execute ASL
        
        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        
        
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        Assert.AreEqual(0, m6502.A);
    }
    #endregion
    
    #region 0x06 ZeroPage Mode

    [Test]
    public void ASL_ZeroPage_Should_Shift()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0x06) //ASL
            .Returns(0xAB) //ZPA
            .Returns(0x04); //Operand
        
        m6502.RegisterDevice(mockRam.Object, 1);
        
        m6502.Clock(); //Fetch ASL
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ASL);
        
        m6502.Clock(); //Read PC
        m6502.Clock(); //Execute ASL
        m6502.Clock(); //Write Result
        m6502.Clock(); //Execute ASL

        
        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(0xAB, false), Times.Once);
        mockRam.Verify(x => x.Write(0xAB, 8), Times.Once);
        
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }
    
    [Test]
    public void ASL_ZeroPage_Should_Shift_And_Set_Negative_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0x06) //Read ASL
            .Returns(0x1A) //ZPA Read Address
            .Returns(0x70) //ASL
            .Returns(0x0A) //ASL
            .Returns(0xA0); //ASL

        m6502.RegisterDevice(mockRam.Object, 1);
        
        m6502.Clock(); //Fetch ASL
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ASL);
        
        m6502.Clock(); //Fetch Execute ASL ZPA
        m6502.Clock(); //Fetch Execute ASL ZPA
        m6502.Clock(); //Execute ASL
        m6502.Clock(); //Execute ASL

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(0x1A, false), Times.Once);
        mockRam.Verify(x => x.Write(0x1A, 0xE0), Times.Once);


        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
    }
    
    [Test]
    public void ASL_ZeroPage_Should_Shift_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0x06) //Read ASL
            .Returns(0x1A) //ZPA Read Address
            .Returns(0x00) //ASL
            .Returns(0x0A) //ASL
            .Returns(0xA0); //ASL

        m6502.RegisterDevice(mockRam.Object, 1);
        
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch ASL
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ASL);
        
        m6502.Clock(); //Execute ASL
        
        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(0x1A, false), Times.Once);
        mockRam.Verify(x => x.Write(0x1A, 0x00), Times.Once);
        
        
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }
    
    [Test]
    public void ASL_ZeroPage_Should_Shift_And_Set_Carry_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0x06) //Read ASL
            .Returns(0x1A) //ZPA Read Address
            .Returns(0x82) //ASL
            .Returns(0x0A) //ASL
            .Returns(0xA0); //ASL

        m6502.RegisterDevice(mockRam.Object, 1);
        
        m6502.Clock(); //Fetch ASL
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ASL);
        
        m6502.Clock(); //Execute ASL
        m6502.Clock(); //Execute ASL
        m6502.Clock(); //Execute ASL
        m6502.Clock(); //Execute ASL
        
        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(0x1A, false), Times.Once);
        mockRam.Verify(x => x.Write(0x1A, 4), Times.Once);
        
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        Assert.AreEqual(0, m6502.A);
    }
    #endregion
    
    #region 0x16 Zero Page, X

    [Test]
    public void Accumulator_Zero_Page_X()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2)  //LDX
            .Returns(0x05)  //DATA
            .Returns(0x16)  //fetch should return opcode ASL;
            .Returns(0x05)  //Lo Byte
            .Returns(0x0A); //DATA

        m6502.RegisterDevice(mockRam.Object, 1);
        
        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch ADC ZPX
            
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ASL);
            
        m6502.Clock(); //Read Lo Byte
        m6502.Clock(); //Read Lo Byte + X Offset.
        m6502.Clock(); //Execute ASL
        m6502.Clock(); //Execute ASL

        mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(5));
        mockRam.Verify(x => x.Read(0x00, false), Times.Once);
        mockRam.Verify(x => x.Read(0x01, false), Times.Once);
        mockRam.Verify(x => x.Read(0x02, false), Times.Once);
        mockRam.Verify(x => x.Read(0x08, false), Times.Once);
        mockRam.Verify(x => x.Read(0x05, false), Times.Once);
        mockRam.Verify(x => x.Write(0x05, 0x14), Times.Once);
        
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));;
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
        Assert.AreEqual(0, m6502.A);
    }
    
    #endregion

    #region 0x0E Absolute

    [Test]
    public void ASL_Absolute()
    {
        Assert.Fail();
    }

    #endregion

    #region 0x1E Absolute X

    [Test]
    public void ASL_Absolute_X()
    {
        Assert.Fail();
    }

    #endregion
}
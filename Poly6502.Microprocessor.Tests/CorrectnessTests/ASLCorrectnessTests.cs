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
            .Returns(0xA9) //LDA
            .Returns(0x04) //Operand
            .Returns(0x06) //ASL
            .Returns(0xAB) //ZPA
            .Returns(0x04); //Operand
            
            

        m6502.RegisterDevice(mockRam.Object, 1);
        
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch ASL
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.ASL);
        
        m6502.Clock(); //Read PC
        m6502.Clock(); //Execute ASL
        m6502.Clock(); //Write Result
        m6502.Clock(); //Execute ASL
        
        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(0xAB, false), Times.Once);
        mockRam.Verify(x => x.Write(0xAB, 4), Times.Once);
        
        
        Assert.AreEqual(4, m6502.A);
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
            .Returns(0xA9) //LDA
            .Returns(0x40) //Operand
            .Returns(0x06); //ASL

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
    public void ASL_ZeroPage_Should_Shift_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0) //Operand
            .Returns(0x06); //ASL

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
    public void ASL_ZeroPage_Should_Shift_And_Set_Carry_Flag()
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
}
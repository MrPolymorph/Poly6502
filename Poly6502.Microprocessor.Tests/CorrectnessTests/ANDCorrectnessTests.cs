using Moq;
using NUnit.Framework;
using Poly6502.Microprocessor.Flags;
using Poly6502.Microprocessor.Interfaces;
using Poly6502.Microprocessor.Models;

namespace Poly6502.Microprocessor.Tests.CorrectnessTests;

public class ANDCorrectnessTests
{
    #region 0x29 Immediate Mode

    [Test]
    public void AND_Immediate_Should_AND_Memory_With_Accumulator()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0x05) //Operand
            .Returns(0x29) //AND
            .Returns(0x05); //Operand

        m6502.RegisterDevice(mockRam.Object, 1);


        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);

        Assert.AreEqual(0x05 & 0x05, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Immediate_Should_AND_Memory_With_Accumulator_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0x00) //Operand
            .Returns(0x29) //AND
            .Returns(0x00); //Operand

        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);

        Assert.AreEqual(0, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Immediate_Should_AND_Memory_With_Accumulator_And_Set_Negative_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0x90) //Operand
            .Returns(0x29) //AND
            .Returns(0x80); //Operand

        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);

        Assert.AreEqual(0x90 & 0x80, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    #endregion

    #region 0x25 ZeroPage Mode

    [Test]
    public void AND_ZeroPage_Should_AND_Memory_With_Accumulator()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0x05) //Operand
            .Returns(0x25) //AND
            .Returns(0x0F); //Operand

        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND
        m6502.Clock(); //Execute AND
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Never);

        Assert.AreEqual(0x05 & 0x05, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_ZeroPage_Should_AND_Memory_With_Accumulator_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0x01) //Operand
            .Returns(0x25) //AND
            .Returns(0x0A); //Operand

        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read ZPA
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Never);

        Assert.AreEqual(0x01 & 0x0A, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_ZeroPage_Should_AND_Memory_With_Accumulator_And_Set_Negative_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0x80) //Operand
            .Returns(0x25) //AND
            .Returns(0x0A) //ZPA
            .Returns(0x80); //Operand

        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND
        m6502.Clock(); //Execute AND
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);

        Assert.AreEqual(0x90 & 0x80, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    #endregion

    #region 0x35 ZeroPageX Mode

    [Test]
    public void AND_ZeroPage_X_Should_AND_Memory_With_Accumulator()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x05) //Operand
            .Returns(0x35) //AND
            .Returns(0x0B) // ZPX
            .Returns(0x05); //operand

        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC + X
        m6502.Clock(); //Read Operand
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5 + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0B, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Never);

        Assert.AreEqual(0x05 & 0x05, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_ZeroPage_X_Should_AND_Memory_With_Accumulator_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x00) //Operand
            .Returns(0x35) //AND
            .Returns(0x0B) // ZPX
            .Returns(0x00); //operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC + X
        m6502.Clock(); //Read Operand
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5 + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0B, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Never);

        Assert.AreEqual(0, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_ZeroPage_X_Should_AND_Memory_With_Accumulator_And_Set_Negative_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x80) //Operand
            .Returns(0x35) //AND
            .Returns(0x0B) // ZPX
            .Returns(0x80); //operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC + X
        m6502.Clock(); //Read Operand
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5 + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0B, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Never);

        Assert.AreEqual(0x90 & 0x80, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    #endregion

    #region 0x2D Absolute Mode

    [Test]
    public void AND_Absolute_Should_AND_Memory_With_Accumulator()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA9) //LDA
            .Returns(0x05) //LDA Operand
            .Returns(0x2D) //get AND
            .Returns(0x05) //get lo
            .Returns(0xA) //get high
            .Returns(0x05); //Operand
        
        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch Accumulator 
        m6502.Clock(); //Execute Accumulator
        m6502.Clock(); //Fetch AND
        
        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);
        
        m6502.Clock(); //get low
        m6502.Clock(); //get hi
        m6502.Clock(); //Execute AND
        
        mockRam.Verify(x => x.Read(It.IsAny<ushort>(), It.IsAny<bool>()), Times.Exactly(6));

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(0xA05, false), Times.Once);

        Assert.AreEqual(0x05 & 0x05, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Absolute_Should_AND_Memory_With_Accumulator_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x00) //Operand
            .Returns(0x2D) //AND
            .Returns(0x0B) //Lo
            .Returns(0x05) //Hi
            .Returns(0x05); //Operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC + X
        m6502.Clock(); //Read Operand
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Once);
        mockRam.Verify(x => x.Read(0x050B, false), Times.Once);
        mockRam.Verify(x => x.Read(7, false), Times.Never);

        Assert.AreEqual(0, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Absolute_Should_AND_Memory_With_Accumulator_And_Set_Negative_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x80) //Operand
            .Returns(0x2D) //AND
            .Returns(0x0B) //Lo
            .Returns(0x05) //Hi
            .Returns(0x90); //Operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC + X
        m6502.Clock(); //Read Operand
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Once);
        mockRam.Verify(x => x.Read(0x050B, false), Times.Once);
        mockRam.Verify(x => x.Read(7, false), Times.Never);

        Assert.AreEqual(0x90 & 0x80, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    #endregion

    #region 0x3D Absolute X Mode

    [Test]
    public void AND_Absolute_X_Should_AND_Memory_With_Accumulator()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x05) //Operand
            .Returns(0x3D) //AND
            .Returns(0x0B) //Lo
            .Returns(0x05) //Hi
            .Returns(0x05); //Operand

        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Lo Byte
        m6502.Clock(); //Hi Byte
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Once);
        mockRam.Verify(x => x.Read(0x050B + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(7, false), Times.Never);

        Assert.AreEqual(0x05 & 0x05, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Absolute_X_Should_AND_Memory_With_Accumulator_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x00) //Operand
            .Returns(0x3D) //AND
            .Returns(0x0B) //Lo
            .Returns(0x05) //Hi
            .Returns(0x05); //Operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC + X
        m6502.Clock(); //Read Operand
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Once);
        mockRam.Verify(x => x.Read(0x050B + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(7, false), Times.Never);

        Assert.AreEqual(0, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Absolute_X_Should_AND_Memory_With_Accumulator_And_Set_Negative_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x80) //Operand
            .Returns(0x3D) //AND
            .Returns(0x0B) //Lo
            .Returns(0x05) //Hi
            .Returns(0x90); //Operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC + X
        m6502.Clock(); //Read Operand
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Once);
        mockRam.Verify(x => x.Read(0x050B + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(7, false), Times.Never);

        Assert.AreEqual(0x90 & 0x80, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
    }
    #endregion
    
    #region 0x39 Absolute Y Mode
    [Test]
    public void AND_Absolute_Y_Should_AND_Memory_With_Accumulator()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA0) //LDY
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x05) //Operand
            .Returns(0x39) //AND
            .Returns(0x0B) //Lo
            .Returns(0x05) //Hi
            .Returns(0x05); //Operand

        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Lo Byte
        m6502.Clock(); //Hi Byte
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Once);
        mockRam.Verify(x => x.Read(0x050B + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(7, false), Times.Never);

        Assert.AreEqual(0x05 & 0x05, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Absolute_Y_Should_AND_Memory_With_Accumulator_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA0) //LDY
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x00) //Operand
            .Returns(0x39) //AND
            .Returns(0x0B) //Lo
            .Returns(0x05) //Hi
            .Returns(0x05); //Operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC + X
        m6502.Clock(); //Read Operand
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Once);
        mockRam.Verify(x => x.Read(0x050B + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(7, false), Times.Never);

        Assert.AreEqual(0, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Absolute_Y_Should_AND_Memory_With_Accumulator_And_Set_Negative_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA0) //LDY
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x80) //Operand
            .Returns(0x39) //AND
            .Returns(0x0B) //Lo
            .Returns(0x05) //Hi
            .Returns(0x90); //Operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC + X
        m6502.Clock(); //Read Operand
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Once);
        mockRam.Verify(x => x.Read(0x050B + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(7, false), Times.Never);

        Assert.AreEqual(0x90 & 0x80, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
    }
    #endregion
    
    #region 0x21 Indirect X Mode
    [Test]
    public void AND_Indirect_X_Should_AND_Memory_With_Accumulator()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x05) //Operand
            .Returns(0x21) //AND
            .Returns(0x06) //offset
            .Returns(0x07) //Hi
            .Returns(0x07) //lo
            .Returns(0x05); //operand

        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC (offset)
        m6502.Clock(); //Read LoByte (offset + x)
        m6502.Clock(); //Read HiByte (offset + x + 1)
        m6502.Clock(); //Read Hi << 8 | Lo
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(0x06 + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(0x06 + 0x0A + 1, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0707, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Never);

        Assert.AreEqual(0x05 & 0x05, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Indirect_X_Should_AND_Memory_With_Accumulator_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x00) //Operand
            .Returns(0x21) //AND
            .Returns(0x06) //offset
            .Returns(0x07) //Hi
            .Returns(0x07) //lo
            .Returns(0x05); //Operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC (offset)
        m6502.Clock(); //Read LoByte (offset + x)
        m6502.Clock(); //Read HiByte (offset + x + 1)
        m6502.Clock(); //Read Hi << 8 | Lo
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(0x06 + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(0x06 + 0x0A + 1, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0707, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Never);

        Assert.AreEqual(0, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Indirect_X_Should_AND_Memory_With_Accumulator_And_Set_Negative_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA2) //LDX
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x80) //Operand
            .Returns(0x21) //AND
            .Returns(0x06) //offset
            .Returns(0x07) //Hi
            .Returns(0x07) //lo
            .Returns(0x90); //Operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC (offset)
        m6502.Clock(); //Read LoByte (offset + x)
        m6502.Clock(); //Read HiByte (offset + x + 1)
        m6502.Clock(); //Read Hi << 8 | Lo
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(0x06 + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(0x06 + 0x0A + 1, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0707, false), Times.Once);
        mockRam.Verify(x => x.Read(6, false), Times.Never);

        Assert.AreEqual(0x90 & 0x80, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
    }
    #endregion
    
    #region 0x31 (Indirect) Y Mode
    [Test]
    public void AND_Indirect_Y_Should_AND_Memory_With_Accumulator()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA0) //LDY
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x05) //Operand
            .Returns(0x31) //AND
            .Returns(0x0B) //Temp
            .Returns(0x0D) //Hi
            .Returns(0x0D) //lo
            .Returns(0x05); //operand

        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC (temp)
        m6502.Clock(); //Read LoByte (temp)
        m6502.Clock(); //Read HiByte (temp + 1)
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0B, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0C, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0D0D + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(6 + 0x0A, false), Times.Never);

        Assert.AreEqual(0x05 & 0x05, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Indirect_Y_Should_AND_Memory_With_Accumulator_And_Set_Zero_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA0) //LDY
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x00) //Operand
            .Returns(0x31) //AND
            .Returns(0x0B) //Temp
            .Returns(0x0D) //Hi
            .Returns(0x0D) //lo
            .Returns(0x05); //Operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC (temp)
        m6502.Clock(); //Read LoByte (temp)
        m6502.Clock(); //Read HiByte (temp + 1)
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0B, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0C, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0D0D + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(6 + 0x0A, false), Times.Never);

        Assert.AreEqual(0, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.N));
    }

    [Test]
    public void AND_Indirect_Y_Should_AND_Memory_With_Accumulator_And_Set_Negative_Flag()
    {
        var m6502 = new M6502();
        var mockRam = new Mock<IDataBusCompatible>();

        mockRam.SetupSequence(x => x.Read(It.IsAny<ushort>(),
                It.IsAny<bool>()))
            .Returns(0xA0) //LDY
            .Returns(0x0A) //Operand
            .Returns(0xA9) //LDA
            .Returns(0x80) //Operand
            .Returns(0x31) //AND
            .Returns(0x0B) //Temp
            .Returns(0x0D) //Hi
            .Returns(0x0D) //lo
            .Returns(0x90); //Operand


        m6502.RegisterDevice(mockRam.Object, 1);

        m6502.Clock(); //Fetch LDX
        m6502.Clock(); //Execute LDX
        m6502.Clock(); //Fetch LDA
        m6502.Clock(); //Execute LDA
        m6502.Clock(); //Fetch AND

        Assert.AreEqual(m6502.InstructionRegister.OpCodeMethod, m6502.AND);

        m6502.Clock(); //Read PC (temp)
        m6502.Clock(); //Read LoByte (temp)
        m6502.Clock(); //Read HiByte (temp + 1)
        m6502.Clock(); //Execute AND

        mockRam.Verify(x => x.Read(0, false), Times.Once);
        mockRam.Verify(x => x.Read(1, false), Times.Once);
        mockRam.Verify(x => x.Read(2, false), Times.Once);
        mockRam.Verify(x => x.Read(3, false), Times.Once);
        mockRam.Verify(x => x.Read(4, false), Times.Once);
        mockRam.Verify(x => x.Read(5, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0B, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0C, false), Times.Once);
        mockRam.Verify(x => x.Read(0x0D0D + 0x0A, false), Times.Once);
        mockRam.Verify(x => x.Read(6 + 0x0A, false), Times.Never);

        Assert.AreEqual(0x90 & 0x80, m6502.A);
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.C));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.Z));
        Assert.False(m6502.P.HasFlag(StatusRegisterFlags.V));
        Assert.True(m6502.P.HasFlag(StatusRegisterFlags.N));
    }
    #endregion
}

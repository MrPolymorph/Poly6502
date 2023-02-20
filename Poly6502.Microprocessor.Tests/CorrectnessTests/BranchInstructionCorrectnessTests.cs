using NUnit.Framework;

namespace Poly6502.Microprocessor.Tests.CorrectnessTests;

public class BranchInstructionCorrectnessTests
{
    #region 0x10 Branch On Plus

    [Test]
    public void BPL_Branch_On_Plus()
    {
        Assert.Fail();
    }
    #endregion

    #region 0x30 Branch On Minus

    [Test]
    public void BMI_Branch_On_Minus()
    {
        Assert.Fail();
    }
    
    #endregion

    #region 0x50 Branch On Overflow Clear

    [Test]
    public void BVC_Branch_On_Overflow_Clear()
    {
        Assert.Fail();
    }

    #endregion

    #region 0x70 BVS Branch on Overflow Set

    [Test]
    public void BVS_Branch_On_Overflow_Set()
    {
        Assert.Fail();
    }

    #endregion

    #region 0x90 BCC Branch On Carry Clear

    [Test]
    public void Branch_On_Carry_Clear()
    {
        Assert.Fail();
    }

    #endregion

    #region 0xB0 BCS Branch On Carry Set

    [Test]
    public void Branch_On_Carry_Set()
    {
        Assert.Fail();
    }

    #endregion

    #region 0xD0 Branch On Not Equal

    [Test]
    public void Branch_On_Not_Equal()
    {
        Assert.Fail();
    }

    #endregion

    #region 0xF0 Branch On Equal

    [Test]
    public void Branch_On_Equal()
    {
        Assert.Fail();
    }

    #endregion
}
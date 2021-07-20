using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poly6502.Microprocessor;

namespace Poly6502.Tests
{
    [TestClass]
    public class AddressingModeTests
    {
        [TestMethod]
        public void ZeroPage_Should_Do_Something()
        {
            var ram = new Ram.Ram(0xFFFF);
            

            var processor = new M6502();
            
        }

    }
}
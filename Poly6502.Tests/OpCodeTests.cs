using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poly6502.Microprocessor;

namespace Poly6502.Tests
{
    [TestClass]
    public class OpCodeTests
    {
        /// <summary>
        /// This method tests the ORA method, using indirect addressing mode.
        ///
        /// Should take around 5 cycles.
        /// </summary>
        [TestMethod]
        public void Test_Opcode_ORA_Indirect()
        {
            var ram = new Ram.Ram(0xFFFF);
            ram[0xFFFC] = 0xFA;
            ram[0xFFFD] = 0xCA;
            
            
            var processor = new M6502();
            processor.RegisterDevice(ram);

            //Processor should be in a 'reset' state
            Assert.AreEqual(0xFFFC, processor.AddressBusAddress);
            Assert.AreEqual(0, processor.DataBusData);
            
            //Clock to get the low byte of pc
            processor.Clock();
            ram.Clock();
            
            Assert.AreEqual(0xFFFD, processor.AddressBusAddress);
            Assert.AreEqual(0xFA, processor.DataBusData);
            
            //clock to get the hi byte of address.
            processor.Clock();
            ram.Clock();
            
            var expectedAddress = 0xCA << 8 | 0xFA;
            
            Assert.AreEqual(expectedAddress, processor.AddressBusAddress);
            Assert.AreEqual(0xCA, processor.DataBusData);
            
            //Clock to execute 1st ORA cycle
            processor.Clock();
            ram.Clock();
            
            Assert.AreEqual(0, processor.OpCode);
            Assert.AreEqual(false, processor.CpuRead);
            Assert.AreEqual(509, processor.AddressBusAddress);
            Assert.AreEqual(1, processor.DataBusData);
            Assert.AreEqual(0x00FC, processor.SP);
        }
    }
}
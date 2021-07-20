using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poly6502.Microprocessor;

namespace Poly6502.Tests
{
    [TestClass]
    public class BasicFunctionalTests
    {
        [TestMethod]
        public void Microprocessor_Should_Construct()
        {
            var processor = new M6502();
            
            Assert.IsNotNull(processor);
        }

        [TestMethod]
        public void Clocking_Should_Increment_PC()
        {
            var processor = new M6502();

            Assert.AreEqual(0xFFFC, processor.AddressBusAddress);
            
            processor.Clock();

            Assert.AreEqual(0xFFFD, processor.AddressBusAddress);
        }

        [TestMethod]
        public void Clocking_Should_Output_AddressBus()
        {
            var processor = new M6502();
            
            Assert.AreEqual(0xFFFC, processor.AddressBusAddress);
            
            processor.Clock();
            
            Assert.AreEqual(0xFFFD, processor.AddressBusAddress);
            
        }

        [TestMethod]
        public void Clocking_Should_Read_DataBus()
        {
            var processor = new M6502();
            
            var ram = new Ram.Ram(0xFFFF);
            ram[0xFFFC] = 0xAA;
            ram[0xFFFD] = 0xAB;
            
            processor.RegisterDevice(ram);
            
            Assert.AreEqual(0xFFFC, processor.AddressBusAddress);
            Assert.AreEqual(0, processor.DataBusData);
            
            processor.Clock();
            ram.Clock();
            
            Assert.AreEqual(0xFFFD, processor.AddressBusAddress);
            Assert.AreEqual(0xAA, processor.DataBusData);
            
            processor.Clock();
            ram.Clock();

            var expectedAddress = 0xAB << 8 | 0xAA;
            
            Assert.AreEqual(expectedAddress, processor.AddressBusAddress);
            Assert.AreEqual(0xAB, processor.DataBusData);
            
        }
    }
}
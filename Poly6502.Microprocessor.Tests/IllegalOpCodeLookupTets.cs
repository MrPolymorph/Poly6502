using NUnit.Framework;

namespace Poly6502.Microprocessor.Tests
{
    public class IllegalOpCodeLookupTets
    {
        private M6502 _m6502;
        
        [SetUp]
        public void Setup()
        {
            _m6502 = new M6502();
        }
        
        [Test]
        public void Lookup_Should_Return_JAM()
        {
            Operation result = _m6502.OpCodeLookupTable[0x02];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x12];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x22];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x32];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x42];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x52];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x62];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x72];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x92];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0xB2];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0xD2];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0xF2];
            Assert.True(result.CompareInstruction(_m6502.JAM, _m6502.IMP));
        }

        [Test]
        public void Lookup_Should_Return_SLO()
        {
            Operation result = _m6502.OpCodeLookupTable[0x03];
            Assert.True(result.CompareInstruction(_m6502.SLO, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0x07];
            Assert.True(result.CompareInstruction(_m6502.SLO, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x0F];
            Assert.True(result.CompareInstruction(_m6502.SLO, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x13];
            Assert.True(result.CompareInstruction(_m6502.SLO, _m6502.IZY));
            
            result = _m6502.OpCodeLookupTable[0x17];
            Assert.True(result.CompareInstruction(_m6502.SLO, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x1B];
            Assert.True(result.CompareInstruction(_m6502.SLO, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0x1F];
            Assert.True(result.CompareInstruction(_m6502.SLO, _m6502.ABX));
        }

        [Test]
        public void Lookup_Should_Return_NOP()
        {
            Operation result = _m6502.OpCodeLookupTable[0x04];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x0C];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x14];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x1A];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x1C];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ABX));
            
            result = _m6502.OpCodeLookupTable[0x34];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x3A];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x3C];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ABX));
            
            result = _m6502.OpCodeLookupTable[0x44];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x54];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x5A];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x5C];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ABX));
            
            result = _m6502.OpCodeLookupTable[0x64];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x74];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x7A];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0x7C];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ABX));
            
            result = _m6502.OpCodeLookupTable[0x80];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0x82];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0x89];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0xC2];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0xD4];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0xDA];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0xDC];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ABX));
            
            result = _m6502.OpCodeLookupTable[0xE2];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0xEA];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0xF4];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0xFA];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMP));
            
            result = _m6502.OpCodeLookupTable[0xFC];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.ABX));
        }

        [Test]
        public void Lookup_Should_Return_ANC()
        {
            Operation result = _m6502.OpCodeLookupTable[0x0B];
            Assert.True(result.CompareInstruction(_m6502.ANC, _m6502.IMM));

            result = _m6502.OpCodeLookupTable[0x2B];
            Assert.True(result.CompareInstruction(_m6502.ANC2, _m6502.IMM));
        }
        
        [Test]
        public void Lookup_Should_Return_RLA()
        {
            Operation result = _m6502.OpCodeLookupTable[0x27];
            Assert.True(result.CompareInstruction(_m6502.RLA, _m6502.ZPA));

            result = _m6502.OpCodeLookupTable[0x37];
            Assert.True(result.CompareInstruction(_m6502.RLA, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x2F];
            Assert.True(result.CompareInstruction(_m6502.RLA, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x3F];
            Assert.True(result.CompareInstruction(_m6502.RLA, _m6502.ABX));
            
            result = _m6502.OpCodeLookupTable[0x3B];
            Assert.True(result.CompareInstruction(_m6502.RLA, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0x23];
            Assert.True(result.CompareInstruction(_m6502.RLA, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0x33];
            Assert.True(result.CompareInstruction(_m6502.RLA, _m6502.IZY));
        }

        [Test]
        public void Lookup_Should_Return_SRE()
        {
            Operation result = _m6502.OpCodeLookupTable[0x47];
            Assert.True(result.CompareInstruction(_m6502.SRE, _m6502.ZPA));

            result = _m6502.OpCodeLookupTable[0x57];
            Assert.True(result.CompareInstruction(_m6502.SRE, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x4F];
            Assert.True(result.CompareInstruction(_m6502.SRE, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x5F];
            Assert.True(result.CompareInstruction(_m6502.SRE, _m6502.ABX));
            
            result = _m6502.OpCodeLookupTable[0x5B];
            Assert.True(result.CompareInstruction(_m6502.SRE, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0x43];
            Assert.True(result.CompareInstruction(_m6502.SRE, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0x53];
            Assert.True(result.CompareInstruction(_m6502.SRE, _m6502.IZY));
        }

        [Test]
        public void Lookup_Should_Return_ALR()
        {
            Operation result = _m6502.OpCodeLookupTable[0x4B];
            Assert.True(result.CompareInstruction(_m6502.ALR, _m6502.IMM));
        }
        
        [Test]
        public void Lookup_Should_Return_RRA()
        {
            Operation result = _m6502.OpCodeLookupTable[0x67];
            Assert.True(result.CompareInstruction(_m6502.RRA, _m6502.ZPA));

            result = _m6502.OpCodeLookupTable[0x77];
            Assert.True(result.CompareInstruction(_m6502.RRA, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x6F];
            Assert.True(result.CompareInstruction(_m6502.RRA, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x7F];
            Assert.True(result.CompareInstruction(_m6502.RRA, _m6502.ABX));
            
            result = _m6502.OpCodeLookupTable[0x7B];
            Assert.True(result.CompareInstruction(_m6502.RRA, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0x63];
            Assert.True(result.CompareInstruction(_m6502.RRA, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0x73];
            Assert.True(result.CompareInstruction(_m6502.RRA, _m6502.IZY));
        }
        
        [Test]
        public void Lookup_Should_Return_ARR()
        {
            Operation result = _m6502.OpCodeLookupTable[0x6B];
            Assert.True(result.CompareInstruction(_m6502.ARR, _m6502.IMM));
        }
        
        [Test]
        public void Lookup_Should_Return_SAX()
        {
            Operation result = _m6502.OpCodeLookupTable[0x87];
            Assert.True(result.CompareInstruction(_m6502.SAX, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x97];
            Assert.True(result.CompareInstruction(_m6502.SAX, _m6502.ZPY));
            
            result = _m6502.OpCodeLookupTable[0x8F];
            Assert.True(result.CompareInstruction(_m6502.SAX, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x83];
            Assert.True(result.CompareInstruction(_m6502.SAX, _m6502.IZX));
        }
        
        [Test]
        public void Lookup_Should_Return_ANE()
        {
            Operation result = _m6502.OpCodeLookupTable[0x8B];
            Assert.True(result.CompareInstruction(_m6502.ANE, _m6502.IMM));
        }
        
        [Test]
        public void Lookup_Should_Return_SHA()
        {
            Operation result = _m6502.OpCodeLookupTable[0x9F];
            Assert.True(result.CompareInstruction(_m6502.SHA, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0x93];
            Assert.True(result.CompareInstruction(_m6502.SHA, _m6502.IZY));
        }
        
        [Test]
        public void Lookup_Should_Return_TAS()
        {
            Operation result = _m6502.OpCodeLookupTable[0x9B];
            Assert.True(result.CompareInstruction(_m6502.TAS, _m6502.ABY));
        }
        
        [Test]
        public void Lookup_Should_Return_SHY()
        {
            Operation result = _m6502.OpCodeLookupTable[0x9C];
            Assert.True(result.CompareInstruction(_m6502.SHY, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_SHX()
        {
            Operation result = _m6502.OpCodeLookupTable[0x9E];
            Assert.True(result.CompareInstruction(_m6502.SHX, _m6502.ABY));
        }
        
        [Test]
        public void Lookup_Should_Return_LAX()
        {
            Operation result = _m6502.OpCodeLookupTable[0xA7];
            Assert.True(result.CompareInstruction(_m6502.LAX, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0xB7];
            Assert.True(result.CompareInstruction(_m6502.LAX, _m6502.ZPY));
            
            result = _m6502.OpCodeLookupTable[0xAF];
            Assert.True(result.CompareInstruction(_m6502.LAX, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0xBF];
            Assert.True(result.CompareInstruction(_m6502.LAX, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0xA3];
            Assert.True(result.CompareInstruction(_m6502.LAX, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0xB3];
            Assert.True(result.CompareInstruction(_m6502.LAX, _m6502.IZY));
        }

        [Test]
        public void Lookup_Should_Return_LXA()
        {
            Operation result = _m6502.OpCodeLookupTable[0xAB];
            Assert.True(result.CompareInstruction(_m6502.LXA, _m6502.IMM));
        }
        
        [Test]
        public void Lookup_Should_Return_LAS()
        {
            Operation result = _m6502.OpCodeLookupTable[0xBB];
            Assert.True(result.CompareInstruction(_m6502.LAS, _m6502.ABY));
        }
        
        [Test]
        public void Lookup_Should_Return_DCP()
        {
            Operation result = _m6502.OpCodeLookupTable[0xC7];
            Assert.True(result.CompareInstruction(_m6502.DCP, _m6502.ZPA));

            result = _m6502.OpCodeLookupTable[0xD7];
            Assert.True(result.CompareInstruction(_m6502.DCP, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0xCF];
            Assert.True(result.CompareInstruction(_m6502.DCP, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0xDF];
            Assert.True(result.CompareInstruction(_m6502.DCP, _m6502.ABX));
            
            result = _m6502.OpCodeLookupTable[0xDB];
            Assert.True(result.CompareInstruction(_m6502.DCP, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0xC3];
            Assert.True(result.CompareInstruction(_m6502.DCP, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0xD3];
            Assert.True(result.CompareInstruction(_m6502.DCP, _m6502.IZY));
        }
        
        [Test]
        public void Lookup_Should_Return_ISC()
        {
            Operation result = _m6502.OpCodeLookupTable[0xE7];
            Assert.True(result.CompareInstruction(_m6502.ISC, _m6502.ZPA));

            result = _m6502.OpCodeLookupTable[0xF7];
            Assert.True(result.CompareInstruction(_m6502.ISC, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0xEF];
            Assert.True(result.CompareInstruction(_m6502.ISC, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0xFF];
            Assert.True(result.CompareInstruction(_m6502.ISC, _m6502.ABX));
            
            result = _m6502.OpCodeLookupTable[0xFB];
            Assert.True(result.CompareInstruction(_m6502.ISC, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0xE3];
            Assert.True(result.CompareInstruction(_m6502.ISC, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0xF3];
            Assert.True(result.CompareInstruction(_m6502.ISC, _m6502.IZY));
        }
    }
}
using NUnit.Framework;

namespace Poly6502.Microprocessor.Tests
{
    public class Tests
    {
        private M6502 _m6502;
        
        [SetUp]
        public void Setup()
        {
            _m6502 = new M6502();
        }

        [Test]
        public void Lookup_Should_Return_BRK()
        {
            Operation result = _m6502.OpCodeLookupTable[0x00];

            Assert.True(result.CompareInstruction(_m6502.BRK, _m6502.IMP));
        }

        [Test]
        public void Lookup_Should_Return_ORA()
        {
            Operation result = _m6502.OpCodeLookupTable[0x01];
            Assert.True(result.CompareInstruction(_m6502.ORA, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0x05];
            Assert.True(result.CompareInstruction(_m6502.ORA, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x09];
            Assert.True(result.CompareInstruction(_m6502.ORA, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0x0D];
            Assert.True(result.CompareInstruction(_m6502.ORA, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x11];
            Assert.True(result.CompareInstruction(_m6502.ORA, _m6502.IZY));
            
            result = _m6502.OpCodeLookupTable[0x15];
            Assert.True(result.CompareInstruction(_m6502.ORA, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x19];
            Assert.True(result.CompareInstruction(_m6502.ORA, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0x1D];
            Assert.True(result.CompareInstruction(_m6502.ORA, _m6502.ABX));
        }

        [Test]
        public void Lookup_Should_Return_ASL()
        {
            Operation result = _m6502.OpCodeLookupTable[0x06];
            Assert.True(result.CompareInstruction(_m6502.ASL, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x0A];
            Assert.True(result.CompareInstruction(_m6502.ASL, _m6502.ACC));
            
            result = _m6502.OpCodeLookupTable[0x0E];
            Assert.True(result.CompareInstruction(_m6502.ASL, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x16];
            Assert.True(result.CompareInstruction(_m6502.ASL, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x1E];
            Assert.True(result.CompareInstruction(_m6502.ASL, _m6502.ABX));
        }

        [Test]
        public void Lookup_Should_Return_PHP()
        {
            Operation result = _m6502.OpCodeLookupTable[0x08];
            Assert.True(result.CompareInstruction(_m6502.PHP, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_BPL()
        {
            Operation result = _m6502.OpCodeLookupTable[0x10];
            Assert.True(result.CompareInstruction(_m6502.BPL, _m6502.REL));
        }
        
        [Test]
        public void Lookup_Should_Return_CLC()
        {
            Operation result = _m6502.OpCodeLookupTable[0x18];
            Assert.True(result.CompareInstruction(_m6502.CLC, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_JSR()
        {
            Operation result = _m6502.OpCodeLookupTable[0x20];
            Assert.True(result.CompareInstruction(_m6502.JSR, _m6502.ABS));
        }

        [Test]
        public void Lookup_Should_Return_AND()
        {
            Operation result = _m6502.OpCodeLookupTable[0x21];
            Assert.True(result.CompareInstruction(_m6502.AND, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0x25];
            Assert.True(result.CompareInstruction(_m6502.AND, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x29];
            Assert.True(result.CompareInstruction(_m6502.AND, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0x2D];
            Assert.True(result.CompareInstruction(_m6502.AND, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x31];
            Assert.True(result.CompareInstruction(_m6502.AND, _m6502.IZY));
            
            result = _m6502.OpCodeLookupTable[0x35];
            Assert.True(result.CompareInstruction(_m6502.AND, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x39];
            Assert.True(result.CompareInstruction(_m6502.AND, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0x3D];
            Assert.True(result.CompareInstruction(_m6502.AND, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_BIT()
        {
            Operation result = _m6502.OpCodeLookupTable[0x24];
            Assert.True(result.CompareInstruction(_m6502.BIT, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x2C];
            Assert.True(result.CompareInstruction(_m6502.BIT, _m6502.ABS));
        }

        [Test]
        public void Lookup_Should_Return_ROL()
        {
            Operation result = _m6502.OpCodeLookupTable[0x26];
            Assert.True(result.CompareInstruction(_m6502.ROL, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x2A];
            Assert.True(result.CompareInstruction(_m6502.ROL, _m6502.ACC));
            
            result = _m6502.OpCodeLookupTable[0x2E];
            Assert.True(result.CompareInstruction(_m6502.ROL, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x36];
            Assert.True(result.CompareInstruction(_m6502.ROL, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x3E];
            Assert.True(result.CompareInstruction(_m6502.ROL, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_PLP()
        {
            Operation result = _m6502.OpCodeLookupTable[0x28];
            Assert.True(result.CompareInstruction(_m6502.PLP, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_BMI()
        {
            Operation result = _m6502.OpCodeLookupTable[0x30];
            Assert.True(result.CompareInstruction(_m6502.BMI, _m6502.REL));
        }
        
        [Test]
        public void Lookup_Should_Return_SEC()
        {
            Operation result = _m6502.OpCodeLookupTable[0x38];
            Assert.True(result.CompareInstruction(_m6502.SEC, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_RTI()
        {
            Operation result = _m6502.OpCodeLookupTable[0x40];
            Assert.True(result.CompareInstruction(_m6502.RTI, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_EOR()
        {
            Operation result = _m6502.OpCodeLookupTable[0x41];
            Assert.True(result.CompareInstruction(_m6502.EOR, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0x45];
            Assert.True(result.CompareInstruction(_m6502.EOR, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x49];
            Assert.True(result.CompareInstruction(_m6502.EOR, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0x4D];
            Assert.True(result.CompareInstruction(_m6502.EOR, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x51];
            Assert.True(result.CompareInstruction(_m6502.EOR, _m6502.IZY));
            
            result = _m6502.OpCodeLookupTable[0x55];
            Assert.True(result.CompareInstruction(_m6502.EOR, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x59];
            Assert.True(result.CompareInstruction(_m6502.EOR, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0x5D];
            Assert.True(result.CompareInstruction(_m6502.EOR, _m6502.ABX));
        }

        [Test]
        public void Lookup_Should_Return_LSR()
        {
            Operation result = _m6502.OpCodeLookupTable[0x46];
            Assert.True(result.CompareInstruction(_m6502.LSR, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x4A];
            Assert.True(result.CompareInstruction(_m6502.LSR, _m6502.ACC));
            
            result = _m6502.OpCodeLookupTable[0x4E];
            Assert.True(result.CompareInstruction(_m6502.LSR, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x56];
            Assert.True(result.CompareInstruction(_m6502.LSR, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x5E];
            Assert.True(result.CompareInstruction(_m6502.LSR, _m6502.ABX));
        }

        [Test]
        public void Lookup_Should_Return_PHA()
        {
            Operation result = _m6502.OpCodeLookupTable[0x48];
            Assert.True(result.CompareInstruction(_m6502.PHA, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_JMP()
        {
            Operation result = _m6502.OpCodeLookupTable[0x4C];
            Assert.True(result.CompareInstruction(_m6502.JMP, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x6C];
            Assert.True(result.CompareInstruction(_m6502.JMP, _m6502.IND));
        }
        
        [Test]
        public void Lookup_Should_Return_BVC()
        {
            Operation result = _m6502.OpCodeLookupTable[0x50];
            Assert.True(result.CompareInstruction(_m6502.BVC, _m6502.REL));
        }
        
        [Test]
        public void Lookup_Should_Return_CLI()
        {
            Operation result = _m6502.OpCodeLookupTable[0x58];
            Assert.True(result.CompareInstruction(_m6502.CLI, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_RTS()
        {
            Operation result = _m6502.OpCodeLookupTable[0x60];
            Assert.True(result.CompareInstruction(_m6502.RTS, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_ADC()
        {
            Operation result = _m6502.OpCodeLookupTable[0x61];
            Assert.True(result.CompareInstruction(_m6502.ADC, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0x65];
            Assert.True(result.CompareInstruction(_m6502.ADC, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x69];
            Assert.True(result.CompareInstruction(_m6502.ADC, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0x6D];
            Assert.True(result.CompareInstruction(_m6502.ADC, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x71];
            Assert.True(result.CompareInstruction(_m6502.ADC, _m6502.IZY));
            
            result = _m6502.OpCodeLookupTable[0x75];
            Assert.True(result.CompareInstruction(_m6502.ADC, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x79];
            Assert.True(result.CompareInstruction(_m6502.ADC, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0x7D];
            Assert.True(result.CompareInstruction(_m6502.ADC, _m6502.ABX));
        }

        [Test]
        public void Lookup_Should_Return_ROR()
        {
            Operation result = _m6502.OpCodeLookupTable[0x66];
            Assert.True(result.CompareInstruction(_m6502.ROR, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x6A];
            Assert.True(result.CompareInstruction(_m6502.ROR, _m6502.ACC));
            
            result = _m6502.OpCodeLookupTable[0x6E];
            Assert.True(result.CompareInstruction(_m6502.ROR, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x76];
            Assert.True(result.CompareInstruction(_m6502.ROR, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x7E];
            Assert.True(result.CompareInstruction(_m6502.ROR, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_PLA()
        {
            Operation result = _m6502.OpCodeLookupTable[0x68];
            Assert.True(result.CompareInstruction(_m6502.PLA, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_BVS()
        {
            Operation result = _m6502.OpCodeLookupTable[0x70];
            Assert.True(result.CompareInstruction(_m6502.BVS, _m6502.REL));
        }
        
        [Test]
        public void Lookup_Should_Return_SEI()
        {
            Operation result = _m6502.OpCodeLookupTable[0x78];
            Assert.True(result.CompareInstruction(_m6502.SEI, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_STA()
        {
            Operation result = _m6502.OpCodeLookupTable[0x81];
            Assert.True(result.CompareInstruction(_m6502.STA, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0x85];
            Assert.True(result.CompareInstruction(_m6502.STA, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x8D];
            Assert.True(result.CompareInstruction(_m6502.STA, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x91];
            Assert.True(result.CompareInstruction(_m6502.STA, _m6502.IZY));
            
            result = _m6502.OpCodeLookupTable[0x95];
            Assert.True(result.CompareInstruction(_m6502.STA, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0x99];
            Assert.True(result.CompareInstruction(_m6502.STA, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0x9D];
            Assert.True(result.CompareInstruction(_m6502.STA, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_STY()
        {
            Operation result = _m6502.OpCodeLookupTable[0x84];
            Assert.True(result.CompareInstruction(_m6502.STY, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x8C];
            Assert.True(result.CompareInstruction(_m6502.STY, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x94];
            Assert.True(result.CompareInstruction(_m6502.STY, _m6502.ZPX));
        }
        
        [Test]
        public void Lookup_Should_Return_STX()
        {
            Operation result = _m6502.OpCodeLookupTable[0x86];
            Assert.True(result.CompareInstruction(_m6502.STX, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0x8E];
            Assert.True(result.CompareInstruction(_m6502.STX, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0x96];
            Assert.True(result.CompareInstruction(_m6502.STX, _m6502.ZPY));
        }
        
        [Test]
        public void Lookup_Should_Return_DEY()
        {
            Operation result = _m6502.OpCodeLookupTable[0x88];
            Assert.True(result.CompareInstruction(_m6502.DEY, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_TXA()
        {
            Operation result = _m6502.OpCodeLookupTable[0x8A];
            Assert.True(result.CompareInstruction(_m6502.TXA, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_BCC()
        {
            Operation result = _m6502.OpCodeLookupTable[0x90];
            Assert.True(result.CompareInstruction(_m6502.BCC, _m6502.REL));
        }
        
        [Test]
        public void Lookup_Should_Return_TYA()
        {
            Operation result = _m6502.OpCodeLookupTable[0x98];
            Assert.True(result.CompareInstruction(_m6502.TYA, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_TXS()
        {
            Operation result = _m6502.OpCodeLookupTable[0x9A];
            Assert.True(result.CompareInstruction(_m6502.TXS, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_LDY()
        {
            Operation result = _m6502.OpCodeLookupTable[0xA0];
            Assert.True(result.CompareInstruction(_m6502.LDY, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0xA4];
            Assert.True(result.CompareInstruction(_m6502.LDY, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0xAC];
            Assert.True(result.CompareInstruction(_m6502.LDY, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0xB4];
            Assert.True(result.CompareInstruction(_m6502.LDY, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0xBC];
            Assert.True(result.CompareInstruction(_m6502.LDY, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_LDA()
        {
            Operation result = _m6502.OpCodeLookupTable[0xA1];
            Assert.True(result.CompareInstruction(_m6502.LDA, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0xA5];
            Assert.True(result.CompareInstruction(_m6502.LDA, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0xA9];
            Assert.True(result.CompareInstruction(_m6502.LDA, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0xAD];
            Assert.True(result.CompareInstruction(_m6502.LDA, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0xB1];
            Assert.True(result.CompareInstruction(_m6502.LDA, _m6502.IZY));
            
            result = _m6502.OpCodeLookupTable[0xB5];
            Assert.True(result.CompareInstruction(_m6502.LDA, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0xB9];
            Assert.True(result.CompareInstruction(_m6502.LDA, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0xBD];
            Assert.True(result.CompareInstruction(_m6502.LDA, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_TAX()
        {
            Operation result = _m6502.OpCodeLookupTable[0xAA];
            Assert.True(result.CompareInstruction(_m6502.TAX, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_LDX()
        {
            Operation result = _m6502.OpCodeLookupTable[0xA2];
            Assert.True(result.CompareInstruction(_m6502.LDX, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0xA6];
            Assert.True(result.CompareInstruction(_m6502.LDX, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0xAE];
            Assert.True(result.CompareInstruction(_m6502.LDX, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0xB6];
            Assert.True(result.CompareInstruction(_m6502.LDX, _m6502.ZPY));
            
            result = _m6502.OpCodeLookupTable[0xBE];
            Assert.True(result.CompareInstruction(_m6502.LDX, _m6502.ABY));
        }
        
        [Test]
        public void Lookup_Should_Return_BCS()
        {
            Operation result = _m6502.OpCodeLookupTable[0xB0];
            Assert.True(result.CompareInstruction(_m6502.BCS, _m6502.REL));
        }
        
        [Test]
        public void Lookup_Should_Return_CLV()
        {
            Operation result = _m6502.OpCodeLookupTable[0xB8];
            Assert.True(result.CompareInstruction(_m6502.CLV, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_TSX()
        {
            Operation result = _m6502.OpCodeLookupTable[0xBA];
            Assert.True(result.CompareInstruction(_m6502.TSX, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_CPY()
        {
            Operation result = _m6502.OpCodeLookupTable[0xC0];
            Assert.True(result.CompareInstruction(_m6502.CPY, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0xC4];
            Assert.True(result.CompareInstruction(_m6502.CPY, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0xCC];
            Assert.True(result.CompareInstruction(_m6502.CPY, _m6502.ABS));
        }
        
        [Test]
        public void Lookup_Should_Return_CMP()
        {
            Operation result = _m6502.OpCodeLookupTable[0xC1];
            Assert.True(result.CompareInstruction(_m6502.CMP, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0xC5];
            Assert.True(result.CompareInstruction(_m6502.CMP, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0xC9];
            Assert.True(result.CompareInstruction(_m6502.CMP, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0xCD];
            Assert.True(result.CompareInstruction(_m6502.CMP, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0xD1];
            Assert.True(result.CompareInstruction(_m6502.CMP, _m6502.IZY));
            
            result = _m6502.OpCodeLookupTable[0xD5];
            Assert.True(result.CompareInstruction(_m6502.CMP, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0xD9];
            Assert.True(result.CompareInstruction(_m6502.CMP, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0xDD];
            Assert.True(result.CompareInstruction(_m6502.CMP, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_DEC()
        {
            Operation result = _m6502.OpCodeLookupTable[0xC6];
            Assert.True(result.CompareInstruction(_m6502.DEC, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0xCE];
            Assert.True(result.CompareInstruction(_m6502.DEC, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0xD6];
            Assert.True(result.CompareInstruction(_m6502.DEC, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0xDE];
            Assert.True(result.CompareInstruction(_m6502.DEC, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_INY()
        {
            Operation result = _m6502.OpCodeLookupTable[0xC8];
            Assert.True(result.CompareInstruction(_m6502.INY, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_DEX()
        {
            Operation result = _m6502.OpCodeLookupTable[0xCA];
            Assert.True(result.CompareInstruction(_m6502.DEX, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_BNE()
        {
            Operation result = _m6502.OpCodeLookupTable[0xD0];
            Assert.True(result.CompareInstruction(_m6502.BNE, _m6502.REL));
        }
        
        [Test]
        public void Lookup_Should_Return_CLD()
        {
            Operation result = _m6502.OpCodeLookupTable[0xD8];
            Assert.True(result.CompareInstruction(_m6502.CLD, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_CPX()
        {
            Operation result = _m6502.OpCodeLookupTable[0xE0];
            Assert.True(result.CompareInstruction(_m6502.CPX, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0xE4];
            Assert.True(result.CompareInstruction(_m6502.CPX, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0xEC];
            Assert.True(result.CompareInstruction(_m6502.CPX, _m6502.ABS));
        }
        
        [Test]
        public void Lookup_Should_Return_SBC()
        {
            Operation result = _m6502.OpCodeLookupTable[0xE1];
            Assert.True(result.CompareInstruction(_m6502.SBC, _m6502.IZX));
            
            result = _m6502.OpCodeLookupTable[0xE5];
            Assert.True(result.CompareInstruction(_m6502.SBC, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0xE9];
            Assert.True(result.CompareInstruction(_m6502.SBC, _m6502.IMM));
            
            result = _m6502.OpCodeLookupTable[0xED];
            Assert.True(result.CompareInstruction(_m6502.SBC, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0xF1];
            Assert.True(result.CompareInstruction(_m6502.SBC, _m6502.IZY));
            
            result = _m6502.OpCodeLookupTable[0xF5];
            Assert.True(result.CompareInstruction(_m6502.SBC, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0xF9];
            Assert.True(result.CompareInstruction(_m6502.SBC, _m6502.ABY));
            
            result = _m6502.OpCodeLookupTable[0xFD];
            Assert.True(result.CompareInstruction(_m6502.SBC, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_INC()
        {
            Operation result = _m6502.OpCodeLookupTable[0xE6];
            Assert.True(result.CompareInstruction(_m6502.INC, _m6502.ZPA));
            
            result = _m6502.OpCodeLookupTable[0xEE];
            Assert.True(result.CompareInstruction(_m6502.INC, _m6502.ABS));
            
            result = _m6502.OpCodeLookupTable[0xF6];
            Assert.True(result.CompareInstruction(_m6502.INC, _m6502.ZPX));
            
            result = _m6502.OpCodeLookupTable[0xFE];
            Assert.True(result.CompareInstruction(_m6502.INC, _m6502.ABX));
        }
        
        [Test]
        public void Lookup_Should_Return_INX()
        {
            Operation result = _m6502.OpCodeLookupTable[0xE8];
            Assert.True(result.CompareInstruction(_m6502.INX, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_NOP()
        {
            Operation result = _m6502.OpCodeLookupTable[0xEA];
            Assert.True(result.CompareInstruction(_m6502.NOP, _m6502.IMP));
        }
        
        [Test]
        public void Lookup_Should_Return_BEQ()
        {
            Operation result = _m6502.OpCodeLookupTable[0xF0];
            Assert.True(result.CompareInstruction(_m6502.BEQ, _m6502.REL));
        }
        
        [Test]
        public void Lookup_Should_Return_SED()
        {
            Operation result = _m6502.OpCodeLookupTable[0xF8];
            Assert.True(result.CompareInstruction(_m6502.SED, _m6502.IMP));
        }
    }
}
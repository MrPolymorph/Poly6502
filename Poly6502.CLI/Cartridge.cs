using System;
using System.IO;
using System.Runtime.InteropServices;
using Poly6502.Utilities;

namespace Poly6502.CLI
{
    public class Cartridge : AbstractAddressDataBus
    {
        public byte MappedId { get; set; }
        public byte ProgramBanks { get; set; }
        public byte CharacterBanks { get; set; }

        public byte[] ProgramMemory { get; set; }
        public byte[] CharacterMemory { get; set; }

        public void LoadProgram()
        {
            Header header;
            using (var fs = new FileStream("/home/kris/Projects/Poly6502/ROMS/CPU_Tests/nestest.nes", FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    byte[] allData = File.ReadAllBytes("/home/kris/Projects/Poly6502/ROMS/CPU_Tests/nestest.nes");
                    byte[] buffer = br.ReadBytes(Marshal.SizeOf(typeof(Header)));

                    var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    header = (Header) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Header));
                    handle.Free();

                    var mapper = ((header.Mapper2 >> 4) << 4) | (header.Mapper1 >> 4);

                    if ((mapper & 0x04) != 0)
                        br.BaseStream.Seek(512, SeekOrigin.Current);
                    else
                        br.BaseStream.Seek(16, SeekOrigin.Begin);

                    ProgramBanks = header.ProgramRomSize;
                    ProgramMemory = new byte[(ProgramBanks * 16384)];
                    br.Read(ProgramMemory, 0, ProgramMemory.Length);

                    CharacterBanks = header.CharacterRomSize;
                    CharacterMemory = new byte[(CharacterBanks * 8192)];
                    br.Read(CharacterMemory, 0, CharacterMemory.Length);
                }
            }
        }
        

        public override void Clock()
        {
            if (CpuRead)
                Read();
            else
                Write();
        }


        public override void SetRW(bool rw)
        {
            CpuRead = rw;

            if (CpuRead)
                Read();
            else
                Write();
        }

        protected override void OutputDataToDatabus()
        {
            if (CpuRead)
                Read();
            else
                Write();
            
            base.OutputDataToDatabus();
        }

        private void Read()
        {
            //check mapper read
            if(AddressBusAddress >= 0x8000 && AddressBusAddress <= 0xFFFF)
            {
                var mappedAddress = AddressBusAddress & (ProgramBanks > 1 ? 0x7FFF : 0x3FFF);
                DataBusData = ProgramMemory[mappedAddress];
                SetPropagation(true);
            }
            else
            {
                SetPropagation(false);
            }
        }

        private void Write()
        {
            //Cartridge ram is ranged between 0x4020-0xFFFF 
            if (AddressBusAddress >= 0x4020 && AddressBusAddress <= (0x4020 + ProgramMemory.Length))
            {
                //normalise access into cartridge memory
                var address = AddressBusAddress - ProgramMemory.Length;
                ProgramMemory[address] = DataBusData;
            }
        }
        

        public byte Peek(ushort address)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {
                var mappedAddress = address & (ProgramBanks > 1 ? 0x7FFF : 0x3FFF);
                return ProgramMemory[mappedAddress];
            }

            return 0;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Header
    {
        [FieldOffset(0)] public char Name;

        [FieldOffset(4)] public byte ProgramRomSize;

        [FieldOffset(5)] public byte CharacterRomSize;

        [FieldOffset(6)] public byte Mapper1;

        [FieldOffset(7)] public byte Mapper2;

        [FieldOffset(8)] public byte ProgramRamSize;

        [FieldOffset(9)] public byte TvSystem1;

        [FieldOffset(10)] public byte TvSystem2;

        [FieldOffset(11)] public char Padding;
    }
}
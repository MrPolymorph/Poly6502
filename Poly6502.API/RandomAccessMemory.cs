using Poly6502.Utilities;

namespace PolyNES.WRam
{
    public class RandomAccessMemory : AbstractAddressDataBus
    {
        private const int RamSize = 2048;
        
        private byte[] _ram;

        public byte this[int i]
        {
            get { return _ram[i]; }
            set { _ram[i] = value; }
        }

        public RandomAccessMemory(int size = RamSize)
        {
            MinAddressableRange = 0x0000;
            MaxAddressableRange = 0x1FFF;
            
            _ram = new byte[RamSize];

            for (int i = 0; i < size; i++)
            {
                _ram[i] = 0;
            }
        }

        public override void Clock()
        {
        }

        public void SetRam(byte[] ram)
        {
            _ram = ram;
        }
        
        public override void SetRW(bool rw)
        {
            CpuRead = rw;
        }

        public override byte Read(ushort address, bool rOnly = false)
        {
            //check if the address is meant for us?
            if (AddressBusAddress < MaxAddressableRange)
            {
                var actualAddress = address & 0x7FF;
                return _ram[actualAddress];
            }

            return DataBusData;
        }

        public override void Write(ushort address, byte data)
        {
            //check if the address is meant for us?
            if (address < MaxAddressableRange)
            {
                var actualAddress = address & 0x7FF;
                _ram[actualAddress] = data;
            }
        }

        public byte Peek(ushort address)
        {
            if (address < _ram.Length)
                return _ram[address];

            return 0;
        }

        public byte[] Take(ushort start, ushort end)
        {
            if (_ram.Length > end)
                return _ram[start..end];

            throw new IndexOutOfRangeException();
        }
    }
}
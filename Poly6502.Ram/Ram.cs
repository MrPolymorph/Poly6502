using Poly6502.Utilities;

namespace Poly6502.Ram
{
    public class Ram : AbstractAddressDataBus
    {
        private byte[] _ram;

        public byte this[int i]
        {
            get { return _ram[i];}
            set { _ram[i] = value; }
        }

        public Ram(int size)
        {
            _ram = new byte[size];

            for (int i = 0; i < size; i++)
            {
                _ram[i] = 0;
            }
        }

        public override void SetRW(bool rw)
        {
            CpuRead = rw;
        }

        private void Write()
        {
            //check if the address is meant for us?
            if(AddressBusAddress < _ram.Length)
            {
                _ram[AddressBusAddress] = DataBusData;
            }
        }

        private void Read()
        {
            if (AddressBusAddress <= _ram.Length)
            {
                DataBusData = _ram[AddressBusAddress & 0x07FF];
                OutputDataToDatabus();
            }
        }

        public override void Clock()
        {
            if(CpuRead)
                Read();
            else //read any data into ram
                Write();
        }
    }
}
using System;
using Poly6502.Utilities;

namespace Poly6502.Ram
{
    public class Ram : AbstractAddressDataBus
    {
        private byte[] _ram;

        public Ram(int size)
        {
            _ram = new byte[0xFFFF];

             _ram[0xFFFC] = 0x05;
             _ram[0xFFFD] = 0xFC;
             _ram[0x0505] = 0xAA;
        }

        public override void SetRW(bool rw)
        {
            _cpuRead = rw;

            if (_cpuRead)
                OutputDataToDatabus();
        }

        public override void Clock()
        {
            if(_cpuRead)
                OutputDataToDatabus();
            else //read any data into ram
            {
                _ram[_addressBusAddress] = _dataBusData;
            }
        }

        private void OutputDataToDatabus()
        {
            var data = _ram[_addressBusAddress];

            foreach(var device in _dataCompatibleDevices)
            {
                for (int i = 0; i < 8; i++)
                {
                    ushort pw = (ushort)Math.Pow(2, i);
                    var bit = (ushort)(data & pw) >> i;
                    device.DataBusLines[i](bit);
                }
            }
        }
        
        
    }
}
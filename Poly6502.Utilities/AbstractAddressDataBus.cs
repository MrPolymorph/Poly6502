using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Poly6502.Interfaces;

namespace Poly6502.Utilities
{
    public abstract class AbstractAddressDataBus : IAddressBusCompatible, IDataBusCompatible
    {
        public Dictionary<int, Action<float>> AddressBusLines { get; set; }
        public Dictionary<int, Action<float>> DataBusLines { get; set; }

        protected IList<IAddressBusCompatible> _addressCompatibleDevices;
        protected IList<IDataBusCompatible> _dataCompatibleDevices;

        protected byte _dataBusData;
        protected bool _cpuRead;
        protected ushort _addressLocation;

        public AbstractAddressDataBus()
        {
            DataBusLines = new Dictionary<int, Action<float>>();
            AddressBusLines = new Dictionary<int, Action<float>>();

            _addressCompatibleDevices = new List<IAddressBusCompatible>();
            _dataCompatibleDevices = new Collection<IDataBusCompatible>();
            
            //setup data lines
            for (int i = 0; i < 8; i++)
            {
                var i1 = i;
                DataBusLines.Add(i, (inputVoltage) =>
                {
                    if (inputVoltage > 0)
                        _dataBusData |= (byte) (1 << i1);
                    else
                        _dataBusData &= (byte) ~(1 << i1);
                });
            }

            
            //setup address lines
            for (int i = 0; i < 16; i++)
            {
                var i1 = i;
                AddressBusLines.Add(i, (inputVoltage) =>
                {
                    if (inputVoltage > 0)
                        _addressLocation |= (ushort) (1 << i1);
                    else
                        _addressLocation &= (ushort) ~(1 << i1);
                });
            }
        }

        public void RegisterDataCompatibleDevice(IDataBusCompatible device)
        {
            _dataCompatibleDevices.Add(device);
        }

        public abstract void SetRW(bool rw);


        public void RegisterAddressCompatibleDevice(IAddressBusCompatible device)
        {
            _addressCompatibleDevices.Add(device);
        }

        public abstract void Clock();
    }
}
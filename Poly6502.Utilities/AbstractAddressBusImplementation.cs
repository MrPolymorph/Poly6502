using System;
using System.Collections.Generic;
using Poly6502.Interfaces;

namespace Poly6502.Utilities
{
    public abstract class  AbstractAddressBusImplementation : IAddressBusCompatible
    {
        public Dictionary<int, Action<float>> AddressBusLines { get; set; }
        protected ushort _addressLocation;
        protected IList<IAddressBusCompatible> _addressCompatibleDevices;

        protected AbstractAddressBusImplementation()
        {
            AddressBusLines = new Dictionary<int, Action<float>>();
            _addressCompatibleDevices = new List<IAddressBusCompatible>();

            for (int i = 0; i < 15; i++)
            {
                var i1 = i;
                AddressBusLines.Add(i, (inputVoltage) =>
                {
                    if(inputVoltage > 0)
                        _addressLocation |= (ushort)(1 << i1);
                    else
                        _addressLocation &= (ushort)~(1 << i1);
                });
            }
        }
        
        public void RegisterAddressCompatibleDevice(IAddressBusCompatible device)
        {
            _addressCompatibleDevices.Add(device);
        }

        public abstract void Clock();
    }
}
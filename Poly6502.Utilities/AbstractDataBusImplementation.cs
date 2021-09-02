using System;
using System.Collections.Generic;
using Poly6502.Interfaces;

namespace Poly6502.Utilities
{
    public abstract class AbstractDataBusImplementation : IDataBusCompatible
    {
        public Dictionary<int, Action<float>> DataBusLines { get; set; }
        
        protected byte _dataBusData;
        protected bool _cpuRead;
        protected IList<IDataBusCompatible> _dataBusCompatiblesDevices;

        protected AbstractDataBusImplementation()
        {
            DataBusLines = new Dictionary<int, Action<float>>();
            _dataBusCompatiblesDevices = new List<IDataBusCompatible>();

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
        }
        
        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void SetRW(bool rw)
        {
            _cpuRead = rw;
        }

        public void PropagationOverride(bool ovr, object invoker)
        {
            
        }

        public byte Read(ushort address)
        {
            throw new NotImplementedException();
        }

        public void Write(ushort address, byte data)
        {
            throw new NotImplementedException();
        }

        public abstract void Clock();
    }
}
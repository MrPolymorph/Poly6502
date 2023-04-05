using System;
using System.Collections.Generic;
using Poly6502.Microprocessor.Interfaces;

namespace Poly6502.Microprocessor.Utilities
{
    public abstract class AbstractDataBusImplementation : IDataBusCompatible
    {
        private bool _ignorePropagation;
        public bool PropagationOverridden { get; private set; }
        public Dictionary<int, Action<float>> DataBusLines { get; set; }
        
        protected byte DataBusData;
        protected bool CpuRead;
        protected IList<IDataBusCompatible> DataBusCompatiblesDevices;

        protected AbstractDataBusImplementation()
        {
            DataBusLines = new Dictionary<int, Action<float>>();
            DataBusCompatiblesDevices = new List<IDataBusCompatible>();

            PropagationOverridden = false;
            
            for (int i = 0; i < 8; i++)
            {
                var i1 = i;
                DataBusLines.Add(i, (inputVoltage) =>
                {
                    if (inputVoltage > 0)
                        DataBusData |= (byte) (1 << i1);
                    else
                        DataBusData &= (byte) ~(1 << i1);
                });
            }
        }
        
        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void SetRW(bool rw)
        {
            CpuRead = rw;
        }

        public void PropagationOverride(bool ovr, object invoker)
        {
            if (invoker != this && !_ignorePropagation)
            {
                PropagationOverridden = ovr;
            }
        }

        public byte Read(ushort address, bool ronly = false)
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
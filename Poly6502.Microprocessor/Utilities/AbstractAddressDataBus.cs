using System;
using System.Collections.Generic;
using Poly6502.Microprocessor.Interfaces;

namespace Poly6502.Microprocessor.Utilities
{
    public abstract class AbstractAddressDataBus : IAddressBusCompatible, IDataBusCompatible
    {
        private bool _ignorePropagation;

        private readonly HashSet<IAddressBusCompatible> _addressCompatibleDevices;
        protected readonly HashSet<IDataBusCompatible> DataCompatibleDevices;
        protected byte DataBusData { get; set; }
        protected bool CpuRead { get; set; }
        protected ushort RelativeAddress { get; set; }
        
        
        public ushort MaxAddressableRange { get; protected set; }
        public ushort MinAddressableRange { get; protected set; }

        public ushort AddressBusAddress { get; set; }


        public bool PropagationOverridden { get; private set; }
        public Dictionary<int, Action<float>> DataBusLines { get; set; }
        public Dictionary<int, Action<float>> AddressBusLines { get; set; }

        protected AbstractAddressDataBus()
        {
            DataBusData = 0;
            CpuRead = true;
            DataBusLines = new Dictionary<int, Action<float>>();
            AddressBusLines = new Dictionary<int, Action<float>>();
            
            _ignorePropagation = false;
            _addressCompatibleDevices = new HashSet<IAddressBusCompatible>();
            DataCompatibleDevices = new HashSet<IDataBusCompatible>();

            PropagationOverridden = false;

            //setup data lines
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


#if EMULATE_PIN_OUTPUT
            //setup address lines
            for (int i = 0; i < 15; i++)
            {
                var i1 = i;
                AddressBusLines.Add(i, (inputVoltage) =>
                {
                    if(inputVoltage > 0)
                        _addressBusAddress |= (ushort)(1 << i1);
                    else
                        _addressBusAddress &= (ushort)~(1 << i1);
                });
            }
#endif
        }

        public void RegisterDevice(IAddressBusCompatible device)
        {
            if (!_addressCompatibleDevices.Contains(device))
            {
                _addressCompatibleDevices.Add(device);
                
                if (device is IDataBusCompatible compatible)
                {
                    if (!DataCompatibleDevices.Contains(compatible))
                        DataCompatibleDevices.Add(compatible);
                }
            }
        }

        public void RegisterDevice(IDataBusCompatible device,  int propagationPriority)
        {
            if (!DataCompatibleDevices.Contains(device))
            {
                DataCompatibleDevices.Add(device);
                
                if (device is IAddressBusCompatible compatible)
                {
                    if (!_addressCompatibleDevices.Contains(compatible))
                        _addressCompatibleDevices.Add(compatible);
                }
            }
        }

        public void SetAddress(ushort address)
        {
            AddressBusAddress = address;
        }

        protected void IgnorePropagation(bool ovrd)
        {
            _ignorePropagation = ovrd;
        }

        public void PropagationOverride(bool ovr, object invoker)
        {
            if (invoker != this && !_ignorePropagation)
                PropagationOverridden = ovr;
        }

        protected virtual void OutputDataToDatabus()
        {
            OutputDataToDatabus(AddressBusAddress);
        }
        
        protected virtual void OutputDataToDatabus(ushort address)
        {
            if (!PropagationOverridden)
            {
                foreach (var kvp in DataCompatibleDevices)
                {
#if EMULATE_PIN_OUTPUT
                    for (int i = 0; i < 8; i++)
                    {
                        ushort pw = (ushort) Math.Pow(2, i);
                        var bit = (ushort) (DataBusData & pw) >> i;
                        device.DataBusLines[i](bit);
                    }
#else
                    if (CpuRead)
                        DataBusData = kvp.Read(address);
                    else
                        kvp.Write(address, DataBusData);
#endif
                }
            }
        }

        protected void SetPropagation(bool propagate)
        {
            foreach (var device in DataCompatibleDevices)
            {
                device.PropagationOverride(propagate, this);
            }
        }

        public abstract byte Read(ushort address, bool ronly = false);
        public abstract void Write(ushort address, byte data);


        public abstract void Clock();
        public abstract void SetRW(bool rw);
    }
}
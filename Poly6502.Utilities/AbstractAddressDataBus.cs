using System;
using System.Collections.Generic;
using System.Linq;
using Poly6502.Interfaces;

namespace Poly6502.Utilities
{
    public abstract class AbstractAddressDataBus : IAddressBusCompatible, IDataBusCompatible
    {
        private bool _ignorePropagation;
        
        protected readonly IDictionary<int, IAddressBusCompatible> _addressCompatibleDevices;
        protected readonly IDictionary<int, IDataBusCompatible> _dataCompatibleDevices;
        protected byte DataBusData { get; set; }
        protected bool CpuRead { get; set; }
        protected ushort RelativeAddress { get; set; }
        
        public ushort MaxAddressableRange { get; protected set; }
        public ushort MinAddressableRange { get; protected set; }
        
        public ushort AddressBusAddress { get; protected set; }
        public Dictionary<int, Action<float>> AddressBusLines { get; set; }
        
        public bool PropagationOverridden { get; private set; }
        public Dictionary<int, Action<float>> DataBusLines { get; set; }


        protected AbstractAddressDataBus()
        {
            CpuRead = true;
            DataBusLines = new Dictionary<int, Action<float>>();
            AddressBusLines = new Dictionary<int, Action<float>>();
            
            _ignorePropagation = false;
            _addressCompatibleDevices = new Dictionary<int, IAddressBusCompatible>();
            _dataCompatibleDevices = new Dictionary<int, IDataBusCompatible>();

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

        public void RegisterDevice(IAddressBusCompatible device, int propagationPriority)
        {
            if (!_addressCompatibleDevices.ContainsKey(propagationPriority))
            {
                _addressCompatibleDevices.Add(propagationPriority, device);
            }

            if (device is IDataBusCompatible compatible)
            {
                if (!_dataCompatibleDevices.ContainsKey(propagationPriority))
                    _dataCompatibleDevices.Add(propagationPriority, compatible);
            }
        }

        public void RegisterDevice(IDataBusCompatible device,  int propagationPriority)
        {
            if (!_dataCompatibleDevices.ContainsKey(propagationPriority))
            {
                _dataCompatibleDevices.Add(propagationPriority, device);
            }

            if (device is IAddressBusCompatible compatible)
            {
                if (!_addressCompatibleDevices.ContainsKey(propagationPriority))
                    _addressCompatibleDevices.Add(propagationPriority, compatible);
            }
        }

        public void RegisterDevice(IAddressBusCompatible device)
        {
            throw new NotImplementedException();
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
            if (!PropagationOverridden)
            {
                foreach (var kvp in _dataCompatibleDevices.OrderBy(x => x.Key))
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
                        DataBusData = kvp.Value.Read(AddressBusAddress);
                    else
                        kvp.Value.Write(AddressBusAddress, DataBusData);
#endif
                }
            }
        }

        protected void SetPropagation(bool propagate)
        {
            foreach (var device in _dataCompatibleDevices)
            {
                device.Value.PropagationOverride(propagate, this);
            }
        }

        public abstract byte Read(ushort address);
        public abstract void Write(ushort address, byte data);


        public abstract void Clock();
        public abstract void SetRW(bool rw);
    }
}
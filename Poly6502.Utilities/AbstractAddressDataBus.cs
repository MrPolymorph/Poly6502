using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Poly6502.Interfaces;

namespace Poly6502.Utilities
{
    public abstract class AbstractAddressDataBus : IAddressBusCompatible, IDataBusCompatible
    {
        private bool _overrideOutput;
        private bool _ignorePropagation;
        
        public Dictionary<int, Action<float>> AddressBusLines { get; set; }
        public Dictionary<int, Action<float>> DataBusLines { get; set; }

        protected IList<IAddressBusCompatible> _addressCompatibleDevices;
        protected IList<IDataBusCompatible> _dataCompatibleDevices;

        public byte DataBusData { get; protected set; }
        public bool CpuRead { get; protected set; }
        public ushort AddressBusAddress { get; protected set; }
        public ushort RelativeAddress { get; protected set; }

        public AbstractAddressDataBus()
        {
            CpuRead = true;
            DataBusLines = new Dictionary<int, Action<float>>();
            AddressBusLines = new Dictionary<int, Action<float>>();

            _overrideOutput = false;
            _ignorePropagation = false;
            _addressCompatibleDevices = new List<IAddressBusCompatible>();
            _dataCompatibleDevices = new Collection<IDataBusCompatible>();
            
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

            
            //setup address lines
            for (int i = 0; i < 16; i++)
            {
                var i1 = i;
                AddressBusLines.Add(i, (inputVoltage) =>
                {
                    if (inputVoltage > 0)
                        AddressBusAddress |= (ushort) (1 << i1);
                    else
                        AddressBusAddress &= (ushort) ~(1 << i1);

                    if (i1 == 15)
                    {
                        Clock();
                        //OutputDataToDatabus();
                    }
                });
            }
        }

        public void RegisterDevice(AbstractAddressDataBus device)
        {
            var deviceAlreadyAdded = false;

            if (!_addressCompatibleDevices.Contains(device))
                _addressCompatibleDevices.Add(device);
            else
                deviceAlreadyAdded = true;

            if (!_dataCompatibleDevices.Contains(device))
                _dataCompatibleDevices.Add(device);
            else
                deviceAlreadyAdded = true;
            
            if(!deviceAlreadyAdded)
                device.RegisterDevice(this);
        }
        
        public void RegisterDevice(IDataBusCompatible device)
        {
            _dataCompatibleDevices.Add(device);
            
            //check if the device is address bus compatible also
            if (device is IAddressBusCompatible)
            {
                if(!_addressCompatibleDevices.Contains((IAddressBusCompatible) device))
                    _addressCompatibleDevices.Add((IAddressBusCompatible) device);
            }
            
        }
        
        public void RegisterDevice(IAddressBusCompatible device)
        {
            _addressCompatibleDevices.Add(device);
            
            if (device is IDataBusCompatible)
            {
                if(!_dataCompatibleDevices.Contains((IDataBusCompatible) device))
                    _dataCompatibleDevices.Add((IDataBusCompatible) device);
            }
        }

        protected void IgnorePropagation(bool ovrd)
        {
            _ignorePropagation = ovrd;
        }
        
        public void PropagationOverride(bool ovr, object invoker)
        {
            if (invoker != this && !_ignorePropagation)
                _overrideOutput = ovr;
        }

        protected virtual void OutputDataToDatabus()
        {
            if (!_overrideOutput)
            {
                foreach (var device in _dataCompatibleDevices)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        ushort pw = (ushort) Math.Pow(2, i);
                        var bit = (ushort) (DataBusData & pw) >> i;
                        device.DataBusLines[i](bit);
                    }
                }
            }
        }

        protected void SetPropagation(bool propagate)
        {
            foreach (var device in _dataCompatibleDevices)
            {
                device.PropagationOverride(propagate, this);
            }
        }
        
        
        public abstract void Clock();
        public abstract void SetRW(bool rw);
    }
}
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
        private readonly IList<IAddressBusCompatible> _addressCompatibleDevices;

        protected readonly IList<IDataBusCompatible> DataCompatibleDevices;
        protected byte DataBusData { get; set; }
        protected bool CpuRead { get; set; }
        protected ushort AddressBusAddress { get; set; }
        protected ushort RelativeAddress { get; set; }
        public Dictionary<int, Action<float>> AddressBusLines { get; set; }
        public Dictionary<int, Action<float>> DataBusLines { get; set; }


        protected AbstractAddressDataBus()
        {
            CpuRead = true;
            DataBusLines = new Dictionary<int, Action<float>>();
            AddressBusLines = new Dictionary<int, Action<float>>();

            _overrideOutput = false;
            _ignorePropagation = false;
            _addressCompatibleDevices = new List<IAddressBusCompatible>();
            DataCompatibleDevices = new Collection<IDataBusCompatible>();

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
            _addressCompatibleDevices.Add(device);

            if (device is IDataBusCompatible)
            {
                if (!DataCompatibleDevices.Contains((IDataBusCompatible) device))
                    DataCompatibleDevices.Add((IDataBusCompatible) device);
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
                _overrideOutput = ovr;
        }


        protected virtual void OutputDataToDatabus()
        {
            if (!_overrideOutput)
            {
                foreach (var device in DataCompatibleDevices)
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
                        DataBusData = device.Read(AddressBusAddress);
                    else
                        device.Write(AddressBusAddress, DataBusData);
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

        public abstract byte Read(ushort address);
        public abstract void Write(ushort address, byte data);


        public abstract void Clock();
        public abstract void SetRW(bool rw);
    }
}
using System;
using System.Collections.Generic;

namespace Poly6502.Interfaces
{
    public interface IAddressBusCompatible : IClockable
    {
        Dictionary<int, Action<float>> AddressBusLines { get; set; }

        void RegisterDevice(IAddressBusCompatible device);

        void SetAddress(ushort address);
    }
}
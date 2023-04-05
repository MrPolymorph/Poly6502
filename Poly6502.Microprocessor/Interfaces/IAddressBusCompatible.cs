using System;
using System.Collections.Generic;

namespace Poly6502.Microprocessor.Interfaces
{
    public interface IAddressBusCompatible : IClockable
    {
        ushort MinAddressableRange { get; }
        ushort MaxAddressableRange { get; }
    
        Dictionary<int, Action<float>> AddressBusLines { get; set; }

        void RegisterDevice(IAddressBusCompatible device, int propagationPriority);

        void SetAddress(ushort address);
    }
}
using System;
using System.Collections.Generic;
using Poly6502.Microprocessor.Models;

namespace Poly6502.Microprocessor.Interfaces
{
    public interface IDataBusCompatible : IClockable
    {
        bool PropagationOverridden { get; }
        
        Dictionary<int, Action<float>> DataBusLines { get; set; }
        
        /// <summary>
        /// Set the read / write signal
        ///
        /// 1 = CPU wants to write
        /// 0 = CPU wants to read.
        /// </summary>
        /// <param name="rw"></param>
        void SetRW(bool rw);

        byte Read(ushort address, bool ronly = false);
        void Write(ushort address, byte data);
        public void PropagationOverride(bool ovr, object invoker);
    }
}
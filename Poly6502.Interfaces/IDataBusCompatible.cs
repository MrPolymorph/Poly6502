using System;
using System.Collections.Generic;

namespace Poly6502.Interfaces
{
    public interface IDataBusCompatible : IClockable
    {
        public Dictionary<int, Action<float>> DataBusLines { get; set; }
        

        /// <summary>
        /// Set the read / write signal
        ///
        /// 1 = CPU wants to write
        /// 0 = CPU wants to read.
        /// </summary>
        /// <param name="rw"></param>
        void SetRW(bool rw);

        void PropagationOverride(bool ovr, object invoker);
    }
}
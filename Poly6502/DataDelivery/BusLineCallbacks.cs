using System;
using System.Collections.Generic;

namespace Poly6502.DataDelivery
{
    public class BusLineCallbacks
    {
        public Action<float> this[int i] => Callbacks[i];

        public IList<Action<float>> Callbacks { get; }

        public BusLineCallbacks(params Action<float>[] callbacks)
        {
            Callbacks = new List<Action<float>>();
            
            foreach (var cb in callbacks)
            {
                Callbacks.Add(cb);
            }
            
        }
    }
}
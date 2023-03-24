using System;
using Poly6502.Microprocessor.Flags;

namespace Poly6502.Microprocessor.Models
{
    public class ProcessorStateChangedEventArgs : EventArgs
    {
        public StateChangeType StateType { get; }
        public object PreviousValue { get; }
        public object NewValue { get; }
        
        public ProcessorStateChangedEventArgs(StateChangeType type, object previousVal, object newVal)
        {
            StateType = type;
            PreviousValue = previousVal;
            NewValue = newVal;
        }
    }
}
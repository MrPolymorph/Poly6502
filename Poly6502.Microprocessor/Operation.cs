using System;

namespace Poly6502.Microprocessor
{
    public class Operation
    {
        public Action OpCodeMethod { get; }
        public Action AddressingModeMethod { get; }

        public Operation(Action operation, Action addressingMode)
        {
            OpCodeMethod = operation;
            AddressingModeMethod = addressingMode;
        }
    }
}
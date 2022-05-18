using System;

namespace Poly6502.Microprocessor
{
    public class Operation
    {
        public Action OpCodeMethod { get; }
        public Action AddressingModeMethod { get; }
        public int MachineCycles { get; }

        public Operation(Action operation, Action addressingMode, int instructionBytes, int machineCycles)
        {
            OpCodeMethod = operation;
            AddressingModeMethod = addressingMode;
            MachineCycles = machineCycles;
        }

        public bool OpCodeCompare(Action opCode)
        {
            return OpCodeMethod == opCode;
        }

        public bool AddressingModeCompare(Action addressing)
        {
            return AddressingModeMethod == addressing;
        }

        public bool CompareInstruction(Action opCode, Action addressing)
        {
            return OpCodeMethod == opCode && AddressingModeMethod == addressing;
        }
        
        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Operation op)
            {
                return OpCodeMethod == op.OpCodeMethod &&
                       AddressingModeMethod == op.AddressingModeMethod;
            }

            return false;
        }

        protected bool Equals(Operation other)
        {
            return Equals(OpCodeMethod, other.OpCodeMethod) && Equals(AddressingModeMethod, other.AddressingModeMethod);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OpCodeMethod, AddressingModeMethod);
        }
    }
}
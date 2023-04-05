using System;

namespace Poly6502.Microprocessor.Utilities.Arguments
{
    public class AddressChangedEventArgs : EventArgs
    {
        public ushort OldAddress { get; }
        public ushort NewAddress { get; }
        
        public AddressChangedEventArgs(ushort oldAddress, ushort newAddress)
        {
            OldAddress = oldAddress;
            NewAddress = newAddress;
        }
    }
}
using System;
using System.IO;
using Poly6502.Interfaces;
using Poly6502.Utilities;

namespace Poly6502.Microprocessor.Tests;

public class TestRam : AbstractAddressDataBus
{
    private const int RamSize = 2048;
        
    private byte[] _ram;

    public byte this[int i]
    {
        get { return _ram[i]; }
        set { _ram[i] = value; }
    }
    
    public TestRam(string filePath)
    {
        _ram = File.ReadAllBytes(filePath);
    }
    
    public override void Clock()
    {
    }

    public override void SetRW(bool rw)
    {
        CpuRead = rw;
    }

    public override byte Read(ushort address, bool rOnly = false)
    {
        //check if the address is meant for us?
        if (AddressBusAddress < MaxAddressableRange)
        {
            var actualAddress = address & 0x7FF;
            return _ram[actualAddress];
        }

        return DataBusData;
    }

    public override void Write(ushort address, byte data)
    {
        //check if the address is meant for us?
        if (address < MaxAddressableRange)
        {
            var actualAddress = address & 0x7FF;
            _ram[actualAddress] = data;
        }
    }

    public byte Peek(ushort address)
    {
        if (address < _ram.Length)
            return _ram[address];

        return 0;
    }

    public byte[] Take(ushort start, ushort end)
    {
        if (_ram.Length > end)
            return _ram[start..end];

        throw new IndexOutOfRangeException();
    }
}
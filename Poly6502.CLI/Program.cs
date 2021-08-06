using System;
using System.Diagnostics;
using Poly6502.Microprocessor;
using Poly6502.Utilities;

namespace Poly6502.CLI
{
    public class Program : AbstractAddressDataBus
    {
        private Process _consoleProc;
        
        private int _dataBus;
        private M6502 _mos6502;
        private Cartridge _cartridge;
        private Ram.Ram _ram;
        private Stopwatch _stopwatch;
        private int _executionTimes = 0;

        public Program()
        {
            _dataBus = 0;
            
            _stopwatch = new Stopwatch();
            _ram = new Ram.Ram(0x1FFF);
            _cartridge = new Cartridge();
            _cartridge.LoadProgram();
            _mos6502 = new M6502();
            _mos6502.RegisterDevice(_cartridge);
            _mos6502.RegisterDevice(_ram);
            _ram.RegisterDevice(_cartridge);
        }
        

        private void ClockTheCPU()
        {
            if (_executionTimes < 1_000_000)
            {
                if(!_mos6502.OpCodeInProgress)
                    OutputToConsole();
                
                Console.Read();
                
                //Clock the CPU
                _mos6502.Clock();
                
                //this ordering is important to keep propagation ordering 
                _ram.Clock();
                _cartridge.Clock();
                _executionTimes++;
                
            }
            else if (_executionTimes >= 1_000_000 || _stopwatch.Elapsed.Seconds >= 1)
            {
                Console.WriteLine($"#cyl/sec {_executionTimes}");
                _stopwatch.Restart();
                _executionTimes = 0;
            }
        }

        private void Run()
        {
            _stopwatch.Start();
            _mos6502.RES();
            do
            {
                ClockTheCPU();
            } while (!Console.KeyAvailable);

            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
        }
        

        public override void SetRW(bool rw)
        {
            
        }

        public override void Clock()
        {
            throw new NotImplementedException();
        }

        private void OutputToConsole()
        {
            var op =  _mos6502.OpCodeLookupTable[_mos6502.OpCode];
            
            
            Console.WriteLine($"{_mos6502.OpCode:X2} {_mos6502.InstructionLoByte:X2} {_mos6502.InstructionHiByte:X2} {op.OpCodeMethod.Method.Name} ${(_mos6502.InstructionHiByte << 8 | _mos6502.InstructionLoByte):X4} A:{_mos6502.A:X2} X:{_mos6502.X:X2} Y:{_mos6502.Y:X2} P:{(int)_mos6502.P} SP:{_mos6502.SP:X4}");
        }
        
        public override byte DirectRead(ushort address)
        {
            throw new NotImplementedException();
        }
    }
}
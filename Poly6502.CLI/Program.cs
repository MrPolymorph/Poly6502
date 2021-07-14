using System;
using System.Diagnostics;
using Poly6502.Utilities;

namespace Poly6502.CLI
{
    public class Program : AbstractAddressDataBus
    {
        private int _dataBus;
        private Processor _mos6502;
        private Ram.Ram _ram;
        private Stopwatch _stopwatch;
        private int _executionTimes = 0;

        public Program()
        {
            _dataBus = 0;


            _stopwatch = new Stopwatch();

            _ram = new Ram.Ram(0xFFFF);
            _mos6502 = new Processor();
            _mos6502.RegisterDataCompatibleDevice(_ram);
            _mos6502.RegisterAddressCompatibleDevice(_ram);
            _ram.RegisterDataCompatibleDevice(_mos6502);
        }
        

        private void ClockTheCPU()
        {
            if (_executionTimes < 1_000_000)
            {
                //Clock the CPU
                _mos6502.Clock();
                _ram.Clock();
                Console.WriteLine($"Data: 0x{_dataBus:X2}");
                _executionTimes++;
            }
            else if (_executionTimes >= 1_000_000 || _stopwatch.Elapsed.Seconds >= 1)
            {
                Console.WriteLine(_executionTimes);
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
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Poly6502.CLI;
using Poly6502.Microprocessor;
using Poly6502.Visualiser.Models;
using ReactiveUI;

namespace Poly6502.Visualiser.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly LogLoader _logLoader;

        private Task _clockingThread;
        
        private M6502 _m6502;
        private Cartridge _cartridge;
        private Ram.Ram _ram;
        private Stopwatch _stopwatch;
        private int _executionTimes;

        public ICommand ClockCommand { get; }
        public ICommand LoadLogCommand { get; }
        public ICommand RunCommand { get; set; }
        
        public ObservableCollection<LogLine> LogLines { get; set; }
        public ObservableCollection<OpCodeVerification> OpCodePassFail { get; set; }

        public ushort AddressBusAddress => _m6502.AddressBusAddress;
        public byte DataBusData => _m6502.DataBusData;
        public string OpCode => _m6502.OpCodeLookupTable[_m6502.OpCode].OpCodeMethod.Method.Name;
        
        public float Pin1 => _m6502.InputVoltage;
        public string Pin2 => "";
        public string Pin3 => "";
        public string Pin4 => "";
        public string Pin5 => "";
        public string Pin6 => "";
        public string Pin7 => "";
        public string Pin8 => "";
        public byte Pin9 => (byte) ((_m6502.AddressBusAddress & (1 << 0)) != 0 ? 1 : 0);
        public byte Pin10 => (byte) ((_m6502.AddressBusAddress & (1 << 1)) != 0 ? 1 : 0);
        public byte Pin11 => (byte) ((_m6502.AddressBusAddress & (1 << 2)) != 0 ? 1 : 0);
        public byte Pin12 => (byte) ((_m6502.AddressBusAddress & (1 << 3)) != 0 ? 1 : 0);
        public byte Pin13 => (byte) ((_m6502.AddressBusAddress & (1 << 4)) != 0 ? 1 : 0);
        public byte Pin14 => (byte) ((_m6502.AddressBusAddress & (1 << 5)) != 0 ? 1 : 0);
        public byte Pin15 => (byte) ((_m6502.AddressBusAddress & (1 << 6)) != 0 ? 1 : 0);
        public byte Pin16 => (byte) ((_m6502.AddressBusAddress & (1 << 7)) != 0 ? 1 : 0);
        public byte Pin17 => (byte) ((_m6502.AddressBusAddress & (1 << 8)) != 0 ? 1 : 0);
        public byte Pin18 => (byte) ((_m6502.AddressBusAddress & (1 << 9)) != 0 ? 1 : 0);
        public byte Pin19 => (byte) ((_m6502.AddressBusAddress & (1 << 10)) != 0 ? 1 : 0);
        public byte Pin20 => (byte) ((_m6502.AddressBusAddress & (1 << 11)) != 0 ? 1 : 0);
        public string Pin21 => "";
        public byte Pin22 => (byte) ((_m6502.AddressBusAddress & (1 << 12)) != 0 ? 1 : 0);
        public byte Pin23 => (byte) ((_m6502.AddressBusAddress & (1 << 13)) != 0 ? 1 : 0);
        public byte Pin24 => (byte) ((_m6502.AddressBusAddress & (1 << 14)) != 0 ? 1 : 0);
        public byte Pin25 => (byte) ((_m6502.AddressBusAddress & (1 << 15)) != 0 ? 1 : 0);
        public byte Pin26 => (byte) ((_m6502.DataBusData & (1 << 7)) != 0 ? 1 : 0);
        public byte Pin27 => (byte) ((_m6502.DataBusData & (1 << 6)) != 0 ? 1 : 0);
        public byte Pin28 => (byte) ((_m6502.DataBusData & (1 << 5)) != 0 ? 1 : 0);
        public byte Pin29 => (byte) ((_m6502.DataBusData & (1 << 4)) != 0 ? 1 : 0);
        public byte Pin30 => (byte) ((_m6502.DataBusData & (1 << 3)) != 0 ? 1 : 0);
        public byte Pin31 => (byte) ((_m6502.DataBusData & (1 << 2)) != 0 ? 1 : 0);
        public byte Pin32 => (byte) ((_m6502.DataBusData & (1 << 1)) != 0 ? 1 : 0);
        public byte Pin33 => (byte) ((_m6502.DataBusData & (1 << 0)) != 0 ? 1 : 0);
        public byte Pin34 => (byte) (_m6502.CpuRead == true ? 1 : 0);
        public string Pin35 => "";
        public string Pin36 => "";
        public string Pin37 => "";
        public string Pin38 => "";
        public string Pin39 => "";
        public string Pin40 => "";
        

        public MainWindowViewModel()
        {
            _stopwatch = new Stopwatch();
            _ram = new Ram.Ram(0x1FFF);
            _cartridge = new Cartridge();
            _cartridge.LoadProgram();
            _m6502 = new M6502();
            _m6502.RegisterDevice(_cartridge);
            _m6502.RegisterDevice(_ram);
            _ram.RegisterDevice(_cartridge);
            
            ClockCommand = ReactiveCommand.Create(ClockCPU);
            LoadLogCommand = ReactiveCommand.Create(LoadLog);
            RunCommand = ReactiveCommand.Create(Run);
            
            LogLines = new ObservableCollection<LogLine>();
            OpCodePassFail = new ObservableCollection<OpCodeVerification>();
            
            _logLoader = new LogLoader();
        }

        private int _currentLine = 0;
        public void ClockCPU()
        {
            if (_executionTimes < 1_000_000)
            {
                //Clock the CPU
                _m6502.Clock();
                
                //this ordering is important to keep propagation ordering 
                _ram.Clock();
                _cartridge.Clock();
                _executionTimes++;

                if (!_m6502.OpCodeInProgress)
                {
                    Verify();
                    _currentLine++;
                }

            }
            else if (_executionTimes >= 1_000_000 || _stopwatch.Elapsed.Seconds >= 1)
            {
                _stopwatch.Restart();
                _executionTimes = 0;
            }
            
            UpdateBindings();
        }

        private void Verify()
        {
            var item = LogLines[_currentLine];

            var currentOp = new OpCodeVerification()
            {
                Expected = item,
                Actual = new LogLine()
                {
                    OpCode = _m6502.OpCode,
                    Data1 = _m6502.InstructionLoByte,
                    Data2 = _m6502.InstructionHiByte
                }
            };

            currentOp.Pass = item.Compare(_m6502.OpCode, _m6502.InstructionLoByte, _m6502.InstructionHiByte);
            
            OpCodePassFail.Add(currentOp);
        }
        
        private void UpdateBindings()
        {
            this.RaisePropertyChanged(nameof(Pin1));
            this.RaisePropertyChanged(nameof(Pin2));
            this.RaisePropertyChanged(nameof(Pin3));
            this.RaisePropertyChanged(nameof(Pin4));
            this.RaisePropertyChanged(nameof(Pin5));
            this.RaisePropertyChanged(nameof(Pin6));
            this.RaisePropertyChanged(nameof(Pin7));
            this.RaisePropertyChanged(nameof(Pin8));
            this.RaisePropertyChanged(nameof(Pin9));
            this.RaisePropertyChanged(nameof(Pin10));
            this.RaisePropertyChanged(nameof(Pin11));
            this.RaisePropertyChanged(nameof(Pin12));
            this.RaisePropertyChanged(nameof(Pin13));
            this.RaisePropertyChanged(nameof(Pin14));
            this.RaisePropertyChanged(nameof(Pin15));
            this.RaisePropertyChanged(nameof(Pin16));
            this.RaisePropertyChanged(nameof(Pin17));
            this.RaisePropertyChanged(nameof(Pin18));
            this.RaisePropertyChanged(nameof(Pin19));
            this.RaisePropertyChanged(nameof(Pin20));
            
            this.RaisePropertyChanged(nameof(Pin21));
            this.RaisePropertyChanged(nameof(Pin22));
            this.RaisePropertyChanged(nameof(Pin23));
            this.RaisePropertyChanged(nameof(Pin24));
            this.RaisePropertyChanged(nameof(Pin25));
            this.RaisePropertyChanged(nameof(Pin26));
            this.RaisePropertyChanged(nameof(Pin27));
            this.RaisePropertyChanged(nameof(Pin28));
            this.RaisePropertyChanged(nameof(Pin29));
            this.RaisePropertyChanged(nameof(Pin30));
            this.RaisePropertyChanged(nameof(Pin31));
            this.RaisePropertyChanged(nameof(Pin32));
            this.RaisePropertyChanged(nameof(Pin33));
            this.RaisePropertyChanged(nameof(Pin34));
            this.RaisePropertyChanged(nameof(Pin35));
            this.RaisePropertyChanged(nameof(Pin36));
            this.RaisePropertyChanged(nameof(Pin37));
            this.RaisePropertyChanged(nameof(Pin38));
            this.RaisePropertyChanged(nameof(Pin39));
            this.RaisePropertyChanged(nameof(Pin40));
            
            this.RaisePropertyChanged(nameof(AddressBusAddress));
            this.RaisePropertyChanged(nameof(DataBusData));
            this.RaisePropertyChanged(nameof(OpCode));
        }

        private void Run()
        {
            _clockingThread = new Task(Clock);
            
            _clockingThread.Start();
        }

        private void Clock()
        {
            _stopwatch.Start();
            _m6502.RES();
            do
            {
                ClockCPU();
            } while (!Console.KeyAvailable);
        }
        
        private async Task LoadLog()
        {
            var logItems = await _logLoader.LoadLog("/home/kris/Documents/nestest.log");
            LogLines = new ObservableCollection<LogLine>(logItems);
            this.RaisePropertyChanged(nameof(LogLines));
        }
    }
}
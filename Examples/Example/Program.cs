using Poly6502.Microprocessor;
using Poly6502.Microprocessor.Utilities;

namespace Example
{
    public class Example
    {
        public static void Main(string[] args)
        {
            var cpu = new M6502();
            var ram = new Ram();
            var extendedRam = new ExtendedRam();

            cpu.RES(0);
            
            Console.WriteLine("Registering RAM");
            cpu.RegisterDevice(ram);
            Console.WriteLine("Registering Extended RAM");
            cpu.RegisterDevice(extendedRam);
            
            //register the ram devices with each other
            //so that propagation works
            ram.RegisterDevice(extendedRam);

            Console.WriteLine("Clocking CPU");
            //clocking will perform a fetch and execute the entire opcode.
            cpu.Clock(); //LDA
            cpu.Clock(); //LDX
            
            Console.WriteLine($"A register is 0x{cpu.A:X2} X register is 0x{cpu.X:X2}");
        }

        public class Ram : AbstractAddressDataBus
        {
            /// <summary>
            /// The RAM is 2KB in size
            /// </summary>
            private const int RamSize = 2048;

            private byte[] _ram;

            public byte this[int i]
            {
                get { return _ram[i]; }
                set { _ram[i] = value; }
            }

            public Ram()
            {
                MinAddressableRange = 0x000;
                MaxAddressableRange = 0x800;

                _ram = new byte[RamSize];
                
                for (int i = 0; i < RamSize; i++)
                {
                    _ram[i] = 0;
                }
                
                _ram[0] = 0xAD; //LDA ABS
                _ram[1] = 0x03; //Instruction Lo
                _ram[2] = 0x00; //Instruction Hi;
                _ram[3] = 0xAE; //Load A with 0xAE
                _ram[4] = 0x00; //Instruction Lo Byte
                _ram[5] = 0x08; //Instruction Hi Byte

                Console.WriteLine("2Kb Extended RAM initialised");
            }

            public override void Clock()
            {
                //If you want to emulate clocking ram
                //you can do that here
            }

            public override void SetRW(bool rw)
            {
                //A flag set by the CPU to say its either
                //either reading/writing from/to the 'Data' bus.
                CpuRead = rw;
            }

            public override byte Read(ushort address, bool rOnly = false)
            {
                SetPropagation(false);
                Console.WriteLine($"RAM received read request at address 0x{address:X4}");
                
                //check if the address is meant for us?
                if (address < MaxAddressableRange)
                {
                    SetPropagation(true);
                    var data = _ram[address];
                    Console.WriteLine($"Request valid, returning data: 0x{data:X2}");
                    return data;
                }

                //if the address is not for us, return 0
                //but Propagation is not set, so the CPU will not use it.
                Console.WriteLine($"Request invalid");
                return 0;
            }

            public override void Write(ushort address, byte data)
            {
                Console.WriteLine($"RAM received write request at address 0x{address:X4} with data 0x{data}");
                
                //check if the address is meant for us?
                if (address <= MaxAddressableRange)
                {
                    Console.WriteLine($"Request valid, writing data: 0x{data:X2} to 0x{address:X4}");
                    _ram[address] = data;
                }
            }
        }

        public class ExtendedRam : AbstractAddressDataBus
        {
            /// <summary>
            /// The RAM is 2KB in size
            /// </summary>
            private const int RamSize = 2048;

            private byte[] _ram;

            public byte this[int i]
            {
                get { return _ram[i]; }
                set { _ram[i] = value; }
            }

            public ExtendedRam()
            {
                MinAddressableRange = 0x800;
                MaxAddressableRange = 0x1000;

                _ram = new byte[RamSize];
                
                for (int i = 0; i < RamSize; i++)
                {
                    _ram[i] = 0;
                }
                
                _ram[0] = 0xFF; //LDX operand
                Console.WriteLine("2Kb RAM initialised");
            }

            public override void Clock()
            {
                //If you want to emulate clocking ram
                //you can do that here
            }

            public override void SetRW(bool rw)
            {
                //A flag set by the CPU to say its either
                //either reading/writing from/to the 'Data' bus.
                CpuRead = rw;
            }

            public override byte Read(ushort address, bool rOnly = false)
            {
                Console.WriteLine($"Extended RAM received read request at address 0x{address:X4}");
                
                //check if the address is meant for us?
                if (address >= MinAddressableRange && address < MaxAddressableRange)
                {
                    //Map to address within the 2kb range.
                    var actualAddress = address & 0x7FF;
                    SetPropagation(true);
                    var data = _ram[actualAddress];
                    Console.WriteLine($"Request valid, returning data: 0x{data:X2}");
                    return data;
                }

                //if the address is not for us, return 0
                Console.WriteLine($"Request invalid");
                return 0;
            }

            public override void Write(ushort address, byte data)
            {
                Console.WriteLine($"Extended RAM received write request at address 0x{address:X4} with data 0x{data:X2}");
                
                //check if the address is meant for us?
                if (address >= MinAddressableRange && address <= MaxAddressableRange)
                {
                    var actualAddress = address & 0x7FF;
                    Console.WriteLine($"Request valid, writing data: 0x{data:X2} to 0x{actualAddress:X4}");
                    _ram[actualAddress] = data;
                }
            }
        }
    }
}
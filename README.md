# Poly6502

![example workflow](https://github.com/MrPolymorph/Poly6502/actions/workflows/build.yml/badge.svg)
![example workflow](https://github.com/MrPolymorph/Poly6502/actions/workflows/codeql.yml/badge.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=MrPolymorph_Poly6502&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=MrPolymorph_Poly6502)
[![NuGet version (Poly6502)](https://img.shields.io/badge/nuget-v1.0.1-blue)](https://www.nuget.org/packages/Poly6502.Microprocessor/)

This is a WIP emulation of the MOS Technologies 6502 microprocessor.

The ultimate goal is to have an accurate emulation with a full suite of comprehensive 
passing tests.

## Building

---
The emulator is written in .Net 6 and can be built on most operating
systems compatible with .net 6.

Download & install the .NET sdk for your system https://dotnet.microsoft.com/en-us/download/dotnet/6.0

Create your own fork of the repository at  https://github.com/MrPolymorph/Poly6502.git.
Then clone your fork where you wish to have the project, with the command:

```bash
git clone https://github.com/<YOUR_USERNAME>/poly6502.git
```

This will copy the GitHub repository contents into a new folder in the current directory called `poly6502`. Change into this directory before doing anything else:

```bash
cd poly6502
```

You then need to build the project using the following command

```bash
dotnet build
```

This will start building the Poly6502.slm file.

Your output should be similar to 

```bash
Microsoft (R) Build Engine version 17.0.0+c9eb9dd64 for .NET
Copyright (C) Microsoft Corporation. All rights reserved.

  Determining projects to restore...
  All projects are up-to-date for restore.
  Poly6502.Interfaces -> /Users/Kris/Documents/GitHub/Poly6502/Poly6502.Interfaces/bin/Debug/net6.0/Poly6502.Interfaces.dll
  Poly6502.Utilities -> /Users/Kris/Documents/GitHub/Poly6502/Poly6502.Utilities/bin/Debug/net6.0/Poly6502.Utilities.dll
  Poly6502.Microprocessor -> /Users/Kris/Documents/GitHub/Poly6502/Poly6502.Microprocessor/bin/Debug/net6.0/Poly6502.Microprocessor.dll
  Poly6502.Microprocessor.Tests -> /Users/Kris/Documents/GitHub/Poly6502/Poly6502.Microprocessor.Tests/bin/Debug/net6.0/Poly6502.Microprocessor.Tests.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.00
```

Once the build is complete, you can verify unit tests by running.

```bash
dotnet test
```

## How to use

You can either build the project using the instructions in the build step, or you can reference the 
latest nuget package in your project.

create a new processor object with

```C#
var cpu = new M6502()
```

you can call RES to reset the CPU and pass in a value for the program counter.
If you don't pass any value in, it will use the reset vector 0xFFFC 0xFFFD to pull the first opcode

```C#
var cpu = new M6502()
cpu.RES(); //Reset using reset vector

cpu.RES(0xC000); //set the program counter to 0xC000
```

## Contributing

---

All contributions are welcome. This is a group effort, and even small contributions can make a difference. 
Some tasks also don't require much knowledge to get started.
.

For more information on getting started, see our Contributing Guide and our Code Review Guidelines to see what code quality guidelines we follow.

## Resources

---
There are lots of great resources out there, here are just a few to help you get started.

| Link                                                            | Description              |
|-----------------------------------------------------------------|--------------------------|
 | https://www.masswerk.at/6502/6502_instruction_set.html          | 6502 Instruction Set     |
| https://www.princeton.edu/~mae412/HANDOUTS/Datasheets/6502.pdf  | Synertek 6502 Data Sheet |
| http://archive.6502.org/datasheets/rockwell_r650x_r651x.pdf     | Rockwell 6502 Data Sheet |
| https://www.nesdev.org/wiki/Cycle_counting                      | Cycle Counting           |

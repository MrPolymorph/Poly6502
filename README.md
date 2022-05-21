# Poly6502
This is a WIP emulation of the MOS Technologies 6502 microprocessor.

The ultimate goal is to have an accurate emulation with a full suite of comprehensive 
passing tests.

---
##Building

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

##Contributing

---

All contributions are welcome. This is a group effort, and even small contributions can make a difference. 
Some tasks also don't require much knowledge to get started.
.

For more information on getting started, see our Contributing Guide and our Code Review Guidelines to see what code quality guidelines we follow.
# Build instructions for P4API.NET 

These are Build instructions for P4API.net and its required DLL and various test utilities

## The .NET Core (6.0) cross-platform port

With the 2021.2 release, `P4API.NET` has been extended to support .NET CORE and to be cross-platform on three Target OS's (Windows, Linux, OSX)

P4API.NET consists of a DLL written in C++ which contains the Perforce C++ API, which is wrapped by a layer of C# code which exports the .NET interface.

Within this directory are several subprojects.  p4bridge, p4bridge-unit-test, p4bridgeStressTest, p4api.net and p4api.net-unit-test

## Getting to know your solutions

### p4apicore.net.sln

[p4apicore.net.sln](p4apicore.net.sln) - This solution includes projects [p4api.net.csproj](./p4api.net/p4api.net.csproj)  and [p4api.net-unit-test.csproj](./p4api.net-unit-test/p4api.net-unit-test.csproj). 
Both of the projects target frameworks .NET Framework 4.6.2, .Net Core 6.0 and .NET Standard 2.0.
> It only includes the C# portions of the project.
> It can be built using the `dotnet build p4apicore.net.sln` command.
This solution can be opened by the "dotnet" executable on any supported platform, or by VS2019+ on windows only.
This is the primary solution to be used for building and/or debugging P4API.NET and its unit tests.

### Sample Applications

Sample Console Applications for .NET 6.0 are provided here: 
[consoleApps/ControlCTest.sln](consoleApps/ControlCTest.sln) and [consoleApps/P4DotNetConsole.sln](consoleApps/P4DotNetConsole.sln)

Sample Applications for .NET framework are provided here:
[examples/examples.sln](examples/examples.sln)

### What about VS Code?

The workspace file [p4apinet.code-workspace](./p4apinet.code-workspace) opens the P4API.NET C# source tree for the assembly and its Unit Tests.  
This workspace has not been customized for C# and cannot be used to build the distribution on any platform.

The workspace file [p4bridge.code-workspace](./p4bridge.code-workspace) shows the P4BRIDGE C++ source tree and the source for the p4bridge-unit-test. 
This workspace has not been customized for C++ and CMake and cannot be used to build the distribution on any platform.

Enhancing these workspaces to actually do builds is an exercise for the reader ;)

## Setting up the C++ build environment

C++ is used for the *p4bridge*, *p4bridge-unit-test* and *p4bridgeStressTest* projects, which are all in subdirectories of the p4api.net top-level directory.   All C++ builds use Cmake.

### Updating cmake

The *p4bridge*, *p4bridge-unit-test* and *p4bridgeStressTest* projects require cmake 3.20 or better, (for CMake Profile support).

The default installation of cmake on ubuntu will require updating.
to update to the latest version on ubuntu, follow package updating instructions here: [CMake APT repository](https://apt.kitware.com)

### Getting the P4API and openssl 3 libraries

We've included some sample setup scripts for copying the P4API and openssl libraries into their expected source subdirectories.  They are found in the root p4api.net directory.
look for [setuplibrary.bat](./setuplibrary.bat), [setuplibrary_linux.sh](./setuplibrary_linux.sh) and [setuplibrary_osx.sh](./setuplibrary_osx.sh)

These scripts are just examples, they represent what I had to do to get my environment to work correctly, they will need to be edited to work in your environment.

The library directories must be in place before building the *p4bridge* project.

## Getting to know your subprojects

### Building the **p4bridge DLL**

The p4bridge DLL is required by `P4API.NET` and is a C++ project built by cmake.

The same p4bridge DLL will work in both .NET Core and .NET Framework Assemblies.

The source and build instructions for the bridge DLL is in the *p4bridge* subdirectory.

 [P4Bridge.md](p4bridge/P4Bridge.md)

### Building and running the **Bridge-Unit-Test**

The bridge-unit-test is a C++ project which provides a test suite for the p4bridge.
The source, build and configuration instructions for these tests is in the *p4bridge-unit-test* subdirectory.

 [BridgeUnitTest.md](p4bridge-unit-test/BridgeUnitTest.md)

### Building and running the **p4bridgeStressTest**

 The p4bridgeStressTest is a C++ project which provides a performance test for the p4bridge. The source and instructions for this test is in the p4bridgeStressTest subdirectory.

 [p4bridgeStressTest.md](p4bridgeStressTest/p4bridgeStressTest.md)

### Building `P4API.NET`

**IMPORTANT!** - The C++ bridge must have already been built before building P4API.NET.

Make sure you have an updated .NET 6.0 SDK (or better) installed for your platform.
download from here: [Microsoft .NET Downloads](https://dotnet.microsoft.com/download)

in the p4api.net root folder run
`dotnet build p4apicore.net.sln`

 This will create The .NET DLL in `p4api.net\bin\Debug\net60\p4api.net.dll`, `p4api.net\bin\Debug\net462\p4api.net.dll`, `p4api.net\bin\Debug\netstandard2.0\p4api.net.dll`
 And the Unit test in `\p4api.net-unit-test\bin\Debug\net60\p4api.net-unit-test.dll`, `\p4api.net-unit-test\bin\Debug\net462\p4api.net-unit-test.dll`, `\p4api.net-unit-test\bin\Debug\netstandard2.0\p4api.net-unit-test.dll`
 It will also copy the p4bridge.dll from the appropriate place in the ../p4bridge output into the default output directory.

 On Windows, you can specify a specific Configuration and Platform with parameters:
 `dotnet build p4apicore.net.sln -c Release -p:Platform=x86`

 On Windows, if no Platform is specified the default is 'x64'.
 On Windows, If no Configuration is specified the default is 'Debug'.

As part of the P4API.NET project build, the (previously build) p4bridge DLLs are copied 
into proper runtime locations. 

### Running `P4API.NET Unit Test`

* Using the **dotnet** command,  in the *p4api.net* root folder run:
`dotnet test p4apicore.net.sln`

> Filter on the dotnet framework to run upon
>> `dotnet test p4apicore.net.sln -f net60`
>
> Filter on the specific test to run
>> `dotnet test p4apicore.net.sln -f net60 --filter TestName`


* **Visual Studio 2019** can be used to debug the .NET unit tests if you are running in Windows.
Open p4apicore.net.sln, then use the Test Explorer Window to run and manage the tests.
One unique feature of the VS2019 debugger is that it makes it possible to debug both .NET  C# and the P4BRIDGE C++ in the same session.

* **JetBrains Rider** can also be used to debug the .NET unit tests on all platforms,  just open p4apicore.net.sln and the Unit Tests will show up in the Unit Test Explorer window.  
Unfortunately, you cannot debug through the C++ code using Rider.

### Creating a nuGet package

1. This only works on WINDOWS with DOTNET CORE so far.
2. Build the four varieties of p4bridge first
3. `dotnet pack p4apicore.net.sln`

This will create an Assembly DLL in *.\bin\p4api.net\bin\Debug\net6.0\p4api.net.dll*
and a nuget package in:
*p4api.net\bin\Debug\p4api.net.VERSION.nupkg*

### Deploy the nuget package for testing

Deploy this package to a local test repository with: 
`nuget add c:\dev\p4dotnetdev\p4api.net\p4api.net\bin\Debug\p4api.net.2021.2.xxx.yyyy.nupkg -Source C:\dev\local-nuget`

### List packages in the local test repository

`nuget list -Source c:\dev\local-nuget -PreRelease p4api.net`
returns: *2022.2.xxx.yyyy*

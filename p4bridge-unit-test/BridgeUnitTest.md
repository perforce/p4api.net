# how to use the bridge unit tests

Last Update 01/03/2022  by NRM

## How they work

P4API.NET depends on a platform specific DLL to be available for each supported platform,
The source code for this DLL is C++ in the *../bridge* directory.
To test this DLL we provide a C++ test scaffolding in the *bridge-unit-test* directory.

With the 2021.1 release of P4API.NET we added support for Linux and OSX builds to make us compatible with .NET CORE 5.0

## Setting up your C++ dependencies

C++ builds require the correct libraries and include files for P4API and SSL.
See "Instructions for building P4BRIDGE" in [P4Bridge.md](../p4bridge/P4Bridge.md) for more details

## How to customize the bridge unit test for your environment and platform

You must modify *UnitTestConfig.h* to set
a number of **#defines** which need to be set correctly
for your filesystem, platform and hardware.

Three platform #defines are in use:

\#define      | Platform
--------------| --------
**OS_NT**     | Windows
**OS_LINUX**  | Linux
**OS_MACOSX** | OSX

Make sure you change the following definitions for the platform you are building on.

\#define      | Description
------------- | --------------
**P4D**       | The path to the p4d executable, it **must** be the 2020.2 release or later
**TAR**       | The path to the tar executable
**MYTESTDIR** | A path to your bridge unit test directory, this directory will be created and destroyed many times by the unit test. It must point to a free location on your machine.
**MYTESTDIR8** | A path to your bridge unit test directory (for utf8 tests), this directory will be created and destroyed many times by the unit test. It must point to a free location on your machine.

## The unit test build environment

The official build environment is CMake on each platform. We require CMake 3.20 or above.

 Download cmake for your platform from here: [Cmake Downloads](https://cmake.org/download/)

 Cmake presets are stored in the file [CMakePresets.json](CMakePresets.json)

 Available presets are: **x64-Release**, **x64-Debug**, **x86-Release**,  **x86-Debug**, **osx-Release**, **osx-Debug**, **linux-Release**,  **linux-Debug**

The cmake build file is [CMakeLists.txt](CmakeLists.txt)

### Updating resources on windows

The p4bridge-unit-test depends on the resource file [p4bridge-unit-test.rc](p4bridge-unit-test.rc) which, if changed
must be built using "rc",  `rc /r p4bridge-unit-test.rc`  which will create the file `p4bridge-unit-test.res` required by the windows build.
TODO:  This step should be done automatically by cmake.


> In case rc is not found in a command prompt, then environment path variable might need an additional entry to the path of rc
>
> And in case of following error `fatal error RC1015: cannot open include file 'afxres.h'`, while running command `rc /r p4bridge-unit-test.rc`
>
> Provide required paths for the missing header files with a flag `/i`, example below 
> 
> `rc /r /i "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Tools\MSVC\14.29.30133\atlmfc\include;C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um;C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\shared" p4bridge-unit-test.rc`


### Building the p4bridge Unit Test

The p4bridge-unit-test build requires the p4bridge code, so both will be built at the same time.

 1. Generate the build environment `cmake --preset=x64-Release` this will create an MSVC build toolchain for an x64 Windows release in `out/x64/Release`.
 2. Build the environment using `cmake --build out/x64/Release --config Release`
 on Windows it is important to pass a "--config" option ( **Release** or **Debug** ) which matches the preset build type.
 3. The Executable and associated files will end up in `out/x64/Release/bin`

### Running the bridge unit test

The Bridge unit test is an executable which needs to be run.

For Windows x64 debug release, BridgeTest.exe is built in `.\out\x64\debug\bin`
The executable needs to know the path to the p4bridge-unit-test folder, so it is passed using the '-s' option.

`cd out\x64\debug\bin`
`BridgeUnit -s <path_to>/p4api.net/p4bridge-unit-test`

which will run all tests
A successful run of all tests will end with:
> Tests Passed 42, TestFailed: 0 \
> Hit enter to exit

or to speed things up, just a specific test can be specified as an argument
`BridgeUnit -s <path_to>/p4api.net/p4bridge-unit-test <Test_To_Run>`

## debugging notes for Windows Visual Studio 2019

When cmake creates the build environment, a solution file is also created which can be open and debugged using Visual Studio.

*out\x64\debug\BridgeUnit.sln*  is the solution I usually use for debugging.

## debugging notes for gdb

These were helpful to me once ...

> directory  /opt/perforce/clients/perforce/depot/main/p4
> set substitute-path ./p4 /opt/perforce/clients/perforce/depot/main/p4
> show substitute-path
>
> Save configurations in ~/.gdbinit

## debugging notes for lldb

> https://www.mono-project.com/docs/debug+profile/debug/lldb-source-map/
>
> // mac
> settings set -- target.source-map /private/var/tmp/124633488/depot/main/p4 /Users/nmorse/Perforce/nmorse_ws_new/depot/main/p4
>
> // windows clion 32
> settings set -- target.source-map c:\tmp\124769422\depot\main\p4 c:\dev\internal\depot\main\p4

## sample build scripts

These scripts were used to simplify command line builds, and may prove useful to you after some appropriate editing.  **buildall.bat**, **buildall_linux.sh**, **buildall_osx.sh**, **builddebug_osx.sh** and **builddebug_linux.sh**

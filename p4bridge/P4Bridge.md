# Instructions for building the P4API.NET BRIDGE

This DLL depends upon a recent copy of the P4API, and upon openssl 1.1.1

This DLL is used by the P4API.NET assembly, and works in both .NET framework and .NET core applications. 

Since this DLL is now cross-platform, CMake is the "official" build tool of the bridge.

## Get the prerequisites for your platform

**WINDOWS**:  Visual studio 2019 must be installed with the desktop c++ SDK.

**LINUX**:    gcc 8.1.0 or better

**OSX**:      gcc 8.1.0 or better

## Get the right P4API, and then copy it into the correct subdirectory of `../p4api.net`

There are several P4API setup scripts in the p4api.net root directory which can help with setting up
 the required libraries.

The p4bridge is cross platform, and (in windows) supports multiple builds from the same directory.
  The p4api location is dependent on platform and build type

Location | Type of P4API      | Download Location
---------| -------------------|-----------------
../p4api_release | x64 windows static release | [P4API x64 static](http://ftp.perforce.com/perforce/r21.1/bin.ntx64/p4api_vs2017_static_openssl1.1.1.zip)
../p4api_debug | x64 windows static debug | [P4API x64 static debug](http://ftp.perforce.com/perforce/r21.1/bin.ntx64/p4api_vs2017_static_vsdebug_openssl1.1.1.zip)
../p4api_x86_release | x86 windows static release |[P4API x86 static](http://ftp.perforce.com/perforce/r21.1/bin.ntx86/p4api_vs2017_static_openssl1.1.1.zip)  
../p4api_x86_debug | x86 windows static debug |[P4API x86 static debug](http://ftp.perforce.com/perforce/r21.1/bin.ntx86/p4api_vs2017_static_vsdebug_openssl1.1.1.zip)
../p4api | linux (all) | [P4API for linux](http://ftp.perforce.com/perforce/r21.1/bin.linux26x86_64/p4api-glibc2.3-openssl1.1.1.tgz)
../p4api | osx (all) | [P4API for osx](http://ftp.perforce.com/perforce/r21.1/bin.macosx105x86_64/p4api-openssl1.1.1.tgz)

## Set up openssl libraries

The default openssl version is 1.1.1
Download the correct openSSL library for your OS and release type, 
then copy the \*.lib files into the "lib" subdirectory of your p4api* location.
  
OpenSSL binaries can be downloaded from here: [OpenSSL Downloads](https://wiki.openssl.org/index.php/Binaries)

## Install CMAKE, version 3.20 or above

 Download cmake for your platform from here: [Cmake Downloads](https://cmake.org/download/)

## About Presets and Build Directories

 Cmake presets are stored in [CMakePresets.json](CMakePresets.json)
 This file may need some editing to configure build specific options (like compiler location)

Every preset uses a unique build directory under ./out as seen in the table below

  Preset      | Platform | Architecture | Build Type | OUT_DIRECTORY
  ------------|----------|--------------|------------|----------------
  x64-Release | Windows | x64 | Release | ./out/x64/Release
  x64-Debug   | Windows | x64 | Debug | ./out/x64/Debug
  x86-Release | Windows | x86 | Release | ./out/x86/Release
  x86-Debug   | Windows | x86 | Debug | ./out/x86/Debug
  osx-Release | OSX | Intel 64 bit | Release | ./out/osx-Release
  osx-Debug   | OSX | Intel 64 bit | Debug   | ./out/osx-Debug
  linux-Release | Linux | Intel 64 bit | Release | ./out/linux-Release
  linux-Debug | Linux | Intel 64 bit | Debug | ./out/linux-Debug

## Building the Bridge DLL

 1. Generate the build environment `cmake --preset=PRESET`. this will create an Cmake build toolchain for the specified preset and initialize its OUT_DIRECTORY.
 2. Build the code within the build directory using `cmake --build OUT_DIRECTORY --config Release`   (The "--config Release" option is only needed for windows builds)
 on Windows it is important to pass a "--config" option ( **Release** or **Debug** ) which matches the preset build type.
 3. The DLL and associated files will be created in the OUT_DIRECTORY

 You can clean a build environment with `cmake --build OUT_DIRECTORY --target clean`

## Sample build scripts

 I've provided some build scripts which I used to simplify building from the command line.  These are samples only and will need to be modified for your environment.  
 These include **buildall.bat**, **builddebug.bat**, **builddebug_linux.sh**, **builddebug_osx.sh**, **buildall_linux.sh**
 and **buildall_osx.sh**




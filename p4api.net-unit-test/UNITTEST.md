# This is the .NET unit test for P4API.NET

It uses the MSTEST unit test framework.

These tests can be run in **Visual Studio 2019** in Windows. Find them under the "Unit Test" window
after you open either solution
[p4apicore.net.sln](../p4apicore.net.sln)  (for .NET 6.0) or
[p4api.net.sln](../p4api.net.sln) (for .NET Framework)

The Dotnet Core tests can also be run Cross-Platform in **Jetbrains Rider** use the Unit Test window after you the open solution
[p4apicore.net.sln](../p4apicore.net.sln)

The configuration file for unit tests is [appsettings.json](appsettings.json) in this directory.
Make sure the settings for your OS are correct before running the tests.

Variable to set   |  Platform | Purpose
----------------- | --------- | ---------
**WindowsTestDirectory** | Windows | Test directory
**WindowsP4dPath** | Windows | Path to the p4d.exe executable
**WindowsTarPath** | Windows | Path to the tar.exe executable
**LinuxTestDirectory** | Linux | Test directory
**LinuxP4dPath** | Linux | Path to the p4d executable
**LinuxTarPath** | Linux | Path to the tar executable
**OsxTestDirectory** | OSX | Test directory
**OsxP4dPath** | OSX | Path to the p4d executable
**OsxTarPath** | OSX | Path to the tar executable
**ServerPort** | ALL Platforms | P4d Server host and port



# ProtonVPN for Windows

Building guide of the ProtonVPN for Windows. ProtonVPN for Windows is .NET Framework
application created using C# and C++ programming languages.

## Prerequisites

To build ProtonVPN the following tools have to be installed:

- Visual Studio Community 2019
- Windows Driver Kit (WDK) for Windows 10, version 1903
- WixToolset 3.11.1
- Python for Windows 3.8
- Advanced Installer 16.6.1
- Go 1.13.5
- [TDM-GCC MinGW Compiler](https://sourceforge.net/projects/tdm-gcc/)

To enable the sending of crash reports to Sentry server the environment variable
SENTRY_DSN should contain valid Sentry DSN value during build. If environment variable
SENTRY_DSN is not set, sending of crash reports will be disabled.

### Visual Studio 2019

When installing Visual Studio 2019 16.4.2 select the following configuration:

- Workloads:
  - .NET desktop development
  - Desktop development with C++
- Individual components:
  - C++ ATL for latest v142 build tools with Spectre Mitigations (x86 & x64)
  - MSVC v142-VS 2019 C++ x64/x86 Spectre-mitigated libs (v14.24)

The required individual components might vary depending on the Visual Studio version.
The above selection is for Visual Studio Community 2019 16.4.2.

### Python 3.8

When installing Python for Windows check the "Add Python 3.8 to PATH".

When Python is installed install additional Python modules by running this command:  
`py -m pip install requests pywin32`

## Building

The application can be built using the default Visual Studio build
environment. To build the setup files use the following steps:

- Select the "Release" solution configuration in Visual Studio.
- Build the "ProtonVPN" solution.
- Build the "Setup\ProtonVPNTap.aip" project using Advanced Installer.
This creates TAP setup file "ProtonVPNTap.exe" in "Setup\ProtonVPNTap-SetupFiles"
folder. Otherwise, keep the previously built file. The code signing USB key must be
provided during build.
- Build the "Setup\ProtonVPN.aip" project using Advanced Installer. This creates 
ProtonVPN setup file "ProtonVPN_win_x.x.x.exe" in "Setup\ProtonVPN-SetupFiles" folder,
where "x.x.x" is the application version number. The code signing USB key must be
provided during build.

## Testing

The tests of this application are found in the "Test" folder. They can be run
using the default testing tools of Visual Studio.
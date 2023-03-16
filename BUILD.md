# Building Proton VPN Windows app

**Building guide of the Proton VPN Windows app.** 
Proton VPN Windows app is .NET 6 application created using C# and C++ programming languages.

## Prerequisites

To build Proton VPN the following tools have to be installed:

- [Visual Studio Community 2022](https://visualstudio.microsoft.com/downloads/)   (see details [here](#visual-studio))
- [Windows Software Development Kit (SDK)](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)
- [Windows Driver Kit (WDK)](https://learn.microsoft.com/en-us/windows-hardware/drivers/download-the-wdk)
- [Go (1.13.5+)](https://go.dev/dl/)   (see details [here](#go))
- [TDM-GCC MinGW Compiler](https://jmeubank.github.io/tdm-gcc/download/)

*To enable the sending of crash reports to Sentry server the environment variable SENTRY_DSN_V2 should contain valid Sentry DSN value during build. If environment variable SENTRY_DSN_V2 is not set, sending of crash reports will be disabled.*

Once all the [prerequisites](#prerequisites) have been installed
1. Clone the [repository](https://github.com/ProtonVPN/win-app)
2. Open a command prompt as an administrator
3. Navigate to the repository folder
4. Run: `git submodule update --init`
5. Run: `BuildDependencies.bat`
	- Check for error logs during the execution. It is possible that some nuget packages need to be restored. 
	- Open the solution(s) that gave the error(s), restore the nuget packages and try again

### Visual Studio

When installing Visual Studio 2022 select the following configuration:

- Workloads:
  - .NET desktop development
  - Desktop development with C++
- Individual components:
  - C++ ATL for latest v143 build tools with Spectre Mitigations (x86 & x64)
  - MSVC v143 - VS 2022 C++ x64/x86 Spectre-mitigated libs (Latest)

*The required individual components might vary depending on the Visual Studio version.*

After the installation, check that MS Build has been added to the PATH. (add if missing)

| Environment | Variable | Value |
| ----------- | -------- | ----- |
| System Variables | Path | C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\ |

### Go

Once installed, check the environment variables for Go. (add if missing)

| Environment | Variable | Value |
| ----------- | -------- | ----- |
| User Variables | GOPATH | C:\Users\<userame>\go |
| User Variables | Path | %USERPROFILE%\go\bin |
| System Variables | Path | C:\Program Files\Go\bin |

## Building

The application can be built using the default Visual Studio build
environment. To build the setup files use the following steps:

- Select the "Release" solution configuration in Visual Studio.
- Build the "ProtonVPN" solution.

## Testing

The tests of this application are found in the "Test" folder. They can be run
using the default testing tools of Visual Studio.

## Services

The **ProtonVPN Service** and **ProtonVPN Wireguard** services targets the installation folder by default 
("C:\Program Files\Proton\VPN\<version>").

For development, it is possible to: 
1. re-target the services to the repository output folder
```
SC CONFIG "ProtonVPN Service" binPath="<repository-folder>\src\bin\ProtonVPNService.exe"
NET STOP "ProtonVPN Service"

SC CONFIG "ProtonVPN Wireguard" binPath="<repository-folder>\src\bin\ProtonVPN.WireGuardService.exe C:\ProgramData\ProtonVPN\Wireguard\ProtonVPN.conf"
NET STOP "ProtonVPN Wireguard"
```
2. or to configure the solution to build into the installation folder directly

## Templates

Please check [VisualStudioItemTemplates/README.txt](VisualStudioItemTemplates/README.txt) to use recommended Proton default templates.
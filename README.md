# ProtonVPN for Windows

Copyright (c) 2020 Proton Technologies AG

This repository holds the ProtonVPN Windows application.  
For a detailed build information see [BUILD](BUILD.md).  
For licensing information see [COPYING](COPYING.md).

## Description

The [ProtonVPN](https://protonvpn.com) for Windows is intended for every ProtonVPN service user,
paid or free and supports all functionalities available to authenticated users (user signup instead happens on the web site).

### The application

The app consists of these interacting parts:
- ProtonVPN GUI application
- ProtonVPN Service
- ProtonVPN Update Service
- OpenVPN
- TAP adapter
- Split Tunnel driver

#### GUI application

The ProtonVPN GUI app is installed into "C:\Program Files (x86)\Proton Technologies\ProtonVPN" 
directory by default. The main executable is "ProtonVPN.exe".

ProtonVPN GUI app starts ProtonVPN Service and Update Service when launched and stops services
when closed.

App logs are saved to "%LOCALAPPDATA%\ProtonVPN\Logs" directory.

The ProtonVPN build using Debug configuration optionally loads its configuration from file 
"ProtonVPN.config" in the app directory. This file is not deployed during install. If the configuration
file doesn't exist or contains not valid values the app tries to save default configuration 
used in the app.

To monitor Http traffic of ProtonVPN GUI app using Fiddler or another tool, you might need to disable
TLS certificate pinning. To disable TLS certificate pinning the configuration file with empty
"TlsPinningConfig" value should be provided:
```
    ...
    "TlsPinningConfig": {}
    ...
```

#### ProtonVPN Service

The Windows service "ProtonVPN Service" is installed into
"C:\Program Files (x86)\Proton Technologies\ProtonVPN" directory by default. Service
executable is "ProtonVPNService.exe". The service is started and stopped by the ProtonVPN
GUI app.

During installation, the service is configured to be started and stopped by the unprivileged
interactive users.

Service executable supports installation and uninstallation of service. Passing "install" on
command line to "ProtonVPNService.exe" installs the service, passing "uninstall" - uninstalls.
This installation method doesn't configure service security settings.

Service is responsible for interaction with OpenVPN, managing Windows firewall and Split Tunnel
driver.

Service logs are saved to "%ALLUSERSPROFILE%\ProtonVPN\Logs" directory.

#### ProtonVPN Update Service

The Windows service "ProtonVPN Update Service" is installed into
"C:\Program Files (x86)\Proton Technologies\ProtonVPN" directory by default. Service
executable is "ProtonVPN.UpdateService.exe". The service is started and stopped by the ProtonVPN
GUI app.

During installation, the service is configured to be started and stopped by the unprivileged
interactive users.

Service is responsible for checking, downloading and installing app updates.

Service logs are saved to "%ALLUSERSPROFILE%\ProtonVPN\UpdaterLogs" directory.

To monitor Http traffic of ProtonVPN update service using Fiddler or another tool, you need to disable
TLS certificate pinning. To disable TLS certificate pinning the configuration file with empty
"TlsPinningConfig" value should be provided:
```
    ...
    "TlsPinningConfig": {}
    ...
```

Also, the file "ProtonVPN.UpdateService.exe.config" should be put into the app folder with the
following content or an existing file should be updated to contain the "<system.net>" section:
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <startup> 
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2" />
  </startup>
  <system.net>
    <defaultProxy enabled="true">
      <proxy proxyaddress="http://127.0.0.1:8888" bypassonlocal="False"/>
    </defaultProxy>
  </system.net>
</configuration>
```

This forces the ProtonVPN update service to send all Web traffic through local proxy openend
by the Fiddler.

#### OpenVPN

The ProtonVPN uses OpenVPN for maintaining a VPN tunnel. The new OpenVPN process is started on each
connect to a VPN and closed on disconnect. Communication with the OpenVPN process is maintained through
TCP management interface.

OpenVPN is installed into "C:\Program Files (x86)\Proton Technologies\ProtonVPN\Resources\"
directory by default. The OpenVPN config file is static, it doesn't change for each VPN server.

The OpenVPN is built from official source by applying a patch to support ProtonVPN specific
TAP adapter. See [win-openvpn](https://github.com/ProtonVPN/win-openvpn) repository.

#### TAP adapter

TAP adapter "TAP-ProtonVPN Windows Adapter V9" is used by the OpenVPN. 

The TAP adapter is installed into "C:\Program Files (x86)\Proton Technologies\ProtonVPNTap"
directory by default.

The TAP adapter is built from official source by applying a patch to have ProtonVPN specific
name and identification. See [win-tap-adapter](https://github.com/ProtonVPN/win-tap-adapter) repository.

#### Split Tunnel driver

The kernel-mode driver "ProtonVPN SplitTunnel" is used for redirecting socket bindings when
Split Tunnel is enabled.

The driver is installed as a system service. It is started when connecting to VPN and stopped
when disconnecting by ProtonVPN Service when Split Tunnel is enabled.

## Folder structure

The main repository folder contains the .NET Visual Studio solution of the
ProtonVPN Windows app named ProtonVPN.

### Forlder "ci" 

Contains continuous integration scripts.

### Forlder "packages" 

It contains NuGet packages of the ProtonVPN solution.

### Forlder "Setup"

This folder contains Advanced Installer setup project files, resources included in the installer,
and built installer files. Subfolders contain:

- "Images" - images for inclusion into the installer.
- "ProtonVPN-SetupFiles" - built ProtonVPN installer files.
- "ProtonVPNTap-SetupFiles" - built TAP adapter installer files. The latest successfully
  built TAP adapter installer file is required to build the ProtonVPN installer.
- "SplitTunnel" - SplitTunnel Callout driver for inclusion into the installer.

### Folder "src"

This folder contains Visual Studio solution projects.

### Folder "src\bin"

This folder contains Visual Studio project build output. This folder can be safely 
deleted as it's content is recreated by building the solution.

### Folder "src\srp"

This folder contains GIT submodule of [ProtonMail SRP library](https://github.com/ProtonMail/go-srp).

### Folder "test"

This folder contains test projects of the ProtonVPN solution. 

## Solution

ProtonVPN for Windows is created using C# and C++ programming languages, WPF and MVVM 
technologies. The Visual Studio solution consists of a series of projects:
- **ProtonVPN.App** - the main project which builds to ProtonVPN GUI app executable.
  It contains startup logic and GUI (view models and views).
- **ProtonVPN.Common** - the classes shared between projects.
- **ProtonVPN.Core** - the business logic of the application.
- **ProtonVPN.ErrorMessage** - displays an error message when the application cannot
  be run. Builds to an executable.
- **ProtonVPN.InstallActions** - the C++ actions used by the app installer.
- **ProtonVPN.IpFilter** - the C++ library for configuring Windows firewall filters.
- **ProtonVPN.Native** - the C# wrapper around Windows system libraries.
- **ProtonVPN.NetworkFilter** - the C# wrapper around C++ library for configuring Windows firewall.
- **ProtonVPN.NetworkUtil** - the C++ library for changing network configuration.
- **ProtonVPN.Resource** - contains resources shared between projects.
- **ProtonVPN.Service** - the Windows service which handles VPN, Windows firewall and 
  Split Tunneling.
- **ProtonVPN.Service.Contract** - contains the service contract.
- **ProtonVPN.SplitTunnelDriver** - the Split Tunnel callout driver written in C++.
- **ProtonVPN.TapInstaller** - the TAP install action used in the app installer.
- **ProtonVPN.TlsVerify** - the command line utility which verifies the VPN server certificate.
- **ProtonVPN.Update** - the application update module used in the update service.
- **ProtonVPN.UpdateService** - the Windows service which handles the app udates.
- **ProtonVPN.UpdateServiceContract** - contains the update service contract.
- **ProtonVPN.Vpn** - the OpenVPN management module used in the service.

Solution folder "Test" contains test projects.


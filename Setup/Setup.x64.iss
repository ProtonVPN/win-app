[Setup]
ArchitecturesAllowed=x64os

#define Architecture "x64"
#define SourcePath GetEnv("BUILD_PATH")
#define IsBTISource SourcePath == "src/bin/win-x64/BTI/publish"
#if IsBTISource
#define OutputBaseSuffix "_BTI"
#else
#define OutputBaseSuffix ""
#endif

#include "SetupBase.iss"

[Files]
Source: "Native\x64\tunnel.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "Native\x64\wintun.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "Native\x64\wireguard.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "Native\x64\wireguard-tunnel-tcp.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;

Source: "Native\x64\libcrypto-3-x64.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "Native\x64\libpkcs11-helper-1.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "Native\x64\libssl-3-x64.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "Native\x64\openvpn.exe"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "Native\x64\vcruntime140.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;

Source: "Native\x64\ProtonVPN.CalloutDriver.sys"; DestDir: "{app}\{#VersionFolder}\Resources"; AfterInstall: InstallNetworkDriver;

Source: "tap\x64\tapinstall.exe"; DestDir: "{app}\{#VersionFolder}\Resources\tap";
Source: "tap\x64\OemVista.inf"; DestDir: "{app}\{#VersionFolder}\Resources\tap";
Source: "tap\x64\tapprotonvpn.cat"; DestDir: "{app}\{#VersionFolder}\Resources\tap";
Source: "tap\x64\tapprotonvpn.Sys"; DestDir: "{app}\{#VersionFolder}\Resources\tap";

Source: "Dependencies\{#VcRedistX64Name}"; Flags: dontcopy;
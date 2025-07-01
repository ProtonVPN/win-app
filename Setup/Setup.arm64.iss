[Setup]
ArchitecturesAllowed=arm64

#define Architecture "arm64"
#define SourcePath GetEnv("BUILD_PATH")
#define IsBTISource SourcePath == "src/bin/win-arm64/BTI/publish"
#if IsBTISource
#define OutputBaseSuffix "_BTI"
#else
#define OutputBaseSuffix ""
#endif

#include "SetupBase.iss"

[Files]
Source: "Native\arm64\tunnel.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "Native\arm64\wintun.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "Native\arm64\wireguard.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "Native\arm64\wireguard-tunnel-tcp.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;

Source: "Native\arm64\libcrypto-3-arm64.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "Native\arm64\libpkcs11-helper-1.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "Native\arm64\libssl-3-arm64.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "Native\arm64\openvpn.exe"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;

Source: "Native\arm64\ProtonVPN.CalloutDriver.sys"; DestDir: "{app}\{#VersionFolder}\Resources"; AfterInstall: InstallNetworkDriver;

Source: "tap\arm64\tapinstall.exe"; DestDir: "{app}\{#VersionFolder}\Resources\tap";
Source: "tap\arm64\OemVista.inf"; DestDir: "{app}\{#VersionFolder}\Resources\tap";
Source: "tap\arm64\tapprotonvpn.cat"; DestDir: "{app}\{#VersionFolder}\Resources\tap";
Source: "tap\arm64\tapprotonvpn.sys"; DestDir: "{app}\{#VersionFolder}\Resources\tap";

Source: "Dependencies\{#VcRedistArm64Name}"; Flags: dontcopy;
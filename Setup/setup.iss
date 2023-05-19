#define MyAppVersion "3.0.0"
#define MyAppName "Proton VPN" 
#define MyAppExeName "ProtonVPN.exe"
#define LauncherExeName "ProtonVPN.Launcher.exe"

#define MyPublisher "Proton AG"

#define ServiceName "ProtonVPN Service"
#define ServiceExe "ProtonVPNService.exe"

#define WireGuardServiceName "ProtonVPN WireGuard"
#define WireGuardServiceExe "ProtonVPN.WireGuardService.exe"

#define NetworkDriverName "ProtonVPNCallout"
#define NetworkDriverFileName "Resources\ProtonVPN.CalloutDriver.sys"

#define Hash ""
#define VersionFolder "v" + MyAppVersion

#include "CodeDependencies.iss"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\Proton\VPN
DefaultGroupName=Proton
DisableDirPage=auto
DisableProgramGroupPage=auto
AppPublisher={#MyPublisher}
UninstallDisplayIcon={app}\{#LauncherExeName}
UninstallDisplayName={#MyAppName}
OutputBaseFilename=ProtonVPN_{#VersionFolder}
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
OutputDir=Installers
ArchitecturesInstallIn64BitMode=x64
SetupIconFile=Images\protonvpn.ico
SetupLogging=yes
DisableFinishedPage=yes
DisableStartupPrompt=yes
VersionInfoProductTextVersion={#MyAppVersion}-{#hash}
VersionInfoVersion={#MyAppVersion}
AppCopyright=© 2022 {#MyPublisher}
SignTool=signtool sign /a /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td SHA256 /fd SHA256 $f

[Messages]
SetupWindowTitle={#MyAppName}

[Files]
Source: "..\src\bin\ProtonVPN.Launcher.exe"; DestDir: "{app}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Launcher.dll"; DestDir: "{app}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Launcher.deps.json"; DestDir: "{app}";
Source: "..\src\bin\ProtonVPN.Launcher.runtimeconfig.json"; DestDir: "{app}";
Source: "..\src\ProtonVPN.NativeHost\bin\ProtonVPN.exe"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\ProtonVPN.NativeHost\bin\nethost.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.InstallActions.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.InstallActions.x86.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.dll.config"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.runtimeconfig.json"; DestDir: "{app}\{#VersionFolder}"
Source: "..\src\bin\ProtonVPN.RestoreInternet.exe"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.RestoreInternet.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.RestoreInternet.dll.config"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.RestoreInternet.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.RestoreInternet.runtimeconfig.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.TlsVerify.exe"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.TlsVerify.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.TlsVerify.dll.config"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.TlsVerify.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.TlsVerify.runtimeconfig.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.WireGuardService.exe"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.WireGuardService.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.WireGuardService.runtimeconfig.json"; DestDir: "{app}\{#VersionFolder}"
Source: "..\src\bin\ProtonVPNService.exe"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPNService.dll"; DestDir: "{app}\{#VersionFolder}"; AfterInstall: InstallService; Flags: signonce;
Source: "..\src\bin\ProtonVPNService.deps.json"; DestDir: "{app}\{#VersionFolder}"; AfterInstall: InstallService;
Source: "..\src\bin\ProtonVPNService.runtimeconfig.json"; DestDir: "{app}\{#VersionFolder}"; AfterInstall: InstallService;
Source: "..\src\bin\7za.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Albireo.Base32.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ARSoft.Tools.Net.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Autofac.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\BouncyCastle.Crypto.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ByteSize.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\CalcBinding.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Caliburn.Micro.Core.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Caliburn.Micro.Platform.Core.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Caliburn.Micro.Platform.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\DeviceId.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\DeviceId.Windows.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\DeviceId.Windows.Wmi.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\DnsClient.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\DynamicExpresso.Core.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\GalaSoft.MvvmLight.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Caliburn.Micro.Platform.Core.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\log4net.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Microsoft.Bcl.AsyncInterfaces.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Microsoft.Toolkit.Uwp.Notifications.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Microsoft.Web.WebView2.Core.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Microsoft.Web.WebView2.Wpf.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Microsoft.Win32.Registry.AccessControl.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Microsoft.Windows.SDK.NET.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Newtonsoft.Json.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\OxyPlot.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\OxyPlot.Wpf.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\OxyPlot.Wpf.Shared.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\PInvoke.Kernel32.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\PInvoke.Windows.Core.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\PInvoke.Windows.ShellScalingApi.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Polly.Contrib.WaitAndRetry.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Polly.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Announcements.Contracts.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Announcements.Contracts.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Announcements.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Announcements.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Announcements.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Announcements.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Api.Contracts.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Api.Contracts.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Api.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Api.dll.config"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Api.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Api.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Api.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Common.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Common.dll.config"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Common.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Common.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Common.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Core.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Core.dll.config"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Core.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Crypto.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Crypto.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Dns.Contracts.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Dns.Contracts.dll.config"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Dns.Contracts.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Dns.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Dns.dll.config"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Dns.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Dns.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Dns.Installers.dll.config"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Dns.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.EntityMapping.Contracts.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.EntityMapping.Contracts.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.EntityMapping.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.EntityMapping.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.EntityMapping.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.EntityMapping.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.HumanVerification.Contracts.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.HumanVerification.Contracts.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.HumanVerification.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.HumanVerification.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.HumanVerification.Contracts.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.HumanVerification.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.HumanVerification.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.HumanVerification.Gui.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.HumanVerification.Gui.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.IssueReporting.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.IssueReporting.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.IssueReporting.Contracts.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.IssueReporting.Contracts.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.IssueReporting.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.IssueReporting.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Native.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Native.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.NetworkFilter.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.NetworkFilter.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.ProcessCommunication.App.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.ProcessCommunication.App.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.ProcessCommunication.App.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.ProcessCommunication.App.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.ProcessCommunication.Common.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.ProcessCommunication.Common.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.ProcessCommunication.Contracts.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.ProcessCommunication.Contracts.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.ProcessCommunication.EntityMapping.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.ProcessCommunication.EntityMapping.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.ProcessCommunication.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.ProcessCommunication.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.ProcessCommunication.Service.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.ProcessCommunication.Service.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.ProcessCommunication.Service.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.ProcessCommunication.Service.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Resource.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Resource.dll.config"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Resource.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Translations.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Translations.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Update.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Update.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Update.Contracts.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Update.Contracts.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Update.Installers.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Update.Installers.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.Vpn.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.Vpn.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ProtonVPN.WireGuardDriver.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\ProtonVPN.WireGuardDriver.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\src\bin\ReswPlusLib.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Sentry.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Configuration.ConfigurationManager.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Diagnostics.EventLog.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Drawing.Common.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.IO.Packaging.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Text.Encodings.Web.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Text.Json.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Management.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Private.ServiceModel.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.ServiceProcess.ServiceController.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Reflection.Context.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Runtime.Caching.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Security.Cryptography.Pkcs.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Security.Cryptography.ProtectedData.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Security.Cryptography.Xml.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Security.Permissions.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.ServiceModel.Duplex.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.ServiceModel.Federation.dll"; DestDir: "{app}\{#VersionFolder}"
Source: "..\src\bin\System.ServiceModel.Http.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.ServiceModel.NetTcp.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.ServiceModel.Primitives.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.ServiceModel.Security.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.ServiceModel.Syndication.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.ServiceProcess.ServiceController.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\System.Windows.Extensions.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\WinRT.Runtime.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Grpc.Core.Api.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Grpc.Core.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Grpc.Net.Client.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Grpc.Net.Common.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\grpc_csharp_ext.x64.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Microsoft.Win32.Registry.AccessControl.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Microsoft.Win32.SystemEvents.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\Microsoft.Xaml.Behaviors.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\protobuf-net.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\protobuf-net.Grpc.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\bin\protobuf-net.Grpc.Native.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;

Source: "..\src\bin\en-US\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\en-US"; Flags: signonce;
Source: "..\src\bin\cs-CZ\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\cs-CZ"; Flags: signonce;
Source: "..\src\bin\de-DE\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\de-DE"; Flags: signonce;
Source: "..\src\bin\fa-IR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\fa-IR"; Flags: signonce;
Source: "..\src\bin\fr-FR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\fr-FR"; Flags: signonce;
Source: "..\src\bin\nl-NL\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\nl-NL"; Flags: signonce;
Source: "..\src\bin\hr-HR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\hr-HR"; Flags: signonce;
Source: "..\src\bin\id-ID\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\id-ID"; Flags: signonce;
Source: "..\src\bin\it-IT\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\it-IT"; Flags: signonce;
Source: "..\src\bin\pl-PL\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\pl-PL"; Flags: signonce;
Source: "..\src\bin\pt-PT\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\pt-PT"; Flags: signonce;
Source: "..\src\bin\pt-BR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\pt-BR"; Flags: signonce;
Source: "..\src\bin\ro-RO\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\ro-RO"; Flags: signonce;
Source: "..\src\bin\ru-RU\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\ru-RU"; Flags: signonce;
Source: "..\src\bin\es-ES\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\es-ES"; Flags: signonce;
Source: "..\src\bin\es-419\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\es-419"; Flags: signonce;
Source: "..\src\bin\uk-UA\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\uk-UA"; Flags: signonce;
Source: "..\src\bin\tr-TR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\tr-TR"; Flags: signonce;
Source: "..\src\bin\be-BY\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\be-BY"; Flags: signonce;

Source: "..\src\bin\Resources\GoSrp.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\src\bin\Resources\ProtonVPN.IPFilter.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\src\bin\Resources\ProtonVPN.NetworkUtil.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\src\bin\Resources\LocalAgent.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\src\bin\Resources\libcrypto-1_1-x64.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\src\bin\Resources\libpkcs11-helper-1.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\src\bin\Resources\libssl-1_1-x64.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\src\bin\Resources\openvpn.exe"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\src\bin\Resources\vcruntime140.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\src\bin\Resources\config.ovpn"; DestDir: "{app}\{#VersionFolder}\Resources";
Source: "..\src\ProtonVPN.Vpn\Resources\wireguard.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\ProtonVPN.Vpn\Resources\tunnel.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;

Source: "..\src\bin\runtimes\win-x64\native\WebView2Loader.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;

Source: "tap\tapinstall.exe"; DestDir: "{app}\{#VersionFolder}\Resources\tap"
Source: "tap\OemVista.inf"; DestDir: "{app}\{#VersionFolder}\Resources\tap"
Source: "tap\tapprotonvpn.cat"; DestDir: "{app}\{#VersionFolder}\Resources\tap"
Source: "tap\tapprotonvpn.Sys"; DestDir: "{app}\{#VersionFolder}\Resources\tap";

Source: "SplitTunnel\ProtonVPN.CalloutDriver.sys"; DestDir: "{app}\{#VersionFolder}\Resources"; AfterInstall: InstallNetworkDriver;
Source: "tun\wintun.dll"; DestDir: "{app}\{#VersionFolder}\Resources";
Source: "GuestHoleServers.json"; DestDir: "{app}\{#VersionFolder}\Resources";
Source: "NetCoreCheck_x64.exe"; Flags: dontcopy;
Source: "NetCoreCheckCA.dll"; Flags: dontcopy;

[Icons]
Name: "{group}\Proton VPN"; Filename: "{app}\{#LauncherExeName}"
Name: "{commondesktop}\Proton VPN"; Filename: "{app}\{#LauncherExeName}"

[Languages]
Name: "en_US"; MessagesFile: "compiler:Default.isl"
Name: "cs_CZ"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "de_DE"; MessagesFile: "compiler:Languages\German.isl"
Name: "fr_FR"; MessagesFile: "compiler:Languages\French.isl"
Name: "nl_NL"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "it_IT"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "pl_PL"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "pt_BR"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "pt_PT"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "ru_RU"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "es_ES"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "uk_UA"; MessagesFile: "compiler:Languages\Ukrainian.isl"
Name: "tr_TR"; MessagesFile: "compiler:Languages\Turkish.isl"

[UninstallDelete]
Type: filesandordirs; Name: "{commonappdata}\ProtonVPN"
Type: filesandordirs; Name: "{localappdata}\ProtonVPN"

[Dirs]
Name: "{localappdata}\ProtonVPN\DiagnosticLogs"

[Code]
function InitLogger(logger: Longword): Integer;
external 'InitLogger@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function InitLoggerUninstall(logger: Longword): Integer;
external 'InitLogger@{%TEMP}\ProtonVPN.InstallActions.x86.dll cdecl delayload uninstallonly';

function UpdateTaskbarIconTarget(launcherPath: String): Integer;
external 'UpdateTaskbarIconTarget@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function UninstallProduct(upgradeCode: String): Integer;
external 'UninstallProduct@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function UninstallTapAdapter(tapFilesPath: String): Integer;
external 'UninstallTapAdapter@ProtonVPN.InstallActions.x86.dll cdecl delayload uninstallonly';

function RemoveWfpObjects(): Integer;
external 'RemoveWfpObjects@ProtonVPN.InstallActions.x86.dll cdecl delayload uninstallonly';

function SaveOldUserConfigFolder(): Integer;
external 'SaveOldUserConfigFolder@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function RestoreOldUserConfigFolder(applicationPath: String): Integer;
external 'RestoreOldUserConfigFolder@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function InstallWindowsService(name, displayName, path: String): Integer;
external 'InstallService@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function IsProcessRunning(processName: String): Boolean;
external 'IsProcessRunning@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function IsProcessRunningByPath(processPath: String): Boolean;
external 'IsProcessRunningByPath@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function InstallCalloutDriver(name, displayName, path: String): Integer;
external 'InstallCalloutDriver@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function UninstallService(name: String): Integer;
external 'UninstallService@{%TEMP}\ProtonVPN.InstallActions.x86.dll cdecl delayload uninstallonly';

function lstrlenW(lpString: Cardinal): Cardinal;
external 'lstrlenW@kernel32.dll stdcall';

function lstrcpyW(lpStringDest: String; lpStringSrc: Cardinal): Integer;
external 'lstrcpyW@kernel32.dll stdcall';

type
  TInt64Array = array of Int64;

var
  IsToReboot, IsVerySilent: Boolean;

function NeedRestart(): Boolean;
begin
  Result := IsToReboot;
end;

procedure LogProc(ptr: Cardinal);
var
  length: Cardinal;
  line: String;
begin
  length := lstrlenW(ptr);
  line := '';
  SetLength(line, length);

  if length > 0 then begin
    lstrcpyW(line, ptr);
    Log(line);
  end;
end;

procedure DeleteNonRunningVersions(const Directory: string);
var
  VersionFolder: TFindRec;
  VersionFolderPath: String;
  i: Integer;
  Processes: array of String;
  IsRunningProcessFound: Boolean;
begin
  Processes := ['ProtonVPN.exe', 'ProtonVPNService.exe', 'ProtonVPN.WireGuardService.exe'];
  if FindFirst(ExpandConstant(Directory + '\v*'), VersionFolder) then
  try
    repeat
      VersionFolderPath := AddBackslash(Directory) + AddBackslash(VersionFolder.Name)
      IsRunningProcessFound := False;
      for i := 0 to GetArrayLength(Processes) - 1 do
      begin
        if IsProcessRunningByPath(VersionFolderPath + Processes[i]) then
        begin
          Log('Running process detected: ' + VersionFolderPath + Processes[i]);
          IsRunningProcessFound := True;
          Break;
        end;
      end;

      if IsRunningProcessFound then
        Log('Skipping version ' + VersionFolder.Name + ' as running processes were found.')
      else
      begin
        if DelTree(VersionFolderPath, True, True, True) then
          Log('An old version ' + VersionFolder.Name + ' was removed.')
        else
          Log('Failed to remove an old version: ' + VersionFolder.Name + 'Error: ' + SysErrorMessage(DLLGetLastError));
      end;
    until
      not FindNext(VersionFolder);
  finally
    FindClose(VersionFolder);
  end;
end;

function InstallServiceInner(name, displayName, path: String): Integer;
begin
  Log('Installing Service ' + name + ' Path: ' + path);
  Result := InstallWindowsService(name, displayName, path);
  Log('Service install returned: ' + IntToStr(Result)); 
end;

procedure InstallService();
begin
  InstallServiceInner('{#ServiceName}', '{#ServiceName}', ExpandConstant('{app}') + '\{#VersionFolder}' + '\{#ServiceExe}');
end;

function InstallCalloutDriverInner(name, displayName, path: String): Integer;
begin
  Log('Installing callout driver ' + name + ' Path: ' + path);
  Result := InstallCalloutDriver(name, displayName, path);
  Log('Callout driver install returned: ' + IntToStr(Result)); 
end;

procedure InstallNetworkDriver();
begin
  InstallCalloutDriverInner('{#NetworkDriverName}', '{#NetworkDriverName}', ExpandConstant('{app}') + '\{#VersionFolder}' + '\{#NetworkDriverFileName}');
end;

function IsWindowsVersionEqualOrHigher(Major, Minor, Build: Integer): Boolean;
var
  Version: TWindowsVersion;
begin
  GetWindowsVersionEx(Version);
  Result :=
    (Version.Major > Major) or
    ((Version.Major = Major) and (Version.Minor > Minor)) or
    ((Version.Major = Major) and (Version.Minor = Minor) and (Version.Build >= Build));
end;

procedure SetIsVerySilent();
var
  i: Integer;
begin
  isVerySilent := False;
  for i := 1 to ParamCount do
    if CompareText(ParamStr(i), '/verysilent') = 0 then
    begin
      IsVerySilent := True;
      break;
    end;
end;

function InitializeSetup(): Boolean;
var
  Version: String;
  ErrCode: Integer;
  WindowsVersion: TWindowsVersion;
begin
  SetIsVerySilent();
  if IsWindowsVersionEqualOrHigher(10, 0, 17763) = False then begin
    if WizardSilent() = false then begin
      MsgBox('This application does not support your Windows version. You will be redirected to a download page with an application suitable for your Windows version. ', mbInformation, MB_OK);
      ShellExec('open', 'https://protonvpn.com/free-vpn/windows/windows7', '', '', SW_SHOW, ewNoWait, ErrCode);
    end;
    Result := False;
    exit;
  end;
  if RegValueExists(HKEY_LOCAL_MACHINE,'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppName}_is1', 'DisplayVersion') then begin
    RegQueryStringValue(HKEY_LOCAL_MACHINE,'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppName}_is1', 'DisplayVersion', Version);
    if Version > '{#MyAppVersion}' then begin
      if WizardSilent() = false then begin
        MsgBox(ExpandConstant('{#MyAppName} is already installed with the newer version ' + Version + '.'), mbInformation, MB_OK);
      end;
      Result := False;
    end
    else begin
      Result := True;
    end;
  end
  else begin
    Result := True;
  end;

  if Result = False then begin
    exit;
  end;

  InitLogger(CreateCallback(@LogProc));
  Dependency_AddDotNet60Asp;
  Dependency_AddDotNet60Desktop;
  Dependency_AddVC2015To2022;
  Result := true;
end;

function InitializeUninstall: Boolean;
begin
  Log('InitializeUninstall');
  Result := FileCopy(ExpandConstant('{app}\{#VersionFolder}\ProtonVPN.InstallActions.x86.dll'), ExpandConstant('{%TEMP}\ProtonVPN.InstallActions.x86.dll'), False);
  InitLoggerUninstall(CreateCallback(@LogProc));
end;

function UninstallServiceInner(name: String): Integer;
begin
  Log('Uninstalling Service ' + name);
  Result := UninstallService(name);
  Log('Service uninstall returned: ' + IntToStr(Result));
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
    DeleteNonRunningVersions(ExpandConstant('{app}'));
    Log('Trying to save user settings for the old ProtonVPN app if it is installed');
    SaveOldUserConfigFolder();
    Log('Trying to update taskbar icon path if exists');
    UpdateTaskbarIconTarget(ExpandConstant('{app}\{#VersionFolder}\{#MyAppExeName}'));
    Log('Trying to uninstall an old version of ProtonVPN app');
    UninstallProduct('{2B10124D-2F81-4BB1-9165-4F9B1B1BA0F9}');
    Log('Trying to uninstall an old version of ProtonVPN TUN adapter');
    UninstallProduct('{FED0679F-A292-4507-AEF5-DD2BB8898A36}');
    Log('Trying to uninstall an old version of ProtonVPN TAP adapter');
    UninstallProduct('{E23B9F7F-AA0A-481A-8ECA-FA69794BF50A}');
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  logfilepathname, logfilename, newfilepathname, langCode, launcherArgs: String;
  res: Integer;
begin
  if CurStep = ssDone then begin
    logfilepathname := ExpandConstant('{log}');
    logfilename := ExtractFileName(logfilepathname);
    newfilepathname := ExpandConstant('{localappdata}') + '\ProtonVPN\DiagnosticLogs\ProtonVPN_install.log';
    FileCopy(logfilepathname, newfilepathname, false);
    if IsProcessRunning('{#MyAppExeName}') then begin
      exit;
    end;
    RestoreOldUserConfigFolder(ExpandConstant('{app}'));
    if not IsToReboot or WizardForm.NoRadio.Checked = True then begin
      launcherArgs := '';
      if WizardSilent() = false then begin
        langCode := ActiveLanguage();
        StringChangeEx(langCode, '_', '-', True);
        launcherArgs := '/lang ' + langCode;
      end;
      if IsVerySilent = false then begin
        ExecAsOriginalUser(ExpandConstant('{app}\{#LauncherExeName}'), launcherArgs, '', SW_SHOW, ewNoWait, res);
      end;
    end;
  end
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var res, errorCode: Integer;
begin
  Log('CurUninstallStepChanged(' + IntToStr(Ord(CurUninstallStep)) + ') called');
  if CurUninstallStep = usUninstall then begin
    Log('Killing {#MyAppExeName} process');
    ShellExec('open', 'taskkill.exe', '/f /im {#MyAppExeName}', '', SW_HIDE, ewNoWait, errorCode);
    Log('taskkill returned ' + IntToStr(errorCode) + ' code');
    UninstallServiceInner('{#ServiceName}');
    UninstallServiceInner('{#WireGuardServiceName}');
    UninstallServiceInner('{#NetworkDriverName}');
    Log('Uninstalling TAP adapter driver');
    res := UninstallTapAdapter(ExpandConstant('{app}\{#VersionFolder}\Resources\tap'));
    Log('TAP uninstallation returned: ' + IntToStr(res));
    res := RemoveWfpObjects();
    Log('RemoveWfpObjects returned: ' + IntToStr(res));
  end;
end;
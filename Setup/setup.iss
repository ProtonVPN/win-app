#define MyAppVersion "4.0.0"
#define MyAppName "Proton VPN" 
#define ClientName "ProtonVPN.Client"
#define MyAppExeName ClientName + ".exe"
#define LegacyMyAppExeName "ProtonVPN.exe"
#define LauncherExeName "ProtonVPN.Launcher.exe"
#define AppUserModelID "Proton.VPN"

#define MyPublisher "Proton AG"

#define ServiceName "ProtonVPN Service"
#define ServiceExe "ProtonVPNService.exe"

#define WireGuardServiceName "ProtonVPN WireGuard"
#define WireGuardServiceExe "ProtonVPN.WireGuardService.exe"

#define NetworkDriverName "ProtonVPNCallout"
#define NetworkDriverFileName "Resources\ProtonVPN.CalloutDriver.sys"

#define InstallLogPath "{app}\Install.log.txt"

#define Hash ""
#define VersionFolder "v" + MyAppVersion
#define DisableAutoUpdateInstallerArg "/DISABLEAUTOUPDATE"
#define DisableAutoUpdateClientArg "-DisableAutoUpdate"
#define RegistryRunPath "Software\Microsoft\Windows\CurrentVersion\Run"
#define LegacyClientName "ProtonVPN"

#define SourcePath GetEnv("BUILD_PATH")
#define IsBTISource SourcePath == "BTI/publish"
#if IsBTISource
#define OutputBaseSuffix "_BTI"
#else
#define OutputBaseSuffix ""
#endif

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\Proton\VPN
DefaultGroupName=Proton
DisableDirPage=yes
AlwaysShowDirOnReadyPage=yes
DisableProgramGroupPage=auto
AppPublisher={#MyPublisher}
UninstallDisplayIcon={app}\{#LauncherExeName}
UninstallDisplayName={#MyAppName}
OutputBaseFilename=ProtonVPN_{#VersionFolder}{#OutputBaseSuffix}
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
LZMAUseSeparateProcess=yes
LZMANumBlockThreads=6
OutputDir=Installers
ArchitecturesInstallIn64BitMode=x64
SetupIconFile=Images\protonvpn.ico
SetupLogging=yes
DisableFinishedPage=yes
DisableStartupPrompt=yes
VersionInfoProductTextVersion={#MyAppVersion}-{#hash}
VersionInfoVersion={#MyAppVersion}
AppCopyright=© 2023 {#MyPublisher}
SignTool=signtool sign /a /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td SHA256 /fd SHA256 $f

[Messages]
SetupWindowTitle={#MyAppName}

[Registry]
Root: HKCR; Subkey: "ProtonVPN"; Flags: uninsdeletekey;
Root: HKCR; Subkey: "AppUserModelId\{#AppUserModelID}"; Flags: uninsdeletekey;

[Files]
Source: "..\{#SourcePath}\ProtonVPN.Launcher.exe"; DestDir: "{app}"; Flags: signonce;

Source: "..\{#SourcePath}\*.exe"; Excludes: "ProtonVPN.Launcher.exe,ProtonVPNService.exe,createdump.exe"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\{#SourcePath}\*.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\{#SourcePath}\*.pri"; DestDir: "{app}\{#VersionFolder}";
Source: "..\{#SourcePath}\*.deps.json"; DestDir: "{app}\{#VersionFolder}";

Source: "..\{#SourcePath}\ProtonVPNService.exe"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce; AfterInstall: InstallService;

Source: "..\{#SourcePath}\en-us\Microsoft.ui.xaml.dll.mui"; DestDir: "{app}\{#VersionFolder}\en-us";
Source: "..\{#SourcePath}\fr-FR\Microsoft.ui.xaml.dll.mui"; DestDir: "{app}\{#VersionFolder}\fr-FR";

Source: "..\{#SourcePath}\Resources\GoSrp.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\libcrypto-1_1-x64.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\libpkcs11-helper-1.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\libssl-1_1-x64.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\LocalAgent.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\ProtonVPN.InstallActions.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\ProtonVPN.InstallActions.x86.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\ProtonVPN.IPFilter.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\ProtonVPN.NetworkUtil.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\vcruntime140.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\openvpn.exe"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\config.ovpn"; DestDir: "{app}\{#VersionFolder}\Resources";

Source: "..\src\ProtonVPN.Vpn\Resources\wireguard.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\ProtonVPN.Vpn\Resources\tunnel.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;

Source: "tap\tapinstall.exe"; DestDir: "{app}\{#VersionFolder}\Resources\tap"
Source: "tap\OemVista.inf"; DestDir: "{app}\{#VersionFolder}\Resources\tap"
Source: "tap\tapprotonvpn.cat"; DestDir: "{app}\{#VersionFolder}\Resources\tap"
Source: "tap\tapprotonvpn.Sys"; DestDir: "{app}\{#VersionFolder}\Resources\tap";

Source: "SplitTunnel\ProtonVPN.CalloutDriver.sys"; DestDir: "{app}\{#VersionFolder}\Resources"; AfterInstall: InstallNetworkDriver;
Source: "tun\wintun.dll"; DestDir: "{app}\{#VersionFolder}\Resources";
Source: "GuestHoleServers.json"; DestDir: "{app}\{#VersionFolder}\Resources";

Source: "..\{#SourcePath}\Microsoft.UI.Xaml\Assets\*.png"; DestDir: "{app}\{#VersionFolder}\Microsoft.UI.Xaml\Assets";

Source: "..\{#SourcePath}\Assets\*.ico"; DestDir: "{app}\{#VersionFolder}\Assets";

Source: "..\{#SourcePath}\Assets\Illustrations\Dark\*"; DestDir: "{app}\{#VersionFolder}\ProtonVPN.Client.Common.UI\Assets\Illustrations\Dark";
Source: "..\{#SourcePath}\Assets\Illustrations\Light\*"; DestDir: "{app}\{#VersionFolder}\ProtonVPN.Client.Common.UI\Assets\Illustrations\Light";
Source: "..\{#SourcePath}\Assets\Illustrations\*"; DestDir: "{app}\{#VersionFolder}\ProtonVPN.Client.Common.UI\Assets\Illustrations";

Source: "..\{#SourcePath}\Assets\Icons\App\*"; DestDir: "{app}\{#VersionFolder}\ProtonVPN.Client.Common.UI\Assets\Icons\App";
Source: "..\{#SourcePath}\Assets\Icons\Streaming\*"; DestDir: "{app}\{#VersionFolder}\ProtonVPN.Client.Common.UI\Assets\Icons\Streaming";

Source: "..\{#SourcePath}\Assets\Flags\Dark\*"; DestDir: "{app}\{#VersionFolder}\ProtonVPN.Client.Common.UI\Assets\Flags\Dark";
Source: "..\{#SourcePath}\Assets\Flags\Light\*"; DestDir: "{app}\{#VersionFolder}\ProtonVPN.Client.Common.UI\Assets\Flags\Light";
Source: "..\{#SourcePath}\Assets\Flags\*"; DestDir: "{app}\{#VersionFolder}\ProtonVPN.Client.Common.UI\Assets\Flags";

[Icons]
Name: "{group}\Proton VPN"; Filename: "{app}\{#LauncherExeName}"
Name: "{commondesktop}\Proton VPN"; Filename: "{app}\{#LauncherExeName}"; Tasks: desktopicon; AppUserModelID: "{#AppUserModelID}";

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}";

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
Name: "sl_SI"; MessagesFile: "compiler:Languages\Slovenian.isl"

[UninstallDelete]
Type: filesandordirs; Name: "{app}\{#VersionFolder}\ServiceData"
Type: filesandordirs; Name: "{app}\{#VersionFolder}\Resources"
Type: files; Name: "{#InstallLogPath}"

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

function RemovePinnedIcons(shortcutPath: String): Integer;
external 'RemovePinnedIcons@ProtonVPN.InstallActions.x86.dll cdecl delayload uninstallonly';

function RemoveWfpObjects(): Integer;
external 'RemoveWfpObjects@ProtonVPN.InstallActions.x86.dll cdecl delayload uninstallonly';

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
  IsToReboot, IsVerySilent, IsToDisableAutoUpdate: Boolean;

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
  VersionFolderPath, ProcessPath: String;
  i: Integer;
  Processes: array of String;
  IsRunningProcessFound: Boolean;
begin
  Log('Using directory ' + Directory + ' to find previous app versions for deletion');
  Processes := ['ProtonVPN.Client.exe', 'ProtonVPN.exe', 'ProtonVPNService.exe', 'ProtonVPN.WireGuardService.exe'];
  if FindFirst(ExpandConstant(Directory + '\v*'), VersionFolder) then
  try
    repeat
      Log('Found version folder ' + VersionFolder.Name);
      VersionFolderPath := AddBackslash(Directory) + AddBackslash(VersionFolder.Name)
      IsRunningProcessFound := False;
      for i := 0 to GetArrayLength(Processes) - 1 do
      begin
        ProcessPath := VersionFolderPath + Processes[i];
        Log('Checking if the process ' + ProcessPath + ' is running');
        if IsProcessRunningByPath(ProcessPath) then
        begin
          Log('Running process detected: ' + ProcessPath);
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

// Returns 0 (zero) if versions are equal, -1 (negative one) if version1 is older and 1 (positive one) if version1 is more recent
function CompareVersions(version1, version2: String): Integer;
var
    packVersion1, packVersion2: Int64;
begin
    if not StrToVersion(version1, packVersion1) then packVersion1 := 0;
    if not StrToVersion(version2, packVersion2) then packVersion2 := 0;
    Result := ComparePackedVersion(packVersion1, packVersion2);
end;

procedure SetIsToDisableAutoUpdate();
begin
  IsToDisableAutoUpdate := Pos('{#DisableAutoUpdateInstallerArg}', GetCmdTail()) > 0;
  if IsToDisableAutoUpdate = true then begin
    Log('The app will be launched with auto updates disabled.');
  end;
end;

function InitializeSetup(): Boolean;
var
  Version: String;
  ErrCode: Integer;
begin
  SetIsVerySilent();
  SetIsToDisableAutoUpdate();
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
    if CompareVersions(Version, '{#MyAppVersion}') > 0 then begin
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
  Result := true;
end;

function InitializeUninstall: Boolean;
begin
  Log('InitializeUninstall');
  Result := FileCopy(ExpandConstant('{app}\{#VersionFolder}\Resources\ProtonVPN.InstallActions.x86.dll'), ExpandConstant('{%TEMP}\ProtonVPN.InstallActions.x86.dll'), False);
  InitLoggerUninstall(CreateCallback(@LogProc));
end;

function UninstallServiceInner(name: String): Integer;
begin
  Log('Uninstalling Service ' + name);
  Result := UninstallService(name);
  Log('Service uninstall returned: ' + IntToStr(Result));
end;

procedure DeleteStartupApp(name: String);
begin
    if RegValueExists(HKEY_CURRENT_USER, ExpandConstant('{#RegistryRunPath}'), name) then
      if RegDeleteValue(HKEY_CURRENT_USER, ExpandConstant('{#RegistryRunPath}'), name) then
        Log(name + ' startup record was removed successfully.')
      else
        Log('Failed to remove ' + name + 'startup record')
    else
      Log(name + ' startup record does not exist.');
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
    DeleteNonRunningVersions(ExpandConstant('{app}'));

    Log('Trying to update taskbar icon path if exists');
    UpdateTaskbarIconTarget(ExpandConstant('{app}\{#VersionFolder}\{#MyAppExeName}'));

    Log('Trying to uninstall an old version of ProtonVPN app');
    UninstallProduct('{2B10124D-2F81-4BB1-9165-4F9B1B1BA0F9}');

    Log('Trying to uninstall an old version of ProtonVPN TUN adapter');
    UninstallProduct('{FED0679F-A292-4507-AEF5-DD2BB8898A36}');

    Log('Trying to uninstall an old version of ProtonVPN TAP adapter');
    UninstallProduct('{E23B9F7F-AA0A-481A-8ECA-FA69794BF50A}');

    Log('Trying to delete a legacy app startup record if exists');
    DeleteStartupApp(ExpandConstant('{#LegacyClientName}'));

    Result := '';
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  logfilepathname, logfilename, newfilepathname, langCode, launcherArgs: String;
  res: Integer;
begin
  if CurStep = ssDone then begin
    logfilepathname := ExpandConstant('{log}');
    logfilename := ExtractFileName(logfilepathname);
    newfilepathname := ExpandConstant('{#InstallLogPath}');
    FileCopy(logfilepathname, newfilepathname, false);
    if IsProcessRunning('{#MyAppExeName}') or IsProcessrunning('{#LegacyMyAppExeName}') then begin
      exit;
    end;
    if not IsToReboot or WizardForm.NoRadio.Checked = True then begin
      launcherArgs := '';
      if WizardSilent() = false then begin
        langCode := ActiveLanguage();
        StringChangeEx(langCode, '_', '-', True);
        launcherArgs := '-Language ' + langCode;
      end;
      if IsToDisableAutoUpdate = true then begin
        launcherArgs := launcherArgs + ' {#DisableAutoUpdateClientArg}';
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

    Log('Trying to delete client startup record if exists');
    DeleteStartupApp(ExpandConstant('{#ClientName}'));
  end;
end;
#define MyAppVersion "3.0.0"
#define MyAppName "Proton VPN" 
#define MyAppExeName "ProtonVPN.exe"
#define LauncherExeName "ProtonVPN.Launcher.exe"
#define AppUserModelID "Proton.VPN"

#define MyPublisher "Proton AG"

#define ServiceName "ProtonVPN Service"
#define ServiceExe "ProtonVPNService.exe"

#define WireGuardServiceName "ProtonVPN WireGuard"
#define WireGuardServiceExe "ProtonVPN.WireGuardService.exe"

#define NetworkDriverName "ProtonVPNCallout"
#define NetworkDriverFileName "Resources\ProtonVPN.CalloutDriver.sys"

#define ProtonDriveDownloaderName "ProtonDrive.Downloader.exe"

#define Hash ""
#define VersionFolder "v" + MyAppVersion
#define AppFolder "Proton\VPN"
#define SourcePath GetEnv("BUILD_PATH")
#define IsBTISource SourcePath == "src/bin/win-x64/BTI/publish"
#if IsBTISource
#define OutputBaseSuffix "_BTI"
#else
#define OutputBaseSuffix ""
#endif

#include "CodeDependencies.iss"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={pf}
DefaultGroupName=Proton
DisableDirPage=auto
DisableProgramGroupPage=auto
AppPublisher={#MyPublisher}
UninstallDisplayIcon={app}\{#LauncherExeName}
UninstallDisplayName={#MyAppName}
OutputBaseFilename=ProtonVPN_{#VersionFolder}{#OutputBaseSuffix}
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
OutputDir=Installers
ArchitecturesInstallIn64BitMode=x64
SetupIconFile=Images\protonvpn.ico
SetupLogging=yes
DisableFinishedPage=yes
DisableStartupPrompt=yes
DirExistsWarning=no
VersionInfoProductTextVersion={#MyAppVersion}-{#hash}
VersionInfoVersion={#MyAppVersion}
AppCopyright=© 2022 {#MyPublisher}
SignTool=signtool sign /a /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td SHA256 /fd SHA256 $f

[Messages]
SetupWindowTitle={#MyAppName}

[Registry]
Root: HKCR; Subkey: "ProtonVPN"; Flags: uninsdeletekey;
Root: HKCR; Subkey: "AppUserModelId\{#AppUserModelID}"; Flags: uninsdeletekey;
; Old registry values when we didn't have AppUserModelID set
Root: HKCR; Subkey: "AppUserModelId\{{6D809377-6AF0-444B-8957-A3773F02200E}\Proton\VPN"; Flags: deletekey uninsdeletekey;
Root: HKCR; Subkey: "ProtonVPN"; ValueType: string; ValueName: "URL Protocol"; ValueData: "";
Root: HKCR; Subkey: "ProtonVPN\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#VersionFolder}\{#MyAppExeName}"" ""%1""";

[Files]
Source: "..\src\ProtonVPN.NativeHost\bin\ProtonVPN.exe"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\ProtonVPN.NativeHost\bin\nethost.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;

Source: "..\{#SourcePath}\ProtonVPN.Launcher.exe"; DestDir: "{app}"; Flags: signonce;

Source: "..\{#SourcePath}\ProtonVPNService.exe"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\{#SourcePath}\ProtonVPNService.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\{#SourcePath}\ProtonVPNService.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\{#SourcePath}\ProtonVPNService.runtimeconfig.json"; DestDir: "{app}\{#VersionFolder}"; AfterInstall: InstallService;

Source: "..\{#SourcePath}\*.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\{#SourcePath}\*.exe"; Excludes: "ProtonVPN.exe,ProtonVPN.Launcher.exe,ProtonVPNService.exe,createdump.exe"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\{#SourcePath}\*.deps.json"; DestDir: "{app}\{#VersionFolder}";
Source: "..\{#SourcePath}\*.dll.config"; DestDir: "{app}\{#VersionFolder}";

Source: "..\{#SourcePath}\runtimes\win-x64\native\*"; DestDir: "{app}\{#VersionFolder}\runtimes\win-x64\native";

Source: "..\{#SourcePath}\en-US\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\en-US"; Flags: signonce;
Source: "..\{#SourcePath}\cs-CZ\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\cs-CZ"; Flags: signonce;
Source: "..\{#SourcePath}\de-DE\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\de-DE"; Flags: signonce;
Source: "..\{#SourcePath}\fa-IR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\fa-IR"; Flags: signonce;
Source: "..\{#SourcePath}\fr-FR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\fr-FR"; Flags: signonce;
Source: "..\{#SourcePath}\nl-NL\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\nl-NL"; Flags: signonce;
Source: "..\{#SourcePath}\hr-HR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\hr-HR"; Flags: signonce;
Source: "..\{#SourcePath}\id-ID\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\id-ID"; Flags: signonce;
Source: "..\{#SourcePath}\it-IT\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\it-IT"; Flags: signonce;
Source: "..\{#SourcePath}\pl-PL\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\pl-PL"; Flags: signonce;
Source: "..\{#SourcePath}\pt-PT\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\pt-PT"; Flags: signonce;
Source: "..\{#SourcePath}\pt-BR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\pt-BR"; Flags: signonce;
Source: "..\{#SourcePath}\ro-RO\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\ro-RO"; Flags: signonce;
Source: "..\{#SourcePath}\ru-RU\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\ru-RU"; Flags: signonce;
Source: "..\{#SourcePath}\es-ES\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\es-ES"; Flags: signonce;
Source: "..\{#SourcePath}\es-419\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\es-419"; Flags: signonce;
Source: "..\{#SourcePath}\uk-UA\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\uk-UA"; Flags: signonce;
Source: "..\{#SourcePath}\tr-TR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\tr-TR"; Flags: signonce;
Source: "..\{#SourcePath}\be-BY\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\be-BY"; Flags: signonce;
Source: "..\{#SourcePath}\ka-GE\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\ka-GE"; Flags: signonce;
Source: "..\{#SourcePath}\el-GR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\el-GR"; Flags: signonce;
Source: "..\{#SourcePath}\fi-FI\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\fi-FI"; Flags: signonce;
Source: "..\{#SourcePath}\ko-KR\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\ko-KR"; Flags: signonce;
Source: "..\{#SourcePath}\zh-TW\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\zh-TW"; Flags: signonce;
Source: "..\{#SourcePath}\sv-SE\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\sv-SE"; Flags: signonce;
Source: "..\{#SourcePath}\ja-JP\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\ja-JP"; Flags: signonce;

Source: "..\{#SourcePath}\Resources\*.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\*.exe"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\src\ProtonVPN.Vpn\Resources\wireguard.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;
Source: "..\src\ProtonVPN.Vpn\Resources\tunnel.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;

Source: "tap\tapinstall.exe"; DestDir: "{app}\{#VersionFolder}\Resources\tap"
Source: "tap\OemVista.inf"; DestDir: "{app}\{#VersionFolder}\Resources\tap"
Source: "tap\tapprotonvpn.cat"; DestDir: "{app}\{#VersionFolder}\Resources\tap"
Source: "tap\tapprotonvpn.Sys"; DestDir: "{app}\{#VersionFolder}\Resources\tap";

Source: "SplitTunnel\ProtonVPN.CalloutDriver.sys"; DestDir: "{app}\{#VersionFolder}\Resources"; AfterInstall: InstallNetworkDriver;
Source: "tun\wintun.dll"; DestDir: "{app}\{#VersionFolder}\Resources";
Source: "GuestHoleServers.json"; DestDir: "{app}\{#VersionFolder}\Resources";

[Icons]
Name: "{group}\Proton VPN"; Filename: "{app}\{#LauncherExeName}"
Name: "{commondesktop}\Proton VPN"; Filename: "{app}\{#LauncherExeName}"; Tasks: desktopicon; AppUserModelID: "{#AppUserModelID}";

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; 
Name: "installProtonDrive"; Description: "{cm:InstallProtonDriveTitle}"; Check: ShouldDisplayProtonDriveCheckbox;

[Run]
Filename: "{app}\{#VersionFolder}\{#ProtonDriveDownloaderName}"; Parameters: "{code:GetDriveInstallPath}"; Tasks: installProtonDrive; Flags: postinstall nowait runascurrentuser skipifsilent;

[Languages]
Name: "en_US"; MessagesFile: "compiler:Default.isl,Strings\Default.isl"
Name: "cs_CZ"; MessagesFile: "compiler:Languages\Czech.isl,Strings\Czech.isl"
Name: "ja_JP"; MessagesFile: "compiler:Languages\Japanese.isl,Strings\Japanese.isl"
Name: "de_DE"; MessagesFile: "compiler:Languages\German.isl,Strings\German.isl"
Name: "fi_FI"; MessagesFile: "compiler:Languages\Finnish.isl,Strings\Finnish.isl"
Name: "fr_FR"; MessagesFile: "compiler:Languages\French.isl,Strings\French.isl"
Name: "nl_NL"; MessagesFile: "compiler:Languages\Dutch.isl,Strings\Dutch.isl"
Name: "it_IT"; MessagesFile: "compiler:Languages\Italian.isl,Strings\Italian.isl"
Name: "pl_PL"; MessagesFile: "compiler:Languages\Polish.isl,Strings\Polish.isl"
Name: "pt_BR"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl,Strings\BrazilianPortuguese.isl"
Name: "pt_PT"; MessagesFile: "compiler:Languages\Portuguese.isl,Strings\Portuguese.isl"
Name: "ru_RU"; MessagesFile: "compiler:Languages\Russian.isl,Strings\Russian.isl"
Name: "es_ES"; MessagesFile: "compiler:Languages\Spanish.isl,Strings\Spanish.isl"
Name: "uk_UA"; MessagesFile: "compiler:Languages\Ukrainian.isl,Strings\Ukrainian.isl"
Name: "tr_TR"; MessagesFile: "compiler:Languages\Turkish.isl,Strings\Turkish.isl"

[UninstallDelete]
Type: filesandordirs; Name: "{commonappdata}\ProtonVPN"
Type: filesandordirs; Name: "{localappdata}\ProtonVPN"
Type: filesandordirs; Name: "{app}\{#VersionFolder}\Resources"

[Dirs]
Name: "{localappdata}\ProtonVPN\DiagnosticLogs"
Name: "{commonappdata}\ProtonVPN\Updates"; AfterInstall: SetFolderPermissions;

[Code]
function InitLogger(logger: Longword): Integer;
external 'InitLogger@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function InitLoggerUninstall(logger: Longword): Integer;
external 'InitLogger@{%TEMP}\ProtonVPN.InstallActions.x86.dll cdecl delayload uninstallonly';

function UpdateTaskbarIconTarget(launcherPath: String): Integer;
external 'UpdateTaskbarIconTarget@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function UninstallProduct(upgradeCode: String): Integer;
external 'UninstallProduct@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function IsProductInstalled(upgradeCode: String): Integer;
external 'IsProductInstalled@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function SetUpdatesFolderPermission(updatesFolderPath: String): Integer;
external 'SetUpdatesFolderPermission@files:ProtonVPN.InstallActions.x86.dll cdecl delayload';

function UninstallTapAdapter(tapFilesPath: String): Integer;
external 'UninstallTapAdapter@ProtonVPN.InstallActions.x86.dll cdecl delayload uninstallonly';

function RemovePinnedIcons(shortcutPath: String): Integer;
external 'RemovePinnedIcons@ProtonVPN.InstallActions.x86.dll cdecl delayload uninstallonly';

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
  IsToReboot, IsVerySilent, IsToDisableAutoUpdate, IsInstallPathModified: Boolean;

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

procedure SetFolderPermissions();
begin
  SetUpdatesFolderPermission(ExpandConstant('{commonappdata}\ProtonVPN\Updates'));
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
  Processes := ['ProtonVPN.exe', 'ProtonVPNService.exe', 'ProtonVPN.WireGuardService.exe'];
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

procedure SetIsToDisableAutoUpdate();
begin
  IsToDisableAutoUpdate := Pos('/DisableAutoUpdate', GetCmdTail()) > 0;
  if IsToDisableAutoUpdate = true then begin
    Log('The app will be launched with auto updates disabled.');
  end;
end;

function InitializeSetup(): Boolean;
var
  Version: String;
  ErrCode: Integer;
  WindowsVersion: TWindowsVersion;
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
  Dependency_AddWebView2;
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

function IsUpgrade: Boolean;
begin
  Result := FileExists(ExpandConstant('{app}\{#LauncherExeName}'));
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  vpnFolderPath: String;
begin
    vpnFolderPath := '{app}';
    if IsUpgrade = False then
      vpnFolderPath := vpnFolderPath + '\{#AppFolder}';

    DeleteNonRunningVersions(ExpandConstant(vpnFolderPath));
    Log('Trying to save user settings for the old ProtonVPN app if it is installed');
    SaveOldUserConfigFolder();
    Log('Trying to update taskbar icon path if exists');
    UpdateTaskbarIconTarget(ExpandConstant(vpnFolderPath + '\{#VersionFolder}\{#MyAppExeName}'));
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
      if IsToDisableAutoUpdate = true then begin
        launcherArgs := launcherArgs + ' /DisableAutoUpdate';
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
    RemovePinnedIcons(ExpandConstant('{commondesktop}\Proton VPN.lnk'));
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

function ShouldDisplayProtonDriveCheckbox: Boolean;
begin
  Result := IsProductInstalled('{F3B95BD2-1311-4B82-8B4A-B9EB7C0500ED}') = 0;
end;

function GetDriveInstallPath(value: String): String;
var
  path: String;
begin
    path := WizardForm.DirEdit.Text;
    StringChangeEx(path, ExpandConstant('\{#AppFolder}'), '\Proton\Drive', True);
    Result := '"' + path + '"';
end;

procedure CurPageChanged(CurPageID: Integer);
begin
    if (CurPageID = wpPreparing) and (IsInstallPathModified = False) and (IsUpgrade = False) then begin
      IsInstallPathModified := true;
      WizardForm.DirEdit.Text := WizardForm.DirEdit.Text + ExpandConstant('\{#AppFolder}');
    end;
end;

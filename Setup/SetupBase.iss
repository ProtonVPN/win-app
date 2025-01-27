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

#define ProtonInstallerName "ProtonInstaller.exe"
#define Webview2InstallerName "MicrosoftEdgeWebview2Setup.exe"
#define InstallLogPath "{app}\Install.log.txt"
#define ClearAppDataClientArg "-DoUninstallActions"

#define ProtonDriveUpgradeCode "{F3B95BD2-1311-4B82-8B4A-B9EB7C0500ED}"

#define Hash ""
#define VersionFolder "v" + MyAppVersion
#define AppFolder "Proton\VPN"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\{#AppFolder}
DefaultGroupName=Proton
DisableDirPage=yes
AlwaysShowDirOnReadyPage=yes
DisableProgramGroupPage=auto
AppPublisher={#MyPublisher}
UninstallDisplayIcon={app}\{#LauncherExeName}
UninstallDisplayName={#MyAppName}
OutputBaseFilename=ProtonVPN_{#VersionFolder}_{#Architecture}{#OutputBaseSuffix}
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
OutputDir=Installers
SetupIconFile=Images\protonvpn.ico
SetupLogging=yes
DisableFinishedPage=yes
DisableStartupPrompt=yes
DisableReadyPage=yes
DirExistsWarning=no
VersionInfoProductTextVersion={#MyAppVersion}-{#hash}
VersionInfoVersion={#MyAppVersion}
AppCopyright=© 2022 {#MyPublisher}

SignTool=signtool sign /a /tr http://timestamp.sectigo.com /td SHA256 /fd SHA256 $f

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
Source: "..\{#SourcePath}\Resources\ProtonVPN.InstallActions.dll"; DestDir: "{app}\{#VersionFolder}"; Flags: signonce;

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
Source: "..\{#SourcePath}\sk-SK\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\sk-SK"; Flags: signonce;
Source: "..\{#SourcePath}\nn-NO\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\nn-NO"; Flags: signonce;
Source: "..\{#SourcePath}\nb-NO\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\nb-NO"; Flags: signonce;
Source: "..\{#SourcePath}\sl-SI\ProtonVPN.Translations.resources.dll"; DestDir: "{app}\{#VersionFolder}\sl-SI"; Flags: signonce;

Source: "..\{#SourcePath}\Resources\ProtonVPN.InstallActions.x86.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\LocalAgent.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\ProtonVPN.IPFilter.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\ProtonVPN.NetworkUtil.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;
Source: "..\{#SourcePath}\Resources\GoSrp.dll"; DestDir: "{app}\{#VersionFolder}\Resources"; Flags: signonce;

Source: "GuestHoleServers.json"; DestDir: "{app}\{#VersionFolder}\Resources";
Source: "Dependencies\{#Webview2InstallerName}"; Flags: dontcopy;

Source: "Images\Proton*.bmp"; Flags: dontcopy;

[Icons]
Name: "{group}\Proton VPN"; Filename: "{app}\{#LauncherExeName}"
Name: "{commondesktop}\Proton VPN"; Filename: "{app}\{#LauncherExeName}"; Tasks: desktopicon; AppUserModelID: "{#AppUserModelID}";

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopShortcuts}"; 

[Languages]
Name: "en_US"; MessagesFile: "compiler:Default.isl,Strings\Default.isl"
Name: "cs_CZ"; MessagesFile: "compiler:Languages\Czech.isl,Strings\Czech.isl"
Name: "ja_JP"; MessagesFile: "compiler:Languages\Japanese.isl,Strings\Japanese.isl"
Name: "de_DE"; MessagesFile: "compiler:Languages\German.isl,Strings\German.isl"
Name: "fi_FI"; MessagesFile: "compiler:Languages\Finnish.isl,Strings\Finnish.isl"
Name: "fr_FR"; MessagesFile: "compiler:Languages\French.isl,Strings\French.isl"
Name: "nl_NL"; MessagesFile: "compiler:Languages\Dutch.isl,Strings\Dutch.isl"
Name: "it_IT"; MessagesFile: "compiler:Languages\Italian.isl,Strings\Italian.isl"
Name: "nb_NO"; MessagesFile: "compiler:Languages\Norwegian.isl,Strings\Norwegian.isl"
Name: "pl_PL"; MessagesFile: "compiler:Languages\Polish.isl,Strings\Polish.isl"
Name: "pt_BR"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl,Strings\BrazilianPortuguese.isl"
Name: "pt_PT"; MessagesFile: "compiler:Languages\Portuguese.isl,Strings\Portuguese.isl"
Name: "ru_RU"; MessagesFile: "compiler:Languages\Russian.isl,Strings\Russian.isl"
Name: "es_ES"; MessagesFile: "compiler:Languages\Spanish.isl,Strings\Spanish.isl"
Name: "uk_UA"; MessagesFile: "compiler:Languages\Ukrainian.isl,Strings\Ukrainian.isl"
Name: "tr_TR"; MessagesFile: "compiler:Languages\Turkish.isl,Strings\Turkish.isl"
Name: "sl_SI"; MessagesFile: "compiler:Languages\Slovenian.isl,Strings\Slovenian.isl"

[UninstallDelete]
Type: filesandordirs; Name: "{app}\{#VersionFolder}\ServiceData"
Type: filesandordirs; Name: "{app}\{#VersionFolder}\Resources"
Type: files; Name: "{#InstallLogPath}"

[Code]
function lstrlenW(lpString: Cardinal): Cardinal;
external 'lstrlenW@kernel32.dll stdcall';

function lstrcpyW(lpStringDest: String; lpStringSrc: Cardinal): Integer;
external 'lstrcpyW@kernel32.dll stdcall';

function InitLogger(logger: Longword): Integer;
external 'InitLogger@files:ProtonVPN.InstallActions.x86.dll cdecl';

function UpdateTaskbarIconTarget(launcherPath: String): Integer;
external 'UpdateTaskbarIconTarget@files:ProtonVPN.InstallActions.x86.dll cdecl';

function UninstallProduct(upgradeCode: String): Integer;
external 'UninstallProduct@files:ProtonVPN.InstallActions.x86.dll cdecl';

function IsProductInstalled(upgradeCode: String): Integer;
external 'IsProductInstalled@files:ProtonVPN.InstallActions.x86.dll cdecl';

function SaveOldUserConfigFolder(): Integer;
external 'SaveOldUserConfigFolder@files:ProtonVPN.InstallActions.x86.dll cdecl';

function RestoreOldUserConfigFolder(applicationPath: String): Integer;
external 'RestoreOldUserConfigFolder@files:ProtonVPN.InstallActions.x86.dll cdecl';

function InstallWindowsService(name, displayName, path: String): Integer;
external 'InstallService@files:ProtonVPN.InstallActions.x86.dll cdecl';

function IsProcessRunning(processName: String): Boolean;
external 'IsProcessRunning@files:ProtonVPN.InstallActions.x86.dll cdecl';

function IsProcessRunningByPath(processPath: String): Boolean;
external 'IsProcessRunningByPath@files:ProtonVPN.InstallActions.x86.dll cdecl';

function InstallCalloutDriver(name, displayName, path: String): Integer;
external 'InstallCalloutDriver@files:ProtonVPN.InstallActions.x86.dll cdecl';

function LaunchUnelevatedProcess(processPath, args: String; isToWait: Boolean): Integer;
external 'LaunchUnelevatedProcess@files:ProtonVPN.InstallActions.x86.dll cdecl';

function UninstallService(name: String): Integer;
external 'UninstallService@{app}\{#VersionFolder}\Resources\ProtonVPN.InstallActions.x86.dll cdecl uninstallonly';

function InitLoggerUninstall(logger: Longword): Integer;
external 'InitLogger@{app}\{#VersionFolder}\Resources\ProtonVPN.InstallActions.x86.dll cdecl uninstallonly';

function UninstallTapAdapter(tapFilesPath: String): Integer;
external 'UninstallTapAdapter@{app}\{#VersionFolder}\Resources\ProtonVPN.InstallActions.x86.dll cdecl uninstallonly';

function RemovePinnedIcons(shortcutPath: String): Integer;
external 'RemovePinnedIcons@{app}\{#VersionFolder}\Resources\ProtonVPN.InstallActions.x86.dll cdecl uninstallonly';

function RemoveWfpObjects(): Integer;
external 'RemoveWfpObjects@{app}\{#VersionFolder}\Resources\ProtonVPN.InstallActions.x86.dll cdecl uninstallonly';

function LaunchUnelevatedProcessOnUninstall(processPath, args: String; isToWait: Boolean): Integer;
external 'LaunchUnelevatedProcess@{app}\{#VersionFolder}\Resources\ProtonVPN.InstallActions.x86.dll cdecl uninstallonly';

type
  TInt64Array = array of Int64;

var
  IsToReboot, IsSilent, IsVerySilent, IsNotSilent, IsToDisableAutoUpdate: Boolean;
  InstallationProgressLabel: TNewStaticText;
  ProductDriveCheckBox, ProductMailCheckBox, ProductPassCheckBox: TNewCheckBox;

const
  ProductLogoWidth = 140;
  ProductLogoHeight = 36;
  PanelWidth = 500;
  PanelHeight = 50;
  Padding = 10;
  PanelSpacing = 10;

procedure OnProductDriveClick(Sender: TObject);
begin
  ProductDriveCheckBox.Checked := not ProductDriveCheckBox.Checked;
end;

procedure OnProductMailClick(Sender: TObject);
begin
  ProductMailCheckBox.Checked := not ProductMailCheckBox.Checked;
end;

procedure OnProductPassClick(Sender: TObject);
begin
  ProductPassCheckBox.Checked := not ProductPassCheckBox.Checked;
end;

procedure InitializeWizard;
var
  // Proton product vars
  HeaderLabel, SubHeaderLabel: TLabel;
  ProductDriveLabelA, ProductMailLabelA, ProductPassLabelA: TLabel;
  ProductDriveLabelB, ProductMailLabelB, ProductPassLabelB: TLabel;
  ProductDriveImage, ProductMailImage, ProductPassImage: TBitmapImage;
  ProductDrivePanel, ProductMailPanel, ProductPassPanel: TPanel;
  ProductDrivePanelOverlay, ProductMailPanelOverlay, ProductPassPanelOverlay: TLabel;
  IsProductDriveInstalled, IsProductMailInstalled, IsProductPassInstalled, IsArm64: Boolean;
  ProductPadding: Int64;
begin
  IsArm64 := ExpandConstant('{#Architecture}') = 'arm64';
  InstallationProgressLabel := TNewStaticText.Create(WizardForm);
  InstallationProgressLabel.Parent := WizardForm.InstallingPage;
  InstallationProgressLabel.Top := ScaleY(100);
  InstallationProgressLabel.Left := 0;
  
  HeaderLabel := TLabel.Create(WizardForm.SelectTasksPage);
  HeaderLabel.Parent := WizardForm.SelectTasksPage;
  HeaderLabel.Caption := CustomMessage('InstallerTitle');
  HeaderLabel.AutoSize := True;
  HeaderLabel.WordWrap := True;
  HeaderLabel.Top := 0;
  HeaderLabel.Width := ScaleX(PanelWidth);
  HeaderLabel.Font.Size := 14;

  IsProductMailInstalled := RegValueExists(HKEY_CURRENT_USER, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\proton_mail', 'DisplayVersion');
  IsProductDriveInstalled := IsProductInstalled('{#ProtonDriveUpgradeCode}') <> 0;
  IsProductPassInstalled := RegValueExists(HKEY_CURRENT_USER, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\ProtonPass', 'DisplayVersion');

  SubHeaderLabel := TLabel.Create(WizardForm.SelectTasksPage);
  SubHeaderLabel.Parent := WizardForm.SelectTasksPage;
  SubHeaderLabel.Top := HeaderLabel.Top + HeaderLabel.Height + ScaleY(32);
  SubHeaderLabel.Caption := CustomMessage('InstallOtherApps');
  SubHeaderLabel.WordWrap := True;
  SubHeaderLabel.Width := ScaleX(PanelWidth);
  SubHeaderLabel.Font.Size := 8;
  SubHeaderLabel.Font.Style := [fsBold];

  if not IsProductMailInstalled or not IsProductDriveInstalled or not IsProductPassInstalled then begin
    ExtractTemporaryFile('ProtonMail.bmp');
    ExtractTemporaryFile('ProtonDrive.bmp');
    ExtractTemporaryFile('ProtonPass.bmp');

    ProductPadding := ScaleY(PanelSpacing);

    // Proton Mail
    ProductMailPanel := TPanel.Create(WizardForm.SelectTasksPage);
    ProductMailPanel.Parent := WizardForm.SelectTasksPage;
    ProductMailPanel.SetBounds(0, SubHeaderLabel.Top + SubHeaderLabel.Height + ProductPadding, ScaleX(PanelWidth), ScaleY(PanelHeight));
    ProductMailPanel.BevelOuter := bvNone;

    ProductMailCheckBox := TNewCheckBox.Create(ProductMailPanel);
    ProductMailCheckBox.Parent := ProductMailPanel;
    ProductMailCheckBox.Top := ScaleX(Padding);
    ProductMailCheckBox.Left := ScaleX(Padding);
    ProductMailCheckBox.Width := ScaleX(14);
    ProductMailCheckBox.Height := ScaleY(14);
    ProductMailCheckBox.Checked := not IsProductMailInstalled;

    ProductMailImage := TBitmapImage.Create(ProductMailPanel);
    ProductMailImage.Parent := ProductMailPanel;
    ProductMailImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\ProtonMail.bmp'));
    ProductMailImage.Stretch := True;
    ProductMailImage.SetBounds(ProductMailCheckBox.Left + ScaleX(21), ProductMailCheckBox.Top - ScaleY(2), ScaleX(82), ScaleY(22));

    ProductMailLabelA := TLabel.Create(ProductMailPanel);
    ProductMailLabelA.Parent := ProductMailPanel;
    ProductMailLabelA.Caption := CustomMessage('FreeTrial') + ' - ';
    ProductMailLabelA.AutoSize := True;
    ProductMailLabelA.Top := ProductMailImage.Top + ProductMailImage.Height + ScaleY(5);
    ProductMailLabelA.Left := ProductMailImage.Left;
    ProductMailLabelA.Width := ScaleX(PanelWidth - Padding);
    ProductMailLabelA.Font.Style := [fsBold];

    ProductMailLabelB := TLabel.Create(ProductMailPanel);
    ProductMailLabelB.Parent := ProductMailPanel;
    ProductMailLabelB.Caption := CustomMessage('ProtonMailDescription');
    ProductMailLabelB.AutoSize := True;
    ProductMailLabelB.Top := ProductMailLabelA.Top;
    ProductMailLabelB.Left := ProductMailLabelA.Left + ProductMailLabelA.Width;
    ProductMailLabelB.WordWrap := True;

    ProductMailPanelOverlay := TLabel.Create(ProductMailPanel);
    ProductMailPanelOverlay.Parent := ProductMailPanel;
    ProductMailPanelOverlay.Width := ScaleX(PanelWidth);
    ProductMailPanelOverlay.Height := ScaleY(PanelHeight);
    ProductMailPanelOverlay.Transparent := True;
    ProductMailPanelOverlay.OnClick := @OnProductMailClick;

    if IsProductMailInstalled then begin
      ProductMailPanel.Visible := False;
      ProductMailPanel.Height := 0;
      ProductPadding := 0;
    end;

    // Proton Drive
    ProductDrivePanel := TPanel.Create(WizardForm.SelectTasksPage);
    ProductDrivePanel.Parent := WizardForm.SelectTasksPage;
    ProductDrivePanel.SetBounds(0, ProductMailPanel.Top + ProductMailPanel.Height + ProductPadding, ScaleX(PanelWidth), ScaleY(PanelHeight));
    ProductDrivePanel.BevelOuter := bvNone;

    ProductDriveCheckBox := TNewCheckBox.Create(ProductDrivePanel);
    ProductDriveCheckBox.Parent := ProductDrivePanel;
    ProductDriveCheckBox.Top := ScaleX(Padding);
    ProductDriveCheckBox.Left := ScaleX(Padding);
    ProductDriveCheckBox.Width := ScaleX(14);
    ProductDriveCheckBox.Height := ScaleY(14);
    ProductDriveCheckBox.Checked := not IsProductDriveInstalled;

    ProductDriveImage := TBitmapImage.Create(ProductDrivePanel);
    ProductDriveImage.Parent := ProductDrivePanel;
    ProductDriveImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\ProtonDrive.bmp'));
    ProductDriveImage.Stretch := True;
    ProductDriveImage.SetBounds(ProductDriveCheckBox.Left + ScaleX(21), ProductDriveCheckBox.Top - ScaleY(2), ScaleX(86), ScaleY(22));

    ProductDriveLabelA := TLabel.Create(ProductDrivePanel);
    ProductDriveLabelA.Parent := ProductDrivePanel;
    ProductDriveLabelA.Caption := CustomMessage('Free') + ' - ';
    ProductDriveLabelA.AutoSize := True;
    ProductDriveLabelA.Top := ProductDriveImage.Top + ProductDriveImage.Height + ScaleY(5);
    ProductDriveLabelA.Left := ProductDriveImage.Left;
    ProductDriveLabelA.Width := ScaleX(PanelWidth - Padding);
    ProductDriveLabelA.Font.Style := [fsBold];

    ProductDriveLabelB := TLabel.Create(ProductDrivePanel);
    ProductDriveLabelB.Parent := ProductDrivePanel;
    ProductDriveLabelB.Caption := CustomMessage('ProtonDriveDescription');
    ProductDriveLabelB.AutoSize := True;
    ProductDriveLabelB.Top := ProductDriveLabelA.Top;
    ProductDriveLabelB.Left := ProductDriveLabelA.Left + ProductDriveLabelA.Width;
    ProductDriveLabelB.WordWrap := True;

    ProductDrivePanelOverlay := TLabel.Create(ProductDrivePanel);
    ProductDrivePanelOverlay.Parent := ProductDrivePanel;
    ProductDrivePanelOverlay.Width := ScaleX(PanelWidth);
    ProductDrivePanelOverlay.Height := ScaleY(PanelHeight);
    ProductDrivePanelOverlay.Transparent := True;
    ProductDrivePanelOverlay.OnClick := @OnProductDriveClick;

    if IsProductDriveInstalled then begin
      ProductDrivePanel.Visible := False;
      ProductDrivePanel.Height := 0;
      ProductPadding := 0;
    end else
      ProductPadding := ScaleX(PanelSpacing);

    // Proton Pass
    ProductPassPanel := TPanel.Create(WizardForm.SelectTasksPage);
    ProductPassPanel.Parent := WizardForm.SelectTasksPage;
    ProductPassPanel.SetBounds(0, ProductDrivePanel.Top + ProductDrivePanel.Height + ProductPadding, ScaleX(PanelWidth), ScaleY(PanelHeight));
    ProductPassPanel.BevelOuter := bvNone;

    ProductPassCheckBox := TNewCheckBox.Create(ProductPassPanel);
    ProductPassCheckBox.Parent := ProductPassPanel;
    ProductPassCheckBox.Top := ScaleX(Padding);
    ProductPassCheckBox.Left := ScaleX(Padding);
    ProductPassCheckBox.Width := ScaleX(14);
    ProductPassCheckBox.Height := ScaleY(14);
    ProductPassCheckBox.Checked := not IsProductPassInstalled;

    ProductPassImage := TBitmapImage.Create(ProductPassPanel);
    ProductPassImage.Parent := ProductPassPanel;
    ProductPassImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\ProtonPass.bmp'));
    ProductPassImage.Stretch := True;
    ProductPassImage.SetBounds(ProductPassCheckBox.Left + ScaleX(21), ProductPassCheckBox.Top - ScaleY(2), ScaleX(84), ScaleY(22));

    ProductPassLabelA := TLabel.Create(ProductPassPanel);
    ProductPassLabelA.Parent := ProductPassPanel;
    ProductPassLabelA.Caption := CustomMessage('Free') + ' - ';
    ProductPassLabelA.AutoSize := True;
    ProductPassLabelA.Top := ProductPassImage.Top + ProductPassImage.Height + ScaleY(5);
    ProductPassLabelA.Left := ProductPassImage.Left;
    ProductPassLabelA.Width := ScaleX(PanelWidth - Padding);
    ProductPassLabelA.Font.Style := [fsBold];

    ProductPassLabelB := TLabel.Create(ProductPassPanel);
    ProductPassLabelB.Parent := ProductPassPanel;
    ProductPassLabelB.Caption := CustomMessage('ProtonPassDescription');
    ProductPassLabelB.AutoSize := True;
    ProductPassLabelB.Top := ProductPassLabelA.Top;
    ProductPassLabelB.Left := ProductPassLabelA.Left + ProductPassLabelA.Width;
    ProductPassLabelB.WordWrap := True;

    ProductPassPanelOverlay := TLabel.Create(ProductPassPanel);
    ProductPassPanelOverlay.Parent := ProductPassPanel;
    ProductPassPanelOverlay.Width := ScaleX(PanelWidth);
    ProductPassPanelOverlay.Height := ScaleY(PanelHeight);
    ProductPassPanelOverlay.Transparent := True;
    ProductPassPanelOverlay.OnClick := @OnProductPassClick;

    if IsProductPassInstalled then begin
      ProductPassPanel.Visible := False;
      ProductPassPanel.Height := 0;
    end;

    WizardForm.TasksList.Top := ProductPassPanel.Top + ProductPassPanel.Height + ScaleY(16);
    WizardForm.TasksList.Left := ProductPassCheckBox.Left - ScaleX(4);
  end;

  // Hide top window section
  WizardForm.SelectTasksLabel.Visible := False;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpSelectTasks then begin
    WizardForm.MainPanel.Visible := False;
    WizardForm.InnerNotebook.Top := ScaleY(40);
    WizardForm.NextButton.Caption := SetupMessage(msgButtonInstall);
  end else begin
    WizardForm.MainPanel.Visible := True;
    WizardForm.InnerNotebook.Top := ScaleY(72);
    WizardForm.NextButton.Caption := SetupMessage(msgButtonNext);
  end;
end;

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

procedure SetSilentModes();
var
  i: Integer;
begin
  IsVerySilent := False;
  IsSilent := False;
  for i := 1 to ParamCount do
    if CompareText(ParamStr(i), '/verysilent') = 0 then
      IsVerySilent := True
    else if CompareText(ParamStr(i), '/silent') = 0 then
      IsSilent := True;
  IsNotSilent := (IsVerySilent = false) and (IsSilent = false);
end;

procedure SetIsToDisableAutoUpdate();
begin
  IsToDisableAutoUpdate := Pos('/DisableAutoUpdate', GetCmdTail()) > 0;
  if IsToDisableAutoUpdate = true then begin
    Log('The app will be launched with auto updates disabled.');
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

function InitializeSetup(): Boolean;
var
  Version: String;
  ErrCode: Integer;
begin
  SetSilentModes();
  SetIsToDisableAutoUpdate();
  if IsWindowsVersionEqualOrHigher(10, 0, 19041) = False then begin
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
  InitLoggerUninstall(CreateCallback(@LogProc));
  Result := True;
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
    Result := '';
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  logfilepathname, logfilename, newfilepathname, langCode, launcherArgs, productArguments: String;
  res: Integer;
begin
  productArguments := '';
  launcherArgs := '';
  if IsNotSilent then begin
    if Assigned(ProductMailCheckBox) and ProductMailCheckBox.Checked then begin
      productArguments := productArguments + ' /Mail';
      launcherArgs := launcherArgs + ' /MailInstalled';
    end;
    if Assigned(ProductDriveCheckBox) and ProductDriveCheckBox.Checked then begin
      productArguments := productArguments + ' /Drive';
      launcherArgs := launcherArgs + ' /DriveInstalled';
    end;
    if Assigned(ProductPassCheckBox) and ProductPassCheckBox.Checked then begin
      productArguments := productArguments + ' /Pass';
      launcherArgs := launcherArgs + ' /PassInstalled';
    end;
  end;

  if CurStep = ssDone then begin
    logfilepathname := ExpandConstant('{log}');
    logfilename := ExtractFileName(logfilepathname);
    newfilepathname := ExpandConstant('{#InstallLogPath}');
    FileCopy(logfilepathname, newfilepathname, false);
    if IsProcessRunning('{#MyAppExeName}') then begin
      exit;
    end;
    RestoreOldUserConfigFolder(ExpandConstant('{app}'));
    if (not IsToReboot or WizardForm.NoRadio.Checked = True) and IsVerySilent = false then begin
      if WizardSilent() = false then begin
        langCode := ActiveLanguage();
        StringChangeEx(langCode, '_', '-', True);
        launcherArgs := launcherArgs + ' /lang ' + langCode;
      end;
      if IsToDisableAutoUpdate = true then begin
        launcherArgs := launcherArgs + ' /DisableAutoUpdate';
      end;

      if IsNotSilent then
        launcherArgs := launcherArgs + ' /CleanInstall';

      ExecAsOriginalUser(ExpandConstant('{app}\{#LauncherExeName}'), Trim(launcherArgs), '', SW_SHOW, ewNoWait, res);
    end;
  end
  else if CurStep = ssPostInstall then begin
    if IsNotSilent then begin
      if not RegValueExists(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv') then begin
        InstallationProgressLabel.Caption := CustomMessage('InstallingWebview2Runtime');
        WizardForm.Refresh();
        ExtractTemporaryFile('{#Webview2InstallerName}');
        LaunchUnelevatedProcess(ExpandConstant('{tmp}\{#Webview2InstallerName}'), '/silent /install', True);
      end;
      if productArguments <> '' then
        if WizardIsTaskSelected('desktopicon') then
          productArguments := productArguments + ' /CreateDesktopShortcut';
        LaunchUnelevatedProcess(ExpandConstant('{app}\{#VersionFolder}\{#ProtonInstallerName}'), Trim(productArguments), False);
    end;
  end;
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
    LaunchUnelevatedProcessOnUninstall(ExpandConstant('{app}\{#VersionFolder}\{#MyAppExeName}'), '{#ClearAppDataClientArg}', True);
    UnloadDLL(ExpandConstant('{app}\{#VersionFolder}\Resources\ProtonVPN.InstallActions.x86.dll'));
  end;
end;
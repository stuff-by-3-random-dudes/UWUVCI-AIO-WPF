; Inno Setup Script for UWUVCI AIO
; Version 3.Z-B
; Supports Windows and Wine

[Setup]
AppName=UWUVCI AIO
AppId=UWUVCI AIO
AppVersion=3.Z-B
DefaultDirName={userprofile}\UWUVCI_AIO
UninstallDisplayIcon={app}\UWUVCI AIO.exe
OutputBaseFilename=UWUVCI_INSTALLER
Compression=lzma2
PrivilegesRequired=lowest
DisableDirPage=no
DisableProgramGroupPage=yes
DefaultGroupName=UWUVCI AIO

[Files]
Source: "app\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs;
Source: "dotnetfx481.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall;

[Run]
Filename: "{tmp}\dotnetfx481.exe"; Parameters: "/quiet /norestart"; StatusMsg: "Installing .NET Framework 4.8.1..."; Check: not IsRunningUnderWine and NeedsDotNet481; Flags: runhidden;
Filename: "{app}\Readme.txt"; Flags: shellexec postinstall nowait;
Filename: "{app}\UWUVCI AIO.exe"; Description: "Launch UWUVCI AIO"; Flags: postinstall nowait unchecked;

[Icons]
Name: "{group}\UWUVCI AIO"; Filename: "{app}\UWUVCI AIO.exe";
Name: "{group}\Uninstall UWUVCI AIO"; Filename: "{uninstallexe}";
Name: "{autodesktop}\UWUVCI AIO"; Filename: "{app}\UWUVCI AIO.exe"; Tasks: desktopicon;
Name: "{autodesktop}\UWUVCI AIO Debug Mode"; Filename: "{app}\UWUVCI DEBUG MODE.bat"; Tasks: desktopicon;
Name: "{autodesktop}\UWUVCI AIO ReadMe"; Filename: "{app}\Readme.txt"; Tasks: desktopicon;

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[UninstallDelete]
Type: filesandordirs; Name: "{app}"; Check: ConfirmUninstall;

[CustomMessages]
default.NameAndVersion=%1 version %2
default.AdditionalIcons=Additional shortcuts:
default.CreateDesktopIcon=Create a &desktop shortcut
default.UninstallProgram=Uninstall %1
default.LaunchProgram=Launch %1

[Code]
var
  WelcomePage: TWizardPage;

function IsDotNetInstalled(version: string): Boolean;
var
  Success: Boolean;
  Installed: Cardinal;
begin
  Success := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Installed);
  Result := Success and (Installed >= 528372); // .NET 4.8.1 release key is 528372
end;

function NeedsDotNet481: Boolean;
begin
  Result := not IsDotNetInstalled('4.8.1');
end;

function IsRunningUnderWine: Boolean;
var
  WineCheck: String;
begin
  Result := RegQueryStringValue(HKEY_LOCAL_MACHINE, 'Software\Wine', '', WineCheck);
  if not Result then
    Result := GetEnv('WINELOADER') <> '';
end;

function IsAlreadyInstalled: Boolean;
var
  InstallPath: string;
begin
  InstallPath := ExpandConstant('{userappdata}\UWUVCI_AIO');  // Use a safe fallback path
  Result := DirExists(InstallPath);  // Check if the folder exists
end;


function ConfirmUninstall: Boolean;
begin
  Result := MsgBox('Are you sure you want to uninstall UWUVCI AIO?', mbConfirmation, MB_YESNO) = IDYES;
end;

function GetInstallDir(Default: string): string;
var
  HomePath: string;
begin
  if IsRunningUnderWine then
  begin
    // Get $HOME for Unix-like systems
    if GetEnv('HOME') <> '' then
      HomePath := GetEnv('HOME')
    else
      HomePath := ExpandConstant('{userdocs}'); // Fallback for Wine users

    Result := HomePath + '/.UWUVCI_AIO';
  end
  else
  begin
    // Get USERPROFILE for Windows
    if GetEnv('USERPROFILE') <> '' then
      HomePath := GetEnv('USERPROFILE')
    else
      HomePath := ExpandConstant('{userdocs}'); // Fallback for Windows

    Result := HomePath + '\UWUVCI_AIO';
  end;
end;

function IsOneDrivePath(Path: string): Boolean;
begin
  Result := (Pos('OneDrive', Path) > 0);
end;

procedure AddWelcomePage;
begin
  WelcomePage := CreateCustomPage(wpWelcome, 'Welcome to UWUVCI AIO', 'Thank you for using UWUVCI AIO!');

  with TNewStaticText.Create(WizardForm) do
  begin
    Parent := WelcomePage.Surface;
    Left := ScaleX(10);
    Top := ScaleY(10);
    Width := WelcomePage.SurfaceWidth - ScaleX(40);
    Caption := 'This installer will guide you through setting up UWUVCI AIO.' + #13#10 +
               'Before you proceed, please ensure that:' + #13#10 +
               '- You have at least 15GB of free disk space.' + #13#10 +
               '- You are not installing in a OneDrive folder.' + #13#10 +
               '- If using Wine, you have run `winetricks dotnet48`.' + #13#10#13#10 +
               'Click Next to continue!';
    AutoSize := True;
  end;
end;

procedure InitializeWizard;
var
  InstallPath: string;
begin
  InstallPath := GetInstallDir('');

  if IsOneDrivePath(InstallPath) then
  begin
    MsgBox('UWUVCI AIO cannot be installed in a OneDrive folder due to compatibility issues.' + #13#10 +
           'Please choose a different location.', mbError, MB_OK);
    Abort;
  end;

  AddWelcomePage;

  if IsAlreadyInstalled then
  begin
    MsgBox('A previous version of UWUVCI AIO is already installed.' + #13#10 +
           'Installing over it may overwrite existing files.', mbInformation, MB_OK);
  end;
end;

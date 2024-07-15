;InnoSetupVersion=6.0.0 (Unicode)

[Setup]
AppName=UWUVCI AIO
AppId=UWUVCI AIO
AppVersion=3.Z-Again
DefaultDirName={userdocs}\UWUVCI AIO
UninstallDisplayIcon={app}\UWUVCI AIO.exe
OutputBaseFilename=UWUVCI_INSTALLER
Compression=lzma2
PrivilegesRequired=lowest
DisableDirPage=no
DisableProgramGroupPage=yes


[Files]
Source: "{app}\UWUVCI AIO.exe"; DestDir: "{app}"; MinVersion: 0.0,6.0; 
Source: "{app}\UWUVCI DEBUG MODE.bat"; DestDir: "{app}"; MinVersion: 0.0,6.0; 
Source: "{app}\GameBaseClassLibrary.dll"; DestDir: "{app}"; MinVersion: 0.0,6.0; 
Source: "{app}\Readme.txt"; DestDir: "{app}"; MinVersion: 0.0,6.0; 
Source: "{app}\UWUVCI VWII.exe"; DestDir: "{app}"; MinVersion: 0.0,6.0; 
Source: "{app}\bin\vwii\Tools\ASH.exe"; DestDir: "{app}\bin\vwii\Tools"; MinVersion: 0.0,6.0; Flags: ignoreversion 
Source: "{app}\bin\vwii\Tools\ICSharpCode.SharpZipLib.dll"; DestDir: "{app}\bin\vwii\Tools"; MinVersion: 0.0,6.0; Flags: ignoreversion 
Source: "{app}\bin\vwii\Tools\ThemeMii.exe"; DestDir: "{app}\bin\vwii\Tools"; MinVersion: 0.0,6.0; Flags: ignoreversion 

[Run]
Filename: "{app}\Readme.txt"; MinVersion: 0.0,6.0; Flags: shellexec skipifdoesntexist postinstall skipifsilent nowait
Filename: "{app}\UWUVCI AIO.exe"; Description: "{cm:LaunchProgram,UWUVCI AIO v3.Z Again}"; MinVersion: 0.0,6.0; Flags: postinstall skipifsilent nowait

[Icons]
Name: "{autoprograms}\UWUVCI AIO"; Filename: "{app}\UWUVCI AIO.exe"; MinVersion: 0.0,6.0; 
Name: "{autodesktop}\UWUVCI AIO"; Filename: "{app}\UWUVCI AIO.exe"; Tasks: desktopicon; MinVersion: 0.0,6.0; 

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; MinVersion: 0.0,6.0; 

[InstallDelete]
Type: filesandordirs; Name: "{app}\UWUVCI AIO.exe"; 
Type: filesandordirs; Name: "{app}\bin\bases"; 
Type: filesandordirs; Name: "{app}\bin\Tools"; 

[UninstallDelete]
Type: filesandordirs; Name: "{app}\bin"; 
Type: filesandordirs; Name: "{app}\InjectedGames"; 
Type: filesandordirs; Name: "{app}\SourceFiles"; 
Type: filesandordirs; Name: "{app}\configs"; 
Type: filesandordirs; Name: "{localappdata}\UWUVCI_AIO_WPF"; 

[CustomMessages]
default.NameAndVersion=%1 version %2
default.AdditionalIcons=Additional shortcuts:
default.CreateDesktopIcon=Create a &desktop shortcut
default.CreateQuickLaunchIcon=Create a &Quick Launch shortcut
default.ProgramOnTheWeb=%1 on the Web
default.UninstallProgram=Uninstall %1
default.LaunchProgram=Launch %1
default.AssocFileExtension=&Associate %1 with the %2 file extension
default.AssocingFileExtension=Associating %1 with the %2 file extension...
default.AutoStartProgramGroupDescription=Startup:
default.AutoStartProgram=Automatically start %1
default.AddonHostProgramNotFound=%1 could not be located in the folder you selected.%n%nDo you want to continue anyway?

[Languages]
; These files are stubs
; To achieve better results after recompilation, use the real language files


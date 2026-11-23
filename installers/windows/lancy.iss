; Lancy - Windows Installer (Inno Setup)
;
; Prerequisites before running this:
;   1. dotnet publish ..\..\src\Lancy.Avalonia\Lancy.Avalonia.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
;   2. Have Inno Setup 6+ installed (https://jrsoftware.org/isinfo.php)
;
; Build with: iscc lancy.iss
; Output: .\Output\Lancy-Setup-{version}.exe

#define MyAppName "Lancy"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "Roderic Villena"
#define MyAppExeName "Lancy.exe"

[Setup]
AppId={{697F31DC-A55D-45E6-B305-539331D44136}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
VersionInfoCopyright=Copyright (C) 2024 {#MyAppPublisher}
DisableProgramGroupPage=yes
; Icon file: generated from the app-icon-source.png design asset via
; installers/../src/Lancy.Avalonia/Assets/app.ico (multi-resolution ICO).
SetupIconFile=..\..\src\Lancy.Avalonia\Assets\app.ico
OutputDir=Output
OutputBaseFilename=Lancy-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
CloseApplications=force
RestartApplications=no
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Assumes the self-contained single-file publish output described above
; lands in .\publish relative to this .iss file.
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; AppUserModelID: "RodericVillena.Lancy"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; AppUserModelID: "RodericVillena.Lancy"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\Lancy"

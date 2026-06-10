; =====================================================================
; Inno Setup Script for BackupCR Agent
; =====================================================================

[Setup]
AppId={{C89A7E13-1498-4B52-AF09-DFB8A6E3B95B}
AppName=BackupCR
AppVersion=1.0
AppPublisher=BackupCR Corp
DefaultDirName={autopf}\BackupCR
DefaultGroupName=BackupCR
OutputDir=publish_installer
OutputBaseFilename=BackupCR_Setup
SetupIconFile=icons\icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin

[Languages]
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startup"; Description: "Iniciar o BackupCR automaticamente com o Windows"; GroupDescription: "Inicialização:"

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\BackupCR.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "database.json.example"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\BackupCR"; Filename: "{app}\BackupCR.exe"
Name: "{autodesktop}\BackupCR"; Filename: "{app}\BackupCR.exe"; Tasks: desktopicon
Name: "{userstartup}\BackupCR"; Filename: "{app}\BackupCR.exe"; Parameters: "/minimized"; Tasks: startup

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "BackupCR"; ValueData: """{app}\BackupCR.exe"" /minimized"; Tasks: startup; Flags: uninsdeletevalue

[Run]
Filename: "{app}\BackupCR.exe"; Description: "{cm:LaunchProgram,BackupCR}"; Flags: nowait postinstall skipifsilent

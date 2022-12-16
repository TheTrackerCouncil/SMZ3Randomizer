; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!
#define public Dependency_NoExampleSetup
#include "CodeDependencies.iss"

#define MyAppName "SMZ3 Cas' Randomizer"
#define MyAppVersion "8.0.0-beta.1"
#define MyAppPublisher "Vivelin"
#define MyAppURL "https://github.com/Vivelin/SMZ3Randomizer"
#define MyAppExeName "Randomizer.App.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{C3CC1ADA-86E9-4C12-94DA-741538A9B36B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\SMZ3 Cas Randomizer
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
Compression=lzma
SolidCompression=yes
WizardStyle=modern
OutputBaseFilename=SMZ3CasRandomizerSetup_{#MyAppVersion}     

[Code]
function InitializeSetup: Boolean;
begin
  Dependency_AddDotNet60Desktop;
  Dependency_AddDotNet60Asp;
  Result := True;
end;

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "netcorecheck.exe"; Flags: dontcopy noencryption
Source: "netcorecheck_x64.exe"; Flags: dontcopy noencryption
Source: "..\src\Randomizer.App\Sprites\*"; DestDir: "{app}\Sprites"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\src\Randomizer.App\bin\Release\net6.0-windows\publish\win-x64\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion; Check: Is64BitInstallMode;
Source: "..\src\Randomizer.App\bin\Release\net6.0-windows\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: Is64BitInstallMode;
Source: "..\src\Randomizer.App\bin\Release\net6.0-windows\publish\win-x86\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion; Check: "not Is64BitInstallMode";
Source: "..\src\Randomizer.App\bin\Release\net6.0-windows\publish\win-x86\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: "not Is64BitInstallMode";
Source: "..\src\Randomizer.Data\maps.json"; DestDir: "{localappdata}\SMZ3CasRandomizer"; Flags: comparetimestamp
Source: "..\src\Randomizer.SMZ3.Tracking\AutoTracking\LuaScripts\*"; DestDir: "{localappdata}\SMZ3CasRandomizer\AutoTrackerScripts"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\src\Randomizer.Data\Configuration\Yaml\*"; DestDir: "{localappdata}\SMZ3CasRandomizer\Configs"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


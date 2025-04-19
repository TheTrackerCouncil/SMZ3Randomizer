; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!
#define public Dependency_NoExampleSetup
#include "CodeDependencies.iss"

#define MyAppName "SMZ3 Cas' Randomizer"
#define MyAppPublisher "Vivelin"
#define MyAppURL "https://github.com/Vivelin/SMZ3Randomizer"
#define MyAppExeName "SMZ3CasRandomizer.exe"
#define MyAppVersion GetStringFileInfo("..\src\TrackerCouncil.Smz3.UI\bin\Release\net8.0\win-x64\publish\" + MyAppExeName, "ProductVersion")
#define MyAppVersion Copy(MyAppVersion, 0, Pos('+', MyAppVersion)-1)

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
ArchitecturesInstallIn64BitMode=x64
OutputBaseFilename=SMZ3CasRandomizerSetupWin_{#MyAppVersion}     

[Code]
function InitializeSetup: Boolean;
begin
  Dependency_AddDotNet80Desktop;
  Dependency_AddDotNet80Asp;
  Result := True;
end;

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[InstallDelete]
Type: filesandordirs; Name: "{app}\Sprites"
Type: filesandordirs; Name: "{app}\TrackerSprites"

[Files]
Source: "netcorecheck.exe"; Flags: dontcopy noencryption
Source: "netcorecheck_x64.exe"; Flags: dontcopy noencryption
Source: "..\sprites\Sprites\*"; DestDir: "{localappdata}\SMZ3CasRandomizer\DefaultData\Sprites"; Excludes: "\bin\*,obj\*,*.cs,*.csproj"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\trackersprites\*"; DestDir: "{localappdata}\SMZ3CasRandomizer\DefaultData\TrackerSprites"; Excludes: "\bin\*,obj\*,*.cs,*.csproj"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\src\TrackerCouncil.Smz3.UI\bin\Release\net8.0\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion; Check: Is64BitInstallMode;
Source: "..\src\TrackerCouncil.Smz3.UI\bin\Release\net8.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: Is64BitInstallMode;
Source: "..\src\TrackerCouncil.Smz3.UI\bin\Release\net8.0\win-x86\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion; Check: "not Is64BitInstallMode";
Source: "..\src\TrackerCouncil.Smz3.UI\bin\Release\net8.0\win-x86\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: "not Is64BitInstallMode";
Source: "..\src\TrackerCouncil.Smz3.Data\maps.json"; DestDir: "{localappdata}\SMZ3CasRandomizer"; Flags: comparetimestamp
Source: "..\configs\Profiles\*"; DestDir: "{localappdata}\SMZ3CasRandomizer\DefaultData\Configs"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\configs\Schemas\*"; DestDir: "{localappdata}\SMZ3CasRandomizer\DefaultData\Schemas"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


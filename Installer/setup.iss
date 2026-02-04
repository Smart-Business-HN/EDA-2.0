; EDA 2.0 Installer Script
; Inno Setup Script
; Download Inno Setup from: https://jrsoftware.org/isdl.php

#define MyAppName "EDA 2.0"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Tu Empresa"
#define MyAppExeName "EDA.PRESENTATION.exe"
#define MyAppURL "https://example.com"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
AppId={{E8D2A1B3-4C5D-6E7F-8A9B-0C1D2E3F4A5B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=LICENSE.txt
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequired=admin
OutputDir=Output
OutputBaseFilename=EDA2_Setup_{#MyAppVersion}
SetupIconFile=..\EDA 2.0\Assets\StoreLogo.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
; Require Windows 10 or later
MinVersion=10.0.17763

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Application files (from publish folder)
Source: "..\EDA 2.0\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Use template appsettings.json with empty connection string
Source: "appsettings.template.json"; DestDir: "{app}"; DestName: "appsettings.json"; Flags: ignoreversion

; SQL Server LocalDB installer (download from Microsoft)
; Download from: https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb
Source: "SqlLocalDB.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsLocalDBInstalled

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Install LocalDB if not already installed
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\SqlLocalDB.msi"" /qn IACCEPTSQLLOCALDBLICENSETERMS=YES"; StatusMsg: "Instalando SQL Server LocalDB..."; Flags: waituntilterminated; Check: not IsLocalDBInstalled

; Launch application after install
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Check if SQL Server LocalDB is already installed
function IsLocalDBInstalled: Boolean;
var
  ResultCode: Integer;
begin
  Result := False;
  // Try to find SqlLocalDB.exe in Program Files
  if FileExists(ExpandConstant('{pf}\Microsoft SQL Server\160\Tools\Binn\SqlLocalDB.exe')) then
    Result := True
  else if FileExists(ExpandConstant('{pf}\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe')) then
    Result := True
  else if FileExists(ExpandConstant('{pf}\Microsoft SQL Server\140\Tools\Binn\SqlLocalDB.exe')) then
    Result := True
  else if FileExists(ExpandConstant('{pf}\Microsoft SQL Server\130\Tools\Binn\SqlLocalDB.exe')) then
    Result := True;

  // Also check via registry
  if not Result then
  begin
    Result := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\16.0') or
              RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\15.0') or
              RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\14.0') or
              RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\13.0');
  end;
end;

// Check .NET Desktop Runtime
function IsDotNetInstalled: Boolean;
var
  ResultCode: Integer;
begin
  // Check if .NET 10 is installed (adjust version as needed)
  Result := Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;

  // Check for .NET Desktop Runtime
  if not IsDotNetInstalled then
  begin
    if MsgBox('Se requiere .NET Desktop Runtime 10.0 o superior.' + #13#10 +
              'Desea abrir la pagina de descarga?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/10.0', '', '', SW_SHOW, ewNoWait, ResultCode);
    end;
    Result := False;
  end;
end;

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

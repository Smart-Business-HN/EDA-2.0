; =============================================================================
; Script de Inno Setup para EDA 2.0
; Incluye: SQL Server LocalDB + Windows App SDK Runtime
; =============================================================================

#define MyAppName "EDA 2.0"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Smart Business"
#define MyAppExeName "EDA.PRESENTATION.exe"

[Setup]
AppId={{AC821E7E-A3A8-4BDB-A880-49F8772C70B0}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=EDA2_Setup_v{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
SetupIconFile=app-icon.ico
WizardStyle=modern
DisableProgramGroupPage=yes

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Messages]
spanish.WelcomeLabel2=Este asistente instalará [name] en su computadora.%n%nSe instalarán los siguientes componentes si es necesario:%n- SQL Server LocalDB 2022%n- Windows App SDK Runtime%n%nSe recomienda cerrar todas las aplicaciones antes de continuar.

[Tasks]
Name: "desktopicon"; Description: "Crear icono en el escritorio"; GroupDescription: "Iconos adicionales:"

[Files]
; Archivos de la aplicación (publicados)
Source: "..\EDA 2.0\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Prerequisitos (descargar estos archivos y colocarlos en la carpeta Prerequisites)
Source: "Prerequisites\SqlLocalDB.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsLocalDBInstalled
Source: "Prerequisites\WindowsAppRuntimeInstall-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsWindowsAppSDKInstalled

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Instalar SQL Server LocalDB si no está instalado
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\SqlLocalDB.msi"" /qn IACCEPTSQLLOCALDBLICENSETERMS=YES"; StatusMsg: "Instalando SQL Server LocalDB..."; Flags: waituntilterminated; Check: not IsLocalDBInstalled

; Instalar Windows App SDK Runtime si no está instalado
Filename: "{tmp}\WindowsAppRuntimeInstall-x64.exe"; Parameters: "--quiet"; StatusMsg: "Instalando Windows App SDK Runtime..."; Flags: waituntilterminated; Check: not IsWindowsAppSDKInstalled

; Crear instancia de LocalDB si no existe (busca en varias versiones)
Filename: "{commonpf}\Microsoft SQL Server\170\Tools\Binn\SqlLocalDB.exe"; Parameters: "create MSSQLLocalDB"; StatusMsg: "Configurando base de datos..."; Flags: waituntilterminated runhidden skipifdoesntexist
Filename: "{commonpf}\Microsoft SQL Server\160\Tools\Binn\SqlLocalDB.exe"; Parameters: "create MSSQLLocalDB"; StatusMsg: "Configurando base de datos..."; Flags: waituntilterminated runhidden skipifdoesntexist

; Iniciar instancia de LocalDB (busca en varias versiones)
Filename: "{commonpf}\Microsoft SQL Server\170\Tools\Binn\SqlLocalDB.exe"; Parameters: "start MSSQLLocalDB"; StatusMsg: "Iniciando servidor de base de datos..."; Flags: waituntilterminated runhidden skipifdoesntexist
Filename: "{commonpf}\Microsoft SQL Server\160\Tools\Binn\SqlLocalDB.exe"; Parameters: "start MSSQLLocalDB"; StatusMsg: "Iniciando servidor de base de datos..."; Flags: waituntilterminated runhidden skipifdoesntexist

; Ejecutar la aplicación al finalizar
Filename: "{app}\{#MyAppExeName}"; Description: "Ejecutar {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Detener LocalDB al desinstalar (opcional)
; Filename: "sqllocaldb"; Parameters: "stop MSSQLLocalDB"; Flags: waituntilterminated runhidden

[Code]
// Verificar si SQL Server LocalDB está instalado
function IsLocalDBInstalled: Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec('sqllocaldb', 'info', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

// Verificar si Windows App SDK está instalado
function IsWindowsAppSDKInstalled: Boolean;
var
  WinAppSDKPath: String;
begin
  WinAppSDKPath := ExpandConstant('{commonpf}\WindowsApps');
  Result := DirExists(WinAppSDKPath + '\Microsoft.WindowsAppRuntime*');
end;

// Verificar requisitos del sistema
function InitializeSetup: Boolean;
var
  Version: TWindowsVersion;
begin
  GetWindowsVersionEx(Version);

  // Requiere Windows 10 1809 o superior
  if (Version.Major < 10) or ((Version.Major = 10) and (Version.Build < 17763)) then
  begin
    MsgBox('Esta aplicación requiere Windows 10 versión 1809 o superior.', mbError, MB_OK);
    Result := False;
    Exit;
  end;

  Result := True;
end;

// Mostrar progreso de instalación
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Aquí podrías ejecutar scripts adicionales de configuración
  end;
end;

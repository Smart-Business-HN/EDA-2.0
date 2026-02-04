# EDA 2.0 Installer

Este directorio contiene los archivos necesarios para crear el instalador de EDA 2.0 usando Inno Setup.

## Requisitos

1. **Inno Setup**: Descargar e instalar desde https://jrsoftware.org/isdl.php
2. **SQL Server LocalDB MSI**: Descargar desde Microsoft y colocar como `SqlLocalDB.msi` en este directorio

## Archivos

| Archivo | Descripcion |
|---------|-------------|
| `setup.iss` | Script de Inno Setup |
| `LICENSE.txt` | Licencia de usuario final (EULA) |
| `appsettings.template.json` | Configuracion inicial con conexion vacia |
| `SqlLocalDB.msi` | SQL Server LocalDB installer (descargar manualmente) |

## Descargar SQL Server LocalDB

1. Ir a: https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb
2. Descargar SQL Server 2022 Express LocalDB
3. Extraer el MSI y colocarlo en este directorio como `SqlLocalDB.msi`

Alternativamente, descargar directamente:
- SQL Server 2022 LocalDB: https://download.microsoft.com/download/...

## Pasos para crear el instalador

### 1. Publicar la aplicacion

```powershell
cd "c:\Repos\EDA-2.0"
dotnet publish "EDA 2.0\EDA.PRESENTATION.csproj" -c Release -r win-x64 --self-contained true
```

### 2. Compilar el instalador

1. Abrir Inno Setup Compiler
2. Abrir el archivo `setup.iss`
3. Presionar Ctrl+F9 o Build > Compile

### 3. Resultado

El instalador se generara en:
```
Installer\Output\EDA2_Setup_1.0.0.exe
```

## Configuracion del instalador

El archivo `setup.iss` tiene las siguientes configuraciones personalizables:

```iss
#define MyAppName "EDA 2.0"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Tu Empresa"
#define MyAppURL "https://example.com"
```

## Que hace el instalador

1. Verifica que .NET Desktop Runtime 10.0 este instalado
2. Instala SQL Server LocalDB si no esta presente
3. Copia los archivos de la aplicacion
4. Crea accesos directos en el menu inicio y escritorio
5. Configura la desinstalacion

## Flujo de primer inicio

1. Usuario ejecuta EDA 2.0 por primera vez
2. La aplicacion detecta que no hay conexion configurada
3. Muestra el wizard de configuracion de base de datos
4. Usuario elige LocalDB (1 PC) o SQL Server (multiples PCs)
5. Prueba la conexion
6. Guarda la configuracion y reinicia
7. La aplicacion aplica migraciones y muestra el login

## Soporte para multiples PCs

Para configurar EDA 2.0 en multiples computadoras conectadas a un servidor central:

1. Instalar SQL Server Express en el servidor
2. Configurar SQL Server para aceptar conexiones remotas
3. Crear la base de datos `eda_db`
4. En cada PC cliente, durante el wizard, seleccionar "Conectar a SQL Server"
5. Ingresar la IP o nombre del servidor, credenciales, etc.

## Notas

- El instalador requiere privilegios de administrador
- Compatible con Windows 10 version 1809 o superior
- La aplicacion es self-contained (no requiere .NET instalado por separado despues de .NET 10)

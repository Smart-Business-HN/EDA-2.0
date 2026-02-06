=====================================================================
       INSTRUCCIONES PARA CREAR EL INSTALADOR DE EDA 2.0
=====================================================================

PASO 1: PUBLICAR LA APLICACION
---------------------------------------------------------------------
Ejecuta este comando desde la raiz del proyecto:

    dotnet publish "EDA 2.0\EDA.PRESENTATION.csproj" -c Release -r win-x64 --self-contained true -p:Platform=x64

Los archivos se generaran en:
    EDA 2.0\bin\x64\Release\net10.0-windows10.0.19041.0\win-x64\publish\


PASO 2: DESCARGAR LOS PREREQUISITOS
---------------------------------------------------------------------
Coloca los siguientes archivos en la carpeta "Prerequisites":

1. SQL Server LocalDB 2022:
   - Descarga: https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SqlLocalDB.msi
   - Guardar como: Prerequisites\SqlLocalDB.msi

2. Windows App SDK Runtime:
   - Descarga: https://aka.ms/windowsappsdk/1.5/latest/windowsappruntimeinstall-x64.exe
   - Guardar como: Prerequisites\windowsappruntimeinstall-x64.exe


PASO 3: CONFIGURAR EL SCRIPT
---------------------------------------------------------------------
1. Abre "EDA2_Setup.iss" en Inno Setup Compiler
2. Cambia {YOUR-GUID-HERE} por un GUID unico
   (usa Tools > Generate GUID en Inno Setup)
3. Ajusta "MyAppPublisher" con el nombre de tu empresa
4. Verifica que el icono exista o comenta esa linea


PASO 4: COMPILAR EL INSTALADOR
---------------------------------------------------------------------
1. Abre Inno Setup Compiler
2. File > Open > selecciona "EDA2_Setup.iss"
3. Build > Compile (o presiona F9)

El instalador se genera en: Installer\Output\EDA2_Setup_v1.0.0.exe


=====================================================================
                    NOTAS IMPORTANTES
=====================================================================

CONEXION A BASE DE DATOS
---------------------------------------------------------------------
Tu aplicacion usa:
    Server=localhost;Database=eda_db;...

Si quieres usar LocalDB en lugar de SQL Server completo,
cambia la conexion en appsettings.json a:
    Server=(localdb)\MSSQLLocalDB;Database=eda_db;...


ESTRUCTURA DE CARPETAS
---------------------------------------------------------------------
Installer/
    EDA2_Setup.iss
    README.txt
    Prerequisites/
        SqlLocalDB.msi
        windowsappruntimeinstall-x64.exe
    Output/
        EDA2_Setup_v1.0.0.exe (generado)


REQUISITOS DEL SISTEMA
---------------------------------------------------------------------
- Windows 10 version 1809 o superior
- Arquitectura 64-bit (x64)
- 500 MB de espacio en disco

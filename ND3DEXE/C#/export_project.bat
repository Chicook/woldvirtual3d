@echo off
cd /d "%~dp0..\.."
echo Buscando Godot...

REM Buscar Godot en ubicaciones comunes
set GODOT_PATH=
if exist "C:\Users\%USERNAME%\AppData\Local\Programs\Godot\Godot_v4.5-stable_win64.exe" set GODOT_PATH=C:\Users\%USERNAME%\AppData\Local\Programs\Godot\Godot_v4.5-stable_win64.exe
if exist "C:\Program Files\Godot\Godot_v4.5-stable_win64.exe" set GODOT_PATH=C:\Program Files\Godot\Godot_v4.5-stable_win64.exe
if exist "C:\Godot\Godot_v4.5-stable_win64.exe" set GODOT_PATH=C:\Godot\Godot_v4.5-stable_win64.exe

if "%GODOT_PATH%"=="" (
    echo ERROR: Godot no encontrado. Abre Godot manualmente y exporta desde: Project ^> Export
    echo O ajusta GODOT_PATH en este script.
    pause
    exit /b 1
)

echo Godot encontrado: %GODOT_PATH%
echo Exportando a: build\WoldVirtual3D.exe
echo.

mkdir build 2>nul
"%GODOT_PATH%" --path "%CD%" --export-release "Windows Desktop" "build\WoldVirtual3D.exe"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo EXE GENERADO EXITOSAMENTE!
    echo Ubicacion: %CD%\build\WoldVirtual3D.exe
    echo ========================================
) else (
    echo.
    echo ERROR: Abre Godot y exporta manualmente desde Project ^> Export
)

pause


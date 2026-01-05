@echo off
REM ============================================
REM Script de Build para WoldVirtual3D Viewer
REM Genera ejecutable .exe para Windows
REM ============================================

setlocal enabledelayedexpansion

echo.
echo ============================================
echo   WoldVirtual3D Viewer - Build Script
echo ============================================
echo.

REM Cambiar al directorio del proyecto C#
cd /d "%~dp0"

REM Variables de configuracion
set BUILD_DIR=bin\Release\net8.0-windows
set OUTPUT_EXE=WoldVirtual3D.Viewer.exe

echo Compilando proyecto...
echo.

REM Limpiar build anterior
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

REM Compilar el proyecto en modo Release
echo [1/2] Compilando proyecto C#...
dotnet build WoldVirtual3D.Viewer.csproj -c Release
if !ERRORLEVEL! NEQ 0 (
    echo [ERROR] Fallo en la compilacion
    pause
    exit /b 1
)
echo [OK] Compilacion exitosa
echo.

REM Verificar que el ejecutable existe
if not exist "%BUILD_DIR%\%OUTPUT_EXE%" (
    echo [ERROR] El archivo .exe no se genero correctamente
    echo Buscando en: %CD%\%BUILD_DIR%
    pause
    exit /b 1
)

REM Obtener tamano del archivo
for %%F in ("%BUILD_DIR%\%OUTPUT_EXE%") do set FILE_SIZE=%%~zF
set /a FILE_SIZE_MB=%FILE_SIZE%/1024/1024

echo.
echo ============================================
echo   BUILD COMPLETADO EXITOSAMENTE!
echo ============================================
echo.
echo Ejecutable: %BUILD_DIR%\%OUTPUT_EXE%
echo Tamano: %FILE_SIZE_MB% MB
echo Ubicacion completa: %CD%\%BUILD_DIR%\%OUTPUT_EXE%
echo.
echo Para ejecutar:
echo   cd %BUILD_DIR%
echo   %OUTPUT_EXE%
echo.
echo ============================================
echo.

pause

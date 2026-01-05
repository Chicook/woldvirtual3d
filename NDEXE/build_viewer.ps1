# Script de compilación para WoldVirtual3D Viewer
# Requiere .NET 8.0 SDK instalado

Write-Host "=== WoldVirtual3D Viewer Build Script ===" -ForegroundColor Green
Write-Host "Compilando el visor..." -ForegroundColor Yellow

# Cambiar al directorio del proyecto
Set-Location $PSScriptRoot\WoldVirtual3DViewer

# Restaurar dependencias
Write-Host "Restaurando dependencias..." -ForegroundColor Cyan
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al restaurar dependencias" -ForegroundColor Red
    exit 1
}

# Compilar en Release
Write-Host "Compilando en modo Release..." -ForegroundColor Cyan
dotnet build --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error en la compilación" -ForegroundColor Red
    exit 1
}

# Publicar como aplicación autocontenida
Write-Host "Publicando aplicación..." -ForegroundColor Cyan
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output ..\WoldVirtual3DViewer_Published

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error en la publicación" -ForegroundColor Red
    exit 1
}

Write-Host "=== Compilación exitosa ===" -ForegroundColor Green
Write-Host "El visor se encuentra en: $($PSScriptRoot)\WoldVirtual3DViewer_Published" -ForegroundColor Green
Write-Host "Ejecutable principal: WoldVirtual3DViewer.exe" -ForegroundColor Green

# Crear archivo ZIP para distribución
Write-Host "Creando archivo ZIP para distribución..." -ForegroundColor Cyan
$zipPath = "$PSScriptRoot\WoldVirtual3DViewer_v1.0.0.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path "$PSScriptRoot\WoldVirtual3DViewer_Published\*" -DestinationPath $zipPath

Write-Host "Archivo ZIP creado: $zipPath" -ForegroundColor Green
Write-Host ""
Write-Host "=== Instrucciones de uso ===" -ForegroundColor Yellow
Write-Host "1. Copie la carpeta 'WoldVirtual3DViewer_Published' o extraiga el ZIP"
Write-Host "2. Ejecute 'WoldVirtual3DViewer.exe'"
Write-Host "3. Siga los pasos del asistente para registrar su PC y crear su cuenta"
Write-Host "4. Guarde los archivos ZIP de respaldo en un lugar seguro"
Write-Host "5. Inicie sesión para acceder a WoldVirtual3D"

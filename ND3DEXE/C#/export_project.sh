#!/bin/bash

echo "Exportando proyecto Godot a .exe..."
echo ""

# Ruta al ejecutable de Godot (ajusta según tu instalación)
GODOT_PATH="/usr/local/bin/godot"

# Verificar si existe Godot
if [ ! -f "$GODOT_PATH" ]; then
    echo "ERROR: No se encuentra Godot en: $GODOT_PATH"
    echo "Por favor, ajusta la ruta en este script."
    exit 1
fi

# Crear directorio build si no existe
mkdir -p build

# Exportar proyecto
"$GODOT_PATH" --path "$(dirname "$(dirname "$(readlink -f "$0")")")" --export-release "Windows Desktop" "build/WoldVirtual3D.exe"

if [ $? -eq 0 ]; then
    echo ""
    echo "Exportación exitosa!"
    echo "El .exe se encuentra en: build/WoldVirtual3D.exe"
else
    echo ""
    echo "ERROR en la exportación."
    exit 1
fi


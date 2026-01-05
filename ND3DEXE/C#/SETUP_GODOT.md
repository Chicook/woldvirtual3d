# Configuración de Godot para WoldVirtual3D Viewer

## Requisito

El visor 3D requiere el motor Godot para ejecutar las escenas 3D. **No es necesario instalar Godot en el sistema**, solo necesitas incluir el ejecutable en la carpeta de la aplicación.

## Instrucciones de Configuración

### Opción 1: Configuración Manual (Recomendado para Desarrollo)

1. **Descarga Godot:**
   - Visita: https://godotengine.org/download
   - Descarga la versión **Godot 4.x** (recomendado: 4.3, 4.4, o 4.5)
   - Selecciona la versión **Windows 64-bit** (Godot_v4.x-stable_win64.exe)

2. **Crea la carpeta Godot:**
   - Navega a la carpeta donde está el ejecutable del visor:
     ```
     D:\woldvirtual3d\ND3DEXE\C#\bin\Release\net8.0-windows\
     ```
   - Crea una carpeta llamada `Godot` en esta ubicación

3. **Copia Godot:**
   - Copia el archivo `Godot_v4.x-stable_win64.exe` que descargaste
   - Pégalo en la carpeta `Godot` que acabas de crear
   - (Opcional) Renómbralo a `Godot.exe` para simplificar

4. **Estructura final:**
   ```
   ND3DEXE/C#/bin/Release/net8.0-windows/
   ├── WoldVirtual3D.Viewer.exe
   ├── Godot/
   │   └── Godot.exe (o Godot_v4.x-stable_win64.exe)
   └── [otros archivos...]
   ```

### Opción 2: Script de Configuración Automática

Puedes crear un script que descargue y configure Godot automáticamente (pendiente de implementar).

## Verificación

1. Ejecuta `WoldVirtual3D.Viewer.exe`
2. Completa el proceso de registro (PC → Avatar → Usuario)
3. Inicia sesión
4. La escena 3D debería cargarse automáticamente

## Notas Importantes

- **No es necesario instalar Godot** en el sistema Windows
- El ejecutable de Godot debe estar en la carpeta `Godot/` junto al `.exe` del visor
- Funciona con cualquier versión de Godot 4.x (4.0, 4.1, 4.2, 4.3, 4.4, 4.5)
- El visor buscará Godot primero en la carpeta local antes de buscar en el sistema

## Distribución

Cuando distribuyas la aplicación, asegúrate de incluir:
- `WoldVirtual3D.Viewer.exe` y sus DLLs
- La carpeta `Godot/` con el ejecutable de Godot
- La carpeta `DTUSER/` con las configuraciones (se crea automáticamente)

## Solución de Problemas

**Error: "Godot no encontrado"**
- Verifica que la carpeta `Godot/` existe junto al ejecutable
- Verifica que `Godot.exe` (o `Godot_v4.x-stable_win64.exe`) está en la carpeta `Godot/`
- Asegúrate de que el ejecutable de Godot no está bloqueado por Windows (click derecho → Propiedades → Desbloquear)


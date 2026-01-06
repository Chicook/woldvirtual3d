# Plan de Reparación y Mejora del Código - WoldVirtual3D Viewer

## 1. Estado Actual

El proyecto se encuentra funcional y gran parte de la deuda técnica ha sido resuelta. Se han solucionado los problemas de compilación, de foco de entrada y se ha limpiado el código legado en los servicios principales.

## 2. Tareas de Refactorización (Estado: Completado/En Progreso)

### A. Limpieza de `GodotService.cs` (COMPLETADO)

- **Estado**: Se han eliminado rutas hardcoded y limpiado imports no utilizados.
- **Acciones Realizadas**:
  1. Se eliminaron las rutas fijas (`D:\woldvirtual3d`) y se reemplazaron por búsqueda relativa y almacenamiento en `AppData/Local`.
  2. Se eliminaron constantes y `DllImport` no utilizados (`SetForegroundWindow`, `WS_POPUP`).
  3. Se verificó la inexistencia de métodos obsoletos (`LaunchGodotSceneAsync`).

### B. Actualización de `LoginViewModel.cs` (COMPLETADO)

- **Estado**: El ViewModel utiliza correctamente el sistema de navegación y no depende de métodos de lanzamiento obsoletos.
- **Acciones Realizadas**:
  1. Se verificó que la navegación a `GameViewModel` delega correctamente el inicio del juego.
  2. El código es thread-safe y maneja excepciones adecuadamente.

### C. Consolidación de `GameViewModel.cs` (COMPLETADO)

- **Estado**: Código limpio, thread-safe y sin duplicidades.
- **Acciones Realizadas**:
  1. Se extrajo `ChatMessageItem` a su propio archivo, resolviendo errores CS0101/CS0229.
  2. Se verificó el uso de `RunOnUIThread` para operaciones sobre `ObservableCollection`.
  3. Se implementó un sistema robusto de foco (`FocusGame`) que combina llamadas WPF y Win32.

### D. Gestión de Foco (Win32 Interop) (COMPLETADO)

- **Estado**: Sistema híbrido implementado y funcional.
- **Acciones Realizadas**:
  1. `GameViewModel` expone `FocusGame()` que asegura que la ventana de Godot reciba inputs de teclado (flechas/espacio).
  2. `GameOverlayWindow` gestiona clics en el fondo transparente devolviendo el foco al juego.

## 3. Próximos Pasos (Mantenimiento)

1. **Pruebas de Integración**: Verificar que el ejecutable de Godot se detecte correctamente en diferentes máquinas (gracias a la nueva lógica de rutas).
2. **Monitorización**: Vigilar los logs para asegurar que no haya excepciones silenciosas en el proceso de incrustado.

## 4. Notas Técnicas

- **Compilación**: Ejecutar siempre `dotnet clean` tras refactorizaciones grandes de clases para limpiar cachés de obj/bin.
- **Dependencias**: Mantener `System.Management` y `Newtonsoft.Json` actualizados.

## 5. Historial de Errores Resueltos

Se han detectado y resuelto los siguientes errores reportados por la consola (CS0101, CS0229, CS0111), causados por archivos de compilación intermedios corruptos o duplicidad temporal de definiciones:

- `CS0101`: Definición duplicada de `GameViewModel`.
- `CS0229`: Ambigüedad en propiedades (`_statusText`, `_wcvCoinBalance`, etc.).
- `CS0111`: Miembros definidos múltiples veces con los mismos parámetros.

**Solución aplicada**:

1. Se extrajo la clase `ChatMessageItem` a su propio archivo en `Models/ChatMessageItem.cs`.
2. Se eliminó la definición anidada o duplicada de `ChatMessageItem` en `GameViewModel.cs`.
3. Se recomienda hacer limpieza de la solución (`dotnet clean`) para eliminar cualquier artefacto residual en `obj/` y `bin/`.

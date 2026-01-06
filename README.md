# Registro de Cambios - WoldVirtual3D Viewer
**Fecha:** 06 de Enero de 2026

Este documento detalla las modificaciones, refactorizaciones y nuevas implementaciones realizadas en el proyecto **WoldVirtual3DViewer** para mejorar la estabilidad, la interfaz de usuario y la integración con el motor Godot.

## 1. Incrustación del Motor Godot (Hosting)
Se ha reescrito completamente el sistema de incrustación de la ventana de Godot para solucionar problemas de foco (teclado/ratón) y mejorar la estabilidad.

- **Nuevo Host Nativo (`ExternalWindowHost`):**
  - Se eliminó la dependencia de `WindowsFormsHost` (que causaba conflictos de foco).
  - Implementación directa de `HwndHost` (WPF) utilizando la API Win32.
  - Uso de `LibraryImport` (P/Invoke moderno) para funciones como `SetParent`, `MoveWindow`, `SetWindowLong`.
  
- **Gestión de Foco (Input Fix):**
  - Implementación de lógica para transferir el foco explícitamente a la ventana de Godot (`SetFocus`) al hacer clic o interactuar.
  - Solución al problema donde el avatar no se movía (teclas flechas/espacio no detectadas).

## 2. Nueva Interfaz de Usuario (UI Overhaul)
Se ha rediseñado la interfaz gráfica para ofrecer una experiencia más moderna y profesional, alejándose del estilo "prototipo".

- **Pantalla de Inicio de Sesión (`LoginView`):**
  - Diseño centrado con tarjeta flotante (`Border` con `DropShadowEffect`).
  - Fondo oscuro (Dark Theme) coherente con aplicaciones de metaverso.
  - Campos de texto y botones estilizados.

- **Ventana Principal (`MainWindow`):**
  - **Menú de Preferencias:** Se sustituyó el botón "Seleccionar Motor" por un menú superior `Preferencias -> Seleccionar Motor`.
  - **Indicador de Economía:** Se añadió el balance de moneda **"WVC Coin"** en la barra superior (color dorado).

- **Sistema de Chat (Overlay):**
  - Implementación de una ventana transparente (`GameOverlayWindow`) superpuesta al juego.
  - **Estilo Second Life / OpenSim:** Historial de chat en la esquina inferior izquierda y barra de escritura en la parte inferior.
  - **Click-Through:** La ventana permite hacer clic a través de las zonas vacías para interactuar con el mundo 3D.

## 3. Refactorización y Calidad de Código
Se han corregido múltiples errores de compilación, advertencias de análisis estático y deudas técnicas.

- **Corrección de Errores Críticos:**
  - `CS0103` (InitializeComponent missing): Solucionado mediante limpieza de `obj/bin` y verificación de clases `partial`.
  - `CS0101/CS0229` (Clases duplicadas): Se movió `ChatMessageItem` a su propio archivo en la carpeta `Models`.
  - `CS0104` (Ambigüedad de referencias): Resolución de conflictos entre `System.Drawing.Point` y `System.Windows.Point`, y `OpenFileDialog`.

- **Modernización de C# (.NET 8):**
  - Uso de **Collection Expressions** (C# 12) para inicializar listas (`[]`).
  - Simplificación de instancias con `new()`.
  - Eliminación de `DllImport` en favor de `LibraryImport` (Mejor rendimiento y compatibilidad AOT).

- **Limpieza de Código:**
  - Eliminación de parámetros y variables no utilizadas (Warnings `IDE0060`, `IDE0059`).
  - Métodos estáticos marcados correctamente (`CA1822`).

## 4. Archivos Clave Modificados/Creados
- **Vistas:** `LoginView.xaml`, `MainWindow.xaml`, `GameOverlayWindow.xaml` (Nuevo), `GameView.xaml`.
- **Lógica (ViewModels):** `GameViewModel.cs` (Lógica de chat y arranque), `MainViewModel.cs` (Navegación y configuración).
- **Servicios:** `GodotService.cs` (Detección y lanzamiento de procesos).
- **Infraestructura:** `ExternalWindowHost.cs` (Clase crítica para el embedding).

---
*Generado automáticamente tras la sesión de trabajo del 06/01/2026.*

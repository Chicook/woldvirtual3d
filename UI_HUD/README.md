# 🎮 MetaverseHUD - Sistema de UI Profesional

## Descripción
Sistema de interfaz de usuario profesional para el visor 3D de WoldVirtual3DlucIA, inspirado en **Second Life** y **OpenSimulator**.

## Características

### 🔝 Barra Superior (TopBar)
- **Icono del mundo** 🌐 con nombre del mundo virtual
- **Ubicación actual** 📍 con nombre de la región
- **Coordenadas** ⊕ en tiempo real (X, Y, Z)
- **Reloj del mundo** ☀ con hora del sistema
- **Contador de FPS** con colores según rendimiento

### 🔽 Barra Inferior (BottomBar)
Botones de acción estilo SL/OpenSim:
- 🚶 **Mover** - Modo caminar
- 🦅 **Volar** - Modo vuelo
- 💬 **Chat** - Panel de comunicación
- 📦 **Inventario** - Gestión de objetos
- 🗺 **Mapa** - Mapa del mundo
- 🔨 **Construir** - Modo construcción
- 🔍 **Buscar** - Búsqueda de lugares/personas
- 📷 **Foto** - Captura de pantalla
- ⚙ **Configuración** - Opciones

### 🗺 Mini-Mapa
- Vista aérea simplificada del terreno
- Marcador de posición del avatar
- Indicador de norte
- Controles de zoom (+/-)
- Grid configurable

### 👤 Panel de Estado del Avatar
- **Barra de vida** ❤ (verde)
- **Barra de energía** ⚡ (azul)
- **Indicador de velocidad** 🏃

### 💬 Panel de Chat
- Pestañas: Local, Región, IM
- Mensajes con formato BBCode
- Campo de entrada con placeholder
- Botón enviar

## Estructura de Archivos

```
UI_HUD/
├── MetaverseHUD.tscn          # Escena principal del HUD
├── README.md                   # Esta documentación
├── components/
│   └── ChatPanel.tscn         # Panel de chat
├── scripts/
│   ├── MetaverseHUD.gd        # Script principal
│   ├── MinimapRenderer.gd     # Renderizado del minimapa
│   └── HUDUpdater.gd          # Actualizador de información
├── themes/
│   └── metaverse_theme.tres   # Theme visual profesional
└── icons/                      # Iconos personalizados (futuro)
```

## Uso

### Integración Básica
El HUD ya está integrado en `bsprincipal.tscn`. Se carga automáticamente.

### Controles de Teclado
| Tecla | Acción |
|-------|--------|
| `I` | Abrir/cerrar inventario |
| `M` | Abrir/cerrar mapa |
| `B` | Activar modo construcción |
| `Enter` | Abrir/cerrar chat |
| `Tab` | Mostrar/ocultar HUD |
| `ESC` | Liberar ratón |

### API del Script

```gdscript
# Obtener referencia al HUD
var hud = $MetaverseHUD

# Cambiar nombre de la región
hud.set_region_name("Nueva Región")

# Cambiar nombre del mundo
hud.set_world_name("Mi Mundo")

# Establecer jugador
hud.set_player(player_node)

# Mostrar notificación
hud.show_notification("¡Bienvenido!", 3.0)

# Alternar visibilidad
hud.toggle_hud_visibility()
```

## Paleta de Colores

### Colores Principales
- **Fondo oscuro**: `#0A0C14` (rgba: 0.04, 0.05, 0.08, 0.92)
- **Borde activo**: `#3373BF` (rgba: 0.2, 0.45, 0.75, 0.6)
- **Texto principal**: `#E5EBF2` (rgba: 0.9, 0.92, 0.95, 1.0)
- **Acento azul**: `#3399FF` (rgba: 0.2, 0.6, 1.0, 1.0)

### Colores de Estado
- **Vida (Verde)**: `#33B84D` (rgba: 0.2, 0.72, 0.3, 0.9)
- **Energía (Azul)**: `#4D80E6` (rgba: 0.3, 0.5, 0.9, 0.9)
- **Alerta (Amarillo)**: `#E6CC66` (rgba: 0.9, 0.8, 0.4, 1.0)
- **Error (Rojo)**: `#E68080` (rgba: 0.9, 0.5, 0.5, 1.0)

## Personalización

### Cambiar Tema
Edita `themes/metaverse_theme.tres` para modificar:
- Colores de fondo
- Bordes y esquinas redondeadas
- Tamaños de fuente
- Estilos de botones

### Añadir Nuevos Botones
En `MetaverseHUD.tscn`, añade botones al nodo `BottomBar/HBoxContainer`.

## Compatibilidad
- **Godot**: 4.5+
- **Resoluciones**: 1280x720 a 4K
- **Escalado UI**: Automático

## Inspiración
Este diseño está inspirado en los viewers de:
- **Second Life** (Firestorm, oficial)
- **OpenSimulator** (Singularity, Kokua)

## Versión
- **v1.0.0** - Implementación inicial
- Sistema HUD profesional completo
- Integración con escena principal

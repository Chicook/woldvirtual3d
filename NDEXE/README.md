# WoldVirtual3D Viewer - NDEXE

Visor 3D para el metaverso WoldVirtual3D, creado desde cero siguiendo arquitectura modular.

## Estructura

```
NDEXE/
├── Forms/              # Formularios Windows Forms
│   └── MainViewerForm.cs
├── Services/            # Servicios del visor
│   ├── GodotService.cs
│   └── SceneManager.cs
├── Models/              # Modelos de datos
│   └── SceneData.cs
├── Controls/            # Controles UI personalizados
├── Rendering/           # Componentes de renderizado
├── Server/              # Servidor interno
├── Program.cs           # Punto de entrada
├── Viewer3D.cs          # Clase principal del visor
├── WoldVirtual3D.Viewer.csproj
└── app.manifest
```

## Requisitos

- .NET 8.0 SDK
- Godot Engine 4.5+ (debe estar en carpeta Godot/ o en PATH)
- Windows 10/11

## Compilación

```bash
dotnet build -c Release
```

## Configuración

El visor busca Godot.exe en las siguientes ubicaciones:
1. `[Ejecutable]/Godot/Godot.exe`
2. `[Ejecutable]/../Godot/Godot.exe`
3. `C:\Program Files\Godot\Godot.exe`
4. PATH del sistema

El proyecto Godot debe estar en la misma carpeta o en una carpeta padre del ejecutable.

## Arquitectura

- **Modular**: Cada componente tiene responsabilidades claras
- **Asíncrono**: Operaciones de carga y gestión son asíncronas
- **Extensible**: Fácil agregar nuevos servicios y componentes


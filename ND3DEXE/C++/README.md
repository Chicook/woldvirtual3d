# Sistema C++ para WoldVirtual3D

## Descripción

Implementación en C++ de componentes de alto rendimiento para WoldVirtual3D, incluyendo red P2P y gestión de base de datos.

## Estructura de Archivos

### p2p_network.h / p2p_network.cpp

- Sistema de red P2P distribuida de bajo nivel
- Gestión de sockets UDP/TCP
- Descubrimiento de nodos y sincronización

### database_manager.h / database_manager.cpp

- Gestor de base de datos SQLite de alto rendimiento
- Operaciones CRUD optimizadas
- Hash de contraseñas con OpenSSL SHA256

### register_types.h / register_types.cpp

- Registro de clases GDExtension
- Inicialización del módulo

## Compilación

### Requisitos

- CMake 3.16+
- Compilador C++17 (GCC, Clang, MSVC)
- Godot C++ bindings (godot-cpp)
- SQLite3
- OpenSSL

### Pasos

1. Clonar godot-cpp en el directorio raíz del proyecto
2. Compilar godot-cpp siguiendo su documentación
3. Ejecutar:

```bash
mkdir build
cd build
cmake ..
make
```

### Windows

```bash
mkdir build
cd build
cmake .. -G "Visual Studio 17 2022"
cmake --build . --config Release
```

## Integración con Godot

1. Crear archivo `.gdextension` en el directorio del proyecto
2. Configurar la ruta a la biblioteca compilada
3. Las clases estarán disponibles como nodos en Godot

## Uso

Las clases `P2PNetworkCpp` y `DatabaseManager` se pueden usar desde GDScript o C# como nodos normales de Godot.

## Dependencias

- Godot 4.5+
- SQLite3
- OpenSSL
- Winsock2 (Windows)


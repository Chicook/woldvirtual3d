# Visor 3D - Sistema de Autenticación y Red P2P

## Descripción

Sistema modular de visor 3D con autenticación de usuarios y red P2P distribuida para WoldVirtual3D.

## Estructura de Archivos

### LoginManager.cs

- Gestión de inicio de sesión y autenticación
- Registro de nuevos usuarios
- Gestión de sesiones persistentes

### UserDatabase.cs

- Operaciones CRUD con base de datos SQLite
- Almacenamiento en `DTUSER/database/userdata.db`
- Hash de contraseñas con SHA256

### Viewer3D.cs

- Visor 3D principal
- Carga `bsprincipal.tscn` después del login exitoso
- Coordinación entre login y escena 3D

### P2PNetwork.cs

- Red P2P distribuida
- Descubrimiento de nodos
- Sincronización de datos entre peers

### DataModels.cs

- Modelos de datos: UserData, SessionData, P2PNode, P2PMessage

## Flujo de Ejecución

1. **Inicio**: Viewer3D se inicializa y muestra UI de login
2. **Autenticación**: Usuario ingresa credenciales → LoginManager valida → UserDatabase verifica
3. **Carga de Escena**: Si login exitoso → se carga `bsprincipal.tscn`
4. **Red P2P**: Se inicializa red P2P para sincronización

## Base de Datos

Ubicación: `DTUSER/database/userdata.db`

Tablas:

- `users`: Información de usuarios
- `sessions`: Sesiones activas
- `user_settings`: Configuraciones y preferencias

## Red P2P

Puertos:

- UDP: 7777 (descubrimiento)
- TCP: 7778 (conexiones estables)

Directorio: `redp2pDTR/`

## Dependencias

- Godot 4.5+
- System.Data.SQLite
- .NET 6.0+


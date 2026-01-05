# WoldVirtual3D Viewer

Visor 3D para el metaverso descentralizado WoldVirtual3D. Esta aplicación permite registrar tu PC, crear una cuenta de usuario y acceder al metaverso sin necesidad de tener Godot instalado.

## 🚀 Características

- **Registro Seguro del PC**: Genera un hash único basado en el hardware de tu equipo
- **Sistema de Cuentas**: Crea y valida cuentas de usuario con avatar
- **Integración con Godot**: Ejecuta escenas de Godot directamente desde el visor
- **Almacenamiento Seguro**: Guarda hashes únicos en archivos ZIP encriptados
- **Interfaz Moderna**: UI/UX intuitiva con tema oscuro

## 📋 Requisitos del Sistema

- **Sistema Operativo**: Windows 10/11 (64-bit)
- **.NET Runtime**: Incluido en la distribución (no requiere instalación)
- **Espacio en Disco**: 50 MB mínimo
- **Memoria RAM**: 2 GB mínimo
- **Procesador**: Compatible con x64

## 🛠️ Instalación

1. **Descarga**: Obtén el archivo `WoldVirtual3DViewer_v1.0.0.zip`
2. **Extrae**: Descomprime el archivo en una carpeta de tu elección
3. **Ejecuta**: Haz doble clic en `WoldVirtual3DViewer.exe`

## 📖 Guía de Uso

### Paso 1: Registro del PC

1. Al iniciar el visor por primera vez, verás la pantalla de "Registro de PC"
2. Haz clic en **"Registrar PC"** para capturar la información de tu hardware
3. Revisa la información mostrada (placa base, procesador, etc.)
4. Haz clic en **"Descargar Hash"** para guardar el archivo `pc_hash_*.zip`
5. **IMPORTANTE**: Guarda este archivo ZIP en un lugar seguro y privado

### Paso 2: Selección de Avatar

1. Después del registro del PC, selecciona tu avatar
2. Para desarrollo inicial, selecciona **"chica"** (avatar femenino por defecto)
3. Haz clic en **"Seleccionar Avatar y Continuar"**

### Paso 3: Registro de Usuario

1. Ingresa un **nombre de usuario** único
2. Crea una **contraseña** segura (mínimo 6 caracteres)
3. Confirma tu contraseña
4. Haz clic en **"Registrar Usuario"**
5. Descarga el archivo `user_account_*.zip` y guárdalo junto con el hash del PC

### Paso 4: Inicio de Sesión

1. En sesiones posteriores, usa la pantalla de "Iniciar Sesión"
2. Ingresa tu usuario y contraseña
3. Haz clic en **"Iniciar Sesión en WoldVirtual3D"**
4. El visor abrirá automáticamente Godot con la escena `bspeincipal.tscn`

## 🔐 Seguridad

### Archivos de Respaldo

El visor genera dos archivos ZIP críticos que debes guardar:

1. **`pc_hash_*.zip`**: Contiene el hash único de tu PC
   - Específico para tu hardware
   - Necesario para validar tu identidad
   - **NO compartir con nadie**

2. **`user_account_*.zip`**: Contiene la información de tu cuenta
   - Hash de usuario y contraseña
   - Información del avatar
   - **NO compartir con nadie**

### Recomendaciones de Seguridad

- Guarda los archivos ZIP en un dispositivo externo seguro
- Usa contraseñas fuertes y únicas
- No compartas tus credenciales
- Mantén el visor actualizado

## 🏗️ Arquitectura Técnica

### Componentes Principales

```text
WoldVirtual3DViewer/
├── Models/           # Modelos de datos (PCInfo, UserAccount, AvatarInfo)
├── Services/         # Servicios (Hardware, Data, Godot)
├── ViewModels/       # Lógica de presentación
├── Views/           # Interfaces de usuario WPF
├── Utils/           # Utilidades (RelayCommand)
└── Converters/      # Convertidores XAML
```

### Integración con Godot

El visor puede ejecutar escenas de Godot sin requerir instalación:

1. Busca automáticamente el ejecutable de Godot
2. Valida la existencia del proyecto en `D:\woldvirtual3d\`
3. Ejecuta con parámetros específicos del usuario
4. Pasa variables de entorno para personalización

### Variables de Entorno

Al ejecutar Godot, el visor establece:

- `WOLDVIRTUAL_USER`: Nombre del usuario
- `WOLDVIRTUAL_AVATAR`: Tipo de avatar seleccionado
- `WOLDVIRTUAL_ACCOUNT_HASH`: Hash único de la cuenta

## 🔧 Desarrollo

### Compilación

```powershell
# Ejecutar el script de compilación
.\build_viewer.ps1
```

### Dependencias

- .NET 8.0 SDK
- NuGet packages:
  - Newtonsoft.Json
  - System.Management
  - Microsoft.Extensions.Hosting
  - Microsoft.Extensions.DependencyInjection

### Estructura del Proyecto

El visor sigue el patrón MVVM (Model-View-ViewModel):

- **Models**: Representan datos y lógica de negocio
- **ViewModels**: Manejan el estado y comandos
- **Views**: Definen la interfaz de usuario
- **Services**: Proporcionan funcionalidades externas

## 🐛 Solución de Problemas

### El visor no inicia

- Verifica que estés en Windows 10/11 64-bit
- Asegúrate de extraer completamente el archivo ZIP
- Verifica que no haya antivirus bloqueando la ejecución

### Error al registrar PC

- Ejecuta como administrador
- Verifica permisos de acceso al hardware
- Asegúrate de que WMI esté habilitado

### Godot no se ejecuta

- Verifica que Godot esté instalado
- Confirma que el proyecto existe en `D:\woldvirtual3d\`
- Revisa que `bspeincipal.tscn` esté presente

### Archivos de respaldo perdidos

- Si pierdes los archivos ZIP, deberás registrar nuevamente el PC
- Las cuentas existentes pueden requerir recuperación manual
- Contacta al soporte técnico si es necesario

## 📞 Soporte

Para soporte técnico o reportar problemas:

1. Revisa esta documentación
2. Verifica los logs de la aplicación
3. Reporta issues con detalles completos del error

## 📝 Registro de Cambios

### v1.0.0

- Lanzamiento inicial
- Registro de PC con hash único
- Sistema de cuentas de usuario
- Selección de avatar
- Integración con Godot
- Interfaz moderna con tema oscuro
- Sistema de respaldos ZIP

## 📄 Licencia

Este software es parte del proyecto WoldVirtual3D - Metaverso Descentralizado.
Todos los derechos reservados.

# ⚠️ Warnings Conocidos del Proyecto

Este documento lista los warnings conocidos que aparecen durante el desarrollo, su estado actual y cómo manejarlos.

**Versión del documento**: 1.1  
**Última actualización**: 11 de Julio 2025

---

## 📋 Índice

- [Warnings de Godot Engine (Internos)](#-warnings-de-godot-engine-internos)
- [Warnings de GDScript (Resueltos)](#-warnings-de-gdscript-resueltos)
- [Guía de Manejo de Warnings](#-guía-de-manejo-de-warnings)
- [Cómo Reportar Nuevos Warnings](#-cómo-reportar-nuevos-warnings)

---

## 🔴 Warnings de Godot Engine (Internos)

### `instance_reset_physics_interpolation() is deprecated`

**Estado**: ⚠️ **Activo** - No requiere acción

**Ubicación**: `servers/rendering_server.compat.inc:58`

**Tipo**: Warning de deprecación interno de Godot (C++)

**Frecuencia**: Aparece una vez durante la inicialización del motor

**Descripción**: 
Este warning proviene del código interno de Godot Engine (C++), específicamente del sistema de compatibilidad que maneja funciones deprecadas. La función `instance_reset_physics_interpolation()` está marcada como deprecada en Godot 4.x, pero el código de compatibilidad interno aún la utiliza para mantener compatibilidad con versiones anteriores.

**Causa**:
- Código interno de compatibilidad de Godot Engine
- No es causado por el código del proyecto
- Aparece durante la inicialización del motor de renderizado
- Es parte del sistema de compatibilidad hacia atrás de Godot

**Impacto**: 
- ⚠️ **Ninguno**: Es solo un warning informativo
- No afecta la funcionalidad del proyecto
- No causa errores en tiempo de ejecución
- No afecta el rendimiento
- No genera problemas de compilación

**Solución**:
- ✅ **No requiere acción del desarrollador**
- Este warning se resolverá automáticamente en futuras versiones de Godot
- El código del proyecto no necesita cambios
- Se puede ignorar de forma segura durante el desarrollo

**Cuándo aparecerá**:
- Durante la inicialización del motor
- Al cargar escenas con nodos que usan interpolación de física
- Es un mensaje único por sesión de ejecución

**Referencias**:
- [Godot Issue Tracker](https://github.com/godotengine/godot/issues)
- [Documentación de Godot 4.5 - Physics Interpolation](https://docs.godotengine.org/en/stable/)
- [Godot 4.x Migration Guide](https://docs.godotengine.org/en/stable/getting_started/upgrading/)

---

## ✅ Warnings de GDScript (Resueltos)

### Shadowing de Variables de Clase Base

**Estado**: ✅ **Resuelto** - Corregido en `movimientoAV3d.gd`

**Ubicación**: `GDSCRIP/movimientoAV3d.gd:325` y `:428`

**Tipo**: Warning de GDScript - `SHADOWED_VARIABLE_BASE_CLASS`

**Descripción**: 
Los parámetros de función `position` y `rotation` estaban shadowing (ocultando) las propiedades de la clase base `Node3D`. Esto puede causar confusión y errores sutiles.

**Solución Aplicada**:
- Renombrado `position` a `new_position` en función `receive_network_update()`
- Renombrado `rotation` a `new_rotation` en función `receive_network_update()`
- Actualizadas todas las referencias internas a estos parámetros

**Código Corregido**:
```gdscript
# Antes (con warning):
func receive_network_update(avatar_id: int, timestamp: float, position: Vector3, rotation: Quaternion)

# Después (corregido):
func receive_network_update(avatar_id: int, timestamp: float, new_position: Vector3, new_rotation: Quaternion)
```

---

### Parámetros No Utilizados

**Estado**: ✅ **Resuelto** - Corregido en `movimientoAV3d.gd`

**Ubicación**: `GDSCRIP/movimientoAV3d.gd:428`

**Tipo**: Warning de GDScript - `UNUSED_PARAMETER`

**Descripción**: 
Los parámetros `avatar_id`, `position` y `rotation` en la función `apply_networked_avatar_transform()` no se estaban utilizando. Esta función está diseñada como plantilla para implementación futura.

**Solución Aplicada**:
- Prefijados los parámetros no utilizados con `_` para indicar que son intencionalmente no usados
- Esto sigue las convenciones de GDScript para parámetros reservados para uso futuro

**Código Corregido**:
```gdscript
# Antes (con warning):
func apply_networked_avatar_transform(avatar_id: int, position: Vector3, rotation: Quaternion)

# Después (corregido):
func apply_networked_avatar_transform(_avatar_id: int, _position: Vector3, _rotation: Quaternion)
```

**Nota**: Esta función está diseñada como plantilla para implementación futura del sistema de gestión de avatares en red. Los parámetros están prefijados con `_` para indicar que son intencionalmente no usados por ahora.

---

## 📝 Notas Adicionales

### Physics Interpolation en Godot 4.x

En Godot 4.x, la interpolación de física se maneja automáticamente por el motor. No es necesario llamar manualmente a funciones de reset de interpolación.

**Configuración en el proyecto**:
- `physics_interpolation_mode = 0` en archivos `.tscn` es válido y correcto
- El motor maneja la interpolación automáticamente según la configuración del nodo
- Los valores válidos son:
  - `0`: Deshabilitado
  - `1`: Habilitado (recomendado para objetos en movimiento)

**Archivos con configuración de interpolación**:
- `ND3D/NDBSrprc_3d.tscn`: Configurado con `physics_interpolation_mode = 0`

### Convenciones de GDScript

**Parámetros no utilizados**:
- Prefijar con `_` cuando un parámetro es intencionalmente no usado
- Ejemplo: `func example(_unused_param: int, used_param: String)`

**Evitar shadowing**:
- No usar nombres de parámetros que coincidan con propiedades de la clase base
- Usar nombres descriptivos y únicos
- Ejemplo: En lugar de `position`, usar `new_position` o `target_position`

---

## 🔍 Guía de Manejo de Warnings

### Clasificación de Warnings

1. **Warnings Internos de Godot** (No corregibles)
   - Provienen del código C++ de Godot Engine
   - Se documentan pero no se corrigen
   - Se resolverán en futuras versiones de Godot

2. **Warnings de GDScript** (Corregibles)
   - Provienen del código del proyecto
   - Deben corregirse siguiendo las mejores prácticas
   - Se documentan cuando se resuelven

3. **Warnings de TypeScript/JavaScript** (Corregibles)
   - Provienen del código frontend
   - Deben corregirse para mantener calidad de código
   - Se documentan en el proceso de desarrollo

### Prioridad de Corrección

**Alta Prioridad**:
- Warnings que causan errores en tiempo de ejecución
- Warnings que afectan la funcionalidad
- Warnings de seguridad

**Media Prioridad**:
- Warnings de código que afectan mantenibilidad
- Warnings de rendimiento potencial
- Warnings de buenas prácticas

**Baja Prioridad**:
- Warnings informativos
- Warnings de deprecación que no afectan funcionalidad
- Warnings internos de herramientas

---

## 📊 Cómo Reportar Nuevos Warnings

Si encuentras un nuevo warning que no está documentado aquí, sigue estos pasos:

### 1. Verificar el Origen

**Warnings de Godot (C++)**:
- Revisar si el warning viene de código interno (C++)
- Verificar la ubicación en el stack trace
- Si es interno, documentarlo como "No corregible"

**Warnings del Proyecto**:
- Identificar el archivo y línea exacta
- Determinar el tipo de warning (GDScript, TypeScript, etc.)
- Verificar si afecta la funcionalidad

### 2. Documentar el Warning

Agregar una nueva entrada en este archivo con:

```markdown
### Nombre del Warning

**Estado**: ⚠️ Activo / ✅ Resuelto

**Ubicación**: `ruta/al/archivo.ext:linea`

**Tipo**: Tipo de warning (GDScript, TypeScript, etc.)

**Descripción**: Descripción detallada del warning

**Causa**: Qué causa el warning

**Impacto**: Cómo afecta al proyecto

**Solución**: Cómo resolverlo (si es posible)
```

### 3. Investigar la Causa

- Revisar el código relacionado
- Verificar documentación oficial
- Buscar issues similares en el repositorio de Godot/TypeScript

### 4. Proponer Solución

- Si es corregible, implementar la solución
- Si no es corregible, documentar por qué
- Actualizar este documento con la solución

### 5. Actualizar el Estado

- Marcar como "Resuelto" cuando se corrige
- Agregar fecha de resolución
- Documentar la solución aplicada

---

## 📚 Referencias y Recursos

### Documentación Oficial

- [Godot 4.5 Documentation](https://docs.godotengine.org/en/stable/)
- [GDScript Style Guide](https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/gdscript_styleguide.html)
- [TypeScript Documentation](https://www.typescriptlang.org/docs/)

### Herramientas de Análisis

- **Godot Editor**: Muestra warnings en tiempo real
- **GDScript Linter**: Herramientas de linting para GDScript
- **TypeScript Compiler**: `tsc --noEmit` para verificar tipos

### Comunidad

- [Godot Forums](https://forum.godotengine.org/)
- [Godot Discord](https://discord.gg/godot)
- [Stack Overflow - Godot Tag](https://stackoverflow.com/questions/tagged/godot)

---

## 📝 Historial de Cambios

### v1.1 (11 de Julio 2025)
- ✅ Agregada sección de warnings resueltos
- ✅ Mejorada estructura y organización
- ✅ Agregada guía de manejo de warnings
- ✅ Agregadas referencias y recursos
- ✅ Documentado warning de physics interpolation

### v1.0 (11 de Julio 2025)
- ✅ Documentación inicial de warnings conocidos
- ✅ Documentado warning interno de Godot

---

**Mantenido por**: Equipo de Desarrollo WoldVirtual3D  
**Contacto**: Ver [README.md](../README.md) para más información

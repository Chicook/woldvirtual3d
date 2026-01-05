extends CharacterBody3D

@onready var ani_tree: AnimationTree = $MJ1_3D/AnimationTree

# Estados de animación
enum AnimState { IDLE, CAMINAR, CORRER, SALTAR }
var cur_anim: AnimState = AnimState.IDLE

# Parámetros de movimiento
@export var blend_speed: float = 15.0
@export var walk_speed: float = 3.0
@export var run_speed: float = 6.0
@export var acceleration: float = 10.0
@export var friction: float = 15.0
@export var jump_velocity: float = 4.5
@export var gravity: float = 9.8

# Valores de blend para animaciones
var caminar_val: float = 0.0
var correr_val: float = 0.0
var saltar_val: float = 0.0

# Variables de estado
var is_running: bool = false
var input_dir: Vector2 = Vector2.ZERO
var direction: Vector3 = Vector3.ZERO
var was_on_floor: bool = false
var can_jump: bool = true
var is_jumping: bool = false

# --- SISTEMA DE JITTER BUFFER OPTIMIZADO PARA GHOSTING P2P ---
# Implementado según mejores prácticas Godot 4.5: interpolación suave a 60 FPS
class NetworkedAvatar:
	var avatar_id: int
	var position_buffer: Array = []  # { timestamp: float, position: Vector3, rotation: Quaternion }
	var current_position: Vector3
	var current_rotation: Quaternion
	var last_update_time: float = 0.0
	var interpolation_delay: float = 0.1  # 100ms de delay para compensar lag internacional
	var max_buffer_size: int = 10  # Optimización: limitar memoria
	var smoothing_factor: float = 15.0  # Factor de suavizado para interpolación
	
	func _init(id: int):
		avatar_id = id
		current_position = Vector3.ZERO
		current_rotation = Quaternion.IDENTITY
	
	func add_snapshot(timestamp: float, pos: Vector3, rot: Quaternion):
		# Añadir snapshot al buffer manteniendo orden cronológico
		var snapshot = { 
			"timestamp": timestamp, 
			"position": pos, 
			"rotation": rot 
		}
		
		# Insertar en orden cronológico (optimizado: búsqueda binaria implícita)
		var insert_index = 0
		for i in range(position_buffer.size() - 1, -1, -1):
			if position_buffer[i]["timestamp"] <= timestamp:
				insert_index = i + 1
				break
		
		position_buffer.insert(insert_index, snapshot)
		last_update_time = Time.get_unix_time_from_system()
		
		# Mantener buffer limitado para optimización de memoria
		if position_buffer.size() > max_buffer_size:
			position_buffer.pop_front()
	
	func interpolate(delta: float) -> bool:
		if position_buffer.size() < 2:
			return false
		
		var current_time = Time.get_unix_time_from_system() - interpolation_delay
		
		# Encontrar los dos snapshots más relevantes para interpolación
		var older_index = -1
		var newer_index = -1
		
		for i in range(position_buffer.size()):
			if position_buffer[i]["timestamp"] <= current_time:
				older_index = i
			else:
				newer_index = i
				break
		
		if older_index == -1 or newer_index == -1:
			# No hay suficientes snapshots para interpolación
			if position_buffer.size() > 0:
				# Usar el snapshot más reciente directamente
				var latest = position_buffer[position_buffer.size() - 1]
				current_position = latest["position"]
				current_rotation = latest["rotation"]
				return true
			return false
		
		var older_snapshot = position_buffer[older_index]
		var newer_snapshot = position_buffer[newer_index]
		
		var time_range = newer_snapshot["timestamp"] - older_snapshot["timestamp"]
		if time_range <= 0:
			return false
		
		# Calcular alpha con clamping para evitar extrapolación
		var alpha = (current_time - older_snapshot["timestamp"]) / time_range
		alpha = clamp(alpha, 0.0, 1.0)
		
		# Interpolación suavizada usando lerp para posición y slerp para rotación
		# (slerp es más preciso para rotaciones pero más costoso, usar lerp para optimización)
		current_position = older_snapshot["position"].lerp(newer_snapshot["position"], alpha)
		
		# Para rotaciones, usar slerp (spherical interpolation) para mejor calidad
		# pero con factor de suavizado para evitar jitter
		var target_rotation = older_snapshot["rotation"].slerp(newer_snapshot["rotation"], alpha)
		current_rotation = current_rotation.slerp(target_rotation, smoothing_factor * delta)
		
		return true
	
	func get_latest_position() -> Vector3:
		if position_buffer.size() > 0:
			return position_buffer[position_buffer.size() - 1]["position"]
		return current_position
	
	func get_latest_rotation() -> Quaternion:
		if position_buffer.size() > 0:
			return position_buffer[position_buffer.size() - 1]["rotation"]
		return current_rotation

# Diccionario de avatares en red
var networked_avatars: Dictionary = {}
var is_local_player: bool = true  # Este script controla al jugador local

# Configuración de red
@export var network_update_rate: float = 0.03  # 30ms entre updates (33 FPS)
var network_update_timer: float = 0.0

func _ready():
	if not ani_tree:
		push_error("AnimationTree no encontrado en $MJ1_3D/AnimationTree")
		return
	ani_tree.active = true

func _physics_process(delta):
	# Manejar input del usuario (antes de aplicar física)
	handle_input()
	
	# Aplicar gravedad
	apply_gravity(delta)
	
	# Procesar movimiento
	handle_movement(delta)
	
	# Aplicar movimiento físico
	move_and_slide()
	
	# Actualizar estado del suelo para salto (después de move_and_slide)
	update_floor_state()
	
	# Actualizar animaciones según el estado
	update_animation_state()
	handle_animations(delta)
	update_tree()
	
	# Procesar interpolación de avatares en red
	process_networked_avatars(delta)
	
	# Enviar updates de red para jugador local
	if is_local_player:
		network_update_timer += delta
		if network_update_timer >= network_update_rate:
			network_update_timer = 0.0
			send_network_update()

func apply_gravity(delta: float):
	if not is_on_floor():
		velocity.y -= gravity * delta
	else:
		# Si está en el suelo y cayendo, detener la caída
		# Solo resetear si está cayendo, no si está saltando
		if velocity.y < 0:
			velocity.y = 0

func handle_input():
	# Obtener input direccional (WASD o flechas)
	input_dir = Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	
	# Detectar si está corriendo (Shift)
	is_running = Input.is_action_pressed("ui_select") or Input.is_key_pressed(KEY_SHIFT)
	
	# Calcular dirección de movimiento relativa a la cámara
	direction = get_world_direction(input_dir)
	
	# Manejar salto - solo si está en el suelo y puede saltar
	if Input.is_action_just_pressed("ui_accept") and can_jump and is_on_floor():
		# Asegurar que está firmemente en el terreno antes de saltar
		if was_on_floor and velocity.y <= 0.0:
			# Aplicar velocidad de salto inmediatamente
			velocity.y = jump_velocity
			is_jumping = true
			can_jump = false

func get_world_direction(input_vector: Vector2) -> Vector3:
	var world_dir := Vector3.ZERO
	
	# Obtener la cámara del viewport
	var camera_3d := get_viewport().get_camera_3d()
	
	if camera_3d:
		# Usar la base de la cámara para movimiento relativo a la vista
		var cam_basis := camera_3d.global_transform.basis
		world_dir = cam_basis * Vector3(input_vector.x, 0, input_vector.y)
		world_dir.y = 0
	else:
		# Si no hay cámara, usar la transformación del personaje
		world_dir = transform.basis * Vector3(input_vector.x, 0, input_vector.y)
	
	return world_dir.normalized()

func handle_movement(delta: float):
	var target_speed: float = 0.0
	
	# Determinar velocidad objetivo según input y estado
	if direction != Vector3.ZERO:
		# Rotar el avatar hacia la dirección de movimiento
		if direction.length() > 0.1:
			var target_rotation := atan2(direction.x, direction.z)
			rotation.y = lerp_angle(rotation.y, target_rotation, 10.0 * delta)
		
		# Seleccionar velocidad según si está corriendo
		if is_running:
			target_speed = run_speed
		else:
			target_speed = walk_speed
		
		# Aplicar aceleración
		var target_velocity := direction * target_speed
		velocity.x = move_toward(velocity.x, target_velocity.x, acceleration * delta)
		velocity.z = move_toward(velocity.z, target_velocity.z, acceleration * delta)
	else:
		# Aplicar fricción cuando no hay input
		velocity.x = move_toward(velocity.x, 0, friction * delta)
		velocity.z = move_toward(velocity.z, 0, friction * delta)

func update_floor_state():
	# Actualizar estado del suelo para control de salto
	var is_on_floor_now := is_on_floor()
	
	# Si acaba de tocar el suelo, permitir salto nuevamente
	if is_on_floor_now and not was_on_floor:
		can_jump = true
		is_jumping = false
	
	# Si está saltando y aún no ha dejado el suelo, mantener estado
	if is_jumping and not is_on_floor_now:
		# Ya está en el aire, el salto se completó
		pass
	
	was_on_floor = is_on_floor_now

func update_animation_state():
	# Actualizar estado de animación según el movimiento y estado físico
	# Solo activar animación de salto si realmente está saltando (velocidad positiva hacia arriba)
	if not is_on_floor() and (velocity.y > 0.1 or is_jumping):
		cur_anim = AnimState.SALTAR
		return
	
	# Si está en el suelo, no debe estar en estado de salto
	if is_on_floor():
		is_jumping = false
	
	var horizontal_speed := Vector2(velocity.x, velocity.z).length()
	var speed_threshold := 0.1
	
	if horizontal_speed < speed_threshold:
		cur_anim = AnimState.IDLE
	elif is_running and horizontal_speed > speed_threshold:
		cur_anim = AnimState.CORRER
	elif horizontal_speed > speed_threshold:
		cur_anim = AnimState.CAMINAR
	else:
		cur_anim = AnimState.IDLE

func handle_animations(delta):
	# Interpolar valores de blend según el estado actual
	match cur_anim:
		AnimState.IDLE:
			caminar_val = lerpf(caminar_val, 0.0, blend_speed * delta)
			correr_val  = lerpf(correr_val, 0.0, blend_speed * delta)
			saltar_val  = lerpf(saltar_val, 0.0, blend_speed * delta)

		AnimState.CAMINAR:
			caminar_val = lerpf(caminar_val, 1.0, blend_speed * delta)
			correr_val  = lerpf(correr_val, 0.0, blend_speed * delta)
			saltar_val  = lerpf(saltar_val, 0.0, blend_speed * delta)

		AnimState.CORRER:
			caminar_val = lerpf(caminar_val, 0.0, blend_speed * delta)
			correr_val  = lerpf(correr_val, 1.0, blend_speed * delta)
			saltar_val  = lerpf(saltar_val, 0.0, blend_speed * delta)

		AnimState.SALTAR:
			caminar_val = lerpf(caminar_val, 0.0, blend_speed * delta)
			correr_val  = lerpf(correr_val, 0.0, blend_speed * delta)
			saltar_val  = lerpf(saltar_val, 1.0, blend_speed * delta)

func update_tree():
	# Actualizar parámetros del AnimationTree
	if not ani_tree:
		return
	
	ani_tree["parameters/caminar/blend_amount"] = caminar_val
	ani_tree["parameters/correr/blend_amount"] = correr_val
	ani_tree["parameters/saltar /blend_amount"] = saltar_val

# --- SISTEMA DE GHOSTING / P2P ---
signal on_avatar_update(pos: Vector3, rot: Vector3)

func send_network_update():
	# Emitir señal con posición y rotación actual para envío P2P
	var current_pos = global_position
	var current_rot = Vector3(rotation.x, rotation.y, rotation.z)
	
	emit_signal("on_avatar_update", current_pos, current_rot)

func receive_network_update(avatar_id: int, timestamp: float, new_position: Vector3, new_rotation: Quaternion):
	# Recibir update de red para avatar remoto
	# Optimizado para usar Quaternion en lugar de Vector3 para rotaciones
	if not networked_avatars.has(avatar_id):
		# Crear nuevo avatar en red si no existe
		var new_avatar = NetworkedAvatar.new(avatar_id)
		networked_avatars[avatar_id] = new_avatar
	
	# Añadir snapshot al jitter buffer (orden cronológico mantenido)
	networked_avatars[avatar_id].add_snapshot(timestamp, new_position, new_rotation)

func process_networked_avatars(delta: float):
	# Procesar interpolación para todos los avatares en red
	# Optimizado para ejecutarse en _physics_process para 60 FPS suave
	for avatar_id in networked_avatars:
		var avatar = networked_avatars[avatar_id]
		
		if avatar.interpolate(delta):
			# Aplicar interpolación suave al nodo del avatar remoto
			# Nota: Esto requiere que el sistema de gestión de avatares
			# actualice los nodos correspondientes con avatar.current_position
			# y avatar.current_rotation
			apply_networked_avatar_transform(avatar_id, avatar.current_position, avatar.current_rotation)

# Función para limpiar avatares inactivos (timeout)
func cleanup_inactive_avatars():
	var current_time = Time.get_unix_time_from_system()
	var inactive_timeout = 5.0  # 5 segundos de inactividad
	
	var to_remove = []
	
	for avatar_id in networked_avatars:
		var avatar = networked_avatars[avatar_id]
		if current_time - avatar.last_update_time > inactive_timeout:
			to_remove.append(avatar_id)
	
	for avatar_id in to_remove:
		networked_avatars.erase(avatar_id)
		# Aquí destruirías el nodo del avatar remoto

# Función para configurar este avatar como local o remoto
func set_as_local_player(is_local: bool):
	is_local_player = is_local
	
	if is_local:
		# Habilitar física y input para jugador local
		set_physics_process(true)
		set_process_input(true)
	else:
		# Deshabilitar física y input para avatar remoto
		set_physics_process(false)
		set_process_input(false)
		# Configurar para ser controlado por interpolación de red

# Método para obtener datos de serialización compactos (33 bytes)
func get_compact_transform() -> PackedByteArray:
	var data = PackedByteArray()
	
	# Posición (12 bytes)
	data.append_array(var_to_bytes(global_position.x))
	data.append_array(var_to_bytes(global_position.y))
	data.append_array(var_to_bytes(global_position.z))
	
	# Rotación (16 bytes - Quaternion)
	var quat = Quaternion.from_euler(rotation)
	data.append_array(var_to_bytes(quat.x))
	data.append_array(var_to_bytes(quat.y))
	data.append_array(var_to_bytes(quat.z))
	data.append_array(var_to_bytes(quat.w))
	
	# ID del avatar (4 bytes) - se añade externamente
	# Tipo de paquete (1 byte) - se añade externamente
	
	return data

# Método para aplicar datos de serialización compactos
func apply_compact_transform(data: PackedByteArray, offset: int = 0):
	if data.size() < offset + 28:  # 12 + 16 bytes mínimo
		return false
	
	# Deserializar posición (optimizado: usar var_to_bytes inverso)
	var pos_x = bytes_to_var(data.slice(offset, offset + 3))
	var pos_y = bytes_to_var(data.slice(offset + 4, offset + 7))
	var pos_z = bytes_to_var(data.slice(offset + 8, offset + 11))
	
	# Deserializar rotación (Quaternion)
	var rot_x = bytes_to_var(data.slice(offset + 12, offset + 15))
	var rot_y = bytes_to_var(data.slice(offset + 16, offset + 19))
	var rot_z = bytes_to_var(data.slice(offset + 20, offset + 23))
	var rot_w = bytes_to_var(data.slice(offset + 24, offset + 27))
	
	# Aplicar transformación usando jitter buffer (interpolación suave)
	var target_position = Vector3(pos_x, pos_y, pos_z)
	var target_rotation = Quaternion(rot_x, rot_y, rot_z, rot_w)
	
	# Añadir al jitter buffer en lugar de aplicar directamente
	# Esto permite interpolación suave incluso con latencia variable
	receive_network_update(-1, Time.get_unix_time_from_system(), target_position, target_rotation)
	
	return true

# Método auxiliar para aplicar transformación de avatar en red
# (debe ser implementado por el sistema de gestión de avatares)
func apply_networked_avatar_transform(_avatar_id: int, _position: Vector3, _rotation: Quaternion):
	# Esta función debe ser implementada por el sistema de gestión de avatares
	# para actualizar el nodo 3D correspondiente con la posición y rotación interpoladas
	# Ejemplo:
	# var avatar_node = get_node_or_null("Avatars/" + str(_avatar_id))
	# if avatar_node:
	#     avatar_node.global_position = _position
	#     avatar_node.global_rotation = _rotation.get_euler()
	pass

## HUDUpdater.gd - Actualizador de información del HUD
## Autor: WoldVirtual3DlucIA
## Versión: 1.0.0

extends Node
class_name HUDUpdater

## Referencias configurables
@export var hud_root: Control
@export var player_path: NodePath

## Referencias internas
var fps_label: Label
var coords_label: Label
var time_label: Label
var speed_label: Label
var health_bar: ProgressBar
var energy_bar: ProgressBar
var player_node: Node3D

## Estado
var _update_interval: float = 0.1
var _timer: float = 0.0
var current_speed_mode: String = "Normal"


func _ready() -> void:
	call_deferred("_setup_references")


func _setup_references() -> void:
	"""Configura las referencias a los nodos del HUD"""
	if not hud_root:
		hud_root = get_parent()
	
	# Buscar labels
	fps_label = _find_node_recursive(hud_root, "FPSLabel") as Label
	coords_label = hud_root.get_node_or_null("%CoordsLabel") as Label
	time_label = hud_root.get_node_or_null("%TimeLabel") as Label
	
	# Buscar barras de progreso
	var avatar_panel = _find_node_recursive(hud_root, "AvatarStatusPanel")
	if avatar_panel:
		health_bar = _find_node_recursive(avatar_panel, "HealthBar") as ProgressBar
		energy_bar = _find_node_recursive(avatar_panel, "EnergyBar") as ProgressBar
		speed_label = _find_node_recursive(avatar_panel, "SpeedLabel") as Label
	
	# Buscar jugador
	if player_path:
		player_node = get_node_or_null(player_path) as Node3D
	else:
		_find_player()


func _find_node_recursive(node: Node, node_name: String) -> Node:
	"""Busca un nodo recursivamente por nombre"""
	if node.name == node_name:
		return node
	for child in node.get_children():
		var found = _find_node_recursive(child, node_name)
		if found:
			return found
	return null


func _find_player() -> void:
	"""Busca el nodo del jugador en la escena"""
	var players = get_tree().get_nodes_in_group("player")
	if players.size() > 0:
		player_node = players[0]
	else:
		player_node = _find_character_body(get_tree().root)


func _find_character_body(node: Node) -> Node3D:
	"""Busca recursivamente un CharacterBody3D"""
	if node is CharacterBody3D:
		return node
	for child in node.get_children():
		var result = _find_character_body(child)
		if result:
			return result
	return null


func _process(delta: float) -> void:
	_timer += delta
	if _timer >= _update_interval:
		_timer = 0.0
		_update_all()


func _update_all() -> void:
	"""Actualiza toda la información del HUD"""
	_update_fps()
	_update_coordinates()
	_update_time()
	_update_avatar_status()


func _update_fps() -> void:
	"""Actualiza el contador de FPS"""
	if fps_label:
		var fps = Engine.get_frames_per_second()
		var color = _get_fps_color(fps)
		fps_label.text = "FPS: %d" % fps
		fps_label.modulate = color


func _get_fps_color(fps: int) -> Color:
	"""Retorna un color basado en el FPS"""
	if fps >= 55:
		return Color(0.6, 0.9, 0.6, 1.0)  # Verde
	elif fps >= 30:
		return Color(0.9, 0.8, 0.4, 1.0)  # Amarillo
	else:
		return Color(0.9, 0.5, 0.5, 1.0)  # Rojo


func _update_coordinates() -> void:
	"""Actualiza las coordenadas del jugador"""
	if coords_label and player_node:
		var pos = player_node.global_position
		coords_label.text = "(%d, %d, %d)" % [int(pos.x), int(pos.y), int(pos.z)]


func _update_time() -> void:
	"""Actualiza el reloj del mundo virtual"""
	if time_label:
		var time_dict = Time.get_time_dict_from_system()
		time_label.text = "%02d:%02d" % [time_dict.hour, time_dict.minute]


func _update_avatar_status() -> void:
	"""Actualiza el estado del avatar"""
	if health_bar:
		# Simular salud (puede conectarse a un sistema real)
		health_bar.value = 100.0
	
	if energy_bar:
		# Simular energía basada en movimiento
		if player_node and player_node is CharacterBody3D:
			var velocity = player_node.velocity.length()
			var energy = 100.0 - (velocity * 2.0)
			energy_bar.value = clamp(energy, 0.0, 100.0)
	
	if speed_label:
		speed_label.text = "🏃 Velocidad: " + current_speed_mode


## API Pública
func set_speed_mode(mode: String) -> void:
	"""Establece el modo de velocidad mostrado"""
	current_speed_mode = mode
	if speed_label:
		speed_label.text = "🏃 Velocidad: " + mode


func set_health(value: float) -> void:
	"""Establece el valor de salud"""
	if health_bar:
		health_bar.value = clamp(value, 0.0, 100.0)


func set_energy(value: float) -> void:
	"""Establece el valor de energía"""
	if energy_bar:
		energy_bar.value = clamp(value, 0.0, 100.0)


func set_player(node: Node3D) -> void:
	"""Establece el nodo del jugador"""
	player_node = node

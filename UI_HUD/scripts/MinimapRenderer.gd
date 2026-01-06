## MinimapRenderer.gd - Renderizador de mini-mapa estilo Second Life
## Autor: WoldVirtual3DlucIA
## Versión: 1.0.0

extends Panel
class_name MinimapRenderer

## Configuración del mini-mapa
@export var map_scale: float = 0.5
@export var map_size: float = 256.0
@export var rotation_follows_camera: bool = true
@export var show_grid: bool = true
@export var grid_size: float = 32.0

## Colores del mapa
const COLOR_TERRAIN: Color = Color(0.15, 0.35, 0.25, 1.0)
const COLOR_WATER: Color = Color(0.1, 0.25, 0.45, 0.8)
const COLOR_PLAYER: Color = Color(0.2, 0.9, 0.4, 1.0)
const COLOR_GRID: Color = Color(0.3, 0.5, 0.7, 0.2)
const COLOR_NORTH: Color = Color(0.9, 0.3, 0.3, 0.8)

## Referencias
var player_node: Node3D = null
var terrain_node: Node3D = null
var player_marker: Label = null

## Estado interno
var _center_offset: Vector2 = Vector2.ZERO
var _player_rotation: float = 0.0
var _zoom_level: float = 1.0


func _ready() -> void:
	_find_player()
	_find_terrain()
	_setup_marker()
	queue_redraw()


func _process(_delta: float) -> void:
	if player_node:
		_update_player_position()
		queue_redraw()


func _draw() -> void:
	var rect_size = size
	var center = rect_size / 2.0
	
	# Fondo del mapa (agua)
	draw_rect(Rect2(Vector2.ZERO, rect_size), COLOR_WATER)
	
	# Dibujar terreno simplificado (isla circular)
	_draw_terrain(center, rect_size)
	
	# Dibujar grid
	if show_grid:
		_draw_grid(center, rect_size)
	
	# Dibujar indicador norte
	_draw_north_indicator(center)
	
	# Dibujar borde del mapa
	_draw_border(rect_size)


func _draw_terrain(center: Vector2, rect_size: Vector2) -> void:
	"""Dibuja una representación simplificada del terreno"""
	var island_radius = min(rect_size.x, rect_size.y) * 0.35
	
	# Isla principal (forma irregular)
	var points: PackedVector2Array = []
	var num_points = 24
	for i in range(num_points):
		var angle = (float(i) / num_points) * TAU
		var noise_offset = sin(angle * 3.0) * 0.15 + cos(angle * 5.0) * 0.1
		var radius = island_radius * (1.0 + noise_offset)
		var point = center + Vector2(cos(angle), sin(angle)) * radius
		points.append(point)
	
	# Color con gradiente simulado
	draw_colored_polygon(points, COLOR_TERRAIN)
	
	# Contorno de costa
	var coast_color = Color(0.6, 0.5, 0.35, 0.6)
	for i in range(points.size()):
		var next_i = (i + 1) % points.size()
		draw_line(points[i], points[next_i], coast_color, 2.0, true)


func _draw_grid(center: Vector2, rect_size: Vector2) -> void:
	"""Dibuja la cuadrícula del mapa"""
	var grid_spacing = grid_size * map_scale * _zoom_level
	var half_width = rect_size.x / 2.0
	var half_height = rect_size.y / 2.0
	
	# Líneas verticales
	var x = fmod(_center_offset.x * map_scale, grid_spacing)
	while x < rect_size.x:
		draw_line(Vector2(x, 0), Vector2(x, rect_size.y), COLOR_GRID, 1.0)
		x += grid_spacing
	
	# Líneas horizontales
	var y = fmod(_center_offset.y * map_scale, grid_spacing)
	while y < rect_size.y:
		draw_line(Vector2(0, y), Vector2(rect_size.x, y), COLOR_GRID, 1.0)
		y += grid_spacing


func _draw_north_indicator(center: Vector2) -> void:
	"""Dibuja el indicador de dirección norte"""
	var indicator_pos = Vector2(size.x - 15, 15)
	var indicator_size = 10.0
	
	# Flecha norte
	var north_points: PackedVector2Array = [
		indicator_pos + Vector2(0, -indicator_size),
		indicator_pos + Vector2(-indicator_size * 0.5, indicator_size * 0.5),
		indicator_pos + Vector2(indicator_size * 0.5, indicator_size * 0.5)
	]
	draw_colored_polygon(north_points, COLOR_NORTH)
	
	# Letra N
	draw_string(ThemeDB.fallback_font, indicator_pos + Vector2(-4, -indicator_size - 2), 
		"N", HORIZONTAL_ALIGNMENT_CENTER, -1, 9, COLOR_NORTH)


func _draw_border(rect_size: Vector2) -> void:
	"""Dibuja el borde decorativo del mapa"""
	var border_color = Color(0.3, 0.5, 0.75, 0.6)
	draw_rect(Rect2(Vector2.ZERO, rect_size), border_color, false, 2.0)


func _find_player() -> void:
	"""Busca el nodo del jugador"""
	await get_tree().process_frame
	var players = get_tree().get_nodes_in_group("player")
	if players.size() > 0:
		player_node = players[0]
	else:
		var root = get_tree().root
		player_node = _find_character_body(root)


func _find_character_body(node: Node) -> Node3D:
	"""Busca recursivamente un CharacterBody3D"""
	if node is CharacterBody3D:
		return node
	for child in node.get_children():
		var result = _find_character_body(child)
		if result:
			return result
	return null


func _find_terrain() -> void:
	"""Busca el nodo del terreno"""
	await get_tree().process_frame
	terrain_node = get_tree().root.find_child("Terrain3D", true, false)


func _setup_marker() -> void:
	"""Configura el marcador del jugador"""
	player_marker = get_node_or_null("PlayerMarker")


func _update_player_position() -> void:
	"""Actualiza la posición del marcador del jugador"""
	if player_node and player_marker:
		# Obtener rotación del jugador para el indicador
		_player_rotation = player_node.rotation.y
		
		# Rotar el marcador
		player_marker.rotation = _player_rotation


## API Pública
func set_zoom(level: float) -> void:
	"""Establece el nivel de zoom del mapa"""
	_zoom_level = clamp(level, 0.5, 3.0)
	queue_redraw()


func zoom_in() -> void:
	"""Acerca el mapa"""
	set_zoom(_zoom_level * 1.2)


func zoom_out() -> void:
	"""Aleja el mapa"""
	set_zoom(_zoom_level / 1.2)


func set_player(node: Node3D) -> void:
	"""Establece el nodo del jugador"""
	player_node = node

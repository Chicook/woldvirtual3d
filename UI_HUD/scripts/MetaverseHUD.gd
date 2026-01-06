## MetaverseHUD.gd - Sistema de HUD profesional estilo Second Life/OpenSimulator
## Autor: WoldVirtual3DlucIA
## Versión: 1.0.0

extends Control
class_name MetaverseHUD

## Señales para eventos del HUD
signal inventory_toggled(visible: bool)
signal chat_toggled(visible: bool)
signal map_toggled(visible: bool)
signal settings_toggled(visible: bool)
signal build_mode_toggled(enabled: bool)

## Referencias a nodos del HUD
@onready var top_bar: PanelContainer = $TopBar
@onready var bottom_bar: PanelContainer = $BottomBar
@onready var minimap_panel: PanelContainer = $MinimapPanel
@onready var avatar_panel: PanelContainer = $AvatarStatusPanel
@onready var location_label: Label = $TopBar/HBoxContainer/LocationLabel
@onready var region_label: Label = $TopBar/HBoxContainer/RegionLabel
@onready var coords_label: Label = $TopBar/HBoxContainer/CoordsLabel
@onready var time_label: Label = $TopBar/HBoxContainer/TimeLabel

## Referencia al jugador
var player_node: Node3D = null
var current_region_name: String = "Isla Virtual"
var world_name: String = "WoldVirtual3D"

## Estado del HUD
var is_inventory_open: bool = false
var is_chat_open: bool = false
var is_map_open: bool = false
var is_build_mode: bool = false
var hud_visible: bool = true

## Configuración visual
const FADE_DURATION: float = 0.25
const UPDATE_INTERVAL: float = 0.1

var _update_timer: float = 0.0


func _ready() -> void:
	_setup_hud()
	_connect_signals()
	print("[MetaverseHUD] Sistema HUD iniciado")


func _process(delta: float) -> void:
	_update_timer += delta
	if _update_timer >= UPDATE_INTERVAL:
		_update_timer = 0.0
		_update_hud_info()


func _setup_hud() -> void:
	"""Configura el HUD inicial"""
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	_find_player()


func _find_player() -> void:
	"""Busca el nodo del jugador en la escena"""
	await get_tree().process_frame
	var players = get_tree().get_nodes_in_group("player")
	if players.size() > 0:
		player_node = players[0]
	else:
		# Buscar por tipo CharacterBody3D
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


func _connect_signals() -> void:
	"""Conecta señales de los botones del HUD"""
	# Las conexiones se harán desde la escena
	pass


func _update_hud_info() -> void:
	"""Actualiza la información mostrada en el HUD"""
	_update_location_info()
	_update_time_display()


func _update_location_info() -> void:
	"""Actualiza información de ubicación"""
	if location_label:
		location_label.text = world_name
	
	if region_label:
		region_label.text = current_region_name
	
	if coords_label and player_node:
		var pos = player_node.global_position
		coords_label.text = "(%d, %d, %d)" % [int(pos.x), int(pos.y), int(pos.z)]


func _update_time_display() -> void:
	"""Actualiza el reloj del mundo virtual"""
	if time_label:
		var dict = Time.get_time_dict_from_system()
		time_label.text = "%02d:%02d" % [dict.hour, dict.minute]


## API Pública
func set_player(node: Node3D) -> void:
	"""Establece el nodo del jugador"""
	player_node = node


func set_region_name(name: String) -> void:
	"""Establece el nombre de la región actual"""
	current_region_name = name


func set_world_name(name: String) -> void:
	"""Establece el nombre del mundo"""
	world_name = name


func toggle_hud_visibility() -> void:
	"""Alterna la visibilidad del HUD completo"""
	hud_visible = !hud_visible
	var tween = create_tween()
	tween.tween_property(self, "modulate:a", 1.0 if hud_visible else 0.0, FADE_DURATION)


func show_notification(message: String, duration: float = 3.0) -> void:
	"""Muestra una notificación temporal"""
	# Implementación básica - puede expandirse
	print("[Notificación] " + message)


## Handlers de botones
func _on_inventory_pressed() -> void:
	is_inventory_open = !is_inventory_open
	emit_signal("inventory_toggled", is_inventory_open)


func _on_chat_pressed() -> void:
	is_chat_open = !is_chat_open
	emit_signal("chat_toggled", is_chat_open)


func _on_map_pressed() -> void:
	is_map_open = !is_map_open
	emit_signal("map_toggled", is_map_open)


func _on_build_pressed() -> void:
	is_build_mode = !is_build_mode
	emit_signal("build_mode_toggled", is_build_mode)


func _on_settings_pressed() -> void:
	emit_signal("settings_toggled", true)


func _unhandled_key_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed:
		match event.keycode:
			KEY_I:
				_on_inventory_pressed()
			KEY_ENTER:
				_on_chat_pressed()
			KEY_M:
				_on_map_pressed()
			KEY_B:
				_on_build_pressed()
			KEY_TAB:
				toggle_hud_visibility()

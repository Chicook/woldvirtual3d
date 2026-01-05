extends Node

# Island Manager
# Responsabilidad: Gestionar el ciclo de vida de la isla local y generar el WorldID.

const ISLAND_SCENE_PATH = "WoldVirtualv01.ADMIN.BT\rprc_3d.tscn"  # Ajustado a recursos locales en BSINIMTVRS

var current_world_id = ""

func _ready():
	print("IslandManager: Iniciando...")
	# Simular inicio de sesión: Cargar isla privada inmediatamente
	load_world()  # Cargar al iniciar sesión
	current_world_id = _generate_world_id()
	print("IslandManager: WorldID generado basado en hardware: ", current_world_id)
	
	# Timer para ghosting de avatar (30ms)
	# var avatar_timer = Timer.new()
	# avatar_timer.timeout.connect(_on_avatar_timer_timeout)
	# avatar_timer.wait_time = 0.03  # 30ms
	# add_child(avatar_timer)
	# avatar_timer.start()

func load_world():
	print("IslandManager: Comando de carga recibido.")
	if get_tree().current_scene.scene_file_path != ISLAND_SCENE_PATH:
		_load_private_island_scene_change()
	else:
		print("IslandManager: Ya estamos en la isla.")

func _load_private_island_scene_change():
	print("IslandManager: Cambiando a escena de isla privada: ", ISLAND_SCENE_PATH)
	if ResourceLoader.exists(ISLAND_SCENE_PATH):
		get_tree().change_scene_to_file(ISLAND_SCENE_PATH)
		# El print de 'Scene loaded' ocurrirá en el _ready de la nueva escena (o del Autoload al recargar?)
		# Autoloads persisten. Así que _ready NO se ejecuta de nuevo.
		# Necesitamos detectar el cambio de escena.
		await get_tree().create_timer(0.5).timeout
		print("IslandManager: Scene loaded: ", ISLAND_SCENE_PATH)
	else:
		printerr("IslandManager ERROR: No se encuentra la escena en ", ISLAND_SCENE_PATH)

func _generate_world_id() -> String:
	# Genera un ID único basado en el hardware y OS
	# Formato: OS_ID + HASH_PARTIAL
	var os_id = OS.get_unique_id()
	if os_id == null or os_id == "":
		os_id = "UNKNOWN_DEVICE"
	
	# Añadimos un timestamp para asegurar unicidad temporal si es necesario, 
	# aunque el usuario pidió basado en hardware.
	# Para "Dueño y Host", el hardware ID es bueno.
	var raw_id = os_id + OS.get_model_name()
	var final_id = raw_id.sha256_text().substr(0, 16).to_upper()
	
	return final_id

# Función para obtener el ID actual
# func get_world_id():
# 	return current_world_id\n\nfunc _on_avatar_timer_timeout():\n    # Asumir que hay un nodo avatar en la escena\n    var avatar = get_node_or_null(\"Avatar\")  # Ajustar path según estructura de escena\n    if avatar:\n        var pos = avatar.global_transform.origin\n        var rot = avatar.global_transform.basis.get_rotation_quaternion()\n        # Enviar a C# vía BridgeService\n        var bridge = get_node(\"/root/BridgeService\")\n        if bridge:\n            bridge.send_cmd(\"SendAvatarUpdate\", {\n                \"id\": 1,  # ID de avatar local\n                \"x\": pos.x, \"y\": pos.y, \"z\": pos.z,\n                \"rx\": rot.x, \"ry\": rot.y, \"rz\": rot.z, \"rw\": rot.w,\n                \"targetEp\": null  # Asumir targetEp desde conexión actual\n            })\n        else:\n            printerr(\"IslandManager: BridgeService no encontrado\")\n    else:\n        printerr(\"IslandManager: Avatar no encontrado\")

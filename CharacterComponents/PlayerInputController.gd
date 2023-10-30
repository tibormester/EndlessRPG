class_name PlayerInputController extends CharacterComponent

##USE SIGNALS TO SEND INPUTS->ACTIONS

var vel = Vector3.ZERO
var dir = Vector3.ZERO

@onready var p = get_parent()

func reset_stats():
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	stats = {
		"Sprinting" = false,
		"Mouse Sensitivitiy" = -0.05
	}
	

#Class that intercepts user inputs and translates it into actions and commands
#for other Character components, for example WASD becomes relative movement commands for HumanoidMovement Component
func _process(delta):
	process_input(delta)

func process_input(delta):
	var direction = Vector2.ZERO
	dir = Vector3.ZERO
	#-----
	#Walking
	#-----
	var cam_xform = p.stats.get("Cameras").get(p.stats.get("Camera")).global_transform
	if Input.is_action_pressed("Move Forward"):
		direction.y += 1
	if Input.is_action_pressed("Move Back"):
		direction.y -= 1
	if Input.is_action_pressed("Move Left"):
		direction.x -= 1
	if Input.is_action_pressed("Move Right"):
		direction.x += 1
	if direction != Vector2.ZERO:
		direction = direction.normalized()
		
	dir += cam_xform.basis.x * direction.x
	dir += -cam_xform.basis.z * direction.y
	#Send the signal to the movement component with the calculated direction
	if dir.length() != 0:
		p.calling_action.emit("Move Relative", dir)
	
	#-----
	#Toggle Sprinting while shifting, Alters this directly....
	if Input.is_action_just_pressed("Sprint"):
		p.stats["Sprinting"] = true
	if Input.is_action_just_released("Sprint"):
		p.stats["Sprinting"] = false
	#-----
	
	#Jumping
	#-----
	if Input.is_action_pressed("Jump") and (p.is_on_floor() or p.is_on_wall()):
		#If the character can jump, try to jump.... maybe these checks should be in movement controller??
		p.calling_action.emit("Jump")
	#-----
	
	#Freeing Cursor on esc
	#-----
	if Input.is_action_just_pressed("ui_cancel"):
		if Input.mouse_mode == Input.MOUSE_MODE_VISIBLE:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
		else:
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	#-----
	
		
func _input(event):
	if event.is_action_pressed("Display Stats"):
		p.calling_action.emit("Display Stats")
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		p.calling_action.emit("Look Relative", event.relative * p.stats.get("Mouse Sensitivity", -0.05))
	#Cycling through cameras...
	#CHANGE THIS TO CHECK STATS AND ACTUALLY CYCLE THROUGH THE CHARACTERS CAMERAS!!!!
	if event.is_action_pressed("Change Camera"):
		get_viewport().get_camera_3d().current = false
		
	if p.stats.get("Zoom") != null :
		if event.is_action("Zoom In"):
			p.stats.get("Zoom").spring_length -= .3
		if event.is_action("Zoom Out"):
			p.stats.get("Zoom").spring_length += .3
		p.stats.get("Zoom").spring_length = clampf(p.stats.get("Zoom").spring_length, p.stats.get("Max Zoom In", 0.0), p.stats.get("Max Zoom Out", 0.0))
		
##COPY and Paster
#var camera : Camera3D # = p.stats.get("Cameras").get(p.stats.get("Camera")) : Camera3D
#var look_dir : Vector3
#var camera_sens
#var sens_mod
#var mouse_captured = true
#func capture_mouse() -> void:
#	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
#	mouse_captured = true
#
#func release_mouse() -> void:
#	Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
#	mouse_captured = false
#
#func _rotate_camera(sens_mod: float = 1.0) -> void:
#	camera.rotation.y -= look_dir.x * camera_sens * sens_mod
#	camera.rotation.x = clamp(camera.rotation.x - look_dir.y * camera_sens * sens_mod, -1.5, 1.5)
#
#func _unhandled_input(event: InputEvent) -> void:
#	if event is InputEventMouseMotion:
#		look_dir = event.relative * 0.001
#		if mouse_captured: _rotate_camera()
#	if Input.is_action_just_pressed("jump"): pass #jumping = true
#	if Input.is_action_just_pressed("exit"): get_tree().quit()


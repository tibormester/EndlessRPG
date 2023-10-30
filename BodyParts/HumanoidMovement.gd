class_name HumanoidMovement extends CharacterComponent

#Listen for Movement Action Signals and then translate it into movement
#Needs to translate commands/actions to proper motions

var vel = Vector3.ZERO
var dir = Vector3.ZERO

@onready var p = get_parent()

func reset_stats():
	stats = {
		"Sprinting" = false,
	}
func reset_actions():
	actions = {
		"Look Relative" = look_relative,
		"Move Relative" = move_relative,
		"Jump" = jump,
	}


	
func look_relative(direction : Vector2):
	p.stats.get("X Rotator", p).rotate_x(deg_to_rad(direction.y))
	p.rotate_y(deg_to_rad(direction.x))
		
	var camera_rot = p.stats.get("X Rotator", p).rotation_degrees
	camera_rot.x = clamp(camera_rot.x, -1*p.stats.get("Max View Decline", 0.0), p.stats.get("Max View Incline", 0.0))
	p.stats.get("X Rotator", p).rotation_degrees = camera_rot
	
func move_relative(direction : Vector3):
	dir = direction
	
func jump():
	if p.is_on_floor():
		vel.y = p.stats.get("Jump Speed")
	
func process_movement(delta):
	dir.y = 0
	dir = dir.normalized()
	
	if not p.is_on_floor():
		vel.y += delta * p.stats.get("Gravity", 0.0)
	#DEFINE Vertical-Independent  Velocity
	var hvel = vel
	hvel.y = 0
	
	var target = dir
	if p.stats.get("Sprinting", false):
		target *= p.stats.get("Sprint Speed", 0.0)
	else:
		target *= p.stats.get("Run Speed", 0.0)
	
	var accel
	if dir.dot(hvel) > 0:
		if p.stats.get("Sprinting", false):
			accel = p.stats.get("Sprint Acceleration", 0.0)
			p.calling_action.emit("Running", accel)#, p.stats.get("Sprint Speed", 0.0))
		else:
			accel = p.stats.get("Acceleration", 0.0)
			p.calling_action.emit("Running", accel)#, p.stats.get("Run Speed", 0.0))
	else:
		accel = p.stats.get("Deacceleration", 0.0)
		p.calling_action.emit("Idling", accel)
		
		
	hvel = hvel.lerp(target, accel * delta)
	vel.x = hvel.x
	vel.z = hvel.z
	p.velocity = vel
	p.move_and_slide()
	
func _process(delta):
	process_movement(delta)
	dir = Vector3.ZERO
	

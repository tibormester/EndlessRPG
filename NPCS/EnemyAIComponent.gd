class_name EnemyAIComponent extends CharacterComponent

var target : Character
@onready var p : Character

# Called when the node enters the scene tree for the first time.
func _ready():
	p = get_parent()
	target = get_tree().current_scene.get_child(0)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	var relative_target_position =  (Vector2(target.position.x,target.position.z)) - Vector2(p.position.x,p.position.z)
#	var angle = p.rotation
	p.look_at(target.global_position, Vector3(0,1,0))
	#var delta_angle = p.rotation
	#delta_angle -= angle
	#p.rotation = angle
	#p.calling_action.emit("Look Relative", Vector2( delta * Vector2(delta_angle.x, delta_angle.z)))
	
	var distance = relative_target_position.length()
	if distance < 40.0 and 6 < distance :
		p.calling_action.emit("Move Relative", Vector3(relative_target_position.x, 0.0, relative_target_position.y).limit_length(delta))

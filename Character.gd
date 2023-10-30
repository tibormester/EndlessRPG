class_name Character extends CharacterBody3D

signal updating_stats(stat_block : Dictionary)
signal calling_action(action : String, params)

@export var stats : Dictionary
@export var actions : Dictionary

# Called when the node enters the scene tree for the first time.
func _ready():
	#Sets Default stats
	reset_stats()
	#connects each child to the updating stats function
	for child in get_children():
		if child is BodyPart or child is CharacterComponent :
			updating_stats.connect(Callable(child, "_on_updating_stats"))
			calling_action.connect(Callable(child, "_on_calling_action"))

	#get the updated stats 
	updating_stats.emit(stats)
	#Change the collision box to be halfway between the torso and the feet with height to span the distance
	var h = stats.get("Feet Height",{}).get("Heights",[1.0]).max()
	$CollisionShape3D.shape.radius = stats.get("Feet Size", 0.1)
	$CollisionShape3D.translate(Vector3(0.0,-0.5 * h, 0.0))
	$CollisionShape3D.shape.height =  h
	

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
	#reset_stats()
	#updating_stats.emit(stats)

#Override with default stat values
func reset_stats():
	stats = {
		"Gravity" = -35.0,
		"Feet Height" = {
			"Level" = 0,
			"Heights" = [0.0],
			"Height" = 0.0,
		},
	}

class_name Torso extends BodyPart

@onready var neck = preload("res://BodyParts/Head.tscn").instantiate()
@onready var left_shoulder =  preload("res://BodyParts/Arm.tscn").instantiate()
@onready var right_shoulder = preload("res://BodyParts/Arm.tscn").instantiate()
@onready var left_thigh = preload("res://BodyParts/Leg.tscn").instantiate()
@onready var right_thigh = preload("res://BodyParts/Leg.tscn").instantiate()
@onready var equipped = preload("res://Equipment/Sword.tscn").instantiate()


func reset_stats():
	#Add all bodypart scenes to the tree at the correct joint
	right_shoulder.hand = equipped
	$Neck.add_child(neck)
	$LeftShoulder.add_child(left_shoulder)
	$RightShoulder.add_child(right_shoulder)
	$LeftThigh.add_child(left_thigh)
	$RightThigh.add_child(right_thigh)
	stats = {
		#Health component Stats
		"Max Health" = 30.0,
		"Health" = 30.0,
		#Hitbox Component Stats
		"Feet Height" = {
			"Level" = 2,
			"Heights" = [.75],
			"Height" = .75,
		},
	}
func reset_actions():
	actions = {
		"Running" = func lambda(speed):$AnimationPlayer.play("Walking", 0.05, speed / 10.0),
		"Idling" = func lambda(speed): $AnimationPlayer.play("RESET", 0.35, speed / 10.0),
	}

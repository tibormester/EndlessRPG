class_name GoblinTorso extends Torso



func reset_stats():
	neck = preload("res://NPCS/Goblin/GoblinHead.tscn").instantiate()
	left_shoulder =  preload("res://NPCS/Goblin/GoblinArm.tscn").instantiate()
	right_shoulder = preload("res://NPCs/Goblin/GoblinArm.tscn").instantiate()
	left_thigh = preload("res://NPCs/Goblin/GoblinLeg.tscn").instantiate()
	right_thigh = preload("res://NPCs/Goblin/GoblinLeg.tscn").instantiate()
	equipped = preload("res://Equipment/Sword.tscn").instantiate()
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
		"Running" = func lambda(speed): $AnimationPlayer.play("Walking", 0.05, speed /1.0),
		"Idling" = func lambda(speed): $AnimationPlayer.play("RESET", 0.35, speed / 10.0),
		"Swing" = func lambda(speed): $AnimationPlayer.play("Swing", 0.5, speed / 10.0),
	}

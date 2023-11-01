class_name Arm extends BodyPart

@export var hand : Equipment

@onready var p = get_parent()

func _init(hand_slot = null):
	if hand_slot != null:
		hand = hand_slot


func reset_stats():
	if hand != null:
		$Hand.add_child(hand)
	stats = {
		#Health Component
		"Max Health" = 10,
		"Health" = 10.0,
	}
	
func reset_actions():
	actions = {
		"Swing" = func lambda():
			if hand == null:
				return
			$AnimationPlayer.play("Swing Out", 0.1, 10.0)
			await get_tree().create_timer(0.1).timeout
			$AnimationPlayer.play("Swing In", 0.3, 10.0)
			await get_tree().create_timer(0.3).timeout
			$AnimationPlayer.play("Idle", 0.3, 10.0)
			#await get_tree().create_timer(0.6).timeout
			,
		"Punch" = func lambda(): pass,
	}

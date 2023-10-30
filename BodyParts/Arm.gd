class_name Arm extends BodyPart

@export var hand : Equipment

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

class_name Sword extends Equipment


var damage = 500

func _on_body_entered(body):
	if true: #body is BodyPart:
		body.take_damage(damage)
	#body.queue_free()
	print_debug(body)

class_name Head extends BodyPart
	
func reset_stats():
	stats = {
		#Health Component Stats
		"Max Health" = 10,
		"Health" = 10.0,
		#Movement Component Stats
		"Max Zoom In" = 0,
		"Max Zoom Out" = 10,
		"Max View Decline" = 35,
		"Max View Incline" = 40,
		"X Rotator" = get_parent(),
		#Player Input Stats
		"Camera" = "Third Person",
		"Zoom" = $SpringArm3D,
		"Cameras" = {
			"First Person" = $Camera3D,
			"Third Person" = $SpringArm3D/ThirdPersonCamera,
		},
	}
#No Need to adjust how to respond to adding stats since everything is additive

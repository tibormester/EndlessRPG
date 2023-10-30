class_name Leg extends BodyPart

func reset_stats():
	stats = {
		#Health Component
		"Max Health" = 15,
		"Health" = 15.0,
		#Movement Component
		"Run Speed" = 10.0,
		"Jump Speed" = 6.0,
		"Acceleration" = 2.5,
		"Deacceleration" = 6.0,
		
		"Sprint Speed" = 20.0,
		"Sprint Acceleration" = 9.0,
		
		#Collision Component
		"Feet Size" = 0.1,
		"Feet Height" = {
			"Level" = 1,
			"Heights" = [1.3],
			"Height" = 1.3,
		},
		
	}

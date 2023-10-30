class_name HealthUIComponent extends CharacterComponent

@onready var p = get_owner()

func reset_stats():
	stats = {
		"Max Health" = 0,
		"Health" = 0.0,
		"Health Log" = {
			
		}
	}

func _process(delta):
	$HealthBar.text = str(p.stats.get("Health", 0.0)) + "/" + str(p.stats.get("Max Health", 0.0)) + " Health"

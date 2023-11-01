class_name BodyPart extends RigidBody3D

signal updating_stats(stat_block : Dictionary)
signal calling_action(action : String, params)

@export var stats : Dictionary
@export var actions : Dictionary
# Called when the node enters the scene tree for the first time.
func _ready():
	reset_stats()
	reset_actions()
	for child in get_children():
		if child is Marker3D and child.get_child_count() == 1:
			#This is for ignoring joins
			child = child.get_child(0)
		if child is BodyPart or child is CharacterComponent or child is Equipment:
			updating_stats.connect( Callable(child,"_on_updating_stats"))
			calling_action.connect( Callable(child,"_on_calling_action"))
			#.bind(stats) unecessary if we pass stats when we emit?
	#get the updated stats
	updating_stats.emit(stats)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass

func _on_calling_action(action : String, params = null):
	calling_action.emit(action, params)
	if action in actions.keys():
		if params != null:
			actions[action].bind(params).call()
		else:
			actions[action].call()

#Default updating stats behavior, Character sends signal updating stats with stat dictionary
#Any connected nodes will recieve, first query their connected children to update 
#then send their final updates last
#Adds all the stats in the current node to the dictionary either to the 
#existing value if int or float or replacement otherwise, if the stat value is a dictionary
#we recursively update that dictionary with our Dictionary
func _on_updating_stats(s : Dictionary, add = stats):
	updating_stats.emit(s)
	#Have to separate the emitting the signal from recursively calling updating stats or else get stack overflow
	add_stats(s, add)
			
#"Character" = {
#			"Level" = 0 ,
#			"Heights" = [0]
#			"Height" = 0,
#		},
#"Torso" = {
#			"Level" = 2,
#			"Heights" = [.75]
#			"Height" = .75,
#		},
#"Legs" = {
#			"Level" = 1,
#			"Heights" = [1.3]
#			"Height" = 1.3,
#		},
#Limbs have a level, feet at 1 and increasing as going up, character starts at level 0
#as we add current heights if current level < level current level = level
#if the level is <= to current level add another current height otherwise sum to each
		
func add_stats(s : Dictionary, add ):
	for stat in add:
		if stat in s:
			#Not perferct, ways for height to skip and bloated, consider changing to orientation + dimensions... maybe k-d trees?
			if stat == "Feet Height":
				#If the adding stat is lower or same level down add a leaf node of the highest height
				if s[stat].get("Level", 0) >= add[stat].get("Level", 0):
					s[stat]["Heights"].append(add[stat].get("Heights", [0.0]).max())
				#When the adding stat is higher up, incremenet all current leaves by the new level and increase the level to match
				else:
					s[stat]["Heights"] = s[stat].get("Heights", [0.0]).map(func increment(num): return num + add[stat].get("Height", 0.0))
					s[stat]["Level"] = add[stat]["Level"]
			elif s[stat] is int or s[stat] is float:
				s[stat] += add.get(stat)
			elif s[stat] is Dictionary and add[stat] is Dictionary:
				add_stats(s[stat], add[stat])
			else:
				s[stat] = add[stat]
		else:
			s[stat] = stats.get(stat)

#Override with default stat values
func reset_stats():
	stats = {}
#Override with default actions and their callables
func reset_actions():
	actions = {}
	
func take_damage(damage : float):
	if stats.has("Health"):
		stats["Health"] = stats.get("Health", 0.0) - damage
	if stats.get("Health") <= 0.0:
		queue_free()

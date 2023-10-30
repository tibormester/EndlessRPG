class_name CharacterComponent extends Node

#Default class for components attached to a character
signal updating_stats(stat_block : Dictionary)
signal calling_action(action : String, params)

@export var stats : Dictionary #"Statistic Name(DISPLAYABLE)" : Value
@export var actions : Dictionary #"Action Name" : Callable accepting params
	
# Called when the node enters the scene tree for the first time.
func _ready():
	reset_stats()
	reset_actions()
	for child in get_children():
		if child is Marker3D:
			child = child.get_child(0)
		if child is BodyPart or child is CharacterComponent or child is Equipment:
			updating_stats.connect( Callable(child,"_on_updating_stats"))
			calling_action.connect( Callable(child, "_on_calling_action"))
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
			actions[action].call(params)
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
	#Have to separate the emitting the signal from recursively calling updating stats
	add_stats(s, add)

#We can probably define a multiply stats function too...
func add_stats(s : Dictionary, add ):
	for stat in add:
		if stat in s:
			if s[stat] is int or s[stat] is float:
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
func reset_actions():
	actions = {}

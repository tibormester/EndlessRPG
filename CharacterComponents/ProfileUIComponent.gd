class_name ProfileUIComponent extends CharacterComponent

@onready var p = get_owner()

func reset_actions():
	actions = {
		"Display Stats" = display_stats
	}
	
func display_stats():
	update_stats()
	$StatPanel.visible = !$StatPanel.visible
	
	
func update_stats():
	reset_stats()
	for stat in p.stats.keys():
		var label = create_label(stat, p.stats[stat])
		if label is Control:
			$StatPanel/StatList.add_child(label)
	
func reset_stats():
	for child in $StatPanel/StatList.get_children():
		child.free()

func create_label(stat, value): 
	if value is Dictionary:
		if stat == "Feet Height":
			return
		var list = Container.new()
		for s in value.keys():
			var l = create_label(s, value.get(s, null))
			if l is Control:
				list.add_child(l)
		return list
	var label = Label.new()
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.vertical_alignment = VERTICAL_ALIGNMENT_FILL
	if value  is Node:
		return null
	else:
		label.text = str(stat) + ": " + str(value)
	return label


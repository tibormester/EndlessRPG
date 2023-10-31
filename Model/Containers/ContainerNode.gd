class_name ContainerNode extends Sortable

var children = []

func _init():
	super()
	keys.append("Container")

func get_children(key = null):
	if key == null:
		return children
	else:
		var vals = []
		for child in children:
			if child is Sortable and child.has_key(key):
				vals.append(child)
		return vals

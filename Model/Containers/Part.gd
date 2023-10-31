class_name Part extends ContainerNode

var mesh : MeshInstance3D #Appearance
var hitbox : RigidBody3D #Physics object

var equipped = false #If this object is part of a character or if it is dropped on the ground

func _init():
	super()
	keys.append("Part")

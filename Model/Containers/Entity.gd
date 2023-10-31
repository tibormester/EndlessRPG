class_name Entity extends ContainerNode

var parent_chunk : Chunk
var hit_box : CollisionObject3D
var position : Vector3 #Relative position within the chunk
var rotation : Vector3 #vector pointing in the direction theyre looking, should probably be a vector 4...

func _init():
	super()
	keys.appened("Entity")

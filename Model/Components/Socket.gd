class_name Socket extends Component

var location : Vector3
var orientation : Vector3 # prolly needs to be a vector4

var parent : BodyPart
var child : Part

var filter : Callable #Call it on a Part type and if it returns true we can equip

func _init():
	super()
	keys.append("Socket")

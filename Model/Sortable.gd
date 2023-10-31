class_name Sortable extends Object

var name = "Default Name"
var description = "Default Description"
var keys = [] #Array of strings that the sortable is of type


#Given either a String or list of string checks if the object contains or doesnt contain the key/keys 
func has_key(key : Variant, not_has = false, all = false) -> bool :
	#If just a single key or when recursively checking all one by one
	if key is String :
		#Returns when it finds a match
		if keys.has(key):
			#If not_has is true then returns false because keys does have, otherwas returns not_has at the end (the opposite)
			return not not_has
	#If there are more than one key
	elif key is Array:
		#fore each key, if we need to check all the result of all has to be true otherwise same as above
		var has = true
		for k in key:
			if all:
				has = has and has_key(k, not_has) 
			elif keys.has(k):
				return not not_has
		if all:
			return has
	return not_has

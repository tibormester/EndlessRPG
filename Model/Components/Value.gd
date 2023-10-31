class_name Value extends Component

####NEEDS TO BE REDONE so that it can keep a list of alterations to the value that can be both appeneded to and undonde...

#Basically a variant with name, description, keys, and a value
var thresholds = {} #value, callable pairs

#Design value so that if it changes past a threshold, it calls that threshold action, it does this in order so u cant get one shot if u have somethign to save u
#Note that the set is done before calling the actions so the actions can use the new set value, also it is possible to have max and min values for val
var max = null
var min = null
var value:
	get:
		return value
	set(_value):
		var val = value
		if max != null and _value > max:
			_value = max
		if min != null and _value < min:
			_value = min
		value = _value
		if not thresholds.is_empty():
			var points = thresholds.keys
			if points is Array:
				points.sort()
			if _value < val:
				points.reverse() 
				for point in points:
					if point <= val and _value <= point:
						if thresholds[point] is Callable: thresholds[point].call()
			else:
				for point in points:
					if point <= _value and val <= point:
						if thresholds[point] is Callable: thresholds[point].call()


func _init():
	super()
	keys.append("Value")

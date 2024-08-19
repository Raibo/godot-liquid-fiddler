extends Control

class_name JsonEdit

signal text_changed()
signal text_set()
@export var save_field_name: String

var text: String:
	set(new_value):
		$CodeEdit.text = new_value
		text_set.emit()
	get:
		return $CodeEdit.text
	
func beautify():
	text = %UtilAccessNode.call("FormatJson", text)

func emit_text_changed():
	text_changed.emit()

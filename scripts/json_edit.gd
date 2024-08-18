extends Control

class_name JsonEdit

signal text_changed()

var text: String:
	set(new_value):
		$CodeEdit.text = new_value
	get:
		return $CodeEdit.text
	
func beautify():
	text = %UtilAccessNode.call("FormatJson", text)

func emit_text_changed():
	text_changed.emit()

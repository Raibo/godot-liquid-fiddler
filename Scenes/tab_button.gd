extends Button

class_name TabButton

@export var tab_index: int = 0
signal tab_pressed(tab_index: int)

func _ready():
	pressed.connect(transmit_pressed)

func transmit_pressed():
	tab_pressed.emit(tab_index)

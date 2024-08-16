extends VBoxContainer

signal tab_pressed(tab_index: int)

func _ready():
	for child:Node in get_children():
		if (child is TabButton):
			child.tab_pressed.connect(_on_child_tab_pressed)

func set_toggle(index: int):
	var button_children: Array[Node] = get_children().filter(func(e): return e is Button);
	
	for i in button_children.size():
		var button = button_children[i]
		
		if (button is Button):
			var should_be_toggled = i == index
			button.set_pressed_no_signal(should_be_toggled)

func _on_child_tab_pressed(tab_index: int):
	tab_pressed.emit(tab_index)

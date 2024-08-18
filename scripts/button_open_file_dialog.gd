extends Button

@export var node_to_write_path: Node

func open_file_dialog():
	%FileDialog.popup()
	

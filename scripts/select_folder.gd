extends Button

@export var node_to_write_path: Node

func open_select_folder_dialog():
	%SelectFolderDialog.popup()
	

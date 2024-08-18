extends VBoxContainer

@export var node_to_write_path: Node

func press():
	%SelectFolderDialog.popup()


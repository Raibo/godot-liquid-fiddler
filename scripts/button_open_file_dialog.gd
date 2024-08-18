extends Button

@export var node_to_write_path: Node
@onready var exec_path: String = OS.get_executable_path()

func open_file_dialog():
	var def_path = $"..".get_setting("DefaultFolderPath")
	%FileDialog.current_dir = def_path if %UtilAccessNode.call("FolderExists", def_path) else exec_path
	%FileDialog.popup()

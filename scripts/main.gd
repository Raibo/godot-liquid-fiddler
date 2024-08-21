extends Control

@export var force_tab_button: Node


# Called when the node enters the scene tree for the first time.
func _ready():
	#await get_tree().create_timer(0.1).timeout
	%FileAccessNode.set("ExecPath", OS.get_executable_path())
	var args = OS.get_cmdline_args()
	
	for arg in args:
		if (arg.ends_with(".lfv")):
			%FileAccessNode.set("CurrentSaveFile", arg.replace('\\', '/'))
			print("got path argument " + arg)
	
	%FileAccessNode.call("LoadSettings")
	
	if (force_tab_button and "pressed" in force_tab_button):
		force_tab_button.pressed.emit()

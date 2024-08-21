extends Control


# Called when the node enters the scene tree for the first time.
func _ready():
	#await get_tree().create_timer(7).timeout
	var args = OS.get_cmdline_args()
	
	for arg in args:
		if (arg.ends_with(".lfv")):
			%FileAccessNode.set("CurrentSaveFile", arg.replace('\\', '/'))
			print("got path argument " + arg)
	
	%FileAccessNode.call("LoadSettings")

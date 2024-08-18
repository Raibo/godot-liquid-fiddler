extends VBoxContainer


func update_setting(setting_name:String, new_value: String):
	var settings = %FileAccessNode.get("Settings") as Dictionary
	var old_value = settings.get(setting_name, "")
	
	if (old_value == new_value):
		return
	
	settings[setting_name] = new_value
	%FileAccessNode.call("SaveSettings")


func get_setting(setting_name:String) -> String:
	var settings = %FileAccessNode.get("Settings") as Dictionary
	return settings[setting_name] if settings.has(setting_name) else ""


func load_settings(settings: Dictionary):
	for node in get_children():
		if (node.has_method("_on_load_settings")):
			node._on_load_settings(settings)

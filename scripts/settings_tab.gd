extends VBoxContainer


func update_setting(setting_name: String, new_value: Variant):
	var settings = %FileAccessNode.get("Settings") as Dictionary	
	settings[setting_name] = new_value
	%FileAccessNode.call("SaveSettings")


func get_setting(setting_name:String) -> String:
	var setting = %FileAccessNode.call("GetSetting", setting_name)
	return setting if setting else ""


func load_settings(settings: Dictionary):
	for node in get_children():
		if (node.has_method("_on_load_settings")):
			node._on_load_settings(settings)
	
	reload_liquid()

func reload_liquid():
	var liquid_filters = %FileAccessNode.call("GetSetting", "LiquidFilters")
	var liquid_tags = %FileAccessNode.call("GetSetting", "LiquidTags")
	
	if not liquid_filters or not liquid_tags:
		return
	
	for filter in liquid_filters:
		%UtilAccessNode.call("LoadFilters", filter["Path"], filter["ClassName"])
	
	for tag in liquid_tags:
		%UtilAccessNode.call("LoadTag", tag["Path"], tag["ClassName"], tag["TagName"])

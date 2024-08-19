extends HBoxContainer

signal loaded_values()


func create_save_data() -> Dictionary:
	var save_data = { } as Dictionary
	var tabs = %TabContainer.get_children() as Array[Node]
	
	for tab in tabs:
		var save_field_name = tab.get("save_field_name")
		
		if (save_field_name):
			save_data[save_field_name] = tab.get("text")
	
	return save_data


func save():
	var save_data = create_save_data()
	var current_save_file_path = %FileAccessNode.get("CurrentSaveFile")
	%FileAccessNode.call("SaveValues", save_data, current_save_file_path)


func open_save_as_dialog():
	%SaveAsDialog.popup()


func open_load_dialog():
	%LoadDialog.popup()


func save_as(file_path: String):
	var save_data = create_save_data()
	var success = %FileAccessNode.call("SaveValues", save_data, file_path) as bool
	
	if (success):
		%FileAccessNode.set("CurrentSaveFile", file_path)


func load_from_file(path: String):
	var values = %FileAccessNode.call("LoadValues", path) as Dictionary
	
	if (values == null):
		return
	
	var tabs = %TabContainer.get_children() as Array[Node]
	
	for tab in tabs:
		var save_field_name = tab.get("save_field_name")
		
		if (values.has(save_field_name) and "text" in tab):
			tab.text = values[save_field_name]

	loaded_values.emit()

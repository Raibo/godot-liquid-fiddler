extends HBoxContainer

signal loaded_values()


func _input(event):
	if event.is_action_pressed("Save") and not $ButtonSave.disabled:
		save()


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
		$ButtonSave.disabled = false


func load_default_values():
	await get_tree().create_timer(0.1).timeout
	var path = %SettingsTab.get_setting("DefaultValuesPath")
	load_from_file(path)
	$ButtonSave.disabled = true
	%FileAccessNode.set("CurrentSaveFile", null)


func load_from_file(path: String):
	var values = %FileAccessNode.call("LoadValues", path)
	
	if (values == null):
		return
	
	var tabs = %TabContainer.get_children() as Array[Node]
	
	for tab in tabs:
		var save_field_name = tab.get("save_field_name")
		
		if (values.has(save_field_name) and "text" in tab):
			tab.text = values[save_field_name]
	
	$ButtonSave.disabled = false
	%FileAccessNode.set("CurrentSaveFile", path)
	loaded_values.emit()

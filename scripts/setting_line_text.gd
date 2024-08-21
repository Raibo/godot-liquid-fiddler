extends HBoxContainer

signal setting_changed(setting_name: String, new_setting: String)

@export var setting_name: String

@onready var exec_path: String = OS.get_executable_path()


func get_setting(s_name: String) -> String:
	return $"..".get_setting(s_name)


func _on_text_path_text_changed(new_text):
	setting_changed.emit(setting_name, new_text)


func _on_file_dialog_dir_selected(dir_path):
	%TextPath.text = dir_path
	setting_changed.emit(setting_name, dir_path)


func _on_file_text_changed(file_path):
	setting_changed.emit(setting_name, file_path)


func _on_file_dialog_file_selected(file_path):
	var base_path = %FileAccessNode.get("WorkingDir")
	
	var relative_file_path = %UtilAccessNode.call("GetRelativePath", base_path, file_path) as String
	
	%TextPath.text = file_path if relative_file_path.begins_with("..") else relative_file_path
	setting_changed.emit(setting_name, relative_file_path)


func _on_load_settings(settings: Dictionary):
	%TextPath.text = settings[setting_name] if settings.has(setting_name) else ""

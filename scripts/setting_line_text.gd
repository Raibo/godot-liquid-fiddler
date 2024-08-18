extends HBoxContainer

signal setting_changed(setting_name: String, new_setting: String)

@export var setting_name: String

@onready var exec_path: String = OS.get_executable_path()


func get_setting(setting_name: String) -> String:
	return $"..".get_setting(setting_name)


func _on_text_path_text_changed(new_text):
	setting_changed.emit(setting_name, new_text)


func _on_file_dialog_dir_selected(dir_path):
	%TextPath.text = dir_path
	setting_changed.emit(setting_name, dir_path)


func _on_file_dialog_file_selected(file_path):
	var def_path = get_setting("DefaultFolderPath")
	var base_path = def_path if %UtilAccessNode.call("FolderExists", def_path) else exec_path
	var relative_file_path = %UtilAccessNode.call("GetRelativePath", base_path, file_path)
	%TextPath.text = relative_file_path
	setting_changed.emit(setting_name, relative_file_path)


func _on_load_settings(settings: Dictionary):
	%TextPath.text = settings[setting_name] if settings.has(setting_name) else ""

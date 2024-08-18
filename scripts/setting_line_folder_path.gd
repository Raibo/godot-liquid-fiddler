extends HBoxContainer

signal setting_changed(setting_name: String, new_setting: String)

@export var setting_name: String


func _on_text_path_text_changed(new_text):
	setting_changed.emit(setting_name, new_text)


func _on_file_dialog_dir_selected(dir_path):
	setting_changed.emit(setting_name, dir_path)
	%TextPath.text = dir_path

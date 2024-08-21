extends VBoxContainer

signal disappeared()
signal value_updated()

@export var value_nodes: Array[Node]
@export var going_to_disappear: bool
var value: Dictionary

func disappear():
	going_to_disappear = true
	queue_free()
	disappeared.emit()

func open_file_dialog():
	var defFolder = $FileAccessNode.call("GetSetting", "DefaultFolderPath") as String
	
	if defFolder and not defFolder.is_empty():
		$FileDialog.current_dir = defFolder
		
	$FileDialog.popup()

func update_value(_whatever: String):
	value = {}
	
	for node in value_nodes:
		value[node.field_name] = node.text
	
	value_updated.emit()

func set_fields_match_value(new_value):
	for node in value_nodes:
		if "field_name" in node:
			node.text = new_value.get(node.field_name, "")


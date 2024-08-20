extends VBoxContainer

signal setting_updated(setting_name: String, new_value: Variant)

@export var setting_name: String
@export var item: PackedScene
@export var values: Array[Dictionary]

func add_element():
	add_element_with_value(null)

func add_element_with_value(value):
	var node = item.instantiate()
	add_child(node)
	move_child(node, 3)
	
	if (value):
		node.value = value
		node.set_fields_match_value(value)
	
	node.disappeared.connect(_on_element_disappear)
	node.value_updated.connect(update_values)

func update_values():
	values.clear()
	var nodes = get_children()
	
	for node in nodes:
		if ("going_to_disappear" in node and node.going_to_disappear):
			continue
		
		if ("value" in node and node.value):
			values.append(node.value)
	
	setting_updated.emit(setting_name, values)


func _on_load_settings(_settings):
	var settings = $FileAccessNode.call("GetSetting", setting_name)
	
	if (settings == null):
		return
	
	for item in settings:
		add_element_with_value(item)

func _on_element_disappear():
	update_values()

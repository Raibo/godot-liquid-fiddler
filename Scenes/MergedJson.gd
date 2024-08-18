extends CodeEdit

@export var sources: Array[Node]
@export var source_paths: Array[String]

func _ready():
	for i in sources.size():
		if (sources[i] != null and sources[i].has_signal("text_changed") \
		and !sources[i].text_changed.is_connected(update_merged_json)):
			sources[i].text_changed.connect(update_merged_json)
		
		if (sources[i] != null and sources[i].has_signal("text_set") \
		and !sources[i].text_set.is_connected(update_merged_json)):
			sources[i].text_set.connect(update_merged_json)

func update_merged_json():
	var jsons: Array[String] = []
	var paths: Array[String] = []
	
	for i in sources.size():
		jsons.append(sources[i].text)
		paths.append(source_paths[i] if i < source_paths.size() else "")

	var merged_json = %UtilAccessNode.call("MergeJsons", jsons, paths)
	text = merged_json if merged_json else ""
	

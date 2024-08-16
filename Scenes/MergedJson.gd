extends JsonEdit

@export var sources: Array[JsonEdit]

func update_merged_json():
	var jsons: Array[String]
	jsons.assign(sources.map(func(x: JsonEdit): return x.text))
	
	var paths: Array[String]
	paths.assign(sources.map(func(x: JsonEdit): return x.scope_path))
	
	var merged_json = util_node.call("MergeJsons", jsons, paths)
	text = merged_json if merged_json else ""


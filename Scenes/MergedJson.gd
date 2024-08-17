extends CodeEdit

@export var scope_path: String
@export var sources: Array[Node]

func update_merged_json():
	var jsons: Array[String]
	var paths: Array[String]
	
	for i in sources.size():
		jsons.append(sources[i].text)
		paths.append(sources[i].scope_path if sources[i].scope_path else "")
	
	var merged_json = %UtilAccessNode.call("MergeJsons", jsons, paths)
	text = merged_json if merged_json else ""


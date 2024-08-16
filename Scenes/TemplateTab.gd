extends HBoxContainer

@export var util_node: Node
@export var scope: CodeEdit
@export var template: CodeEdit
@export var renderOutput: CodeEdit

func render():
	var templateText = template.text if template else null
	var scopeText = scope.text if scope else null
	var renderedText = util_node.call("Render", templateText, scopeText)
	
	if (renderedText is String):
		renderOutput.text = renderedText

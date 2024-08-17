extends HBoxContainer

@export var scope: CodeEdit
@export var template: CodeEdit
@export var renderOutput: CodeEdit

func render():
	var templateText = template.text if template else ""
	var scopeText = scope.text if scope else ""
	var renderedText = %UtilAccessNode.call("Render", templateText, scopeText)
	
	if (renderedText is String):
		if (renderOutput):
			renderOutput.text = renderedText
		$CodeEditRender.text = renderedText

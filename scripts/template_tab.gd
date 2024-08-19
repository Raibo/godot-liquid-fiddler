extends HBoxContainer

@export var scope: CodeEdit
@export var template: CodeEdit
@export var renderOutput: CodeEdit

@export var save_field_name: String

var text: String:
	get:
		return template.text if template else ""
	set(new_value):
		template.text = new_value
		render()

func render():
	var templateText = text
	var scopeText = scope.text if scope else ""
	var renderedText = %UtilAccessNode.call("Render", templateText, scopeText)
	
	if (renderedText is String):
		if (renderOutput):
			renderOutput.text = renderedText
		
		$CodeEditRender.text = renderedText

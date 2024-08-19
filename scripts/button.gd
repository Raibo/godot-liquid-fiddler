extends Button

func test():
	var assemblyPath = "C:/Coding/LiquidExtenstions/Dlls/LiquidExtensions.dll"
	var output2 = %UtilAccessNode.call("LoadTag", assemblyPath, "LiquidExtensions.CoolTag", "cool_tag")
	#var output = %UtilAccessNode.call("LoadFilters", assemblyPath, "LiquidExtensions.AdditionalFilters")
	pass

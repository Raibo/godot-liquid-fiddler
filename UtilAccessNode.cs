using Godot;
using LiquidFiddle.NonScriptCode;
using System.Collections.Generic;

#nullable enable

public partial class UtilAccessNode : Node
{
    private string Render(string? templateText, string? scopeJson)
    {
        templateText ??= string.Empty;
        scopeJson ??= "{}";

        var scope = JsonUtil.Parse(scopeJson);

        var result = LiquidUtil.Render(templateText, new Dictionary<string, object?>());

        if (result.IsFailure)
            return string.Join('\n', result.Error);

        return result.Value;
    }

    private string MergeJsons(string[] jsons, string[] paths)
    {
        var mergedData = new Dictionary<string, object?>();

        for (int i = 0; i < jsons.Length; i++)
        {
            var json = jsons[i];
            var parseOutcome = JsonUtil.Parse(json);

            if (parseOutcome.IsFailure || parseOutcome.Value is null)
                continue;

            var path = paths[i];

            if (!string.IsNullOrWhiteSpace(path))
            {
                mergedData = new Dictionary<string, object?>
                {
                    [path] = mergedData,
                };
            }

            InsertDictionary(mergedData, parseOutcome.Value);
        }

        return JsonUtil.Serialize(mergedData);
    }

    private void InsertDictionary(Dictionary<string, object?> destination, Dictionary<string, object?> source)
    {
        foreach (var (key, sourceValue) in source)
        {
            destination.TryGetValue(key, out var destValue);

            switch (destValue, sourceValue)
            {
                case (Dictionary<string, object?> destDict, Dictionary<string, object?> sourceDict):
                    InsertDictionary(destDict, sourceDict);
                    break;

                default:
                    destination[key] = sourceValue;
                    break;
            }
        }
    }
}

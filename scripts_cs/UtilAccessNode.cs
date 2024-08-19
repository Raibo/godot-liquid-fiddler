using Godot;
using LiquidFiddle.NonScriptCode;
using System.Collections.Generic;
using System.IO;

#nullable enable

public partial class UtilAccessNode : Node
{
    private string Render(string? templateText, string? scopeJson)
    {
        templateText ??= string.Empty;
        scopeJson ??= "{}";

        var scopeParseResult = JsonUtil.Parse(scopeJson);

        var scope = scopeParseResult.IsSuccess
            ? scopeParseResult.Value
            : new();

        var result = LiquidUtil.Render(templateText, scope);

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

            var currentData = parseOutcome.Value;
            var path = paths[i];

            if (!string.IsNullOrWhiteSpace(path))
            {
                currentData = new Dictionary<string, object?>
                {
                    [path] = currentData,
                };
            }

            InsertDictionary(mergedData, currentData);
        }

        return JsonUtil.Serialize(mergedData);
    }

    private string? FormatJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        var dataResult = JsonUtil.Parse(json);

        if (dataResult.IsFailure)
            return json;

        return JsonUtil.Serialize(dataResult.Value);
    }

    private string GetRelativePath(string execPath, string absolutePath) =>
        Path.GetRelativePath(execPath, absolutePath).Replace('\\', '/');

    private string GetAbsolutePath(string execPath, string relativePath) =>
        Path.Combine(execPath, relativePath).Replace('\\', '/');

    private bool FolderExists(string path) =>
        Directory.Exists(path);

    private string[] LoadFilters(string assemblyPath, string className) =>
        LiquidUtil.LoadFiltersFromAssembly(assemblyPath, className);

    private bool LoadTag(string assemblyPath, string className, string tagName) =>
        LiquidUtil.LoadTagFromAssembly(assemblyPath, className, tagName);

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

using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace LiquidFiddle.NonScriptCode;

public static class JsonUtil
{
    public static Result<Dictionary<string, object?>> Parse(string json)
    {
        try
        {
            var deserializeOutput = TrickyDeserialize(json);

            if (deserializeOutput is Dictionary<string, object?> dictionary)
                return dictionary;

            return new();
        }
        catch (Exception ex)
        {
            return Result.Failure<Dictionary<string, object?>>(ex.Message);
        }
    }

    public static string Serialize(Dictionary<string, object?> data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        return json;
    }

    private static object? TrickyDeserialize(string json)
    {
        var root = JsonConvert.DeserializeObject<JToken>(json);

        return ConvertValue(root);

        object? ConvertValue(JToken? value)
        {
            return value?.Type switch
            {
                JTokenType.Array => value.ToObject<List<JValue>>()!
                    .Select(ConvertValue)
                    .ToList(),

                JTokenType.Object => value.Values<JProperty>()
                    .Select(p => new KeyValuePair<string, JToken> (p!.Name, p.Value.ToObject<JToken>()!))
                    .ToDictionary(kvp => kvp.Key, kvp => ConvertValue(kvp.Value)),

                JTokenType.Null => null,

                _ => value?.ToObject<object?>(),
            };
        }
    }
}

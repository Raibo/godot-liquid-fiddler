using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Linq;

namespace LiquidFiddle.scripts_cs.utils;
public static class ConversionUtil
{
    public static System.Collections.Generic.Dictionary<string, object> ToCsDict(Godot.Collections.Dictionary gdDict)
    {
        var scDict = new System.Collections.Generic.Dictionary<string, object>();

        foreach (var item in gdDict)
            scDict.Add(item.Key.AsString(), GetCsValue(item.Value));

        return scDict;

        object GetCsValue(Variant inp)
        {
            return inp.VariantType switch
            {
                Variant.Type.Dictionary => ToCsDict(inp.AsGodotDictionary()),
                Variant.Type.Array => inp.AsGodotArray<Variant>().Select(GetCsValue).ToArray(),
                Variant.Type.String => inp.AsString(),
                Variant.Type.Int => inp.AsInt64(),
                Variant.Type.Float => inp.AsDouble(),
                Variant.Type.Bool => inp.AsBool(),
                _ => throw new Exception($"Unexpected Godot.Dictionary element type {inp.VariantType.ToString()}")
            };
        }
    }

    public static Dictionary ToGdDict(System.Collections.Generic.Dictionary<string, object> scDict)
    {
        var gdDict = new Godot.Collections.Dictionary();

        foreach (var (key, value) in scDict)
            gdDict.Add(key, GetVariantValue(value));

        return gdDict;

        Variant GetVariantValue(object inp)
        {
            return inp switch
            {
                System.Collections.Generic.Dictionary<string, object> dict => ToGdDict(dict),
                string s => Variant.From(s),
                long l => Variant.From(l),
                double d => Variant.From(d),
                bool b => Variant.From(b),
                IEnumerable en => EnumerableToGdArray(en),
                _ => throw new Exception($"Unexpected Godot.Dictionary element type {inp?.GetType().ToString() ?? "null"}")
            };
        }

        Godot.Collections.Array EnumerableToGdArray(IEnumerable input)
        {
            var arr = new Godot.Collections.Array();

            foreach (var item in input)
                arr.Add(GetVariantValue(item));

            return arr;
        }
    }
}

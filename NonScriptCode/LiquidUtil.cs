using CSharpFunctionalExtensions;
using DotLiquid;
using DotLiquid.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

#nullable enable

public static class LiquidUtil
{
    public static Result<string, List<string>> Render(string templateText, Dictionary<string, object?> scope)
    {
        var template = Template.Parse(templateText);
        var hash = Hash.FromDictionary(scope);
        var renderedText = template.Render(hash, CultureInfo.InvariantCulture);

        if (template.Errors.Any())
            return template.Errors.Select(e => e.Message).ToList();

        return renderedText;
    }

    public static void RegisterFilter(Type filterContainingClass)
    {
        Template.RegisterFilter(filterContainingClass);
    }

    public static void RegisterTag(Type tagType, string tagName)
    {
        var openMethod = typeof(Template).GetMethod("RegisterTag");
        var closedMethod = openMethod!.MakeGenericMethod(tagType);
        closedMethod.Invoke(null, new[] { tagName });
    }

    public static void ResetLiquidExtensions()
    {
        // Reset tags
        var type = typeof(Template);
        var tagsProperty = type.GetProperty("Tags", BindingFlags.NonPublic | BindingFlags.Static);
        var tagsVal = tagsProperty!.GetValue(null);

        var tagsDict = tagsVal as IDictionary;
        tagsDict!.Clear();
    }
}

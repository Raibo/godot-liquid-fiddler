using CSharpFunctionalExtensions;
using DotLiquid;
using DotLiquid.Util;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

#nullable enable

public static class LiquidUtil
{
    private static AssemblyLoadContext LoadContext = new("LiquidExtensions", isCollectible: true);

    private static Type _templateType;
    private static Type _hashType;

    private static Type _defaultTemplateType;
    private static Type _defaultHashType;

    private static MethodInfo _templateParseMethod;
    private static MethodInfo _templateRenderMethod;
    private static PropertyInfo _templateErrorsProp;
    private static MethodInfo _hashFromDictionaryMethod;

    static LiquidUtil()
    {
        _templateType = typeof(Template);
        _hashType = typeof(Hash);

        _defaultTemplateType = typeof(Template);
        _defaultHashType = typeof(Hash);

        RebindToNewTypes();
    }

    private static void RebindToNewTypes()
    {
        _templateParseMethod = _templateType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) })!;
        _templateRenderMethod = _templateType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance, new[] { _hashType, typeof(IFormatProvider) })!;
        _templateErrorsProp = _templateType.GetProperty("Errors", BindingFlags.Public | BindingFlags.Instance)!;
        _hashFromDictionaryMethod = _hashType.GetMethod("FromDictionary", BindingFlags.Public | BindingFlags.Static)!;
    }

    public static Result<string, List<string>> Render(string templateText, Dictionary<string, object?> scope)
    {
        scope ??= new();

        try
        {
            var template = _templateParseMethod.Invoke(null, new object[] { templateText });
            var hash = _hashFromDictionaryMethod.Invoke(null, new object[] { scope });
            var renderedText = _templateRenderMethod.Invoke(template, new object[] { hash!, CultureInfo.InvariantCulture }) as string;
            var renderErrors = _templateErrorsProp.GetValue(template) as List<Exception>;

            if (renderErrors!.Any())
                return renderErrors!.Select(e => e.Message).ToList();

            return renderedText!;
        }
        catch(Exception ex)
        {
            return ex.InnerException?.Message ?? string.Empty;
        }
    }

    public static void RegisterFilter(Type filterContainingClass)
    {
        var registerFilterMethod = _templateType.GetMethod("RegisterFilter", BindingFlags.Public | BindingFlags.Static);
        registerFilterMethod!.Invoke(null, new object[] { filterContainingClass });
    }

    public static void RegisterTag(Type tagType, string tagName)
    {
        var openMethod = _templateType.GetMethod("RegisterTag");
        var closedMethod = openMethod!.MakeGenericMethod(tagType);
        closedMethod.Invoke(null, new[] { tagName });
    }

    public static string[] LoadFiltersFromAssembly(string assemblyPath, string className)
    {
        var assembly = GetAssembly(assemblyPath);
        var type = assembly.GetType(className);

        if (type is null)
            return Array.Empty<string>();

        RegisterFilter(type);

        return type.GetMethods(BindingFlags.Static | BindingFlags.Public).Select(m => m.Name).ToArray();
    }

    public static bool LoadTagFromAssembly(string assemblyPath, string className, string tagName)
    {
        var assembly = GetAssembly(assemblyPath);
        var type = assembly.GetType(className);

        if (type is null)
            return false;

        RegisterTag(type, tagName);

        return true;
    }

    private static Assembly GetAssembly(string assemblyPath, AssemblyName? assemblyName = null)
    {
        // Look in existing AssemblyLoadContexts
        if (assemblyName is not null)
        {
            var alreadyLoadedAssembly = LoadContext.Assemblies.SingleOrDefault(a => a.GetName().Name == assemblyName.Name);

            if (alreadyLoadedAssembly is not null)
            {
                GD.Print($"Assembly {assemblyName.FullName} duplicate by name");
                return alreadyLoadedAssembly;
            }

            var haveInDefaultContextAssembly = AssemblyLoadContext.Default.Assemblies.SingleOrDefault(a => a.GetName().Name == assemblyName.Name);

            if (haveInDefaultContextAssembly is not null)
            {
                GD.Print($"Assembly {assemblyName.FullName} got from default context");
                return haveInDefaultContextAssembly;
            }
        }

        var similarByPathAssembly = LoadContext.Assemblies.SingleOrDefault(a => assemblyPath.Contains(a.GetName().Name));

        if (similarByPathAssembly is not null)
        {
            GD.Print($"Assembly {assemblyName?.FullName ?? Path.GetFileName(assemblyPath)} duplicate by path");
            return similarByPathAssembly;
        }

        // Actually load a file
        var assembly = LoadContext.LoadFromAssemblyPath(assemblyPath);
        GD.Print($"Assembly {assembly.GetName().FullName} loaded");

        if (assembly.FullName!.Contains("DotLiquid"))
        {
            _templateType = assembly.GetType("DotLiquid.Template")!;
            _hashType = assembly.GetType("DotLiquid.Hash")!;
            RebindToNewTypes();
        }

        // Get dependencies
        var refs = assembly.GetReferencedAssemblies();
        var resolver = new AssemblyDependencyResolver(assemblyPath);

        foreach (var item in refs)
        {
            if (LoadContext.Assemblies.Any(a => a.GetName() == item))
                continue;

            var refPath = resolver.ResolveAssemblyToPath(item);

            if (refPath is null)
                continue;

            GetAssembly(refPath, item);
        }

        return assembly;
    }

    public static void ResetLiquidExtensions()
    {
        // Reset tags
        var type = typeof(Template);
        var tagsProperty = type.GetProperty("Tags", BindingFlags.NonPublic | BindingFlags.Static);
        var tagsVal = tagsProperty!.GetValue(null);
        
        var tagsDict = tagsVal as IDictionary;
        tagsDict!.Clear();

        // Reset filters
        // ??
    }
}

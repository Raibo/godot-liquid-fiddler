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

        if (string.IsNullOrWhiteSpace(className))
        {
            GD.Print($"Cannot load filter, class name is blank");
            return Array.Empty<string>();
        }

        if (assembly is null)
            return Array.Empty<string>();

        var type = assembly.GetType(className);

        if (type is null)
        {
            GD.Print($"Cannot load filter, class {className} not found in assembly {assembly.GetName().Name}");
            return Array.Empty<string>();
        }

        RegisterFilter(type);

        var loadedFilters = type.GetMethods(BindingFlags.Static | BindingFlags.Public).Select(m => m.Name).ToArray();

        foreach (var item in loadedFilters)
            GD.Print($"Loaded filter {item} from assembly {assembly.GetName().Name}");

        return loadedFilters;
    }

    public static bool LoadTagFromAssembly(string assemblyPath, string className, string tagName)
    {
        var assembly = GetAssembly(assemblyPath);

        if (string.IsNullOrWhiteSpace(className))
        {
            GD.Print($"Cannot load tag {tagName}, class name is blank");
            return false;
        }

        if (assembly is null)
            return false;

        var type = assembly.GetType(className);

        if (type is null)
        {
            GD.Print($"Cannot load tag {tagName}, class {className} not found in assembly {assembly.GetName().Name}");
            return false;
        }

        RegisterTag(type, tagName);

        GD.Print($"Loaded tag {tagName} from assembly {assembly.GetName().Name}");
        return true;
    }

    private static Assembly? GetAssembly(string? assemblyPath, AssemblyName? assemblyName = null)
    {
        if (assemblyPath is null && assemblyName is null)
        {
            GD.Print("Attempted to load an unknown assembly");
            return null;
        }

        string? assemblyFullPath = null;

        if (assemblyPath is not null)
            assemblyFullPath = Path.GetFullPath(assemblyPath, Path.GetDirectoryName(FileAccessNode.ExecPathStatic)!);

        if (assemblyName is not null && assemblyName.Name != "DotLiquid")
        {
            // Look if already loaded in LoadContext
            var alreadyLoadedAssembly = LoadContext.Assemblies.SingleOrDefault(a => a.GetName().FullName == assemblyName.FullName);

            if (alreadyLoadedAssembly is not null)
            {
                GD.Print($"Assembly {assemblyName.FullName} already loaded");
                return alreadyLoadedAssembly;
            }

            // Try to load by name
            Assembly? byNameAssembly = null;

            try
            {
                byNameAssembly = LoadContext.LoadFromAssemblyName(assemblyName);
            }
            catch (Exception)
            { }

            if (byNameAssembly is not null)
            {
                GD.Print($"Assembly {assemblyName.FullName} loaded by name");
                return byNameAssembly;
            }
        }

        // Look if loaded something with similar path
        //if (assemblyFullPath is not null)
        //{
        //    var similarByPathAssembly = LoadContext.Assemblies.SingleOrDefault(a => assemblyFullPath.Contains(a.GetName().Name));

        //    if (similarByPathAssembly is not null)
        //    {
        //        GD.Print($"Assembly {assemblyName?.FullName ?? Path.GetFileName(assemblyFullPath)} duplicate by path");
        //        return similarByPathAssembly;
        //    }
        //}

        // Actually load a file
        Assembly? assembly = null;
        try
        {
            assembly = LoadContext.LoadFromAssemblyPath(assemblyFullPath);
            GD.Print($"Assembly {assembly.GetName().FullName} loaded from folder");
        }
        catch (Exception)
        {
            GD.Print($"Error loading assembly by path \"{assemblyFullPath}\"");
            return null;
        }

        if (assembly.FullName!.Contains("DotLiquid"))
        {
            _templateType = assembly.GetType("DotLiquid.Template")!;
            _hashType = assembly.GetType("DotLiquid.Hash")!;
            RebindToNewTypes();
        }

        // Get dependencies
        var refs = assembly.GetReferencedAssemblies();
        var resolver = new AssemblyDependencyResolver(assemblyFullPath);

        foreach (var item in refs)
        {
            var refPath = resolver.ResolveAssemblyToPath(item);
            GD.Print($"Assembly {assembly.GetName().Name} has dependency {item.Name}");
            GetAssembly(refPath, item);
        }

        return assembly;
    }

    public static void UnloadLiquidExtensions()
    {
        LoadContext.Unload();
        LoadContext = new("LiquidExtensions", isCollectible: true);

        _templateType = _defaultTemplateType;
        _hashType = _defaultHashType;

        RebindToNewTypes();

        GD.Print("Unload extensions");
    }
}

using Godot;
using Godot.Collections;
using LiquidFiddle.NonScriptCode;
using LiquidFiddle.scripts_cs.utils;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

public partial class FileAccessNode : Node
{
    public const string WindowTitle = "Liquid Fiddler";

    [Signal] public delegate void LoadedSettingsEventHandler(Dictionary settings);
    [Signal] public delegate void AfterLoadedSettingsEventHandler();

    [Export] public Dictionary Settings { get => _settings; set => _settings = value; }
    private static Dictionary _settings = new();
    
    public string CurrentSaveFile
    {
        get => _currentSaveFile;
        set
        {
            _currentSaveFile = value;

            var filePathText = string.IsNullOrWhiteSpace(value)
                ? "Default"
                : value;

            DisplayServer.WindowSetTitle($"{WindowTitle} - {filePathText}");
        }
    }

    public static string _currentSaveFile;

    private const string SettingsPath = @"settings.json";
    public string WorkingDir => Directory.GetCurrentDirectory().Replace('\\', '/');
    public string ExecPath { get => _execPath; set => _execPath = value; }
    private static string _execPath;

    private Variant GetSetting(string settingName)
    {
        Settings.TryGetValue(settingName, out var value);
        return value;
    }

    private void LoadSettings()
    {
        var settingsFullPath = Path.Combine(Path.GetDirectoryName(ExecPath), SettingsPath);

        if (!File.Exists(settingsFullPath))
        {
            Settings = new Dictionary
            {
                ["DefaultFolderPath"] = "",
                ["DefaultValuesPath"] = "default-values.lfv",
            };

            EmitSignal(SignalName.LoadedSettings, Settings);
            EmitSignal(SignalName.AfterLoadedSettings);

            return;
        }

        var text = File.ReadAllText(settingsFullPath);
        var loadedSettingsResult = JsonUtil.Parse(text);

        if (loadedSettingsResult.IsFailure)
            return;

        Settings = ConversionUtil.ToGdDict(loadedSettingsResult.Value);
        EmitSignal(SignalName.LoadedSettings, Settings);
        EmitSignal(SignalName.AfterLoadedSettings);
    }

    private void SaveSettings()
    {
        var text = JsonConvert.SerializeObject(ConversionUtil.ToCsDict(Settings), Formatting.Indented);
        File.WriteAllText(SettingsPath, text);
    }

    private bool SaveValues(Dictionary values, string path)
    {
        try
        {
            var text = JsonConvert.SerializeObject(ConversionUtil.ToCsDict(values), Formatting.Indented);
            File.WriteAllText(path, text);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private Dictionary LoadValues(string path)
    {
        try
        {
            var text = File.ReadAllText(path);
            var csDict = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(text);
            return ConversionUtil.ToGdDict(csDict);
        }
        catch
        {
            return null;
        }
    }
}

using Godot;
using Godot.Collections;
using LiquidFiddle.scripts_cs.utils;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

public partial class FileAccessNode : Node
{
    public const string WindowTitle = "Liquid Fiddler";

    [Signal] public delegate void LoadedSettingsEventHandler(Dictionary settings);
    [Signal] public delegate void AfterLoadedSettingsEventHandler();

    [Export] public Dictionary Settings { get; set; } = new();
    
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

    public override void _Ready()
    {
        LoadSettings();
    }

    private string GetSetting(string settingName)
    {
        Settings.TryGetValue(settingName, out var value);
        return value.AsString();
    }

    private void LoadSettings()
    {
        if (!File.Exists(SettingsPath))
        {
            Settings = new Dictionary
            {
                ["DefaultFolderPath"] = WorkingDir,
                ["DefaultValuesPath"] = "default-values.json",
            };

            EmitSignal(SignalName.LoadedSettings, Settings);
            return;
        }

        var text = File.ReadAllText(SettingsPath);
        var loadedSettings = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(text);

        Settings = ConversionUtil.ToGdDict(loadedSettings);
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

using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System;
using System.IO;

public partial class FileAccessNode : Node
{
    [Signal] public delegate void LoadedSettingsEventHandler(Dictionary settings);
    [Export] public Dictionary Settings { get; set; } = new();

    private const string SettingsPath = @".\settings.json";

    public override void _Ready()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        if (!File.Exists(SettingsPath))
            File.WriteAllText(SettingsPath, "");

        var text = File.ReadAllText(SettingsPath);
        var loadedSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);

        Settings = ToGdDict(loadedSettings);
        EmitSignal(SignalName.LoadedSettings, Settings);
    }

    private void SaveSettings()
    {
        var text = JsonConvert.SerializeObject(ToCsDict(Settings), Formatting.Indented);
        File.WriteAllText(SettingsPath, text);
    }

    private Dictionary<string, string> ToCsDict(Dictionary gdDict)
    {
        var scDict = new Dictionary<string, string>();

        foreach (var item in gdDict)
            scDict.Add(item.Key.AsString(), item.Value.AsString());

        return scDict;
    }

    private Dictionary ToGdDict(Dictionary<string, string> scDict)
    {
        var gdDict = new Dictionary();

        foreach (var (key, value) in scDict)
            gdDict.Add(key, value);

        return gdDict;
    }
}

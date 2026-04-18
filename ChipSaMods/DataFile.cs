using ModCore.Events;
using ModCore.Models;
using System;
using System.IO;
using System.Text.Json;
using ChipSaMod;
using static ModCore.Configs.ConfigDtos;

public static class ChipSaModDataStorage
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Data/ChipSaMod";
	private const string ConfigFileName = "data_storage.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };


    public static TemplateData Data { get; private set; } = new TemplateData();

	static ChipSaModDataStorage()
	{
        SerializerOptions.Converters.Add(new PrefabGUIDConverter());
        Load();
	}

	public static void Load()
	{
        EnsureConfigDirectory();
        if (!File.Exists(FullPath))
		{
			Save(); // Create file with default values
			return;
		}

		try
		{
			var jsonData = File.ReadAllText(FullPath);
			Data = JsonSerializer.Deserialize<TemplateData>(jsonData, SerializerOptions) ?? new TemplateData();
		}
		catch (Exception ex)
		{
			Data = new TemplateData();
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}

	public static void Save()
	{
		try
		{
            EnsureConfigDirectory();
			var jsonData = JsonSerializer.Serialize(Data, SerializerOptions);
			File.WriteAllText(FullPath, jsonData);
		}
		catch (Exception ex)
		{
            Plugin.PluginLog.LogInfo(ex.ToString());
        }
	}

    private static void EnsureConfigDirectory()
    {
        // Check if the directory exists, if not, create it
        if (!Directory.Exists(ConfigDirectoryName))
        {
            Directory.CreateDirectory(ConfigDirectoryName);
        }
    }
}

public class TemplateData
{
	public int TestField { get; set; } = 5; // Default value
}

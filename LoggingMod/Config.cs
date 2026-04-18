using System;
using System.IO;
using System.Text.Json;
using LoggingMod;
using static ModCore.Configs.ConfigDtos;

public static class LoggingModConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/TemplateMod";
	private const string ConfigFileName = "template_mod_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };


    public static LoggingModConfigData Config { get; private set; } = new LoggingModConfigData();

	static LoggingModConfig()
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
			Config = JsonSerializer.Deserialize<LoggingModConfigData>(jsonData, SerializerOptions) ?? new LoggingModConfigData();
		}
		catch (Exception ex)
		{
			Config = new LoggingModConfigData();
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}

	public static void Save()
	{
		try
		{
            EnsureConfigDirectory();
			var jsonData = JsonSerializer.Serialize(Config, SerializerOptions);
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

public class LoggingModConfigData
{
	public int TestField { get; set; } = 5; // Default value
}

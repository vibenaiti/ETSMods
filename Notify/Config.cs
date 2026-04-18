using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VArenaNotify;
using TemplateMod;
using static ModCore.Configs.ConfigDtos;

public static class VArenaNotifyConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/VArena/Config/VNotifyMod";
	private const string ConfigFileName = "vnotify_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };


    public static VArenaNotifyConfigData Config { get; private set; } = new VArenaNotifyConfigData();

	static VArenaNotifyConfig()
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
			Config = JsonSerializer.Deserialize<VArenaNotifyConfigData>(jsonData, SerializerOptions) ?? new VArenaNotifyConfigData();
		}
		catch (Exception ex)
		{
			Config = new VArenaNotifyConfigData();
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

public class VArenaNotifyConfigData
{
	public string playerConnectedNotification { get; set; } = "";
	public string playerNewNotification { get; set; } = "";
	public string playerDisconnectedNotification { get; set; } = "";
	
	public List<string> welcomePlayerNotification { get; set; } = new List<string>();
	public List<AutoAnnouncement> autoAnnouncementNotifications { get; set; } = new List<AutoAnnouncement>();
}

public class AutoAnnouncement
{
	public int delayInSeconds { get; set; } = new ();
	public List<string> announcement { get; set; } = new List<string>();
}

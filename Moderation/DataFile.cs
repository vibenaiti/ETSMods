using ModCore.Events;
using ModCore.Models;
using System;
using System.IO;
using System.Text.Json;
using Moderation;
using static ModCore.Configs.ConfigDtos;
using System.Collections.Generic;

public static class ModerationModDataStorage
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Data/ModerationMod";
	private const string ConfigFileName = "data_storage.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };


    public static ModerationData Data { get; private set; } = new ModerationData();

	static ModerationModDataStorage()
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
			Data = JsonSerializer.Deserialize<ModerationData>(jsonData, SerializerOptions) ?? new ModerationData();
		}
		catch (Exception ex)
		{
			Data = new ModerationData();
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

public class ModerationData
{
	public HashSet<ulong> MutedPlayers { get; set; } = new();
	public Dictionary<ulong, LastClan> PlayersToLastClan {get;set;} = new();
}

public class LastClan
{
    public DateTime DateLeftClan { get; set; }
    public int ClanNetworkId { get; set; }
}
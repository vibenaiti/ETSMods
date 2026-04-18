using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Serialization;
using Achievements;
using ModCore;
using ProjectM;
using Stunlock.Core;
using static ModCore.Configs.ConfigDtos;

public static class DataStorageFile
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Data/Achievements";
	private const string ConfigFileName = "data_storage.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };


    public static DataStorageFileData Data { get; private set; } = new DataStorageFileData();

	static DataStorageFile()
	{
        SerializerOptions.Converters.Add(new PrefabGUIDConverter());
        Load();
	}

    public async static void Load()
    {
        EnsureConfigDirectory();
        if (!File.Exists(FullPath))
        {
            Save(); // Create file with default values
            return;
        }

        try
        {
            var jsonData = await File.ReadAllTextAsync(FullPath);
            var dataIntKeys = JsonSerializer.Deserialize<DataStorageFileData>(jsonData, SerializerOptions);
            if (dataIntKeys != null)
            {
                Data.ServerStartTime = dataIntKeys.ServerStartTime;
                Data.ServerStartDateTime = dataIntKeys.ServerStartDateTime;
                Globals.ServerStartTime = Data.ServerStartTime;
                Globals.ServerStartDateTime = Data.ServerStartDateTime;
                Data.KilledBosses = dataIntKeys.KilledBossesIntKeys.ToDictionary(
                    kvp => IntToPrefabGUID(kvp.Key),
                    kvp => kvp.Value);
            }
        }
        catch (Exception ex)
        {
            Achievements.Plugin.PluginLog.LogInfo(ex.ToString());
        }
    }

    public async static void Save()
    {
        try
        {
            EnsureConfigDirectory();
            var dataIntKeys = new DataStorageFileData
            {
                ServerStartTime = Data.ServerStartTime,
                ServerStartDateTime = Data.ServerStartDateTime,
                KilledBossesIntKeys = Data.KilledBosses.ToDictionary(
                    kvp => PrefabGUIDToInt(kvp.Key),
                    kvp => kvp.Value)
            };
            Globals.ServerStartTime = Data.ServerStartTime;
            Globals.ServerStartDateTime = Data.ServerStartDateTime;
            var jsonData = JsonSerializer.Serialize(dataIntKeys, SerializerOptions);
            await File.WriteAllTextAsync(FullPath, jsonData);
        }
        catch (Exception ex)
        {
            Achievements.Plugin.PluginLog.LogInfo(ex.ToString());
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

    // Conversion from PrefabGUID to int
    public static int PrefabGUIDToInt(PrefabGUID guid)
    {
        return guid.GuidHash;
    }

    // Conversion from int to PrefabGUID
    public static PrefabGUID IntToPrefabGUID(int id)
    {
        return new PrefabGUID(id);
    }
}

public class DataStorageFileData
{
    public double ServerStartTime { get; set; } = 0;
    public DateTime ServerStartDateTime { get; set; } = DateTime.MinValue;
    [JsonIgnore] public Dictionary<PrefabGUID, DateTime> KilledBosses { get; set; } = new Dictionary<PrefabGUID, DateTime>();
    public Dictionary<int, DateTime> KilledBossesIntKeys { get; set; } = new Dictionary<int, DateTime>();
}


public class KilledBossEntry
{
    public PrefabGUID Id { get; set; }
    public DateTime KillTime { get; set; }
}

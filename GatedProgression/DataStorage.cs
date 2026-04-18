using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProjectM;
using GatedProgression;
using ModCore.Data;
using ModCore.Models;
using static ModCore.Configs.ConfigDtos;
using Stunlock.Core;

public static class DataStorage
{
    private const string ConfigDirectoryName = "Bepinex/config/ETS/Data/GatedProgression";
    private const string ConfigFileName = "data_storage.json";
    private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };


    public static DataStorageData Data { get; private set; } = new DataStorageData();

    static DataStorage()
    {
        SerializerOptions.Converters.Add(new PrefabGUIDConverter());
        SerializerOptions.Converters.Add(new PlayerConverter());
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
            Data = JsonSerializer.Deserialize<DataStorageData>(jsonData, SerializerOptions) ?? new DataStorageData();
        }
        catch (Exception ex)
        {
            Data = new DataStorageData();
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

public class DataStorageData
{
    public HashSet<PrefabGUID> LockedBosses { get; set; } = new();
};
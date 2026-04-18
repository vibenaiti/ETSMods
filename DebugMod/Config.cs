using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using DebugMod;
using DebugMod.Managers;
using ModCore.Data;
using ModCore.Services;
using ProjectM;
using Stunlock.Core;
using static ModCore.Configs.ConfigDtos;

public static class DebugModConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/DebugMod";
	private const string ConfigFileName = "debug_mod_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };


    public static DebugModConfigData Config { get; private set; } = new DebugModConfigData();
    private static FileSystemWatcher? fileWatcher;
    private static void InitializeFileWatcher()
    {
        fileWatcher = new FileSystemWatcher
        {
            Path = ConfigDirectoryName,
            Filter = ConfigFileName,
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        fileWatcher.Changed += OnConfigFileChanged;
    }

    private static Timer? debounceTimer;
    private static void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        debounceTimer?.Dispose(); // Cancel any pending load operation
        debounceTimer = new Timer(_ => Load(), null, TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
    }

    public static void Initialize()
    {
        SerializerOptions.Converters.Add(new PrefabGUIDConverter());
        Load();
        InitializeFileWatcher();
    }

    public static void Dispose()
    {
        if (fileWatcher != null)
        {
            fileWatcher.Changed -= OnConfigFileChanged; // Unsubscribe from the event
            fileWatcher.Dispose(); // Dispose the watcher
            fileWatcher = null; // Allow for garbage collection
        }
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
			Config = JsonSerializer.Deserialize<DebugModConfigData>(jsonData, SerializerOptions) ?? new DebugModConfigData();
            var action = () =>
            {
                SunProtectionZoneManager.SunProtectionArea = Config.SunProtectionZone.ToRectangleZone();
            };
            ActionScheduler.RunActionOnMainThread(action);
        }
		catch (Exception ex)
		{
			Config = new DebugModConfigData();
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}

	public async static void Save()
	{
		try
		{
            EnsureConfigDirectory();
			var jsonData = JsonSerializer.Serialize(Config, SerializerOptions);
			await File.WriteAllTextAsync(FullPath, jsonData);
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

public class DebugModConfigData
{
    public int TestField { get; set; } = 5; // Default value
    public RectangleZoneDto SunProtectionZone { get; set; } = new();
	public Dictionary<string, Dictionary<string, PrefabGUID>> AbilityPresets { get; set;} = new()
    {
        {
            "FireAbilities", new Dictionary<string, PrefabGUID>()
            {
                { "Auto", PrefabGUID.Empty}, // Assuming PrefabGUID takes an int in its constructor for the example
                { "Weapon1", Prefabs.AB_Cardinal_LightNova_AbilityGroup },
                { "Weapon2", Prefabs.AB_Cardinal_LightNova_AbilityGroup },
                { "Dash", Prefabs.AB_Cardinal_LightNova_AbilityGroup },
                { "Spell1", Prefabs.AB_Cardinal_LightNova_AbilityGroup },
                { "Spell2", Prefabs.AB_Cardinal_LightNova_AbilityGroup },
                { "Ult", Prefabs.AB_Cardinal_LightNova_AbilityGroup },
            }
        }
    };
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenWorldEvents;
using ProjectM;
using ModCore.Data;
using static ModCore.Configs.ConfigDtos;
using System.Threading;
using Stunlock.Core;

public static class OpenWorldEventsConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/OpenWorldEvents";
	private const string ConfigFileName = "open_world_events_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };
    public static OpenWorldEventsConfigData Config { get; private set; } = new OpenWorldEventsConfigData();

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
        SerializerOptions.Converters.Add(new TimeOnlyConverter());
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
			Config = JsonSerializer.Deserialize<OpenWorldEventsConfigData>(jsonData, SerializerOptions) ?? new OpenWorldEventsConfigData();
		}
		catch (Exception ex)
		{
			Config = new OpenWorldEventsConfigData();
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

public class OpenWorldEventsConfigData
{
    public bool AutomaticEventsEnabled { get; set; } = true;
    public bool RevealMap { get; set; } = true;
    public int CurrencyMultiplierDuringEvents { get; set; } = 3;
    public float DefaultDurabilityLoss { get; set; } = 0.125f;
    public int DaysIntoWipeToStartAutomaticEvents { get; set; } = 3;
    public int DurabilityDelayAfterEvent { get; set; } = 300;

    public class LootItem
    {
        public PrefabGUID ItemPrefabGUID { get; set; } = Prefabs.Item_Ingredient_Crystal;
        public int Quantity { get; set; } = 1;
        public int Weight { get; set; } = 1;
        public PrefabGUID BloodType { get; set; } = Prefabs.AB_BloodBuff_DamageReduction_Warrior;
        public int BloodQuality { get; set; } = 100;
        public int Durability { get; set; } = -1;
    }
}

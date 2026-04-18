using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenWorldEvents;
using ProjectM;
using ModCore.Data;
using static ModCore.Configs.ConfigDtos;
using System.Threading;
using static OpenWorldEventsConfigData;
using Stunlock.Core;

public static class ResourceNodeConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/OpenWorldEvents";
	private const string ConfigFileName = "resource_node_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };
    public static ResourceNodeConfigData Config { get; private set; } = new ResourceNodeConfigData();

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
			Config = JsonSerializer.Deserialize<ResourceNodeConfigData>(jsonData, SerializerOptions) ?? new ResourceNodeConfigData();
		}
		catch (Exception ex)
		{
			Config = new ResourceNodeConfigData();
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

public class ResourceNodeConfigData
{
    public bool AnnounceEvent { get; set; } = true;
    public bool DisableDurabilityLossDuringEvent { get; set; } = false;
    public float ReflectedDamageFraction { get; set; } = 1.0f;
    public PrefabGUID ResourceNodePrefab { get; set; } = Prefabs.TM_WerewolfTree_05_Stage1;
    public int InfiniteResourceNodeDelay { get; set; } = 180;
    public int InfiniteResourceNodeDurationSeconds { get; set; } = 120;
    public int NumberOfLightsInNode { get; set; } = 5;
    public int LightColorIndex { get; set; } = 4;
    public List<TimeOnly> ResourceNodeSpawnTimes { get; set; } = new()
    {
        new TimeOnly(1, 1),
        new TimeOnly(13, 1),
        new TimeOnly(19, 1),
    };
    public List<CoordinateDto> ResourceNodeSpawnLocations { get; set; } = new()
    {
        new CoordinateDto
        {
            X = -1000f,
            Y = 0f,
            Z = -510f
        },
    };
    public List<LootItem> InfiniteResourceNodeItemsOptions { get; set; } = new()
    {
            new LootItem()
            {
                ItemPrefabGUID = Prefabs.Item_Ingredient_Mineral_CopperIngot,
                Quantity = 10,
                Weight = 50,
            },
            new LootItem()
            {
                ItemPrefabGUID = Prefabs.Item_Ingredient_Mineral_IronBar,
                Quantity = 5,
                Weight = 30
            },
            new LootItem()
            {
                ItemPrefabGUID = Prefabs.Item_Ingredient_Mineral_DarkSilverBar,
                Quantity = 1,
                Weight = 10
            },
            new LootItem()
            {
                ItemPrefabGUID = Prefabs.Item_Ingredient_Mineral_GoldBar,
                Quantity = 1,
                Weight = 5
            },
            new LootItem()
            {
                ItemPrefabGUID = Prefabs.Item_Ingredient_Plant_Thistle,//replace with lesser stygian
                Quantity = 100,
                Weight = 5
            }
    };
}

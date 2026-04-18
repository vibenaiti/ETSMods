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
using static OpenWorldEventsConfigData;

public static class HungerGamesConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/OpenWorldEvents";
	private const string ConfigFileName = "hunger_games.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };
    public static HungerGamesConfigData Config { get; private set; } = new HungerGamesConfigData();

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
			Config = JsonSerializer.Deserialize<HungerGamesConfigData>(jsonData, SerializerOptions) ?? new HungerGamesConfigData();
		}
		catch (Exception ex)
		{
			Config = new HungerGamesConfigData();
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

public class HungerGamesConfigData
{
    public int SignupTimeSeconds { get; set; } = 60;
    public int MaxStormHeight { get; set; } = 4;
    public int Level1SecondsBeforePoison { get; set; } = 240;
    public int Level2SecondsBeforePoison { get; set; } = 180;
    public int Level3SecondsBeforePoison { get; set; } = 120;
    public int Level4SecondsBeforePoison { get; set; } = 60;
    public float PercentPlayersAliveBeforeOpeningLevel2Doors { get; set; } = 0.75f;
    public float PercentPlayersAliveBeforeOpeningLevel3Doors { get; set; } = 0.5f;
    public float PercentPlayersAliveBeforeOpeningLevel4Doors { get; set; } = 0.25f;
    public RectangleZoneDto EntireArenaZone { get; set; } = new();
    public float ChanceForHigherTierLoot { get; set; } = 0.01f;
    public List<ItemDTO> WinnerRewards { get; set; } = new() 
    {
        new()
        {
            ItemPrefabGUID = Prefabs.Item_Ingredient_Document,
            Quantity = 1
        }
    };
    public CoordinateDto WaitingRoomTeleportCoordinates { get; set; } = new();
    public RectangleZoneDto WaitingRoomArea { get; set; } = new();
    public RectangleZoneDto FinalSafeZone { get; set; } = new();
    public List<UnitSpawnPoint> UnitSpawnPoints { get; set; } = new()
    {
        new()
        {
            Level = 100,
            Quantity = 5,
            SpawnableUnits = new() { Prefabs.CHAR_CopperGolem },
            SpawnPoint = new()
            {
                X = 0,
                Y = 0,
                Z = 0
            }
        }
    };
    public LootTable Tier1Drops { get; set; } = new()
    {
        RollsPerChest = 5,
        LootDrops = new() 
        {
            new LootItem()
            {
                ItemPrefabGUID = Prefabs.Item_Ingredient_Wood_Standard,
                Quantity = 50,
                Weight = 1
            }
        },
    };
    public LootTable Tier2Drops { get; set; } = new()
    {
        RollsPerChest = 5,
        LootDrops = new()
        {
            new LootItem()
            {
                ItemPrefabGUID = Prefabs.Item_Ingredient_Wood_Standard,
                Quantity = 50,
                Weight = 1
            }
        },
    };
    public LootTable Tier3Drops { get; set; } = new()
    {
        RollsPerChest = 5,
        LootDrops = new()
        {
            new LootItem()
            {
                ItemPrefabGUID = Prefabs.Item_Ingredient_Wood_Standard,
                Quantity = 50,
                Weight = 1
            }
        },
    };
    public LootTable Tier4Drops { get; set; } = new()
    {
        RollsPerChest = 5,
        LootDrops = new()
        {
            new LootItem()
            {
                ItemPrefabGUID = Prefabs.Item_Ingredient_Wood_Standard,
                Quantity = 50,
                Weight = 1
            }
        },
    };

    public LootTable Tier5Drops { get; set; } = new()
    {
        RollsPerChest = 5,
        LootDrops = new()
        {
            new LootItem()
            {
                ItemPrefabGUID = Prefabs.Item_Ingredient_Wood_Standard,
                Quantity = 50,
                Weight = 1
            }
        },
    };
}

public class LootTable
{
    public List<LootItem> LootDrops { get; set; } = new();
    public int RollsPerChest { get; set; } = 5;
}

public class UnitSpawnPoint
{
    public CoordinateDto SpawnPoint { get; set; } = new();
    public int Quantity { get; set; } = 1;
    public int Level { get; set; } = 70;
    public List<PrefabGUID> SpawnableUnits { get; set; } = new()
    {
        Prefabs.CHAR_Forest_Bear_Dire_Vblood
    };

}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenWorldEvents;
using ProjectM;
using ModCore.Data;
using static ModCore.Configs.ConfigDtos;
using System.Threading;

public static class DuelsConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/OpenWorldEvents";
	private const string ConfigFileName = "duels_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };
    public static DuelsConfigData Config { get; private set; } = new DuelsConfigData();

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
			Config = JsonSerializer.Deserialize<DuelsConfigData>(jsonData, SerializerOptions) ?? new DuelsConfigData();
		}
		catch (Exception ex)
		{
			Config = new DuelsConfigData();
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

public class DuelsConfigData
{
    public bool AnnounceEvent { get; set; } = true;
    public int SignupTimeSeconds { get; set; } = 60;
    public int WinnerLootTimeSeconds { get; set; } = 60;
    public int MaxDuelTime { get; set; } = 300;
    public int MaxLevelDifference { get; set; } = 10;
    public List<ItemDTO> LesserDuelRewards { get; set; } = new()
    {
        new ItemDTO
        {
            ItemPrefabGUID = Prefabs.Item_Ingredient_Crystal,
            Quantity = 5
        }
    };
    public List<ItemDTO> GreaterDuelRewards { get; set; } = new();
    public RectangleZoneDto AllArenasRectangleZone { get; set; } = new()
    {
    };
    public CoordinateDto FlashDuelWaitingRoom { get; set; } = new CoordinateDto
    {
        X = 0,
        Y = 0,
        Z = 0,
    };
    public List<ArenaLocationDto> Arenas { get; set; } = new() 
    { 
        new ArenaLocationDto
        {
            Location1 = new CoordinateDto()
            {
                X = 0,
                Y = 0,
                Z = 0
            },
            Location2 = new CoordinateDto()
            {
                X = 0,
                Y = 0,
                Z = 0
            }
        }
    };

    public List<TimeOnly> LesserFlashDuelTimes { get; set; } = new()
    {
        new TimeOnly(1, 0),
        new TimeOnly(13, 0),
        new TimeOnly(19, 0),
    };

    public List<TimeOnly> GreaterFlashDuelTimes { get; set; } = new()
    {
        new TimeOnly(1, 0),
        new TimeOnly(13, 0),
        new TimeOnly(19, 0),
    };
}

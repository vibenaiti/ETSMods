using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenWorldEvents;
using ProjectM;
using ModCore.Data;
using static ModCore.Configs.ConfigDtos;
using System.Threading;

public static class BossContestConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/OpenWorldEvents";
	private const string ConfigFileName = "boss_contest_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };
    public static BossContestConfigData Config { get; private set; } = new BossContestConfigData();

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
			Config = JsonSerializer.Deserialize<BossContestConfigData>(jsonData, SerializerOptions) ?? new BossContestConfigData();
		}
		catch (Exception ex)
		{
			Config = new BossContestConfigData();
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

public class BossContestConfigData
{
    public int BossLevelAbovePlayers { get; set; } = 5;
    public int SignupTimeSeconds { get; set; } = 60;
    public int MaxBossFightTime { get; set; } = 300;
    public int MaxPlayerLevelDifference { get; set; } = 10;
    public List<ItemDTO> WinnerRewards { get; set; } = new()
    {
        new()
        {
            ItemPrefabGUID = Prefabs.Item_Ingredient_DemonFragment,
            Quantity = 1
        }
    };
    public RectangleZoneDto AllArenasRectangleZone { get; set; } = new()
    {
    };
    public CoordinateDto BossContestWaitingRoom { get; set; } = new CoordinateDto
    {
        X = 0,
        Y = 0,
        Z = 0,
    };
    public List<BossArenaPairDto> ArenaPairs { get; set; } = new List<BossArenaPairDto>
    {
        new BossArenaPairDto
        {
            Arena1 = new BossArenaLocationDto
            {
                PlayerLocation = new CoordinateDto { X = 0, Y = 0, Z = 0 },
                BossLocation = new CoordinateDto { X = 0, Y = 0, Z = 0 }
            },
            Arena2 = new BossArenaLocationDto
            {
                PlayerLocation = new CoordinateDto { X = 0, Y = 0, Z = 0 },
                BossLocation = new CoordinateDto { X = 0, Y = 0, Z = 0 }
            }
        }
    };

    public List<TimeOnly> BossContestTimes { get; set; } = new()
    {
        new TimeOnly(1, 0),
        new TimeOnly(13, 0),
        new TimeOnly(19, 0),
    };
}

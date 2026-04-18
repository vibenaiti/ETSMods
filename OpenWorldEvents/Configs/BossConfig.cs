using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenWorldEvents;
using ProjectM;
using ModCore.Data;
using static ModCore.Configs.ConfigDtos;
using System.Threading;

public static class BossConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/OpenWorldEvents";
	private const string ConfigFileName = "boss_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };
    public static BossConfigData Config { get; private set; } = new BossConfigData();

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
			Config = JsonSerializer.Deserialize<BossConfigData>(jsonData, SerializerOptions) ?? new BossConfigData();
		}
		catch (Exception ex)
		{
			Config = new BossConfigData();
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

public class BossConfigData
{
    public List<ItemDTO> BossItems { get; set; } = new()
    {
        new ItemDTO()
    };

    public ItemDTO BossContributionRewards { get; set; } = new()
    {
        ItemPrefabGUID = Prefabs.Item_Ingredient_DemonFragment,
        Quantity = 100
    };

    public int DefaultBossLevel { get; set; } = 90;
    public int DefaultBossHP { get; set; } = 3000;
    public int BossDelaySeconds { get; set; } = 300;

    public List<TimeOnly> BossSpawnTimes { get; set; } = new()
    {
        new TimeOnly(15, 0),
        new TimeOnly(21, 0),
        new TimeOnly(3, 0),
    };
    public List<CoordinateDto> BossSpawnLocations { get; set; } = new()
    {
        new CoordinateDto
        {
            X = -1000f,
            Y = 0f,
            Z = -510f
        },
    };
}

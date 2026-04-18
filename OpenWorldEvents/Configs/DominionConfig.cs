using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenWorldEvents;
using ProjectM;
using ModCore.Data;
using static ModCore.Configs.ConfigDtos;
using System.Threading;

public static class DominionConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/OpenWorldEvents";
	private const string ConfigFileName = "dominion_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };
    public static DominionConfigData Config { get; private set; } = new DominionConfigData();

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
			Config = JsonSerializer.Deserialize<DominionConfigData>(jsonData, SerializerOptions) ?? new DominionConfigData();
		}
		catch (Exception ex)
		{
			Config = new DominionConfigData();
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

public class DominionConfigData
{
    public int DominionDelaySeconds { get; set; } = 300;
    public int DominionDuration { get; set; } = 300;
    public bool DisableDurabilityLossDuringEvent { get; set; } = true;
    public List<TimeOnly> DominionSpawnTimes { get; set; } = new()
    {
        new TimeOnly(5, 0),
        new TimeOnly(11, 0),
        new TimeOnly(17, 0),
        new TimeOnly(23, 0),
    };
    public List<CoordinateDto> DominionSpawnLocations { get; set; } = new()
    {
        new CoordinateDto
        {
            X = -1000f,
            Y = 0f,
            Z = -510f
        },
    };

    public List<ItemDTO> WinnerRewards { get; set; } = new()
    {
        new()
        {
            ItemPrefabGUID = Prefabs.Item_Ingredient_Document,
            Quantity = 1
        }
    };
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using GatedProgression;
using ModCore.Data;
using Stunlock.Core;
using static ModCore.Configs.ConfigDtos;

public static class GatedProgressionConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/GatedProgression";
	private const string ConfigFileName = "gated_progression_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };


    public static GatedProgressionConfigData Config { get; private set; } = new GatedProgressionConfigData();
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
			Config = JsonSerializer.Deserialize<GatedProgressionConfigData>(jsonData, SerializerOptions) ?? new GatedProgressionConfigData();
		}
		catch (Exception ex)
		{
			Config = new GatedProgressionConfigData();
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

public class GatedProgressionConfigData
{
    public Dictionary<string, HashSet<PrefabGUID>> LockedBossGroups { get; set; } = new()
    {
        { "day1", new HashSet<PrefabGUID>
            {
                Prefabs.CHAR_Bandit_Frostarrow_VBlood,
                Prefabs.CHAR_Bandit_Foreman_VBlood
            }
        },
        { "day2", new HashSet<PrefabGUID>
            {
                Prefabs.CHAR_Bandit_Frostarrow_VBlood,
                Prefabs.CHAR_Bandit_Foreman_VBlood
            }
        },
        { "day3", new HashSet<PrefabGUID>
            {
                Prefabs.CHAR_Bandit_Frostarrow_VBlood,
                Prefabs.CHAR_Bandit_Foreman_VBlood
            }
        },
        { "day4", new HashSet<PrefabGUID>
            {
                Prefabs.CHAR_Bandit_Frostarrow_VBlood,
                Prefabs.CHAR_Bandit_Foreman_VBlood
            }
        },
    };
}

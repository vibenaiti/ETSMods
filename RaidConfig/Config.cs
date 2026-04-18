using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using BepInEx.Configuration;
using ModCore.Services;
using RaidConfig;
using RaidConfig.Managers;
using static ModCore.Configs.ConfigDtos;

public static class RaidConfigConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/RaidConfig";
	private const string ConfigFileName = "raid_config_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

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

    public static RaidConfigConfigData Config { get; private set; } = new RaidConfigConfigData();

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
			Config = JsonSerializer.Deserialize<RaidConfigConfigData>(jsonData, SerializerOptions) ?? new RaidConfigConfigData();
            var action = () =>
            {
                Plugin.ModifyShardRepairFromEventCompletion();
                Plugin.ModifyGolemShield();
            };
            ActionScheduler.RunActionOnMainThread(action);
        }
		catch (Exception ex)
		{
			Config = new RaidConfigConfigData();
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

public class RaidConfigConfigData
{
    public int NaturalShardDecayInHours { get; set; } = 20;
    public float StoredShardDecayRate { get; set; } = 1;
    public float DurabilityPercentRestoreOnGateCompletion { get; set; } = 0.3f;
    public int NumberOfProtectedPrisons { get; set; } = 1;
    public int MaxUnitsPerVerminNest { get; set; } = -1;
    public int MaxUnitsPerTomb { get; set; } = -1;
    public int Day1GolemHP { get; set; } = 500;
    public int Day2GolemHP { get; set; } = 1000;
    public int Day3GolemHP { get; set; } = 1750;
    public int Day4GolemHP { get; set; } = 2500;
    public string MondayStartTime { get; set; } = "00:00:00";
    public string MondayEndTime { get; set; } = "00:00:00";
    public string TuesdayStartTime { get; set; } = "00:00:00";
    public string TuesdayEndTime { get; set; } = "00:00:00";
    public string WednesdayStartTime { get; set; } = "00:00:00";
    public string WednesdayEndTime { get; set; } = "00:00:00";
    public string ThursdayStartTime { get; set; } = "00:00:00";
    public string ThursdayEndTime { get; set; } = "00:00:00";
    public string FridayStartTime { get; set; } = "19:00:00";
    public string FridayEndTime { get; set; } = "22:00:00";
    public string SaturdayStartTime { get; set; } = "19:00:00";
    public string SaturdayEndTime { get; set; } = "22:00:00";
    public string SundayStartTime { get; set; } = "18:00:00";
    public string SundayEndTime { get; set; } = "21:00:00";
}

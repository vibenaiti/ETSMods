using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Achievements;
using ProjectM;
using ModCore.Data;
using static ModCore.Configs.ConfigDtos;
using System.Threading;
using Stunlock.Core;

public static class AchievementsConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/Achievements";
	private const string ConfigFileName = "achievements_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };


    public static AchievementsConfigData Config { get; private set; } = new AchievementsConfigData();
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
			Config = JsonSerializer.Deserialize<AchievementsConfigData>(jsonData, SerializerOptions) ?? new AchievementsConfigData();
		}
		catch (Exception ex)
		{
			Config = new AchievementsConfigData();
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

public class AchievementsConfigData
{
	public bool RewardAllBosses { get; set; } = false;
	public HashSet<PrefabGUID> BossesToReward { get; set; } = new() 
	{ 
		Prefabs.CHAR_Bandit_Tourok_VBlood,
    };

	public bool UsePhysicalCurrency { get; set; } = false;
	public PrefabGUID RewardPrefabGUID { get; set; } = Prefabs.Item_Ingredient_Plant_Thistle;
	public int QuantityPerBossKill { get; set; } = 25;
}

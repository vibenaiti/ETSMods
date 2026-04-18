using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using VipMod;
using static ModCore.Configs.ConfigDtos;

public static class VipModConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/VipMod";
	private const string ConfigFileName = "vip_mod_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public static VipModConfigData Config { get; private set; } = new VipModConfigData();
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
			Config = JsonSerializer.Deserialize<VipModConfigData>(jsonData, SerializerOptions) ?? new VipModConfigData();
		}
		catch (Exception ex)
		{
			Config = new VipModConfigData();
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

public class VipModConfigData
{
	public Dictionary<ulong, string> Vips { get; set; } = new()
	{
		{ 76561199119557999, "testplayer" }
	};
    public Dictionary<ulong, string> SuperVips { get; set; } = new()
    {
        { 76561199119557999, "testplayer" }
    };
    public int MaxPlayersNonVips { get; set; } = 50;
    public int MaxPlayersVips { get; set; } = 50;
    public bool DebugSkipAdminCheck { get; set; } = false;
    public int PointsPerDailyLogin { get; set; } = 5;
    public int VipPointsPerDailyLogin { get; set; } = 10;
    public int SuperVipPointsPerDailyLogin { get; set; } = 20;
}
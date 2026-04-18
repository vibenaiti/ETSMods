using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using DiscordBot;
using static ModCore.Configs.ConfigDtos;

public static class DiscordBotConfig
{
    private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/DiscordBot";
    private const string ConfigFileName = "discord_bot.json";
    private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

    public static DiscordBotConfigData Config { get; private set; } = new DiscordBotConfigData();
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
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var jsonData = File.ReadAllText(FullPath);
            Config = JsonSerializer.Deserialize<DiscordBotConfigData>(jsonData, options) ?? new DiscordBotConfigData();
        }
        catch (Exception ex)
        {
            Config = new DiscordBotConfigData();
            Plugin.PluginLog.LogInfo(ex.ToString());
        }
    }

    public static void Save()
    {
        try
        {
            EnsureConfigDirectory();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var jsonData = JsonSerializer.Serialize(Config, options);
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

public class DiscordBotConfigData
{
    public ulong GuildID { get; set; } = 0;
    public string Token { get; set; } = "";
}

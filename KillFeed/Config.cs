using ProjectM;
using System;
using System.IO;
using System.Text.Json;
using ModCore.Data;
using KillFeed;
using static ModCore.Configs.ConfigDtos;
using System.Collections.Generic;
using System.Threading;
using System.Text.Json.Serialization;
using UnityEngine;
using Stunlock.Core;

public static class KillFeedConfig
{
    private const string ConfigDirectoryName = "BepInEx/config/ETS/Config/KillFeed";
    private const string ConfigFileName = "killfeed.json";
    private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };
    public static KillFeedConfigData Config { get; private set; } = new KillFeedConfigData();
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
            Config = JsonSerializer.Deserialize<KillFeedConfigData>(jsonData, SerializerOptions) ?? new KillFeedConfigData();
        }
        catch (Exception ex)
        {
            Config = new KillFeedConfigData();
            Plugin.PluginLog.LogInfo(ex.ToString());
        }
    }

    public static void Save()
    {
        EnsureConfigDirectory();

        try
        {
            var jsonData = JsonSerializer.Serialize(Config, SerializerOptions);
            File.WriteAllText(FullPath, jsonData);
        }
        catch (Exception ex)
        {
            // Consider logging the exception if needed
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

public class KillFeedConfigData
{
    public bool UsePhysicalCurrency { get; set; } = false;
    public float ShardDurabilityGainedPerKill { get; set; } = 50;
    public int GriefKillLevelDifference { get; set; } = 15;
    public int CurrencyRewardedPerKill { get; set; } = 1;
    public int BonusCurrencyWhileWearingShard { get; set; } = 2;
    public string VirtualCurrencyName { get; set; } = "Points";

    [JsonIgnore]
    public Dictionary<int, KillStreakTitleInfo> KillStreakGainedTiers { get; set; } = new Dictionary<int, KillStreakTitleInfo>()
    {
        { 5, new KillStreakTitleInfo
            {
                Title = "Bloodthirsty!",
                TitleColor = ExtendedColor.GhostWhite,
                TitlePrefix = "is feeling "
            }
        },
        { 10, new KillStreakTitleInfo
            {
                Title = "Insatiable Hunger!",
                TitleColor = ExtendedColor.Maroon,
                TitlePrefix = "has an "
            }
        },
        { 15, new KillStreakTitleInfo
            {
                Title = "Sinister Desires!",
                TitleColor = ExtendedColor.Crimson,
                TitlePrefix = "is filled with "
            }
        },
        { 20, new KillStreakTitleInfo
            {
                Title = "Dreaded Destroyer!",
                TitleColor = ExtendedColor.Red,
                TitlePrefix = "the "
            }
        },
        { 25, new KillStreakTitleInfo
            {
                Title = "Fiendish Presence!",
                TitleColor = ExtendedColor.BestRed,
                TitlePrefix = "commands a "
            }
        },
        { 30, new KillStreakTitleInfo
            {
                Title = "Shadowy Slaughterer!",
                TitleColor = ExtendedColor.DarkViolet,
                TitlePrefix = "is a "
            }
        },
        { 35, new KillStreakTitleInfo
            {
                Title = "Unholy Might!",
                TitleColor = ExtendedColor.LawnGreen,
                TitlePrefix = "'s veins fill with "
            }
        },
        { 40, new KillStreakTitleInfo
            {
                Title = "Server Reaper!",
                TitleColor = ExtendedColor.Turquoise,
                TitlePrefix = "has become the "
            }
        },
        { 45, new KillStreakTitleInfo
            {
                Title = "",
                TitleColor = ExtendedColor.GhostWhite,
                TitlePrefix = "beat all odds. "
            }
        }
    };

    [JsonIgnore]
    public Dictionary<int, KillStreakTitleInfo> KillStreakLostTiers { get; set; } = new Dictionary<int, KillStreakTitleInfo>()
    {
        { 5, new KillStreakTitleInfo
            {
                NameExtension = "'s",
                Title = "thirst for blood!",
                TitleColor = ExtendedColor.GhostWhite,
                TitlePrefix = "has ended"
            }
        },
        { 10, new KillStreakTitleInfo
            {
                NameExtension = "'s",
                Title = "hunger!",
                TitleColor = ExtendedColor.Maroon,
                TitlePrefix = "starved"
            }
        },
        { 15, new KillStreakTitleInfo
            {
                NameExtension = "'s",
                Title = "desire for death!",
                TitleColor = ExtendedColor.Crimson,
                TitlePrefix = "fulfilled"
            }
        },
        { 20, new KillStreakTitleInfo
            {
                NameExtension = "'s",
                Title = "doom!",
                TitleColor = ExtendedColor.Red,
                TitlePrefix = "is"
            }
        },
        { 25, new KillStreakTitleInfo
            {
                NameExtension = "'s",
                Title = "fiendish presence!",
                TitleColor = ExtendedColor.BestRed,
                TitlePrefix = "cut down"
            }
        },
        { 30, new KillStreakTitleInfo
            {
                NameExtension = "'s",
                Title = "darkness!",
                TitleColor = ExtendedColor.DarkViolet,
                TitlePrefix = "dispersed"
            }
        },
        { 35, new KillStreakTitleInfo
            {
                NameExtension = "'s",
                Title = "might!",
                TitleColor = ExtendedColor.LawnGreen,
                TitlePrefix = "ended"
            }
        },
        { 40, new KillStreakTitleInfo
            {
                Title = "of Server Reaper!",
                TitleColor = ExtendedColor.Turquoise,
                TitlePrefix = "has cleansed"
            }
        },
        { 45, new KillStreakTitleInfo
            {
                Title = "against all odds.".Bold(),
                TitleColor = ExtendedColor.GhostWhite,
                TitlePrefix = "has defeated"
            }
        }
    };

    public PrefabGUID CurrencyPrefab { get; set; } = Prefabs.Item_Ingredient_Plant_Thistle;
    public class KillStreakTitleInfo
    {
        public string TitlePrefix { get; set; } = "";
        public string Title { get; set; } = "";
        public string NameExtension { get; set; } = "";
        public Color32 TitleColor { get; set; } = ExtendedColor.BestRed;
        public PrefabGUID BuffGuid { get; set; }
        public ModifyUnitStatBuff_DOTS StatBuff { get; set; }
    }
}

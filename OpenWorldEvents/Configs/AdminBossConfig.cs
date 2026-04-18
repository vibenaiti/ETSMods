using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenWorldEvents;
using ProjectM;
using Stunlock.Core;
using ModCore.Data;
using static ModCore.Configs.ConfigDtos;
using System.Threading;
using ModCore.Models;

public static class AdminBossConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/OpenWorldEvents";
	private const string ConfigFileName = "admin_boss_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
    private static JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };
    public static AdminBossConfigData Config { get; private set; } = new AdminBossConfigData();

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
			Config = JsonSerializer.Deserialize<AdminBossConfigData>(jsonData, SerializerOptions) ?? new AdminBossConfigData();
		}
		catch (Exception ex)
		{
			Config = new AdminBossConfigData();
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

public class AdminBossConfigData
{
    public Dictionary<string, AdminBoss> AdminBosses { get; set; } = new()
    {
        {
            "willis", new AdminBoss()
        },
        {
            // Visual: none (vampire model). Abilities: storm shift + configurable.
            "storm", new AdminBoss
            {
                BossName = "Storm",
                Health = 5000,
                PhysicalPower = 80,
                SpellPower = 80,
                CooldownRecoveryRate = 2f,
                SpeedModifier = 5f,
                AttackSpeedModifier = 1.5f,
                AbilityBar = new()
                {
                    Auto   = Prefabs.AB_Storm_PolarityShift_AbilityGroup,
                }
            }
        },
        {
            // Visual: Werewolf VBlood form.
            // ShapeshiftBuffGUID: Buff_General_Shapeshift_Werewolf_VBlood (-622259665)
            // Abilities: WerewolfChieftain kit (MeleeAttack auto, ShadowDash, MultiBite, Knockdown, Stealth)
            "werewolf", new AdminBoss
            {
                BossName = "Willfred the Werewolf",
                ShapeshiftBuffGUID = new PrefabGUID(-622259665),
                Health = 6000,
                PhysicalPower = 90,
                SpellPower = -1,
                CooldownRecoveryRate = 1.5f,
                SpeedModifier = 5.5f,
                AttackSpeedModifier = 1.5f,
                AbilityBar = new()
                {
                    Auto   = Prefabs.AB_WerewolfChieftain_MeleeAttack_AbilityGroup,  // -988264305
                    Dash   = Prefabs.AB_WerewolfChieftain_ShadowDash_AbilityGroup,   // -566065717
                    Spell1 = Prefabs.AB_WerewolfChieftain_MultiBite_AbilityGroup,    // -174926399
                    Spell2 = Prefabs.AB_WerewolfChieftain_Knockdown_AbilityGroup,    // 1445822330
                    Ult    = Prefabs.AB_WerewolfChieftain_Stealth_AbilityGroup,      // -192549213
                }
            }
        },
        {
            // Visual: Gargoyle (Tailor phase 2).
            // ShapeshiftBuffGUID: AB_Tailor_Shapeshift_Gargoyle_Buff (-395216184)
            // Abilities: Legion Gargoyle kit (ForwardSwipe auto, FlyStart dash, WingShield, FlyEnd)
            "gargoyle", new AdminBoss
            {
                BossName = "Gargoyle",
                ShapeshiftBuffGUID = new PrefabGUID(-395216184),
                Health = 5000,
                PhysicalPower = 75,
                SpellPower = -1,
                CooldownRecoveryRate = 1.5f,
                SpeedModifier = 5f,
                AttackSpeedModifier = 1.2f,
                AbilityBar = new()
                {
                    Auto   = Prefabs.AB_Legion_Gargoyle_ForwardSwipe_AbilityGroup,  // -915929892
                    Dash   = Prefabs.AB_Gargoyle_FlyStart_AbilityGroup,             // -382913708
                    Spell1 = Prefabs.AB_Gargoyle_WingShield_AbilityGroup,           // 1460741503
                    Ult    = Prefabs.AB_Gargoyle_FlyEnd_AbilityGroup,               // 1563014858
                }
            }
        },
        {
            // Visual: Bear form. Bear abilities applied by the shapeshift buff itself — no extra override needed.
            // ShapeshiftBuffGUID: AB_Shapeshift_Bear_Buff (-1569370346)
            "bear", new AdminBoss
            {
                BossName = "Bear",
                ShapeshiftBuffGUID = new PrefabGUID(-1569370346),
                Health = 6000,
                PhysicalPower = 95,
                SpellPower = -1,
                CooldownRecoveryRate = 1.5f,
                SpeedModifier = 5f,
                AttackSpeedModifier = 1.3f,
            }
        },
        {
            // Visual: Geomancer golem form.
            // ShapeshiftBuffGUID: AB_Geomancer_Transform_ToGolem_Buff (174249800)
            // Abilities: Geomancer kit (RockSlam auto, EnragedSmash, UndergroundTremors, RaiseGuardians)
            "geomancer", new AdminBoss
            {
                BossName = "Geomancer",
                ShapeshiftBuffGUID = new PrefabGUID(174249800),
                Health = 7000,
                PhysicalPower = 100,
                SpellPower = -1,
                CooldownRecoveryRate = 1.2f,
                SpeedModifier = 4f,
                AttackSpeedModifier = 1f,
                AbilityBar = new()
                {
                    Auto   = Prefabs.AB_Geomancer_RockSlam_AbilityGroup,                // -221719333
                    Spell1 = Prefabs.AB_Geomancer_EnragedSmash_AbilityGroup,            // 1079488801
                    Spell2 = Prefabs.AB_Geomancer_UndergroundTremmors_AbilityGroup,     // -1148606177
                    Ult    = Prefabs.AB_Geomancer_Golem_RaiseGuardians_AbilityGroup,    // -598112885
                }
            }
        },
    };
}

public class AdminBoss
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
    public string BossName = "Willis";
    public int Health { get; set; } = 3000;
    public int PhysicalPower { get; set; } = 50;
    public int SpellPower { get; set; } = 50;
    public float CooldownRecoveryRate { get; set; } = 1.5f;
    public float SpeedModifier { get; set; } = 4.2f;
    public float AttackSpeedModifier { get; set; } = 1.2f;
    public bool CCImmune { get; set; } = true;
    /// <summary>
    /// Optional shapeshift buff for visual/model transformation.
    ///   Werewolf VBlood : -622259665  (Buff_General_Shapeshift_Werewolf_VBlood)
    ///   Gargoyle Tailor : -395216184  (AB_Tailor_Shapeshift_Gargoyle_Buff)
    ///   Geomancer Golem : 174249800   (AB_Geomancer_Transform_ToGolem_Buff)
    ///   Bear            : -1569370346 (AB_Shapeshift_Bear_Buff) — built-in abilities
    /// </summary>
    public PrefabGUID? ShapeshiftBuffGUID { get; set; } = null;
    /// <summary>
    /// Optional CHAR_ prefab to copy AbilityGroupSlotBuffer from (gives boss abilities to player).
    ///   Werewolf : 2079933370  (CHAR_WerewolfChieftain_VBlood_GateBoss_Major)
    ///   Gargoyle : -65981941   (CHAR_Legion_Gargoyle)
    ///   Geomancer: -1065970933 (CHAR_Geomancer_Human_VBlood)
    ///   Bear     : not needed — bear shapeshift sets abilities natively
    /// </summary>
    public PrefabGUID? SourcePrefabGUID { get; set; } = null;
    public AbilityBar AbilityBar { get; set; } = new()
    {
        Auto = Prefabs.AB_Storm_PolarityShift_AbilityGroup
    };
}

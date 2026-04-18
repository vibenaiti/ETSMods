using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProjectM;
using ShopMod;
using ModCore.Data;
using static ModCore.Configs.ConfigDtos;
using System.Threading;
using Stunlock.Core;
using ModCore.Services;

public static class ShopModConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Config/ShopMod";
	private const string ConfigFileName = "shop_mod.json";
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

    public static ShopModConfigData Config { get; private set; } = new ShopModConfigData();

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
            Config = JsonSerializer.Deserialize<ShopModConfigData>(jsonData, SerializerOptions) ?? new ShopModConfigData();
            var action = () => Plugin.MakeSpecialCurrenciesSoulbound();
            ActionScheduler.RunActionOnMainThread(action);
        }
		catch (Exception ex)
		{
			Config = new ShopModConfigData();
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

public class ShopModConfigData
{
	public bool ShopEnabled { get; set; } = true;
	public PrefabGUID SpecialPhysicalCurrency { get; set; } = Prefabs.Item_Ingredient_Crystal;
	public List<PrefabGUID> PrefabGUIDsToNotDropOnDeath{ get; set; } = new() 
	{ 
		Prefabs.Item_Ingredient_Plant_Thistle
    };

	public List<RectangleZoneDto> ShopRectangleZones { get; set; } = new()
	{
		new RectangleZoneDto
		{
			Bottom = 0,
			Top = 0,
			Left = 0,
			Right = 0
		}
	};

	public List<TraderDto> Traders { get; set; } = new List<TraderDto>
	{
		new TraderDto
		{
			TraderItems = new() 
			{
				new TraderItemDto
				{
					InputItem = Prefabs.Item_Ingredient_DemonFragment,
					InputAmount = 1,
					OutputItem = Prefabs.Item_Ingredient_Plant_Thistle,
					OutputAmount = 50,
					AutoRefill = true,
					StockAmount = 1,
                }
			},
			UnitSpawn = new() 
			{
                PrefabGUID = Prefabs.CHAR_Trader_Farbane_RareGoods_T01,
				Description = "demonicfragment"
            }
		}
	};

	public ItemDTO JewelCost { get; set; } = new ItemDTO
	{
		ItemPrefabGUID = Prefabs.Item_Ingredient_Plant_Thistle,
		Quantity = 100
	};

    public ItemDTO LegendaryCost { get; set; } = new ItemDTO
    {
        ItemPrefabGUID = Prefabs.Item_Ingredient_Plant_Thistle,
        Quantity = 100
    };

    public ItemDTO ServantUpgradeCost { get; set; } = new ItemDTO
    {
        ItemPrefabGUID = Prefabs.Item_Ingredient_Plant_Thistle,
        Quantity = 25
    };

    public ItemDTO PrisonerCost { get; set; } = new ItemDTO
    {
        ItemPrefabGUID = Prefabs.Item_Ingredient_Plant_Thistle,
        Quantity = 100
    };

    public ItemDTO HorseUpgradeCost { get; set; } = new ItemDTO
    {
        ItemPrefabGUID = Prefabs.Item_Ingredient_Plant_Thistle,
        Quantity = 100
    };

    public float ServantUpgradeAmount { get; set; } = 0.1f;

    public bool PreventHatDropOnDeath { get; set; } = true;
    public float ChanceOfNamedArtifactFromCustomVendor { get; set; } = 0.05f;
    public HashSet<string> SpecialTraderNames { get; set; } = new()
    {
        "hats",
        "cloaks",
        "seeds"
    };
    public List<ItemDTO> Day1KitItems { get; set; } = new()
    {
        new ItemDTO
        {
            ItemPrefabGUID = Prefabs.Item_Weapon_Axe_T03_Copper,
            Quantity = 1
        }
    };
    public List<ItemDTO> Day2KitItems { get; set; } = new() 
    { 
        new ItemDTO
        {
            ItemPrefabGUID = Prefabs.Item_Weapon_Axe_T03_Copper,
            Quantity = 1
        }
    };
    public List<ItemDTO> Day3KitItems { get; set; } = new();
    public List<ItemDTO> Day4AndBeyondKitItems { get; set; } = new();
}

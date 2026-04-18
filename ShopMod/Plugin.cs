using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM;
using ProjectM.Shared;
using ShopMod.Managers;
using System.Reflection;
using Unity.Collections;
using ModCore;
using ModCore.Data;
using ModCore.Helpers;
using ModCore.Services;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using Il2CppSystem.Runtime.Remoting.Messaging;
using System.Collections.Generic;
using Stunlock.Core;
using static ProjectM.SpawnBuffsAuthoring.SpawnBuffElement_Editor;
using ModCore.Events;

namespace ShopMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ModCore")]
[BepInDependency("PointsMod")]
public class Plugin : BasePlugin
{
	internal static Harmony Harmony;
	internal static ManualLogSource PluginLog;

	public override void Load()
	{
		PluginLog = Log;
		// Plugin startup logic
		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
		// Harmony patching
		Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		Harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
    	GameEvents.OnServerStart += OnServerStart;
    }

	public override bool Unload()
	{
		Harmony?.UnpatchSelf();
        ShopZoneManager.Dispose();
        CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        ShopModConfig.Dispose();
        LegendaryVendorManager.Dispose();
        return true;
    }

	

	private static void ModifyPrefabs()
    {
        foreach (var prefabGuid in ShopModConfig.Config.PrefabGUIDsToNotDropOnDeath)
        {
            var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(prefabGuid);

            if (prefabEntity.Has<ItemData>())
            {
                var itemData = prefabEntity.Read<ItemData>();
                itemData.ItemCategory |= ItemCategory.BloodBound;
                itemData.ItemCategory &= ~ItemCategory.TeleportBound; //this doesn't work
                prefabEntity.Write(itemData);

                var map = Core.gameDataSystem.ItemHashLookupMap;
                map[prefabGuid] = itemData;
            }
        }

        if (ShopModConfig.Config.PreventHatDropOnDeath)
        {
            var prefabEntities = Helper.GetPrefabEntitiesByComponentTypes<ItemData, EquippableData>();
            foreach (var prefabEntity in prefabEntities)
            {
                var equippableData = prefabEntity.Read<EquippableData>();
                if (equippableData.EquipmentType == EquipmentType.Headgear)
                {
                    var itemData = prefabEntity.Read<ItemData>();
                    itemData.ItemCategory |= ItemCategory.BloodBound;
                    prefabEntity.Write(itemData);

                    var map = Core.gameDataSystem.ItemHashLookupMap;
                    map[prefabEntity.GetPrefabGUID()] = itemData;
                }
            }
        }

        SetDemonicFragmentStackSize();
        MakeSpecialCurrenciesSoulbound();
        RemoveCosmeticsFromDropTables();
    }

    public static void MakeSpecialCurrenciesSoulbound()
    {
        var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(ShopModConfig.Config.SpecialPhysicalCurrency);
        var itemData = prefabEntity.Read<ItemData>();
        itemData.MaxAmount = 4000;
        itemData.ItemCategory |= ItemCategory.SoulBound;
        prefabEntity.Write(itemData);
        var map = Core.gameDataSystem.ItemHashLookupMap;
        map[ShopModConfig.Config.SpecialPhysicalCurrency] = itemData;
    }

    private static void SetDemonicFragmentStackSize()
    {
        var demonicFragment = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Item_Ingredient_DemonFragment);
        var itemData = demonicFragment.Read<ItemData>();
        itemData.MaxAmount = 1;
        demonicFragment.Write(itemData);
        var map = Core.gameDataSystem.ItemHashLookupMap;
        map[Prefabs.Item_Ingredient_DemonFragment] = itemData;
    }

    private static void RemoveCosmeticsFromDropTables()
    {
        var entities = Helper.GetPrefabEntitiesByComponentTypes<ItemDataDropGroupBuffer>();
        var itemsToRemove = new List<int>();
        foreach (var entity in entities)
        {
            var buffer = entity.ReadBuffer<ItemDataDropGroupBuffer>();
            for (var i = 0; i < buffer.Length; i++)
            {
                var itemDataDropGroup = buffer[i];
                if (itemDataDropGroup.Type == DropItemType.Item)
                {
                    var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(itemDataDropGroup.DropItemPrefab);
                    if (prefabEntity.Has<EquippableData>())
                    {
                        var equippableData = prefabEntity.Read<EquippableData>();
                        if (equippableData.EquipmentType == EquipmentType.Headgear || equippableData.EquipmentType == EquipmentType.Cloak)
                        {
                            itemsToRemove.Add(i);
                        }
                    }
                }
            }
            for (int i = itemsToRemove.Count - 1; i >= 0; i--)
            {
                buffer.RemoveAt(itemsToRemove[i]);
            }
            itemsToRemove.Clear();
        }
        entities.Dispose();

        entities = Helper.GetPrefabEntitiesByComponentTypes<DropTableDataBuffer>();
        foreach (var entity in entities)
        {
            var buffer = entity.ReadBuffer<DropTableDataBuffer>();
            for (var i = 0; i < buffer.Length; i++)
            {
                var itemDataDropGroup = buffer[i];
                if (itemDataDropGroup.ItemType == DropItemType.Item)
                {
                    var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(itemDataDropGroup.ItemGuid);
                    if (prefabEntity.Has<EquippableData>())
                    {
                        var equippableData = prefabEntity.Read<EquippableData>();
                        if (equippableData.EquipmentType == EquipmentType.Headgear || equippableData.EquipmentType == EquipmentType.Cloak)
                        {
                            itemsToRemove.Add(i);
                        }
                    }
                }
            }
            for (int i = itemsToRemove.Count - 1; i >= 0; i--)
            {
                buffer.RemoveAt(itemsToRemove[i]);
            }
            itemsToRemove.Clear();
        }
    
	}
	private static void OnServerStart()
	{

		ModifyPrefabs();
        ShopModConfig.Initialize();
        CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        ShopZoneManager.Initialize();
        //LegendaryVendorManager.Initialize();
		DataStorage.Load();
		//PointsManager.Initialize();
	}
}
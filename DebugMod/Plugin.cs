using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using DebugMod.Commands.Waypoints;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using System.Reflection;
using ModCore.Listeners;
using ModCore.Services;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using ModCore.Data;
using System.Collections.Generic;
using Stunlock.Core;
using ModCore;
using DebugMod.Managers;
using ProjectM.Gameplay.Scripting;
using Unity.Collections;
using DebugMod.Listeners;
using Unity.Entities;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using ProjectM.Shared;
using ModCore.Events;

namespace DebugMod;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
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
        CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        FromCharacterListener.Dispose();
        TestManager.Dispose();
        AdminModeManager.Dispose();
        AutoReviveManager.Dispose();
        SpectatingManager.Dispose();
        DebugModConfig.Dispose();
        RenameManager.Dispose();
        CursedForestManager.Dispose();
        SunProtectionZoneManager.Dispose();
        FreeBuildManager.Dispose();
        InventoryChangedListener.Dispose();
        UnstuckManager.Dispose();
        return true;
    }

	

	public static void ModifyPrefabs()
    {
        ModifyBearForm();
        ModifyCurseBuff();
        MakeShatteredWeaponsDropOnDeath();
        ModifyWeaponLevelOfRiftCrystals();
        ModifyBloodEssenceStackSize();
    }

    private static void ModifyBloodEssenceStackSize()
    {
        var prefabBlood = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Item_BloodEssence_T01);
        var itemData = prefabBlood.Read<ItemData>();
        itemData.MaxAmount = 250;
        prefabBlood.Write(itemData);
        if (Core.gameDataSystem.ItemHashLookupMap.TryGetValue(Prefabs.Item_BloodEssence_T01, out var itemData2))
        {
            itemData2.MaxAmount = 250;
            var itemDataLookupMap = Core.gameDataSystem.ItemHashLookupMap;
            itemDataLookupMap[Prefabs.Item_BloodEssence_T01] = itemData2;
        }
    }

    private static void ModifyWeaponLevelOfRiftCrystals()
    {
        var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.TM_Noctem_RiftCrystalChild_Tier02);
        var entityCategory = prefabEntity.Read<EntityCategory>();
        entityCategory.ResourceLevel._Value = 70;
        prefabEntity.Write(entityCategory);

        var entities = Helper.GetEntitiesByComponentTypes<EntityCategory, YieldResourcesOnDamageTaken>(EntityQueryOptions.IncludeDisabledEntities);
        foreach (var entity in entities)
        {
            if (entity.GetPrefabGUID() == Prefabs.TM_Noctem_RiftCrystalChild_Tier02)
            {
                var category = entity.Read<EntityCategory>();
                category.ResourceLevel._Value = 70;
                entity.Write(category);
            }

        }
    }

    private static void MakeShatteredWeaponsDropOnDeath()
    {
        var entities = Helper.GetPrefabEntitiesByComponentTypes<ShatteredItem>();
        foreach (var entity in entities)
        {
            var itemData = entity.Read<ItemData>();
            itemData.ItemCategory &= ~ItemCategory.BloodBound;
            entity.Write(itemData);

            var map = Core.gameDataSystem.ItemHashLookupMap;
            map[entity.GetPrefabGUID()] = itemData;
        }
    }

    private static void ModifyCurseBuff()
    {
        var curseBuff = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Buff_General_CurseOfTheForest_Area);
        var curseAreaDebuffData = curseBuff.Read<Script_CursedAreaDebuff_DataServer>();
        curseAreaDebuffData.ImmunityBuff = PrefabGUID.Empty;
        curseBuff.Write(curseAreaDebuffData);

        var wispBuff = Helper.GetPrefabEntityByPrefabGUID(Prefabs.AB_Interact_Curse_Wisp_Buff);
        var lifetime = wispBuff.Read<LifeTime>();
        lifetime.Duration = 60;
        wispBuff.Write(lifetime);
    }

    private static void ModifyBearForm()
    {
        var bearBuffs = new List<PrefabGUID>
        {
            Prefabs.AB_Shapeshift_Bear_Buff,
            Prefabs.AB_Shapeshift_Bear_Skin01_Buff,
        };
        foreach (var bearBuff in bearBuffs)
        {
            var buffEntity = Helper.GetPrefabEntityByPrefabGUID(bearBuff);
            var buffer = buffEntity.ReadBuffer<ReplaceAbilityOnSlotBuff>();
            for (var i = 0; i < buffer.Length; i++)
            {
                var replaceAbilityOnSlotBuff = buffer[i];
                if (i == 1) //q
                {
                    replaceAbilityOnSlotBuff.NewGroupId = Prefabs.AB_Bear_Shapeshift_AreaAttack_Group;
                }
                else if (i == 3) //t
                {
                    replaceAbilityOnSlotBuff.NewGroupId = Prefabs.AB_Bear_Dire_Dash_AbilityGroup;
                }
                else if (i == 6) // c
                {
                    replaceAbilityOnSlotBuff.NewGroupId = Prefabs.AB_Bear_Dire_AreaAttack_AbilityGroup;
                }
                buffer[i] = replaceAbilityOnSlotBuff;
            }
        }
    
	}
	private static void OnServerStart()
	{

		DebugModConfig.Initialize();
        CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        TestManager.Initialize();
        AdminModeManager.Initialize();
        //AutoReviveManager.Initialize();
        WaypointManager.LoadWaypoints();
		RenameManager.Initialize();
        CursedForestManager.Initialize();
        SunProtectionZoneManager.Initialize();
        UnstuckManager.Initialize();
        if (FreeBuildManager.IsFreeBuildEnabled())
        {
            FreeBuildManager.Initialize();
        }
		ModifyPrefabs();

        //InventoryChangedListener.Initialize();
        //FromCharacterListener.Initialize();
	}
}
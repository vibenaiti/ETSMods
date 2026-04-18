using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM;
using System.Reflection;
using Unity.Collections;
using ModCore.Services;
using ModCore;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using Unity.Mathematics;
using ModCore.Data;
using ModCore.Helpers;
using RaidConfig.Managers;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;
using System.Collections.Generic;
using static ProjectM.SpawnBuffsAuthoring.SpawnBuffElement_Editor;
using ModCore.Events;

namespace RaidConfig;


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
        RaidTimeManager.Dispose();
        RaidManager.Dispose();
        RaidConfigConfig.Dispose();
        ShardManager.Dispose();
        return true;
    }

    private static void FixCastleAttackTimer()
    {
        var serverGameSettingsSystem = VWorld.Server.GetExistingSystemManaged<ServerGameSettingsSystem>();
        var castleAttackTimer = serverGameSettingsSystem._Settings.CastleUnderAttackTimer;
        var entity = Helper.GetEntitiesByComponentTypes<ServerGameBalanceSettings>()[0];
        var serverGameBalanceSettings = entity.Read<ServerGameBalanceSettings>();
        serverGameBalanceSettings.CastleUnderAttackTimer = new half(castleAttackTimer);
        entity.Write(serverGameBalanceSettings);
    }

	private static void ModifyPrefabs()
	{
		if (RaidConfigConfig.Config.MaxUnitsPerVerminNest > 0)
		{
            var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.TM_UnitStation_VerminNest);
            var unitSpawnerStation = prefabEntity.Read<UnitSpawnerstation>();
            unitSpawnerStation.MaxSpawnedUnits = RaidConfigConfig.Config.MaxUnitsPerVerminNest;
            prefabEntity.Write(unitSpawnerStation);
        }
		
		if (RaidConfigConfig.Config.MaxUnitsPerTomb > 0)
		{
            var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.TM_UnitStation_Tomb);
            var unitSpawnerStation = prefabEntity.Read<UnitSpawnerstation>();
            unitSpawnerStation.MaxSpawnedUnits = RaidConfigConfig.Config.MaxUnitsPerTomb;
            prefabEntity.Write(unitSpawnerStation);
        }

        ModifyShardRepairFromEventCompletion();
        ModifyShardDespawnTime();
        //AllowShardholdersToUseBatform();
        MakeShardsNormalAgain();
        ModifyGolemShield();
    }

    public static void ModifyGolemShield()
    {
        var daysPassed = Helper.GetServerTimeAdjusted() / 60 / 60 / 24;
        var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.AB_Shapeshift_Golem_T02_Buff);
        var absorbBuff = prefabEntity.Read<AbsorbBuff>();
        int shieldAmount;
        if (daysPassed < 1)
        {
            shieldAmount = RaidConfigConfig.Config.Day1GolemHP;
        }
        else if (daysPassed < 2)
        {
            shieldAmount = RaidConfigConfig.Config.Day2GolemHP;
        }
        else if (daysPassed < 3)
        {
            shieldAmount = RaidConfigConfig.Config.Day3GolemHP;
        }
        else
        {
            shieldAmount = RaidConfigConfig.Config.Day4GolemHP;
        }
        prefabEntity.Remove<SiegeWeaponAbsorbCapByServerSettings>();
        absorbBuff.AbsorbValue = shieldAmount;
        absorbBuff.AbsorbCap = shieldAmount;
        prefabEntity.Write(absorbBuff);
    }

    public static void ModifyShardDespawnTime()
    {
        var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Resource_Drop_SoulShard);
        var destroyAfterDuration = prefabEntity.Read<DestroyAfterDuration>();
        destroyAfterDuration.Duration = 60 * 15;
        prefabEntity.Write(destroyAfterDuration);
    }

    public static void ModifyShardRepairFromEventCompletion()
	{
        if (RaidConfigConfig.Config.DurabilityPercentRestoreOnGateCompletion >= 0)
        {
            var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.AB_FeedGateBoss_04_Complete_AreaTriggerBuff);
            var buffer = prefabEntity.ReadBuffer<ModifyItemDurabilityOnGameplayEvent>();
            for (var i = 0; i < buffer.Length; i++)
            {
                var modifyItemDurabilityOnGameplayEvent = buffer[i];
                modifyItemDurabilityOnGameplayEvent.DurabilityFactor = RaidConfigConfig.Config.DurabilityPercentRestoreOnGateCompletion;
                buffer[i] = modifyItemDurabilityOnGameplayEvent;
            }
        }

        if (RaidConfigConfig.Config.NaturalShardDecayInHours > 0)
        {
            var prefabEntities = Helper.GetPrefabEntitiesByComponentTypes<Relic, LoseDurabilityOverTime>();
            foreach (var prefabEntity in prefabEntities)
            {
                var loseDurabilityOverTime = prefabEntity.Read<LoseDurabilityOverTime>();
                loseDurabilityOverTime.TimeUntilBroken = RaidConfigConfig.Config.NaturalShardDecayInHours * 60 * 60;
                prefabEntity.Write(loseDurabilityOverTime);
            }

            var entities = Helper.GetEntitiesByComponentTypes<Relic, LoseDurabilityOverTime>();
            foreach (var entity in entities)
            {
                var loseDurabilityOverTime = entity.Read<LoseDurabilityOverTime>();
                loseDurabilityOverTime.TimeUntilBroken = RaidConfigConfig.Config.NaturalShardDecayInHours * 60 * 60;
                entity.Write(loseDurabilityOverTime);
            }
        }
    }

    private static void MakeShardsNormalAgain()
    {
        var itemMap = Core.gameDataSystem.ItemHashLookupMap;
        var shards = Helper.GetEntitiesByComponentTypes<Relic, LoseDurabilityOverTime>(EntityQueryOptions.IncludeAll);
        foreach (var shard in shards)
        {
            var itemData = shard.Read<ItemData>();
            itemData.ItemCategory = ItemCategory.Soulshard;
            shard.Write(itemData);
            itemMap[shard.Read<PrefabGUID>()] = itemData;
        }

        List<PrefabGUID> shardContainers = new()
        {
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Dracula,
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Manticore,
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Monster,
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Solarus,
        };
        foreach (var shardContainer in shardContainers)
        {
            var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(shardContainer);
            var buffer = prefabEntity.ReadBuffer<InventoryInstanceElement>();
            for (var i = 0; i < buffer.Length; i++)
            {
                var item = buffer[i];
                item.RestrictedCategory = (long)ItemCategory.Soulshard;
                buffer[i] = item;
            }
        }

        var storageEntities = Helper.GetEntitiesByComponentTypes<RestrictedInventory>(EntityQueryOptions.IncludeDisabledEntities);
        foreach (var storageEntity in storageEntities)
        {
            if (shardContainers.Contains(storageEntity.GetPrefabGUID()))
            {
                var restrictedInventory = storageEntity.Read<RestrictedInventory>();
                restrictedInventory.RestrictedItemCategory = ItemCategory.Soulshard;
                storageEntity.Write(restrictedInventory);
            }
        }
    }

    /*private static void AllowShardholdersToUseBatform()
    {
        var itemMap = Core.gameDataSystem.ItemHashLookupMap;
        var shards = Helper.GetEntitiesByComponentTypes<Relic, LoseDurabilityOverTime>(EntityQueryOptions.IncludeAll);
        foreach (var shard in shards)
        {
            var itemData = shard.Read<ItemData>();
            //itemData.ItemCategory &= ~ItemCategory.Soulshard;
            itemData.ItemCategory = ItemCategory.Magic;
            shard.Write(itemData);
            itemMap[shard.Read<PrefabGUID>()] = itemData;
        }

        List<PrefabGUID> shardContainers = new()
        {
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Dracula,
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Manticore,
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Monster,
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Solarus,
        };
        foreach (var shardContainer in shardContainers)
        {
            var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(shardContainer);
            var buffer = prefabEntity.ReadBuffer<InventoryInstanceElement>();
            for (var i = 0; i < buffer.Length; i++)
            {
                var item = buffer[i];
                item.RestrictedCategory = (long)ItemCategory.ALL;
                buffer[i] = item;
            }
        }
    }*/


	private static void OnServerStart()
	{
		RaidConfigConfig.Initialize();
        CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        RaidTimeManager.Initialize();
		RaidManager.Initialize();
		ShardManager.Initialize();
		FixCastleAttackTimer();
		ModifyPrefabs();
	}
}

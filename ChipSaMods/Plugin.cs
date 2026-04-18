using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM;
using System.Reflection;
using ModCore.Services;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Events;
using ChipSaMod.Managers;
using ModCore.Data;
using ModCore.Helpers;
using ModCore;
using static ProjectM.SpawnBuffsAuthoring.SpawnBuffElement_Editor;
using Unity.Collections;
using Stunlock.Core;
using ProjectM.Shared;
using System.Collections.Generic;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;

namespace ChipSaMod;


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
        ChipSaManager.Dispose();
        CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        ChipSaModConfig.Dispose();
        return true;
    }

	

	private static void ModifyPrefabs()
	{
		ModifyGolemShield();
		ModifyConsumables();
        ModifyDropOnDeath();
        ModifyDropTables();

        var startingTombPrefab = Helper.GetPrefabEntityByPrefabGUID(Prefabs.TM_Respawn_TombCoffin);
        startingTombPrefab.Add<DestroyOnSpawn>();
    }

	public static void DisableBuildCosts()
	{
        SetDebugSettingEvent BuildCostsDisabledSetting = new SetDebugSettingEvent()
        {
            SettingType = DebugSettingType.BuildCostsDisabled,
            Value = true
        };

        var debugEventsSystem = VWorld.Server.GetExistingSystemManaged<DebugEventsSystem>();
        debugEventsSystem.SetDebugSetting(0, ref BuildCostsDisabledSetting);
    }

    public static void ModifyDropOnDeath()
    {
		var prefabEntities = Helper.GetPrefabEntitiesByComponentTypes<ItemData>();
        var map = Core.gameDataSystem.ItemHashLookupMap;
        foreach (var prefabEntity in prefabEntities)
		{
			if (prefabEntity.Has<Relic>()) continue;

			var prefabGuid = prefabEntity.GetPrefabGUID();
            var itemData = prefabEntity.Read<ItemData>();
			itemData.ItemCategory |= ItemCategory.BloodBound;
            map[prefabGuid] = itemData;
        }
        prefabEntities.Dispose();
    }

    public static void ModifyDropTables()
    {
        var entities = Helper.GetPrefabEntitiesByComponentTypes<DropTableDataBuffer>(); //this code makes giving items via admin console not work
        foreach (var entity in entities)
        {
            var prefabDropTableBuffer = entity.ReadBuffer<DropTableDataBuffer>();
            for (var i = 0; i < prefabDropTableBuffer.Length; i++)
            {
                var dropTableDataBuffer = prefabDropTableBuffer[i];
                dropTableDataBuffer.Quantity = 0;
                prefabDropTableBuffer[i] = dropTableDataBuffer;
            }
        }
        entities.Dispose();

        entities = Helper.GetPrefabEntitiesByComponentTypes<DropTableBuffer>(); //this code makes giving items via admin console not work
        foreach (var entity in entities)
        {
            var buffer = entity.ReadBuffer<DropTableBuffer>();
            for (var i = 0; i < buffer.Length; i++)
            {
                var dropTableBuffer = buffer[i];
                dropTableBuffer.DropTableGuid = PrefabGUID.Empty;
                buffer[i] = dropTableBuffer;
            }
        }
        entities.Dispose();
    }

    public static void ModifyGolemShield()
	{
        var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.AB_Shapeshift_Golem_T02_Buff);
		prefabEntity.Remove<SiegeWeaponAbsorbCapByServerSettings>();
        var absorbBuff = prefabEntity.Read<AbsorbBuff>();
        absorbBuff.AbsorbCap = ChipSaModConfig.Config.GolemShield;
        absorbBuff.AbsorbValue = ChipSaModConfig.Config.GolemShield;
        absorbBuff.AbsorbModifier = 1;
        prefabEntity.Write(absorbBuff);
    }

	public static void ModifyConsumables()
	{
		Dictionary<PrefabGUID, PrefabGUID> itemsToModify = new()
		{
			{ Prefabs.Item_Consumable_HealingPotion_T02, Prefabs.AB_Consumable_HealingPotion_T02_Activate},
			{ Prefabs.Item_Consumable_HealingPotion_T01, Prefabs.AB_Consumable_HealingPotion_T01_Activate}
        };

		foreach (var item in itemsToModify)
		{
            var entity = Helper.GetPrefabEntityByPrefabGUID(item.Key);
            var itemData = entity.Read<ItemData>();
            itemData.RemoveOnConsume = false;
            entity.Write(itemData);

            entity = Helper.GetPrefabEntityByPrefabGUID(item.Value);
            var buffer = entity.ReadBuffer<DropTableBuffer>();
            buffer.Clear();
        }
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

    
	private static void OnServerStart()
	{

		ChipSaModConfig.Initialize();
        CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        ChipSaManager.Initialize();
        ChipSaModDataStorage.Load();
		ModifyPrefabs();
		DisableBuildCosts();
        FixCastleAttackTimer();
	}
}
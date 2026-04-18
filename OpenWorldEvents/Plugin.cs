using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using OpenWorldEvents.Managers;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using System;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using ModCore;
using ModCore.Data;
using ModCore.Events;
using ModCore.Factories;
using ModCore.Helpers;
using ModCore.Services;
using static ModCore.Frameworks.CommandFramework.CommandFramework;

namespace OpenWorldEvents;


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
		Harmony.PatchAll(Assembly.GetExecutingAssembly());
    	GameEvents.OnServerStart += OnServerStart;
    }

	public override bool Unload()
	{
		Harmony?.UnpatchSelf();
        EventHelper.SetDeathDurabilityLoss(0.125f);
        EventScheduler.Dispose();
        CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

        DonkeyManager.Dispose();
        ChestManager.Dispose();
        BossManager.Dispose();
        InfiniteResourceNodeManager.Dispose();
        FlashDuelsManager.Dispose();
        BossContestManager.Dispose();
        ScavengerHuntChestManager.Dispose();
        AdminBossManager.Dispose();
        HungerGamesManager.Dispose();
        DominionManager.Dispose();

        OpenWorldEventsConfig.Dispose();
        HorseConfig.Dispose();
        ChestConfig.Dispose();
        BossConfig.Dispose();
        ResourceNodeConfig.Dispose();
        DuelsConfig.Dispose();
        BossContestConfig.Dispose();
        ScavengerHuntChestConfig.Dispose();
        AdminBossConfig.Dispose();
        CursedFogConfig.Dispose();
        HungerGamesConfig.Dispose();
        DominionConfig.Dispose();

        return true;
    }


	private static void OnServerStart()
	{

		OpenWorldEventsConfig.Initialize();
        DuelsConfig.Initialize();
        HorseConfig.Initialize();
        DonkeyManager.CleanUpBuffsOnServerStart();
        BossConfig.Initialize();
        ResourceNodeConfig.Initialize();
        ChestConfig.Initialize();
        ScavengerHuntChestConfig.Initialize();
        BossContestConfig.Initialize();
        ScavengerHuntChestManager.Initialize(true);
        AdminBossConfig.Initialize();
        AdminBossManager.RestorePlayersToDefault();
        CursedFogConfig.Initialize();
        HungerGamesConfig.Initialize();
        DominionConfig.Initialize();
        CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        SearchForUndisposedEntities();
        EventScheduler.Initialize();
    }

    //if the server crashes, we will lose track of the special event entities and their map icons will never get cleaned up
    //so we look for them whenever the server starts or the plugin reloads and remove them
    private static void SearchForUndisposedEntities()
    {
        var mapIconEntities = Helper.GetEntitiesByComponentTypes<MapIconData>();
        foreach (var mapIconEntity in mapIconEntities)
        {
            var prefabGuid = mapIconEntity.GetPrefabGUID();

            if (prefabGuid == Prefabs.MapIcon_DraculasCastle)
            {
                Helper.DestroyEntity(mapIconEntity);
            }
        }
        mapIconEntities.Dispose();

        var spawnedEntities = Helper.GetEntitiesByComponentTypes<CanFly, AggroConsumer>();
        foreach (var entity in spawnedEntities)
        {
            if (UnitFactory.HasCategoryOld(entity, "donkey"))
            {
                Helper.DestroyEntity(entity);
            }
            else if (UnitFactory.HasCategory(entity, "eventboss"))
            {
                Helper.DestroyEntity(entity);
            }
        }
        spawnedEntities.Dispose();

        spawnedEntities = Helper.GetEntitiesByComponentTypes<CanFly, CastleHeartConnection>();
        foreach (var entity in spawnedEntities)
        {
            if (entity.GetPrefabGUID() == Prefabs.TM_WorldChest_Epic_01_Full)
            {
                if (UnitFactory.HasCategory(entity, "eventchest"))
                {
                    Helper.DestroyEntity(entity);
                }
            }
        }

        DonkeyManager.CleanUpPreviousEntities();
    }
}
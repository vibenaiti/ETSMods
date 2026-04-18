using System.Globalization;
using System.Threading;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Debugging;
using ProjectM.Gameplay.Clan;
using ProjectM.Network;
using ModCore.Listeners;
using ModCore.Patches;
using ModCore.Services;
using Unity.Entities;
using static ModCore.PrefabSpawnerService;
using ModCore.Data;
using ProjectM.Tiles;
using System.Collections.Generic;
using ModCore.Helpers;
using ProjectM.Scripting;

namespace ModCore;

public static class Core
{
	public static bool HasInitialized = false;
	public static DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystemManaged<DebugEventsSystem>();
	public static JewelSpawnSystem jewelSpawnSystem = VWorld.Server.GetExistingSystemManaged<JewelSpawnSystem>();
	public static AdminAuthSystem adminAuthSystem = VWorld.Server.GetExistingSystemManaged<AdminAuthSystem>();
	public static PrefabCollectionSystem prefabCollectionSystem = VWorld.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
	public static ClanSystem_Server clanSystem = VWorld.Server.GetExistingSystemManaged<ClanSystem_Server>();
	public static EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystemManaged<EntityCommandBufferSystem>();
	public static TraderSyncSystem traderSyncSystem = VWorld.Server.GetExistingSystemManaged<TraderSyncSystem>();
	public static ServerBootstrapSystem serverBootstrapSystem = VWorld.Server.GetExistingSystemManaged<ServerBootstrapSystem>();
	public static GameDataSystem gameDataSystem = VWorld.Server.GetExistingSystemManaged<GameDataSystem>();
	public static ModificationSystem modificationSystem = VWorld.Server.GetExistingSystemManaged<ModificationSystem>();
	public static MapZoneCollectionSystem mapZoneCollectionSystem = VWorld.Server.GetExistingSystemManaged<MapZoneCollectionSystem>();
	public static GameplayEventDebuggingSystem gameplayEventDebuggingSystem = VWorld.Server.GetExistingSystemManaged<GameplayEventDebuggingSystem>();
	public static GameplayEventsSystem gameplayEventsSystem = VWorld.Server.GetExistingSystemManaged<GameplayEventsSystem>();
	public static ServerScriptMapper serverScriptMapper = VWorld.Server.GetExistingSystemManaged<ServerScriptMapper>();
	public static ShapeshiftSystem shapeshiftSystem = VWorld.Server.GetExistingSystemManaged<ShapeshiftSystem>();
	public static ServerGameManager serverGameManager => serverScriptMapper.GetServerGameManager();
	public static Entity networkIdLookupEntity;
	public static Entity tileModelSpatialLookupSystemEntity;
	public static Entity radialZoneSystemCurseEntity;
	public static Entity dayNightCycleEntity;
	public static ServerGameSettingsSystem serverGameSettingsSystem = VWorld.Server.GetExistingSystemManaged<ServerGameSettingsSystem>();
	
	public static unsafe void Initialize()
	{
		if (HasInitialized) return;
		// Re-acquire system references in case they were null at class load time
		debugEventsSystem = VWorld.Server.GetExistingSystemManaged<DebugEventsSystem>();
		jewelSpawnSystem = VWorld.Server.GetExistingSystemManaged<JewelSpawnSystem>();
		adminAuthSystem = VWorld.Server.GetExistingSystemManaged<AdminAuthSystem>();
		prefabCollectionSystem = VWorld.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
		clanSystem = VWorld.Server.GetExistingSystemManaged<ClanSystem_Server>();
		entityCommandBufferSystem = VWorld.Server.GetExistingSystemManaged<EntityCommandBufferSystem>();
		traderSyncSystem = VWorld.Server.GetExistingSystemManaged<TraderSyncSystem>();
		serverBootstrapSystem = VWorld.Server.GetExistingSystemManaged<ServerBootstrapSystem>();
		gameDataSystem = VWorld.Server.GetExistingSystemManaged<GameDataSystem>();
		modificationSystem = VWorld.Server.GetExistingSystemManaged<ModificationSystem>();
		mapZoneCollectionSystem = VWorld.Server.GetExistingSystemManaged<MapZoneCollectionSystem>();
		gameplayEventDebuggingSystem = VWorld.Server.GetExistingSystemManaged<GameplayEventDebuggingSystem>();
		gameplayEventsSystem = VWorld.Server.GetExistingSystemManaged<GameplayEventsSystem>();
		serverScriptMapper = VWorld.Server.GetExistingSystemManaged<ServerScriptMapper>();
		shapeshiftSystem = VWorld.Server.GetExistingSystemManaged<ShapeshiftSystem>();
		serverGameSettingsSystem = VWorld.Server.GetExistingSystemManaged<ServerGameSettingsSystem>();
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		
/*		DiscordBotConfig.Initialize();
		DiscordBot.InitializeAsync();*/
		PlayerService.Initialize();
		FileRoleStorage.Initialize();
		PlayerSpawnHandler.Initialize();
		StatRecorderService.Initialize();
		CastleTerritoryCache.Initialize();
		AbilityCastEndedEventListener.Initialize();
        Listeners.StatChangeListener.Initialize();
		RemovePvpProtectionListener.Initialize();

		networkIdLookupEntity = Helper.GetEntitiesByComponentTypes<NetworkIdSystem.Singleton>(EntityQueryOptions.IncludeSystems)[0];
		tileModelSpatialLookupSystemEntity = Helper.GetEntitiesByComponentTypes<TileModelSpatialLookupSystem.Singleton>(EntityQueryOptions.IncludeSystems)[0];
		//radialZoneSystemCurseEntity = VWorld.Server.GetExistingSystem((SystemTypeIndex)830).m_Entity;
		radialZoneSystemCurseEntity = Helper.FindSystemEntityByName("ProjectM.Gameplay.Scripting.RadialZoneSystem_Curse_Server");
		dayNightCycleEntity = Helper.GetEntitiesByComponentTypes<DayNightCycle>()[0];
		//TargetAoeListener.Initialize();

		HasInitialized = true;
	}

	public static void Dispose()
	{
		HasInitialized = false;
/*		DiscordBotConfig.Dispose();
		DiscordBot.Dispose();*/
		PlayerService.Dispose();
		FileRoleStorage.Dispose();
		PlayerSpawnHandler.Dispose();
		StatRecorderService.Dispose();
		ManuallySpawnedPrefabListener.Dispose();
		AbilityCastEndedEventListener.Dispose();
		Listeners.StatChangeListener.Dispose();
		RemovePvpProtectionListener.Dispose();
		//TargetAoeListener.Dispose();
	}
}


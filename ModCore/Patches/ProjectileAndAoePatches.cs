using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ModCore.Data;
using ProjectM.Gameplay.Systems;
using System;
using ModCore.Events;
using ModCore.Services;
using ProjectM.Shared;
using static ProjectM.HitColliderCast;
using ModCore.Listeners;
using Unity.Entities;
using Unity.Transforms;
using ProjectM.Gameplay.Scripting;
using ModCore.Helpers;
using ProjectM.Scripting;
using ModCore.Models;
using ProjectM.Network;
using ProjectM.CastleBuilding;
using Il2CppInterop.Runtime;

namespace ModCore.Patches;

/*[HarmonyPatch(typeof(ProjectileSystem_Spawn_Server), nameof(ProjectileSystem_Spawn_Server.OnUpdate))]
public static class ProjectileSystem_Spawn_ServerPatch
{
	public static void Prefix(ProjectileSystem_Spawn_Server __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var prefabGuid = entity.Read<PrefabGUID>();
				var owner = entity.Read<EntityOwner>().Owner;
				if (!owner.Exists())
				{
					owner = entity.Read<EntityCreator>().Creator._Entity;
					if (!owner.Exists())
					{
						owner = entity;
					}
				}
				if (owner.Exists())
				{
					if (owner.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(owner);
						GameEvents.RaisePlayerProjectileCreated(player, entity);
					}
					else
					{
						GameEvents.RaiseUnitProjectileCreated(owner, entity);
					}
				}
			}
			catch (Exception e)
			{
				entities.Dispose();
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}

[HarmonyPatch(typeof(HitCastColliderSystem_OnSpawn), nameof(HitCastColliderSystem_OnSpawn.OnUpdate))]
public static class HitCastColliderSystem_OnSpawnPatch
{
	public static void Prefix(HitCastColliderSystem_OnSpawn __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (entity.Exists())
			{
				var owner = entity.Read<EntityOwner>().Owner;
				if (!owner.Exists())
				{
					owner = entity.Read<EntityCreator>().Creator._Entity;
					if (!owner.Exists())
					{
						owner = entity;
					}
				}
				if (owner.Exists())
				{
					if (owner.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(owner);
						GameEvents.RaisePlayerHitColliderCastCreated(player, entity);
					}
					else
					{
						GameEvents.RaiseUnitHitColliderCastCreated(owner, entity);
					}
				}
			}
		}
		entities.Dispose();
	}
}

[HarmonyPatch(typeof(HitCastColliderSystem_OnUpdate), nameof(HitCastColliderSystem_OnUpdate.OnUpdate))]
public static class HitCastColliderSystem_OnUpdatePatch
{
	public static void Prefix(HitCastColliderSystem_OnUpdate __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				if (entity.Exists())
				{
					var owner = entity.Read<EntityOwner>().Owner;
					if (!owner.Exists())
					{
						owner = entity.Read<EntityCreator>().Creator._Entity;
						if (!owner.Exists())
						{
							owner = entity;
						}
					}
					if (owner.Exists())
					{
						if (owner.Has<PlayerCharacter>())
						{
							var player = PlayerService.GetPlayerFromCharacter(owner);
							GameEvents.RaisePlayerHitColliderCastUpdate(player, entity);
						}
						else
						{
							GameEvents.RaiseUnitHitColliderCastUpdate(owner, entity);
						}
					}
				}
			}
			catch (Exception e)
			{
				entities.Dispose();
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
		entities.Dispose();
	}
}

public static class TargetAoeListener
{
	private static EntityQueryOptions Options = EntityQueryOptions.Default;
	private static EntityQueryDesc QueryDesc;
	private static EntityQuery Query;
	private static bool Initialized = false;

	public static void Initialize()
	{
		if (Initialized) return;
		QueryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
					{
				new ComponentType(Il2CppType.Of<TargetAoE>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<HitColliderCast>(), ComponentType.AccessMode.ReadWrite)
					},
			Options = Options
		};
		Query = VWorld.Server.EntityManager.CreateEntityQuery(QueryDesc);

		GameEvents.OnGameFrameUpdate += OnUpdate;
		Initialized = true;
	}

	public static void Dispose()
	{
		GameEvents.OnGameFrameUpdate -= OnUpdate;
		Initialized = false;
	}

	private static void OnUpdate()
	{
		var entities = Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var owner = entity.Read<EntityOwner>().Owner;
			if (owner.Exists())
			{
				if (owner.Exists())
				{
					if (owner.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(owner);
						GameEvents.RaisePlayerAoeCreated(player, entity);
					}
					else
					{
						GameEvents.RaiseUnitAoeCreated(owner, entity);
					}
				}
			}
		}

		entities.Dispose();
	}
}*/

using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ModCore.Services;
using ModCore.Events;
using System;
using ModCore.Listeners;

namespace ModCore.Patches;


[HarmonyPatch(typeof(DeathEventListenerSystem), nameof(DeathEventListenerSystem.OnUpdate))]
public static class DeathEventListenerSystemPatch
{
	public static void Prefix(DeathEventListenerSystem __instance)
	{
		if (GameEvents.OnPlayerDeath == null && GameEvents.OnUnitDeath == null) return;

		var entities = __instance._DeathEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var deathEvent = entity.Read<DeathEvent>();
			try
			{
				if (deathEvent.Died.Has<PlayerCharacter>())
				{	
					var player = PlayerService.GetPlayerFromCharacter(deathEvent.Died);
					GameEvents.OnPlayerDeath?.Invoke(player, deathEvent);
				}
				else
				{
					Listeners.StatChangeListener.SummonToGrandparentPlayerCharacter.Remove(deathEvent.Died);
					GameEvents.OnUnitDeath?.Invoke(deathEvent.Died, deathEvent);
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}

[HarmonyPatch(typeof(VampireDownedServerEventSystem), nameof(VampireDownedServerEventSystem.OnUpdate))]
public static class VampireDownedPatch
{
	public static void Postfix(VampireDownedServerEventSystem __instance)
	{
		if (GameEvents.OnPlayerDowned == null) return;

		var downedEvents = __instance.__query_1174204813_0.ToEntityArray(Allocator.Temp);
		foreach (var entity in downedEvents)
		{
			try
			{
				if (!VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, VWorld.Server.EntityManager, out var victimEntity))
				{
					Plugin.PluginLog.LogMessage("Couldn't get victim entity");
					return;
				}

				var downBuff = entity.Read<VampireDownedBuff>();

				if (!VampireDownedServerEventSystem.TryFindRootOwner(downBuff.Source, 1, VWorld.Server.EntityManager, out var killerEntity))
				{
					Plugin.PluginLog.LogMessage("Couldn't get killer entity");
					return;
				}
				
				var victimPlayer = PlayerService.GetPlayerFromCharacter(victimEntity);
				GameEvents.OnPlayerDowned?.Invoke(victimPlayer, killerEntity);
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
		downedEvents.Dispose();
	}
}

[HarmonyPatch(typeof(KillEventSystem), nameof(KillEventSystem.OnUpdate))]
public static class KillEventSystemPatch
{
	public static void Prefix(KillEventSystem __instance)
	{
		if (GameEvents.OnPlayerUnstuck == null) return;

		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.OnPlayerUnstuck?.Invoke(player, entity);
		}
		entities.Dispose();
	}
}


[HarmonyPatch(typeof(LinkMinionToOwnerOnSpawnSystem), nameof(LinkMinionToOwnerOnSpawnSystem.OnUpdate))]
public static class LinkMinionToOwnerOnSpawnSystemPatch
{
	public static void Prefix(LinkMinionToOwnerOnSpawnSystem __instance)
	{
		if (GameEvents.OnPlayerHealthChanged == null && GameEvents.OnUnitHealthChanged == null) return;

		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var owner = entity.Read<EntityOwner>().Owner;
			if (owner.Exists())
			{
				if (owner.Has<EntityOwner>())
				{
					var grandParent = owner.Read<EntityOwner>().Owner;
					if (grandParent.Exists())
					{
						if (grandParent.Has<PlayerCharacter>())
						{
							Listeners.StatChangeListener.SummonToGrandparentPlayerCharacter[entity] = grandParent;
						}
					}
				}
			}
		}
		entities.Dispose();
	}
}

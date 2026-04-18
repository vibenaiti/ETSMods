using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ModCore.Services;
using ModCore.Events;
using System;

namespace ModCore.Patches;

[HarmonyPatch(typeof(AbilityCastStarted_SpawnPrefabSystem_Server), nameof(AbilityCastStarted_SpawnPrefabSystem_Server.OnUpdate))]
public static class AbilityCastStarted_SpawnPrefabSystem_ServerPatch
{

	public static void Prefix(AbilityCastStarted_SpawnPrefabSystem_Server __instance)
	{
		//__instance.__OnUpdate_LambdaJob0_entityQuery.LogComponentTypes();
		
		try
		{
			if (GameEvents.OnPlayerStartedCasting == null) return;
			var entities = __instance.__query_577032100_0.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{
				var abilityCastStartedEvent = entity.Read<AbilityCastStartedEvent>();

				if (abilityCastStartedEvent.Character.Exists() && abilityCastStartedEvent.Character.Has<PlayerCharacter>())
				{
					var player = PlayerService.GetPlayerFromCharacter(abilityCastStartedEvent.Character);
					GameEvents.OnPlayerStartedCasting?.Invoke(player, entity);
				}
			}
			entities.Dispose();
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo(e.ToString());
		}
	}
}

using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ProjectM.Gameplay.Systems;
using System;
using ModCore.Events;
using ModCore.Services;
using ProjectM.Network;

namespace ModCore.Patches;


/*[HarmonyPatch(typeof(InteractSystemServer), nameof(InteractSystemServer.OnUpdate))]
public static class InteractSystemServerPatch
{
	public static void Prefix(InteractSystemServer __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var interactor = entity.Read<Interactor>();
				if (entity.Has<PlayerCharacter>())
				{
					var playerCharacter = entity.Read<PlayerCharacter>();
					if (playerCharacter.UserEntity.Exists())
					{
						var player = PlayerService.GetPlayerFromCharacter(entity);
						GameEvents.RaisePlayerInteracted(player, interactor);
					}
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}*/

/*[HarmonyPatch(typeof(InteractValidateAndStopSystemServer), nameof(InteractValidateAndStopSystemServer.OnUpdate))]
public static class InteractValidateAndStopSystemServerPatch
{
	public static void Prefix(InteractValidateAndStopSystemServer __instance)
	{
		if (GameEvents.OnPlayerStoppedInteractingWithObject == null) return;
		var entities = __instance._StopInteractQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var stopInteractingWithObjectEvent = entity.Read<StopInteractingWithObjectEvent>();
				var player = PlayerService.GetPlayerFromUser(entity.Read<FromCharacter>().User);
				GameEvents.OnPlayerStoppedInteractingWithObject(player, entity, stopInteractingWithObjectEvent);
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}
*/


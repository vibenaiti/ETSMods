using HarmonyLib;
using ProjectM;
using ModCore.Models;
using ModCore.Services;
using ProjectM.Terrain;
using Unity.Collections;
using ModCore.Events;

namespace ModCore.Patches;

[HarmonyPatch(typeof(UpdateUserWorldRegionSystem), nameof(UpdateUserWorldRegionSystem.OnUpdate))]
public static class UpdateUserWorldRegionSystemPatch
{
	public static void Prefix(UpdateUserWorldRegionSystem __instance)
	{
		if (GameEvents.OnPlayerEnteredRegion == null) return;
		
		var entities = __instance._CurrentWorldRegionChangedEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var currentWorldRegionChangedEvent = entity.Read<CurrentWorldRegionChangedEvent>();
			var player = PlayerService.GetPlayerFromUser(currentWorldRegionChangedEvent.User);
			GameEvents.OnPlayerEnteredRegion?.Invoke(player, entity, currentWorldRegionChangedEvent);
		}
		entities.Dispose();
	}
}

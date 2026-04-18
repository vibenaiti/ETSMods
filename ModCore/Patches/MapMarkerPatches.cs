using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ModCore.Services;
using ModCore.Events;
using ProjectM.Network;

namespace ModCore.Patches;


[HarmonyPatch(typeof(SetMapMarkerSystem), nameof(SetMapMarkerSystem.OnUpdate))]
public static class SetMapMarkerSystemPatch
{
	static void Prefix(SetMapMarkerSystem __instance)
	{
		if (GameEvents.OnPlayerSetMapMarker == null) return;

		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var setMapMarkerEvent = entity.Read<SetMapMarkerEvent>();
			var player = PlayerService.GetPlayerFromUser(entity.Read<FromCharacter>().User);
			GameEvents.OnPlayerSetMapMarker?.Invoke(player, entity, setMapMarkerEvent);
		}
		entities.Dispose();
	}
}

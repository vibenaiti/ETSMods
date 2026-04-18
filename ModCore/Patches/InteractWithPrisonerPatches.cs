using HarmonyLib;
using Unity.Collections;
using ProjectM.Network;
using ProjectM.Gameplay.Clan;
using ModCore.Services;
using ModCore.Events;
using System;
using ProjectM;
using static ProjectM.Network.ClanEvents_Client;
using ModCore.Data;
using ModCore.Helpers;

namespace ModCore.Patches;


[HarmonyPatch(typeof(InteractWithPrisonerSystem), nameof(InteractWithPrisonerSystem.OnUpdate))]
public static class InteractWithPrisonerSystemPatch
{
	public static void Prefix(InteractWithPrisonerSystem __instance)
	{
		if (GameEvents.OnPlayerInteractedWithPrisoner == null) return;

		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var interactWithPrisonerEvent = entity.Read<InteractWithPrisonerEvent>();
			var player = PlayerService.GetPlayerFromUser(entity.Read<FromCharacter>().User);
			GameEvents.OnPlayerInteractedWithPrisoner?.Invoke(player, entity, interactWithPrisonerEvent);
		}
		entities.Dispose();
	}
}

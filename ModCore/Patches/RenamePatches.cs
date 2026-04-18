using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Mathematics;
using ModCore.Services;
using ModCore.Events;
using ModCore.Models;
using System;
using ModCore.Data;
using ModCore.Listeners;
using Unity.Entities;
using ProjectM.Network;
using UnityEngine.Jobs;
using ModCore.Helpers;
using static ProjectM.Network.InteractEvents_Client;

namespace ModCore.Patches;


[HarmonyPatch(typeof(NameableInteractableSystem), nameof(NameableInteractableSystem.OnUpdate))]
public static class NameableInteractableSystemPatch
{
	static void Prefix(NameableInteractableSystem __instance)
	{
		if (GameEvents.OnPlayerRenamedEntity == null) return;

		var entities = __instance._RenameQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var renameInteractable = entity.Read<RenameInteractable>();
			var player = PlayerService.GetPlayerFromUser(entity.Read<FromCharacter>().User);
			GameEvents.OnPlayerRenamedEntity?.Invoke(player, entity, renameInteractable);
		}
		entities.Dispose();
	}
}


using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ProjectM.Network;
using ModCore.Services;
using Unity.Entities;
using static ProjectM.AbilityCastStarted_SpawnPrefabSystem_Server;
using ModCore.Data;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using ModCore.Events;
using System;

namespace ModCore.Patches;

[HarmonyPatch(typeof(UseConsumableSystem), nameof(UseConsumableSystem.OnUpdate))]
public static class UseConsumableSystemPatch
{

	public static void Prefix(UseConsumableSystem __instance)
	{
		if (GameEvents.OnPlayerUsedConsumable == null) return;

		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var useItemEvent = entity.Read<UseItemEvent>();
				if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, fromCharacter.Character, useItemEvent.SlotIndex, out var item))
				{
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);

					GameEvents.OnPlayerUsedConsumable?.Invoke(player, entity, item);
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
			
		entities.Dispose();
	}
}

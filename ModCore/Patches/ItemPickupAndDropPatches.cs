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

namespace ModCore.Patches;

/*[HarmonyPatch(typeof(ItemPickupSystem), nameof(ItemPickupSystem.OnUpdate))]
public static class ItemPickupSystemPatch
{
	public static void Prefix(ItemPickupSystem __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			
		}
	}
}*/

[HarmonyPatch(typeof(DropInventoryItemSystem), nameof(DropInventoryItemSystem.OnUpdate))]
public static class DropInventoryItemSystemPatch
{
	public static void Prefix(DropInventoryItemSystem __instance)
	{
		if (GameEvents.OnItemWasDropped == null) return;
		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var dropItemEvent = entity.Read<DropInventoryItemEvent>();
			if (Helper.TryGetEntityFromNetworkId(dropItemEvent.Inventory, out var inventoryEntity))
			{
				var fromCharacter = entity.Read<FromCharacter>();
				if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, inventoryEntity, dropItemEvent.SlotIndex, out var inventoryBuffer))
				{
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					GameEvents.OnItemWasDropped?.Invoke(player, entity, inventoryBuffer.ItemType, dropItemEvent.SlotIndex);
				}
			}
		}
		entities.Dispose();
	}
}

[HarmonyPatch(typeof(DropItemSystem), nameof(DropItemSystem.OnUpdate))]
public static class DropItemSystemPatch
{
	public static void Prefix(DropItemSystem __instance)
	{
		if (GameEvents.OnItemWasDropped == null) return;
		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var dropItemEvent = entity.Read<DropItemAtSlotEvent>();
			var fromCharacter = entity.Read<FromCharacter>();
			if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, fromCharacter.Character, dropItemEvent.SlotIndex, out var inventoryBuffer))
			{
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				GameEvents.OnItemWasDropped?.Invoke(player, entity, inventoryBuffer.ItemType, dropItemEvent.SlotIndex);
			}
		}
		entities.Dispose();
	}
}


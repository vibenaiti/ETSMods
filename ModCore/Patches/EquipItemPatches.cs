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
using static ModCore.Events.GameEvents;

namespace ModCore.Patches;

[HarmonyPatch(typeof(EquipItemSystem), nameof(EquipItemSystem.OnUpdate))]
public static class EquipItemSystemPatch
{
	public static void Prefix(EquipItemSystem __instance)
	{
		if (GameEvents.OnPlayerEquippedItem == null) return;

		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var equipItemEvent = entity.Read<EquipItemEvent>();
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.OnPlayerEquippedItem(player, entity, equipItemEvent);
		}
		entities.Dispose();
	}
	//
}

[HarmonyPatch(typeof(EquipItemFromInventorySystem), nameof(EquipItemFromInventorySystem.OnUpdate))]
public static class EquipItemFromInventorySystemPatch
{
	public static void Prefix(EquipItemFromInventorySystem __instance)
	{
		if (GameEvents.OnPlayerEquippedItemFromInventory == null) return;

		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var equipItemFromInventoryEvent = entity.Read<EquipItemFromInventoryEvent>();
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.OnPlayerEquippedItemFromInventory(player, entity, equipItemFromInventoryEvent);
		}
		entities.Dispose();
	}
	
}

[HarmonyPatch(typeof(UnEquipItemSystem), nameof(UnEquipItemSystem.OnUpdate))]
public static class UnEquipItemSystemPatch
{
	public static void Prefix(UnEquipItemSystem __instance)
	{
		if (GameEvents.OnPlayerUnequippedItem == null) return;

		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var unequipItemEvent = entity.Read<UnequipItemEvent>();
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.OnPlayerUnequippedItem(player, entity, unequipItemEvent);
		}
		entities.Dispose();
	}
}

[HarmonyPatch(typeof(EquipmentTransferSystem), nameof(EquipmentTransferSystem.OnUpdate))]
public static class EquipmentTransferSystemPatch
{
	public static void Prefix(EquipmentTransferSystem __instance)
	{
		if (GameEvents.OnPlayerTransferredEquipmentToEquipment == null) return;

		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var equipmentToEquipmentTransferEvent = entity.Read<EquipmentToEquipmentTransferEvent>();
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.OnPlayerTransferredEquipmentToEquipment(player, entity, equipmentToEquipmentTransferEvent);
		}
		entities.Dispose();
	}
}

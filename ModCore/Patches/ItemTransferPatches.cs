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

[HarmonyPatch(typeof(MoveItemBetweenInventoriesSystem), nameof(MoveItemBetweenInventoriesSystem.OnUpdate))]
public static class MoveItemBetweenInventoriesSystemPatch
{
	public static void Prefix(MoveItemBetweenInventoriesSystem __instance)
	{
		if (GameEvents.OnPlayerTransferredItem == null) return;

		var entities = __instance._MoveItemBetweenInventoriesEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var moveItemBetweenInventoriesEvent = entity.Read<MoveItemBetweenInventoriesEvent>();
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.OnPlayerTransferredItem?.Invoke(player, entity, moveItemBetweenInventoriesEvent);
		}
		entities.Dispose();
	}
}

[HarmonyPatch(typeof(MoveAllItemsBetweenInventoriesSystem), nameof(MoveAllItemsBetweenInventoriesSystem.OnUpdate))]
public static class MoveAllItemsBetweenInventoriesSystemPatch
{
	public static void Prefix(MoveAllItemsBetweenInventoriesSystem __instance)
	{
		if (GameEvents.OnPlayerTransferredAllItems == null) return;

		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var moveAllItemsBetweenInventoriesEvent = entity.Read<MoveAllItemsBetweenInventoriesEvent>();
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.OnPlayerTransferredAllItems?.Invoke(player, entity, moveAllItemsBetweenInventoriesEvent);
		}
		entities.Dispose();
	}
}

[HarmonyPatch(typeof(MoveAllItemsBetweenInventoriesV2System), nameof(MoveAllItemsBetweenInventoriesV2System.OnUpdate))]
public static class MoveAllItemsBetweenInventoriesV2SystemPatch
{
	public static void Prefix(MoveAllItemsBetweenInventoriesV2System __instance)
	{
		if (GameEvents.OnPlayerTransferredAllItemsV2 == null) return;

		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var moveAllItemsBetweenInventoriesEvent = entity.Read<MoveAllItemsBetweenInventoriesEventV2>();
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.OnPlayerTransferredAllItemsV2?.Invoke(player, entity, moveAllItemsBetweenInventoriesEvent);
		}
		entities.Dispose();
	}
}

[HarmonyPatch(typeof(SmartMergeItemsBetweenInventoriesSystem), nameof(SmartMergeItemsBetweenInventoriesSystem.OnUpdate))]
public static class SmartMergeItemsBetweenInventoriesSystemPatch
{
	public static void Prefix(SmartMergeItemsBetweenInventoriesSystem __instance)
	{
		if (GameEvents.OnPlayerSmartMergedItems == null) return;

		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var smartMergeItemsBetweenInventoriesEvent = entity.Read<SmartMergeItemsBetweenInventoriesEvent>();
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.OnPlayerSmartMergedItems?.Invoke(player, entity, smartMergeItemsBetweenInventoriesEvent);
		}
		entities.Dispose();
	}
}


[HarmonyPatch(typeof(ReactToInventoryChangedSystem), nameof(ReactToInventoryChangedSystem.OnUpdate))]
public static class ReactToInventoryChangedSystemPatch
{
	public static void Prefix(ReactToInventoryChangedSystem __instance)
	{
		if (GameEvents.OnPlayerInventoryChanged == null) return;

		try
		{
			var entities = __instance.__query_2096870026_0.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{
				var inventoryChangedEvent = entity.Read<InventoryChangedEvent>();
				if (inventoryChangedEvent.InventoryEntity.Has<InventoryConnection>())
				{
					var owner = inventoryChangedEvent.InventoryEntity.Read<InventoryConnection>().InventoryOwner;
					if (owner.Exists() && owner.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(owner);
						GameEvents.OnPlayerInventoryChanged?.Invoke(player, entity, inventoryChangedEvent);
					}
				}
			}
			entities.Dispose();
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo(e);
		}
	}
}




/*[HarmonyPatch(typeof(SmartMergeItemsBetweenInventoriesSystem), nameof(SmartMergeItemsBetweenInventoriesSystem.OnUpdate))]
public static class SmartMergeItemsBetweenInventoriesSystemPatch
{
	public static void Prefix(SmartMergeItemsBetweenInventoriesSystem __instance)
	{
		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			//raise event?
		}
		entities.Dispose();
	}
}*/

/*[HarmonyPatch(typeof(UnEquipItemSystem), nameof(UnEquipItemSystem.OnUpdate))]
public static class UnEquipItemSystemPatch
{
	public static void Prefix(UnEquipItemSystem __instance)
	{
		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var unequipItemEvent = entity.Read<UnequipItemEvent>();
			if (Helper.TryGetEntityFromNetworkId(unequipItemEvent.ToInventory, out var targetInventory))
			{
				var targetPrefabGUID = targetInventory.Read<PrefabGUID>();
				var fromCharacter = entity.Read<FromCharacter>();
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				//raise event?
			}
		}
		entities.Dispose();
	}
}*/

/*[HarmonyPatch(typeof(UnEquipBagIntoInventorySystem), nameof(UnEquipBagIntoInventorySystem.OnUpdate))]
public static class UnEquipBagIntoInventorySystemPatch
{
	public static void Prefix(UnEquipBagIntoInventorySystem __instance)
	{
		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var unequipItemEvent = entity.Read<UnequipBagIntoInventoryEvent>();
			if (Helper.TryGetEntityFromNetworkId(unequipItemEvent.ToInventory, out var targetInventory))
			{
				var targetPrefabGUID = targetInventory.Read<PrefabGUID>();
				var fromCharacter = entity.Read<FromCharacter>();
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				//raise event?
			}
		}
		entities.Dispose();
	}
}
*/

using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectM.Gameplay.Clan;
using static ProjectM.Network.ClanEvents_Client;
using ModCore.Services;
using ModCore.Models;
using ModCore.Data;
using ModCore.Configs;
using Il2CppSystem;
using Unity.Jobs;
using UnityEngine.Jobs;
using Il2CppSystem.Runtime.Remoting.Messaging;
using Stunlock.Core;
using Unity.Entities.UniversalDelegates;

namespace ModCore.Helpers;

public static partial class Helper
{
	public static List<EquipmentType> EquipmentTypes = new List<EquipmentType> 
	{
		EquipmentType.Headgear,
		EquipmentType.Chest,
		EquipmentType.Weapon,
		EquipmentType.MagicSource,
		EquipmentType.Footgear,
		EquipmentType.Legs,
		EquipmentType.Cloak,
		EquipmentType.Gloves,
	};

	//this is horribly inefficient, don't use this outside of one-off scripts
	public static bool TryFindOwnerOfItem(Entity itemEntity, out Player itemOwner, out int slot)
	{
		var allPlayers = PlayerService.CharacterCache.Values;
		foreach (var player in allPlayers)
		{
			for (var i = 0; i < 36; i++)
			{
				if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, player.Character, i, out InventoryBuffer item))
				{
					if (item.ItemEntity._Entity == itemEntity)
					{
						itemOwner = player;
						slot = i;
						return true;
					}
				}
			}
		}
		itemOwner = default;
		slot = -1;
		return false;
	}

	public static void ClearPlayerInventory(Player player, bool all = false)
	{
		int start = 8;
		if (all)
		{
			start = 0;
		}

		for (int i = start; i < player.Inventory.ReadBuffer<InventoryBuffer>().Length; i++)
		{
			InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, player.Character, i);
		}
	}

	public static void ClearEntityInventory(Entity entity, bool all = false)
	{
		var buffer = entity.ReadBuffer<InventoryInstanceElement>();
		foreach (var item in buffer)
		{
			if (item.ExternalInventoryEntity._Entity.Exists())
			{
				InventoryUtilitiesServer.ClearInventory(VWorld.Server.EntityManager, item.ExternalInventoryEntity._Entity);
			}
		}
	}

	public static void RepairGear(Entity Character, bool repair = true)
	{
		Equipment equipment = Character.Read<Equipment>();
		NativeList<Entity> equippedItems = new NativeList<Entity>(Allocator.Temp);
		equipment.GetAllEquipmentEntities(equippedItems);
		foreach (var equippedItem in equippedItems)
		{
			if (equippedItem.Has<Durability>())
			{
				var durability = equippedItem.Read<Durability>();
				if (repair)
				{
					durability.Value = durability.MaxDurability;
				}
				else
				{
					durability.Value = 0;
				}

				equippedItem.Write(durability);
			}
		}

		equippedItems.Dispose();

		for (int i = 0; i < 36; i++)
		{
			if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, Character, i,
					out InventoryBuffer item))
			{
				var itemEntity = item.ItemEntity._Entity;
				if (itemEntity.Has<Durability>())
				{
					var durability = itemEntity.Read<Durability>();
					if (repair)
					{
						durability.Value = durability.MaxDurability;
					}
					else
					{
						durability.Value = 0;
					}

					itemEntity.Write(durability);
				}
			}
		}
	}

	public static void ClearInventorySlot(Player player, int itemSlot)
	{
		ClearInventorySlot(player.Character, itemSlot);
	}

	public static void ClearInventorySlot(Entity inventoryEntity, int itemSlot)
	{
		InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, inventoryEntity, itemSlot);
	}

	public static void RemoveItemAtSlotFromInventory(Player player, PrefabGUID itemPrefab, int itemSlot)
	{
		RemoveItemAtSlotFromInventory(player.Character, itemPrefab, itemSlot);
	}

	public static void RemoveItemAtSlotFromInventory(Entity inventoryEntity, PrefabGUID itemPrefab, int itemSlot)
	{
		if (Helper.GetPrefabEntityByPrefabGUID(itemPrefab).Has<Relic>())
		{
			if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, inventoryEntity, itemSlot, out InventoryBuffer item))
			{
				if (item.ItemEntity._Entity.Exists())
				{
					Helper.DestroyEntity(item.ItemEntity._Entity);
				}
			}
		}
		ClearInventorySlot(inventoryEntity, itemSlot);
	}

	public static void RemoveItemAtSlot(Player player, PrefabGUID itemPrefab, int slot)
	{
		if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, player.Character, slot, out InventoryBuffer item))
		{
			if (item.ItemEntity._Entity.Exists())
			{
				Helper.DestroyEntity(item.ItemEntity._Entity);
			}
		}
		InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, player.Character, slot);
	}

	//doesn't destroy the entity, just removes it from the inventory buffer
	public static void RemoveEquippedItemFromInventory(Entity itemEntity)
	{
		if (itemEntity.Has<InventoryItem>())
		{
			var inventoryOwner = itemEntity.Read<InventoryItem>().ContainerEntity;
			Entity inventoryEntity;
			if (!inventoryOwner.Has<InventoryBuffer>() && inventoryOwner.Has<InventoryInstanceElement>())
			{
				var buffer = inventoryOwner.ReadBuffer<InventoryInstanceElement>();
				if (buffer.Length > 0)
				{
					inventoryEntity = buffer[0].ExternalInventoryEntity._Entity;
				}
				else
				{
					return;
				}
			}
			else
			{
				inventoryEntity = inventoryOwner;
			}
			if (inventoryEntity.Exists() && inventoryEntity.Has<InventoryBuffer>())
			{
				var buffer = inventoryEntity.ReadBuffer<InventoryBuffer>();
				for (var i = 0; i < buffer.Length; i++)
				{
					var item = buffer[i];
					if (item.ItemEntity._Entity == itemEntity)
					{
						buffer[i] = new InventoryBuffer();
						break;
					}
				}
			}
		}
	}

	public static void RemoveEquippedItemFromInventory(Entity itemEntity, int slot)
	{
		if (itemEntity.Has<InventoryItem>())
		{
			var inventoryOwner = itemEntity.Read<InventoryItem>().ContainerEntity;
			Entity inventoryEntity;
			if (!inventoryOwner.Has<InventoryBuffer>() && inventoryOwner.Has<InventoryInstanceElement>())
			{
				var buffer = inventoryOwner.ReadBuffer<InventoryInstanceElement>();
				if (buffer.Length > 0)
				{
					inventoryEntity = buffer[0].ExternalInventoryEntity._Entity;
				}
				else
				{
					return;
				}
			}
			else
			{
				inventoryEntity = inventoryOwner;
			}
			if (inventoryEntity.Exists() && inventoryEntity.Has<InventoryBuffer>())
			{
				var buffer = inventoryEntity.ReadBuffer<InventoryBuffer>();
				buffer[slot] = new InventoryBuffer();
			}
		}
	}

	public static void RemoveItemFromInventory(Entity inventoryEntity, PrefabGUID item, int amount)
	{
		InventoryUtilitiesServer.RemoveItemGetRemainder(VWorld.Server.EntityManager, inventoryEntity, item, amount, out var remainder);
	}

	public static void CompletelyRemoveItemFromInventory(Player player, PrefabGUID itemPrefab)
	{
		while (Helper.GetPrefabEntityByPrefabGUID(itemPrefab).Has<Relic>() && InventoryUtilities.TryGetItemSlot(VWorld.Server.EntityManager, player.Character, itemPrefab, out var slot))
		{
			RemoveItemAtSlotFromInventory(player, itemPrefab, slot);
		}
		InventoryUtilitiesServer.TryRemoveItemFromInventories(VWorld.Server.EntityManager, player.Character, itemPrefab, 100000);
	}

	public static void RemoveItemFromInventory(Player player, PrefabGUID itemPrefab, int quantity = 1)
	{
		InventoryUtilitiesServer.TryRemoveItemFromInventories(VWorld.Server.EntityManager, player.Character, itemPrefab, quantity);
	}

	public static bool TryGetItemFromInventory(Player player, PrefabGUID prefabGUID, out Entity itemEntity)
	{
		return Core.serverGameManager.TryGetInventoryItem(player.Character, prefabGUID, out itemEntity);
	}

	public static bool TryGetItemFromInventory(Entity inventoryEntity, PrefabGUID prefabGUID, out Entity itemEntity)
	{
		return Core.serverGameManager.TryGetInventoryItem(inventoryEntity, prefabGUID, out itemEntity);
	}

	public static bool PlayerHasItemInInventories(Player player, PrefabGUID itemPrefab)
	{
		NativeList<Entity> inventories = new NativeList<Entity>(Allocator.Temp);
		InventoryUtilities.TryGetInventoryEntities(VWorld.Server.EntityManager, player.Character, ref inventories);
		return InventoryUtilities.HasItemInInventories(VWorld.Server.EntityManager, inventories, itemPrefab, 1);
	}

	public static bool PlayerHasEnoughItemsInInventory(Player player, PrefabGUID itemPrefab, int quantity)
	{
		var quantityInInventory = InventoryUtilities.GetItemAmountInInventories(VWorld.Server.EntityManager, player.Character, itemPrefab);
		if (quantityInInventory >= quantity)
		{
			return true;
		}
		return false;
	}

	//this won't get picked up by our current OnDrop listeners -- modify this to use DropInventoryItemEvent in the future
	public static void DropItemFromInventory(Player player, PrefabGUID item)
	{
		var entity = Helper.CreateEntityWithComponents<DropItemAroundPosition, FromCharacter>();
		var slot = InventoryUtilities.GetItemSlot(VWorld.Server.EntityManager, player.Character, item);
		if (slot > -1)
		{
			InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, player.Character, slot, out var invBuffer);

			entity.Write(player.ToFromCharacter());
			entity.Write(new DropItemAroundPosition
			{
				ItemEntity = invBuffer.ItemEntity._Entity,
				ItemHash = invBuffer.ItemType,
				Amount = invBuffer.Amount,
				Position = player.Position
			});
			var action = () =>
			{
				Helper.ClearInventorySlot(player, slot); //delay this slightly so that the event has time to process
			};
			ActionScheduler.RunActionOnceAfterDelay(action, .1);	
		}
	}

/*	public static bool AddItemToInventoryWorkaround(Entity inventory, PrefabGUID guid, int amount, out Entity entity, bool equip = true, int slot = -1)
	{
		entity = Entity.Null;

		Entity originalInventory = inventory;
		// Ensure the inventory references the correct entity in case it's a player character.
		if (inventory.Has<InventoryInstanceElement>())
		{
			inventory = inventory.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
		}

		if (!inventory.Has<InventoryBuffer>()) return false;

		// Retrieve the prefab entity based on the GUID and read its item data.
		var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(guid);
		var itemData = prefabEntity.Read<ItemData>();
		var buffer = inventory.ReadBuffer<InventoryBuffer>();

		bool result;
		int amountRemaining;
		if (itemData.MaxAmount <= 1 && amount > 1)
		{
			result = AddUniqueItems(buffer, inventory, prefabEntity, itemData, guid, amount, out entity, out amountRemaining);
		}
		else
		{
			// For non-unique items or single instance additions
			result = AddNonUniqueItems(buffer, inventory, prefabEntity, itemData, guid, amount, out entity, slot, out amountRemaining);
		}

		if (amountRemaining > 0 && originalInventory.Has<Translation>())
		{
			while (amountRemaining > 0)
			{
				var amountToDrop = System.Math.Min(amountRemaining, itemData.MaxAmount);
				CreateDroppedItem(guid, amountToDrop, originalInventory.Read<Translation>().Value);
				amountRemaining -= amountToDrop;
			}
			return true;
		}
		return result;
	}*/

	private static bool AddUniqueItems(DynamicBuffer<InventoryBuffer> buffer, Entity inventory, Entity prefabEntity, ItemData itemData, PrefabGUID guid, int amount, out Entity entity, out int amountRemaining)
	{
		entity = Entity.Null;
		// Iterate over each slot until all items are added or no space left
		for (var i = 0; i < buffer.Length; i++)
		{
			if (buffer[i].ItemType != PrefabGUID.Empty)
				continue;

			// Instantiate and configure the new item entity.
			entity = VWorld.Server.EntityManager.Instantiate(prefabEntity);
			InventoryUtilitiesServer.SetInventoryContainer(VWorld.Server.EntityManager, entity, inventory);
			var inventoryBufferData = new InventoryBuffer
			{
				ItemEntity = NetworkedEntity.ServerEntity(entity),
				Amount = 1,
				ItemType = guid,
			};

			buffer[i] = inventoryBufferData;
			InventoryUtilitiesServer.CreateInventoryChangedEvent(VWorld.Server.EntityManager, inventory, guid, 1, entity, InventoryChangedEventType.Obtained);
			if (--amount <= 0)
			{
				amountRemaining = amount;
				return true;
			}
		}
		amountRemaining = amount;
		return false;
	}

	private static bool AddNonUniqueItems(DynamicBuffer<InventoryBuffer> buffer, Entity inventory, Entity prefabEntity, ItemData itemData, PrefabGUID guid, int amount, out Entity entity, int specifiedSlot, out int amountRemaining)
	{
		if (specifiedSlot == -1)
		{
			// Check all slots if no specific slot is specified.
			return CheckAndFillSlots(buffer, inventory, prefabEntity, itemData, guid, amount, out entity, out amountRemaining);
		}
		else
		{
			// Attempt to add to the specified slot, and if unable, fill other slots.
			if (buffer[specifiedSlot].ItemType == PrefabGUID.Empty)
			{
				entity = VWorld.Server.EntityManager.Instantiate(prefabEntity);
				InventoryUtilitiesServer.SetInventoryContainer(VWorld.Server.EntityManager, entity, inventory);
				var inventoryBufferData = new InventoryBuffer
				{
					ItemEntity = NetworkedEntity.ServerEntity(entity),
					Amount = System.Math.Min(amount, itemData.MaxAmount), // Respect max amount per slot
					ItemType = guid,
				};

				buffer[specifiedSlot] = inventoryBufferData;
				InventoryUtilitiesServer.CreateInventoryChangedEvent(VWorld.Server.EntityManager, inventory, guid, inventoryBufferData.Amount, entity, InventoryChangedEventType.Obtained);
				amount -= inventoryBufferData.Amount;
				if (amount <= 0)
				{
					amountRemaining = amount;
					return true;
				}
				
			}
			return CheckAndFillSlots(buffer, inventory, prefabEntity, itemData, guid, amount, out entity, out amountRemaining);
		}
	}

	private static bool CheckAndFillSlots(DynamicBuffer<InventoryBuffer> buffer, Entity inventory, Entity prefabEntity, ItemData itemData, PrefabGUID guid, int amount, out Entity entity, out int amountRemaining)
	{
		entity = Entity.Null;
		for (var i = 0; i < buffer.Length && amount > 0; i++)
		{
			if (buffer[i].ItemType != PrefabGUID.Empty)
				continue;
			entity = VWorld.Server.EntityManager.Instantiate(prefabEntity);
			InventoryUtilitiesServer.SetInventoryContainer(VWorld.Server.EntityManager, entity, inventory);
			var inventoryBufferData = new InventoryBuffer
			{
				ItemEntity = NetworkedEntity.ServerEntity(entity),
				Amount = System.Math.Min(amount, itemData.MaxAmount), // Respect max amount per slot
				ItemType = guid,
			};

			buffer[i] = inventoryBufferData;
			InventoryUtilitiesServer.CreateInventoryChangedEvent(VWorld.Server.EntityManager, inventory, guid, inventoryBufferData.Amount, entity, InventoryChangedEventType.Obtained);
			amount -= inventoryBufferData.Amount;
		}

		amountRemaining = amount;
		return amount <= 0;
	}

/*	public static bool CreateDroppedItem(PrefabGUID prefabGUID, int amount, float3 pos)
	{
		var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(prefabGUID);
		var entity = VWorld.Server.EntityManager.Instantiate(prefabEntity);
		var itemData = prefabEntity.Read<ItemData>();

		var dropItemPrefabEntity = Helper.GetPrefabEntityByPrefabGUID(itemData.DropItemPrefab);
		var dropItemEntity = VWorld.Server.EntityManager.Instantiate(dropItemPrefabEntity);
		dropItemEntity.Write(new ItemPickup
		{
			ItemAmount = amount,
			ItemId = prefabGUID
		});

		var buffer = dropItemEntity.ReadBuffer<InventoryBuffer>();
		
		buffer.Add(new InventoryBuffer
		{
			ItemEntity = entity,
			Amount = amount,
			ItemType = prefabGUID,
		});

		dropItemEntity.Write(new Translation
		{
			Value = pos
		});
		return true;
	}*/

	public static void CreateDroppedItem(Player player, PrefabGUID prefabGUID, int amount)
	{
		CreateDroppedItem(player.Position, prefabGUID, amount);
	}

	public static void CreateDroppedItem(float3 pos, PrefabGUID prefabGUID, int amount)
	{
		if (Core.gameDataSystem.ItemHashLookupMap.TryGetValue(prefabGUID, out var itemData))
		{
			while (amount > 0)
			{
				try
				{
					var amountToDrop = System.Math.Min(amount, itemData.MaxAmount);
					Core.serverScriptMapper._ServerGameManager.CreateDroppedItemEntity(pos, prefabGUID, amountToDrop);
					amount -= amountToDrop;
				}
				catch (System.Exception e)
				{
					Plugin.PluginLog.LogInfo($"Could not create dropped item: {pos} {prefabGUID.LookupName()} {amount}");
					var action = () => CreateDroppedItem(pos, prefabGUID, amount);
					ActionScheduler.RunActionOnceAfterFrames(action, 60);
					return;
				}
			}
		}
	}

	public static bool AddItemToInventory(Entity recipient, string needle, int amount, out Entity entity, bool equip = true)
	{
		if (TryGetItemPrefabDataFromString(needle, out PrefabData prefab))
		{
			return AddItemToInventory(recipient, prefab.PrefabGUID, amount, out entity, equip);
		}

		entity = default;
		return false;
	}

	public static bool AddItemToInventory(Player player, PrefabGUID guid, int amount, out Entity entity, bool equip = true, int slot = 0)
	{
		return AddItemToInventory(player.Character, guid, amount, out entity, equip, slot);
	}

	//forceEquip doesn't currently remove your existing item back into your inventory -- automatically removes item from inventory, only use when you don't know where the item is
	public static void EquipItem(Entity entity, Entity itemEntity, bool forceEquip = false)
	{
		if (itemEntity.Has<EquippableData>())
		{
			var equippableData = itemEntity.Read<EquippableData>();

			if (entity.Has<Equipment>())
			{
				var equipment = entity.Read<Equipment>();
				if (forceEquip || !equipment.IsEquipped(equippableData.EquipmentType))
				{
					SetEquipmentSlot(entity, equipment, equippableData.EquipmentType, itemEntity);
					Helper.RemoveEquippedItemFromInventory(itemEntity);
				}
			}
			else if (entity.Has<ServantEquipment>())
			{
				var servantEquipment = entity.Read<ServantEquipment>();
				if (forceEquip || !servantEquipment.IsEquipped(equippableData.EquipmentType))
				{
					SetServantEquipmentSlot(ref servantEquipment, equippableData.EquipmentType, itemEntity);
					entity.Write(servantEquipment);
					Helper.RemoveEquippedItemFromInventory(itemEntity);
				}
			}
		}
	}

	public static void EquipItem(Entity entity, Entity itemEntity, int slot)
	{
		if (itemEntity.Has<EquippableData>())
		{
			var equippableData = itemEntity.Read<EquippableData>();

			if (entity.Has<Equipment>())
			{
				var equipment = entity.Read<Equipment>();
				if (!equipment.IsEquipped(equippableData.EquipmentType))
				{
					SetEquipmentSlot(entity, equipment, equippableData.EquipmentType, itemEntity);
					Helper.RemoveEquippedItemFromInventory(itemEntity, slot);
				}
			}
			else if (entity.Has<ServantEquipment>())
			{
				var servantEquipment = entity.Read<ServantEquipment>();
				if (!servantEquipment.IsEquipped(equippableData.EquipmentType))
				{
					SetServantEquipmentSlot(ref servantEquipment, equippableData.EquipmentType, itemEntity);
					entity.Write(servantEquipment);
					Helper.RemoveEquippedItemFromInventory(itemEntity, slot);
				}
			}
		}
	}

	private static void SetEquipmentSlot(Entity equipmentOwner, Equipment equipment, EquipmentType equipmentType, Entity itemEntity)
	{
		var slot = new EquipmentSlot
		{
			SlotEntity = NetworkedEntity.ServerEntity(itemEntity),
			SlotId = itemEntity.GetPrefabGUID()
		};

		switch (equipmentType)
		{
			case EquipmentType.Chest:
				equipment.ArmorChestSlot = slot;
				break;
			case EquipmentType.Weapon:
				equipment.WeaponSlot = slot;
				break;
			case EquipmentType.MagicSource:
				equipment.GrimoireSlot = slot;
				break;
			case EquipmentType.Footgear:
				equipment.ArmorFootgearSlot = slot;
				break;
			case EquipmentType.Legs:
				equipment.ArmorLegsSlot = slot;
				break;
			case EquipmentType.Gloves:
				equipment.ArmorGlovesSlot = slot;
				break;
			case EquipmentType.Bag:
				equipment.BagSlot = slot;
				break;
			case EquipmentType.Cloak:
				equipment.CloakSlot = slot;
				break;
			case EquipmentType.Headgear:
				equipment.ArmorHeadgearSlot = slot;
				break;
		}
		equipmentOwner.Write(equipment);
		var eventEntity = Helper.CreateEntityWithComponents<EquipmentChangedEvent>();
		eventEntity.Write(new EquipmentChangedEvent
		{
			EquipmentType = equipmentType,
			ItemEntity = itemEntity,
			ChangeType = EquipmentChangedEventType.Equipped,
			Item = itemEntity.GetPrefabGUID(),
			Target = equipmentOwner
		});
	}

	private static void SetServantEquipmentSlot(ref ServantEquipment equipment, EquipmentType equipmentType, Entity itemEntity)
	{
		switch (equipmentType)
		{
			case EquipmentType.Chest:
				equipment.ArmorChestSlotEntity = NetworkedEntity.ServerEntity(itemEntity);
				equipment.ArmorChestSlotId = itemEntity.GetPrefabGUID();
				break;
			case EquipmentType.Weapon:
				equipment.WeaponSlotEntity = NetworkedEntity.ServerEntity(itemEntity);
				equipment.WeaponSlotId = itemEntity.GetPrefabGUID();
				break;
			case EquipmentType.MagicSource:
				equipment.GrimoireSlotEntity = NetworkedEntity.ServerEntity(itemEntity);
				equipment.GrimoireSlotId = itemEntity.GetPrefabGUID();
				break;
			case EquipmentType.Footgear:
				equipment.ArmorFootgearSlotEntity = NetworkedEntity.ServerEntity(itemEntity);
				equipment.ArmorFootgearSlotId = itemEntity.GetPrefabGUID();
				break;
			case EquipmentType.Legs:
				equipment.ArmorLegsSlotEntity = NetworkedEntity.ServerEntity(itemEntity);
				equipment.ArmorLegsSlotId = itemEntity.GetPrefabGUID();
				break;
			case EquipmentType.Gloves:
				equipment.ArmorGlovesSlotEntity = NetworkedEntity.ServerEntity(itemEntity);
				equipment.ArmorGlovesSlotId = itemEntity.GetPrefabGUID();
				break;
		}
	}

	public static bool AddItemToInventory(Entity recipient, PrefabGUID guid, int amount, out Entity entity, bool equip = true, int slot = 0, bool drop = true)
	{
		var inventoryResponse = Core.serverGameManager.TryAddInventoryItem(recipient, guid, amount, new Nullable_Unboxed<int>(slot), false);
		if (inventoryResponse.RemainingAmount > 0 && drop)
		{
			if (recipient.Has<Translation>())
			{
				CreateDroppedItem(recipient.Read<Translation>().Value, guid, inventoryResponse.RemainingAmount);
			}
		}
		if (inventoryResponse.Success)
		{
			entity = inventoryResponse.NewEntity;
			if (equip)
			{
				EquipItem(recipient, entity);
			}
			return true;
		}
		else
		{
			entity = new Entity();
			return false;
		}
		
/*		return AddItemToInventoryWorkaround(recipient, guid, amount, out entity, equip, slot);
		var player = PlayerService.GetPlayerFromCharacter(recipient);
		var giveEvent = new GiveDebugEvent
		{
			Amount = amount,
			PrefabGuid = guid
		};
		
		
		entity = Entity.Null;

		return true;
		var itemSettings = AddItemSettings.Create(VWorld.Server.EntityManager, Core.gameDataSystem.ItemHashLookupMap);
		itemSettings.EquipIfPossible = equip;
		itemSettings.DropRemainder = true;
		itemSettings.StartIndex = new Nullable_Unboxed<int>(slot);
		var inventoryResponse = InventoryUtilitiesServer.TryAddItem(itemSettings, recipient, guid, amount);
		if (inventoryResponse.Success)
		{
			entity = inventoryResponse.NewEntity;
			return true;
		}
		else
		{
			entity = new Entity();
			return false;
		}*/
	}
	
	public static bool TryGetItemAtSlot(Player player, int slot, out InventoryBuffer item)
	{
		return InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, player.Character, slot, out item);
	}

	public static bool TryGetItemAtSlot(Entity inventoryEntity, int slot, out InventoryBuffer item)
	{
		return InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, inventoryEntity, slot, out item);
	}

	public static void EquipJewelAtSlot(Player player, int inventoryIndex)
	{
		Entity equipJewelEventEntity = VWorld.Server.EntityManager.CreateEntity(
			ComponentType.ReadWrite<FromCharacter>(),
			ComponentType.ReadWrite<EquipJewelEvent>()
		);
		equipJewelEventEntity.Write(player.ToFromCharacter());
		equipJewelEventEntity.Write(new EquipJewelEvent
		{
			InventoryIndex = inventoryIndex
		});
	}

	public static void UnequipItem(Player player, EquipmentType equipmentType, int slot = 0)
	{
		var entity = Helper.CreateEntityWithComponents<FromCharacter, UnequipItemEvent>();
		entity.Write(player.ToFromCharacter());
		entity.Write(new UnequipItemEvent
		{
			EquipmentType = equipmentType,
			ToInventory = player.Character.Read<NetworkId>(),
			ToSlotIndex = slot
		});
	}

/*	public static void UnequipItemToTargetInventory(Player player, EquipmentType equipmentType, Entity targetInventory, int slot = -1)
	{
		if (slot == -1)
		{
			var buffer = targetInventory.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity.ReadBuffer<InventoryBuffer>();
			int index;
			for (index = 0; index < buffer.Length; index++)
			{
				if (buffer[index].ItemType == PrefabGUID.Empty)
				{
					slot = index;
					break;
				}
			}
		}
		
		var entity = Helper.CreateEntityWithComponents<FromCharacter, UnequipItemEvent>();
		entity.Write(player.ToFromCharacter());
		entity.Write(new UnequipItemEvent
		{
			EquipmentType = equipmentType,
			ToInventory = targetInventory.Read<NetworkId>(),
			ToSlotIndex = slot
		});
	}

	public static void UnequipAllItemsToTargetInventory(Player player, Entity targetInventory)
	{
		for (var i = 0; i < EquipmentTypes.Count; i++)
		{
			Helper.UnequipItemToTargetInventory(player, EquipmentTypes[i], targetInventory);
		}
	}*/

	public static void UnequipAllItems(Player player)
	{
		for (var i = 0; i < EquipmentTypes.Count; i++)
		{
			Helper.UnequipItem(player, EquipmentTypes[i], i);
		}
	}

	public static void UnequipAllItemsToTargetInventory(Player player, Entity targetInventory)
	{
		if (!targetInventory.Exists()) return;

		if (!targetInventory.Has<InventoryBuffer>() && targetInventory.Has<InventoryInstanceElement>())
		{
			targetInventory = targetInventory.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
		}
		if (!targetInventory.Exists()) return;

		var equipment = player.Character.Read<Equipment>();
		var results = new NativeList<Entity>(Allocator.Temp);
		equipment.GetAllEquipmentEntities(results, true);
		foreach (var item in results)
		{
			var buffer = targetInventory.ReadBuffer<InventoryBuffer>();
			for (var i = 0; i < buffer.Length; i++)
			{
				var bufferItem = buffer[i];
				if (bufferItem.ItemType == PrefabGUID.Empty)
				{
					bufferItem.ItemEntity = item;
					InventoryUtilitiesServer.SetInventoryContainer(VWorld.Server.EntityManager, item, targetInventory);
					bufferItem.ItemType = item.GetPrefabGUID();
					bufferItem.Amount = 1;
					buffer[i] = bufferItem;
					equipment.UnequipItem(VWorld.Server.EntityManager, player.Character, item.Read<EquippableData>().EquipmentType);
					player.Character.Write(equipment);
					break;
				}
			}
		}
	}

	//equip if possible..
	public static void TransferAllItemsToTargetInventory(Entity sourceInventory, Entity targetInventory)
	{
		var targetInventoryOwner = targetInventory;
		if (sourceInventory.Has<InventoryInstanceElement>() && !sourceInventory.Has<InventoryBuffer>())
		{
			sourceInventory = sourceInventory.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
		}
		if (targetInventory.Has<InventoryInstanceElement>() && !targetInventory.Has<InventoryBuffer>())
		{
			targetInventory = targetInventory.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
			if (targetInventory.Has<InventoryConnection>())
			{
				targetInventoryOwner = targetInventory.Read<InventoryConnection>().InventoryOwner;
			}
		}

		var inventoryBufferSource = sourceInventory.ReadBuffer<InventoryBuffer>();
		var inventoryBufferTarget = targetInventory.ReadBuffer<InventoryBuffer>();

		var emptySlots = new List<int>();
		for (int i = 0; i < inventoryBufferTarget.Length; i++)
		{
			if (inventoryBufferTarget[i].ItemType == PrefabGUID.Empty)
			{
				emptySlots.Add(i);
			}
		}

		//1st pass to equip (to make sure bag is equipped)
		if (targetInventoryOwner.Has<PlayerCharacter>())
		{
			var player = PlayerService.GetPlayerFromCharacter(targetInventoryOwner);
			for (var i = 0; i < inventoryBufferSource.Length; i++)
			{
				var item = inventoryBufferSource[i];
				if (item.ItemType != PrefabGUID.Empty)
				{
					var itemEntity = item.ItemEntity._Entity;
					if (itemEntity.Exists() && itemEntity.Has<EquippableData>())
					{
						if (!player.Character.Read<Equipment>().IsEquipped(itemEntity.Read<EquippableData>().EquipmentType))
						{
							Helper.EquipItem(targetInventoryOwner, itemEntity, i);
						}
					}
				}
			}
		}


		for (var i = 0; i < inventoryBufferSource.Length; i++)
		{
			var item = inventoryBufferSource[i];
			if (item.ItemType != PrefabGUID.Empty)
			{
				if (emptySlots.Count > 0)
				{
					// Fill into the first empty slot
					InventoryUtilitiesServer.SetInventoryContainer(VWorld.Server.EntityManager, item.ItemEntity._Entity, targetInventory);
					inventoryBufferTarget[emptySlots[0]] = item;
					emptySlots.RemoveAt(0); // Remove the filled slot
					item = new InventoryBuffer();
					inventoryBufferSource[i] = item;
				}
			}
		}
	}

	public static string GetItemName(PrefabGUID itemPrefabGUID)
	{
		if (Items.PrefabsToNames.TryGetValue(itemPrefabGUID, out var name))
		{
			if (name == "")
			{
				return itemPrefabGUID.LookupName();
			}
			return name;
		}
		else
		{
			return itemPrefabGUID.LookupName();
		}
	}

    public static void MergeInventoriesWorkaround(Entity sourceInventory, Entity targetInventory)
    {
		if (!sourceInventory.Exists() || !targetInventory.Exists()) return;

        // Check and handle external inventory reference
        if (targetInventory.Has<InventoryInstanceElement>())
        {
            targetInventory = targetInventory.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
        }
		if (sourceInventory.Has<InventoryInstanceElement>())
		{
			sourceInventory = sourceInventory.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
		}

		if (!sourceInventory.Exists() || !targetInventory.Exists()) return;

		var sourceBuffer = sourceInventory.ReadBuffer<InventoryBuffer>();
		var targetBuffer = targetInventory.ReadBuffer<InventoryBuffer>();

        // First, try to merge items into existing slots
        for (int i = 0; i < targetBuffer.Length; i++)
        {
            var targetItem = targetBuffer[i];

            for (int j = sourceBuffer.Length - 1; j >= 0; j--)
            {
                var sourceItem = sourceBuffer[j];
                if (sourceItem.ItemType != PrefabGUID.Empty && sourceItem.ItemType == targetItem.ItemType && targetItem.Amount < targetItem.ItemEntity._Entity.Read<ItemData>().MaxAmount)
                {
                    var sourceItemData = sourceItem.ItemEntity._Entity.Read<ItemData>();

                    // Calculate the possible amount to transfer
                    int amountToAdd = System.Math.Min(sourceItem.Amount, sourceItemData.MaxAmount - targetItem.Amount);

                    // Update source and target item amounts
                    sourceItem.Amount -= amountToAdd;
                    targetItem.Amount += amountToAdd;

					// Update the data back to the entity
					sourceBuffer[j] = sourceItem;
                    targetBuffer[i] = targetItem;

                    // If source item is depleted, reset it to default
                    if (sourceItem.Amount == 0)
                    {
						Helper.DestroyEntity(sourceItem.ItemEntity._Entity);
						sourceBuffer[j] = new InventoryBuffer();
                    }
                }
            }
        }

        // Next, handle any remaining items by finding empty slots or adding new slots if possible
        for (int j = sourceBuffer.Length - 1; j >= 0; j--)
        {
            var sourceItem = sourceBuffer[j];
			if (sourceItem.ItemType == PrefabGUID.Empty) continue;
            var sourceItemData = sourceItem.ItemEntity._Entity.Read<ItemData>();
            if (sourceItem.Amount > 0)
            {
                bool itemPlaced = false;
                for (int i = 0; i < targetBuffer.Length && !itemPlaced; i++)
                {
                    var targetItem = targetBuffer[i];
                    if (targetItem.ItemType == PrefabGUID.Empty || (targetItem.ItemType == sourceItem.ItemType && targetItem.Amount < sourceItemData.MaxAmount))
                    {
                        // Calculate the possible amount to transfer
                        int amountToAdd = System.Math.Min(sourceItem.Amount, sourceItemData.MaxAmount - targetItem.Amount);

                        // Update source and target item amounts
                        sourceItem.Amount -= amountToAdd;
                        targetItem.Amount += amountToAdd;

                        // Update the data back to the entity
                        targetBuffer[i] = targetItem;
						sourceBuffer[j] = sourceItem;

                        if (sourceItem.Amount == 0)
                        {
                            Helper.DestroyEntity(sourceItem.ItemEntity._Entity);
							sourceBuffer[j] = new InventoryBuffer();
                        }

                        itemPlaced = true;
                    }
                }
            }
        }
    }



    public static void MoveItemToTargetInventory(Entity item, Entity targetInventory)
	{
		//InventoryUtilitiesServer.Internal_TryMoveItem(VWorld.Server.EntityManager, Core.gameDataSystem.ItemHashLookupMap, )
	}
}

using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using ModCore;
using ModCore.Data;
using ProjectM.UI;
using ProjectM.Shared;
using Il2CppInterop.Runtime;
using ModCore.Helpers;
using ModCore.Models;
using ProjectM.CastleBuilding;
using Stunlock.Core;
using System.Linq;
using ProjectM.Gameplay.Scripting;
using ModCore.Factories;
using Unity.Entities.UniversalDelegates;

namespace QuickStash
{
    public class QuickStashServer
    {
        private static readonly HashSet<PrefabGUID> _itemRefreshGuids = new() {
            Prefabs.Item_Ingredient_Mineral_SilverOre,
            Prefabs.Item_Ingredient_Coin_Silver,
            Prefabs.Item_Ingredient_Coin_Royal
        };

        private static readonly Dictionary<Player, DateTime> _lastMerge = new();

        internal static bool MergeInventories(Player player)
        {
            if (_lastMerge.ContainsKey(player) && DateTime.Now - _lastMerge[player] < TimeSpan.FromSeconds(0.5))
            {
                return false;
            }

            if (!(Helper.IsInBase(player, out var territoryEntity, out var territoryAlignment) && territoryAlignment == Helper.TerritoryAlignment.Friendly && !Helper.HasBuff(player, Prefabs.AB_Shapeshift_Bat_TakeFlight_Buff)))
            {
                player.ReceiveMessage($"You must be within your castle territory to stash your items".Error());
                return false;
            }
            _lastMerge[player] = DateTime.Now;

            var gameDataSystem = VWorld.Server.GetExistingSystemManaged<GameDataSystem>();
            var stashEntities = QuickStashShared.GetStashEntities(VWorld.Server.EntityManager);
            var heart = territoryEntity.Read<CastleTerritory>().CastleHeart;

            foreach (var stashEntity in stashEntities)
            {
                if (stashEntity.Read<CastleHeartConnection>().CastleHeartEntity._Entity != heart) continue;

                var nameableInteractable = VWorld.Server.EntityManager.GetComponentData<NameableInteractable>(stashEntity);
                if (nameableInteractable.Name.ToString().EndsWith("*"))
                {
                    continue;
                }

                //var stashInventoryEntity = stashEntity.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;

                
                MergeInventoriesWorkaround(player.Character, stashEntity);
                /*                var eventEntity = Helper.CreateEntityWithComponents<FromCharacter, SmartMergeItemsBetweenInventoriesEvent>();
                                eventEntity.Write(player.ToFromCharacter());//
                                Plugin.PluginLog.LogInfo($"making event with ids: {player.Character.Read<NetworkId>()} {stashEntity.Read<NetworkId>()}");
                                eventEntity.Write(new SmartMergeItemsBetweenInventoriesEvent
                                {
                                    FromInventory = player.Character.Read<NetworkId>(),
                                    ToInventory = stashEntity.Read<NetworkId>(),
                                    Options = SmartMergeOptions.None
                                });*/

            }

            // Refresh silver debuff
/*            foreach (var prefabGuid in _itemRefreshGuids)
            {
                InventoryUtilitiesServer.CreateInventoryChangedEvent(VWorld.Server.EntityManager, player.Character, prefabGuid, 1000, Entity.Null, InventoryChangedEventType.Moved); //todo check me
            }*/

            return true;
        }

        

        public static void MergeInventoriesWorkaround(Entity sourceInventoryEntity, Entity targetInventoryEntity)
        {
            if (!sourceInventoryEntity.Exists() || !targetInventoryEntity.Exists())
            {
                Plugin.PluginLog.LogInfo("Either source or target inventory does not exist.");
                return;
            }

            if (targetInventoryEntity.Has<InventoryInstanceElement>())
            {
                targetInventoryEntity = targetInventoryEntity.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
            }
            if (sourceInventoryEntity.Has<InventoryInstanceElement>())
            {
                sourceInventoryEntity = sourceInventoryEntity.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
            }

            if (!sourceInventoryEntity.Exists() || !targetInventoryEntity.Exists())
            {
                Plugin.PluginLog.LogInfo("Either redirected source or target inventory does not exist.");
                return;
            }

            var sourceInventory = sourceInventoryEntity.ReadBuffer<InventoryBuffer>();
            var targetInventory = targetInventoryEntity.ReadBuffer<InventoryBuffer>();

            Queue<int> emptySlots = new Queue<int>();
            Dictionary<PrefabGUID, PriorityQueue<int, int>> priorityQueues = new Dictionary<PrefabGUID, PriorityQueue<int, int>>();

            // Populate empty slot stack and priority queues for each item type in the target inventory
            for (int i = 0; i < targetInventory.Length; i++)
            {
                if (!Core.gameDataSystem.ItemHashLookupMap.TryGetValue(targetInventory[i].ItemType, out var targetData))
                {
                    emptySlots.Enqueue(i);
                }
                else
                {
                    if (!priorityQueues.ContainsKey(targetInventory[i].ItemType))
                    {
                        priorityQueues[targetInventory[i].ItemType] = new PriorityQueue<int, int>();
                    }
                    priorityQueues[targetInventory[i].ItemType].Enqueue(i, targetData.MaxAmount - targetInventory[i].Amount);
                }
            }

            // Merge items from the source inventory to the target inventory
            for (int i = 0; i < sourceInventory.Length; i++)
            {
                var sourceItem = sourceInventory[i];

                if (sourceItem.ItemType == PrefabGUID.Empty) continue;

                if (priorityQueues.ContainsKey(sourceItem.ItemType) && Core.gameDataSystem.ItemHashLookupMap.TryGetValue(sourceInventory[i].ItemType, out var sourceData))
                {
                    while (sourceInventory[i].Amount > 0)
                    {
                        while (sourceInventory[i].Amount > 0 && priorityQueues[sourceInventory[i].ItemType].Count > 0)
                        {
                            int targetIndex = priorityQueues[sourceItem.ItemType].Dequeue();
                            int transferAmount = Math.Min(sourceInventory[i].Amount, sourceData.MaxAmount - targetInventory[targetIndex].Amount);
                            InventoryUtilitiesServer.CreateInventoryChangedEvent(VWorld.Server.EntityManager, sourceInventoryEntity, sourceItem.ItemType, transferAmount, targetInventory[targetIndex].ItemEntity._Entity, InventoryChangedEventType.Moved);
                            var targetItem = targetInventory[targetIndex];
                            targetItem.Amount = targetInventory[targetIndex].Amount + transferAmount;
                            targetInventory[targetIndex] = targetItem;

                            sourceItem.Amount = sourceInventory[i].Amount - transferAmount;

                            if (sourceItem.Amount == 0)
                            {
                                Helper.DestroyEntity(sourceItem.ItemEntity._Entity);
                                sourceInventory[i] = new InventoryBuffer();
                            }
                            else
                            {
                                sourceInventory[i] = sourceItem;
                            }

                            if (targetItem.Amount < sourceData.MaxAmount)
                            {
                                priorityQueues[sourceItem.ItemType].Enqueue(targetIndex, sourceData.MaxAmount - targetInventory[targetIndex].Amount);
                            }
                        }

                        if (sourceItem.Amount > 0 && emptySlots.Count > 0)
                        {
                            int emptySlot = emptySlots.Dequeue();
                            int transferAmount = Math.Min(sourceInventory[i].Amount, sourceData.MaxAmount);
                            targetInventory[emptySlot] = sourceItem;
                            InventoryUtilitiesServer.CreateInventoryChangedEvent(VWorld.Server.EntityManager, sourceInventoryEntity, sourceItem.ItemType, sourceItem.Amount, targetInventory[emptySlot].ItemEntity._Entity, InventoryChangedEventType.Moved);
                            sourceInventory[i] = new InventoryBuffer();

                            if (transferAmount < sourceData.MaxAmount)
                            {
                                priorityQueues[sourceItem.ItemType].Enqueue(emptySlot, sourceData.MaxAmount - transferAmount);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        internal static bool CleanChests(Player player)
        {
            Entity heart = Entity.Null;
            if (_lastMerge.ContainsKey(player) && DateTime.Now - _lastMerge[player] < TimeSpan.FromSeconds(0.5))
            {
                return false;
            }

            if (Helper.TryGetCurrentCastleTerritory(player, out var territoryEntity))
            {
                heart = territoryEntity.Read<CastleTerritory>().CastleHeart;
                if (!heart.Exists() || !Team.IsAllies(player.Character.Read<Team>(), heart.Read<Team>()))
                {
                    player.ReceiveMessage($"You must be within your castle territory to clean your chests".Error());
                    return false;
                }
            }

            _lastMerge[player] = DateTime.Now;

            var stashEntities = QuickStashShared.GetStashEntities(VWorld.Server.EntityManager);
            foreach (var stashEntity in stashEntities)
            {
                if (stashEntity.Read<CastleHeartConnection>().CastleHeartEntity._Entity != heart) continue;

                var nameableInteractable = VWorld.Server.EntityManager.GetComponentData<NameableInteractable>(stashEntity);
                if (nameableInteractable.Name.ToString().EndsWith("-"))
                {
                    InventoryUtilitiesServer.ClearInventory(VWorld.Server.EntityManager, stashEntity);
                }
            }

            return true;
        }
    }
}

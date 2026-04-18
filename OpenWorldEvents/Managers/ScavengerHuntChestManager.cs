using ProjectM;
using System.Collections.Generic;
using Unity.Entities;
using ModCore;
using ModCore.Data;
using ModCore.Helpers;
using ModCore.Services;
using System.Threading;
using System;
using Unity.Mathematics;
using ModCore.Factories;
using ProjectM.Network;
using PointsMod;
using ProjectM.CastleBuilding;
using ModCore.Models;
using ProjectM.Terrain;
using ModCore.Events;
using Unity.Transforms;
using static ModCore.Events.GameEvents;

namespace OpenWorldEvents.Managers
{
    public static class ScavengerHuntChestManager
    {
        public static Dictionary<string, Entity> ChestNameToChests = new();
        private static List<Timer> Timers = new();
        private static bool HasInitialized = false;
        public static void Initialize(bool firstLoad = false)
        {
            if (!HasInitialized)
            {
                if (firstLoad)
                {
                    FindExistingChests();
                }
                
                if (ChestNameToChests.Count > 0)
                {
                    GameEvents.OnPlayerTransferredAllItems += HandleOnPlayerTransferredAllItems;
                    GameEvents.OnPlayerTransferredItem += HandleOnPlayerTransferredItem;
                    HasInitialized = true;
                }
            }
        }

        public static void Dispose(bool hard = true)
        {
            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();

            GameEvents.OnPlayerTransferredAllItems -= HandleOnPlayerTransferredAllItems;
            GameEvents.OnPlayerTransferredItem -= HandleOnPlayerTransferredItem;
            ChestNameToChests.Clear();
            HasInitialized = false;
        }

        public static void FindExistingChests()
        {
            var chests = Helper.GetEntitiesByComponentTypes<CanFly, DestroyAfterTimeOnInventoryChange>();
            foreach (var chest in chests)
            {
                var category = UnitFactory.GetCategory(chest);
                if (ScavengerHuntChestConfig.Config.ScavengerHuntChestLoot.TryGetValue(category, out var scavengerChestData))
                {
                    ChestNameToChests[category] = chest;
                }
            }
        }

        public static void HandleOnPlayerTransferredItem(Player player, Entity eventEntity, MoveItemBetweenInventoriesEvent moveItemBetweenInventoriesEvent)
        {
            if (Helper.TryGetEntityFromNetworkId(moveItemBetweenInventoriesEvent.FromInventory, out var fromInventory))
            {
                var category = UnitFactory.GetCategory(fromInventory);
                if (ScavengerHuntChestConfig.Config.ScavengerHuntChestLoot.TryGetValue(category, out var scavengerChestData))
                {
                    if (!fromInventory.Has<VampireDownedBuff>())
                    {
                        //Helper.SendSystemMessageToAllClients($"{player.Name.Colorify(ExtendedColor.ClanNameColor)} has looted the {scavengerChestData.ChestName} in {player.WorldZoneString}!");
                        ChestNameToChests.Remove(category);
                        if (ChestNameToChests.Count == 0)
                        {
                            Dispose();
                        }
                        fromInventory.Add<VampireDownedBuff>(); //using as a flag
                        var action = () =>
                        {
                            if (fromInventory.Exists())
                            {
                                Helper.DestroyEntity(fromInventory);
                            }
                        };
                        ActionScheduler.RunActionOnceAfterDelay(action, 300);
                    }
                }
            }
        }

        public static void HandleOnPlayerTransferredAllItems(Player player, Entity eventEntity, MoveAllItemsBetweenInventoriesEvent moveAllItemsBetweenInventoriesEvent)
        {
            if (Helper.TryGetEntityFromNetworkId(moveAllItemsBetweenInventoriesEvent.FromInventory, out var fromInventory))
            {
                var category = UnitFactory.GetCategory(fromInventory);
                if (ScavengerHuntChestConfig.Config.ScavengerHuntChestLoot.TryGetValue(category, out var scavengerChestData))
                {
                    if (!fromInventory.Has<VampireDownedBuff>())
                    {
                        Helper.SendSystemMessageToAllClients($"{player.Name.Colorify(ExtendedColor.ClanNameColor)} has looted the {scavengerChestData.ChestName} in {player.WorldZoneString}!");
                        ChestNameToChests.Remove(category);
                        if (ChestNameToChests.Count == 0)
                        {
                            Dispose();
                        }
                        fromInventory.Add<VampireDownedBuff>(); //using as a flag
                        var action = () =>
                        {
                            if (fromInventory.Exists())
                            {
                                Helper.DestroyEntity(fromInventory);
                            }
                        };
                        ActionScheduler.RunActionOnceAfterDelay(action, 300);
                    }
                }
            }
        }

        public static bool SpawnChest(string chestName, Player player)
        {
            if (ScavengerHuntChestConfig.Config.ScavengerHuntChestLoot.TryGetValue(chestName, out var chestData))
            {
                Unit unit;
                if (chestData.Greater)
                {
                    unit = new Unit(Prefabs.Chain_Container_WorldChest_Epic_01);
                }
                else
                {
                    unit = new Unit(Prefabs.Chain_Container_WorldChest_Iron_01);
                }

                UnitFactory.SpawnUnitWithCallback(unit, player.Position, (e) =>
                {
                    var activeChild = e.Read<SpawnChainData.ActiveChildElement>();
                    var chest = activeChild.ActiveEntity;
                    chest.Add<CanFly>();
                    UnitFactory.AddCategory(chest, chestName.ToLower());
                    chest.Write(player.Character.Read<Rotation>());
                    chest.Remove<DropInInventoryOnSpawn>();
                    chest.Remove<DestroyWhenInventoryIsEmpty>();
                    chest.Remove<TransitionWhenInventoryIsEmpty>();
                    var destroyAfterTimeOnInventoryChange = chest.Read<DestroyAfterTimeOnInventoryChange>();
                    destroyAfterTimeOnInventoryChange.Duration = float.MaxValue;
                    chest.Write(destroyAfterTimeOnInventoryChange);
                    var destroyAfterDuration = chest.Read<DestroyAfterDuration>();
                    destroyAfterDuration.Duration = float.MaxValue;
                    chest.Write(destroyAfterDuration);

                    var action = () =>
                    {
                        if (chest.Exists())
                        {
                            foreach (var item in chestData.Items)
                            {
                                Helper.AddItemToInventory(chest, item.ItemPrefabGUID, item.Quantity, out Entity itemEntity);
                            }
                            chest.Add<DestroyWhenInventoryIsEmpty>();
                            ChestNameToChests[chestName] = chest;
                            Initialize();
                        }
                        if (e.Exists())
                        {
                            e.Write(new SpawnChainData.ActiveChildElement());
                            Helper.DestroyEntity(e);
                        }
                    };
                    Timers.Add(ActionScheduler.RunActionOnceAfterFrames(action, 2));
                });
                return true;
            }
            return false;
        }

        public static void AnnounceChest(Player player, string chestName)
        {
            if (TryFindChestByName(chestName, out var chest))
            {
                if (ScavengerHuntChestConfig.Config.ScavengerHuntChestLoot.TryGetValue(chestName, out var chestData))
                {
                    var timer = ActionScheduler.RunActionOnceAfterDelay(() =>
                    {
                        chest.Add<Interactable>(); //todo remove interactable on chest spawn
                    }, ScavengerHuntChestConfig.Config.ChestDelaySeconds);
                    Timers.Add(timer);
                    string eventType = chestData.Greater ? "[Greater Event] Greater Cache -" : "[Lesser Event] Lesser Cache -";
                    Helper.SendSystemMessageToAllClients($"{eventType} A chest has spawned in {player.WorldZoneString}. Follow the clues to find the loot!");
                }
            }
        }

        public static bool TryFindChestByName(string chestName, out Entity foundChest)
        {
            return ChestNameToChests.TryGetValue(chestName, out foundChest);
        }
    }
}

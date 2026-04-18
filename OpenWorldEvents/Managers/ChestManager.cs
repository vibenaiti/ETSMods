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

namespace OpenWorldEvents.Managers
{
    public static class ChestManager
    {
        private const string Category = "eventchest";
        private static List<Timer> Timers = new();
        public static Dictionary<Entity, Entity> ChestToMapIcon = new();
        private static System.Random random = new();
        public static void Initialize()
        {
            var cleanUpMapIconAction = () =>
            {
                var chestsToRemove = new List<Entity>();
                foreach (var kvp in ChestToMapIcon)
                {
                    if (!kvp.Key.Exists())
                    {
                        chestsToRemove.Add(kvp.Key);
                    }
                }

                foreach (var chest in chestsToRemove)
                {
                    DisposeChest(chest);
                }
            };

            var timer = ActionScheduler.RunActionEveryInterval(cleanUpMapIconAction, 1);
            Timers.Add(timer);
            EventHelper.SetDeathDurabilityLoss(0);
            PointsMod.Globals.CurrencyMultiplier = OpenWorldEventsConfig.Config.CurrencyMultiplierDuringEvents;
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
            foreach (var mapIcon in ChestToMapIcon.Values)
            {
                if (mapIcon.Exists())
                {
                    Helper.DestroyEntity(mapIcon);
                }
            }
            ChestToMapIcon.Clear();
            DisposeLingeringUnits();
            if (hard)
            {
                EventHelper.SetDeathDurabilityLoss(OpenWorldEventsConfig.Config.DefaultDurabilityLoss);
                PointsMod.Globals.CurrencyMultiplier = 1;
            }
            else
            {
                Helper.SendSystemMessageToAllClients($"{"Durability loss".White()} will be re-enabled in {Helper.FormatTime(OpenWorldEventsConfig.Config.DurabilityDelayAfterEvent).Emphasize()}!".Warning());
                var action = () =>
                {
                    Helper.SendSystemMessageToAllClients($"{"Durability loss".White()} is now {"enabled".Emphasize()}!".Warning());
                    EventHelper.SetDeathDurabilityLoss(OpenWorldEventsConfig.Config.DefaultDurabilityLoss);
                    PointsMod.Globals.CurrencyMultiplier = 1;
                };
                Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, OpenWorldEventsConfig.Config.DurabilityDelayAfterEvent));
            }
        }

        public static void DisposeLingeringUnits()
        {
            var spawnedEntities = Helper.GetEntitiesByComponentTypes<CanFly, InventoryOwner>();
            foreach (var entity in spawnedEntities)
            {
                if (UnitFactory.HasCategory(entity, Category))
                {
                    Helper.DestroyEntity(entity);
                }
            }
            spawnedEntities.Dispose();

            var mapIconEntities = Helper.GetEntitiesByComponentTypes<MapIconData>();
            foreach (var mapIconEntity in mapIconEntities)
            {
                var prefabGuid = mapIconEntity.GetPrefabGUID();
                if (prefabGuid == Prefabs.MapIcon_DraculasCastle)
                {
                    Helper.DestroyEntity(mapIconEntity);
                }
            }
            mapIconEntities.Dispose();
        }

        public static void SpawnChestAtRandomLocation(int index = -1)
        {
            if (index == -1)
            {
                index = random.Next(ChestConfig.Config.ChestSpawnLocations.Count);
            }
            SpawnChest(ChestConfig.Config.ChestSpawnLocations[index].ToFloat3());
        }

        public static void SpawnChest(float3 pos)
        {
            Dispose();
            Unit unit = new Unit(Prefabs.Chain_Container_WorldChest_Epic_01);
            unit.Category = Category;
            UnitFactory.SpawnUnitWithCallback(unit, pos, (e) =>
            {
                var activeChild = e.Read<SpawnChainData.ActiveChildElement>();
                var chest = activeChild.ActiveEntity;

                Helper.CreateAndAttachMapIconToEntity(chest, Prefabs.MapIcon_DraculasCastle, (mapIcon) =>
                {
                    ChestToMapIcon[chest] = mapIcon;
                    if (ChestToMapIcon.Count == 1)
                    {
                        Initialize();
                    }
                });
                chest.Remove<DropInInventoryOnSpawn>();
                chest.Remove<DestroyWhenInventoryIsEmpty>();
                chest.Remove<Interactable>();
                chest.Remove<TransitionWhenInventoryIsEmpty>();
                var destroyAfterDuration = chest.Read<DestroyAfterDuration>();
                destroyAfterDuration.Duration = float.MaxValue;
                chest.Write(destroyAfterDuration);
                var timer = ActionScheduler.RunActionOnceAfterDelay(() =>
                {
                    chest.Add<DestroyWhenInventoryIsEmpty>();
                    Helper.SendSystemMessageToAllClients($"The {"Gladiator Chest".Emphasize()} is now {"available to loot".White()}!".Warning());
                    chest.Add<Interactable>();
                }, ChestConfig.Config.ChestDelaySeconds);
                Timers.Add(timer);

                var action = () =>
                {
                    if (chest.Exists())
                    {
                        foreach (var item in ChestConfig.Config.ChestItems)
                        {
                            Helper.AddItemToInventory(chest, item.ItemPrefabGUID, item.Quantity, out Entity itemEntity);
                        }
                    }
                    if (e.Exists())
                    {
                        e.Write(new SpawnChainData.ActiveChildElement());
                        Helper.DestroyEntity(e);
                    }
                };
                Timers.Add(ActionScheduler.RunActionOnceAfterFrames(action, 2));
                
                EventHelper.SetDeathDurabilityLoss(0);
                Helper.SendSystemMessageToAllClients($"The {"Gladiator Chest".Emphasize()} has spawned in the {"Colosseum".Emphasize()} and will be available to {"loot".White()} after {Helper.FormatTime(ChestConfig.Config.ChestDelaySeconds).White()}.".Warning());
                Helper.SendSystemMessageToAllClients($"Look for the {"Dracula's Castle".Emphasize()} icon on the map to find it -- {"durability loss is disabled".White()} during the event.".Warning());
            });
        }

        public static void DisposeChest(Entity chest)
        {
            bool chestSpawned = ChestToMapIcon.Count > 0;
            
            if (ChestToMapIcon.TryGetValue(chest, out var mapIcon))
            {
                Helper.DestroyEntity(mapIcon);
                ChestToMapIcon.Remove(chest);
                if (chest.Exists())
                {
                    Helper.DestroyEntity(chest);
                }
            }
            if (ChestToMapIcon.Count == 0)
            {
                Dispose(false);
                
                if (chestSpawned)
                {
                    Helper.SendSystemMessageToAllClients($"The {"Gladiator Chest".Emphasize()} has been {"Looted".Emphasize()}!".Warning());
                }
            }
        }
    }
}

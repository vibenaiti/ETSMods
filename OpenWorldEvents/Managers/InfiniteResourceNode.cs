using ProjectM;
using System.Collections.Generic;
using Unity.Entities;
using ModCore;
using ModCore.Data;
using ModCore.Helpers;
using ModCore.Services;
using System.Threading;
using ModCore.Events;
using ModCore.Models;
using static OpenWorldEventsConfigData;
using System;
using Unity.Mathematics;
using System.Linq;
using ModCore.Factories;
using ProjectM.CastleBuilding;
using PointsMod;
using UnityEngine.UIElements;
using static Unity.Entities.TypeManager;
using UnityEngine;
using Stunlock.Core;
using OpenWorldEvents.Models;

namespace OpenWorldEvents.Managers
{
    public static class InfiniteResourceNodeManager
    {
        private const string Category = "resourcenodeevent";
        private static List<Timer> Timers = new();
        public static Dictionary<Entity, Entity> ResourceNodeToMapIcon = new();
        private static System.Random Random = new();
        private static int Multiplier = 100000;
        private static CumulativeLootTable LootTable;

        public static void Initialize()
        {
            if (ResourceNodeConfig.Config.DisableDurabilityLossDuringEvent)
            {
                EventHelper.SetDeathDurabilityLoss(0);
            }

            PointsMod.Globals.ActiveEvents.Add(Category);
            PointsMod.Globals.CurrencyMultiplier = OpenWorldEventsConfig.Config.CurrencyMultiplierDuringEvents;
            GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
            GameEvents.OnGameFrameUpdate += OnUpdate;
            LootTable = new(ResourceNodeConfig.Config.InfiniteResourceNodeItemsOptions);
            
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
            foreach (var kvp in ResourceNodeToMapIcon)
            {
                if (kvp.Value.Exists())
                {
                    Helper.DestroyEntity(kvp.Value);
                }

                if (kvp.Key.Exists())
                {
                    Helper.DestroyEntity(kvp.Key);
                }
            }

            ResourceNodeToMapIcon.Clear();
            GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
            GameEvents.OnGameFrameUpdate -= OnUpdate;
            DisposeLingeringUnits();
            RestorePlayerResourceYieldBonuses();

            if (hard)
            {
                PointsMod.Globals.ActiveEvents.Remove(Category);
                PointsMod.Globals.CurrencyMultiplier = 1;
                EventHelper.SetDeathDurabilityLoss(OpenWorldEventsConfig.Config.DefaultDurabilityLoss);
            }
            else
            {
                var action = () =>
                {
                    if (ResourceNodeConfig.Config.AnnounceEvent && ResourceNodeConfig.Config.DisableDurabilityLossDuringEvent)
                    {
                        Helper.SendSystemMessageToAllClients($"{"Durability loss".White()} is now {"enabled".Emphasize()}!".Warning());
                    }
                    PointsMod.Globals.ActiveEvents.Remove(Category);
                    EventHelper.SetDeathDurabilityLoss(OpenWorldEventsConfig.Config.DefaultDurabilityLoss);
                    PointsMod.Globals.CurrencyMultiplier = 1;
                };
                Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, OpenWorldEventsConfig.Config.DurabilityDelayAfterEvent));
                if (ResourceNodeConfig.Config.AnnounceEvent && ResourceNodeConfig.Config.DisableDurabilityLossDuringEvent)
                {
                    Helper.SendSystemMessageToAllClients($"{"Durability loss".White()} will be re-enabled in {Helper.FormatTime(OpenWorldEventsConfig.Config.DurabilityDelayAfterEvent).Emphasize()}!".Warning());
                }
            }
        }

        public static void DisposeLingeringUnits()
        {
            var spawnedEntities = Helper.GetEntitiesByComponentTypes<CanFly, YieldResourcesOnDamageTaken>();
            foreach (var entity in spawnedEntities)
            {
                if (UnitFactory.HasCategory(entity, Category))
                {
                    Helper.DestroyEntity(entity);
                }
            }

            spawnedEntities.Dispose();

            spawnedEntities = Helper.GetEntitiesByComponentTypes<CanFly, DyeableCastleObject>();
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

        private static void HandleOnPlayerDamageDealt(Player player, Entity eventEntity, DealDamageEvent dealDamageEvent)
        {
            if (eventEntity.Exists())
            {
                if (!ResourceNodeToMapIcon.ContainsKey(dealDamageEvent.Target)) return;

                var commandBuffer = Core.entityCommandBufferSystem.CreateCommandBuffer();
                

                float damage;
                if (dealDamageEvent.MainType == MainDamageType.Physical)
                {
                    damage = player.Character.Read<UnitStats>().PhysicalPower;
                }
                else
                {
                    damage = player.Character.Read<UnitStats>().SpellPower;
                }
                
                DealDamageParameters dealDamageParameters = new DealDamageParameters
                {
                    DealDamageFlags = dealDamageEvent.DealDamageFlags,
                    RawDamagePercent = 0,
                    RawDamageValue = damage * dealDamageEvent.MainFactor * ResourceNodeConfig.Config.ReflectedDamageFraction,
                    MainFactor = 1,
                    MainType = dealDamageEvent.MainType,
                    MaterialModifiers = dealDamageEvent.MaterialModifiers,
                };
                DealDamageEvent.CreateDealDamageEvent(commandBuffer, player.Character, dealDamageParameters, player.Character);
                if (dealDamageEvent.RawDamagePercent == 1)
                {
                    eventEntity.Destroy();
                }
            }
        }

        private static void OnUpdate()
        {
            foreach (var node in ResourceNodeToMapIcon.Keys)
            {
                if (!node.Exists()) continue;
                var buffer = node.ReadBuffer<YieldResourcesOnDamageTaken>();
                var randomIndex = Random.Next(ResourceNodeConfig.Config.InfiniteResourceNodeItemsOptions.Count);
                if (ResourceNodeConfig.Config.InfiniteResourceNodeItemsOptions.Count > 0)
                {
                    var randomItem = LootTable.GetRandomItem();
                    buffer.Clear();
                    var modifier = 10;
                    var yieldResourcesOnDamageTaken = new YieldResourcesOnDamageTaken()
                    {
                        Amount = (randomItem.Quantity * Multiplier) * modifier,
                        ItemType = randomItem.ItemPrefabGUID,
                    };
                    buffer.Add(yieldResourcesOnDamageTaken);
                }
            }
        }

        public static void SpawnResourceNodeAtRandomLocation(int index = -1)
        {
            if (index == -1)
            {
                index = Random.Next(ResourceNodeConfig.Config.ResourceNodeSpawnLocations.Count);
            }

            var randomPosition = ResourceNodeConfig.Config.ResourceNodeSpawnLocations[index].ToFloat3();
            SpawnResourceNode(ResourceNodeConfig.Config.ResourceNodePrefab, randomPosition, ResourceNodeConfig.Config.InfiniteResourceNodeDelay);
        }

        public static void SpawnResourceNode(PrefabGUID prefabGUID, float3 pos, int delay = 0)
        {
            Dispose();
            var unit = new Unit(prefabGUID);
            unit.Category = Category;

            UnitFactory.SpawnUnitWithCallback(unit, pos, (e) =>
            {
                Helper.CreateAndAttachMapIconToEntity(e, Prefabs.MapIcon_DraculasCastle, (mapIcon) =>
                {
                    var entityCategory = e.Read<EntityCategory>();
                    if (entityCategory.MaterialCategory == MaterialCategory.MassiveResource)
                    {
                        entityCategory.MaterialCategory = MaterialCategory.Mineral; //make sure massive nodes are damageable
                    }
                    e.Write(entityCategory);

                    ResourceNodeToMapIcon[e] = mapIcon;
                    if (ResourceNodeToMapIcon.Count == 1)
                    {
                        Initialize();
                    }

                    float radius = 0.05f; // Radius of the circle
                    float angleIncrement = 360.0f / ResourceNodeConfig.Config.NumberOfLightsInNode; // Angle between each light in degrees
                    var newPos = pos;
                    for (var i = 0; i < ResourceNodeConfig.Config.NumberOfLightsInNode; i++)
                    {
                        float angleInRadians = Mathf.Deg2Rad * (angleIncrement * i); // Convert angle to radians
                        newPos.x = pos.x + Mathf.Cos(angleInRadians) * radius;
                        newPos.z = pos.z + Mathf.Sin(angleInRadians) * radius;
                        var unit = new Unit(Prefabs.TM_Castle_ObjectDecor_Simple_Brazier_Orange);
                        unit.Category = Category;
                        UnitFactory.SpawnUnitWithCallback(unit, newPos, (light) =>
                        {
                            light.Add<CanFly>();
                            if (Helper.BuffEntity(light, Helper.CustomBuff1, out var buffEntity, Helper.NO_DURATION))
                            {
                                Helper.ModifyBuff(buffEntity, BuffModificationTypes.Invulnerable | BuffModificationTypes.Immaterial);
                            }

                            var dyeableCastleObject = light.Read<DyeableCastleObject>();
                            dyeableCastleObject.ActiveColorIndex = (byte)ResourceNodeConfig.Config.LightColorIndex;
                            light.Write(dyeableCastleObject);
                        });
                    }

                    var action = () => Helper.DestroyEntity(e);
                    Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, ResourceNodeConfig.Config.InfiniteResourceNodeDurationSeconds + delay));

                    var multiplier = 100000;
                    var originalHealth = e.Read<Health>().MaxHealth.Value;
                    var finalHealth = originalHealth * multiplier;
                    if (delay > 0)
                    {
                        e.Remove<Health>();
                    }

                    if (delay > 0)
                    {
                        var makeHittableAction = () =>
                        {
                            RemovePlayerResourceYieldBonuses();
                            e.Add<Health>();
                            SetHealth(e, finalHealth);
                            if (ResourceNodeConfig.Config.AnnounceEvent)
                            {
                                Helper.SendSystemMessageToAllClients($"The {"Special Resource Node".Emphasize()} is now {"harvestable".White()}!".Warning());
                            }
                        };
                        Timers.Add(ActionScheduler.RunActionOnceAfterDelay(makeHittableAction, delay));
                    }
                    else
                    {
                        RemovePlayerResourceYieldBonuses();
                        SetHealth(e, finalHealth);
                    }

                    var destroyAction = () =>
                    {
                        RestorePlayerResourceYieldBonuses();
                        if (ResourceNodeConfig.Config.AnnounceEvent)
                        {
                            Helper.SendSystemMessageToAllClients($"The {"Special Resource Node".Emphasize()} has despawned!".Warning());
                        }
                        
                        DisposeResourceNode(e);
                    };
                    Timers.Add(ActionScheduler.RunActionOnceAfterDelay(destroyAction,
                        ResourceNodeConfig.Config.InfiniteResourceNodeDurationSeconds + delay));

                    var messageAction = () =>
                    {
                        if (ResourceNodeConfig.Config.AnnounceEvent)
                        {
                            Helper.SendSystemMessageToAllClients($"The {"Special Resource Node".Emphasize()} will despawn in {"60".Emphasize()} seconds!".Warning());
                        }
                    };
                    Timers.Add(ActionScheduler.RunActionOnceAfterDelay(messageAction, ResourceNodeConfig.Config.InfiniteResourceNodeDurationSeconds + delay - 60));

                    var messageAction2 = () =>
                    {
                        if (ResourceNodeConfig.Config.AnnounceEvent)
                        {
                            Helper.SendSystemMessageToAllClients($"The {"Special Resource Node".Emphasize()} will despawn in {"10".Emphasize()} seconds!".Warning());
                        }
                    };
                    Timers.Add(ActionScheduler.RunActionOnceAfterDelay(messageAction2, ResourceNodeConfig.Config.InfiniteResourceNodeDurationSeconds + delay - 10));

                    var durabilityString = ResourceNodeConfig.Config.DisableDurabilityLossDuringEvent ? $" -- {"durability loss is disabled".White()} during the event." : "";
                    if (delay > 0)
                    {
                        if (ResourceNodeConfig.Config.AnnounceEvent)
                        {
                            Helper.SendSystemMessageToAllClients($"A {"Special Resource Node".Emphasize()} has spawned and will be available to harvest in {Helper.FormatTime(delay).ToString().Emphasize()}. It will last for {Helper.FormatTime(ResourceNodeConfig.Config.InfiniteResourceNodeDurationSeconds).Emphasize()}.".Warning());
                            Helper.SendSystemMessageToAllClients($"Look for the {"Dracula's Castle".Emphasize()} icon on the map to find it{durabilityString}".Warning());
                        }
                    }
                    else
                    {
                        if (ResourceNodeConfig.Config.AnnounceEvent)
                        {
                            Helper.SendSystemMessageToAllClients($"A {"Special Resource Node".Emphasize()} has spawned and will last for {Helper.FormatTime(ResourceNodeConfig.Config.InfiniteResourceNodeDurationSeconds).Emphasize()}. Hit it as much as possible for rewards!".Warning());
                            Helper.SendSystemMessageToAllClients($"Look for the {"Dracula's Castle".Emphasize()} icon on the map to find it{durabilityString}".Warning());
                        }
                    }
                });
            });
        }

        private static void RestorePlayerResourceYieldBonuses()
        {
            foreach (var player in PlayerService.CharacterCache.Values)
            {
                Helper.RemoveBuff(player, Helper.CustomBuff2);
            }
        }

        private static void RemovePlayerResourceYieldBonuses()
        {
            foreach (var player in PlayerService.CharacterCache.Values)
            {
                if (Helper.BuffPlayer(player, Helper.CustomBuff2, out var buffEntity, Helper.NO_DURATION))
                {
                    Helper.ApplyStatModifier(buffEntity, new()
                    {
                        Id = ModificationIdFactory.NewId(),
                        ModificationType = ModificationType.Set,
                        Priority = 100,
                        StatType = UnitStatType.ResourcePower,
                        Modifier = 1,
                        Value = 1
                    });

                    Helper.ApplyStatModifier(buffEntity, new()
                    {
                        Id = ModificationIdFactory.NewId(),
                        ModificationType = ModificationType.Set,
                        Priority = 100,
                        StatType = UnitStatType.ResourceYield,
                        Modifier = 1,
                        Value = 1
                    });
                }
            }
        }

        public static void DisposeResourceNode(Entity resourceNode)
        {
            if (ResourceNodeToMapIcon.TryGetValue(resourceNode, out var mapIcon))
            {
                Helper.DestroyEntity(mapIcon);
                ResourceNodeToMapIcon.Remove(resourceNode);
                if (resourceNode.Exists())
                {
                    Helper.DestroyEntity(resourceNode);
                }
            }

            if (ResourceNodeToMapIcon.Count == 0)
            {
                Dispose(false);
            }
        }

        private static void SetHealth(Entity entity, float healthAmount)
        {
            var health = entity.Read<Health>();
            health.MaxHealth._Value = healthAmount;
            health.Value = healthAmount;
            health.MaxRecoveryHealth = healthAmount;
            entity.Write(health);
        }
    }
}
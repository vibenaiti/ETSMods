using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using ModCore.Events;
using ModCore.Listeners;
using ModCore.Models;
using ModCore;
using ModCore.Helpers;
using ModCore.Data;
using ModCore.Services;
using System.Threading;
using ProjectM.Gameplay.Systems;
using static ModCore.Helpers.Helper;
using ModCore.Factories;
using ProjectM.Scripting;
using HarmonyLib;
using static ProjectM.Network.InteractEvents_Client;
using Unity.Collections;
using Stunlock.Core;
using ProjectM.Shared;
using ProjectM.CastleBuilding;
using ProjectM.CastleBuilding.Rebuilding;
using ProjectM.Gameplay;
using ProjectM.Sequencer;
using static ProjectM.Network.CommandUtility;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using ProjectM.Terrain;
using static ModCore.Events.GameEvents;
using ProjectM.UI;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Rendering.HighDefinition;
using System.Diagnostics;
using Unity.Transforms;

namespace RaidConfig.Managers
{
    public static class ShardManager
    {
        private static List<Timer> Timers = new();
        private static Dictionary<Entity, Entity> ExistingShardsToMapIcons = new();
        private static Dictionary<PrefabGUID, PrefabGUID> ShardNecklacesToMapIcons = new()
        {
            { Prefabs.Item_MagicSource_SoulShard_Dracula, Prefabs.MapIcon_Relic_Standard_Dracula },
            { Prefabs.Item_MagicSource_SoulShard_Manticore, Prefabs.MapIcon_Relic_Standard_WingedHorror },
            { Prefabs.Item_MagicSource_SoulShard_Monster, Prefabs.MapIcon_Relic_Standard_TheMonster },
            { Prefabs.Item_MagicSource_SoulShard_Solarus, Prefabs.MapIcon_Relic_Standard_Solarus  },
        };
        private static Dictionary<PrefabGUID, PrefabGUID> ShardMapIconsToNecklaces = new()
        {
            { Prefabs.MapIcon_Relic_Standard_Dracula, Prefabs.Item_MagicSource_SoulShard_Dracula },
            { Prefabs.MapIcon_Relic_Standard_WingedHorror, Prefabs.Item_MagicSource_SoulShard_Manticore },
            { Prefabs.MapIcon_Relic_Standard_TheMonster, Prefabs.Item_MagicSource_SoulShard_Monster },
            { Prefabs.MapIcon_Relic_Standard_Solarus , Prefabs.Item_MagicSource_SoulShard_Solarus },
        };
        private static Dictionary<PrefabGUID, PrefabGUID> ShardBuffsToMapIcons = new()
        {
            { Prefabs.Item_EquipBuff_MagicSource_Soulshard_Dracula, Prefabs.MapIcon_Relic_Standard_Dracula},
            { Prefabs.Item_EquipBuff_MagicSource_Soulshard_Manticore, Prefabs.MapIcon_Relic_Standard_WingedHorror},
            { Prefabs.Item_EquipBuff_MagicSource_Soulshard_TheMonster, Prefabs.MapIcon_Relic_Standard_TheMonster},
            { Prefabs.Item_EquipBuff_MagicSource_Soulshard_Solarus, Prefabs.MapIcon_Relic_Standard_Solarus},
        };
        private static Dictionary<PrefabGUID, PrefabGUID> ShardNecklacesToVisualBuffs = new()
        {
            { Prefabs.Item_MagicSource_SoulShard_Dracula, Prefabs.AB_Vampire_BloodKnight_ThousandSpears_DashBuff   },
            { Prefabs.Item_MagicSource_SoulShard_Manticore, Prefabs.AB_Manticore_Flame_Chaos_Burn_LongDebuff  },
            { Prefabs.Item_MagicSource_SoulShard_Monster, Prefabs.Buff_Monster_FinalStage_Empowered },
            { Prefabs.Item_MagicSource_SoulShard_Solarus, Prefabs.Buff_Cardinal_Shield_Stack  },
        };
        private static Dictionary<PrefabGUID, PrefabGUID> ShardBuffsToVisualBuffs = new()
        {
            { Prefabs.Item_EquipBuff_MagicSource_Soulshard_Dracula, Prefabs.AB_Vampire_BloodKnight_ThousandSpears_DashBuff   },
            { Prefabs.Item_EquipBuff_MagicSource_Soulshard_Manticore, Prefabs.AB_Manticore_Flame_Chaos_Burn_LongDebuff  },
            { Prefabs.Item_EquipBuff_MagicSource_Soulshard_TheMonster, Prefabs.Buff_Monster_FinalStage_Empowered },
            { Prefabs.Item_EquipBuff_MagicSource_Soulshard_Solarus, Prefabs.Buff_Cardinal_Shield_Stack  },
        };

        private static HashSet<PrefabGUID> ShardContainers = new()
        {
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Dracula,
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Manticore,
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Monster,
            Prefabs.TM_Castle_Container_Specialized_Soulshards_Solarus,
        };

        private static Dictionary<PrefabGUID, PrefabGUID> AltarStationsToShardBuffs = new Dictionary<PrefabGUID, PrefabGUID>()
        {
            { Prefabs.TM_CraftingStation_Altar_Frost, Prefabs.AB_Interact_UseRelic_Manticore_Buff },
            { Prefabs.TM_CraftingStation_Altar_Spectral, Prefabs.AB_Interact_UseRelic_Behemoth_Buff },
            { Prefabs.TM_CraftingStation_Altar_Unholy, Prefabs.AB_Interact_UseRelic_Monster_Buff }
        };

        public static void Initialize()
        {
            var action = () => UpdateShards();
            Timers.Add(ActionScheduler.RunActionEveryInterval(action, 3));

            GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
            GameEvents.OnPlayerBuffRemoved += HandleOnPlayerBuffRemoved;
            /*GameEvents.OnPlayerEnteredRegion += HandleOnPlayerEnteredRegion;*/
            GameEvents.OnPlayerTransferredItem += HandleOnPlayerTransferredItem;
            GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
        }


        public static void Dispose()
        {
            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            ExistingShardsToMapIcons.Clear();
            GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
            GameEvents.OnPlayerBuffRemoved -= HandleOnPlayerBuffRemoved;
            /*GameEvents.OnPlayerEnteredRegion -= HandleOnPlayerEnteredRegion;*/
            GameEvents.OnPlayerTransferredItem -= HandleOnPlayerTransferredItem;
            GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
        }

        private static void HandleOnPlayerPlacedStructure(Player player, Entity eventEntity, BuildTileModelEvent buildTileModelEvent)
        {
            if (ShardContainers.Contains(buildTileModelEvent.PrefabGuid))
            {
                eventEntity.Destroy();
                player.ReceiveMessage("You are not allowed to place this structure".Error());
                return;
            }
        }

        public static void HandleOnPlayerEnteredRegion(Player player, Entity eventEntity, CurrentWorldRegionChangedEvent currentWorldRegionChangedEvent)
        {
            if (currentWorldRegionChangedEvent.NewRegion == WorldRegionType.RuinsOfMortium)
            {
                var shard = player.Character.Read<Equipment>().GrimoireSlot.SlotEntity._Entity;
                if (shard.Exists())
                {
                    if (ShardNecklacesToMapIcons.TryGetValue(shard.GetPrefabGUID(), out var shardMapIconPrefab))
                    {
                        AttachMapIconToShard(shard);
                    }
                }
            }
            else if (currentWorldRegionChangedEvent.PreviousRegion == WorldRegionType.RuinsOfMortium)
            {
                var shard = player.Character.Read<Equipment>().GrimoireSlot.SlotEntity._Entity;
                if (shard.Exists())
                {
                    if (ShardNecklacesToMapIcons.ContainsKey(shard.GetPrefabGUID()) && !Helper.IsRaidHour())
                    {
                        var mapIcons = Helper.GetEntitiesByComponentTypes<RelicMapIcon>();
                        foreach (var mapIcon in mapIcons)
                        {
                            var mapIconTargetEntity = mapIcon.Read<MapIconTargetEntity>().TargetEntity._Entity;
                            if (mapIconTargetEntity == shard)
                            {
                                UnattachShardIconFromShard(mapIcon);
                            }
                        }
                    }
                }
            }
        }

        public static void HandleOnPlayerBuffed(Player player, Entity buffEntity, PrefabGUID prefabGUID)
        {
            if (ShardBuffsToVisualBuffs.TryGetValue(prefabGUID, out var visualBuff))
            {                
                if (Helper.BuffPlayer(player, visualBuff, out var buffEntity2, Helper.NO_DURATION))
                {
                    buffEntity2.Remove<GameplayEventListeners>();
                    Helper.ModifyBuff(buffEntity2, BuffModificationTypes.None, true);
                }
            }
        }

        public static void HandleOnPlayerBuffRemoved(Player player, Entity buffEntity, PrefabGUID prefabGUID)
        {
            if (ShardBuffsToVisualBuffs.TryGetValue(prefabGUID, out var visualBuff))
            {
                Helper.RemoveBuff(player, visualBuff);
            }
        }

        public static void UpdateShards()
        {
            var shards = Helper.GetEntitiesByComponentTypes<Relic, LoseDurabilityOverTime>(EntityQueryOptions.IncludeDisabledEntities);
            var shardMapIcons = Helper.GetEntitiesByComponentTypes<RelicMapIcon>();
            ExistingShardsToMapIcons.Clear();
            foreach (var shardMapIcon in shardMapIcons)
            {
                if (shardMapIcon.Has<SpellTarget>())
                {
                    var shardEntity = shardMapIcon.Read<SpellTarget>().Target._Entity;
                    if (!shardEntity.Exists())
                    {
                        Helper.DestroyEntity(shardMapIcon);
                        ExistingShardsToMapIcons.Remove(shardEntity);
                    }
                    else
                    {
                        ExistingShardsToMapIcons[shardEntity] = shardMapIcon;
                    }
                }
                else
                {
                    var mapIconTargetEntity = shardMapIcon.Read<MapIconTargetEntity>().TargetEntity;
                    shardMapIcon.Add<SpellTarget>();
                    shardMapIcon.Write(new SpellTarget
                    {
                        Target = mapIconTargetEntity
                    });
                    ExistingShardsToMapIcons[mapIconTargetEntity._Entity] = shardMapIcon;
                }
            }
            var defaultTime = RaidConfigConfig.Config.NaturalShardDecayInHours * 60 * 60;
            foreach (var shardEntity in shards)
            {
                var shardOwner = shardEntity.Read<Equippable>().EquipTarget._Entity;
                bool shouldShowMapIcon = false;
                if (shardOwner.Exists() && shardOwner.Has<PlayerCharacter>())
                {
                    if (shardOwner.Has<PlayerCharacter>())
                    {
                        var player = PlayerService.GetPlayerFromCharacter(shardOwner);

                        if (player.IsInBase(out var territory, out var territoryAlignment) && territoryAlignment == TerritoryAlignment.Friendly)
                        {
                            shouldShowMapIcon = true;

                            shardEntity.Write(new LoseDurabilityOverTime
                            {
                                TimeUntilBroken = (defaultTime / RaidConfigConfig.Config.StoredShardDecayRate)
                            });
                        }
                        else
                        {
                            if (player.WorldZone == WorldRegionType.RuinsOfMortium)
                            {
                                shouldShowMapIcon = true;
                            }
                            else
                            {
                                shouldShowMapIcon = false;
                            }

                            shardEntity.Write(new LoseDurabilityOverTime
                            {
                                TimeUntilBroken = defaultTime
                            });
                        }
                    }
                }
                else
                {
                    shardEntity.Write(new LoseDurabilityOverTime
                    {
                        TimeUntilBroken = (defaultTime / RaidConfigConfig.Config.StoredShardDecayRate)
                    });
                    shouldShowMapIcon = true;
                }
                if (Helper.IsRaidHour())
                {
                    shouldShowMapIcon = true;
                }

                if (!ExistingShardsToMapIcons.TryGetValue(shardEntity, out var mapIconEntity) || !mapIconEntity.Exists())
                {
                    if (shouldShowMapIcon)
                    {
                        Plugin.PluginLog.LogInfo($"Attempting to create or attach map icon {ExistingShardsToMapIcons.Count}");
                        AttachMapIconToShard(shardEntity);
                    }
                }
                else
                {
                    if (!shouldShowMapIcon)
                    {
                        UnattachShardIconFromShard(mapIconEntity);
                    }
                    else
                    {
                        AttachMapIconToShard(shardEntity);
                    }
                }
            }
        }

        private static void AttachMapIconToShard(Entity shard)
        {
            if (!ExistingShardsToMapIcons.TryGetValue(shard, out var mapIconEntity) || !mapIconEntity.Exists())
            {
                if (ShardNecklacesToMapIcons.TryGetValue(shard.GetPrefabGUID(), out var shardMapIconPrefab))
                {
                    Helper.CreateAndAttachMapIconToEntity(shard, shardMapIconPrefab, (e) =>
                    {
                        e.Add<SpellTarget>();
                        e.Write(new SpellTarget
                        {
                            Target = NetworkedEntity.ServerEntity(shard)
                        });
                        UpdateMapIconTimeRemaining(shard, e);
                        ExistingShardsToMapIcons[shard] = e;
                    });
                }
            }
            else
            {
                mapIconEntity.Add<MapIconTargetEntity>();
                var mapIconTargetEntity = mapIconEntity.Read<MapIconTargetEntity>();
                mapIconTargetEntity.TargetEntity = NetworkedEntity.ServerEntity(shard);
                mapIconTargetEntity.TargetNetworkId = shard.Read<NetworkId>();
                mapIconEntity.Write(mapIconTargetEntity);
                UpdateMapIconTimeRemaining(shard, mapIconEntity);
            }
        }

        private static void UpdateMapIconTimeRemaining(Entity shard, Entity mapIconEntity)
        {
            var durability = shard.Read<Durability>();
            var timeUntilDestroy = (durability.Value / durability.MaxDurability) * shard.Read<LoseDurabilityOverTime>().TimeUntilBroken;
            mapIconEntity.Write(new RelicMapIcon
            {
                TimeUntilDestroy = timeUntilDestroy
            });
        }

        private static void UnattachShardIconFromShard(Entity mapIconEntity)
        {
            mapIconEntity.Write(new MapIconTargetEntity());
            mapIconEntity.Write(new MapIconPosition());
            var shard = mapIconEntity.Read<SpellTarget>().Target._Entity;
            UpdateMapIconTimeRemaining(shard, mapIconEntity);
        }

        private static void HandleOnPlayerTransferredItem(Player player, Entity eventEntity, MoveItemBetweenInventoriesEvent moveItemBetweenInventoriesEvent)
        {
            if (Helper.TryGetEntityFromNetworkId(moveItemBetweenInventoriesEvent.ToInventory, out var targetInventory)) 
            {
                if (AltarStationsToShardBuffs.TryGetValue(targetInventory.GetPrefabGUID(), out var buffGuid))
                {
                    if (Helper.TryGetEntityFromNetworkId(moveItemBetweenInventoriesEvent.FromInventory, out var sourceInventory))
                    {
                        if (Helper.TryGetItemAtSlot(sourceInventory, moveItemBetweenInventoriesEvent.FromSlot, out var item))
                        {
                            if (item.ItemType == Prefabs.Item_Ingredient_DemonFragment)
                            {
                                foreach (var buff in AltarStationsToShardBuffs.Values)
                                {
                                    if (Helper.TryGetBuff(player, buff, out var buffEntity) && buffEntity.Has<CanFly>())
                                    {
                                        Helper.RemoveBuff(player, buff);
                                    }
                                }
                                Helper.ClearInventorySlot(sourceInventory, moveItemBetweenInventoriesEvent.FromSlot);
                                var action = () =>
                                {
                                    if (Helper.BuffPlayer(player, buffGuid, out var buffEntity))
                                    {
                                        buffEntity.Add<CanFly>();
                                    }
                                };
                                var timer = ActionScheduler.RunActionOnceAfterFrames(action, 2);
                            }
                            else
                            {
                                player.ReceiveMessage("You can only place demon fragments into this altar".Error());
                                eventEntity.Destroy();
                            }
                        }
                    }
                }
            }
        }
    }
}
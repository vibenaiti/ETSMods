using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModCore.Events;
using ModCore.Models;
using Unity.Entities;
using ModCore.Data;
using ModCore.Helpers;
using Stunlock.Core;
using ProjectM.CastleBuilding;
using ModCore;
using ModCore.Services;
using ProjectM.Gameplay.Systems;
using Unity.Mathematics;
using Unity.Collections;
using ProjectM.Network;
using Unity.Transforms;
using ProjectM.Terrain;
using static ModCore.Frameworks.CommandFramework.CommandFramework;

namespace ChipSaMod.Managers
{
    public static class ChipSaManager
    {
        private static Dictionary<PrefabGUID, PrefabGUID> TileModelToItem = new()
        {
            { Prefabs.TM_Siege_Structure_T02, Prefabs.Item_Building_Siege_Golem_T02 },
            { Prefabs.TM_EH_Mines_ExplosiveBarrel_Placeable_T01, Prefabs.Item_Building_Explosives_T01 },
            { Prefabs.TM_EH_Mines_ExplosiveBarrel_Placeable_T02, Prefabs.Item_Building_Explosives_T02 },
        };

        public static void Initialize()
        {
            GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
            GameEvents.OnPlayerFirstSpawn += HandleOnPlayerFirstSpawn;
        }

        public static void Dispose()
        {
            GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
            GameEvents.OnPlayerFirstSpawn -= HandleOnPlayerFirstSpawn;
        }

        public static void HandleOnPlayerFirstSpawn(Player player)
        {
            Helper.UnlockAllAbilities(player);
            Helper.UnlockAllContent(player.ToFromCharacter());

            Helper.AddItemToInventory(player, Prefabs.Item_NewBag_T06, 1, out var bagEntity);

            var action = () =>
            {
                foreach (var item in Kits.ArtifactWeapons)
                {
                    Helper.AddItemToInventory(player, item, 1, out var itemEntity);
                }

                foreach (var item in Kits.StartingGear)
                {
                    Helper.AddItemToInventory(player, item, 1, out var itemEntity);
                }

                foreach (var item in Kits.Necks)
                {
                    Helper.AddItemToInventory(player, item, 1, out var itemEntity);
                }

                Helper.AddItemToInventory(player, Prefabs.Item_Consumable_HealingPotion_T01, 1, out var potionEntity);
                Helper.AddItemToInventory(player, Prefabs.Item_Consumable_HealingPotion_T02, 1, out potionEntity);

                Helper.SetPlayerBlood(player, Prefabs.BloodType_Rogue);
                ApplyBuffsToPlayer(player);
                player.Teleport(new float3(-1408, 5.31f, -1285.5f));
            };

            ActionScheduler.RunActionOnceAfterFrames(action, 3);
        }

        private static void ApplyBuffsToPlayer(Player player)
        {
            if (!Helper.HasBuff(player, Prefabs.AB_Consumable_PhysicalPowerPotion_T02_Buff))
            {
                Helper.BuffPlayer(player, Prefabs.AB_Consumable_PhysicalPowerPotion_T02_Buff, out var buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(player, Prefabs.AB_Consumable_SpellPowerPotion_T02_Buff, out buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(player, Prefabs.AB_Consumable_SpellLeechPotion_T01_Buff, out buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(player, Prefabs.AB_Consumable_FireResistancePotion_T01_Buff, out buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(player, Prefabs.AB_Consumable_HolyResistancePotion_T02_Buff, out buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(player, Prefabs.AB_Consumable_WranglerPotion_T01_Buff, out buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(player, Prefabs.AB_Consumable_SunResistancePotion_T01_Buff, out buffEntity, Helper.NO_DURATION, true);
            }
            else
            {
                Helper.RemoveBuff(player, Prefabs.AB_Consumable_PhysicalPowerPotion_T02_Buff);
                Helper.RemoveBuff(player, Prefabs.AB_Consumable_SpellPowerPotion_T02_Buff);
                Helper.RemoveBuff(player, Prefabs.AB_Consumable_SpellLeechPotion_T01_Buff);
                Helper.RemoveBuff(player, Prefabs.AB_Consumable_FireResistancePotion_T01_Buff);
                Helper.RemoveBuff(player, Prefabs.AB_Consumable_HolyResistancePotion_T02_Buff);
                Helper.RemoveBuff(player, Prefabs.AB_Consumable_WranglerPotion_T01_Buff);
                Helper.RemoveBuff(player, Prefabs.AB_Consumable_SunResistancePotion_T01_Buff);
            }
        }

        public static void HandleOnPlayerPlacedStructure(Player player, Entity eventEntity, BuildTileModelEvent buildTileModelEvent)
        {
            if (TileModelToItem.TryGetValue(buildTileModelEvent.PrefabGuid, out var item)) 
            {
                Helper.RemoveItemFromInventory(player, item, 1);
            }
            else if (buildTileModelEvent.PrefabGuid == Prefabs.TM_BloodFountain_CastleHeart)
            {
                var action = () =>
                {
                    if (Helper.TryGetCurrentCastleTerritory(player, out var territoryEntity))
                    {
                        var territory = territoryEntity.Read<CastleTerritory>();
                        var heart = territory.CastleHeart;
                        if (heart.Exists())
                        {
                            var sys = VWorld.Server.GetExistingSystemManaged<CastleHeartEventSystem>();
                            var fromCharacter = player.ToFromCharacter();
                            var castleHeart = heart.Read<CastleHeart>();
                            for (var i = 0; i < 4; i++)
                            {
                                sys.UpgradeCastleHeart(heart, ref fromCharacter, castleHeart);
                                castleHeart = heart.Read<CastleHeart>();
                            }
                            if (heart.TryGetInventoryBuffer(out var buffer))
                            {
                                for (var i = 0; i < buffer.Length; i++)
                                {
                                    var inventory = buffer[i];
                                    inventory.ItemType = Prefabs.Item_BloodEssence_T01;
                                    inventory.Amount = 500;
                                    inventory.MaxAmountOverride = 500;
                                    buffer[i] = inventory;
                                }
                            }

                            var heartHeight = heart.Read<TilePosition>().HeightLevel;
                            if ((heartHeight == territory.MinHeightLevel) || ((heartHeight - 1) == territory.MinHeightLevel))
                            {
                                var blockBuffer = territoryEntity.ReadBuffer<CastleTerritoryBlocks>();
                                var heartPosition = buildTileModelEvent.SpawnTranslation.Value;
                                heartPosition.y = heart.Read<Translation>().Value.y + 0.1f;

                                Queue<float3> toPlace = new Queue<float3>();
                                HashSet<float3> placedPositions = new HashSet<float3>();

                                toPlace.Enqueue(heartPosition);
                                placedPositions.Add(heartPosition);

                                var directions = new float3[]
                                {
                                new float3(5, 0, 0),
                                new float3(-5, 0, 0),
                                new float3(0, 0, 5),
                                new float3(0, 0, -5),
                                };


                                int placedCount = 0;

                                while (toPlace.Count > 0 && placedCount < blockBuffer.Length * 2)
                                {
                                    var position = toPlace.Dequeue();
                                    if (placedCount < blockBuffer.Length * 5)
                                    {
                                        var action = () =>
                                        {
                                            var buildEventEntity = Helper.CreateEntityWithComponents<FromCharacter, BuildTileModelEvent>();
                                            buildEventEntity.Write(player.ToFromCharacter());
                                            buildEventEntity.Write(new BuildTileModelEvent
                                            {
                                                PrefabGuid = Prefabs.TM_Castle_Floor_Foundation_Stone04,
                                                SpawnTranslation = new Translation
                                                {
                                                    Value = position
                                                }
                                            });
                                        };
                                        ActionScheduler.RunActionOnceAfterFrames(action, placedCount + 1);


                                        placedCount++;

                                        foreach (var direction in directions)
                                        {
                                            var newPosition = position + direction;

                                            if (!placedPositions.Contains(newPosition))
                                            {
                                                toPlace.Enqueue(newPosition);
                                                placedPositions.Add(newPosition);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
                ActionScheduler.RunActionOnceAfterFrames(action, 3);
            }
        }
    }
}

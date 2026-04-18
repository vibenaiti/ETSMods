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
using ModCore.Models;
using System.Linq;
using OpenWorldEvents.GameModes;
using ModCore.Events;
using ProjectM.Gameplay.Systems;
using Unity.Transforms;
using Unity.Physics;
using ProjectM.Sequencer;
using static ModCore.Helpers.Helper;
using OpenWorldEvents.Models;
using ProjectM.CastleBuilding;
using ProjectM.Tiles;
using Unity.Collections;
using Stunlock.Core;
using ProjectM.Hybrid.ArmorTest;
using ProjectM.Behaviours;
using System.Runtime.CompilerServices;
using ProjectM.Shared;

namespace OpenWorldEvents.Managers
{
    public static class HungerGamesManager
    {
        private static List<Timer> Timers = new();
        public static Dictionary<Player, float3> PlayersToOriginalLocation = new();
        public static HashSet<Player> LivingPlayers = new();
        public static RectangleZone EntireArea = null;
        public static RectangleZone WaitingRoomArea = null;
        public static RectangleZone FinalSafeZone = null;
        public static System.Random Random = new();
        public static CumulativeLootTable Tier1LootTable;
        public static CumulativeLootTable Tier2LootTable;
        public static CumulativeLootTable Tier3LootTable;
        public static CumulativeLootTable Tier4LootTable;
        public static CumulativeLootTable Tier5LootTable;
        public static Dictionary<Player, Equipment> PlayerToEquipment = new();
        public static Dictionary<Player, List<InventoryBuffer>> PlayerToInventory = new();
        private static HashSet<Entity> SpawnedEntities = new();
        public static bool HasInitialized = false;
        private static int StormHeight = 0;
        private static float PoisonDamage = 0.02f;

        public static void Initialize()
        {
            EntireArea = HungerGamesConfig.Config.EntireArenaZone.ToRectangleZone();
            WaitingRoomArea = HungerGamesConfig.Config.WaitingRoomArea.ToRectangleZone();
            FinalSafeZone = HungerGamesConfig.Config.FinalSafeZone.ToRectangleZone();
            StormHeight = WaitingRoomArea.Height;
            Tier1LootTable = new(HungerGamesConfig.Config.Tier1Drops.LootDrops);
            Tier2LootTable = new(HungerGamesConfig.Config.Tier2Drops.LootDrops);
            Tier3LootTable = new(HungerGamesConfig.Config.Tier3Drops.LootDrops);
            Tier4LootTable = new(HungerGamesConfig.Config.Tier4Drops.LootDrops);
            Tier5LootTable = new(HungerGamesConfig.Config.Tier5Drops.LootDrops);
            GameEvents.OnPlayerSignedUp += HandleOnPlayerSignedUp;
            GameEvents.OnPlayerUnstuck += HandleOnPlayerUnstuck;
            GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
            GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
            GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
            HasInitialized = true;
            
            var startMatchAction = () => 
            {
                GameEvents.OnPlayerUnstuck -= HandleOnPlayerUnstuck;
                GameEvents.OnPlayerSignedUp -= HandleOnPlayerSignedUp;
                Helper.SendSystemMessageToAllClients($"Sign-ups for {"Embrace the Hunger".Colorify(ExtendedColor.Red)} are now closed. Thank you to all our brave tributes. May the odds be ever in your favor!");
                foreach (var player in PlayersToOriginalLocation.Keys)
                {
                    player.ReceiveMessage("The match will begin in 10 seconds");
                }

                var action = () => StartMatch();
                Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, 10));
            };
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(startMatchAction, HungerGamesConfig.Config.SignupTimeSeconds));

            Helper.SendSystemMessageToAllClients($"{"Embrace the Hunger".Colorify(ExtendedColor.Red)} is seeking brave tributes! Type {".tribute".Emphasize()} to volunteer and join the competition. May the odds be ever in your favor!");
        }

        public static void Dispose(bool hard = true)
        {            
            GameEvents.OnPlayerSignedUp -= HandleOnPlayerSignedUp;
            GameEvents.OnPlayerUnstuck -= HandleOnPlayerUnstuck;
            GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
            GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
            GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();
            EntireArea = HungerGamesConfig.Config.EntireArenaZone.ToRectangleZone();
            CleanUp();

            if (hard)
            {
                foreach (var player in LivingPlayers)
                {
                    Helper.Reset(player);
                    if (PlayersToOriginalLocation.ContainsKey(player))
                    {
                        player.Teleport(PlayersToOriginalLocation[player]);
                        ModifyPlayerInteractSpeed(player, 4);
                    }
                    ReturnPlayerLoot(player);
                }
                PlayersToOriginalLocation.Clear();
                LivingPlayers.Clear();
                ModCore.Globals.HungerGamesPlayers = LivingPlayers;
            }
            else
            {
                PlayersToOriginalLocation.Clear();
                LivingPlayers.Clear();
                ModCore.Globals.HungerGamesPlayers = LivingPlayers;
            }
            HasInitialized = false;

            var entities = Helper.GetEntitiesInArea(EntireArea.ToBoundsMinMax(), TileType.All);
            foreach (var entity in entities)
            {
                if (entity.Has<Door>())
                {
                    var door = entity.Read<Door>();
                    door.OpenState = false;
                    entity.Write(door);
                }
            }

            EntireArea = null;
            WaitingRoomArea = null;
            FinalSafeZone = null;
            StormHeight = 0;
            PoisonDamage = 0.02f;
        }

        public static void CleanUp()
        {
            var entities = Helper.GetEntitiesInArea(EntireArea.ToBoundsMinMax(), TileType.All);
            foreach (var entity in entities)
            {
                if (entity.Has<InventoryInstanceElement>() && !entity.Has<PlayerCharacter>() && !entity.Has<CastleHeart>())
                {
                    Helper.ClearEntityInventory(entity);
                }
            }

            foreach (var entity in SpawnedEntities)
            {
                Helper.DestroyEntity(entity);
            }

            var droppedItems = Helper.GetEntitiesByComponentTypes<ItemPickup>(EntityQueryOptions.IncludeDisabledEntities);
            foreach (var item in droppedItems)
            {
                if (EntireArea.Contains(item, false))
                {
                    Helper.DestroyEntity(item);
                }
            }

            var deathContainers = Helper.GetEntitiesByComponentTypes<PlayerDeathContainer>(EntityQueryOptions.IncludeDisabledEntities);
            foreach (var item in deathContainers)
            {
                if (EntireArea.Contains(item, false))
                {
                    Helper.DestroyEntity(item);
                }
            }
        }

        public static void HandleOnPlayerDowned(Player player, Entity killer)
        {
            if (Helper.HasBuff(player, Prefabs.AB_Mutant_Spitter_PoisonRain_PoisonDebuff))
            {
                Helper.KillOrDestroyEntity(player.Character);
            }
        }

        public static void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
        {
            if (LivingPlayers.Contains(player))
            {
                player.Reset(new ResetOptions
                {
                    RemoveConsumables = true
                });
                PrefabSpawnerService.SpawnWithCallback(Prefabs.Resource_PlayerDeathContainer_Drop, player.Position, (container) =>
                {
                    if (container.TryGetInventoryBuffer(out var inventoryBuffer))
                    {
                        var playerBuffer = player.Inventory.ReadBuffer<InventoryBuffer>();
                        var j = 0;
                        for (var i = 0; i < playerBuffer.Length; i++)
                        {
                            if (playerBuffer[i].ItemType == PrefabGUID.Empty) continue;

                            if (j >= inventoryBuffer.Length)
                            {
                                // Expand inventoryBuffer to accommodate more items
                                inventoryBuffer.Add(new InventoryBuffer());
                            }

                            inventoryBuffer[j] = playerBuffer[i];
                            playerBuffer[i] = new InventoryBuffer();
                            j++;
                        }
                        var equipment = player.Equipment;
                        var results = new NativeList<Entity>(Allocator.Temp);
                        equipment.GetAllEquipmentEntities(results, true);
                        foreach (var item in results)
                        {
                            if (item.Read<EquippableData>().EquipmentType == ProjectM.EquipmentType.Weapon) continue;

                            bool addedToEmptySlot = false;
                            for (var i = 0; i < inventoryBuffer.Length; i++)
                            {
                                var slot = inventoryBuffer[i];
                                if (slot.ItemType == PrefabGUID.Empty)
                                {
                                    slot = new InventoryBuffer
                                    {
                                        ItemEntity = item,
                                        Amount = 1,
                                        ItemType = item.GetPrefabGUID(),
                                    };
                                    inventoryBuffer[i] = slot;
                                    addedToEmptySlot = true;
                                    break;
                                }
                            }
                            if (!addedToEmptySlot)
                            {
                                inventoryBuffer.Add(new InventoryBuffer
                                {
                                    ItemEntity = item,
                                    Amount = 1,
                                    ItemType = item.GetPrefabGUID(),
                                });
                            }
                        }
                        if (InventoryUtilities.IsInventoryEmpty(VWorld.Server.EntityManager, container))
                        {
                            Helper.DestroyEntity(container);
                        }
                    }
                });


                LivingPlayers.Remove(player);
                ModCore.Globals.HungerGamesPlayers = LivingPlayers;
                
                if ((LivingPlayers.Count / PlayersToOriginalLocation.Count) <= HungerGamesConfig.Config.PercentPlayersAliveBeforeOpeningLevel2Doors)
                {
                    OpenDoors(2);
                }
                else if ((LivingPlayers.Count / PlayersToOriginalLocation.Count) <= HungerGamesConfig.Config.PercentPlayersAliveBeforeOpeningLevel3Doors)
                {
                    OpenDoors(3);
                }
                else if ((LivingPlayers.Count / PlayersToOriginalLocation.Count) <= HungerGamesConfig.Config.PercentPlayersAliveBeforeOpeningLevel4Doors)
                {
                    OpenDoors(4);
                }

                var action2 = () => ReturnPlayerLoot(player);
                ActionScheduler.RunActionOnceAfterFrames(action2, 3);

                ModifyPlayerInteractSpeed(player, 2);
            }
            if (LivingPlayers.Count <= 4)
            {
                HashSet<Entity> clans = new();

                // Collect unique clans of living players
                foreach (var livingPlayer in LivingPlayers)
                {
                    clans.Add(livingPlayer.Clan);
                }

                // Check if all players are from the same clan or all are clanless with only one player left
                bool matchOver = clans.Count == 1 && (clans.Contains(Entity.Null) ? LivingPlayers.Count == 1 : true);

                if (matchOver)
                {
                    EndMatch(LivingPlayers.ToList());
                }
            }

        }

        public static void HandleOnPlayerBuffed(Player player, Entity buffEntity, PrefabGUID prefabGUID)
        {
            if (prefabGUID == Prefabs.AB_Interact_OpenContainer_Hold)
            {
                var entities = Helper.GetEntitiesByComponentTypes<ModifyMovementDuringCastActive>();
                foreach (var entity in entities)
                {
                    var modifyMovementDuringCastActive = entity.Read<ModifyMovementDuringCastActive>();
                    if (modifyMovementDuringCastActive.Character == player.Character)
                    {
                        entity.Destroy();
                        break;
                    }
                }
            }
        }

        public static void HandleOnPlayerUnstuck(Player player, Entity eventEntity)
        {
            if (eventEntity.Exists() && PlayersToOriginalLocation.ContainsKey(player))
            {
                player.ReceiveMessage("You can't unstuck while signed up for hunger games".Error());
                eventEntity.Destroy();
            }
        }

        public static void HandleOnPlayerSignedUp(Player player)
        {
            if (!Helper.IsInBase(player, out var territoryEntity, out var territoryAlignment) || territoryAlignment != Helper.TerritoryAlignment.Friendly)
            {
                player.ReceiveMessage("You must be in the safety of your territory to sign up as a tribute.".Error());
            }
            else if (Helper.HasBuff(player, Prefabs.Buff_InCombat_PvPVampire))
            {
                player.ReceiveMessage("You cannot sign up while in PvP combat!".Error());
            }
            else if (Helper.HasBuff(player, Prefabs.AB_Shapeshift_Bat_TakeFlight_Buff))
            {
                player.ReceiveMessage("You cannot sign up while in bat form!".Error());
            }
            else if (Helper.HasBuff(player, Prefabs.AB_Interact_Throne_Dracula_Buff_Sit))
            {
                player.ReceiveMessage("You cannot sign up while seated on Dracula's throne!".Error());
            }
            else if (!player.IsAlive)
            {
                player.ReceiveMessage("You must be alive to join!".Error());
            }
            else if (PlayersToOriginalLocation.ContainsKey(player))
            {
                player.ReceiveMessage("You have already joined!".Error());
            }
            else
            {
                PlayersToOriginalLocation[player] = player.Position;
                LivingPlayers.Add(player);
                ModCore.Globals.HungerGamesPlayers = LivingPlayers;
                TransferPlayerLootAway(player);
                player.Teleport(HungerGamesConfig.Config.WaitingRoomTeleportCoordinates.ToFloat3());
                /*var action = () =>
                {
                    Helper.PlaySequenceOnPosition(player.Position, Sequences.AB_Militia_Glassblower_GlassRain_Trigger);
                };
                Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, 1.0f));*/
                player.Reset(new ResetOptions()
                {
                    RemoveBuffs = true,
                    RemoveConsumables = true,
                    RemoveShapeshifts = true,
                });
                Helper.SetPlayerBlood(player, Prefabs.BloodType_None, 0);
                if (Helper.BuffPlayer(player, Helper.CustomBuff1, out var buffEntity, Helper.NO_DURATION))
                {
                    Helper.ModifyBuff(buffEntity, BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.ImmuneToSun);
                }
            }
        }

        public static void ModifyPlayerInteractSpeed(Player player, int seconds)
        {
            return;
            var buffer = player.Character.ReadBuffer<AbilityGroupSlotBuffer>();
            foreach (var item in buffer)
            {
                var abilityGroupEntity = item.GroupSlotEntity._Entity.Read<AbilityGroupSlot>().StateEntity._Entity;
                if (abilityGroupEntity.GetPrefabGUID() == Prefabs.AB_Interact_OpenContainer_Hold_AbilityGroup)
                {
                    abilityGroupEntity.LogComponentTypes();

                    var buffer2 = item.GroupSlotEntity._Entity.Read<AbilityGroupSlot>().StateEntity._Entity.ReadBuffer<AbilityStateBuffer>();
                    foreach (var abilityState in buffer2)
                    {
                        var castEntity = abilityState.StateEntity._Entity;
                        castEntity.LogPrefabName();
                        if (castEntity.GetPrefabGUID() == Prefabs.AB_Interact_OpenContainer_Hold_Cast)
                        {
                            var abilityCastTimeData = castEntity.Read<AbilityCastTimeData>();
                            abilityCastTimeData.MaxCastTime._Value = seconds;
                            Plugin.PluginLog.LogInfo(abilityCastTimeData.PostCastTime);
                            castEntity.Write(abilityCastTimeData);
                        }
                    }
                }
            }
        }

        public static void StartMatch()
        {
            DistributeRandomLootToChests();
            SpawnUnits();
            PrepareStorm();

            foreach (var player in PlayersToOriginalLocation.Keys)
            {
                Helper.RemoveBuff(player, Helper.CustomBuff1);
                Helper.BuffPlayer(player, Prefabs.Buff_General_PvPProtected, out var buffEntity, 5);
                ModifyPlayerInteractSpeed(player, 2);
            }

            var entities = Helper.GetEntitiesInArea(EntireArea.ToBoundsMinMax(), TileType.All);
            foreach (var entity in entities)
            {
                if (entity.Has<CastleHeartConnection>() && entity.Has<Health>() && entity.Has<EntityCategory>())
                {
                    if (Helper.BuffEntity(entity, Helper.CustomBuff1, out var buffEntity, Helper.NO_DURATION))
                    {
                        Helper.ModifyBuff(buffEntity, BuffModificationTypes.Invulnerable);
                    }
                }
                if (entity.Has<Door>())
                {
                    if (entity.Has<TilePosition>())
                    {
                        var tilePosition = entity.Read<TilePosition>();
                        if (tilePosition.HeightLevel == EntireArea.Height)
                        {
                            var door = entity.Read<Door>();
                            door.OpenState = true;
                            entity.Write(door);
                        }
                    }
                }
            }
            //open doors
        }

        public static void EndMatch(List<Player> winners)
        {
            List<string> winningMembers = new List<string>();
            var rewards = HungerGamesConfig.Config.WinnerRewards;
            if (winners.Count > 0)
            {
                var action = () =>
                {
                    foreach (var winner in winners)
                    {
                        if (PlayersToOriginalLocation.TryGetValue(winner, out var location))
                        {
                            winner.Reset();
                            winner.Teleport(location);
                            ReturnPlayerLoot(winner);
                            winningMembers.Add(winner.ToString().Colorify(ExtendedColor.ClanNameColor));
                            ModifyPlayerInteractSpeed(winner, 4);
                            var awardAction = () =>
                            {
                                foreach (var item in rewards)
                                {
                                    winner.ReceiveMessage($"You have been awarded {item.Quantity.ToString().Emphasize()} {Helper.GetItemName(item.ItemPrefabGUID).Emphasize()}(s) for winning!".Success());
                                    Helper.AddItemToInventory(winner, item.ItemPrefabGUID, item.Quantity, out var itemEntity);
                                }
                            };
                            ActionScheduler.RunActionOnceAfterFrames(awardAction, 6);
                        }
                        PlayersToOriginalLocation.Remove(winner);
                    }
                    string winningMembersMessage = string.Join(", ", winningMembers);
                    if (winningMembers.Count == 1)
                    {
                        Helper.SendSystemMessageToAllClients($"{winningMembersMessage} has won {"Embrace the Hunger".Colorify(ExtendedColor.Red)}!");
                    }
                    else
                    {
                        Helper.SendSystemMessageToAllClients($"{winningMembersMessage} have won {"Embrace the Hunger".Colorify(ExtendedColor.Red)}!");
                    }
                    Dispose(false);
                };
                ActionScheduler.RunActionOnceAfterDelay(action, 5);
            }
            else
            {
                Helper.SendSystemMessageToAllClients($"For mysterious reasons, {"Embrace the Hunger".Colorify(ExtendedColor.Red)} has ended with no winner!");
                Dispose();
            }
        }

        private static void StormUpdate()
        {
            foreach (var player in LivingPlayers)
            {
                bool appliedToxic = false;
                if (player.Height <= StormHeight && !FinalSafeZone.Contains(player) && !player.HasBuff(Prefabs.Admin_Observe_Invisible_Buff) && !player.HasBuff(Prefabs.Admin_Observe_Ghost_Buff))
                {
                    if (Helper.BuffPlayer(player, Prefabs.AB_Mutant_Spitter_PoisonRain_PoisonDebuff, out var buffEntity, Helper.NO_DURATION))
                    {
                        buffEntity.Remove<ModifyMovementSpeedBuff>();

                        var buffer = buffEntity.ReadBuffer<DealDamageOnGameplayEvent>();
                        for (var i = 0; i < buffer.Length; i++)
                        {
                            var dealDamageOnGameplayEvent = buffer[i];
                            dealDamageOnGameplayEvent.Parameters.RawDamagePercent = PoisonDamage;
                            dealDamageOnGameplayEvent.Parameters.DealDamageFlags &= (int)~DealDamageFlag.IsDoT;
                            buffer[i] = dealDamageOnGameplayEvent;
                        }
                        buffEntity.Add<Hideable>();
                        appliedToxic = true;
                    }
                }
                if (!appliedToxic)
                {
                    if (Helper.TryGetBuff(player, Prefabs.AB_Mutant_Spitter_PoisonRain_PoisonDebuff, out var buffEntity) && buffEntity.Has<Hideable>())
                    {
                        Helper.DestroyBuff(buffEntity);
                    }
                }
            }
        }

        private static void PrepareStorm()
        {
            var action = () => StormLevel1();
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, HungerGamesConfig.Config.Level1SecondsBeforePoison - 10));

            /*var increasePoisonDamageAction = () =>
            {
                PoisonDamage += 0.01f;
            };
            Timers.Add(ActionScheduler.RunActionEveryInterval(increasePoisonDamageAction, 60));*/
        }

        private static void RaiseStormIn30Seconds()
        {
            var increaseStormLevelAction = () =>
            {
                StormHeight++;
            };
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(increaseStormLevelAction, 30));
        }

        private static void StormLevel1()
        {
            StormHeight = WaitingRoomArea.Height;

            OpenDoors(2);

            foreach (var player in LivingPlayers)
            {
                player.ReceiveMessage("The 1st floor will be covered in poison in 30 seconds.");
            }

            var action = () =>
            {
                var poisonPlayersInStormAction = () => StormUpdate();
                Timers.Add(ActionScheduler.RunActionEveryInterval(poisonPlayersInStormAction, 1));

                var action = () => StormLevel2();
                Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, HungerGamesConfig.Config.Level2SecondsBeforePoison - 30));
            };
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, 10));
        }

        private static void StormLevel2()
        {
            OpenDoors(3);
            foreach (var player in LivingPlayers)
            {
                player.ReceiveMessage("The 2nd floor will be covered in poison in 30 seconds.");
            }

            RaiseStormIn30Seconds();
            var action = () => StormLevel3();
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, HungerGamesConfig.Config.Level3SecondsBeforePoison - 30));
        }

        private static void StormLevel3()
        {
            OpenDoors(4);
            foreach (var player in LivingPlayers)
            {
                player.ReceiveMessage("The 3rd floor will be covered in poison in 30 seconds.");
            }

            RaiseStormIn30Seconds();

            var action = () => StormLevel4();
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, HungerGamesConfig.Config.Level4SecondsBeforePoison - 30));
        }

        private static void StormLevel4()
        {
            foreach (var player in LivingPlayers)
            {
                player.ReceiveMessage("The 4th floor will be covered in poison in 30 seconds. Only the center is safe!");
            }
            RaiseStormIn30Seconds();
        }

        public static void SpawnUnits()
        {
            foreach (var spawnPoint in HungerGamesConfig.Config.UnitSpawnPoints)
            {
                var randomIndex = Random.Next(spawnPoint.SpawnableUnits.Count);
                for (var i = 0; i < spawnPoint.Quantity; i++) 
                {
                    var unit = new Unit(spawnPoint.SpawnableUnits[randomIndex]);
                    unit.Level = spawnPoint.Level;
                    unit.Category = "hungergames";
                    UnitFactory.SpawnUnitWithCallback(unit, spawnPoint.SpawnPoint.ToFloat3(), (e) =>
                    {
                        /*e.Write(new FactionReference
                        {
                            FactionGuid = new ModifiablePrefabGUID(Prefabs.Faction_VampireHunters)
                        });*/
                        SpawnedEntities.Add(e);
                    });
                }
            }
        }

        public static void TransferPlayerLootAway(Player player)
        {
            PlayerToEquipment[player] = player.Equipment;
            var originalInventoryBuffer = player.Inventory.ReadBuffer<InventoryBuffer>();
            var storedBuffer = new List<InventoryBuffer>();
            for (var i = 0; i < originalInventoryBuffer.Length; i++)
            {
                var inventoryBuffer = originalInventoryBuffer[i];
                storedBuffer.Add(inventoryBuffer);
                originalInventoryBuffer[i] = new InventoryBuffer();
            }
            PlayerToInventory[player] = storedBuffer;

            var equipment = player.Equipment;
            var results = player.EquipmentEntities;
            foreach (var itemEntity in results)
            {
                if (!itemEntity.Exists()) continue;

                var equipmentType = itemEntity.Read<EquippableData>().EquipmentType;
                //equipment.UnequipItem(VWorld.Server.EntityManager, player.Character, equipmentType);
                var eventEntity = Helper.CreateEntityWithComponents<EquipmentChangedEvent>();
                eventEntity.Write(new EquipmentChangedEvent
                {
                    EquipmentType = equipmentType,
                    ItemEntity = itemEntity,
                    ChangeType = EquipmentChangedEventType.Unequipped,
                    Item = itemEntity.GetPrefabGUID(),
                    Target = player.Character
                });
            }
            player.Character.Write(new Equipment());
            Helper.AddItemToInventory(player, Prefabs.Item_Weapon_Slashers_T01_Bone, 1, out var slasher);
        }

        public static void ReturnPlayerLoot(Player player)
        {
            if (PlayerToEquipment.TryGetValue(player, out var equipment))
            {
                var results = new NativeList<Entity>(Allocator.Temp);
                equipment.GetAllEquipmentEntities(results, true);
                player.Character.Write(equipment);
                foreach (var itemEntity in results)
                {
                    if (!itemEntity.Exists()) continue;
                    var eventEntity = Helper.CreateEntityWithComponents<EquipmentChangedEvent>();
                    eventEntity.Write(new EquipmentChangedEvent
                    {
                        EquipmentType = itemEntity.Read<EquippableData>().EquipmentType,
                        ItemEntity = itemEntity,
                        ChangeType = EquipmentChangedEventType.Equipped,
                        Item = itemEntity.GetPrefabGUID(),
                        Target = player.Character
                    });
                }
                PlayerToEquipment.Remove(player);
            }
            var action = () =>
            {
                if (PlayerToInventory.TryGetValue(player, out var inventoryBuffer))
                {
                    var buffer = player.Inventory.ReadBuffer<InventoryBuffer>();
                    for (var i = 0; i < inventoryBuffer.Count; i++)
                    {
                        buffer[i] = inventoryBuffer[i];
                    }
                    PlayerToInventory.Remove(player);
                }
            };
            ActionScheduler.RunActionOnceAfterFrames(action, 3);
        }

        public static void OpenDoors(int floor)
        {
            if (!HasInitialized) return;

            var height = EntireArea.Height + (floor - 1);
            var entities = Helper.GetEntitiesInArea(EntireArea.ToBoundsMinMax(), TileType.All);
            bool messaged = false;
            foreach (var entity in entities)
            {
                if (entity.Has<Door>())
                {
                    var doorHeight = entity.Read<TilePosition>().HeightLevel;
                    if (doorHeight == height)
                    {
                        var door = entity.Read<Door>();
                        if (!door.OpenState)
                        {
                            if (!messaged && floor != 1)
                            {
                                foreach (var player in LivingPlayers)
                                {
                                    player.ReceiveMessage($"Floor {floor} is now open!");
                                }
                                messaged = true;
                            }
                        }
                        door.OpenState = true;
                        entity.Write(door);
                    }
                }
            }
        }

        public static void CloseDoors(int floor)
        {
            if (!HasInitialized) return;

            var height = EntireArea.Height + (floor - 1);
            var entities = Helper.GetEntitiesInArea(EntireArea.ToBoundsMinMax(), TileType.All);
            foreach (var entity in entities)
            {
                if (entity.Has<Door>())
                {
                    var doorHeight = entity.Read<TilePosition>().HeightLevel;
                    if (doorHeight == height)
                    {
                        var door = entity.Read<Door>();
                        door.OpenState = false;
                        entity.Write(door);
                    }
                }
            }
        }

        public static void DistributeRandomLootToChests()
        {
            var bounds = EntireArea.ToBoundsMinMax();
            var entities = Helper.GetEntitiesInArea(bounds, TileType.All);
            var tilePositionLookup = VWorld.Server.EntityManager.GetComponentLookup<TilePosition>();
            foreach (var entity in entities)
            {
                var inventoryEntity = entity;
                if (entity.Has<InventoryInstanceElement>() && !entity.Has<PlayerCharacter>() && !entity.Has<CastleHeart>())
                {
                    if (!entity.Has<InventoryBuffer>() && entity.Has<InventoryInstanceElement>())
                    {
                        inventoryEntity = entity.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
                    }
                    if (inventoryEntity.Exists())
                    {
                        if (inventoryEntity.Has<RestrictedInventory>())
                        {
                            inventoryEntity.Write(new RestrictedInventory()
                            {
                                RestrictedItemCategory = ItemCategory.ALL
                            });
                        }
                        
                        var buffer = inventoryEntity.ReadBuffer<InventoryBuffer>();
                        if (buffer.Length <= 0) continue;
                    }
                    if (tilePositionLookup.HasComponent(entity))
                    {
                        var tilePosition = tilePositionLookup[entity];
                        int tier = (tilePosition.HeightLevel - EntireArea.Height) + 1;
                        int threshold = (int)(HungerGamesConfig.Config.ChanceForHigherTierLoot * 1000000);
                        int randomNumber = Random.Next(0, 1000000);
                        if (randomNumber < threshold)
                        {
                            tier++;
                        }
                        if (entity.GetPrefabGUID() == Prefabs.TM_Stash_Chest_Wood_General_Halloween01Variant)
                        {
                            tier = 5;
                        }
                        CumulativeLootTable lootTable;
                        int numberOfRolls;
                        if (tier == 1)
                        {
                            lootTable = Tier1LootTable;
                            numberOfRolls = HungerGamesConfig.Config.Tier1Drops.RollsPerChest;
                        }
                        else if (tier == 2)
                        {
                            lootTable = Tier2LootTable;
                            numberOfRolls = HungerGamesConfig.Config.Tier2Drops.RollsPerChest;
                        }
                        else if (tier == 3)
                        {
                            lootTable = Tier3LootTable;
                            numberOfRolls = HungerGamesConfig.Config.Tier3Drops.RollsPerChest;
                        }
                        else if (tier == 4)
                        {
                            lootTable = Tier4LootTable;
                            numberOfRolls = HungerGamesConfig.Config.Tier4Drops.RollsPerChest;
                        }
                        else if (tier == 5)
                        {
                            lootTable = Tier5LootTable;
                            numberOfRolls = HungerGamesConfig.Config.Tier5Drops.RollsPerChest;
                        }
                        else
                        {
                            continue;
                        }

                        var numberOfItemsToGenerate = math.min(numberOfRolls, lootTable.Items.Count);
                        
                        HashSet<PrefabGUID> existingItems = new();
                        for (var i = 0; i < numberOfItemsToGenerate; i++)
                        {
                            var randomItem = lootTable.GetRandomItem();

                            if (existingItems.Contains(randomItem.ItemPrefabGUID)) continue;

                            if (Helper.AddItemToInventory(inventoryEntity, randomItem.ItemPrefabGUID, randomItem.Quantity, out var itemEntity))
                            {
                                if (randomItem.ItemPrefabGUID == Prefabs.Item_Consumable_PrisonPotion_Bloodwine)
                                {
                                    var blood = new StoredBlood()
                                    {
                                        BloodQuality = randomItem.BloodQuality,
                                    };
                                    itemEntity.Write(blood);
                                }
                                if (randomItem.Durability != -1)
                                {
                                    if (itemEntity.Has<Durability>())
                                    {
                                        var durability = itemEntity.Read<Durability>();
                                        durability.Value = randomItem.Durability;
                                        itemEntity.Write(durability);
                                    }
                                }
                            }
                            existingItems.Add(randomItem.ItemPrefabGUID);
                        }
                    }
                }
            }
        }
    }
}

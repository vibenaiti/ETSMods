using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using ModCore;
using ProjectM;
using Unity.Transforms;
using Unity.Mathematics;
using ModCore.Data;
using Unity.Entities;
using ProjectM.CastleBuilding;
using ModCore.Services;
using static ModCore.Helpers.Helper;
using ProjectM.Network;
using System;
using UnityEngine;
using ModCore.Factories;
using ProjectM.Gameplay.Scripting;
using ProjectM.Terrain;
using Unity.Physics;
using Stunlock.Core;
using Unity.Collections;
using ProjectM.Gameplay;
using ProjectM.Tiles;
using static ProjectM.SpawnChainBlobAsset;
using DebugMod.Managers;
using ProjectM.Shared.Systems;
using ProjectM.Behaviours;
using static ProjectM.Tiles.TileMapCollisionMath;
using System.Collections.Generic;
using ProjectM.Gameplay.WarEvents;
using ProjectM.Shared;
using UnityEngine.Rendering.HighDefinition;
using ProjectM.Scripting;
using System.Collections.Immutable;
using System.Linq;
using ProjectM.Shared.WarEvents;
using ProjectM.Gameplay.Systems;
using static ProjectM.SpawnBuffsAuthoring.SpawnBuffElement_Editor;
using ProjectM.UI;
using ProjectM.Debugging;
using System.Diagnostics;
using ModCore.Patches;
using Il2CppSystem.Runtime.Remoting.Channels;
using ProjectM.Transmog;
using static ProjectM.TeleportEvents_ToServer;
using ProjectM.Sequencer.Debugging;
using Stunlock.Core.Authoring;
using Stunlock.Sequencer;
using UnityEngine.Rendering;

namespace DebugMod.Commands
{
    public class TestCommands
    {
        [Command("test", description: "test", adminOnly: true)]
        public unsafe static void TestCommand(Player sender)
        {
            sender.Clan.LogComponentTypes();
/*            var entities = Helper.GetPrefabEntitiesByComponentTypes<InteractAbilityBuffer>();
            foreach (var entity in entities)
            {
                var buffer = entity.ReadBuffer<InteractAbilityBuffer>();
                foreach (var item in buffer)
                {
                    if (item.Ability == Prefabs.AB_Interact_OpenContainer_Hold_AbilityGroup)
                    {
                        entity.LogPrefabName();
                    }
                }
            }*/

            /*var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.AB_Interact_OpenContainer);

            prefabEntity.LogComponentTypes();*/

            /*            var entity = Helper.GetHoveredEntity<CastleHeartConnection>(sender.User);
                        var buffer = entity.ReadBuffer<InteractAbilityBuffer>();
                        for (var i = 0; i < buffer.Length; i++)
                        {
                            var item = buffer[i];
                            if (item.Ability == Prefabs.AB_Interact_OpenContainer_Hold_AbilityGroup)
                            {

                            }
                        }
                        foreach (var item in buffer)
                        {
                            item.Ability.LogPrefabName();
                        }*/
            /*TeleportEvents_ToServer + TeleportToWaypointEvent*/
            /*var sortedEntities = entities.OrderByDescending(entity => entity.Read<Health>().Value);
            foreach (var item in sortedEntities)
            {
                Plugin.PluginLog.LogInfo($"{item.Read<Health>().Value} {item.LookupName()}");
            }*/

            /*            var entity = Helper.GetHoveredEntity(sender.User);
                        var buffer = entity.ReadBuffer<DropTableBuffer>();
                        foreach (var item in buffer)
                        {
                            var dropTableEntity = Helper.GetPrefabEntityByPrefabGUID(item.DropTableGuid);
                            var buffer2 = dropTableEntity.ReadBuffer<DropTableDataBuffer>();
                            foreach (var item2 in buffer2)
                            {
                                item2.ItemGuid.LogPrefabName();
                            }
                        }*/
            /*            List<Entity> dropTables = new();
                        var prefabEntities = Helper.GetPrefabEntitiesByComponentTypes<DropTableDataBuffer>();
                        foreach (var prefabEntity in prefabEntities)
                        {
                            var buffer = prefabEntity.ReadBuffer<DropTableDataBuffer>();
                            foreach (var item in buffer)
                            {
                                if (item.ItemGuid == Prefabs.Item_NetherShard_T02)
                                {
                                    dropTables.Add(prefabEntity);
                                    prefabEntity.LogPrefabName();
                                }
                            }
                        }
                        foreach (var dropTable in dropTables)
                        {
                            var buffer = dropTable.ReadBuffer<DropTableDataBuffer>();
                            foreach (var item in buffer)
                            {
                                Plugin.PluginLog.LogInfo($"{dropTable.LookupName()} {item.ItemGuid.LookupName()} {item.DropRate} {item.Quantity}");
                            }
                        }*/
            //buffer.Clear();

            //TraderEntryGeneratorBlob

            /*            var entities = Helper.GetPrefabEntitiesByComponentTypes<AutoChainInstanceData>();
                        foreach (var entity in entities)
                        {
                            entity.LogPrefabName();
            *//*                var name = entity.LookupName();
                            if (name.StartsWith("Chain_"))
                            {
                                entity.LogComponentTypes();
                                break;
                            }*//*
                        }*/
            /*            var item2 = sender.Character.Read<Equipment>().GrimoireSlot.SlotEntity._Entity.Read<ItemData>();
                        Plugin.PluginLog.LogInfo(item2.ItemCategory);

                        var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.TM_Castle_Container_Specialized_Soulshards_Manticore);
                        var buffer = prefabEntity.ReadBuffer<InventoryInstanceElement>();
                        for (var i = 0; i < buffer.Length; i++)
                        {
                            var item = buffer[i];
                            item.RestrictedCategory = (long)ItemCategory.ALL;
                            item.RestrictedType = Prefabs.Item_MagicSource_SoulShard_Manticore;
                            buffer[i] = item;
                        }*/
            /*            var shardContainer = Helper.GetHoveredEntity<CastleHeartConnection>(sender.User);
                        buffer = shardContainer.ReadBuffer<InventoryInstanceElement>();
                        for (var i = 0; i < buffer.Length; i++)
                        {
                            var item = buffer[i];
                            var inventory = item.ExternalInventoryEntity._Entity;
                            var restrictedInventory = inventory.Read<RestrictedInventory>();
                            Plugin.PluginLog.LogInfo(restrictedInventory.RestrictedItemCategory);
                            restrictedInventory.RestrictedItemType.LogPrefabName();
                            restrictedInventory.RestrictedItemCategory = ItemCategory.Magic;
                            inventory.Write(restrictedInventory);
                        }*/

            /*            var systemEntities = Helper.GetEntitiesByComponentTypes<SystemInstance>(EntityQueryOptions.IncludeSystems);
                        foreach (var sys in systemEntities)
                        {
                            var sysInstance = sys.Read<SystemInstance>();
                            var state = *sysInstance.state;
                            if (state.DebugName.ToString().Contains("UpdateTilePositionSystem"))
                            {

                            }
                            foreach (var query in state.m_EntityQueries)
                            {
                                var types = query.GetQueryTypes();
                                foreach (var type in types)
                                {

                                    if (type.ToString().ToLower().Contains("legendaryitemtemplate"))
                                    {
                                        Plugin.PluginLog.LogInfo(state.DebugName);
                                    }
                                }
                            }
                        }*/


            /*var sys = VWorld.Server.GetExistingSystemManaged<ShapeshiftSystem>();
            var conditionChecker = sys._ConditionCheckerFactory.Build(ref state);
            var conditionEntities = new ConditionEntities(sender.Character, sender.Character, sender.Character);
            var condition = new TargetBoolCondition();
            var result = conditionChecker.HasSoulShard(ref conditionEntities, ref condition);

        //Helper.BuffPlayer(sender, Prefabs.AB_Shapeshift_Bat_Landing_AbilityGroup, out var buffEntity);*/
        }

        [Command("sequence", description: "test", adminOnly: true, aliases: ["playsequence"])]
        public static void PlaySequence(Player sender, int sequenceHash, Player target = null)
        {
            if (target == null)
            {
                target = sender;
            }
            //Core.serverGameManager.PlaySequenceOnTarget(target.Character, new SequenceGUID(sequenceHash));
            Core.serverGameManager.PlaySequenceOnPosition(sender.Position, new quaternion(), new SequenceGUID(sequenceHash));
        }

        [Command("tpboss", description: "test", adminOnly: true)]
        public unsafe static void TeleportToBossCommand(Player sender, VBloodPrefabData bossPrefab)
        {
            var spawnDebugEvent = new TeleportToVBloodDebugEvent
            {
                PlayerId = sender.Character.Read<NetworkId>(),
                TargetVBlood = bossPrefab.PrefabGUID
            };
            var fromCharacter = sender.ToFromCharacter();
            Core.debugEventsSystem.TeleportToVBloodEvent(sender.User.Read<User>().Index, ref spawnDebugEvent, Core.entityCommandBufferSystem.CreateCommandBuffer(), ref fromCharacter);
        }

        [Command("destroyshardmapicons", description: "test", adminOnly: true)]
        public unsafe static void DestroyShardMapIconsCommand(Player sender)
        {
            var entities = Helper.GetEntitiesByComponentTypes<RelicMapIcon>();
            foreach (var entity in entities)
            {
                Helper.DestroyEntity(entity);
            }
        }

        [Command("incomingdecay", description: "Reports which territories have the least time remaining", adminOnly: true)]
        public static void IncomingDecayCommand(Player sender)
        {
            // report a list of territories with the least time remaining
            var castleTerritories = Helper.GetEntitiesByComponentTypes<CastleTerritory>(EntityQueryOptions.IncludeDisabled);
            var castleTerritoryToTimeRemaining = new Dictionary<Entity, double>();
            foreach (var castleTerritoryEntity in castleTerritories)
            {
                var castleTerritory = castleTerritoryEntity.Read<CastleTerritory>();
                if (castleTerritory.CastleHeart == Entity.Null) continue;

                var timeRemaining = GetFuelTimeRemaining(castleTerritory.CastleHeart);
                if (timeRemaining > 0)
                {
                    castleTerritoryToTimeRemaining[castleTerritoryEntity] = timeRemaining;
                }
            }
            var sortedTerritories = castleTerritoryToTimeRemaining.OrderBy(kvp => kvp.Value);

            var i = 0;
            foreach (var territory in sortedTerritories)
            {
                if (i == 5) break;
                var territoryIndex = territory.Key.Read<CastleTerritory>().CastleTerritoryIndex;
                sender.ReceiveMessage($"{i+1} - Territory ID: {territoryIndex.ToString().White()}, Time Remaining: {Helper.FormatTime(System.Math.Round(territory.Value)).ToString().White()}");
                i++;
            }
        }

        public static double GetFuelTimeRemaining(Entity castleHeart)
        {
            var castleHeartData = castleHeart.Read<CastleHeart>();
            var secondsPerFuel = (8 * 60) / System.Math.Min(3, Core.serverGameSettingsSystem.Settings.CastleBloodEssenceDrainModifier);
            return (castleHeartData.FuelEndTime - Helper.GetServerTime()) + secondsPerFuel * castleHeartData.FuelQuantity;
        }

        [Command("tpterritory", description: "Teleports you to a territory", adminOnly: true)]
        public static void TpTerritoryCommand(Player sender, int territoryId)
        {
            // report a list of territories with the least time remaining
            var castleTerritories = Helper.GetEntitiesByComponentTypes<CastleTerritory>(EntityQueryOptions.IncludeDisabled);
            var castleTerritoryToTimeRemaining = new Dictionary<Entity, double>();
            foreach (var castleTerritoryEntity in castleTerritories)
            {
                var castleTerritory = castleTerritoryEntity.Read<CastleTerritory>();
                if (castleTerritory.CastleTerritoryIndex == territoryId)
                {
                    if (castleTerritory.CastleHeart != Entity.Null)
                    {
                        sender.Teleport(castleTerritory.CastleHeart.Read<Translation>().Value);
                        sender.ReceiveMessage($"Teleported to territory id: {territoryId}");
                        return;
                    }
                    else
                    {
                        sender.Teleport(castleTerritory.WorldBounds.Max - castleTerritory.WorldBounds.Min);
                        sender.ReceiveMessage($"Teleported to territory id: {territoryId}");
                        return;
                    }
                }
            }
            sender.ReceiveMessage("Could not find a territory with that id");
        }




        [Command("findbase", description: "test", adminOnly: true)]
        public unsafe static void FindBaseCommand(Player sender, Player target)
        {
            var foundBase = Entity.Null;
            var hearts = Helper.GetEntitiesByComponentTypes<CastleHeart>(EntityQueryOptions.IncludeDisabled);
            foreach (var heart in hearts)
            {
                var owner = heart.Read<UserOwner>().Owner._Entity;
                if (owner.Exists())
                {
                    var heartOwner = PlayerService.GetPlayerFromUser(owner);
                    if (heartOwner == target)
                    {
                        foundBase = heart;
                        break;
                    }
                    else if (heartOwner.IsAlliedWith(target))
                    {
                        foundBase = heart;
                    }
                }
            }

            if (foundBase != Entity.Null)
            {
                var eventEntity = Helper.CreateEntityWithComponents<FromCharacter, SetMapMarkerEvent>();
                eventEntity.Write(sender.ToFromCharacter());
                eventEntity.Write(new SetMapMarkerEvent
                {
                    Position = foundBase.Read<Translation>().Value.xz
                }); 
                sender.ReceiveMessage($"Placed map marker on {target.Name}'s base");
            }
            else
            {
                sender.ReceiveMessage("Player has on base".Error());
            } 
        }

        [Command("gethorseowner", description: "test", adminOnly: true)]
        public unsafe static void GetHorseOwnerCommand(Player sender)
        {
            var entity = Helper.GetHoveredTileModel<Mountable>(sender.User);
            List<Entity> entityBuffs = new List<Entity>();
            if (entity.Has<BuffBuffer>())
            {
                var buffs = entity.ReadBuffer<BuffBuffer>();
                foreach (var buff in buffs)
                {
                    entityBuffs.Add(buff.Entity);
                }
            }
            foreach (var entityBuff in entityBuffs)
            {
                if (entityBuff.GetPrefabGUID() == Prefabs.AB_Interact_Mount_Target_LastOwner_BuffIcon)
                {
                    var ownerEntity = entityBuff.Read<EntityOwner>().Owner;
                    if (ownerEntity.Exists())
                    {
                        var owner = PlayerService.GetPlayerFromCharacter(ownerEntity);
                        sender.ReceiveMessage(owner.Name);
                        break;
                    }
                }
            }
        }

        [Command("testsystems", description: "test", adminOnly: true)]
        public unsafe static void TestSystemsCommand(Player sender)
        {
            var systems = Helper.GetEntitiesByComponentTypes<SystemInstance>(EntityQueryOptions.IncludeSystems);
            foreach (var sys in systems)
            {
                var sysInstance = sys.Read<SystemInstance>();
                var state = *sysInstance.state;
                if (state.DebugName.ToString() == "ProjectM.Gameplay.Scripting.RadialZoneSystem_Curse_Server")
                {
                    sysInstance.state->Enabled = false;
                    sys.Write(sysInstance);
                    Plugin.PluginLog.LogInfo(state.m_SystemID);

                }
                //Plugin.PluginLog.LogInfo(state.DebugName);
            }
        }

        [Command("altar", description: "test", adminOnly: true)]
        public static void SpawnAltarCommand(Player sender, PrefabGUID _prefab, int rotationMode = 1, int spawnSnapMode = 5)
        {
            var spawnPosition = Helper.GetSnappedHoverPosition(sender, (SnapMode)spawnSnapMode);
            PrefabSpawnerService.SpawnWithCallback(_prefab, spawnPosition, (e) =>
            {
                if (Helper.BuffEntity(e, Helper.CustomBuff1, out var buffEntity, Helper.NO_DURATION))
                {
                    Helper.ModifyBuff(buffEntity, BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable);
                }
                e.LogComponentTypes();

                sender.ReceiveMessage("Spawned prefab!".Success());
            }, rotationMode);
        }

        [Command("autorevive", adminOnly: true)]
        public unsafe static void AutoReviveCommand(Player sender, Player target = null)
        {
            if (target == null)
            {
                target = sender;
            }
            if (AutoReviveManager.AutoRevivePlayers.Contains(target))
            {
                AutoReviveManager.AutoRevivePlayers.Remove(target);
                sender.ReceiveMessage($"Auto revive disabled for: {target}".White());
            }
            else
            {
                AutoReviveManager.AutoRevivePlayers.Add(target);
                sender.ReceiveMessage($"Auto revive enabled for: {target}".White());
            }
        }

        [Command("immortal", adminOnly: true)]
        public unsafe static void ImmortalCommand(Player sender, Player target = null)
        {
            if (target == null)
            {
                target = sender;
            }

            if (Helper.BuffPlayer(target, Helper.CustomBuff1, out var buffEntity, Helper.NO_DURATION))
            {
                Helper.ModifyBuff(buffEntity, BuffModificationTypes.Invulnerable | BuffModificationTypes.ImmuneToSun);
                Helper.ChangeBuffResistances(target.Character, Prefabs.BuffResistance_Vampire);
            }
        }


        [Command("serverfps", description: "Reports the current server FPS", adminOnly: false, aliases: ["sfps"])]
        public unsafe static void ServerFPSCommand(Player sender)
        {
            var entity = Helper.GetEntitiesByComponentTypes<WorldFrame>()[0];
            var initialFrame = entity.Read<WorldFrame>().Frame;
            var action = () =>
            {
                var newFrame = entity.Read<WorldFrame>().Frame;
                var serverFps = newFrame - initialFrame;
                sender.ReceiveMessage($"The current server FPS is: {serverFps.ToString().Warning()}".White());
            };
            ActionScheduler.RunActionOnceAfterDelay(action, 1);
        }

        [Command("fog", description: "test", adminOnly: true)]
        public unsafe static void FogCommand(Player sender, Player player = null)
        {
            if (player == null)
            {
                player = sender;
            }

            if (Helper.BuffPlayer(player, Prefabs.Buff_General_CurseOfTheForest_Area, out var buffEntity, Helper.NO_DURATION))
            {
                var buff = buffEntity.Read<Buff>();
                buff.Stacks = 100;
                buffEntity.Write(buff);
                var sharedComp = buffEntity.Read<Script_CurseAreaDebuff_DataShared>();
                sharedComp.StackSize = 100;
                buffEntity.Write(sharedComp);
                var serverComp = buffEntity.Read<Script_CursedAreaDebuff_DataServer>();
                serverComp.DynamicStacks = 100;
                serverComp.DecreaseTimeInterval = float.MaxValue;
                buffEntity.Write(serverComp);
            }
        }

        [Command("removefog", description: "test", adminOnly: true)]
        public unsafe static void RemoveFogCommand(Player sender, Player player = null)
        {
            if (player == null)
            {
                player = sender;
            }

            if (Helper.TryGetBuff(player, Prefabs.Buff_General_CurseOfTheForest_Area, out var buffEntity))
            {
                var serverComp = buffEntity.Read<Script_CursedAreaDebuff_DataServer>();
                serverComp.DecreaseTimeInterval = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Buff_General_CurseOfTheForest_Area).Read<Script_CursedAreaDebuff_DataServer>().DecreaseTimeInterval;
                buffEntity.Write(serverComp);
            }
        }

        [Command("spawnmirrorchest", description: "test", adminOnly: true)]
        public unsafe static void SpawnMirrorChestCommand(Player sender)
        {
/*            PrefabSpawnerService.SpawnWithCallback(_prefab, spawnPosition, (e) =>
        {
            e.LogComponentTypes();

            sender.ReceiveMessage("Spawned prefab!".Success());
        }, rotationMode);*/
        }


        [Command("breakfloor", description: "test", adminOnly: true)]
        public unsafe static void BreakFloorCommand(Player sender)
        {
            var entities = Helper.GetEntitiesNearPosition(sender, 5000);
            foreach (var entity in entities)
            {
                if (entity.GetPrefabGUID() == Prefabs.TM_Castle_Floor_Outdoor_Plain01)
                {
                    Helper.DestroyEntity(entity);
                }
            }
        }

        [Command("makefloor", description: "test", adminOnly: true)]
        public unsafe static void MakeFloorCommand(Player sender)
        {
            var pos = sender.Position;

            // Calculate the start position; we subtract half the total length of the tiles
            // 35 tiles * 5 units per tile / 2 to center the player's position
            Vector3 startPos = new Vector3(
                pos.x - (35 * 5 / 2),
                pos.y,
                pos.z - (35 * 5 / 2)
            );

            for (int i = 0; i < 35; i++) // Rows
            {
                for (int j = 0; j < 35; j++) // Columns
                {
                    // Calculate the position for each tile
                    Vector3 tilePos = new Vector3(
                        startPos.x + (i * 5),
                        startPos.y,
                        startPos.z + (j * 5)
                    );

                    // Spawn the tile
                    PrefabSpawnerService.SpawnWithCallback(Prefabs.TM_Castle_Floor_Outdoor_Plain01, tilePos, (e) => { });
                }
            }
        }


        [Command("killallmobs", description: "test", adminOnly: true)]
        public unsafe static void KillAllMobsCommand(Player sender)
        {
            var mobs = Helper.GetEntitiesByComponentTypes<AggroConsumer>(EntityQueryOptions.IncludeDisabled);
            foreach (var mob in mobs)
            {
                Helper.DestroyEntity(mob);
            }
        }


        [Command("tileposition", description: "test", adminOnly: true)]
        public unsafe static void TilePositionCommand(Player sender)
        {
            sender.ReceiveMessage(sender.TilePosition);
        }

        [Command("fillheart", description: "test", aliases: new string[] { "fill-heart" }, adminOnly: true)]
        public unsafe static void FillHeartCommand(Player sender)
        {
            var heart = Helper.GetHoveredTileModel<CastleHeart>(sender.User);
            if (heart.Has<InventoryBuffer>())
            {
                var buffer = heart.ReadBuffer<InventoryBuffer>();
                for (var i = 0; i < buffer.Length; i++)
                {
                    var inventory = buffer[i];
                    inventory.Amount = 4095;
                    inventory.MaxAmountOverride = 4095;
                    buffer[i] = inventory;
                }
            }
        }

        [Command("claim-structures", description: "test", adminOnly: true)]
        public static void ClaimStructuresCommand(Player sender)
        {
            var entities = Helper.GetEntitiesByComponentTypes<CastleTerritory>();
            var adminTerritory = entities[0].Read<CastleTerritory>();
            var adminHeart = adminTerritory.CastleHeart;

            var structures = Helper.GetEntitiesByComponentTypes<CastleHeartConnection, Team>();
            foreach (var structure in structures)
            {
                if (structure.Has<RespawnPoint>() && structure.GetPrefabGUID() != Prefabs.TM_Workstation_Waypoint_Castle) continue;
                var castleHeartConnection = structure.Read<CastleHeartConnection>();
                if (!castleHeartConnection.CastleHeartEntity._Entity.Exists() && structure.Read<Team>().Value == 1)
                {
                    castleHeartConnection.CastleHeartEntity._Entity = adminHeart;
                    structure.Write(castleHeartConnection);

                    var team = structure.Read<Team>();
                    team.Value = sender.Character.Read<Team>().Value;
                    team.FactionIndex = sender.Character.Read<Team>().FactionIndex;
                    structure.Write(team);

                    if (Helper.BuffEntity(structure, Helper.CustomBuff1, out var buffEntity, Helper.NO_DURATION))
                    {
                        Helper.ModifyBuff(buffEntity, BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable);
                    }
                    Plugin.PluginLog.LogInfo($"{structure.Read<Team>().Value} {structure.LookupName()}");
                }
            }
        }

        [Command("stop-time", description: "Stops time", adminOnly: true)]
        public static void StopTimeCommand(Player sender)
        {
            SetDebugSettingEvent DayNightCycleDisabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.DayNightCycleDisabled,
                Value = true
            };

            Core.debugEventsSystem.SetDebugSetting(0, ref DayNightCycleDisabledSetting);
            sender.ReceiveMessage("Stopped time".White());
        }

        [Command("start-time", description: "Starts time", adminOnly: true)]
        public static void StartTimeCommand(Player sender)
        {
            SetDebugSettingEvent DayNightCycleDisabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.DayNightCycleDisabled,
                Value = false
            };

            Core.debugEventsSystem.SetDebugSetting(0, ref DayNightCycleDisabledSetting);
            sender.ReceiveMessage("Started time".White());
        }

        [Command("players", description: "Tells you how many people are online", adminOnly: false)]
        public void PlayerCountCommand(Player sender)
        {
            if (PlayerService.OnlinePlayersWithUsers.Count == 1)
            {
                sender.ReceiveMessage(
                    $"There is {PlayerService.OnlinePlayersWithUsers.Count.ToString().Emphasize()} player online".White());
            }
            else
            {
                sender.ReceiveMessage(
                    $"There are {PlayerService.OnlinePlayersWithUsers.Count.ToString().Emphasize()} players online".White());
            }
        }

        [Command("setmovementspeed", description: "Used for debugging", adminOnly: true)]
        public void SetMovementSpeedCommand(Player sender, float movementSpeed)
        {
            if (Helper.BuffPlayer(sender, Helper.CustomBuff2, out var buffEntity, Helper.NO_DURATION, true))
            {
                Helper.ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    StatType = UnitStatType.MovementSpeed,
                    Modifier = 1,
                    Value = movementSpeed
                });
            }
        }

        [Command("spawnwaypoint", description: "Used for debugging", aliases: new string[] { "spawn-waypoint" },
            adminOnly: true)]
        public void SpawnWaypointCommand(Player sender, int rotationMode = 1, int spawnSnapMode = 5)
        {
            var spawnPosition = Helper.GetSnappedHoverPosition(sender, (SnapMode)spawnSnapMode);
            PrefabSpawnerService.SpawnWithCallback(Prefabs.TM_Workstation_Waypoint_World_UnlockedFromStart,
                spawnPosition, (Entity e) =>
                {
                    var chunkWaypoint = e.Read<ChunkWaypoint>();
                    chunkWaypoint.DefaultUnlocked = true;
                    chunkWaypoint.IsLocked = false;
                    e.Write(chunkWaypoint);
                    sender.ReceiveMessage("Spawned prefab!".Success());
                }, rotationMode);
        }

        [Command("log-nearby-entities", description: "Used for debugging", adminOnly: true)]
        public void LogNearbyEntitiesCommand(Player sender)
        {
            var entities = Helper.GetEntitiesByComponentTypes<PrefabGUID, LocalToWorld>();
            var myPos = sender.Position;

            foreach (var entity in entities)
            {
                var entityPos = entity.Read<LocalToWorld>().Position;
                var distance = math.distance(myPos, entityPos);
                if (distance < 2)
                {
                    Plugin.PluginLog.LogInfo(entity.Read<PrefabGUID>().LookupName());

                    if (entity.Read<PrefabGUID>() == Prefabs.AB_Storm_LightningWall_Object)
                    {
                        entity.LogComponentTypes();
                    }
                }
            }
        }

        [Command("log-components", description: "Logs components of hovered entity", adminOnly: true)]
        public void LogComponentsCommand(Player sender)
        {
            var entity = Helper.GetHoveredEntity(sender.User);
            if (entity != Entity.Null)
            {
                entity.LogComponentTypes();
            }
        }


        [Command("log-position", description: "Logs position of hovered entity", adminOnly: true)]
        public void LogPositionCommand(Player sender)
        {
            var entity = Helper.GetHoveredTileModel(sender.User);
            if (entity != Entity.Null)
            {
                entity.LogPrefabName();
                var localToWorld = entity.Read<LocalToWorld>();
                var message =
                    $"\"X\": {localToWorld.Position.x},\n\"Y\": {localToWorld.Position.y},\n\"Z\": {localToWorld.Position.z}";
                sender.ReceiveMessage(message);
                Plugin.PluginLog.LogInfo(message);
            }
        }


        [Command("log-structure-position", description: "Logs position of hovered entity", adminOnly: true)]
        public void LogStructurePositionCommand(Player sender)
        {
            var entity = Helper.GetHoveredTileModel<CastleHeartConnection>(sender.User);
            if (entity != Entity.Null)
            {
                entity.LogPrefabName();
                var localToWorld = entity.Read<LocalToWorld>();
                var message =
                    $"\"X\": {localToWorld.Position.x},\n\"Y\": {localToWorld.Position.y},\n\"Z\": {localToWorld.Position.z}";
                sender.ReceiveMessage(message);
                Plugin.PluginLog.LogInfo(message);
            }
        }


        [Command("log-tile-position", description: "Logs position of hovered tile", adminOnly: true)]
        public void LogTilePositionCommand(Player sender, int snapMode = (int)Helper.SnapMode.Center)
        {
            var snappedAimPosition = Helper.GetSnappedHoverPosition(sender, (Helper.SnapMode)snapMode);
            var message =
                $"\"X\": {snappedAimPosition.x},\n\"Y\": {snappedAimPosition.y},\n\"Z\": {snappedAimPosition.z}";
            sender.ReceiveMessage(message);
            Plugin.PluginLog.LogInfo(message);
        }

        [Command("log-structure-zone", description: "Logs dimensions of hovered entity", adminOnly: true)]
        public void LogSizeCommand(Player sender, int x, int y)
        {
            var entity = Helper.GetHoveredTileModel(sender.User);
            if (entity != Entity.Null && entity.Has<TileBounds>())
            {
                sender.ReceiveMessage($"Printed zone for: {entity.Read<PrefabGUID>().LookupName()}");
                Plugin.PluginLog.LogInfo(RectangleZone.FromEntity(entity, x, y).ToString());
            }
            else
            {
                sender.ReceiveMessage("Invalid entity");
            }
        }

        [Command(name: "log-height", description: "Gets the tile height where you're standing", usage: ".log-height", adminOnly: true, includeInHelp: false)]
        public void LogHeightCommand(Player sender)
        {
            var tilePosition = sender.Character.Read<TilePosition>();
            sender.ReceiveMessage(tilePosition.HeightLevel.ToString().White());
            Plugin.PluginLog.LogInfo($"Height: {tilePosition.HeightLevel}");
        }

        [Command(name: "log-zone", description: "Gets the zone assuming you are at the bottom left facing north, right then up", usage: ".get-zone", adminOnly: true, includeInHelp: false)]
        public void LogZoneCommand(Player player, int x, int z)
        {
            player.ReceiveMessage($"{RectangleZone.GetZoneByCurrentCoordinates(player, x, z)}");
            Plugin.PluginLog.LogInfo($"{RectangleZone.GetZoneByCurrentCoordinates(player, x, z)}");
        }

        [Command(name: "admin-give", description: "Gives the specified item to a target player", usage: ".ag <item> [qty] [player]", aliases: ["ag"], adminOnly: true)]
        public static void GiveItem(Player sender, ItemPrefabData item, int quantity = 1, Player player = null)
        {
            Player Player = player ?? sender;

            if (Helper.AddItemToInventory(Player.Character, item.PrefabGUID, quantity, out var entity))
            {
                var itemName = item.PrefabGUID.LookupName();
                sender.ReceiveMessage($"Gave : {quantity.ToString().Emphasize()} {itemName.White()} to {Player.Name.Emphasize()}".Success());
            }
        }

        [Command("add-buff", description: "Buff a player with a prefab name or guid", usage: ".add-buff buffGuid, player, duration, persistsThroughDeath", aliases: new string[] { "buff", "bf" }, adminOnly: true)]
        public void BuffCommand(Player sender, PrefabGUID buffGuid, Player player = null, float duration = Helper.NO_DURATION, bool persistsThroughDeath = false)
        {
            var Player = player != null ? player : sender;

            try
            {
                Helper.BuffPlayer(Player, buffGuid, out var buffEntity, duration, persistsThroughDeath);
                sender.ReceiveMessage("Added buff.".Success());
            }
            catch (Exception e)
            {
                sender.ReceiveMessage(e.ToString().Error());
                return;
            }
        }

        [Command("remove-buff", description: "Removes a buff", aliases: new string[] { "unbuff", "debuff", "dbf" }, adminOnly: true)]
        public void UnbuffCommand(Player sender, PrefabGUID buffGuid, Player player = null)
        {
            var Player = player != null ? player : sender;
            Helper.RemoveBuff(Player.Character, buffGuid);
            sender.ReceiveMessage("Removed buff.".Success());
        }

        [Command("buff-target", description: "Used for debugging", adminOnly: true)]
        public static void BuffHoveredTargetCommand(Player sender, PrefabGUID buffGuid,
            float duration = Helper.DEFAULT_DURATION, bool persistsThroughDeath = false)
        {
            Entity entity = Helper.GetHoveredTileModel(sender.User);
            Helper.BuffEntity(entity, buffGuid, out var buffEntity, Helper.NO_DURATION, persistsThroughDeath);
            Helper.ModifyBuff(buffEntity, BuffModificationTypes.None);
            buffEntity.Add<DestroyBuffOnDamageTaken>();

            sender.ReceiveMessage("Done");
        }

        [Command("clear-target-buffs", description: "Used for debugging", adminOnly: true)]
        public static void ClearHoveredTargetBuffsCommand(Player sender)
        {
            Entity entity = Helper.GetHoveredTileModel(sender.User);
            Helper.ClearExtraBuffs(sender.Character, Helper.ResetOptions.FreshMatch);

            sender.ReceiveMessage($"Done: {entity.Read<PrefabGUID>().LookupName()}");
        }

        [Command("remove-target-buff", description: "Removes a buff", adminOnly: true)]
        public void UnbuffTargetCommand(Player sender, PrefabGUID buffGuid)
        {
            var entity = Helper.GetHoveredTileModel(sender.User);
            Helper.RemoveBuff(entity, buffGuid);
            sender.ReceiveMessage("Removed buff.".Success());
        }

        [Command("list-target-buffs", description: "Lists the buffs a hovered character has", adminOnly: true)]
        public void ListTargetBuffsCommand(Player sender)
        {
            var target = Helper.GetHoveredTileModel(sender.User);
            var buffs = Helper.GetAllBuffs(target);
            foreach (var buff in buffs)
            {
                sender.ReceiveMessage(buff.LookupName().White());
            }

            sender.ReceiveMessage($"Done: {target.LookupName()}");
        }

        [Command("clear-buffs", description: "Removes any extra buffs on a player", adminOnly: true)]
        public void ClearBuffsCommand(Player sender, Player player = null)
        {
            var Player = player != null ? player : sender;
            Helper.ClearExtraBuffs(Player.Character, new Helper.ResetOptions
            {
                RemoveConsumables = true,
                RemoveShapeshifts = true
            });
            sender.ReceiveMessage("Extra buffs cleared.".Success());
        }

        [Command("nobuildingcosts", aliases: new string[] { "nobuildcosts", "disablebuildingcosts", "nobuildingcost", "nobuildcost", "disablebuildingcost", "disablebuildcost", "disablebuildcosts" }, adminOnly: true)]
        public void NoBuildCostsCommand(Player sender)
        {
            SetDebugSettingEvent BuildCostsDisabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.BuildCostsDisabled,
                Value = true
            };

            var debugEventsSystem = VWorld.Server.GetExistingSystemManaged<DebugEventsSystem>();
            debugEventsSystem.SetDebugSetting(0, ref BuildCostsDisabledSetting);

            sender.ReceiveMessage("Building costs disabled");
        }

        [Command("enable-freebuild", aliases: new string[] { "enablefreebuild", "enable-free-build" }, adminOnly: true)]
        public void EnableFreeBuildCommand(Player sender)
        {
            SetDebugSettingEvent BuildCostsDisabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.BuildCostsDisabled,
                Value = true
            };

            SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.BuildingPlacementRestrictionsDisabled,
                Value = true
            };

            SetDebugSettingEvent GlobalCastleTerritoryEnabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.GlobalCastleTerritoryEnabled,
                Value = true
            };

            SetDebugSettingEvent FreeBuildingPlacementEnabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.FreeBuildingPlacementEnabled,
                Value = true
            };

            var debugEventsSystem = VWorld.Server.GetExistingSystemManaged<DebugEventsSystem>();
            debugEventsSystem.SetDebugSetting(0, ref BuildCostsDisabledSetting);
            debugEventsSystem.SetDebugSetting(0, ref BuildingPlacementRestrictionsDisabledSetting);
            debugEventsSystem.SetDebugSetting(0, ref GlobalCastleTerritoryEnabledSetting);
            debugEventsSystem.SetDebugSetting(0, ref FreeBuildingPlacementEnabledSetting);
            FreeBuildManager.Initialize();
            sender.ReceiveMessage("Free build enabled");
        }

        [Command("disable-freebuild", aliases: ["disablefreebuild", "disable-free-build"], adminOnly: true)]
        public void DisableFreeBuildCommand(Player sender)
        {
            SetDebugSettingEvent BuildCostsDisabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.BuildCostsDisabled,
                Value = false
            };

            SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.BuildingPlacementRestrictionsDisabled,
                Value = false
            };

            SetDebugSettingEvent GlobalCastleTerritoryEnabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.GlobalCastleTerritoryEnabled,
                Value = false
            };

            SetDebugSettingEvent FreeBuildingPlacementEnabledSetting = new SetDebugSettingEvent()
            {
                SettingType = DebugSettingType.FreeBuildingPlacementEnabled,
                Value = false
            };

            var debugEventsSystem = VWorld.Server.GetExistingSystemManaged<DebugEventsSystem>();
            debugEventsSystem.SetDebugSetting(0, ref BuildCostsDisabledSetting);
            debugEventsSystem.SetDebugSetting(0, ref BuildingPlacementRestrictionsDisabledSetting);
            debugEventsSystem.SetDebugSetting(0, ref GlobalCastleTerritoryEnabledSetting);
            debugEventsSystem.SetDebugSetting(0, ref FreeBuildingPlacementEnabledSetting);
            FreeBuildManager.Dispose();
            sender.ReceiveMessage("Free build disabled");
        }

        [Command("bloodpotion", description: "Creates a Potion with specified Blood Type, Quality, and Quantity", adminOnly: true, aliases: ["bpotion", "bpot"])]
        public static void GiveBloodPotionCommand(Player sender, BloodPrefabData bloodType, int quantity = 1, float quality = 100f)
        {
            sender.ReceiveMessage(bloodType.PrefabGUID.LookupName());
            quality = Mathf.Clamp(quality, 0, 100);
            int i;
            for (i = 0; i < quantity; i++)
            {
                if (Helper.AddItemToInventory(sender.Character, Prefabs.Item_Consumable_PrisonPotion_Bloodwine, 1, out var bloodPotionEntity))
                {
                    var blood = new StoredBlood()
                    {
                        BloodQuality = quality,
                    };
                    bloodPotionEntity.Write(blood);
                }
                else
                {
                    break;
                }
            }

            sender.ReceiveMessage($"Got {i} Blood Potion(s)".White());
        }

        [Command("abilities", description: "Overwrites your ability bar", usage: ".abilities", adminOnly: true)]
        public static void AbilitiesCommand(Player sender, string mode = "")
        {
            Helper.RemoveBuff(sender, Helper.CustomBuff3);

            if (DebugModConfig.Config.AbilityPresets.TryGetValue(mode, out var abilities))
            {
                var action = () => 
                {
                    if (Helper.BuffPlayer(sender, Helper.CustomBuff3, out var buffEntity, Helper.NO_DURATION))
                    {
                        var abilityBar = new AbilityBar
                        {
                            Auto = abilities["Auto"],
                            Weapon1 = abilities["Weapon1"],
                            Weapon2 = abilities["Weapon2"],
                            Dash = abilities["Dash"],
                            Spell1 = abilities["Spell1"],
                            Spell2 = abilities["Spell2"],
                            Ult = abilities["Ult"],
                        };
                        abilityBar.ApplyChangesSoft(buffEntity);
                        sender.ReceiveMessage($"Abilities applied!".White());
                    }
                };
                ActionScheduler.RunActionOnceAfterFrames(action, 2);
            }
        }

        [Command("control", description: "Takes control over hovered NPC (Unstable, work-in-progress)", adminOnly: true)]
        public static void ControlCommand(Player sender)
        {
            ControlDebugEvent controlDebugEvent;
            DebugEventsSystem des = VWorld.Server.GetExistingSystemManaged<DebugEventsSystem>();
            var entityInput = sender.Character.Read<EntityInput>();
            AggroConsumer aggroConsumer;
            if (entityInput.HoveredEntity.Exists())
            {
                Entity newCharacter = entityInput.HoveredEntity;
                if (newCharacter.Has<AggroConsumer>())
                {
                    aggroConsumer = newCharacter.Read<AggroConsumer>();
                    aggroConsumer.Active._Value = false;
                    newCharacter.Write(aggroConsumer);
                }
                if (!newCharacter.Has<PlayerCharacter>())
                {
                    controlDebugEvent = new ControlDebugEvent
                    {
                        EntityTarget = newCharacter,
                        Target = entityInput.HoveredEntityNetworkId
                    };

                    des.ControlUnit(sender.ToFromCharacter(), controlDebugEvent);
                    sender.ReceiveMessage($"Controlling hovered unit");
                    return;
                }
            }
            var oldCharacter = sender.User.Read<Controller>().Controlled._Entity;
            aggroConsumer = oldCharacter.Read<AggroConsumer>();
            aggroConsumer.Active._Value = true;
            oldCharacter.Write(aggroConsumer);
            controlDebugEvent = new ControlDebugEvent
            {
                EntityTarget = sender.Character,
                Target = sender.Character.Read<NetworkId>()
            };
            des.ControlUnit(sender.ToFromCharacter(), controlDebugEvent);
            sender.ReceiveMessage("Controlling self");
        }
    }
}
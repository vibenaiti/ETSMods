using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using ModCore.Events;
using ModCore.Factories;
using ModCore.Models;
using ModCore;
using ModCore.Data;
using ModCore.Helpers;
using Unity.Collections;
using ModCore.Services;
using ProjectM.CastleBuilding;
using System.Threading;
using PointsMod;
using Stunlock.Core;
using ProjectM.Shared;
using ProjectM.Gameplay.Scripting;

namespace OpenWorldEvents.Managers
{
    public static class DonkeyManager
    {
        private const string Category = "Mare-a-thon";
        public static Dictionary<Entity, Entity> HorseToMapIcon = new();
        public static Dictionary<Entity, Player> HorseToLastMounter = new();
        public static Player CurrentMounter = null;
        public static List<Timer> Timers = new();
        private static ServerGameSettings Settings;
        private static System.Random random = new();

        public static void Initialize()
        {
            GameEvents.OnPlayerMounted += HandleOnPlayerMounted;
            GameEvents.OnPlayerDismounted += HandleOnPlayerDismounted;
            GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;

            Settings = VWorld.Server.GetExistingSystemManaged<ServerGameSettingsSystem>().Settings;
            if (HorseConfig.Config.DisableDurabilityLossDuringEvent)
            {
                EventHelper.SetDeathDurabilityLoss(0);
            }
            
            var timer = ActionScheduler.RunActionEveryInterval(OnUpdate, 1f);
            Timers.Add(timer);
            PointsMod.Globals.CurrencyMultiplier = OpenWorldEventsConfig.Config.CurrencyMultiplierDuringEvents;
            PointsMod.Globals.ActiveEvents.Add(Category);
        }

        public static void Dispose(bool hard = true)
        {
            GameEvents.OnPlayerMounted -= HandleOnPlayerMounted;
            GameEvents.OnPlayerDismounted -= HandleOnPlayerDismounted;
            GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;

            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }

            Timers.Clear();

            foreach (var kvp in HorseToMapIcon)
            {
                if (kvp.Key.Exists())
                {
                    Helper.DestroyEntity(kvp.Value);
                    Helper.DestroyEntity(kvp.Key);
                }
            }

            HorseToMapIcon.Clear();
            HorseToLastMounter.Clear();
            CurrentMounter = null;
            DisposeLingeringUnits();
            foreach (var player in PlayerService.CharacterCache.Values)
            {
                Helper.RemoveBuff(player, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff);
            }
            PointsMod.Globals.CurrencyMultiplier = 1;

            if (hard)
            {
                PointsMod.Globals.ActiveEvents.Remove(Category);
                EventHelper.SetDeathDurabilityLoss(OpenWorldEventsConfig.Config.DefaultDurabilityLoss);
                PointsMod.Globals.CurrencyMultiplier = 1;
            }
            else
            {
                
                if (HorseConfig.Config.AnnounceEvent && HorseConfig.Config.DisableDurabilityLossDuringEvent)
                {
                    Helper.SendSystemMessageToAllClients($"{"Durability loss".White()} will be re-enabled in {Helper.FormatTime(OpenWorldEventsConfig.Config.DurabilityDelayAfterEvent).Emphasize()}!".Warning());
                }
                    
                var action = () =>
                {
                    if (HorseConfig.Config.AnnounceEvent && HorseConfig.Config.DisableDurabilityLossDuringEvent)
                    {
                        Helper.SendSystemMessageToAllClients($"{"Durability loss".White()} is now {"enabled".Emphasize()}!".Warning());
                    }
                    PointsMod.Globals.ActiveEvents.Remove(Category);
                    EventHelper.SetDeathDurabilityLoss(OpenWorldEventsConfig.Config.DefaultDurabilityLoss);
                    PointsMod.Globals.CurrencyMultiplier = 1;
                };
                Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, OpenWorldEventsConfig.Config.DurabilityDelayAfterEvent));
            }
        }

        public static void HandleOnPlayerBuffed(Player player, Entity buffEntity, PrefabGUID prefabGUID)
        {
            if (prefabGUID == Prefabs.AB_Subdue_Channeling)
            {
                var target = buffEntity.Read<SpellTarget>().Target._Entity;
                if (target.Exists() && HorseToMapIcon.ContainsKey(target))
                {
                    Helper.DestroyBuff(buffEntity);
                }
            }
        }

        public static void CleanUpPreviousEntities()
        {
            var entities = Helper.GetEntitiesByComponentTypes<Mountable, CanFly>();
            foreach (var entity in entities)
            {
                Helper.DestroyEntity(entity);
            }
        }

        public static void CleanUpBuffsOnServerStart()
        {
            foreach (var player in PlayerService.CharacterCache.Values)
            {
                Helper.RemoveBuff(player, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff);
                if (Helper.TryGetBuff(player, Prefabs.Buff_General_CurseOfTheForest_Area, out var buffEntity))
                {
                    var curseAreaDebuffServer = buffEntity.Read<Script_CursedAreaDebuff_DataServer>();
                    var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Buff_General_CurseOfTheForest_Area);
                    var prefabCurseAreaDebuffServer = prefabEntity.Read<Script_CursedAreaDebuff_DataServer>();
                    curseAreaDebuffServer.DecreaseTimeInterval = prefabCurseAreaDebuffServer.DecreaseTimeInterval;
                    buffEntity.Write(curseAreaDebuffServer);
                }
            }
        }

        public static void DisposeLingeringUnits()
        {
            UnitFactory.DisposeTimers(Category);
            var spawnedEntities = Helper.GetEntitiesByComponentTypes<CanFly, AggroConsumer>();
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

        public static void OnUpdate()
        {
            foreach (var kvp in HorseToMapIcon)
            {
                //if the horse is mounted we have to use the player position instead
                if (CurrentMounter != null)
                {
                    if (CurrentMounter.IsInBase(out var territoryEntity, out var territoryAlignment, true))
                    {
                        if (territoryAlignment == Helper.TerritoryAlignment.Friendly)
                        {
                            Helper.RemoveBuff(CurrentMounter, Prefabs.Buff_General_CurseOfTheForest_Area);
                            if (HorseConfig.Config.AnnounceEvent)
                            {
                                Helper.SendSystemMessageToAllClients($"{CurrentMounter.Name.Colorify(ExtendedColor.ClanNameColor)} has captured the {"Special Horse".Emphasize()}".Warning());
                            }
                            KillHorseAndEndEvent(kvp.Key);
                        }
                    }
                }
                else
                {
                    if (Helper.IsInBase(kvp.Key, out var territoryEntity, out var territoryAlignment, true))
                    {
                        if (territoryAlignment == Helper.TerritoryAlignment.Friendly)
                        {
                            if (HorseConfig.Config.AnnounceEvent)
                            {
                                if (HorseToLastMounter.TryGetValue(kvp.Key, out var player))
                                {
                                    Helper.SendSystemMessageToAllClients($"{player.Name.Colorify(ExtendedColor.ClanNameColor)} has captured the {"Special Horse".Emphasize()}".Warning());
                                }
                            }
                            KillHorseAndEndEvent(kvp.Key);
                        }
                    }
                }
            }
        }

        public static void HandleOnPlayerMounted(Player player, Entity buffEntity)
        {
            var horse = buffEntity.Read<SpellTarget>().Target._Entity;
            if (HorseToMapIcon.ContainsKey(horse))
            {
                if (Helper.BuffPlayer(player, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff, out var buffEntity2, Helper.NO_DURATION))
                {
                    Helper.ModifyBuff(buffEntity2, BuffModificationTypes.WaypointImpair);
                    buffEntity2.Remove<ModifyMovementSpeedBuff>();
                    Helper.BuffPlayer(player, Prefabs.AB_Gallop_Buff, out var buffEntity3);
                }
                if (HorseConfig.Config.CursedFogOnRider)
                {
                    if (Helper.BuffPlayer(player, Prefabs.Buff_General_CurseOfTheForest_Area, out var buffEntity4))
                    {
                        
                        var buff = buffEntity4.Read<Buff>();
                        buff.Stacks = 100;
                        buffEntity4.Write(buff);

                        var cursedAreaDebuff = buffEntity4.Read<Script_CurseAreaDebuff_DataShared>();
                        cursedAreaDebuff.StackSize = 100;
                        buffEntity4.Write(cursedAreaDebuff);

                        var cursedAreaDebuffServer = buffEntity4.Read<Script_CursedAreaDebuff_DataServer>();
                        cursedAreaDebuffServer.DynamicStacks = 100;
                        cursedAreaDebuffServer.DecreaseTimeInterval = float.MaxValue;
                        buffEntity4.Write(cursedAreaDebuffServer);
                    }
                }

                HorseToLastMounter[horse] = player;
                CurrentMounter = player;
            }
        }

        public static void HandleOnPlayerDismounted(Player player, Entity buffEntity)
        {
            var horse = buffEntity.Read<SpellTarget>().Target._Entity;
            if (HorseToMapIcon.ContainsKey(horse))
            {
                Helper.RemoveBuff(player, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff);
                if (Helper.TryGetBuff(player, Prefabs.Buff_General_CurseOfTheForest_Area, out var buffEntity2))
                {
                    var curseAreaDebuffServer = buffEntity2.Read<Script_CursedAreaDebuff_DataServer>();
                    var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Buff_General_CurseOfTheForest_Area);
                    var prefabCurseAreaDebuffServer = prefabEntity.Read<Script_CursedAreaDebuff_DataServer>();
                    curseAreaDebuffServer.DecreaseTimeInterval = prefabCurseAreaDebuffServer.DecreaseTimeInterval;
                    buffEntity2.Write(curseAreaDebuffServer);
                }
            }

            CurrentMounter = null;
        }

        public static void SpawnHorseAtRandomLocation(int index = -1)
        {
            if (index == -1)
            {
                index = random.Next(HorseConfig.Config.HorseSpawnLocations.Count);
            }
            SpawnHorse(HorseConfig.Config.HorseSpawnLocations[index].ToFloat3());
        }

        public static void SpawnHorse(float3 pos)
        {
            Dispose();
            var horse = new Horse();
            horse.Category = "Mare-a-thon";
            horse.Speed = HorseConfig.Config.HorseSpeed;
            horse.Acceleration = HorseConfig.Config.HorseAcceleration;
            horse.IsImmaterial = false;
            horse.IsInvulnerable = true;
            horse.IsTargetable = false;
            horse.DrawsAggro = false;
            horse.KnockbackResistance = true;
            horse.SoftSpawn = true;
            horse.SpawnDelay = HorseConfig.Config.HorseDelaySeconds;
            UnitFactory.SpawnUnitWithCallback(horse, pos, (e) =>
            {
                e.Remove<Interactable>();
                var action = () =>
                {
                    e.Add<Interactable>();
                    if (HorseConfig.Config.AnnounceEvent)
                    {
                        Helper.SendSystemMessageToAllClients($"The {"Special Horse".Emphasize()} is now {"ready to move".White()}!".Warning());
                    }
                };
                Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, HorseConfig.Config.HorseDelaySeconds));

                Helper.BuffEntity(e, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff, out var buffEntity, Helper.NO_DURATION);
                Helper.CreateAndAttachMapIconToEntity(e, Prefabs.MapIcon_DraculasCastle, (iconEntity) =>
                {
                    HorseToMapIcon[e] = iconEntity;
                    if (HorseToMapIcon.Count == 1)
                    {
                        Initialize();
                    }
                    var durabilityString = "";
                    if (HorseConfig.Config.DisableDurabilityLossDuringEvent)
                    {
                        durabilityString = $" -- {"durability loss is disabled".White()} during the event.";
                        EventHelper.SetDeathDurabilityLoss(0);
                    }

                    if (HorseConfig.Config.AnnounceEvent)
                    {
                        Helper.SendSystemMessageToAllClients($"A {"Special Horse".Emphasize()} has spawned! It will be available to move in {Helper.FormatTime(HorseConfig.Config.HorseDelaySeconds).Emphasize()}. Bring it to your base for rewards!".Warning());
                        Helper.SendSystemMessageToAllClients($"Look for the {"Dracula's Castle".Emphasize()} icon on the map to find it{durabilityString}".Warning());
                    }
                });
            });
        }

        public static void KillHorseAndEndEvent(Entity entity)
        {
            try
            {
                if (HorseToMapIcon.TryGetValue(entity, out var mapIconEntity))
                {
                    bool horseSpawned = HorseToMapIcon.Count > 0;

                    if (HorseToLastMounter.TryGetValue(entity, out var player))
                    {
                        foreach (var item in HorseConfig.Config.HorseItems)
                        {
                            Helper.AddItemToInventory(player, item.ItemPrefabGUID, item.Quantity, out var itemEntity);
                        }
                        Helper.RemoveBuff(player, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff);
                    }

                    HorseToMapIcon.Remove(entity);
                    if (HorseToMapIcon.Count == 0)
                    {
                        Dispose();
                    }
                    Helper.DestroyEntity(mapIconEntity);
                }

                var feedableInventory = entity.Read<FeedableInventory>();
                feedableInventory.IsFed = false;
                feedableInventory.RequiredItemType = PrefabGUID.Empty;
                entity.Write(feedableInventory);
                Helper.ClearEntityInventory(entity);
                Helper.KillOrDestroyEntity(entity);
            }
            catch (Exception e)
            {
                Plugin.PluginLog.LogInfo(e.ToString());
            }
        }
    }
}
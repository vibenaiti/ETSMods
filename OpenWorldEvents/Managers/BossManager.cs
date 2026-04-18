using ProjectM;
using System.Collections.Generic;
using Unity.Entities;
using ModCore;
using ModCore.Data;
using ModCore.Helpers;
using ModCore.Services;
using System.Threading;
using ModCore.Factories;
using ModCore.Models;
using System;
using Unity.Mathematics;
using PointsMod;
using ModCore.Events;
using Stunlock.Core;
using ProjectM.Shared;

namespace OpenWorldEvents.Managers
{
    public static class BossManager
    {
        private const string Category = "eventboss";
        private static List<Timer> Timers = new();
        public static Dictionary<Entity, Entity> BossToMapIcon = new();
        private static Dictionary<Player, int> PlayerDamageToBoss = new();
        private static PrefabGUID DropTable = Prefabs.DT_Unit_Demon;
        private static Entity DropTablePrefabEntity;
        private static ServerGameSettings Settings;
        private static System.Random random = new();
        private static List<PrefabGUID> SpawnableBosses = new()
        {
            Prefabs.CHAR_Cursed_Witch_VBlood,
			/*Prefabs.CHAR_Geomancer_Human_VBlood,*/
			Prefabs.CHAR_VHunter_Jade_VBlood,
            Prefabs.CHAR_Undead_Priest_VBlood,
            Prefabs.CHAR_Bandit_Tourok_VBlood,
            Prefabs.CHAR_Spider_Queen_VBlood,
			/*Prefabs.CHAR_Winter_Yeti_VBlood,*/
			Prefabs.CHAR_Bandit_Chaosarrow_VBlood,
            Prefabs.CHAR_Undead_BishopOfDeath_VBlood,
            Prefabs.CHAR_Militia_Leader_VBlood,
            Prefabs.CHAR_Undead_BishopOfShadows_VBlood,
            Prefabs.CHAR_Bandit_Foreman_VBlood,
            Prefabs.CHAR_Bandit_Frostarrow_VBlood,
            Prefabs.CHAR_Forest_Bear_Dire_Vblood,
			/*Prefabs.CHAR_Militia_Nun_VBlood,*/
			Prefabs.CHAR_Bandit_Bomber_VBlood,
            Prefabs.CHAR_Undead_ZealousCultist_VBlood,
			/*Prefabs.CHAR_Poloma_VBlood,*/
			Prefabs.CHAR_BatVampire_VBlood,
            Prefabs.CHAR_ArchMage_VBlood,
            Prefabs.CHAR_Cursed_ToadKing_VBlood,
            Prefabs.CHAR_Militia_Guard_VBlood,
            Prefabs.CHAR_Militia_BishopOfDunley_VBlood,
            Prefabs.CHAR_Harpy_Matriarch_VBlood,
            Prefabs.CHAR_ChurchOfLight_Paladin_VBlood,
            Prefabs.CHAR_VHunter_Leader_VBlood,
            Prefabs.CHAR_Bandit_StoneBreaker_VBlood,
			/*Prefabs.CHAR_ChurchOfLight_Cardinal_VBlood,*/
			/*Prefabs.CHAR_WerewolfChieftain_VBlood,*/
			Prefabs.CHAR_Forest_Wolf_VBlood,
            Prefabs.CHAR_Militia_Longbowman_LightArrow_Vblood,
            Prefabs.CHAR_Wendigo_VBlood,
            Prefabs.CHAR_Bandit_Stalker_VBlood,
            Prefabs.CHAR_Gloomrot_RailgunSergeant_VBlood,
			/*Prefabs.CHAR_Gloomrot_Iva_VBlood,*/
			Prefabs.CHAR_Gloomrot_Purifier_VBlood,
            Prefabs.CHAR_Gloomrot_TheProfessor_VBlood,
            Prefabs.CHAR_Gloomrot_Voltage_VBlood,
            Prefabs.CHAR_Undead_CursedSmith_VBlood,
            Prefabs.CHAR_ChurchOfLight_Sommelier_VBlood,
            Prefabs.CHAR_ChurchOfLight_Overseer_VBlood,
            Prefabs.CHAR_Militia_Scribe_VBlood,
            Prefabs.CHAR_Undead_Infiltrator_VBlood,
            Prefabs.CHAR_Militia_Glassblower_VBlood,
            Prefabs.CHAR_Undead_Leader_Vblood,
        };

        public static void Initialize()
        {
            var cleanUpMapIconAction = () =>
            {
                var BossesToRemove = new List<Entity>();
                foreach (var kvp in BossToMapIcon)
                {
                    if (!kvp.Key.Exists() || kvp.Key.Read<Health>().IsDead)
                    {
                        BossesToRemove.Add(kvp.Key);
                    }
                }

                foreach (var boss in BossesToRemove)
                {
                    DisposeBoss(boss);
                }
            };

            var timer = ActionScheduler.RunActionEveryInterval(cleanUpMapIconAction, 1);
            Timers.Add(timer);

            DropTablePrefabEntity = Helper.GetPrefabEntityByPrefabGUID(DropTable);
            Settings = VWorld.Server.GetExistingSystemManaged<ServerGameSettingsSystem>().Settings;

            EventHelper.SetDeathDurabilityLoss(0);
            PointsMod.Globals.CurrencyMultiplier = OpenWorldEventsConfig.Config.CurrencyMultiplierDuringEvents;

            GameEvents.OnUnitHealthChanged += HandleOnUnitHealthChanged;
        }

        public static void Dispose(bool hard = true)
        {
            GameEvents.OnUnitHealthChanged -= HandleOnUnitHealthChanged;
            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }

            Timers.Clear();
            foreach (var kvp in BossToMapIcon)
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

            BossToMapIcon.Clear();
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

        public static void HandleOnUnitHealthChanged(Entity source, Entity eventEntity, StatChangeEvent statChangeEvent, Entity target, PrefabGUID ability)
        {
            if (BossToMapIcon.ContainsKey(target))
            {
                if (source.Has<PlayerCharacter>())
                {
                    var player = PlayerService.GetPlayerFromCharacter(source);
                    var totalChange = (statChangeEvent.Change == 0 ? Math.Abs(statChangeEvent.OriginalChange) : Math.Abs(statChangeEvent.Change));
                    if (PlayerDamageToBoss.ContainsKey(player))
                    {
                        PlayerDamageToBoss[player] += (int)totalChange;
                    }
                    else
                    {
                        PlayerDamageToBoss[player] = (int)totalChange;
                    }
                }
            }
        }

        public static void DisposeLingeringUnits()
        {
            var spawnedEntities = Helper.GetEntitiesByComponentTypes<CanFly, AggroConsumer, Mountable>();
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

        public static void SpawnRandomBoss(int index = -1)
        {
            if (index == -1)
            {
                index = random.Next(BossConfig.Config.BossSpawnLocations.Count);
            }

            var randomPosition = BossConfig.Config.BossSpawnLocations[index].ToFloat3();
            var randomIndex = random.Next(SpawnableBosses.Count);
            var randomBoss = SpawnableBosses[randomIndex];
            SpawnBoss(randomPosition, randomBoss, BossConfig.Config.DefaultBossLevel, 5,
                BossConfig.Config.DefaultBossHP);
        }

        public static void SpawnBoss(float3 spawnPosition, PrefabGUID _prefab, int level = 100, int spawnSnapMode = 5, int hp = -1, bool rooted = false)
        {
            Dispose();
            var boss = new Boss(_prefab);
            boss.IsRooted = rooted;
            if (hp > 0)
            {
                boss.MaxHealth = hp;
            }

            boss.Category = Category;
            boss.Level = level;
            boss.SoftSpawn = true;
            boss.SpawnDelay = BossConfig.Config.BossDelaySeconds;
            UnitFactory.SpawnUnitWithCallback(boss, spawnPosition, (e) =>
            {
                Helper.CreateAndAttachMapIconToEntity(e, Prefabs.MapIcon_DraculasCastle, (mapIcon) =>
                {
                    BossToMapIcon[e] = mapIcon;
                    if (BossToMapIcon.Count == 1)
                    {
                        Initialize();
                    }

                    var dropTableOnDeath = e.Read<DropTableOnDeath>();
                    dropTableOnDeath.MinRange = 1;
                    dropTableOnDeath.MaxRange = 10;
                    e.Write(dropTableOnDeath);
                    var buffer = e.ReadBuffer<DropTableBuffer>();
                    buffer.Clear();

                    buffer.Add(new DropTableBuffer
                    {
                        DropTableGuid = Prefabs.DT_Unit_Demon,
                        DropTrigger = DropTriggerType.OnDeath
                    });

                    var buffer2 = DropTablePrefabEntity.ReadBuffer<DropTableDataBuffer>();
                    buffer2.Clear();
                    foreach (var item in BossConfig.Config.BossItems)
                    {
                        buffer2.Add(new DropTableDataBuffer
                        {
                            DropRate = 1,
                            ItemGuid = item.ItemPrefabGUID,
                            ItemType = DropItemType.Item,
                            Quantity = item.Quantity / (int)Settings.DropTableModifier_General
                        });
                    }

                    //e.Remove<DynamicallyWeakenAttackers>();
                    EventHelper.SetDeathDurabilityLoss(0);
                    Helper.SendSystemMessageToAllClients($"A {"Special Boss".Emphasize()} has spawned and will be available to attack in {Helper.FormatTime(BossConfig.Config.BossDelaySeconds).ToString().Emphasize()}. Kill him and grab the rewards!".Warning());
                    Helper.SendSystemMessageToAllClients($"Look for the {"Dracula's Castle".Emphasize()} icon on the map to find it -- {"durability loss is disabled".White()} during the event.".Warning());
                    var action = () =>
                    {
                        Helper.RemoveBuff(e, Prefabs.Buff_General_VampireMount_Dead);
                        Helper.SendSystemMessageToAllClients($"The {"Special Boss".Emphasize()} is now {"available to kill".White()}!".Warning());
                    };
                    Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, BossConfig.Config.BossDelaySeconds));
                });
            });
        }

        public static void DisposeBoss(Entity boss)
        {
            bool bossSpawned = BossToMapIcon.Count > 0;

            if (BossToMapIcon.TryGetValue(boss, out var mapIcon))
            {
                Helper.DestroyEntity(mapIcon);
                BossToMapIcon.Remove(boss);
                if (boss.Exists())
                {
                    Helper.DestroyEntity(boss);
                }
            }

            if (BossToMapIcon.Count == 0)
            {
                Dispose(false);
                if (bossSpawned)
                {
                    Helper.SendSystemMessageToAllClients($"The {"Special Boss".Emphasize()} is now {"dead".Emphasize()}!".Warning());
                    RewardPlayersBasedOnDamageDealt();
                }
            }
        }

        public static void RewardPlayersBasedOnDamageDealt()
        {
            int totalDamage = 0;
            foreach (var damage in PlayerDamageToBoss.Values)
            {
                totalDamage += damage;
            }

            foreach (var entry in PlayerDamageToBoss)
            {
                double percentage = (entry.Value / (double)totalDamage);
                var reward = (int)(percentage * BossConfig.Config.BossContributionRewards.Quantity);
                if (reward >= 1)
                {
                    PointsManager.AddPointsToPlayer(entry.Key, PointsType.Main, (int)reward);
                    entry.Key.ReceiveMessage($"You did {Math.Round(percentage * 100, 2).ToString().Emphasize()}% of the damage to the boss. You have been awarded {reward.ToString().Emphasize()} {PointsModConfig.Config.MainVirtualCurrencyName} for your participation in killing the boss".White());
                }
            }
        }
    }
}
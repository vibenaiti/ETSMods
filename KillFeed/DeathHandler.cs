using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using ModCore.Events;
using ModCore.Models;
using ModCore;
using ModCore.Helpers;
using ModCore.Data;
using ModCore.Services;
using System.Threading;
using Iced.Intel;
using ProjectM.Shared;
using Stunlock.Core;
using UnityEngine;

namespace KillFeed
{
    public static class DeathHandler
    {
        public static Dictionary<Player, Player> VictimToKiller = new();
        public static Dictionary<Player, DamageRecord> LastPlayerDamagedReceivedToPlayerDamageDealt = new();
        private static Dictionary<PrefabGUID, PrefabGUID> ShardToBuff = new()
        {
            { Prefabs.Item_MagicSource_SoulShard_Dracula, Prefabs.Item_EquipBuff_MagicSource_Soulshard_Dracula },
            { Prefabs.Item_MagicSource_SoulShard_Manticore, Prefabs.Item_EquipBuff_MagicSource_Soulshard_Manticore },
            { Prefabs.Item_MagicSource_SoulShard_Monster, Prefabs.Item_EquipBuff_MagicSource_Soulshard_TheMonster },
            { Prefabs.Item_MagicSource_SoulShard_Solarus, Prefabs.Item_EquipBuff_MagicSource_Soulshard_Solarus },
        };

        private static Dictionary<PrefabGUID, Color> ShardsToTextColor = new()
        {
            { Prefabs.Item_MagicSource_SoulShard_Dracula, ExtendedColor.FireBrick },
            { Prefabs.Item_MagicSource_SoulShard_Manticore, ExtendedColor.DarkViolet },
            { Prefabs.Item_MagicSource_SoulShard_Monster, ExtendedColor.Yellow },
            { Prefabs.Item_MagicSource_SoulShard_Solarus, ExtendedColor.Chartreuse },
        };
        
        private static List<Timer> Timers = new();
        public static void Initialize()
        {
            GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
            GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
            GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
            var action = () =>
            {
                foreach (var player in PlayerService.CharacterCache.Values)
                {
                    if (!player.Character.Exists()) continue;

                    var currentLevel = (int)player.Character.Read<Equipment>().GetFullLevel();
                    if (Globals.PlayerToMaxLevel.TryGetValue(player, out var level))
                    {

                        Globals.PlayerToMaxLevel[player] = Math.Max(currentLevel, level);
                    }
                    else
                    {
                        Globals.PlayerToMaxLevel[player] = currentLevel;
                    }
                }
            };
            Timers.Add(ActionScheduler.RunActionEveryInterval(action, 20));
        }

        public static void Dispose()
        {
            GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
            GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
            GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();
        }

        private static void HandleOnPlayerDamageDealt(Player player, Entity eventEntity, DealDamageEvent dealDamageEvent)
        {
            if (dealDamageEvent.Target.Has<PlayerCharacter>())
            {
                var targetPlayer = PlayerService.GetPlayerFromCharacter(dealDamageEvent.Target);
                LastPlayerDamagedReceivedToPlayerDamageDealt[targetPlayer] = new DamageRecord
                {
                    Player = player,
                    DamageTime = DateTime.Now
                };
            }
        }

        public static void HandleOnPlayerDeath(Player victim, DeathEvent deathEvent)
        {
            if (Globals.HungerGamesPlayers.Contains(victim)) return;
            if (!VictimToKiller.TryGetValue(victim, out var killer))
            {
                if (!LastPlayerDamagedReceivedToPlayerDamageDealt.TryGetValue(victim, out var damageRecord) || !Helper.HasBuff(victim, Prefabs.Buff_InCombat_PvPVampire))
                {
                    return;
                }
                else
                {
                    if ((DateTime.Now - damageRecord.DamageTime).TotalSeconds <= 30)
                    {
                        killer = damageRecord.Player;
                    }
                }
            }
            int killerLevel = (int)killer.Character.Read<Equipment>().GetFullLevel();
            int victimLevel = (int)victim.Character.Read<Equipment>().GetFullLevel();
            if (!Globals.PlayerToMaxLevel.TryGetValue(killer, out var killerHighestLevel))
            {
                Globals.PlayerToMaxLevel[killer] = killerLevel;
                killerHighestLevel = killerLevel;
            }
            if (!Globals.PlayerToMaxLevel.TryGetValue(victim, out var victimHighestLevel))
            {
                Globals.PlayerToMaxLevel[victim] = victimLevel;
                victimHighestLevel = victimLevel;
            }
            killerHighestLevel = Math.Max(killerLevel, killerHighestLevel);
            victimHighestLevel = Math.Max(victimLevel, victimHighestLevel);

            int reward = KillFeedConfig.Config.CurrencyRewardedPerKill;
            if (PointsMod.Globals.EventActive)
            {
                reward *= PointsMod.Globals.CurrencyMultiplier;
            }
            bool isWithinLevelRange = (killerHighestLevel - victimHighestLevel) < KillFeedConfig.Config.GriefKillLevelDifference;
            var killerColor = ExtendedColor.ClanNameColor;
            var victimColor = ExtendedColor.ClanNameColor;
            bool killerHasShard = false;
            bool victimHasShard = false;

            var entities = Helper.GetEntitiesByComponentTypes<Relic, LoseDurabilityOverTime>();

            foreach (var entity in entities)
            {
                var containerEntity = entity.Read<InventoryItem>().ContainerEntity;
                if (ShardToBuff.TryGetValue(entity.GetPrefabGUID(), out var shardBuff))
                {
                    if (Helper.HasBuff(killer, shardBuff))
                    {
                        if (killer.Level >= 80 && (isWithinLevelRange || PointsMod.Globals.EventActive))
                        {
                            var durability = entity.Read<Durability>();
                            durability.Value += Math.Min(KillFeedConfig.Config.ShardDurabilityGainedPerKill, (durability.MaxDurability - durability.Value));
                            entity.Write(durability);
                        }

                        killerHasShard = true;
                        if (ShardsToTextColor.TryGetValue(entity.GetPrefabGUID(), out var color))
                        {
                            killerColor = color;
                        }
                    }
                    if (Helper.HasBuff(victim, shardBuff))
                    {
                        if (ShardsToTextColor.TryGetValue(entity.GetPrefabGUID(), out var color))
                        {
                            victimHasShard = true;
                            victimColor = color;
                        }
                    }
                }
                else if (containerEntity.Exists() && containerEntity.Has<InventoryConnection>())
                {
                    var inventoryOwner = containerEntity.Read<InventoryConnection>().InventoryOwner;
                    if (inventoryOwner.Exists() && inventoryOwner == killer.Character)
                    {
                        if (killer.Level >= 80 && (isWithinLevelRange || PointsMod.Globals.EventActive))
                        {
                            var durability = entity.Read<Durability>();
                            durability.Value += Math.Min(KillFeedConfig.Config.ShardDurabilityGainedPerKill, (durability.MaxDurability - durability.Value));
                            entity.Write(durability);
                        }
                    }
                }
            }

            var locationString = "";
            if (killerHasShard || victimHasShard)
            {
                locationString = " in " + victim.WorldZoneString;
            }

            if ((isWithinLevelRange && victim.IsOnline) || PointsMod.Globals.EventActive)
            {
                
                if (killerHasShard)
                {
                    reward *= KillFeedConfig.Config.BonusCurrencyWhileWearingShard;
                }
                
                entities.Dispose();
                if (StatsRecord.Data.PlayerStats.TryGetValue(victim, out var victimStats) && victimStats.Streak >= 3)
                {
                    // Find the highest killstreak key that is less than or equal to the victim's current streak
                    var maxStreakKey = KillFeedConfig.Config.KillStreakLostTiers
                        .Keys
                        .Where(streak => streak <= victimStats.Streak)
                        .DefaultIfEmpty()
                        .Max();

                    if (KillFeedConfig.Config.KillStreakLostTiers.TryGetValue(maxStreakKey, out var streakInfoVictim))
                    {
                        Helper.SendSystemMessageToAllClients(($"{killer.FullName.Colorify(killerColor)} {streakInfoVictim.TitlePrefix} {(victim.FullName + streakInfoVictim.NameExtension).Colorify(ExtendedColor.ClanNameColor)} " +
                            $"{streakInfoVictim.Title.Colorify(streakInfoVictim.TitleColor)} [{victimStats.Streak}]{locationString}".Colorify(streakInfoVictim.TitleColor)).Size((victimStats.Streak / 5) + 17));
                    }
                }
                else
                {
                    Helper.SendSystemMessageToAllClients($"{killer.FullName.Colorify(killerColor)} has killed {victim.FullName.Colorify(victimColor)}{locationString}");
                }

                StatsRecord.Data.RecordKill(victim, killer); //record kill once we've reported the broken loss streak since that info will be lost once recorded

                if (StatsRecord.Data.PlayerStats.TryGetValue(killer, out var killerStats) && KillFeedConfig.Config.KillStreakGainedTiers.TryGetValue(killerStats.Streak, out var streakInfo))
                {
                    Helper.SendSystemMessageToAllClients(($"{killer.FullName.Colorify(killerColor)} {streakInfo.TitlePrefix}" + $"{streakInfo.Title.Colorify(streakInfo.TitleColor)} [{killerStats.Streak}]{locationString}".Colorify(streakInfo.TitleColor)).Size((killerStats.Streak / 5) + 17));
/*                    if (Helper.BuffPlayer(killer, streakInfo.BuffGuid, out var buffEntity, Helper.NO_DURATION, true))
                    {
                        Helper.ApplyStatModifier(buffEntity, streakInfo.StatBuff, true);
                    }*/
                }

                if (KillFeedConfig.Config.UsePhysicalCurrency)
                {
                    killer.ReceiveMessage($"You have been awarded {reward.ToString().Warning()} {Helper.GetItemName(KillFeedConfig.Config.CurrencyPrefab).Warning()} for your kill".Success());
                    var action = () =>
                    {
                        Helper.AddItemToInventory(killer.Character, KillFeedConfig.Config.CurrencyPrefab, reward, out var itemEntity);
                    };
                    ActionScheduler.RunActionOnceAfterFrames(action, 3);
                }
                else
                {
                    //killer.ReceiveMessage($"You have been awarded {KillFeedConfig.Config.CurrencyRewardedPerKill.ToString().Warning()} {KillFeedConfig.Config.VirtualCurrencyName.Warning()} for your kill".Success());
                    //PointsManager.AddPointsToPlayer(killer, PointsType.Main, KillFeedConfig.Config.CurrencyRewardedPerKill);
                }
            }
            else
            {
                if (victim.IsOnline)
                {
                    Helper.SendSystemMessageToAllClients($"{killer.FullName.Colorify(killerColor)} ({killerHighestLevel}) {"grief killed".Error()} {victim.FullName.Colorify(victimColor)} ({victimHighestLevel}){locationString}");
                }
            }

            
            VictimToKiller.Remove(victim);
        }

        public static void HandleOnPlayerDowned(Player player, Entity killer)
        {
            if (Helper.HasBuff(player, Prefabs.Buff_InCombat_PvPVampire))
            {
                if (LastPlayerDamagedReceivedToPlayerDamageDealt.TryGetValue(player, out var damageRecord))
                {
                    if ((DateTime.Now - damageRecord.DamageTime).TotalSeconds <= 30)
                    {
                        VictimToKiller[player] = damageRecord.Player;
                    }
                    else
                    {
                        VictimToKiller.Remove(player);
                    }
                }
                else
                {
                    VictimToKiller.Remove(player);
                }
            }
            else
            {
                VictimToKiller.Remove(player);
            }
        }
    }
}

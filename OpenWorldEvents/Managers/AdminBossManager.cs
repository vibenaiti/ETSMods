using ProjectM;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using ModCore;
using ModCore.Data;
using ModCore.Helpers;
using ModCore.Services;
using System.Threading;
using ModCore.Events;
using ModCore.Models;
using ModCore.Factories;
using System;
using ProjectM.Behaviours;
using Stunlock.Core;

namespace OpenWorldEvents.Managers
{
    public static class AdminBossManager
    {
        private static List<Timer> Timers = new();
        public static Dictionary<Player, Entity> AdminBossToMapIcon = new();
        public static Dictionary<Player, AdminBoss> PlayerToBossInfo = new();
        private static Dictionary<Player, int> PlayerDamageToAdminBoss = new();
        public static bool HasInitialized = false;
        public static void Initialize()
        {
            if (!HasInitialized)
            {
                GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
                GameEvents.OnPlayerHealthChanged += HandleOnPlayerHealthChanged;
                GameEvents.OnPlayerBuffRemoved += HandleOnPlayerBuffRemoved;
                GameEvents.OnPlayerReset += HandleOnPlayerReset;
                EventHelper.SetDeathDurabilityLoss(0);
                PointsMod.Globals.CurrencyMultiplier = OpenWorldEventsConfig.Config.CurrencyMultiplierDuringEvents;

                HasInitialized = true;
            }
        }

        /// <summary>Cleanly exits boss mode for a single player, removing all boss buffs.</summary>
        public static bool ExitAdminBossMode(Player player)
        {
            if (!PlayerToBossInfo.TryGetValue(player, out var bossData))
                return false;

            PlayerToBossInfo.Remove(player);
            Helper.RemoveBuff(player, Helper.CustomBuff1);
            RemoveShapeshiftBuff(player, bossData);
            Helper.MakePlayerCcDefault(player);
            return true;
        }

        private static void HandleOnPlayerBuffRemoved(Player player, Entity buffEntity, PrefabGUID prefabGUID)
        {
            // If the shapeshift buff was removed while the player is still in boss mode, re-apply it.
            // (PlayerToBossInfo is already cleared in ExitAdminBossMode before RemoveShapeshiftBuff,
            //  so this only fires when the player tries to exit the form manually.)
            if (!PlayerToBossInfo.TryGetValue(player, out var bossData)) return;
            if (!bossData.ShapeshiftBuffGUID.HasValue) return;
            if (prefabGUID != bossData.ShapeshiftBuffGUID.Value) return;

            Timers.Add(ActionScheduler.RunActionOnceAfterFrames(() =>
                Helper.BuffPlayer(player, bossData.ShapeshiftBuffGUID.Value, out _, Helper.NO_DURATION), 1));
        }

        private static void HandleOnPlayerReset(Player player)
        {
            // .r or any Reset() call exits boss mode cleanly
            if (PlayerToBossInfo.ContainsKey(player))
            {
                ExitAdminBossMode(player);
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

            foreach (var kvp in AdminBossToMapIcon)
            {
                if (kvp.Value.Exists())
                {
                    Helper.DestroyEntity(kvp.Value);
                }
            }
            GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
            GameEvents.OnPlayerHealthChanged -= HandleOnPlayerHealthChanged;
            GameEvents.OnPlayerBuffRemoved -= HandleOnPlayerBuffRemoved;
            GameEvents.OnPlayerReset -= HandleOnPlayerReset;
            RestorePlayersToDefault();
            PlayerDamageToAdminBoss.Clear();
            PlayerToBossInfo.Clear();
            HasInitialized = false;
            PointsMod.Globals.CurrencyMultiplier = 1;

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

        public static void RestorePlayersToDefault()
        {
            var buffs = Helper.GetEntitiesByComponentTypes<Buff, NameableInteractable>();
            foreach (var buff in buffs)
            {
                var owner = PlayerService.GetPlayerFromCharacter(buff.Read<EntityOwner>().Owner);
                Helper.MakePlayerCcDefault(owner);
                Helper.DestroyBuff(buff);
            }

            // Remove shapeshift buffs for all players currently in boss mode
            foreach (var kvp in PlayerToBossInfo)
            {
                RemoveShapeshiftBuff(kvp.Key, kvp.Value);
            }
        }

        private static void RemoveShapeshiftBuff(Player player, AdminBoss bossData)
        {
            if (bossData.ShapeshiftBuffGUID.HasValue)
            {
                Helper.RemoveBuff(player, bossData.ShapeshiftBuffGUID.Value);
            }
        }

        public static void HandleOnPlayerDowned(Player player, Entity killer)
        {
            if (AdminBossToMapIcon.ContainsKey(player))
            {
                Helper.SendSystemMessageToAllClients("The admin boss was defeated!");
            }
        }

        public static void HandleOnPlayerHealthChanged(Entity source, Entity eventEntity, StatChangeEvent statChangeEvent, Player target, PrefabGUID ability)
        {
            if (AdminBossToMapIcon.ContainsKey(target))
            {
                if (source.Has<PlayerCharacter>())
                {
                    var player = PlayerService.GetPlayerFromCharacter(source);
                    var totalChange = (statChangeEvent.Change == 0 ? Math.Abs(statChangeEvent.OriginalChange) : Math.Abs(statChangeEvent.Change));
                    if (PlayerDamageToAdminBoss.ContainsKey(player))
                    {
                        PlayerDamageToAdminBoss[player] += (int)totalChange;
                    }
                    else
                    {
                        PlayerDamageToAdminBoss[player] = (int)totalChange;
                    }
                }
            }
        }

        public static void RewardPlayersBasedOnDamageDealt(AdminBoss adminBossData)
        {
            int totalDamage = 0;
            foreach (var damage in PlayerDamageToAdminBoss.Values)
            {
                totalDamage += damage;
            }

            foreach (var entry in PlayerDamageToAdminBoss)
            {
                double percentage = (entry.Value / (double)totalDamage);
                
                var reward = (int)(percentage * adminBossData.BossContributionRewards.Quantity);
                if (reward >= 1)
                {
                    Helper.AddItemToInventory(entry.Key, adminBossData.BossContributionRewards.ItemPrefabGUID, reward, out var itemEntity);
                    entry.Key.ReceiveMessage($"You did {Math.Round(percentage * 100, 2).ToString().Emphasize()}% of the damage to the boss. You have been awarded {reward.ToString().Emphasize()} {Helper.GetItemName(adminBossData.BossContributionRewards.ItemPrefabGUID)} for your participation in killing {adminBossData.BossName}".White());
                }
            }
        }

        public static bool EnterAdminBossMode(Player player, string bossMode)
        {
            if (AdminBossConfig.Config.AdminBosses.TryGetValue(bossMode, out var bossData))
            {
                // Clean up any previous boss mode for this player
                if (PlayerToBossInfo.TryGetValue(player, out var previousBoss))
                {
                    RemoveShapeshiftBuff(player, previousBoss);
                }

                PlayerToBossInfo[player] = bossData;
                Helper.RemoveBuff(player, Helper.CustomBuff1);

                // Apply shapeshift buff first (visual transform), then stat buff + ability override a few frames later
                if (bossData.ShapeshiftBuffGUID.HasValue)
                {
                    Helper.RemoveBuff(player, bossData.ShapeshiftBuffGUID.Value);
                    Helper.BuffPlayer(player, bossData.ShapeshiftBuffGUID.Value, out _, Helper.NO_DURATION);
                }

                var action = () =>
                {
                    if (Helper.BuffPlayer(player, Helper.CustomBuff1, out var buffEntity))
                    {
                        UnitFactory.AddCategory(buffEntity, "adminboss");
                        if (bossData.CCImmune)
                        {
                            Helper.MakePlayerCcImmune(player);
                        }
                        if (bossData.SpellPower != -1)
                        {
                            Helper.SetStatThroughBuff(buffEntity, UnitStatType.SpellPower, bossData.SpellPower);
                        }
                        if (bossData.PhysicalPower != -1)
                        {
                            Helper.SetStatThroughBuff(buffEntity, UnitStatType.PhysicalPower, bossData.PhysicalPower);
                        }
                        if (bossData.Health != -1)
                        {
                            Helper.SetStatThroughBuff(buffEntity, UnitStatType.MaxHealth, bossData.Health);
                        }
                        if (bossData.SpeedModifier != -1)
                        {
                            Helper.SetStatThroughBuff(buffEntity, UnitStatType.MovementSpeed, bossData.SpeedModifier);
                        }
                        if (bossData.AttackSpeedModifier != -1)
                        {
                            Helper.SetStatThroughBuff(buffEntity, UnitStatType.PrimaryAttackSpeed, bossData.AttackSpeedModifier);
                        }
                        if (bossData.CooldownRecoveryRate != -1)
                        {
                            Helper.SetStatThroughBuff(buffEntity, UnitStatType.CooldownRecoveryRate, bossData.CooldownRecoveryRate);
                        }
                        // Apply ability override via ReplaceAbilityOnSlotBuff on the buff entity
                        // When CustomBuff1 is removed, abilities restore automatically
                        bossData.AbilityBar.ApplyChangesHard(buffEntity);
                        Helper.ModifyBuff(buffEntity, BuffModificationTypes.ImmuneToSun | BuffModificationTypes.DisableDynamicCollision);
                        Helper.ModifyBuffAggro(buffEntity, DisableAggroBuffMode.OthersDontAttackTarget);
                        Helper.RemoveBloodDrain(buffEntity);
                    }
                };
                Timers.Add(ActionScheduler.RunActionOnceAfterFrames(action, 3));
                return true;
            }
            return false;
        }

        public static bool StartEvent(Player player, bool hardMode)
        {
            if (HasInitialized) return false;

            Initialize();
            var eventString = hardMode ? "[Greater Event]".Error() : "[Lesser Event]".Success();
            Helper.SendSystemMessageToAllClients($"{eventString} A {"Special Boss".Emphasize()} has spawned! Kill it for special rewards. The player to do the most damage to the boss will get bonus rewards!");
            Helper.SendSystemMessageToAllClients($"Look for the {"Dracula's Castle".Emphasize()} icon on the map to find it -- {"durability loss is disabled".White()} during the event.".Warning());
            Helper.AttachMapIconToPlayer(player, Prefabs.MapIcon_DraculasCastle, (e) => { AdminBossToMapIcon[player] = e; });

            return true;
        }
    }
}

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
using ModCore.Models;
using System.Linq;
using OpenWorldEvents.GameModes;
using ModCore.Events;
using ModCore.Factories;
using Stunlock.Core;

namespace OpenWorldEvents.Managers
{
    public static class BossContestManager
    {
        private static List<Timer> Timers = new();
        public static List<BossContestGameMode> BossContestGameModes = new List<BossContestGameMode>();
        public static Dictionary<Player, float3> PlayersToOriginalLocation = new();
        public static bool HasInitialized = false;
        private static System.Random Random = new System.Random();
        private static PrefabGUID Boss = PrefabGUID.Empty;

        private static List<PrefabGUID> SpawnableBosses = new()
        {
            Prefabs.CHAR_Cursed_Witch_VBlood,
            Prefabs.CHAR_Geomancer_Human_VBlood,
            Prefabs.CHAR_VHunter_Jade_VBlood,
            Prefabs.CHAR_Undead_Priest_VBlood,
            Prefabs.CHAR_Bandit_Tourok_VBlood,
            Prefabs.CHAR_Spider_Queen_VBlood,
            Prefabs.CHAR_Winter_Yeti_VBlood,
            Prefabs.CHAR_Bandit_Chaosarrow_VBlood,
            Prefabs.CHAR_Undead_BishopOfDeath_VBlood,
            Prefabs.CHAR_Militia_Leader_VBlood,
            Prefabs.CHAR_Undead_BishopOfShadows_VBlood,
            Prefabs.CHAR_Bandit_Foreman_VBlood,
            Prefabs.CHAR_Bandit_Frostarrow_VBlood,
            Prefabs.CHAR_Forest_Bear_Dire_Vblood,
            Prefabs.CHAR_Militia_Nun_VBlood,
            Prefabs.CHAR_Bandit_Bomber_VBlood,
            Prefabs.CHAR_Undead_ZealousCultist_VBlood,
            Prefabs.CHAR_Poloma_VBlood,
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
            Prefabs.CHAR_Gloomrot_Iva_VBlood,
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
            Prefabs.CHAR_Vampire_HighLord_VBlood,
            /*Prefabs.CHAR_Vampire_BloodKnight_VBlood,*/
            /*Prefabs.CHAR_Bandit_Fisherman_VBlood,*/
            Prefabs.CHAR_Vampire_IceRanger_VBlood,
            Prefabs.CHAR_VHunter_CastleMan
        };

        public static void Initialize(PrefabGUID boss)
        {
            if (HasInitialized) return;

            Boss = boss;
            for (var i = 0; i < BossContestConfig.Config.ArenaPairs.Count; i++)
            {
                var bossContestArena = new BossContestGameMode();
                bossContestArena.ArenaNumber = i;
                bossContestArena.PlayerLocation1 = BossContestConfig.Config.ArenaPairs[i].Arena1.PlayerLocation.ToFloat3();
                bossContestArena.BossLocation1 = BossContestConfig.Config.ArenaPairs[i].Arena1.BossLocation.ToFloat3();
                bossContestArena.PlayerLocation2 = BossContestConfig.Config.ArenaPairs[i].Arena2.PlayerLocation.ToFloat3();
                bossContestArena.BossLocation2 = BossContestConfig.Config.ArenaPairs[i].Arena2.BossLocation.ToFloat3();
                bossContestArena.IsOccupied = false;
                BossContestGameModes.Add(bossContestArena);
            }
            
            GameEvents.OnPlayerSignedUp += HandleOnPlayerSignedUp;
            GameEvents.OnPlayerUnstuck += HandleOnPlayerUnstuck;
            GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
            HasInitialized = true;

            /*var count = 0;
            foreach (var player in PlayerService.CharacterCache.Values)
            {
                if (player.IsAlive && player.Level >= 80)
                {
                    HandleOnPlayerSignedUp(player);
                    count++;
                    if (count >= 4) break;
                }
            }*/

            var matchPlayersAction = () => 
            {
                MatchPlayers(PlayersToOriginalLocation.Keys.ToList());
                GameEvents.OnPlayerUnstuck -= HandleOnPlayerUnstuck;
                GameEvents.OnPlayerSignedUp -= HandleOnPlayerSignedUp;
                Helper.SendSystemMessageToAllClients($"Sign-ups have ended. The matches have begun!");
            };
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(matchPlayersAction, BossContestConfig.Config.SignupTimeSeconds));
            
            Helper.SendSystemMessageToAllClients($"Boss Contest Starts in {BossContestConfig.Config.SignupTimeSeconds}s! Type .join in chat to sign up.");
        }

        public static void Dispose()
        {
            Boss = PrefabGUID.Empty;
            for (var i = 0; i < BossContestGameModes.Count; i++)
            {
                if (BossContestGameModes[i].MatchActive)
                {
                    EndMatch(i, BossContestGameModes[i].Player1, BossContestGameModes[i].Player2, false);
                }

                BossContestGameModes[i].Dispose();
            }

            foreach (var kvp in PlayersToOriginalLocation)
            {
                Helper.RemoveBuff(kvp.Key, Helper.CustomBuff1);
                kvp.Key.Teleport(kvp.Value);
            }

            PlayersToOriginalLocation.Clear();
            BossContestGameModes.Clear();
            GameEvents.OnPlayerSignedUp -= HandleOnPlayerSignedUp;
            GameEvents.OnPlayerUnstuck -= HandleOnPlayerUnstuck;
            GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
            HasInitialized = false;
            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();
            CleanUpItems();
        }

        public static void HandleOnPlayerPlacedStructure(Player player, Entity entity, BuildTileModelEvent buildTileModelEvent)
        {
            if (PlayersToOriginalLocation.ContainsKey(player))
            {
                if (entity.Exists())
                {
                    entity.Destroy();
                    player.ReceiveMessage("You can't place structures during Boss Contest!".Error());
                }
            }
        }

        public static void CleanUpItems()
        {
            var zone = BossContestConfig.Config.AllArenasRectangleZone.ToRectangleZone();
            var droppedItems = Helper.GetEntitiesByComponentTypes<ItemPickup>(EntityQueryOptions.IncludeDisabledEntities);
            foreach (var item in droppedItems)
            {
                if (zone.Contains(item))
                {
                    Helper.DestroyEntity(item);
                }
            }

            var deathContainers = Helper.GetEntitiesByComponentTypes<PlayerDeathContainer>(EntityQueryOptions.IncludeDisabledEntities);
            foreach (var item in deathContainers)
            {
                if (zone.Contains(item))
                {
                    Helper.DestroyEntity(item);
                }
            }
        }

        public static void HandleOnPlayerUnstuck(Player player, Entity eventEntity)
        {
            if (eventEntity.Exists() && PlayersToOriginalLocation.ContainsKey(player))
            {
                player.ReceiveMessage("You can't unstuck while queued up for a boss contest duel".Error());
                eventEntity.Destroy();
            }
        }

        public static void HandleOnPlayerSignedUp(Player player)
        {
            if (Helper.HasBuff(player, Prefabs.Buff_InCombat_PvPVampire))
            {
                player.ReceiveMessage("You cannot sign up while in PvP combat!".Error());
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
                player.Teleport(BossContestConfig.Config.BossContestWaitingRoom.ToFloat3());
                if (Helper.BuffPlayer(player, Helper.CustomBuff1, out var buffEntity, Helper.NO_DURATION))
                {
                    Helper.ModifyBuff(buffEntity, BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.DisableDynamicCollision);
                }
            }
        }

        public static BossContestGameMode GetAvailableArena()
        {
            return BossContestGameModes.FirstOrDefault(arena => !arena.IsOccupied);
        }

        public static int FindMatchNumberByPlayer(Player player)
        {
            foreach (var gameMode in BossContestGameModes)
            {
                if (gameMode.Player1 == player || gameMode.Player2 == player)
                {
                    return gameMode.ArenaNumber;
                }
            }

            return -1;
        }

        public static void StartMatch(Player player1, Player player2)
        {
            player1.Reset(new Helper.ResetOptions
            {
                RemoveConsumables = false,
                BuffsToIgnore = new()
                {
                    Prefabs.Buff_Monster_FinalStage_Empowered,
                    Prefabs.AB_Vampire_BloodKnight_ThousandSpears_DashBuff,
                    Prefabs.Buff_Cardinal_Shield_Stack,
                    Prefabs.AB_Manticore_Flame_Chaos_Burn_LongDebuff,
                    Prefabs.AB_Interact_UseRelic_Behemoth_Buff,
                    Prefabs.AB_Interact_UseRelic_Manticore_Buff,
                    Prefabs.AB_Interact_UseRelic_Paladin_Buff,
                    Prefabs.AB_Interact_UseRelic_Monster_Buff,
                }
            });
            player2.Reset(new Helper.ResetOptions
            {
                RemoveConsumables = false,
                BuffsToIgnore = new()
                {
                    Prefabs.Buff_Monster_FinalStage_Empowered,
                    Prefabs.AB_Vampire_BloodKnight_ThousandSpears_DashBuff,
                    Prefabs.Buff_Cardinal_Shield_Stack,
                    Prefabs.AB_Manticore_Flame_Chaos_Burn_LongDebuff,
                    Prefabs.AB_Interact_UseRelic_Behemoth_Buff,
                    Prefabs.AB_Interact_UseRelic_Manticore_Buff,
                    Prefabs.AB_Interact_UseRelic_Paladin_Buff,
                    Prefabs.AB_Interact_UseRelic_Monster_Buff,
                }
            });
            player1.ReceiveMessage($"You are against: {player2.FullName.Colorify(ExtendedColor.ClanNameColor)}");
            player2.ReceiveMessage($"You are against: {player1.FullName.Colorify(ExtendedColor.ClanNameColor)}");
            var arena = GetAvailableArena();
            player1.Teleport(arena.PlayerLocation1);
            player2.Teleport(arena.PlayerLocation2);
            Helper.RemoveBuff(player1, Helper.CustomBuff1);
            Helper.RemoveBuff(player2, Helper.CustomBuff1);
            var bossLevel1 = player1.MaxLevel + BossContestConfig.Config.BossLevelAbovePlayers;
            var bossLevel2 = player2.MaxLevel + BossContestConfig.Config.BossLevelAbovePlayers;
            var randomIndex = Random.Next(SpawnableBosses.Count);
            var boss = Boss;
            if (Boss == PrefabGUID.Empty)
            {
                boss = SpawnableBosses[randomIndex];
            }
            
            arena.SpawnBoss(boss, 1, bossLevel1);
            arena.SpawnBoss(boss, 2, bossLevel2);
            arena.Initialize(player1, player2);
        }

        public static void EndMatch(int arenaNumber, Player winner, Player loser, bool naturalEnd = true)
        {
            winner.Reset(new Helper.ResetOptions
            {
                RemoveConsumables = false,
                BuffsToIgnore = new()
                {
                    Prefabs.Buff_Monster_FinalStage_Empowered,
                    Prefabs.AB_Vampire_BloodKnight_ThousandSpears_DashBuff,
                    Prefabs.Buff_Cardinal_Shield_Stack,
                    Prefabs.AB_Manticore_Flame_Chaos_Burn_LongDebuff
                }
            });
            loser.Reset(new Helper.ResetOptions
            {
                RemoveConsumables = false,
                BuffsToIgnore = new()
                {
                    Prefabs.Buff_Monster_FinalStage_Empowered,
                    Prefabs.AB_Vampire_BloodKnight_ThousandSpears_DashBuff,
                    Prefabs.Buff_Cardinal_Shield_Stack,
                    Prefabs.AB_Manticore_Flame_Chaos_Burn_LongDebuff
                }
            });
            if (naturalEnd)
            {
                foreach (var reward in BossContestConfig.Config.WinnerRewards)
                {
                    Helper.AddItemToInventory(winner, reward.ItemPrefabGUID, reward.Quantity, out var itemEntity);
                }

                winner.ReceiveMessage("You have won the boss contest! You will be teleported back in 10 seconds.".Success());
                loser.ReceiveMessage("You have lost the boss contest! You will be teleported back in 10 seconds.".Error());
                Helper.SendSystemMessageToAllClients($"{winner.ToString().Colorify(ExtendedColor.ClanNameColor)} has defeated {loser.ToString().Colorify(ExtendedColor.ClanNameColor)} in their boss contest!");
                BossContestGameModes[arenaNumber].Dispose();
                var action = () =>
                {
                    if (PlayersToOriginalLocation.TryGetValue(winner, out var location))
                    {
                        winner.Teleport(location);
                    }
                    if (PlayersToOriginalLocation.TryGetValue(loser, out location))
                    {
                        loser.Teleport(location);
                    }
                    PlayersToOriginalLocation.Remove(winner);
                    PlayersToOriginalLocation.Remove(loser);

                    bool allMatchesEnded = true;
                    foreach (var bossContestGameMode in BossContestGameModes)
                    {
                        if (bossContestGameMode.MatchActive)
                        {
                            allMatchesEnded = false;
                            break;
                        }
                    }
                    if (allMatchesEnded)
                    {
                        Helper.SendSystemMessageToAllClients($"All boss contests have ended!");
                        Dispose();
                    }
                };
                Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, 10));
            }
            else
            {
                if (PlayersToOriginalLocation.TryGetValue(winner, out var location))
                {
                    winner.Teleport(location);
                }
                if (PlayersToOriginalLocation.TryGetValue(loser, out location))
                {
                    loser.Teleport(location);
                }
                PlayersToOriginalLocation.Remove(winner);
                PlayersToOriginalLocation.Remove(loser);

                bool allMatchesEnded = true;
                foreach (var bossContestGameMode in BossContestGameModes)
                {
                    if (bossContestGameMode.MatchActive)
                    {
                        allMatchesEnded = false;
                        break;
                    }
                }
                BossContestGameModes[arenaNumber].Dispose();
                if (allMatchesEnded)
                {
                    Dispose();
                }
            }
        }

        public static List<Tuple<Player, Player>> MatchPlayers(List<Player> players)
        {
            // Sort players by their level
            players.Sort((x, y) => x.MaxLevel.CompareTo(y.MaxLevel));

            var pairs = new List<Tuple<Player, Player>>();
            var maxPairings = BossContestConfig.Config.ArenaPairs.Count;
            int i = 0;
            while (i < players.Count - 1 && pairs.Count < maxPairings)
            {
                Player current = players[i];
                if (!current.IsOnline)
                {
                    // If the current player is not online, skip to the next player
                    i++;
                    continue;
                }

                Player closest = null;
                int closestIndex = -1;

                for (int j = i + 1; j < players.Count; j++)
                {
                    if (!players[j].IsOnline)
                    {
                        // Skip players who are not online
                        continue;
                    }

                    if (players[j].IsAlliedWith(current)) 
                    {
                        continue;
                    }

                    if (System.Math.Abs(current.MaxLevel - players[j].MaxLevel) <= BossContestConfig.Config.MaxPlayerLevelDifference)
                    {
                        closest = players[j];
                        closestIndex = j;
                        break;
                    }
                }

                if (closest != null)
                {
                    StartMatch(current, closest);
                    pairs.Add(new Tuple<Player, Player>(current, closest)); // Assuming you still want to track the pairs
                                                                            // Remove the paired players from the list
                    players.RemoveAt(closestIndex); // Remove the second player first to maintain correct index for the first player
                    players.RemoveAt(i); // Remove the first player
                }
                else
                {
                    i++; // Move to the next player if no pair is found
                }
            }

            // Remaining players are those who couldn't be paired or were not online
            foreach (var player in players)
            {
                player.ReceiveMessage("Could not find a good match for you. You have been returned to your original location.".Warning());
                if (PlayersToOriginalLocation.TryGetValue(player, out var location))
                {
                    Helper.RemoveBuff(player, Helper.CustomBuff1);
                    player.Teleport(location);
                    PlayersToOriginalLocation.Remove(player);
                }   
            }

            if (pairs.Count == 0)
            {
                Helper.SendSystemMessageToAllClients($"The boss contests have ended prematurely to a lack of valid participants");
                Dispose();
            }
            return pairs;
        }

    }
}

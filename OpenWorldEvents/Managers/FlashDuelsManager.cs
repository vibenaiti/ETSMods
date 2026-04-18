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

namespace OpenWorldEvents.Managers
{
    public static class FlashDuelsManager
    {
        private static List<Timer> Timers = new();
        public static List<FlashDuelGameMode> FlashDuelGameModes = new List<FlashDuelGameMode>();
        public static Dictionary<Player, float3> PlayersToOriginalLocation = new();
        public static HashSet<Player> LootingWinners = new HashSet<Player>();
        public static bool HasInitialized = false;
        private static bool HardMode = false;

        public static void Initialize(bool hardMode = false)
        {
            if (HasInitialized) return;

            HardMode = hardMode;
            
            for (var i = 0; i < DuelsConfig.Config.Arenas.Count; i++)
            {
                var matchmakingArena = new FlashDuelGameMode(hardMode);
                matchmakingArena.ArenaNumber = i;
                matchmakingArena.Location1 = DuelsConfig.Config.Arenas[i].Location1.ToFloat3();
                matchmakingArena.Location2 = DuelsConfig.Config.Arenas[i].Location2.ToFloat3();
                matchmakingArena.IsOccupied = false;
                FlashDuelGameModes.Add(matchmakingArena);
            }
            
            GameEvents.OnPlayerSignedUp += HandleOnPlayerSignedUp;
            GameEvents.OnPlayerRequestedLeave += HandleOnPlayerRequestedLeave;
            GameEvents.OnPlayerUnstuck += HandleOnPlayerUnstuck;
            HasInitialized = true;

/*            if (PlayerService.TryGetPlayerFromString("SailorUranus", out var player))
            {
                HandleOnPlayerSignedUp(player);
            }*/

            var matchPlayersAction = () => 
            {
                MatchPlayers(PlayersToOriginalLocation.Keys.ToList());
                GameEvents.OnPlayerUnstuck -= HandleOnPlayerUnstuck;
                GameEvents.OnPlayerSignedUp -= HandleOnPlayerSignedUp;
                if (DuelsConfig.Config.AnnounceEvent)
                {
                    Helper.SendSystemMessageToAllClients($"Sign-ups have ended. The matches have begun!");
                }
            };
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(matchPlayersAction, DuelsConfig.Config.SignupTimeSeconds));

            var eventString = HardMode ? "Full Loot Flesh Duels -".Error() : "Flash Duels -".Success();
            if (DuelsConfig.Config.AnnounceEvent)
            {
                if (HardMode)
                {
                    Helper.SendSystemMessageToAllClients($"{eventString} Starts in {DuelsConfig.Config.SignupTimeSeconds}s! Type .fullloot in chat to sign up. Winners take all!");
                }
                else
                {
                    Helper.SendSystemMessageToAllClients($"{eventString} Starts in {DuelsConfig.Config.SignupTimeSeconds}s! Type .join in chat to sign up. Winners take all!");
                }
            }
        }

        public static void Dispose()
        {
            for (var i = 0; i < FlashDuelGameModes.Count; i++)
            {
                if (FlashDuelGameModes[i].MatchActive)
                {
                    EndMatch(i, FlashDuelGameModes[i].Player1, FlashDuelGameModes[i].Player2, false);
                }

                FlashDuelGameModes[i].Dispose();
            }

            foreach (var kvp in PlayersToOriginalLocation)
            {
                Helper.RemoveBuff(kvp.Key, Helper.CustomBuff1);
                kvp.Key.Teleport(kvp.Value);
            }

            PlayersToOriginalLocation.Clear();
            FlashDuelGameModes.Clear();
            LootingWinners.Clear();
            GameEvents.OnPlayerSignedUp -= HandleOnPlayerSignedUp;
            GameEvents.OnPlayerRequestedLeave -= HandleOnPlayerRequestedLeave;
            GameEvents.OnPlayerUnstuck -= HandleOnPlayerUnstuck;
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

        public static void CleanUpItems()
        {
            var zone = DuelsConfig.Config.AllArenasRectangleZone.ToRectangleZone();
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
                player.ReceiveMessage("You can't unstuck while queued up for a flash duel".Error());
                eventEntity.Destroy();
            }
        }

        public static void HandleOnPlayerRequestedLeave(Player player)
        {
            if (LootingWinners.Contains(player))
            {
                if (PlayersToOriginalLocation.TryGetValue(player, out var originalLocation))
                {
                    player.Teleport(originalLocation);
                    player.ReceiveMessage("You have been returned to your original location.");
                    LootingWinners.Remove(player);
                    PlayersToOriginalLocation.Remove(player);
                }
            }
        }

        public static void HandleOnPlayerSignedUp(Player player)
        {
            if (Helper.HasBuff(player, Prefabs.Buff_InCombat_PvPVampire))
            {
                player.ReceiveMessage("You cannot sign up while in PvP combat!".Error());
            }
            else if (Helper.HasBuff(player, Prefabs.AB_Shapeshift_Bat_TakeFlight_Buff))
            {
                player.ReceiveMessage("You cannot sign up while in bat form!".Error());
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
                player.Teleport(DuelsConfig.Config.FlashDuelWaitingRoom.ToFloat3());
                if (Helper.BuffPlayer(player, Helper.CustomBuff1, out var buffEntity, Helper.NO_DURATION))
                {
                    Helper.ModifyBuff(buffEntity, BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.ImmuneToSun);
                }
            }
        }

        public static FlashDuelGameMode GetAvailableArena()
        {
            return FlashDuelGameModes.FirstOrDefault(arena => !arena.IsOccupied);
        }

        public static int FindMatchNumberByPlayer(Player player)
        {
            foreach (var gameMode in FlashDuelGameModes)
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
            Helper.RemoveBuff(player1, Helper.CustomBuff1);
            Helper.RemoveBuff(player2, Helper.CustomBuff1);

            Helper.BuffPlayer(player1, Prefabs.Buff_General_Gloomrot_LightningStun, out var buffEntity, 4);
            Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.AbilityCastImpair);
            Helper.BuffPlayer(player2, Prefabs.Buff_General_Gloomrot_LightningStun, out buffEntity, 4);
            Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.AbilityCastImpair);

            player1.Teleport(arena.Location1);
            player2.Teleport(arena.Location2);
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
            if (naturalEnd)
            {
                LootingWinners.Add(winner);
                var rewards = HardMode ? DuelsConfig.Config.GreaterDuelRewards : DuelsConfig.Config.LesserDuelRewards;
                foreach (var item in rewards)
                {
                    Helper.AddItemToInventory(winner, item.ItemPrefabGUID, item.Quantity, out var itemEntity);
                }
                winner.ReceiveMessage($"Type .leave to return to where you were. You will be automatically returned in {DuelsConfig.Config.SignupTimeSeconds} seconds.");
                if (DuelsConfig.Config.AnnounceEvent)
                {
                    Helper.SendSystemMessageToAllClients($"{winner.ToString().Colorify(ExtendedColor.ClanNameColor)} has defeated {loser.ToString().Colorify(ExtendedColor.ClanNameColor)} in their duel!");
                }
                var action = () =>
                {
                    if (PlayersToOriginalLocation.TryGetValue(winner, out var location))
                    {
                        winner.Teleport(location);
                    }
                    LootingWinners.Remove(winner);
                    PlayersToOriginalLocation.Remove(winner);
                    PlayersToOriginalLocation.Remove(loser);

                    FlashDuelGameModes[arenaNumber].Dispose();
                    bool allMatchesEnded = true;
                    foreach (var flashDuelGameMode in FlashDuelGameModes)
                    {
                        if (flashDuelGameMode.MatchActive)
                        {
                            allMatchesEnded = false;
                            break;
                        }
                    }
                    if (allMatchesEnded)
                    {
                        if (DuelsConfig.Config.AnnounceEvent)
                        {
                            Helper.SendSystemMessageToAllClients($"All duels have ended!");
                        }
                        Dispose();
                    }
                };
                Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, DuelsConfig.Config.WinnerLootTimeSeconds));
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
                LootingWinners.Remove(winner);

                FlashDuelGameModes[arenaNumber].Dispose();
            }
        }

        public static List<Tuple<Player, Player>> MatchPlayers(List<Player> players)
        {
            // Sort players by their level
            players.Sort((x, y) => x.MaxLevel.CompareTo(y.MaxLevel));

            var pairs = new List<Tuple<Player, Player>>();
            var maxPairings = DuelsConfig.Config.Arenas.Count;
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

                    if (System.Math.Abs(current.MaxLevel - players[j].MaxLevel) <= DuelsConfig.Config.MaxLevelDifference)
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
                Helper.SendSystemMessageToAllClients($"The duels have ended prematurely to a lack of valid participants");
                Dispose();
            }
            return pairs;
        }

    }
}

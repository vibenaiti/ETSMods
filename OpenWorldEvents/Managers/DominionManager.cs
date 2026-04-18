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
    public static class DominionManager
    {
        private const string Category = "Dominion";
        private static List<Timer> Timers = new();
        private static HashSet<Entity> SpawnedEntities = new();
        private static CircleZone CircleZone;
        private static Dictionary<Entity, int> TeamsToPoints = new();
        private static HashSet<Player> ParticipatingPlayers = new();
        public static bool MatchActive = false;

        public static void Dispose(bool hard = true)
        {            
            foreach (var entity in SpawnedEntities)
            {
                Helper.DestroyEntity(entity);
            }
            SpawnedEntities.Clear();
            ParticipatingPlayers.Clear();
            TeamsToPoints.Clear();
            CircleZone = null;

            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();

            GameEvents.OnPlayerSpecialChat -= HandleOnPlayerRequestedScore;
            MatchActive = false;

            PointsMod.Globals.ActiveEvents.Remove(Category);
            EventHelper.SetDeathDurabilityLoss(OpenWorldEventsConfig.Config.DefaultDurabilityLoss);
            PointsMod.Globals.CurrencyMultiplier = 1;
        }

        public static void EndMatch()
        {
            var winningTeam = TeamsToPoints
            .OrderByDescending(pair => pair.Value)
            .Take(1);

            List<string> winnersStrings = new();
            foreach (var team in winningTeam)
            {
                if (team.Key.Has<ClanTeam>())
                {
                    var winners = Helper.GetClanMembersFromClan(team.Key);
                    foreach (var winner in winners)
                    {
                        if (ParticipatingPlayers.Contains(winner))
                        {
                            winnersStrings.Add(winner.FullNameColored);
                            foreach (var reward in DominionConfig.Config.WinnerRewards)
                            {
                                Helper.AddItemToInventory(winner.Character, reward.ItemPrefabGUID, reward.Quantity, out var itemEntity);
                                winner.ReceiveMessage($"You have been awarded {reward.Quantity.ToString().Emphasize()} {Helper.GetItemName(reward.ItemPrefabGUID).Emphasize()}(s) for winning!".Success());
                            }
                        }
                    }
                    break;
                }
                else
                {
                    var winner = PlayerService.GetPlayerFromCharacter(team.Key);
                    winnersStrings.Add(winner.FullNameColored);
                    foreach (var reward in DominionConfig.Config.WinnerRewards)
                    {
                        Helper.AddItemToInventory(winner.Character, reward.ItemPrefabGUID, reward.Quantity, out var itemEntity);
                        winner.ReceiveMessage($"You have been awarded {reward.Quantity.ToString().Emphasize()} {Helper.GetItemName(reward.ItemPrefabGUID).Emphasize()}(s) for winning!".Success());
                    }
                    break;
                }
            }
            string winningMembersMessage = string.Join(", ", winnersStrings);

            if (winnersStrings.Count == 1)
            {
                Helper.SendSystemMessageToAllClients($"{winningMembersMessage} has won {"Dominion".Colorify(ExtendedColor.Red)}!");
            }
            else
            {
                Helper.SendSystemMessageToAllClients($"{winningMembersMessage} have won {"Dominion".Colorify(ExtendedColor.Red)}!");
            }

            Dispose();
        }

        public static void SpawnRing(float3 pos)
        {
            PointsMod.Globals.CurrencyMultiplier = OpenWorldEventsConfig.Config.CurrencyMultiplierDuringEvents;
            PointsMod.Globals.ActiveEvents.Add(Category);

            if (DominionConfig.Config.DisableDurabilityLossDuringEvent)
            {
                EventHelper.SetDeathDurabilityLoss(0);
            }

            PrefabSpawnerService.SpawnWithCallback(Prefabs.CHAR_TargetDummy_Footman, pos, (e) =>
            {
                CircleZone = new CircleZone(pos, 12.5f);
                SpawnedEntities.Add(e);
                if (Helper.BuffEntity(e, Prefabs.AB_Dracula_SpellStone_CirclingBlood_Buff01, out var buffEntity))
                {
                    Helper.ModifyBuffAggro(buffEntity, DisableAggroBuffMode.OthersDontAttackTarget);
                    buffEntity.Remove<GameplayEventListeners>();
                    if (Helper.BuffEntity(e, Prefabs.Buff_General_HideCorpse, out var buffEntity2))
                    {
                        Helper.ModifyBuff(buffEntity2, BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable);
                        Helper.BuffEntity(e, Prefabs.AB_Scarecrow_Idle_Buff, out var buffEntity3);
                    }
                }

                Helper.CreateAndAttachMapIconToEntity(e, Prefabs.MapIcon_DraculasCastle, (iconEntity) =>
                {
                    SpawnedEntities.Add(iconEntity);
                    var durabilityString = "";
                    if (HorseConfig.Config.DisableDurabilityLossDuringEvent)
                    {
                        durabilityString = $" -- {"durability loss is disabled".White()} during the event.";
                        EventHelper.SetDeathDurabilityLoss(0);
                    }
                        
                    Helper.SendSystemMessageToAllClients($"A {"Special Circle".Emphasize()} has spawned! It will become active in {Helper.FormatTime(DominionConfig.Config.DominionDelaySeconds).Emphasize()} seconds. Spend the most time inside it before it closes to get rewards!".Warning());
                    Helper.SendSystemMessageToAllClients($"Look for the {"Dracula's Castle".Emphasize()} icon on the map to find it{durabilityString}".Warning());
                });
            });


            var activateDominionAction = () =>
            {
                var action = () =>
                {
                    foreach (var player in PlayerService.OnlinePlayersWithCharacters)
                    {
                        if (CircleZone.Contains(player))
                        {
                            ParticipatingPlayers.Add(player);
                            Entity teamEntity;
                            if (player.Clan.Exists())
                            {
                                teamEntity = player.Clan;
                            }
                            else
                            {
                                teamEntity = player.Character;
                            }

                            if (TeamsToPoints.ContainsKey(teamEntity))
                            {
                                TeamsToPoints[teamEntity]++;
                            }
                            else
                            {
                                TeamsToPoints[teamEntity] = 1;
                            }
                        }
                    }
                };
                Timers.Add(ActionScheduler.RunActionEveryInterval(action, 1));

                var endMatchAction = () => EndMatch();
                Timers.Add(ActionScheduler.RunActionOnceAfterDelay(endMatchAction, DominionConfig.Config.DominionDuration));

                MatchActive = true;
                GameEvents.OnPlayerSpecialChat += HandleOnPlayerRequestedScore;

                if (DominionConfig.Config.DominionDuration > 60)
                {
                    var warnEventEndingSoonAction = () =>
                    {
                        Helper.SendSystemMessageToAllClients($"The Dominion event will end in {"60".Emphasize()} seconds!".Warning());
                    };
                    Timers.Add(ActionScheduler.RunActionOnceAfterDelay(warnEventEndingSoonAction, DominionConfig.Config.DominionDuration - 60));
                };
            };

            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(activateDominionAction, DominionConfig.Config.DominionDelaySeconds));

            Helper.SendSystemMessageToAllClients($"The special circle is now active, get inside!".Warning());
        }

        private static void HandleOnPlayerRequestedScore(Player player, string message)
        {
            var top5Teams = TeamsToPoints
            .OrderByDescending(pair => pair.Value)
            .Take(5);
            int index = 1;
            foreach (var team in top5Teams)
            {
                string clanName = "";
                if (team.Key.Has<ClanTeam>())
                {
                    clanName = team.Key.Read<ClanTeam>().Name.ToString();
                }
                else
                {
                    var teamPlayer = PlayerService.GetPlayerFromCharacter(team.Key);
                    clanName = teamPlayer.Name;
                }
                player.ReceiveMessage($"{index}: {clanName} - {team.Value}");
            }
            
        }

        public static void SpawnRingAtRandomLocation()
        {
            var index = random.Next(DominionConfig.Config.DominionSpawnLocations.Count);
            var pos = DominionConfig.Config.DominionSpawnLocations[index].ToFloat3();
            SpawnRing(pos);
        }
    }
}

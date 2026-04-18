using ProjectM;
using System.Collections.Generic;
using System.Threading;
using ModCore.Events;
using ModCore.Models;
using ModCore.Services;
using ModCore;
using Unity.Entities;
using ModCore.Data;
using ModCore.Helpers;
using ProjectM.CastleBuilding;
using Unity.Transforms;
using ProjectM.Network;
using Stunlock.Core;
using System.Diagnostics;
using static ModCore.Helpers.Helper;
using ProjectM.Shared;
using ModCore.Factories;

namespace RaidConfig.Managers
{
    public static class RaidManager
    {
        public static Dictionary<Entity, Player> CastleHeartToLastDamagingCharacter = new();
        private static Dictionary<Player, Entity> RecentAttackers = new();
        private static bool Initialized = false;
        private static List<Timer> Timers = new();
        public static void Initialize()
        {
            if (!Initialized)
            {
                GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
                GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
                GameEvents.OnPlayerInteractedWithCastleHeart += HandleOnPlayerInteractedWithCastleHeart;
                Initialized = true;
                var action = () => ApplyPoisonToRats();
                Timers.Add(ActionScheduler.RunActionEveryInterval(action, 2));
            }
        }

        public static void Dispose()
        {
            GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
            GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
            GameEvents.OnPlayerInteractedWithCastleHeart -= HandleOnPlayerInteractedWithCastleHeart;
            Initialized = false;
            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();
            RecentAttackers.Clear();
        }

        private static void ApplyPoisonToRats()
        {
            var heartLookup = VWorld.Server.EntityManager.GetComponentLookup<CastleHeart>();
            var tilePositionLookup = VWorld.Server.EntityManager.GetComponentLookup<TilePosition>();
            var castleTerritoryLookup = VWorld.Server.EntityManager.GetComponentLookup<CastleTerritory>();
            var teamLookup = VWorld.Server.EntityManager.GetComponentLookup<Team>();
            
            foreach (var player in PlayerService.OnlinePlayersWithCharacters)
            {
                bool appliedToxic = false;
                if (CastleTerritoryCache.TryGetCastleTerritory(tilePositionLookup, player.Character, out var territoryEntity))
                {
                    var heart = castleTerritoryLookup[territoryEntity].CastleHeart;
                    TerritoryAlignment territoryAlignment;
                    if (heart.Exists())
                    {
                        if (Team.IsAllies(teamLookup[heart], teamLookup[player.Character]))
                        {
                            territoryAlignment = TerritoryAlignment.Friendly;
                        }
                        else
                        {
                            territoryAlignment = TerritoryAlignment.Enemy;
                        }
                    }
                    else
                    {
                        territoryAlignment = TerritoryAlignment.Neutral;
                    }
                    if (territoryAlignment == Helper.TerritoryAlignment.Enemy && !player.HasBuff(Prefabs.AB_Shapeshift_Bat_TakeFlight_Buff))
                    {
                        if (Helper.IsCastleBeingRaided(heartLookup, heart))
                        {
                            if (CastleHeartToLastDamagingCharacter.TryGetValue(heart, out var lastDamagingPlayer))
                            {
                                if (teamLookup[player.Character].Value != teamLookup[lastDamagingPlayer.Character].Value)
                                {
                                    if (!player.HasBuff(Prefabs.Admin_Observe_Invisible_Buff) && !player.HasBuff(Prefabs.Admin_Observe_Ghost_Buff))
                                    {
                                        var owner = PlayerService.GetPlayerFromUser(heart.Read<UserOwner>().Owner._Entity);
                                        var clanMembersOfRaidedBase = owner.GetClanMembers();
                                        bool offlineRaid = true;
                                        foreach (var clanMember in clanMembersOfRaidedBase)
                                        {
                                            if (clanMember.IsOnline)
                                            {
                                                offlineRaid = false;
                                                break;
                                            }
                                        }

                                        if (!offlineRaid && Helper.BuffPlayer(player, Prefabs.AB_Mutant_Spitter_PoisonRain_PoisonDebuff, out var buffEntity, Helper.NO_DURATION))
                                        {
                                            if (!buffEntity.Has<CanFly>())
                                            {
                                                player.ReceiveMessage($"You aren't allowed to be here, this castle is being raided by {lastDamagingPlayer.FullName.Colorify(ExtendedColor.ClanNameColor)}. Leave the territory before you die!".Warning());
                                            }
                                            bool isGolem = false;
                                            if (player.HasBuff(Prefabs.AB_Shapeshift_Golem_T02_Buff))
                                            {
                                                isGolem = true;
                                            }
                                            var buffer = buffEntity.ReadBuffer<DealDamageOnGameplayEvent>();
                                            for (var i = 0; i < buffer.Length; i++)
                                            {
                                                var dealDamageOnGameplayEvent = buffer[i];
                                                if (!isGolem)
                                                {
                                                    dealDamageOnGameplayEvent.Parameters.RawDamagePercent = 0.075f;
                                                }
                                                else
                                                {
                                                    dealDamageOnGameplayEvent.Parameters.RawDamagePercent = 0.3f;
                                                }
                                                dealDamageOnGameplayEvent.Parameters.DealDamageFlags &= (int)~DealDamageFlag.IsDoT;
                                                buffer[i] = dealDamageOnGameplayEvent;
                                            }
                                            buffEntity.Add<CanFly>();
                                            appliedToxic = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            CastleHeartToLastDamagingCharacter.Remove(heart);
                        }
                    }
                }
                if (!appliedToxic)
                {
                    if (Helper.TryGetBuff(player, Prefabs.AB_Mutant_Spitter_PoisonRain_PoisonDebuff, out var buffEntity) && buffEntity.Has<CanFly>())
                    {
                        Helper.DestroyBuff(buffEntity);
                    }
                }
            }
        }

        private static void HandleOnPlayerDamageDealt(Player player, Entity eventEntity, DealDamageEvent dealDamageEvent)
        {
            if (eventEntity.Exists())
            {
                if (dealDamageEvent.MaterialModifiers.StoneStructure > 0 && dealDamageEvent.Target.Has<CastleHeartConnection>())
                {
                    var castleHeart = dealDamageEvent.Target.Read<CastleHeartConnection>().CastleHeartEntity._Entity;
                    if (castleHeart.Exists())
                    {
                        CastleHeartToLastDamagingCharacter[castleHeart] = player;
                        var owner = castleHeart.Read<UserOwner>().Owner._Entity;
                        if (owner.Exists())
                        {
                            var ownerPlayer = PlayerService.GetPlayerFromUser(owner);
                            var clanMembers = ownerPlayer.GetClanMembers();
                            bool isOffline = true;
                            foreach (var clanMember in clanMembers)
                            {
                                if (clanMember.IsOnline)
                                {
                                    isOffline = false;
                                    break;
                                }
                            }
                            if (isOffline)
                            {
                                if (Helper.IsRaidHour())
                                {
                                    if (!RecentAttackers.TryGetValue(player, out var lastAttackedHeart) || lastAttackedHeart != castleHeart)
                                    {
                                        RecentAttackers[player] = castleHeart;
                                        Helper.SendSystemMessageToAllClients($"{player.FullNameColored} is offline raiding {ownerPlayer.FullNameColored}'s base in {player.WorldZoneString}!".Error());
                                        var action = () =>
                                        {
                                            RecentAttackers.Remove(player);
                                        };
                                        Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, 90));
                                    }
                                }
                            }
                        }
                        
                    }
                }
            }
        }

        private static void HandleOnPlayerBuffed(Player player, Entity buffEntity, PrefabGUID prefabGUID)
        {
            if (prefabGUID == Prefabs.AB_Interact_UseResearchstation)
            {
                var spellTarget = buffEntity.Read<SpellTarget>();
                var structure = spellTarget.Target._Entity;
                if (structure.Exists())
                {
                    if (!Team.IsAllies(structure.Read<Team>(), player.Team))
                    {
                        Helper.DestroyBuff(buffEntity);
                        player.ReceiveMessage("You cannot open a research station that does not belong to you!".Error());
                    }
                }
            }
            else if (prefabGUID == Prefabs.AB_Interact_UseSpellSchoolPassiveStation)
            {
                var spellTarget = buffEntity.Read<SpellTarget>();
                var structure = spellTarget.Target._Entity;
                if (structure.Exists())
                {
                    if (!Team.IsAllies(structure.Read<Team>(), player.Team))
                    {
                        Helper.DestroyBuff(buffEntity);
                        player.ReceiveMessage("You cannot open a research station that does not belong to you!".Error());
                    }
                }
            }
        }

        private static void HandleOnPlayerInteractedWithCastleHeart(Player player, Entity eventEntity, CastleHeartInteractEvent castleHeartInteractEvent)
        {
            if (!eventEntity.Exists()) return;

            if (Helper.TryGetEntityFromNetworkId(castleHeartInteractEvent.CastleHeart, out var heart))
            {
                if (castleHeartInteractEvent.EventType == CastleHeartInteractEventType.Raid)
                {
                    CastleHeartToLastDamagingCharacter[heart] = player;
                }
            }
        }
    }
}
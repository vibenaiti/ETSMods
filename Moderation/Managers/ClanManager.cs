using ProjectM;
using Unity.Entities;
using ModCore.Events;
using ModCore.Models;
using ModCore;
using ModCore.Data;
using ModCore.Helpers;
using ProjectM.Network;
using ModCore.Services;
using System.Collections.Generic;
using System;
using static ProjectM.Network.ClanEvents_Client;
using Unity.Collections;

namespace Moderation.Managers
{
    public static class ClanManager
    {
        private static Dictionary<Player, LastClan> PlayerToLastClan = new();
        public static void Initialize()
        {
            GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
            GameEvents.OnPlayerRespondedToClanInvite += HandleOnPlayerRespondedToClanInvite;
            GameEvents.OnPlayerInteractedWithCastleHeart += HandleOnPlayerInteractedWithCastleHeart;
            GameEvents.OnPlayerLeftClan += HandleOnPlayerLeftClan;
            GameEvents.OnPlayerKickedFromClan += HandleOnPlayerKickedFromClan;
            GameEvents.OnPlayerCreatedClan += HandleOnPlayerCreatedClan;
        }

        public static void Dispose()
        {
            GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
            GameEvents.OnPlayerRespondedToClanInvite -= HandleOnPlayerRespondedToClanInvite;
            GameEvents.OnPlayerInteractedWithCastleHeart -= HandleOnPlayerInteractedWithCastleHeart;
            GameEvents.OnPlayerLeftClan -= HandleOnPlayerLeftClan;
            GameEvents.OnPlayerKickedFromClan -= HandleOnPlayerKickedFromClan;
            GameEvents.OnPlayerCreatedClan -= HandleOnPlayerCreatedClan;
        }

        public static void HandleOnPlayerPlacedStructure(Player player, Entity eventEntity, BuildTileModelEvent buildTileModelEvent)
        {
            if (buildTileModelEvent.PrefabGuid == Prefabs.TM_BloodFountain_CastleHeart)
            {
                var clanMates = player.GetClanMembers();
                foreach (var clanMember in clanMates)
                {
                    if (clanMember.User.Read<UserHeartCount>().HeartCount > 0)
                    {
                        eventEntity.Destroy();
                        player.ReceiveMessage("A clan can only have one heart placed at a time!".Error());
                        break;
                    }
                }
            }
        }

        public static void HandleOnPlayerRespondedToClanInvite(Player player, Entity eventEntity, ClanEvents_Client.ClanInviteResponse clanInviteResponse)
        {
            if (clanInviteResponse.Response == InviteRequestResponse.Accept)
            {
                if (PlayerToLastClan.TryGetValue(player, out var lastClan))
                {
                    if (clanInviteResponse.ClanId != NetworkId.CreateNormal(lastClan.ClanNetworkId, (byte)1))
                    {
                        TimeSpan timeSinceLeft = DateTime.Now - lastClan.DateLeftClan; // Calculate the time since the player left the clan
                        double remainingCooldown = ModerationConfig.Config.ClanJoinCooldownMinutes - timeSinceLeft.TotalMinutes; // Calculate remaining cooldown in minutes

                        if (remainingCooldown > 0)
                        {
                            player.ReceiveMessage($"You can't join a new clan for {Math.Ceiling(remainingCooldown)} minutes. Reach out to an admin to bypass this cooldown".Error());
                            eventEntity.Destroy();
                            return;
                        }
                    }
                }

                if (Helper.TryGetEntityFromNetworkId(clanInviteResponse.ClanId, out var clanEntity)) 
                {
                    var clanMembers = Helper.GetClanMembersFromClan(clanEntity);
                    var playerJoiningClanHasHeart = player.User.Read<UserHeartCount>().HeartCount > 0;
                    var clanHasHeart = false;
                    foreach (var clanMember in clanMembers)
                    {
                        if (clanMember.User.Read<UserHeartCount>().HeartCount > 0)
                        {
                            clanHasHeart = true;
                            break;
                        }
                    }
                    if (playerJoiningClanHasHeart && clanHasHeart)
                    {
                        player.ReceiveMessage("Because you already have a heart, your new clan is now over the castle heart limit. Fix as soon as possible, admins have been notified");
                        Helper.NotifyAllAdmins($"{player.FullName} joined {clanMembers[0].FullName}'s clan, resulting in more than one heart");
                        return;
                    }
                }
            }
        }

        public static void HandleOnPlayerCreatedClan(Player player, Entity eventEntity, ClanEvents_Client.CreateClan_Request createClanEvent)
        {
            if (ModerationModDataStorage.Data.PlayersToLastClan.TryGetValue(player.SteamID, out var lastClan))
            {
                TimeSpan timeSinceLeft = DateTime.Now - lastClan.DateLeftClan; // Calculate the time since the player left the clan
                double remainingCooldown = ModerationConfig.Config.ClanJoinCooldownMinutes - timeSinceLeft.TotalMinutes; // Calculate remaining cooldown in minutes

                if (remainingCooldown > 0)
                {
                    player.ReceiveMessage($"You can't join a new clan for {Math.Ceiling(remainingCooldown)} minutes. Reach out to an admin to bypass this cooldown".Error());
                    eventEntity.Destroy();
                    return;
                }
            }
        }

        public static void HandleOnPlayerLeftClan(Player player, Entity eventEntity, ClanEvents_Client.LeaveClan leaveClanEvent)
        {
            ModerationModDataStorage.Data.PlayersToLastClan[player.SteamID] = new LastClan
            {
                ClanNetworkId = leaveClanEvent.ClanId.Normal_Index,
                DateLeftClan = DateTime.Now,
            };
            player.ReceiveMessage($"You must wait {ModerationConfig.Config.ClanJoinCooldownMinutes} minutes to join a different clan. Either rejoin your original clan or reach out to an admin to skip this cooldown");
        }

        public static void HandleOnPlayerKickedFromClan(Player kickingPlayer, Entity eventEntity, ClanEvents_Client.Kick_Request clanKickEvent)
        {
            foreach (var kickedPlayer in PlayerService.CharacterCache.Values)
            {
                if (kickedPlayer.User.Read<User>().Index == clanKickEvent.TargetUserIndex)
                {
                    ModerationModDataStorage.Data.PlayersToLastClan[kickedPlayer.SteamID] = new LastClan
                    {
                        ClanNetworkId = kickingPlayer.Clan.Read<NetworkId>().Normal_Index,
                        DateLeftClan = DateTime.Now,
                    };
                    break;
                }
            }
        }

        public static void HandleOnPlayerInteractedWithCastleHeart(Player player, Entity eventEntity, CastleHeartInteractEvent castleHeartInteractEvent)
        {
            if (!eventEntity.Exists()) return;

            if (castleHeartInteractEvent.EventType == CastleHeartInteractEventType.Claim)
            {
                if (Helper.TryGetEntityFromNetworkId(castleHeartInteractEvent.CastleHeart, out var heartEntity))
                {
                    bool overLimit = false;
                    var clanMembers = player.GetClanMembers();
                    foreach (var clanMember in clanMembers)
                    {
                        if (clanMember.User.Read<UserHeartCount>().HeartCount > 0)
                        {
                            overLimit = true;
                            break;
                        }
                    }

                    var originalOwnerEntity = heartEntity.Read<UserOwner>().Owner._Entity;
                    if (originalOwnerEntity.Exists())
                    {
                        var originalOwner = PlayerService.GetPlayerFromUser(originalOwnerEntity);
                        if (overLimit)
                        {
                            Helper.NotifyAllAdmins($"{player.FullName} has gone over the clan heart limit by claiming a heart from {originalOwner.FullName}");
                        }

                    }
                    else
                    {
                        Helper.NotifyAllAdmins($"{player.FullName} has gone over the clan heart limit by claiming a heart");
                    }
                }
            }
        }
    }
}

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
using Unity.Mathematics;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;

namespace RaidConfig
{
    public static class GolemNotificationManager
    {
        private static List<Timer> Timers = new();
        public static void Initialize()
        {
            GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
        }

        public static void Dispose()
        {
            GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
        }

        private static void HandleOnPlayerPlacedStructure(Player player, Entity eventEntity, BuildTileModelEvent buildTileModelEvent)
        {
            if (eventEntity.Exists())
            {
                if (buildTileModelEvent.PrefabGuid == Prefabs.TM_Siege_Structure_T02)
                {
                    foreach (var onlinePlayer in PlayerService.OnlinePlayersWithUsers)
                    {
                        if (onlinePlayer.IsAdmin)
                        {
                            onlinePlayer.ReceiveMessage($"{player.FullName.Colorify(ExtendedColor.ClanNameColor)} placed a golem.".White());
                        }
                    }
                }
            }
        }
    }
}
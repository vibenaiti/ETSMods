using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using ModCore.Events;
using ModCore.Listeners;
using ModCore.Models;
using ModCore;
using ModCore.Helpers;
using ModCore.Data;
using ModCore.Services;
using System.Threading;

namespace DebugMod.Managers
{
    public static class AutoReviveManager
    {
        public static HashSet<Player> AutoRevivePlayers = new();

        public static void Initialize()
        {
            GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
            GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
        }

        public static void Dispose()
        {
            GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
            GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
            AutoRevivePlayers.Clear();
        }

        private static void HandleOnPlayerDowned(Player player, Entity killer)
        {
            if (AutoRevivePlayers.Contains(player))
            {
                player.Reset();
            }
        }

        private static void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
        {
            if (AutoRevivePlayers.Contains(player))
            {
                Helper.RevivePlayer(player);
            }
        }
    }
}

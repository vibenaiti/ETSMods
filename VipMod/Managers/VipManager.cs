using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModCore.Events;
using ModCore.Models;
using ModCore.Services;
using Stunlock.Network;
using ModCore.Helpers;
using Il2CppSystem;
using HarmonyLib;
using ProjectM.Auth;
using ProjectM.Shared;
using System.Runtime.InteropServices;
using ProjectM.Network;
using Unity.Entities;
using System.Reflection;
using ModCore;

namespace VipMod.Managers
{
    public static class VipManager
    {
        public static HashSet<Player> ConnectedVips = new HashSet<Player>();
        public static HashSet<Player> ConnectedNormals = new HashSet<Player>();
        public static void Initialize()
        {
            GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
            GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
            foreach (var player in PlayerService.OnlinePlayersWithUsers)
            {
                if (player.IsAdmin && !VipModConfig.Config.DebugSkipAdminCheck) continue;

                if (VipModConfig.Config.Vips.ContainsKey(player.SteamID))
                {
                    ConnectedVips.Add(player);
                }
                else
                {
                    ConnectedNormals.Add(player);
                }
            }
        }

        public static void Dispose()
        {
            GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
            GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
            ConnectedVips.Clear();
            ConnectedNormals.Clear();
        }

        public static void HandleOnPlayerDisconnected(Player player)
        {
            ConnectedVips.Remove(player);
            ConnectedNormals.Remove(player);
        }

        public static void HandleOnPlayerConnected(Player player)
        {
            if (VipModConfig.Config.Vips.ContainsKey(player.SteamID))
            {
                PointsManager.TryAwardDailyLoginPoints(player, VipModConfig.Config.VipPointsPerDailyLogin);
            }
            else if (VipModConfig.Config.SuperVips.ContainsKey(player.SteamID))
            {
                PointsManager.TryAwardDailyLoginPoints(player, VipModConfig.Config.SuperVipPointsPerDailyLogin);
            }
            else
            {
                PointsManager.TryAwardDailyLoginPoints(player, VipModConfig.Config.PointsPerDailyLogin);
            }
        }
    }

    // ⚠ TryAuthenticate signature changed in v1.1.11 - using OnUserConnected instead
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public static class ServerBootstrapSystemPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            // Use the event listener approach instead of method patching
            // PlayerService will track connections via GameEvents
        }

        private static void CheckIfPlayerMayJoin(Player player)
        {
            if (!player.User.Exists())
            {
                var action = () => CheckIfPlayerMayJoin(player);
                ActionScheduler.RunActionOnceAfterFrames(action, 2);
                return;
            }
            var isVip = VipModConfig.Config.Vips.ContainsKey(player.SteamID);
            if (player.IsAdmin && !VipModConfig.Config.DebugSkipAdminCheck) return;

            if (isVip)
            {
                if (VipManager.ConnectedVips.Count >= VipModConfig.Config.MaxPlayersVips)
                {
                    Helper.KickPlayer(player, ConnectionStatusChangeReason.ServerFull);
                }
                else
                {
                    VipManager.ConnectedVips.Add(player);
                }
            }
            else
            {
                if (VipManager.ConnectedNormals.Count >= VipModConfig.Config.MaxPlayersNonVips)
                {
                    Helper.KickPlayer(player, ConnectionStatusChangeReason.ServerFull);
                }
                else
                {
                    VipManager.ConnectedNormals.Add(player);
                }
            }
        }
    }
}

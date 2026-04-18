using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Scripting;
using ProjectM.Gameplay.Systems;
using ProjectM.Shared.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using ModCore;
using ModCore.Data;
using ModCore.Events;
using ModCore.Helpers;
using ModCore.Models;
using ModCore.Services;
using ProjectM.Network;
using Stunlock.Network;
using System.Runtime.InteropServices;
using ProjectM.Shared;
using System.Reflection;
using VipMod.Managers;

namespace VipMod.Patches
{

/*    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public static class ServerBootstrapSystemPatch
    {
        public static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnection, int version, ulong platformId, string eosIdString, bool isReconnect, bool connectedAsAdmin, ref User user, Entity userEntity, ConnectAddress primaryAddress, ConnectAddress secondaryAddress)
        {
            var player = PlayerService.GetPlayerFromUser(userEntity);
            var isVip = VipModConfig.Config.Vips.ContainsKey(platformId);
            if ((VipModConfig.Config.DebugSkipAdminCheck || (!VipModConfig.Config.DebugSkipAdminCheck && !player.IsAdmin)) && ((!isVip && VipManager.ConnectedNormals.Count >= VipModConfig.Config.MaxPlayersNonVips) || (isVip && VipManager.ConnectedVips.Count >= VipModConfig.Config.MaxPlayersVips)))
            {
                var action = () => Helper.KickPlayer(platformId, ConnectionStatusChangeReason.ServerFull);
                ActionScheduler.RunActionOnceAfterDelay(action, 0.5f);
            }
        }
    }*/
}

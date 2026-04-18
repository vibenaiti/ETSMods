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
using ProjectM.Gameplay.Systems;
using static ModCore.Helpers.Helper;
using ModCore.Factories;
using ProjectM.Scripting;
using HarmonyLib;
using static ProjectM.Network.InteractEvents_Client;
using Unity.Collections;
using Stunlock.Core;
using System.Diagnostics;

namespace DebugMod.Managers
{
    public static class SunProtectionZoneManager
    {
        private static List<Timer> Timers = new();
        public static RectangleZone SunProtectionArea;
        public static void Initialize()
        {
            SunProtectionArea = DebugModConfig.Config.SunProtectionZone.ToRectangleZone();
            var action = () => HandlePlayerSunProtection();
            Timers.Add(ActionScheduler.RunActionEveryInterval(action, 2));
        }


        public static void Dispose()
        {
            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();
        }
        
        private static void HandlePlayerSunProtection()
        {
            if (SunProtectionArea == null) return;
            if (!Helper.IsDayTime()) return;

            var playersInZone = SunProtectionArea.GetPlayersInsideZone(PlayerService.CharacterCache.Values.ToList());
            foreach (var player in playersInZone)
            {
                if (Helper.BuffPlayer(player, Helper.CustomBuff2, out var buffEntity, 3))
                {
                    Helper.ModifyBuff(buffEntity, BuffModificationTypes.ImmuneToSun);
                }
            }
        }
    }
}
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

namespace RaidConfig.Managers
{
/*    public static class BloodDrainManager
    {
        private static List<Timer> Timers = new();
        public static void Initialize()
        {
            var initialCheckAction = () =>
            {
                UpdateBloodDrainRate();
                var daysPassed = Helper.GetServerTimeAdjusted() / 60 / 60 / 24;
                if (daysPassed < 1)
                {
                    var action = () => UpdateBloodDrainRate();
                    Timers.Add(ActionScheduler.RunActionEveryInterval(action, 3600));
                }
            };
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(initialCheckAction, 10));
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

        private static void UpdateBloodDrainRate()
        {
            var daysPassed = Helper.GetServerTimeAdjusted() / 60 / 60 / 24;
            if (daysPassed < 1)
            {
                ChangeHeartDrainRate(1);
            }
            else
            {
                ChangeHeartDrainRate(5);
            }
        }

        private static void ChangeHeartDrainRate(float rate)
        {
            Core.serverGameSettingsSystem.Settings.CastleBloodEssenceDrainModifier = rate;
            var entity = Helper.GetEntitiesByComponentTypes<ServerGameBalanceSettings>()[0];
            var serverGameBalanceSettings = entity.Read<ServerGameBalanceSettings>();
            serverGameBalanceSettings.CastleBloodEssenceDrainModifier = new half(rate);
            entity.Write(serverGameBalanceSettings);
        }
    }*/
}
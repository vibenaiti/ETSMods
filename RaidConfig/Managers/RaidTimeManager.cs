using BepInEx.Configuration;
using HarmonyLib;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using ModCore.Events;
using ModCore.Models;
using ModCore.Services;
using ModCore;
using System.Reflection;
using ModCore.Helpers;
using Unity.Entities;

namespace RaidConfig.Managers
{
    public class RaidWindow
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public static class RaidSchedule
    {
        public static Dictionary<DayOfWeek, RaidWindow> Windows { get; private set; } = new Dictionary<DayOfWeek, RaidWindow>();

        public static void Initialize()
        {
            LoadRaidWindows();
        }

        private static void LoadRaidWindows()
        {
            Windows.Clear(); // Clear any existing windows

            var RaidConfigConfigDataInstance = RaidConfigConfig.Config; // Access your config instance correctly

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                string startPropertyName = $"{day}StartTime";
                string endPropertyName = $"{day}EndTime";

                PropertyInfo startProperty = typeof(RaidConfigConfigData).GetProperty(startPropertyName);
                PropertyInfo endProperty = typeof(RaidConfigConfigData).GetProperty(endPropertyName);

                if (startProperty == null || endProperty == null)
                {
                    continue;
                }

                string startConfigEntry = (string)startProperty.GetValue(RaidConfigConfigDataInstance);
                string endConfigEntry = (string)endProperty.GetValue(RaidConfigConfigDataInstance);

                if (TimeSpan.TryParse(startConfigEntry, out TimeSpan startTime) && TimeSpan.TryParse(endConfigEntry, out TimeSpan endTime))
                {
                    // Check if start and end times are both "00:00:00", indicating no raid window
                    if (startTime == TimeSpan.Zero && endTime == TimeSpan.Zero)
                    {
                        continue; // Skip this day as it has no raid window
                    }

                    // Adjust for "end of day" representation if necessary and applicable
                    if (endTime == TimeSpan.Zero && startTime != TimeSpan.Zero)
                    {
                        endTime = new TimeSpan(24, 0, 0);
                    }

                    Windows[day] = new RaidWindow
                    {
                        Day = day,
                        Start = startTime,
                        End = endTime
                    };
                }
                else
                {
                    Plugin.PluginLog.LogInfo($"Failed to parse times for {day}. Start: {startConfigEntry}, End: {endConfigEntry}");
                }
            }
        }
    }

    public static class RaidTimeManager
    {
        public enum RaidMode
        {
            ForceOn,
            ForceOff,
            Normal
        }

        private static List<Timer> Timers = new();
        public static RaidMode CurrentRaidMode { get; set; } = RaidMode.Normal;

        public static void Initialize()
        {
            RaidSchedule.Initialize();
            var action = () => CheckAndToggleRaidMode();
            var timer = ActionScheduler.RunActionEveryInterval(action, 1);
            Timers.Add(timer);
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

        private static void CheckAndToggleRaidMode()
        {
            if (CurrentRaidMode == RaidMode.Normal)
            {
                var now = DateTime.Now;
                var currentDay = now.DayOfWeek;
                var currentTime = now.TimeOfDay;
                if (RaidSchedule.Windows.TryGetValue(currentDay, out var window))
                {
                    if (currentTime >= window.Start && currentTime <= window.End)
                    {
                        EnableRaid();
                        return;
                    }
                }

                DisableRaid();
            }
            else if (CurrentRaidMode == RaidMode.ForceOn)
            {
                EnableRaid();
            }
            else if (CurrentRaidMode == RaidMode.ForceOff)
            {
                DisableRaid();
            }
        }

        public static void EnableRaid()
        {
            if (Core.serverGameSettingsSystem._Settings.CastleDamageMode != CastleDamageMode.Always)
            {
                Core.serverGameSettingsSystem._Settings.CastleDamageMode = CastleDamageMode.Always;
                var entity = Helper.GetEntitiesByComponentTypes<ServerGameBalanceSettings>()[0];
                var serverGameBalanceSettings = entity.Read<ServerGameBalanceSettings>();
                serverGameBalanceSettings.CastleDamageMode = CastleDamageMode.Always;
                entity.Write(serverGameBalanceSettings);
            }
        }

        public static void DisableRaid()
        {
            if (Core.serverGameSettingsSystem._Settings.CastleDamageMode != CastleDamageMode.TimeRestricted)
            {
                Core.serverGameSettingsSystem._Settings.CastleDamageMode = CastleDamageMode.TimeRestricted;
                var entity = Helper.GetEntitiesByComponentTypes<ServerGameBalanceSettings>()[0];
                var serverGameBalanceSettings = entity.Read<ServerGameBalanceSettings>();
                serverGameBalanceSettings.CastleDamageMode = CastleDamageMode.TimeRestricted;
                entity.Write(serverGameBalanceSettings);
            }
        }
    }
}
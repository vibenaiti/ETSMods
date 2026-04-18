using ProjectM;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using ModCore;
using ModCore.Data;
using ModCore.Helpers;
using ModCore.Services;
using System.Threading;
using ModCore.Events;
using ModCore.Models;
using ModCore.Factories;

namespace OpenWorldEvents.Managers
{
    using ProjectM.Gameplay.Systems;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public static class EventScheduler
    {
        private static List<Timer> Timers = new();

        public static void ScheduleDailyEvent(Action action, TimeOnly eventTimeOfDay)
        {
            ScheduleNextEvent(action, eventTimeOfDay);
        }

        private static void ScheduleNextEvent(Action action, TimeOnly eventTimeOfDay)
        {
            if (!OpenWorldEventsConfig.Config.AutomaticEventsEnabled)
            {
                Dispose();
                return;
            }

            DateTime now = DateTime.Now;
            DateTime nextEvent = DateTime.Today + eventTimeOfDay.ToTimeSpan();

            if (now > nextEvent)
            {
                nextEvent = nextEvent.AddDays(1);
            }

            var timer = ActionScheduler.RunActionAtTime(() =>
            {
                if (Helper.IsRaidHour())
                {
                    DateTime nextValidEvent = DateTime.Today.AddDays(1) + eventTimeOfDay.ToTimeSpan();
                    ScheduleNextEvent(action, TimeOnly.FromDateTime(nextValidEvent));
                }
                else
                {
                    if (!OpenWorldEventsConfig.Config.AutomaticEventsEnabled)
                    {
                        Dispose();
                        return;
                    }
                    action.Invoke();
                    DateTime nextDayEvent = nextEvent.AddDays(1);
                    ScheduleNextEvent(action, TimeOnly.FromDateTime(nextDayEvent));
                }
            }, nextEvent);
            Timers.Add(timer);
        }

        public static void Initialize()
        {
            if (!OpenWorldEventsConfig.Config.AutomaticEventsEnabled) return;

            var daysPassed = Helper.GetServerTimeAdjusted() / 60 / 60 / 24;
            if (daysPassed < OpenWorldEventsConfig.Config.DaysIntoWipeToStartAutomaticEvents) return;

            foreach (var time in ChestConfig.Config.ChestSpawnTimes)
            {
                var action = () => ChestManager.SpawnChestAtRandomLocation();
                ScheduleDailyEvent(action, time);
            }

            foreach (var time in HorseConfig.Config.HorseSpawnTimes)
            {
                var action = () => DonkeyManager.SpawnHorseAtRandomLocation();
                ScheduleDailyEvent(action, time);
            }

            foreach (var time in BossConfig.Config.BossSpawnTimes)
            {
                var action = () => BossManager.SpawnRandomBoss();
                ScheduleDailyEvent(action, time);
            }

            foreach (var time in ResourceNodeConfig.Config.ResourceNodeSpawnTimes)
            {
                var action = () => InfiniteResourceNodeManager.SpawnResourceNodeAtRandomLocation();
                ScheduleDailyEvent(action, time);
            }

            foreach (var time in DuelsConfig.Config.GreaterFlashDuelTimes)
            {
                var action = () => FlashDuelsManager.Initialize(true);
                ScheduleDailyEvent(action, time);
            }

            foreach (var time in DuelsConfig.Config.LesserFlashDuelTimes)
            {
                var action = () => FlashDuelsManager.Initialize(false);
                ScheduleDailyEvent(action, time);
            }

            foreach (var time in DominionConfig.Config.DominionSpawnTimes)
            {
                var action = () => DominionManager.SpawnRingAtRandomLocation();
                ScheduleDailyEvent(action, time);
            }
        }

        public static void Dispose()
        {
            foreach (var timer in Timers)
            {
                timer?.Dispose();
            }
            Timers.Clear();
        }
    }

}

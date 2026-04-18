using ProjectM;
using System.Collections.Generic;
using System.Threading;
using ModCore.Data;
using ModCore.Helpers;
using ModCore;
using ModCore.Services;
using Unity.Entities;
using Stunlock.Core;

namespace GatedProgression
{
    public static class LockedBossesManager
    {
        private static List<Timer> Timers = new();
        private static bool Initialized = false;
        public static void Initialize()
        {
            if (!Initialized)
            {
                var action = () =>
                {
                    var entities = Helper.GetEntitiesByComponentTypes<VBloodUnit>(EntityQueryOptions.IncludeDisabledEntities);
                    foreach (var entity in entities)
                    {
                        if (DataStorage.Data.LockedBosses.Contains(entity.GetPrefabGUID()))
                        {
                            if (Helper.BuffEntity(entity, Prefabs.ServantMissionBuff, out var buffEntity, Helper.NO_DURATION))
                            {
                            }
                        }
                    }
                    entities.Dispose();
                };
                Timers.Add(ActionScheduler.RunActionEveryInterval(action, 1f));
                Initialized = true;
            }
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
            Initialized = false;
        }

        public static void LockBoss(PrefabGUID prefabGUID)
        {
            DataStorage.Data.LockedBosses.Add(prefabGUID);
            DataStorage.Save();
            if (DataStorage.Data.LockedBosses.Count > 0)
            {
                Initialize();
            }
        }

        public static void UnlockBoss(PrefabGUID prefabGUID)
        {
            DataStorage.Data.LockedBosses.Remove(prefabGUID);
            DataStorage.Save();
            if (DataStorage.Data.LockedBosses.Count == 0)
            {
                Dispose();
            }
            var entities = Helper.GetEntitiesByComponentTypes<VBloodUnit>(EntityQueryOptions.IncludeDisabledEntities);
            foreach (var entity in entities)
            {
                if (entity.GetPrefabGUID() == prefabGUID)
                {
                    Helper.RemoveBuff(entity, Prefabs.ServantMissionBuff);
                }
            }
            entities.Dispose();
        }

        public static void LockGroup(string group)
        {
            if (GatedProgressionConfig.Config.LockedBossGroups.TryGetValue(group, out var bosses))
            {
                foreach (var boss in bosses)
                {
                    DataStorage.Data.LockedBosses.Add(boss);
                }
                DataStorage.Save();
            }
            if (DataStorage.Data.LockedBosses.Count > 0)
            {
                Initialize();
            }
        }

        public static void UnlockGroup(string group)
        {
            if (GatedProgressionConfig.Config.LockedBossGroups.TryGetValue(group, out var bosses))
            {
                foreach (var boss in bosses)
                {
                    DataStorage.Data.LockedBosses.Remove(boss);
                    var entities = Helper.GetEntitiesByComponentTypes<VBloodUnit>(EntityQueryOptions.IncludeDisabledEntities);
                    foreach (var entity in entities)
                    {
                        if (entity.GetPrefabGUID() == boss)
                        {
                            Helper.RemoveBuff(entity, Prefabs.ServantMissionBuff);
                        }
                    }
                    entities.Dispose();
                }
                DataStorage.Save();
                if (DataStorage.Data.LockedBosses.Count == 0)
                {
                    Dispose();
                }
            }
        }
    }
}

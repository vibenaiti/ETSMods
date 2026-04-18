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
using Stunlock.Core;

namespace OpenWorldEvents.Managers
{
    public static class WorkerModeManager
    {
        private static List<Timer> Timers = new();
        public static Dictionary<Player, Entity> PlayerToMapIcon = new();
        private static PrefabGUID WorkerModeBuff = Helper.CustomBuff1;
        public static PrefabGUID MapIconPrefabGUID = Prefabs.MapIcon_POI_Resource_IronVein;
        public static void Initialize()
        {
            GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
        }

        public static void Dispose()
        {
            foreach (var kvp in PlayerToMapIcon)
            {
                if (kvp.Value.Exists())
                {
                    Helper.RemoveBuff(kvp.Key, Helper.CustomBuff1);
                    Helper.DestroyEntity(kvp.Value);
                }
            }
            GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
        }

        private static void HandleOnPlayerDisconnected(Player player)
        {
            if (PlayerToMapIcon.ContainsKey(player))
            {
                DisableWorkerMode(player);
            }
        }

        public static bool IsPlayerInWorkerMode(Player player)
        { 
            return PlayerToMapIcon.ContainsKey(player);
        }

        public static void EnableWorkerMode(Player player)
        {
            if (Helper.BuffPlayer(player, WorkerModeBuff, out var buffEntity, Helper.NO_DURATION, true))
            {
                Helper.ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    ModificationType = ModificationType.Multiply,
                    Id = ModificationIdFactory.NewId(),
                    StatType = UnitStatType.ResourceYield,
                    Modifier = 1,
                    Value = 1.25f
                });
                Helper.ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    ModificationType = ModificationType.Multiply,
                    Id = ModificationIdFactory.NewId(),
                    StatType = UnitStatType.ResourceYield,
                    Modifier = 1,
                    Value = 1.25f
                });
                Helper.AttachMapIconToPlayer(player, MapIconPrefabGUID, (e) =>
                {
                    player.ReceiveMessage("Worker mode has been enabled. Everyone can see you on the map!".White());
                    PlayerToMapIcon[player] = e;
                    if (PlayerToMapIcon.Count == 1)
                    {
                        Initialize();
                    }
                });
            }
        }

        public static void DisableWorkerMode(Player player)
        {
            if (PlayerToMapIcon.TryGetValue(player, out var mapIcon))
            {
                Helper.DestroyEntity(mapIcon);
                PlayerToMapIcon.Remove(player);
                player.ReceiveMessage("Worker mode has been disabled. You no longer show up on the map!".White());
            }
            Helper.RemoveBuff(player, WorkerModeBuff);
            if (PlayerToMapIcon.Count == 0)
            {
                Dispose();
            }
        }
    }
}

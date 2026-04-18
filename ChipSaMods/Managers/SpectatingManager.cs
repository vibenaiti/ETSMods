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

namespace ChipSaMod.Managers
{
    public static class SpectatingManager
    {
        public static Dictionary<Entity, Player> SpectateBuffs = new();
        public static Dictionary<Player, Player> SpectatorToSpectatedPlayer = new();

        public static void Initialize()
        {
            GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;
        }

        public static void Dispose()
        {
            SpectatorToSpectatedPlayer.Clear();
            SpectateBuffs.Clear();
            GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;
        }

        public static bool TryGetSpectatingTarget(Player player, out Player target)
        {
            return SpectatorToSpectatedPlayer.TryGetValue(player, out target);
        }

        public static void RemoveSpectator(Player player)
        {
            if (Helper.TryGetBuff(player, Prefabs.Admin_Observe_Invisible_Buff, out var buffEntity))
            {
                if (SpectateBuffs.ContainsKey(buffEntity))
                {
                    SpectateBuffs.Remove(buffEntity);
                }
                Helper.DestroyBuff(buffEntity);
            }
            SpectatorToSpectatedPlayer.Remove(player);
            if (SpectatorToSpectatedPlayer.Count == 0)
            {
                Dispose();
            }
        }

        public static bool AddSpectator(Player spectator, Player target)
        {
            if (Helper.BuffPlayer(spectator, Prefabs.Admin_Observe_Invisible_Buff, out var spectateBuffEntity))
            {
                if (SpectateBuffs.TryGetValue(spectateBuffEntity, out var previousTarget))
                {
                    if (previousTarget == target)
                    {
                        return false;
                    }
                    SpectateBuffs.Remove(spectateBuffEntity);
                }
                SpectatorToSpectatedPlayer[spectator] = target;
                spectateBuffEntity.Add<TeleportBuff>();
                SpectateBuffs[spectateBuffEntity] = target;
                if (SpectatorToSpectatedPlayer.Count == 1)
                {
                    Initialize();
                }
            }

            return true;
        }

        public static void HandleOnGameFrameUpdate()
        {
            var buffsToRemove = new List<Entity>();
            foreach (var kvp in SpectateBuffs)
            {
                if (kvp.Key.Exists())
                {
                    if (kvp.Key.Has<TeleportBuff>())
                    {
                        var teleportBuff = kvp.Key.Read<TeleportBuff>();
                        teleportBuff.EndPosition = kvp.Value.Position;
                        kvp.Key.Write(teleportBuff);
                    }
                    else
                    {
                        buffsToRemove.Add(kvp.Key);
                        SpectatorToSpectatedPlayer.Remove(kvp.Value);
                    }
                }
                else
                {
                    buffsToRemove.Add(kvp.Key);
                    SpectatorToSpectatedPlayer.Remove(kvp.Value);
                }
            }
            foreach (var buff in buffsToRemove)
            {
                SpectateBuffs.Remove(buff);
            }
        }
    }
}

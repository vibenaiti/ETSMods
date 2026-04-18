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
using ProjectM.Gameplay.Scripting;

namespace OpenWorldEvents.Managers
{
    public static class CursedFogManager
    {
        private static List<Timer> Timers = new();

        public static unsafe void Initialize()
        {
            GameEvents.OnPlayerConnected += HandleOnPlayerConnected;

            var prefab = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Buff_General_CurseOfTheForest_Area);
            var prefabData = prefab.Read<Script_CursedAreaDebuff_DataServer>();
            prefabData.ImmunityBuff = PrefabGUID.Empty;
            prefab.Write(prefabData);

            var sysInstance = Core.radialZoneSystemCurseEntity.Read<SystemInstance>();
            var sysState = sysInstance.state;
            sysState->Enabled = false;
            Core.radialZoneSystemCurseEntity.Write(sysInstance);

            foreach (var player in PlayerService.CharacterCache.Values)
            {
                if (Helper.BuffPlayer(player, Prefabs.Buff_General_CurseOfTheForest_Area, out var buffEntity, Helper.NO_DURATION))
                {
                    var comp = buffEntity.Read<Script_CurseAreaDebuff_DataShared>();
                    var serverComp = buffEntity.Read<Script_CursedAreaDebuff_DataServer>();
                    serverComp.ImmunityBuff = PrefabGUID.Empty;
                    serverComp.DynamicStacksPerTick = 5;
                    buffEntity.Write(serverComp);
                    comp.IsInArea = true;
                    buffEntity.Write(comp);
                }
            }

            var action = () => EndFog();
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, CursedFogConfig.Config.FogDuration));
        }

        public static unsafe void Dispose()
        {
            GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;

            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();

            var prefab = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Buff_General_CurseOfTheForest_Area);
            var prefabData = prefab.Read<Script_CursedAreaDebuff_DataServer>();
            prefabData.ImmunityBuff = PrefabGUID.Empty;
            prefabData.BlockCurseBuff = Prefabs.AB_Interact_Curse_Wisp_Buff;
            prefab.Write(prefabData);

            var sysInstance = Core.radialZoneSystemCurseEntity.Read<SystemInstance>();
            var sysState = sysInstance.state;
            sysState->Enabled = true;
            Core.radialZoneSystemCurseEntity.Write(sysInstance);
        }

        private static void HandleOnPlayerConnected(Player player)
        {
            if (Helper.BuffPlayer(player, Prefabs.Buff_General_CurseOfTheForest_Area, out var buffEntity, Helper.NO_DURATION))
            {
                var comp = buffEntity.Read<Script_CurseAreaDebuff_DataShared>();
                var serverComp = buffEntity.Read<Script_CursedAreaDebuff_DataServer>();
                serverComp.ImmunityBuff = PrefabGUID.Empty;
                serverComp.DynamicStacksPerTick = 5;
                buffEntity.Write(serverComp);
                comp.IsInArea = true;
                buffEntity.Write(comp);
            }
        }

        public static void StartFog()
        {
            Helper.SendSystemMessageToAllClients("A cursed fog begins to fill Vardoran..");
            Initialize();
        }

        public static void EndFog()
        {
            Helper.SendSystemMessageToAllClients("The cursed fog has begun to recede!");
            Dispose();
        }
    }
}

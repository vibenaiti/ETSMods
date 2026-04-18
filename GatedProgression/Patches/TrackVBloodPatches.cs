using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Scripting;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Shared.Systems;
using System;
using System.Collections.Generic;
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
using Stunlock.Core;
using System.Runtime.InteropServices;

namespace GatedProgression.Patches
{
    [HarmonyPatch(typeof(BloodAltarSystem_StartTrackVBloodUnit_System_V2), nameof(BloodAltarSystem_StartTrackVBloodUnit_System_V2.OnUpdate))]
    public static class BloodAltarSystem_StartTrackVBloodUnit_System_V2Patch
    {    
        public static void Prefix(BloodAltarSystem_StartTrackVBloodUnit_System_V2 __instance)
        {
            try
            {
                var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities )
                {
                    var startTrackVBloodUnitEventV2 = entity.Read<StartTrackVBloodUnitEventV2>();
                    var fromCharacter = entity.Read<FromCharacter>();
                    var lockedVBloods = DataStorage.Data.LockedBosses;
                    if (lockedVBloods.Contains(startTrackVBloodUnitEventV2.HuntTarget))
                    {
                        var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
                        player.ReceiveMessage($"The VBlood you are attempting to track is currently disabled.".Error());
                        entity.Destroy();
                    }
                }
            }
            catch (System.Exception e)
            {
                Plugin.PluginLog.LogInfo(e.ToString());
            }
        }
    }
}

using HarmonyLib;
using OpenWorldEvents;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay;
using ProjectM.Gameplay.Scripting;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Shared;
using ProjectM.Shared.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace OpenWorldEvents.Patches
{
    [HarmonyPatch(typeof(UsePortalSystem), nameof(UsePortalSystem.OnUpdate))]
    public static class UsePortalSystemPatch
    {
        public static void Prefix(UsePortalSystem __instance)
        {
            var entities = __instance.__query_695019499_0.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var owner = entity.Read<EntityOwner>().Owner;
                if (Helper.HasBuff(owner, Prefabs.AB_Militia_HoundMaster_QuickShot_Buff))
                {
                    var player = PlayerService.GetPlayerFromCharacter(owner);
                    entity.Remove<UsePortal>();
                    player.ReceiveMessage("You cannot use caves while on the horse!".Error());
                }
                
                //Helper.DestroyBuff(entity);

            }
            entities.Dispose();
        }
    }
}

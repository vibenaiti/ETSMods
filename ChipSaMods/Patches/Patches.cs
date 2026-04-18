using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Scripting;
using ProjectM.Gameplay.Systems;
using ProjectM.Shared.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
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
using ProjectM.Network;
using Il2CppSystem;
using Unity.Mathematics;

namespace ChipSaMod.Patches
{
/*	[HarmonyPatch(typeof(OnDeathSystem), nameof(OnDeathSystem.DropInventoryOnDeath))]
	public static class OnDeathSystemPatch
    {
		public static bool Prefix(OnDeathSystem __instance)
		{
			return false;
		}
	}*/

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyIfCloseEnough))]
    public static class PlaceTileModelSystemPatch
    {
        // Patch for the first overload
        [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyIfCloseEnough), new[] { typeof(EntityManager), typeof(Entity), typeof(float3), typeof(Nullable_Unboxed<Entity>) })]
        public static class VerifyIfCloseEnoughPatch1
        {
            public static void Postfix(PlaceTileModelSystem __instance, bool __result, EntityManager entityManager, Entity character, float3 position, Nullable_Unboxed<Entity> existingTileModel)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.SendRevealedMapData))]
    public static class ServerBootstrapSystemPatch
    {
        public static void Prefix(ServerBootstrapSystem __instance, Entity userEntity, User user)
        {
            var player = PlayerService.GetPlayerFromUser(userEntity);
            ModCore.Helpers.Helper.RevealMapForPlayer(player);
        }
    }
}

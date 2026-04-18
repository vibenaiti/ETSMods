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
	[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.SendRevealedMapData))]
	public static class ServerBootstrapSystemPatch
    {
		public static void Prefix(ServerBootstrapSystem __instance, Entity userEntity, User user)
		{
            if (OpenWorldEventsConfig.Config.RevealMap)
			{
                var player = PlayerService.GetPlayerFromUser(userEntity);
                Helper.RevealMapForPlayer(player);
            }
		}
	}
}

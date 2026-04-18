using HarmonyLib;
using Unity.Collections;
using ProjectM.Network;
using ProjectM.Gameplay.Clan;
using ModCore.Services;
using ModCore.Events;
using System;
using ProjectM;
using static ProjectM.Network.ClanEvents_Client;

namespace ModCore.Patches;


[HarmonyPatch(typeof(TeleportToWaypointEventSystem), nameof(TeleportToWaypointEventSystem.OnUpdate))]
public static class TeleportToWaypointEventSystemPatch
{
	public static void Prefix(TeleportToWaypointEventSystem __instance)
	{
		if (GameEvents.OnPlayerUsedWaygate != null)
		{
			var entities = __instance._TeleportToWaypointEventQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{
				try
				{
					var teleportToWaypointEvent = entity.Read<TeleportEvents_ToServer.TeleportToWaypointEvent>();
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					GameEvents.OnPlayerUsedWaygate?.Invoke(player, entity, teleportToWaypointEvent);
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
					continue;
				}
			}
			entities.Dispose();
		}
	}
}

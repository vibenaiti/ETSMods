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


[HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
public static class PlaceTileModelSystemPatch
{
	public static void Prefix(PlaceTileModelSystem __instance)
	{
		if (GameEvents.OnPlayerPlacedStructure != null)
		{
			var eventEntities = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in eventEntities)
			{
				try
				{
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					var buildTileModelEvent = entity.Read<BuildTileModelEvent>();
					GameEvents.OnPlayerPlacedStructure?.Invoke(player, entity, buildTileModelEvent);
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
					continue;
				}
			}
			eventEntities.Dispose();
		}
	}
}

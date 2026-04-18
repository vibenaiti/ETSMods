using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Mathematics;
using ModCore.Services;
using ModCore.Events;
using ModCore.Models;
using System;
using ModCore.Data;
using ModCore.Listeners;
using Unity.Entities;
using ProjectM.Network;
using UnityEngine.Jobs;
using ProjectM.Gameplay.Systems;

namespace ModCore.Patches;

[HarmonyPatch(typeof(CastleHeartEventSystem), nameof(CastleHeartEventSystem.OnUpdate))]
public static class CastleHeartEventSystemPatch
{
	public static void Postfix(CastleHeartEventSystem __instance)
	{
		if (GameEvents.OnPlayerInteractedWithCastleHeart != null)
		{
			var entities = __instance._CastleHeartInteractEventQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{
				try
				{
					var player = PlayerService.GetPlayerFromUser(entity.Read<FromCharacter>().User);
					var castleHeartInteractEvent = entity.Read<CastleHeartInteractEvent>();
					GameEvents.OnPlayerInteractedWithCastleHeart?.Invoke(player, entity, castleHeartInteractEvent);
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
				}
			}
		}

/*		if (GameEvents.OnCastleHeartChangedState != null)
		{
			var entities = __instance._CastleHeartInteractEventQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{
				try
				{
					var player = PlayerService.GetPlayerFromUser(entity.Read<FromCharacter>().User);
					var castleHeartInteractEvent = entity.Read<CastleHeartInteractEvent>();
					GameEvents.OnPlayerInteractedWithCastleHeart?.Invoke(player, entity, castleHeartInteractEvent);
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
				}
			}
		}*/
	}
}

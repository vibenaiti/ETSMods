using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using ModCore.Data;
using System.Collections.Generic;
using ProjectM.Network;
using ModCore.Services;
using ModCore.Events;
using static ProjectM.CastleBuilding.CastleBlockSystem;
using ModCore.Helpers;
using ModCore.Models;
using System;

namespace ModCore.Patches;

[HarmonyPatch(typeof(ShapeshiftSystem), nameof(ShapeshiftSystem.OnUpdate))]
public static class ShapeshiftSystemPatch
{
	public static void Prefix(ShapeshiftSystem __instance)
	{
		if (GameEvents.OnPlayerShapeshift == null) return;

		var entities = __instance.__query_1988075349_0.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				if (entity.Exists())
				{
					var fromCharacter = entity.Read<FromCharacter>();

					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					GameEvents.OnPlayerShapeshift?.Invoke(player, entity);
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
		entities.Dispose();
	}
}

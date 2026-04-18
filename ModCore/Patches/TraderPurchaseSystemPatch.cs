using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using ProjectM.Network;
using ModCore.Services;
using System.Linq;
using ModCore.Models;
using System;
using ModCore.Events;

namespace ModCore.Patches;


[HarmonyPatch(typeof(TraderPurchaseSystem), nameof(TraderPurchaseSystem.OnUpdate))]
public static class TraderPurchaseSystemPatch
{
	public static void Prefix(TraderPurchaseSystem __instance)
	{
		if (GameEvents.OnPlayerPurchasedItem == null) return;

		var entities = __instance._TraderPurchaseEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				var traderPurchaseEvent = entity.Read<TraderPurchaseEvent>();
				GameEvents.OnPlayerPurchasedItem?.Invoke(player, entity, traderPurchaseEvent);

			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}


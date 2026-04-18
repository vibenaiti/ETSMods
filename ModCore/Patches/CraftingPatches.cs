using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ModCore.Services;
using System;
using ModCore.Events;

namespace ModCore.Patches;


[HarmonyPatch(typeof(StartCharacterCraftingSystem), nameof(StartCharacterCraftingSystem.OnUpdate))]
public static class StartCharacterCraftingSystemPatch
{
	public static void Prefix(StartCharacterCraftingSystem __instance)
	{
		if (GameEvents.OnPlayerStartedCharacterCrafting == null) return;

		var entities = __instance._StartCharacterCraftItemEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				var startCharacterCraftItemEvent = entity.Read<StartCharacterCraftItemEvent>();
				GameEvents.OnPlayerStartedCharacterCrafting?.Invoke(player, entity, startCharacterCraftItemEvent);
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}

[HarmonyPatch(typeof(StartCraftingSystem), nameof(StartCraftingSystem.OnUpdate))]
public static class StartCraftingSystemPatch
{
	public static void Prefix(StartCraftingSystem __instance)
	{
		if (GameEvents.OnPlayerStartedCrafting == null) return;

		var entities = __instance._StartCraftItemEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				var startCraftItemEvent = entity.Read<StartCraftItemEvent>();
				GameEvents.OnPlayerStartedCrafting?.Invoke(player, entity, startCraftItemEvent);
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}




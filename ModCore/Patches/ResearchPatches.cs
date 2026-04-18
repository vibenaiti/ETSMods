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


[HarmonyPatch(typeof(UnlockResearchSystem), nameof(UnlockResearchSystem.OnUpdate))]
public static class UnlockResearchSystemPatch
{
	public static void Prefix(UnlockResearchSystem __instance)
	{
		if (GameEvents.OnPlayerUnlockedResearch == null) return;

		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				var unlockResearchEvent = entity.Read<UnlockResearchEvent>();
				GameEvents.OnPlayerUnlockedResearch?.Invoke(player, entity, unlockResearchEvent);

			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}

[HarmonyPatch(typeof(SpellSchoolProgressionEventSystem), nameof(SpellSchoolProgressionEventSystem.OnUpdate))]
public static class SpellSchoolProgressionEventSystemPatch
{
	public static void Prefix(SpellSchoolProgressionEventSystem __instance)
	{
		if (GameEvents.OnPlayerSharedAllPassives == null) return;

		var entities = __instance._ShareAllPassivesEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				var shareAllSpellSchoolPassives = entity.Read<ShareAllSpellSchoolPassives>();
				GameEvents.OnPlayerSharedAllPassives?.Invoke(player, entity, shareAllSpellSchoolPassives);

			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}

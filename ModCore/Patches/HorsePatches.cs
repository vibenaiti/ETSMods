using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ModCore.Events;
using ModCore.Services;
using System;

namespace ModCore.Patches;

[HarmonyPatch(typeof(MountBuffSpawnSystem_Server), nameof(MountBuffSpawnSystem_Server.OnUpdate))]
public static class MountBuffSpawnSystem_ServerPatch
{
	public static void Prefix(MountBuffSpawnSystem_Server __instance)
	{
		if (GameEvents.OnPlayerMounted == null) return;
		var entities = __instance.__query_1228665165_0.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var character = entity.Read<EntityOwner>().Owner;
				var player = PlayerService.GetPlayerFromCharacter(character);
				GameEvents.OnPlayerMounted?.Invoke(player, entity);
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
		entities.Dispose();
	}
}


[HarmonyPatch(typeof(MountBuffDestroySystem_Shared), nameof(MountBuffDestroySystem_Shared.OnUpdate))]
public static class MountBuffDestroySystem_SharedPatch
{
	public static void Prefix(MountBuffDestroySystem_Shared __instance)
	{
		if (GameEvents.OnPlayerDismounted == null) return;
		var entities = __instance._MountBuffDestroyQueryShared.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var character = entity.Read<EntityOwner>().Owner;
				var player = PlayerService.GetPlayerFromCharacter(character);
				GameEvents.OnPlayerDismounted?.Invoke(player, entity);
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
		entities.Dispose();
	}
}

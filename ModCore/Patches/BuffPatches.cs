using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using ModCore.Data;
using System.Collections.Generic;
using ModCore.Events;
using ModCore.Services;
using ModCore.Helpers;
using ModCore.Factories;
using Unity.Transforms;
using ModCore.Configs;
using Unity.Mathematics;
using ModCore.Models;
using System;
using Stunlock.Core;

namespace ModCore.Patches;


[HarmonyPatch(typeof(BuffDebugSystem), nameof(BuffDebugSystem.OnUpdate))]
public static class BuffDebugSystemPatch
{
	public static void Prefix(BuffDebugSystem __instance)
	{
		if (GameEvents.OnPlayerBuffed == null && GameEvents.OnUnitBuffed == null) return;
		
		var entities = __instance.__query_401358787_0.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var prefabGuid = entity.Read<PrefabGUID>();
				var buffTarget = entity.Read<EntityOwner>().Owner;
				var buff = entity.Read<Buff>();
				if (buff.Target.Exists())
				{
					buffTarget = buff.Target;
				}
				if (buffTarget.Has<PlayerCharacter>())
				{
					var player = PlayerService.GetPlayerFromCharacter(buffTarget);
					GameEvents.OnPlayerBuffed?.Invoke(player, entity, prefabGuid);
				}
				else
				{
					GameEvents.OnUnitBuffed?.Invoke(buffTarget, entity);
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
				continue;
			}

		}
		entities.Dispose();
	}

	[HarmonyPatch(typeof(UpdateBuffsBuffer_Destroy), nameof(UpdateBuffsBuffer_Destroy.OnUpdate))]
	public static class UpdateBuffsBuffer_DestroyPatch
	{
		public static void Prefix(UpdateBuffsBuffer_Destroy __instance)
		{
			if (GameEvents.OnPlayerBuffRemoved == null && GameEvents.OnUnitBuffRemoved == null) return;

			var entities = __instance.__query_401358717_0.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{
				try
				{
					var buffTarget = entity.Read<EntityOwner>().Owner;
					var buff = entity.Read<Buff>();
					if (buff.Target.Exists())
					{
						buffTarget = buff.Target;
					}
					if (buffTarget.Exists())
					{
						if (buffTarget.Has<PlayerCharacter>())
						{
							var player = PlayerService.GetPlayerFromCharacter(buffTarget);
							GameEvents.OnPlayerBuffRemoved?.Invoke(player, entity, entity.GetPrefabGUID());
						}
						else
						{
							GameEvents.OnUnitBuffRemoved?.Invoke(buffTarget, entity);
						}
					}
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



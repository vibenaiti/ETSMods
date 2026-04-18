using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ProjectM.Gameplay.Systems;
using ModCore.Services;
using ProjectM.CastleBuilding;
using ModCore.Events;
using ModCore.Factories;
using ModCore.Helpers;
using static StatRecorderService;
using System;
using ModCore.Data;
using System.Collections.Generic;
using ModCore.Models;
using Unity.Entities;
using Stunlock.Core;

namespace ModCore.Patches;

[HarmonyPatch(typeof(DealDamageSystem), nameof(DealDamageSystem.OnUpdate))]
public static class DealDamageSystemPatch
{
	public static void Prefix(DealDamageSystem __instance)
	{
		if (GameEvents.OnPlayerDamageDealt == null && GameEvents.OnUnitDamageDealt == null) return;

		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var dealDamageEvent = entity.Read<DealDamageEvent>();
				if (dealDamageEvent.Target.Exists())
				{
					if (dealDamageEvent.SpellSource.Exists())
					{
						if (dealDamageEvent.SpellSource.Has<EntityOwner>())
						{
							var owner = dealDamageEvent.SpellSource.Read<EntityOwner>().Owner;

							if (owner.Exists())
							{
								if (owner.Has<EntityOwner>())
								{
									var minionOwner = owner.Read<EntityOwner>().Owner;
									if (minionOwner.Exists() && minionOwner.Has<PlayerCharacter>())
									{
										owner = minionOwner;
									}
								}
								if (owner.Has<PlayerCharacter>())
								{
									var player = PlayerService.GetPlayerFromCharacter(owner);
									GameEvents.OnPlayerDamageDealt?.Invoke(player, entity, dealDamageEvent);
								}
								else
								{
									GameEvents.OnUnitDamageDealt?.Invoke(owner, entity, dealDamageEvent);
								}
							}
							else
							{
								GameEvents.OnUnitDamageDealt?.Invoke(dealDamageEvent.SpellSource, entity, dealDamageEvent);
							}
						}
						else
						{
							GameEvents.OnUnitDamageDealt?.Invoke(dealDamageEvent.SpellSource, entity, dealDamageEvent);
						}
					}
				}
			}
			catch (System.Exception e)
			{
				Plugin.PluginLog.LogInfo($"An error occurred in the deal damage system: {e.ToString()}");
			}
		}
	}
}

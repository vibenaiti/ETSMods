using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectM.Gameplay.Clan;
using static ProjectM.Network.ClanEvents_Client;
using ModCore.Services;
using ModCore.Models;
using ModCore.Data;
using ModCore.Configs;
using Il2CppSystem;
using Unity.Jobs;
using UnityEngine.Jobs;
using ProjectM.Behaviours;
using Stunlock.Core;

namespace ModCore.Helpers;

//this is horrible god help us all
public static partial class Helper
{
	public static void GenerateJewelViaEvent(Player player, string spellName, string mods = "",
		float power = 1, int tier = 4)
	{
		PrefabGUID abilityPrefab = JewelData.abilityToPrefabDictionary[spellName];
		power = 0.15f + power * (1 - 0.15f);
		/*		if (tier == 4)
				{
					GenerateJewelDebugEvent generateJewelDebugEvent = new GenerateJewelDebugEvent();
					generateJewelDebugEvent.AbilityPrefabGuid = abilityPrefab;
					generateJewelDebugEvent.Power = 1;
					generateJewelDebugEvent.Tier = 4;

					Core.debugEventsSystem.GenerateJewelEvent(player.User.Read<User>().Index, ref generateJewelDebugEvent);
				}*/

		if (tier == 4 && mods.Length >= 3)
		{
			int mod0 = System.Convert.ToInt32(mods[0].ToString(), 16) - 1;
			int mod1 = System.Convert.ToInt32(mods[1].ToString(), 16) - 1;
			int mod2 = System.Convert.ToInt32(mods[2].ToString(), 16) - 1;
			int mod3 = System.Convert.ToInt32(mods[3].ToString(), 16) - 1;

			CreateJewelDebugEventV2 createJewelDebugEvent = new CreateJewelDebugEventV2();
			createJewelDebugEvent.AbilityPrefabGuid = abilityPrefab;
			createJewelDebugEvent.Tier = 3;
			if (JewelData.AbilityToSpellMods.ContainsKey(abilityPrefab))
			{
				if (mod0 >= 0 && mod0 < JewelData.AbilityToSpellMods.Count)
				{
					createJewelDebugEvent.SpellMod1 = JewelData.AbilityToSpellMods[abilityPrefab][mod0];
					createJewelDebugEvent.SpellMod1Power = power;
				}
				else
				{
					Plugin.PluginLog.LogInfo($"Tried to spawn jewel with invalid mod, check your config!: {mod0}");
					return;
				}

				if (mod1 >= 0 && mod1 < JewelData.AbilityToSpellMods.Count)
				{
					createJewelDebugEvent.SpellMod2 = JewelData.AbilityToSpellMods[abilityPrefab][mod1];
					createJewelDebugEvent.SpellMod2Power = power;
				}
				else
				{
					Plugin.PluginLog.LogInfo($"Tried to spawn jewel with invalid mod, check your config!: {mod1}");
					return;
				}

				if (mod2 >= 0 && mod2 < JewelData.AbilityToSpellMods.Count)
				{
					createJewelDebugEvent.SpellMod3 = JewelData.AbilityToSpellMods[abilityPrefab][mod2];
					createJewelDebugEvent.SpellMod3Power = power;
				}
				else
				{
					Plugin.PluginLog.LogInfo($"Tried to spawn jewel with invalid mod, check your config!: {mod2}");
					return;
				}

				if (mod3 >= 0 && mod3 < JewelData.AbilityToSpellMods.Count)
				{
					createJewelDebugEvent.SpellMod4 = JewelData.AbilityToSpellMods[abilityPrefab][mod3];
					createJewelDebugEvent.SpellMod4Power = power;
				}
				else
				{
					Plugin.PluginLog.LogInfo($"Tried to spawn jewel with invalid mod, check your config!: {mod2}");
					return;
				}

				var jewelEventEntity = VWorld.Server.EntityManager.CreateEntity(
					ComponentType.ReadWrite<FromCharacter>(),
					ComponentType.ReadWrite<CreateJewelDebugEventV2>(),
					ComponentType.ReadWrite<HandleClientDebugEvent>(),
					ComponentType.ReadWrite<NetworkEventType>(),
					ComponentType.ReadWrite<ReceiveNetworkEventTag>()
				);

				jewelEventEntity.Write(createJewelDebugEvent);

				HandleClientDebugEvent handleClientDebugEvent = jewelEventEntity.Read<HandleClientDebugEvent>();
				handleClientDebugEvent.FromUserIndex = player.User.Read<User>().Index;
				jewelEventEntity.Write(handleClientDebugEvent);
			}
			else
			{
				Plugin.PluginLog.LogInfo(
					$"Tried to spawn jewel with invalid ability, check your config!: {spellName}");
			}
		}
	}

	public static void EquipJewels(Player player)
	{
		int inventoryIndex = 0;
		foreach (var jewel in JewelData.abilityToPrefabDictionary)
		{
			Helper.EquipJewelAtSlot(player, inventoryIndex);
			inventoryIndex++;
		}
	}
}

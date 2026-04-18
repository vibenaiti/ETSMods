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
using ProjectM.CastleBuilding;
using Stunlock.Core;

namespace ModCore.Helpers;

//this is horrible god help us all
public static partial class Helper
{
	private static Dictionary<PrefabGUID, PrefabGUID> BloodTypeToUnit = new()
	{
		{ Prefabs.BloodType_Brute, Prefabs.CHAR_ChurchOfLight_Cleric },
		{ Prefabs.BloodType_Warrior, Prefabs.CHAR_ChurchOfLight_Paladin },
		{ Prefabs.BloodType_Scholar, Prefabs.CHAR_ChurchOfLight_Lightweaver },
		{ Prefabs.BloodType_Rogue, Prefabs.CHAR_ChurchOfLight_Rifleman },
		{ Prefabs.BloodType_Worker, Prefabs.CHAR_ChurchOfLight_Villager_Female },
		{ Prefabs.BloodType_Creature, Prefabs.CHAR_Winter_Bear_Standard },
		{ Prefabs.BloodType_Mutant, Prefabs.CHAR_Mutant_Bear_Standard },
		{ Prefabs.BloodType_Draculin, Prefabs.CHAR_Legion_Nightmare },
		{ Prefabs.BloodType_VBlood, Prefabs.CHAR_Gloomrot_Voltage_VBlood },
		{ Prefabs.BloodType_DraculaTheImmortal, Prefabs.CHAR_Dracula_BloodSoul_Heart },
		{ Prefabs.BloodType_GateBoss, Prefabs.CHAR_Gloomrot_Voltage_VBlood_GateBoss_Major },
		{ Prefabs.BloodType_None, Prefabs.CHAR_Bandit_Miner_VBlood_UNUSED },
	};
	// NOTE: ConsumeBloodDebugEvent and DebugEventsSystem.ConsumeBloodEvent were removed in v1.1.11+.
	// We now directly write to the Blood component on the character entity instead.
	public static void SetPlayerBlood(Player player, PrefabGUID bloodType, float quality = 100)
	{
		if (!player.User.Exists())
		{
			return;
		}
		quality = Clamp(quality, 0, 100);

		var blood = player.Character.Read<Blood>();
		blood.BloodType = bloodType;
		blood.Quality = quality;
		blood.Value = 100;
		player.Character.Write(blood);
	}

	public static void SetDefaultBlood (Player player, string defaultBlood, int quality = 100)
	{
		defaultBlood = defaultBlood.ToLower();
		var blood = player.Character.Read<Blood>();
		bool bloodModified = false;
		
		if (blood.Quality != quality)
			Helper.SetPlayerBlood(player, blood.BloodType, quality);
		
		if (blood.BloodType == Prefabs.BloodType_None || blood.BloodType == Prefabs.BloodType_Worker ||
		    blood.BloodType == Prefabs.BloodType_Mutant)
		{
			if (defaultBlood != "frailed")
			{
				if (defaultBlood == "warrior")
				{
					Helper.SetPlayerBlood(player, Prefabs.BloodType_Warrior, quality);
					bloodModified = true;
				}
				else if (defaultBlood == "scholar")
				{
					Helper.SetPlayerBlood(player, Prefabs.BloodType_Scholar, quality);
					bloodModified = true;
				}
				else if (defaultBlood == "rogue")
				{
					Helper.SetPlayerBlood(player, Prefabs.BloodType_Rogue, quality);
					bloodModified = true;
				}
				else if (defaultBlood == "brute")
				{
					Helper.SetPlayerBlood(player, Prefabs.BloodType_Brute, quality);
					bloodModified = true;
				}
				else if (defaultBlood == "creature")
				{
					Helper.SetPlayerBlood(player, Prefabs.BloodType_Creature, quality);
					bloodModified = true;
				}
			}
		}
		else if (defaultBlood == "frailed")
		{
			Helper.SetPlayerBlood(player, Prefabs.BloodType_None, quality);
			bloodModified = true;
		}

		if (bloodModified)
		{
			player.ReceiveMessage(
				$"Your blood type isn't eligible for this game mode. Setting to the configured default blood".Error());
		}
	}
}

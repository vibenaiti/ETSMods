using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using ModCore;
using ModCore.Data;
using ModCore.Helpers;
using ModCore.Models;
using ModCore.Services;
using Stunlock.Core;
using System.Runtime.InteropServices;

namespace Achievements.Patches
{
	[HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.OnUpdate))]
	public static class VBloodSystemPatch
	{
		private static Dictionary<PrefabGUID, HashSet<Player>> BossToPlayers = new ();

		public static void Prefix (VBloodSystem __instance)
		{
			if (__instance.EventList.Length > 0)
			{
				foreach (var claimVBloodEvent in __instance.EventList)
				{
					var player = PlayerService.GetPlayerFromCharacter(claimVBloodEvent.Target);
					var vBloodUnit = claimVBloodEvent.Source;
					if (DataStorageFile.Data.KilledBosses.TryGetValue(vBloodUnit, out var time))
					{
						if ((DateTime.Now - time).TotalSeconds > 1)
						{
							continue;
						}
						else
						{
							BossToPlayers[vBloodUnit].Add(player);
						}
					}
					else
					{
						DataStorageFile.Data.KilledBosses[vBloodUnit] = DateTime.Now;
						DataStorageFile.Save();
						if (BossToPlayers.TryGetValue(vBloodUnit, out var players))
						{
							BossToPlayers[vBloodUnit].Add(player);
						}
						else
						{
							BossToPlayers[vBloodUnit] = new()
							{
								player
							};
						}

						var action = () => AnnounceBossKill(vBloodUnit, BossToPlayers[vBloodUnit]);
						ActionScheduler.RunActionOnceAfterFrames(action, 30);
					}
				}
			}
		}

		private static void AnnounceBossKill(PrefabGUID vBloodUnit, HashSet<Player> players)
		{
			// Announcement logic remains the same
			List<string> coloredNameList = players.Select(player => $"{player.FullName.Colorify(ExtendedColor.ClanNameColor)}").ToList();
			string playerNamesString;

			if (coloredNameList.Count > 2)
			{
				playerNamesString = $"{string.Join(", ", coloredNameList.Take(Math.Min(5, coloredNameList.Count - 1)))}...";
			}
			else if (coloredNameList.Count == 2)
			{
				playerNamesString = $"{coloredNameList[0]} and {coloredNameList[1]}";
			}
			else
			{
				playerNamesString = coloredNameList.First();
			}

			var haveOrHas = players.Count > 1 ? "have" : "has";

			if (!ModCore.Data.VBloodData.VBloodPrefabToName.TryGetValue(vBloodUnit, out var name))
			{
				name = vBloodUnit.LookupName();
			}

			var timeElapsed = Helper.GetServerTimeAdjusted();
            Helper.SendSystemMessageToAllClients($"{playerNamesString} {haveOrHas} achieved a server first kill on {name.Warning()} in {Helper.FormatTime(timeElapsed).White()}!");
		}
	}


/*	[HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.UnlockProgression))]
	public static class VBloodSystemPatch2
	{
		public static void Prefix(VBloodSystem __instance, EntityManager entityManager, ProgressionUtility.UpdateUnlockedJobData progressionJobData, PrefabGUID vBloodUnit, Entity userEntity, EntityCommandBuffer commandBuffer, PrefabLookupMap prefabMapping, Entity progressionEntity, bool logOnDuplicate = true)
		{
*//*			var player = PlayerService.GetPlayerFromUser(userEntity);
			if (AchievementsConfig.Config.RewardAllBosses || AchievementsConfig.Config.BossesToReward.Contains(vBloodUnit))
			{
				if (AchievementsConfig.Config.UsePhysicalCurrency)
				{
					Helper.AddItemToInventory(player, AchievementsConfig.Config.RewardPrefabGUID, AchievementsConfig.Config.QuantityPerBossKill, out var itemEntity);
				}
				else
				{
					PointsManager.AddPointsToPlayer(player, PointsType.Main, AchievementsConfig.Config.QuantityPerBossKill);
				}

				player.ReceiveMessage($"You got {AchievementsConfig.Config.QuantityPerBossKill.ToString().Warning()} {PointsModConfig.Config.MainVirtualCurrencyName.Warning()} for killing a boss for the first time".Success());
			}*//*
		}
	}*/
}
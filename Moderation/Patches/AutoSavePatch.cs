using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Scripting;
using ProjectM.Gameplay.Systems;
using ProjectM.Shared.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using ModCore;
using ModCore.Data;
using ModCore.Events;
using ModCore.Helpers;
using ModCore.Models;
using ModCore.Services;

namespace Moderation.Patches
{
	[HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
	public static class AutoSavePatch
	{
		public static void Prefix(TriggerPersistenceSaveSystem __instance)
		{
			ModerationModDataStorage.Save();
		}
	}
}

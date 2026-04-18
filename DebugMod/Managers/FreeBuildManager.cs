using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using ModCore.Events;
using ModCore.Listeners;
using ModCore.Models;
using ModCore;
using ModCore.Helpers;
using ModCore.Data;
using ModCore.Services;
using System.Threading;
using ProjectM.Gameplay.Systems;
using static ModCore.Helpers.Helper;
using ModCore.Factories;
using ProjectM.Scripting;
using HarmonyLib;
using static ProjectM.Network.InteractEvents_Client;
using Unity.Collections;
using Stunlock.Core;
using ProjectM.Gameplay.Scripting;
using static ModCore.Events.GameEvents;

namespace DebugMod.Managers
{
    public static class FreeBuildManager
    {
        public static void Initialize()
        {
            GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
        }


        public static void Dispose()
        {
            GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
        }

        public static void HandleOnPlayerPlacedStructure(Player player, Entity eventEntity, BuildTileModelEvent buildTileModel)
        {
            if (buildTileModel.PrefabGuid == Prefabs.TM_BloodFountain_CastleHeart)
            {
                eventEntity.Destroy();
                player.ReceiveMessage("You cannot place a heart while free build is enabled!");
            }
        }

        public static bool IsFreeBuildEnabled()
        {
            return VWorld.Server.GetExistingSystemManaged<ServerDebugSettingsSystem>().ServerDebugSettings.FreeBuildingPlacementEnabled;
        }
    }
}
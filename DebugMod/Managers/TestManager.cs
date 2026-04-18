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
using Unity.Mathematics;

namespace DebugMod.Managers
{
    public static class TestManager
    {
        public static void Initialize()
        {
            //GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
        }


        public static void Dispose()
        {
            //GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
        }

        public static void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
        {
            
        }
    }
}
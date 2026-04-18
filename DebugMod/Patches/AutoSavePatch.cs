using HarmonyLib;
using Il2CppInterop.Runtime;
using ModCore;
using ModCore.Data;
using ModCore.Events;
using ModCore.Helpers;
using ModCore.Models;
using ModCore.Services;
using ProjectM;
using ProjectM.Gameplay.Clan;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Core;
using Stunlock.Network;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DebugMod
{
/*    [HarmonyPatch(typeof(ActiveWorkstationSequenceSystem), nameof(ActiveWorkstationSequenceSystem.OnUpdate))]
    public static class VBloodSystemPatch
    {
        public static void Prefix(ActiveWorkstationSequenceSystem __instance)
        {

            __instance.__query_791096287_0.LogComponentTypes();
        }
    }*/
}

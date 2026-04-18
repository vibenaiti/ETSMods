using HarmonyLib;
using ProjectM.Shared;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using ModCore;
using static ProjectM.Network.InteractEvents_Client;

namespace QuickStash.Patches
{
    [HarmonyPatch(typeof(ValidNameChecker), nameof(ValidNameChecker.GetValidNameResult))]
    public static class ValidNameCheckerPatch
    {
        static void Prefix(string name, ValidNameMode nameMode, ref ValidNameResult __result)
        {
            if (nameMode != ValidNameMode.PlayerNames)
            {
                __result = ValidNameResult.Success;
            }
        }

        static void Postfix(string name, ValidNameMode nameMode, ref ValidNameResult __result)
        {
            if (nameMode != ValidNameMode.PlayerNames)
            {
                __result = ValidNameResult.Success;
            }
        }
    }
}

using HarmonyLib;
using ProjectM;
using ModCore.Services;

namespace ModCore.Patches;

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnGameDataInitialized))]
public static class InitializationPatch1
{
	[HarmonyPostfix]
	public static void OneShot_AfterLoad_InitializationPatch1(ServerBootstrapSystem __instance)
	{
		VWorld.SetServerWorld(__instance.World);
		var action = () => Plugin.OnServerStart();
		ActionScheduler.RunActionOnceAfterDelay(action, 3);
		Plugin.Harmony.Unpatch(typeof(ServerBootstrapSystem).GetMethod("OnGameDataInitialized"), typeof(InitializationPatch1).GetMethod("OneShot_AfterLoad_InitializationPatch1"));
	}
}

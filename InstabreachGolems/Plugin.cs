using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM;
using System.Reflection;
using ModCore.Services;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Events;
using InstabreachGolem.Managers;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Stunlock.Core;
using ModCore;
using ModCore.Data;
using ModCore.Helpers;

namespace InstabreachGolem;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
	internal static Harmony Harmony;
	internal static ManualLogSource PluginLog;

	public override void Load()
	{
		PluginLog = Log;
		// Plugin startup logic
		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
		// Harmony patching
		Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		Harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
    	GameEvents.OnServerStart += OnServerStart;
    }

	public override bool Unload()
	{
		Harmony?.UnpatchSelf();
        InstabreachGolemManager.Dispose();
        CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        InstabreachGolemConfig.Dispose();
        return true;
    }

	

	private static void ModifyPrefabs()
	{
		var prefabsToModify = new List<PrefabGUID>
		{
			Prefabs.AB_Shapesfhit_Golem_T02_MeleeAttack_Hit,
            Prefabs.AB_Shapesfhit_Golem_T02_FistSlam_Hit,
            Prefabs.AB_Shapeshift_Golem_T02_GroundSlam_Hit,
            Prefabs.AB_Shapeshift_Golem_T02_Ranged_Projectile,
        };

		foreach (var prefab in prefabsToModify)
		{
			var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(prefab);
            var buffer = prefabEntity.ReadBuffer<DealDamageOnGameplayEvent>();
            for (var i = 0; i < buffer.Length; i++)
            {
                var dealDamageOnGameplayEvent = buffer[i];
                dealDamageOnGameplayEvent.Parameters.MaterialModifiers.StoneStructure = 60f;
                buffer[i] = dealDamageOnGameplayEvent;
            }
        }
	
	}
	private static void OnServerStart()
	{

		/*InstabreachGolemConfig.Initialize();
        CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
		ModifyPrefabs();
        InstabreachGolemManager.Initialize();*/
	}
}
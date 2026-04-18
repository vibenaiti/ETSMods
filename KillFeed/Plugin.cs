using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using System.Reflection;
using ModCore.Data;
using ModCore.Helpers;
using ModCore.Services;
using ModCore;
using KillFeed.Patches;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Events;

namespace KillFeed;


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
        DeathHandler.Dispose();
        CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        KillFeedConfig.Dispose();
		StatsRecord.Save();
        return true;
    
	}

    
	private static void OnServerStart()
	{

		CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        KillFeedConfig.Initialize();
        DeathHandler.Initialize();
        StatsRecord.Load();
	}
}
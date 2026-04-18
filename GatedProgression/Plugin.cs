using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM;
using System.Reflection;
using ModCore.Services;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Events;

namespace GatedProgression;


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
        LockedBossesManager.Dispose();
        CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        GatedProgressionConfig.Dispose();
        return true;
    
	}

    
	private static void OnServerStart()
	{

		GatedProgressionConfig.Initialize();
        DataStorage.Load();
        if (DataStorage.Data.LockedBosses.Count > 0)
		{
			LockedBossesManager.Initialize();
		}
        CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
	}
}
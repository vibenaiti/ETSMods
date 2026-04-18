using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM;
using System.Reflection;
using ModCore.Services;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Events;
using ModCore;
using TemplateMod.Managers;

namespace TemplateMod;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.Bloodstone")]
[Bloodstone.API.Reloadable]
public class Plugin : BasePlugin, IRunOnInitialized
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
    }

	public override bool Unload()
	{
		Harmony?.UnpatchSelf();
        TestManager.Dispose();
        CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        TemplateModConfig.Dispose();
        return true;
    }

	public void OnGameInitialized()
	{
		if (!HasLoaded())
		{
			ActionScheduler.RunActionOnceAfterDelay(OnGameInitialized, 3);
			return;
		}
        TemplateModConfig.Initialize();
        CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        TestManager.Initialize();
        TemplateModDataStorage.Load();
    }

	private static bool HasLoaded()
	{
		// Hack, check to make sure that entities loaded enough because this function
		// will be called when the plugin is first loaded, when this will return 0
		// but also during reload when there is data to initialize with.
		var collectionSystem = VWorld.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
		return collectionSystem?.SpawnableNameToPrefabGuidDictionary.Count > 0;
	}
}

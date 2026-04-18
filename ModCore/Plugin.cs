using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM;
using ModCore.Events;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Listeners;

namespace ModCore;


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
		CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
	}

	public override bool Unload()
	{
		try
		{
			Core.Dispose();
			Harmony?.UnpatchSelf();
			CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Ran into error while unloading: {e.ToString()}");
		}
		return true;
	}

	public static void OnServerStart()
	{
		PluginLog.LogInfo("Running OnServerStart code");
		Initialize(); // Initialize Core systems before notifying subscribers

		// Invoke each subscriber individually so one failure doesn't break the chain
		if (GameEvents.OnServerStart == null) return;
		foreach (var subscriber in GameEvents.OnServerStart.GetInvocationList())
		{
			try
			{
				subscriber.DynamicInvoke();
			}
			catch (Exception e)
			{
				PluginLog.LogError($"[ModCore] OnServerStart subscriber '{subscriber.Method.DeclaringType?.FullName}' threw: {e.Message}");
			}
		}
	}

	public static void Initialize()
	{
		if (!HasLoaded())
		{
			return;
		}
		Core.Initialize();
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

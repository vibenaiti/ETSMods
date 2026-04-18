using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Services;
using ProjectM;
using ModCore.Events;

namespace QuickStash
{
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
            CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
            QuickStashConfig.Dispose();
            RenameManager.Dispose();
            return true;
        }
    
	private static void OnServerStart()
	{

		QuickStashConfig.Initialize();
            RenameManager.Initialize();
            CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
	}
}
}

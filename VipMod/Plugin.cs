using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM;
using System.Reflection;
using ModCore.Services;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Network;
using Unity.Entities;
using System;
using VipMod.Managers;
using ModCore.Events;

namespace VipMod;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
	internal static Harmony Harmony;
	internal static ManualLogSource PluginLog;

    /*delegate void OriginalTryAuthenticateDelegate(ulong platformId, UserContentFlags defaultUserContent, bool isBot, ref bool isReconnect, ref bool connectedAsAdmin, ref ConnectionStatusChangeReason error, ref User user, ref Entity userEntity);
    static OriginalTryAuthenticateDelegate originalTryAuthenticate;*/

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
        VipManager.Dispose();
        CommandHandler.UnregisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        VipModConfig.Dispose();
        return true;
    }

/*    private void DetourTryAuthenticateMethod()
    {
        Type targetType = typeof(ServerBootstrapSystem);
        Type[] parameterTypes = {
            typeof(ulong), // platformId
            typeof(UserContentFlags), // defaultUserContent
            typeof(bool), // isBot
            typeof(bool).MakeByRefType(), // isReconnect
            typeof(bool).MakeByRefType(), // connectedAsAdmin
            typeof(ConnectionStatusChangeReason).MakeByRefType(), // error
            typeof(User).MakeByRefType(), // user
            typeof(Entity).MakeByRefType() // userEntity
        };
        OriginalTryAuthenticateDelegate myTryAuthenticate = MyTryAuthenticate;
        // Adjusted call to include parameter types
        var detour = DetourUtils.Create(targetType, nameof(ServerBootstrapSystem.TryAuthenticate), parameterTypes, myTryAuthenticate, out originalTryAuthenticate);
    }

    // Your new implementation of the method
    private static void MyTryAuthenticate(ulong platformId, UserContentFlags defaultUserContent, bool isBot, ref bool isReconnect, ref bool connectedAsAdmin, ref ConnectionStatusChangeReason error, ref User user, ref Entity userEntity)
    {
*//*        Plugin.PluginLog.LogInfo($"test1: {VipManager.ConnectedNormals.Count} {VipModConfig.Config.MaxPlayersNonVips}");
        if (!VipModConfig.Config.Vips.ContainsKey(platformId) && VipManager.ConnectedNormals.Count >= VipModConfig.Config.MaxPlayersNonVips)
        {
            error = ConnectionStatusChangeReason.ServerFull;
            Plugin.PluginLog.LogInfo("test2");
        }*//*

        originalTryAuthenticate(platformId, defaultUserContent, isBot, ref isReconnect, ref connectedAsAdmin, ref error, ref user, ref userEntity);
    }*/


	private static void OnServerStart()
	{
		VipModConfig.Initialize();
        CommandHandler.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
        VipManager.Initialize();
        /*DetourTryAuthenticateMethod();*/
	}
}

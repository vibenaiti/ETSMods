using HarmonyLib;
using ProjectM;
using Stunlock.Network;
using ModCore.Services;
using System;
using ModCore.Helpers;
using ModCore.Events;
using ModCore.Models;
using ModCore.Frameworks.CommandFramework;

namespace ModCore.Patches;

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
public static class OnUserDisconnectedPatch
{
	private static AdminAuthSystem adminAuthSystem = VWorld.Server.GetExistingSystemManaged<AdminAuthSystem>();

	public static void Prefix (ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
	{
		try
		{
			if (__instance._NetEndPointToApprovedUserIndex.TryGetValue(netConnectionId, out int userIndex))
			{
				if (userIndex >= 0 && userIndex < __instance._ApprovedUsersLookup.Length)
				{
					var serverClient = __instance._ApprovedUsersLookup[userIndex];
					var User = serverClient.UserEntity;
					var player = PlayerService.GetPlayerFromUser(User);
					PlayerService.OnlinePlayersWithUsers.Remove(player);
					PlayerService.OnlinePlayersWithCharacters.Remove(player);
					PlayerService.OnOnlinePlayerAmountChanged?.Invoke();
					GameEvents.OnPlayerDisconnected?.Invoke(player);
				}
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception in disconnect patch: {e.ToString()}");
		}
	}
}

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class OnUserConnectedPatch
{
	private static AdminAuthSystem adminAuthSystem = VWorld.Server.GetExistingSystemManaged<AdminAuthSystem>();
	public static bool HasLaunched = false;

	public static void Prefix (ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
	{
		try
		{
			if (__instance._NetEndPointToApprovedUserIndex.TryGetValue(netConnectionId, out int userIndex))
			{
				if (userIndex >= 0 && userIndex < __instance._ApprovedUsersLookup.Length)
				{
					var serverClient = __instance._ApprovedUsersLookup[userIndex];
					var User = serverClient.UserEntity;
					if (User.Exists())
					{
						var player = PlayerService.GetPlayerFromUser(User);
						GameEvents.OnPlayerConnected?.Invoke(player);
						if (PlayerService.OnlinePlayersWithUsers.Add(player))
						{
							PlayerService.OnOnlinePlayerAmountChanged?.Invoke();
						}
						if (player.Character.Exists())
						{
							PlayerService.OnlinePlayersWithCharacters.Add(player);
							if (!player.ControlledEntity.Exists() || player.ControlledEntity != player.Character)
							{
								Helper.ControlOriginalCharacter(player);
							}

							
							if (player.IsAdmin)
							{
								Helper.UnlockAllContent(player.ToFromCharacter());
							}
						}
						else
						{
							PlayerSpawnHandler.PlayerFirstTimeSpawn[player] = true;
						}
					}
				}
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception in connect patch: {e.ToString()}");
		}
	}
}

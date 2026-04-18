using HarmonyLib;
using Unity.Collections;
using ProjectM.Network;
using ProjectM.Gameplay.Clan;
using ModCore.Services;
using ModCore.Events;
using System;
using ProjectM;
using static ProjectM.Network.ClanEvents_Client;

namespace ModCore.Patches;


[HarmonyPatch(typeof(ClanSystem_Server), nameof(ClanSystem_Server.OnUpdate))]
public static class ClanSystem_ServerPatch
{
	public static void Prefix(ClanSystem_Server __instance)
	{
		if (GameEvents.OnPlayerInvitedToClan != null)
		{
			var inviteEventEntities = __instance._InvitePlayerToClanQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in inviteEventEntities)
			{
				try
				{
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					GameEvents.OnPlayerInvitedToClan?.Invoke(player, entity);
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
					continue;
				}
			}
			inviteEventEntities.Dispose();
		}

		if (GameEvents.OnPlayerRespondedToClanInvite != null)
		{
			var clanAcceptEntities = __instance._ClanInviteResponseQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in clanAcceptEntities)
			{
				try
				{
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					var clanInviteResponse = entity.Read<ClanEvents_Client.ClanInviteResponse>();
					GameEvents.OnPlayerRespondedToClanInvite?.Invoke(player, entity, clanInviteResponse);
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
					continue;
				}
			}
			clanAcceptEntities.Dispose();
		}

		if (GameEvents.OnPlayerKickedFromClan != null)
		{
			var kickEventEntities = __instance._KickRequestQuery.ToEntityArray(Allocator.Temp);

			foreach (var entity in kickEventEntities)
			{
				try
				{
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					var clanKickEvent = entity.Read<ClanEvents_Client.Kick_Request>();
					GameEvents.OnPlayerKickedFromClan?.Invoke(player, entity, clanKickEvent);
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
					continue;
				}
			}
			kickEventEntities.Dispose();
		}

		if (GameEvents.OnPlayerLeftClan != null) 
		{
			var leaveEventEntities = __instance._LeaveClanEventQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in leaveEventEntities)
			{
				try
				{
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					var leaveClanEvent = entity.Read<ClanEvents_Client.LeaveClan>();
					GameEvents.OnPlayerLeftClan?.Invoke(player, entity, leaveClanEvent);
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
					continue;
				}
			}
			leaveEventEntities.Dispose();
		}

		if (GameEvents.OnPlayerCreatedClan != null)
		{
			var createClanEntities = __instance._CreateClanEventQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in createClanEntities)
			{
				try
				{
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					var createClanEvent = entity.Read<ClanEvents_Client.CreateClan_Request>();
					GameEvents.OnPlayerCreatedClan?.Invoke(player, entity, createClanEvent);
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
					continue;
				}
			}
			createClanEntities.Dispose();
		}
	}
}

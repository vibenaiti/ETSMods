using ModCore.Services;
using ProjectM;

namespace ModCore.Helpers;

public static partial class Helper
{
	public static void SendSystemMessageToAllClients (string message)
	{
		var fixedMessage = new Unity.Collections.FixedString512Bytes(message);
		ServerChatUtils.SendSystemMessageToAllClients(
			VWorld.Server.EntityManager,
			ref fixedMessage
		);
	}

	public static void NotifyAllAdmins(string message)
	{
		foreach (var player in PlayerService.OnlinePlayersWithUsers)
		{
			if (player.IsAdmin)
			{
				player.ReceiveMessage(message);
			}
		}
		Plugin.PluginLog.LogInfo(message);
	}
}

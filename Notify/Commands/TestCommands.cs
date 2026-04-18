using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using VArenaNotify;

namespace TemplateMod.Commands
{
    public class TestCommands
    {
		[Command("autoannouncement", description: "Used for debugging", adminOnly: true)]
		public void AutoAnnouncementCommand(Player sender, int index)
		{
			NotifyManager.SendAutoAnnouncement(index);
		}
		
		[Command("playerwelcomed", description: "Used for debugging", adminOnly: true)]
		public void PlayerWelcomedCommand(Player sender)
		{
			NotifyManager.SendWelcomeMessage(sender);
		}
		
		[Command("playernew", description: "Used for debugging", adminOnly: true)]
		public void PlayerNewCommand(Player sender)
		{
			NotifyManager.SendPlayerNewAnnouncement(sender);
		}
		
		[Command("playerconnected", description: "Used for debugging", adminOnly: true)]
		public void PlayerConnectedCommand(Player sender)
		{
			NotifyManager.SendPlayerConnectedAnnouncement(sender);
		}
		
		[Command("playerdisconnected", description: "Used for debugging", adminOnly: true)]
		public void PlayerDisconnectedCommand(Player sender)
		{
			NotifyManager.SendPlayerDisconnectedAnnouncement(sender);
		}
	}
}

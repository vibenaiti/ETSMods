using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using Moderation;

namespace Moderation.Commands
{
    public class ModerationCommands
    {
		[Command("mute", description: "Used for debugging", adminOnly: true)]
		public void MuteCommand(Player sender, Player player)
		{
            if (ModerationManager.MutePlayer(player))
            {
                sender.ReceiveMessage($"{player} has been muted from global chat.");
                player.ReceiveMessage($"You have been muted from global chat.");
            }
            else
            {
                sender.ReceiveMessage($"{player.Name.Colorify(ExtendedColor.ClanNameColor)} is already muted!".Error());
            }
        }

        [Command("unmute", description: "Used for debugging", adminOnly: true)]
        public void UnmuteCommand(Player sender, Player player)
        {
            if (ModerationManager.UnmutePlayer(player))
            {
                sender.ReceiveMessage($"{player} has been unmuted from global chat.");
                player.ReceiveMessage($"You have been unmuted from global chat.");
            }
            else
            {
                sender.ReceiveMessage($"Couldn't unmute {player.Name.Colorify(ExtendedColor.ClanNameColor)} because they aren't muted".Error());
            }
        }

        [Command("ban", description: "Used for debugging", adminOnly: true)]
        public void BanCommand(Player sender, Player player)
        {
            Helper.BanPlayer(player);
            Helper.SendSystemMessageToAllClients($"{player} has been banned.");
        }

        [Command("unban", description: "Used for debugging", adminOnly: true)]
        public void UnbanCommand(Player sender, Player player)
        {
            Helper.UnbanPlayer(player);
            Helper.SendSystemMessageToAllClients($"{player} has been unbanned.");
        }

        [Command("kick", description: "Used for debugging", adminOnly: true)]
        public void KickCommand(Player sender, Player player, string reason)
        {
            Helper.KickPlayer(player);
            Helper.SendSystemMessageToAllClients($"{player} has been kicked. Reason: {reason}");
        }

        [Command("removeclancd", description: "Used for debugging", adminOnly: true, aliases: ["resetclancd"])]
        public void RemoveClanCDCommand(Player sender, Player player)
        {
            ModerationModDataStorage.Data.PlayersToLastClan.Remove(player.SteamID);
            ModerationModDataStorage.Save();
            sender.ReceiveMessage($"Removed clan join cooldown from: {player.FullName.Colorify(ExtendedColor.ClanNameColor)}");
            player.ReceiveMessage("An admin has removed your clan-joining cooldown");
        }
    }
}

using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using VipMod.Managers;
using ModCore.Services;

namespace VipMod.Commands
{
    public class VipCommands
    {
        [Command("vips", description: "Used for debugging", adminOnly: true)]
        public void VipCommand(Player sender)
        {
            sender.ReceiveMessage($"VIP players: {VipManager.ConnectedVips.Count}");
            sender.ReceiveMessage($"Normal players: {VipManager.ConnectedNormals.Count}");
            var adminCount = 0;
            foreach (var player in PlayerService.OnlinePlayersWithUsers)
            {
                if (player.IsAdmin)
                {
                    adminCount++;
                }
            }
            sender.ReceiveMessage($"Admin players: {adminCount}");

            foreach (var player in PlayerService.OnlinePlayersWithUsers)
            {
                if (VipManager.ConnectedVips.Contains(player))
                {
                    Plugin.PluginLog.LogInfo($"VIP - {player.Name}");
                }
                else if (VipManager.ConnectedNormals.Contains(player))
                {
                    Plugin.PluginLog.LogInfo($"Normal - {player.Name}");
                }
                else if (player.IsAdmin)
                {
                    Plugin.PluginLog.LogInfo($"Admin - {player.Name}");
                }
                else
                {
                    Plugin.PluginLog.LogInfo($"Uncategorized - {player.Name}");
                }
            }
        }
    }
}

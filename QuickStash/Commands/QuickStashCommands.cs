using ProjectM.Network;
using ProjectM;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Models;
using ModCore.Helpers;

namespace QuickStash.Commands
{
    public class QuickStashCommands
    {
        [Command("stash", description: "Compulsively Count on all nearby allied chests, unless their name ends with 9", aliases: new string[]{"cc", "stahs", "stasj", "stasg", "stash'", "staszh"}, includeInHelp:true, category:"QoL", adminOnly: false)]
        public static void QuickStashCommand(Player sender)
        {
            if (QuickStashConfig.Config.Enabled || sender.IsAdmin)
            {
                if (QuickStashServer.MergeInventories(sender))
                {
                    sender.ReceiveMessage("Stashed all items".Success());
                }
            }
            else
            {
                sender.ReceiveMessage("QuickStash is temporarily disabled");
            }
        }

        [Command("clearchests", description: "Deletes all items in chests whose names end with a - or 8", aliases: new string[]{"clc", "trash"}, adminOnly: false)]
        public static void CleanChestsCommand(Player sender)
        {
            if (QuickStashConfig.Config.Enabled || sender.IsAdmin)
            {
                if (QuickStashServer.CleanChests(sender))
                {
                    sender.ReceiveMessage("Deleted all items in marked chests".Success());
                }
            }
            else
            {
                sender.ReceiveMessage("QuickStash is temporarily disabled");
            }
        }
    }
}

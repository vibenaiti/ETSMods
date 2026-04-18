using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using ModCore.Data;

namespace InstabreachGolem.Commands
{
    public class TestCommands
    {
        [Command("breach-golem", description: "Turns admin into a breach golem", adminOnly: true, aliases: ["bg", "bgolem"])]
        public void GolemCommand(Player sender)
        {
            Helper.BuffPlayer(sender, Prefabs.AB_Shapeshift_Golem_T02_Buff, out var buffEntity, Helper.NO_DURATION);
        }
    }
}

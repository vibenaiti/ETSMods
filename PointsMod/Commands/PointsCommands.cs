using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;

namespace VipMod.Commands
{
    public class PointsCommands
    {
		[Command("points", description: "See how many points you have", adminOnly: false, includeInHelp: true, aliases: ["witchdust"], category: "Shop")]
		public void PointsCommand(Player sender)
		{
			sender.ReceiveMessage($"You have {PointsManager.GetPlayerPoints(sender, PointsType.Cosmetic)} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()}".White());
		}

        [Command("getpoints", description: "See how many points you have", adminOnly: true)]
        public void GetPointsCommand(Player sender, Player target)
        {
            sender.ReceiveMessage($"{target.Name} has {PointsManager.GetPlayerPoints(target, PointsType.Cosmetic)} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()}".White());
        }

        //[Command("givepoints", description: "Give points to another player", adminOnly: false, includeInHelp: false, category: "QoL")]
        public void GivePointsCommand(Player sender, Player target, int amount)
        {
            if (sender.IsAlliedWith(target))
            {
                if (PointsManager.HasEnoughPoints(sender, PointsType.Cosmetic, amount))
                {
                    PointsManager.RemovePointsFromPlayer(sender, PointsType.Cosmetic, amount);
                    sender.ReceiveMessage($"You have sent {PointsManager.GetPlayerPoints(sender, PointsType.Cosmetic).ToString().Warning()} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()} to {target.FullName.Colorify(ExtendedColor.ClanNameColor)}. New total: {PointsManager.GetPlayerPoints(sender, PointsType.Cosmetic).ToString().Warning()}".White());
                    PointsManager.AddPointsToPlayer(target, PointsType.Cosmetic, amount);
                    target.ReceiveMessage($"You have received {PointsManager.GetPlayerPoints(sender, PointsType.Cosmetic).ToString().Warning()} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()} from {sender.FullName.Colorify(ExtendedColor.ClanNameColor)}. New total: {PointsManager.GetPlayerPoints(target, PointsType.Cosmetic).ToString().Warning()}".White());
                }                
            }
            else
            {
                sender.ReceiveMessage("You can only send points to clan members".Error());
            }
        }

        [Command("addpoints", description: "Adds points to a player", adminOnly: true)]
        public void AddPointsCommand(Player sender, Player target, int amount)
        {
            PointsManager.AddPointsToPlayer(target, PointsType.Cosmetic, amount);
            PointsManager.Save();
            sender.ReceiveMessage($"You have sent {amount.ToString().Warning()} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()} to {target.FullName.Colorify(ExtendedColor.ClanNameColor)}.".White());
            target.ReceiveMessage($"You have received {amount.ToString().Warning()} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()} from {sender.FullName.Colorify(ExtendedColor.ClanNameColor)}. New total: {PointsManager.GetPlayerPoints(target, PointsType.Cosmetic).ToString().Warning()}".White());
        }

        [Command("removepoints", description: "Removes points from a player", adminOnly: true)]
        public void RemovePointsCommand(Player sender, Player target, int amount)
        {
            PointsManager.RemovePointsFromPlayer(target, PointsType.Cosmetic, amount);
            PointsManager.Save();
            sender.ReceiveMessage($"You have removed {amount.ToString().Warning()} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()} from {target.FullName.Colorify(ExtendedColor.ClanNameColor)}.".White());
            target.ReceiveMessage($"You have been stripped of {amount.ToString().Warning()} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()} by {sender.FullName.Colorify(ExtendedColor.ClanNameColor)}. New total: {PointsManager.GetPlayerPoints(target, PointsType.Cosmetic)}".White().ToString().Warning());
        }

        [Command("setpoints", description: "Sets points for a player", adminOnly: true)]
        public void SetPointsCommand(Player sender, Player target, int amount)
        {
            PointsManager.SetPlayerPoints(target, PointsType.Cosmetic, amount);
            PointsManager.Save();
            sender.ReceiveMessage($"You have set {target.FullName.Colorify(ExtendedColor.ClanNameColor)}'s {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()} to {PointsManager.GetPlayerPoints(sender, PointsType.Cosmetic).ToString().Warning()}.".White());
            target.ReceiveMessage($"Your {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()} have been set to {PointsManager.GetPlayerPoints(sender, PointsType.Cosmetic).ToString().Warning()} by {sender.FullName.Colorify(ExtendedColor.ClanNameColor)}.".White());
        }
    }
}

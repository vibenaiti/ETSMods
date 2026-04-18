using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using ModCore.Data;
using ProjectM;
using ModCore;
using System.Linq;

namespace GatedProgression.Commands
{
    public class GatedProgressionCommands
    {
        [Command("lock-boss", description: "Locks a boss. Use ? to list bosses.", usage: ".lock-boss <name | ?>", aliases: ["lockboss"], adminOnly: true)]
		public void LockBossCommand(Player sender, string bossName)
		{
            if (bossName == "?")
            {
                sender.ReceiveMessage("Available bosses:".Colorify(ExtendedColor.LightServerColor));
                foreach (var boss in ModCore.Data.VBloodData.VBloodPrefabData)
                    sender.ReceiveMessage($"  {boss.OverrideName}".White());
                return;
            }
            if (!Helper.TryGetPrefabDataFromString(bossName, ModCore.Data.VBloodData.VBloodPrefabData, out var prefabData))
            {
                sender.ReceiveMessage($"Boss '{bossName}' not found. Use .lock-boss ? to see bosses.".Error());
                return;
            }
			DataStorage.Data.LockedBosses.Add(prefabData.PrefabGUID);
			DataStorage.Save();
            LockedBossesManager.LockBoss(prefabData.PrefabGUID);
            sender.ReceiveMessage($"Locked {prefabData.OverrideName}".White());
		}

        [Command("unlock-boss", description: "Unlocks a boss. Use ? to list bosses.", usage: ".unlock-boss <name | ?>", aliases: ["unlockboss"], adminOnly: true)]
        public void UnlockBossCommand(Player sender, string bossName)
        {
            if (bossName == "?")
            {
                sender.ReceiveMessage("Locked bosses:".Colorify(ExtendedColor.LightServerColor));
                if (DataStorage.Data.LockedBosses.Count == 0)
                    sender.ReceiveMessage("  None currently locked.".White());
                else
                    foreach (var guid in DataStorage.Data.LockedBosses)
                        sender.ReceiveMessage($"  {guid.LookupName()}".White());
                return;
            }
            if (!Helper.TryGetPrefabDataFromString(bossName, ModCore.Data.VBloodData.VBloodPrefabData, out var prefabData))
            {
                sender.ReceiveMessage($"Boss '{bossName}' not found. Use .unlock-boss ? to see locked bosses.".Error());
                return;
            }
            LockedBossesManager.UnlockBoss(prefabData.PrefabGUID);
            sender.ReceiveMessage($"Unlocked {prefabData.OverrideName}".White());
        }

        [Command("lock-group", description: "Locks a group of bosses", aliases: ["lockgroup"], adminOnly: true)]
        public void LockGroupCommand(Player sender, string groupName)
        {
            LockedBossesManager.LockGroup(groupName);
            sender.ReceiveMessage($"Locked {groupName}".White());
        }

        [Command("unlock-group", description: "Unlocks a group of bosses", aliases: ["unlockgroup"], adminOnly: true)]
        public void UnlockGroupCommand(Player sender, string groupName)
        {
            LockedBossesManager.UnlockGroup(groupName);
            sender.ReceiveMessage($"Unlocked {groupName}".White());
        }

        [Command("list-locked-bosses", description: "Lists the currently locked bosses", aliases: ["listlockedbosses"], adminOnly: true)]
        public void ListLockedBossesCommand(Player sender)
        {
            if (DataStorage.Data.LockedBosses.Count > 0)
            {
                sender.ReceiveMessage("Currently Locked Bosses:".Colorify(ExtendedColor.LightServerColor));
                foreach (var boss in DataStorage.Data.LockedBosses)
                {
                    sender.ReceiveMessage($"{boss.LookupName()}".White());
                }
            }
            else
            {
                sender.ReceiveMessage("No currently locked bosses".White());
            }
        }
    }
}

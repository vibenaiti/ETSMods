using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;

namespace TemplateMod.Commands
{
    public class TestCommands
    {
		[Command("templatetest", description: "Used for debugging", adminOnly: true)]
		public void TemplateTestCommand(Player sender)
		{
			sender.ReceiveMessage($"The test worked: {TemplateModConfig.Config.TestField}");
		}
	}
}

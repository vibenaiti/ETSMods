using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModCore.Models;
using ProjectM;

namespace ModCore;
public static class Globals
{
	public static double ServerStartTime = 0;
	public static DateTime ServerStartDateTime = DateTime.Now;
	public static Dictionary<Player, int> PlayerToMaxLevel = new();
	public static HashSet<Player> HungerGamesPlayers = new();
}

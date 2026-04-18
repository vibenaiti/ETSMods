using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModCore.Configs.ConfigDtos;

namespace PointsMod
{
    public static class Globals
    {
        public static int CurrencyMultiplier = 1;
        public static HashSet<string> ActiveEvents = new();
        public static bool EventActive
        {
            get { return GetEventActive(); }
        }

        private static bool GetEventActive()
        {
            return ActiveEvents.Count > 0;
        }
    }
}

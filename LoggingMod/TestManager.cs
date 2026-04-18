using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModCore.Events;
using ModCore.Models;

namespace LoggingMod
{
    public static class TestManager
    {
        public static void Initialize()
        {
            GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
        }

        public static void Dispose()
        {
            GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
        }

        public static void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
        {
            //do things
        }
    }
}

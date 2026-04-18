using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using ModCore;
using ProjectM;
using Unity.Transforms;
using Unity.Mathematics;
using ModCore.Data;
using Unity.Physics;
using Unity.Entities;
using ProjectM.CastleBuilding;
using ModCore.Services;
using static ModCore.Helpers.Helper;
using ProjectM.Network;
using System;
using UnityEngine;
using ModCore.Factories;

namespace Achievements.Commands
{
    public class AchievementsCommands
    {
        [Command("resettime", description: "Used for debugging", adminOnly: true)]
        public void ResetTimeCommand(Player sender)
        {
            DataStorageFile.Data.ServerStartTime = Helper.GetServerTime();
            DataStorageFile.Data.ServerStartDateTime = DateTime.Now;
            DataStorageFile.Save();
            sender.ReceiveMessage("Time reset done!".Success());
        }
    }
}

using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using ModCore.Events;
using ModCore.Listeners;
using ModCore.Models;
using ModCore;
using ModCore.Helpers;
using ModCore.Data;
using ModCore.Services;
using System.Threading;
using ProjectM.Gameplay.Systems;
using static ModCore.Helpers.Helper;
using ModCore.Factories;
using ProjectM.Scripting;
using HarmonyLib;
using static ProjectM.Network.InteractEvents_Client;
using Unity.Collections;
using Stunlock.Core;
using ProjectM.Gameplay.Scripting;
using Unity.Mathematics;

namespace DebugMod.Managers
{
    public static class UnstuckManager
    {
        private static HashSet<Player> QueuedUnstucks = new();
        public static void Initialize()
        {
            GameEvents.OnPlayerUnstuck += HandleOnPlayerUnstuck;
        }


        public static void Dispose()
        {
            GameEvents.OnPlayerUnstuck -= HandleOnPlayerUnstuck;
            QueuedUnstucks.Clear();
        }

        public static void HandleOnPlayerUnstuck(Player player, Entity eventEntity)
        {
            if (eventEntity.Exists() && !eventEntity.Has<CanFly>())
            {
                eventEntity.Destroy();

                if (!player.IsAlive) return;

                if (QueuedUnstucks.Contains(player))
                {
                    player.ReceiveMessage("You are already queued up to unstuck!".Error());
                    return;
                }

                if (Helper.BuffPlayer(player, Helper.CustomBuff1, out var buffEntity, 5))
                {
                    Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.AbilityCastImpair);
                    player.ReceiveMessage("You will be killed in 5 seconds!");
                    var action = () =>
                    {
                        QueuedUnstucks.Remove(player);
                        if (player.IsAlive)
                        {
                            var newEventEntity = Helper.CreateEntityWithComponents<FromCharacter, KillEvent, CanFly>();
                            newEventEntity.Write(player.ToFromCharacter());
                            newEventEntity.Write(new KillEvent
                            {
                                Filter = KillWhoFilter.OnlyLiving,
                                TargetNetworkId = player.Character.Read<NetworkId>(),
                                Who = KillWho.SpecificNetworkId
                            });
                        }
                    };
                    QueuedUnstucks.Add(player);
                    ActionScheduler.RunActionOnceAfterDelay(action, 5);
                }
            }
        }
    }
}
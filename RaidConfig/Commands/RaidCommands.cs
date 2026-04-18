using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using ProjectM;
using Unity.Collections;
using ModCore.Data;
using ModCore;
using ProjectM.CastleBuilding;
using RaidConfig.Managers;
using ProjectM.Shared;
using ModCore.Services;
using UnityEngine.Jobs;
using ProjectM.Network;
using System.Collections.Generic;
using Stunlock.Core;

namespace RaidConfig.Commands
{
    public class RaidCommands
    {
        //[Command("lockprison", description: "Prevents people from outside of your class from interacting with a prisoner", aliases: new string[] { "lock-prison" }, includeInHelp:false, category:"QoL", adminOnly: false)]
        public static void LockPrisonCommand(Player sender)
        {
            var interactor = sender.Character.Read<Interactor>();
            if (interactor.Target.Exists())
            {
                if (interactor.Target.GetPrefabGUID() != Prefabs.TM_SpecialStation_PrisonCell)
                {
                    sender.ReceiveMessage("You must be interacting with a prison cell to use this command".Error());
                    return;
                }
                if (!Team.IsAllies(interactor.Target.Read<Team>(), sender.Character.Read<Team>()))
                {
                    sender.ReceiveMessage("You cannot lock a prison cell that does not belong to you".Error());
                    return;
                }
                var heart = interactor.Target.Read<CastleHeartConnection>().CastleHeartEntity._Entity;
                var entities = Helper.GetStructuresFromCastleHeart(heart);
                bool hasLockedPrison = false;
                foreach (var entity in entities)
                {
                    if (entity.GetPrefabGUID() == Prefabs.TM_SpecialStation_PrisonCell)
                    {
                        if (Helper.HasBuff(entity, Prefabs.Buff_Voltage_Stage2))
                        {
                            hasLockedPrison = true;
                            break;
                        }
                    }
                }
                if (!hasLockedPrison)
                {
                    Helper.BuffEntity(interactor.Target, Prefabs.Buff_Voltage_Stage2, out var buffEntity, Helper.NO_DURATION);
                }
                else
                {
                    sender.ReceiveMessage($"You can only have {RaidConfigConfig.Config.NumberOfProtectedPrisons} protected prison cell(s) at your base".Error());
                }
                entities.Dispose();
            }
            
        }


        [Command("toggleraid", description: "Forces raid time on or toggles it back to normal", adminOnly: true)]
        public static void ToggleRaidCommand(Player sender, string mode)
        {
            if (mode == "on")
            {
                RaidTimeManager.CurrentRaidMode = RaidTimeManager.RaidMode.ForceOn;
                sender.ReceiveMessage("Forcibly enabled castle PvP".White());
            }
            else if (mode == "normal")
            {
                RaidTimeManager.CurrentRaidMode = RaidTimeManager.RaidMode.Normal;
                sender.ReceiveMessage("Set castle PVP to time-based".White());
            }
            else if (mode == "off")
            {
                RaidTimeManager.CurrentRaidMode = RaidTimeManager.RaidMode.ForceOff;
                sender.ReceiveMessage("Disabled castle PVP".White());
            }
            else
            {
                sender.ReceiveMessage("Invalid mode (should be on/off/normal)".Error());
            }
        }

        [Command("findshards", description: "Lists current shard owners", includeInHelp: true)]
        public static void FindShardsCommand(Player sender)
        {
            sender.ReceiveMessage("Listing shard owners: ");
            var shards = Helper.GetEntitiesByComponentTypes<Relic, LoseDurabilityOverTime>();
            HashSet<PrefabGUID> shardsToAccountFor = new(ShardData.ShardNecklaces);
            foreach (var shard in shards)
            {
                shardsToAccountFor.Remove(shard.GetPrefabGUID());
                if (ShardData.ShardsToTextColor.TryGetValue(shard.GetPrefabGUID(), out var color))
                {
                    var shardOwner = shard.Read<Equippable>().EquipTarget._Entity;
                    if (!shardOwner.Exists())
                    {
                        var containerEntity = shard.Read<InventoryItem>().ContainerEntity;
                        if (containerEntity.Exists())
                        {
                            if (containerEntity.Has<InventoryConnection>())
                            {
                                shardOwner = containerEntity.Read<InventoryConnection>().InventoryOwner;
                            }
                            else
                            {
                                shardOwner = containerEntity;
                            }
                        }
                    }
                    if (shardOwner.Exists())
                    {
                        if (!shardOwner.Has<PlayerCharacter>())
                        {
                            if (Helper.TryGetCurrentCastleTerritory(shardOwner, out var territoryEntity))
                            {
                                var heart = territoryEntity.Read<CastleTerritory>().CastleHeart;
                                if (heart.Exists())
                                {
                                    var player = PlayerService.GetPlayerFromUser(heart.Read<UserOwner>().Owner._Entity);
                                    sender.ReceiveMessage(player.Name.Colorify(color));
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            var player = PlayerService.GetPlayerFromCharacter(shardOwner);
                            sender.ReceiveMessage(player.Name.Colorify(color));
                            continue;
                        }
                    }
                    sender.ReceiveMessage("No Owner".Colorify(color));
                }
            }

            foreach (var shard in shardsToAccountFor)
            {
                if (ShardData.ShardsToTextColor.TryGetValue(shard, out var color))
                {
                    sender.ReceiveMessage("No Owner".Colorify(color));
                }
            }
        }
    }
}

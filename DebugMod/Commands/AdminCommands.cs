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
using System.Collections.Generic;
using UnityEngine;
using ModCore.Factories;
using Unity.Collections.LowLevel.Unsafe;
using ProjectM.Gameplay.Scripting;
using Il2CppInterop.Runtime;
using Unity.Collections;
using Il2CppSystem;
using ProjectM.Tiles;
using Stunlock.Core;
using DebugMod.Managers;

namespace DebugMod.Commands
{
    public class AdminCommands
    {
        [Command("stealth", description: "Stealth admin", adminOnly: true)]
        public void StealthCommand(Player sender)
        {
            if (Helper.BuffPlayer(sender, Prefabs.Admin_Observe_Invisible_Buff, out var buffEntity))
            {
                buffEntity.Add<CanFly>();
                ModifyBuff(buffEntity, BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.DisableMapCollision | BuffModificationTypes.ImmuneToSun | BuffModificationTypes.ImmuneToHazards, true);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    StatType = UnitStatType.CooldownRecoveryRate,
                    Priority = 100,
                    Modifier = 1,
                    Value = 100000
                }, true);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    StatType = UnitStatType.PrimaryAttackSpeed,
                    Priority = 100,
                    Modifier = 1,
                    Value = 5
                }, false);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    Priority = 100,
                    StatType = UnitStatType.MovementSpeed,
                    Modifier = 1,
                    Value = 20
                }, false);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    Priority = 100,
                    StatType = UnitStatType.PhysicalPower,
                    Modifier = 1,
                    Value = 0
                }, false);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    Priority = 100,
                    StatType = UnitStatType.ResourcePower,
                    Modifier = 1,
                    Value = 0
                }, false);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    Priority = 100,
                    StatType = UnitStatType.SpellPower,
                    Modifier = 1,
                    Value = 0
                }, false);


                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    Priority = 100,
                    StatType = UnitStatType.SiegePower,
                    Modifier = 1,
                    Value = 0
                }, false);
            }
        }

        [Command("rename", description: "test", adminOnly: true)]
        public unsafe static void RenamePlayerCommand(Player sender, Player target, string name)
        {
            Helper.RenamePlayer(target.ToFromCharacter(), name);
            sender.ReceiveMessage("Sent rename request".White());
        }

        [Command("removebufffromeveryone", adminOnly: true)]
        public unsafe static void RemoveBuffFromEveryoneCommand(Player sender, PrefabGUID buff)
        {
            foreach (var player in PlayerService.CharacterCache.Values)
            {
                Helper.RemoveBuff(player, buff);
            }
            sender.ReceiveMessage("Removed buff from all players".White());
        }

        [Command("kick-clan", description: "Removes a player from their clan", usage: ".kick-clan rendy",
            aliases: new string[] { "kickclan", "kick clan", "clan kick", "clankick" }, adminOnly: true)]
        public static void KickClanCommand(Player sender, Player player2)
        {
            Helper.RemoveFromClan(player2);
            sender.ReceiveMessage(
                $"{player2.Name.Colorify(ExtendedColor.ClanNameColor)} has been removed from their clan".White());
        }
        
        [Command("get-steamid", description: "get steam id of person", adminOnly: true)]
        public static void SteamIDCommand(Player sender, Player target)
        {
           sender.ReceiveMessage((target.Name + "'s steamID: " + target.SteamID.ToString().Emphasize()).White());
        }
        
        [Command("tristan", description: "tristan", adminOnly: true)]
        public static void TristanCommand(Player sender, Player target)
        {
            var ping = (int)(target.Character.Read<Latency>().Value * 1000);
            sender.ReceiveMessage($"{target.Name}'s latency is {ping.ToString().Emphasize()}ms.".White());
            sender.ReceiveMessage($"{target.Name}'s steamID is {target.SteamID.ToString().Emphasize()}.".White());

            List<string> weaponList = new List<string>();

            for (int i = 0; i < 9; i++)
            {
                if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, target.Character, i, out var item))
                {
                    if (item.ItemEntity._Entity.Exists() && item.ItemEntity._Entity.LookupName().Contains("Weapon"))
                    {
                        weaponList.Add($"{item.ItemEntity._Entity.LookupName().Split("_")[2]}".White());
                    }
                }
            }
		
            sender.ReceiveMessage($"{target.Name}'s weapons: {ToStringWithSpaces(weaponList)}".White());
        }
        
        [Command("log-steamids", description:"Logs steam ids of all connected players", adminOnly:true)]
        public static void LogSteamIds(Player sender)
        {
            Plugin.PluginLog.LogInfo($"Logging online players:");
            foreach (var Player in PlayerService.UserCache.Values)
            {
                if (Player.IsOnline)
                {
                    Plugin.PluginLog.LogInfo($"{Player.Name} {Player.SteamID}");
                }
            }
            sender.ReceiveMessage("Logged Steam IDs to Console.".Success());
        }
        
        public static string ToStringWithSpaces (List<string> _list)
        {
            string StringWithSpaces = "";
            foreach (string s in _list)
            {
                StringWithSpaces += s + " ";
            }

            return StringWithSpaces.Emphasize();
        }

        
        [Command("playerinfo", description: "test", adminOnly: false, category:"Misc", includeInHelp:true)]
        public static void PlayerInfoCommand(Player sender, Player target)
        {
            var clanMembers = target.GetClanMembers();
            foreach (var member in clanMembers)
            {
                sender.ReceiveMessage($"{member.FullName.Colorify(ExtendedColor.ClanNameColor)} {"<size=80%>LV.</size>".Emphasize()}{member.MaxLevel.ToString().Emphasize()}");
            }
        }

        [Command("revive", description: "Revives the target player", adminOnly: true)]
        public static void ReviveCommand(Player sender, Player target = null)
        {
            if (target == null)
            {
                target = sender;
            }
            Helper.RevivePlayer(target);
            sender.ReceiveMessage($"Revived {target.FullName.Colorify(ExtendedColor.ClanNameColor)}".White());
        }

        [Command("down", description: "Downs the target player", adminOnly: true)]
        public static void DownCommand(Player sender, Player target = null)
        {
            if (target == null)
            {
                target = sender;
            }
            Helper.SoftKillPlayer(target);
            sender.ReceiveMessage($"Downed {target.FullName.Colorify(ExtendedColor.ClanNameColor)}".White());
        }

        [Command("kill", description: "Kills the target player", adminOnly: true)]
        public static void KillCommand(Player sender, Player target = null)
        {
            if (target == null)
            {
                target = sender;
            }
            Helper.KillOrDestroyEntity(target.Character);
            sender.ReceiveMessage($"Killed {target.FullName.Colorify(ExtendedColor.ClanNameColor)}".White());
        }

        [Command("reloadadmin", description: "Reloads the admin list.", adminOnly: true)]
        public static void ReloadCommand(Player sender)
        {
            Core.adminAuthSystem._LocalAdminList.Refresh();
            sender.ReceiveMessage("Admin list reloaded!");
        }

        [Command("spawn-unit", description: "Used for debugging", adminOnly: true, aliases: ["su", "spawnunit"])]
        public static void SpawnUnitCommand(Player sender, PrefabGUID _prefab, int quantity = 1, int level = -1)
        {
            for (var i = 0; i < quantity; i++)
            {
                var unit = new Unit(_prefab);
                unit.Level = level;
                var spawnPosition = sender.User.Read<EntityInput>().AimPosition;
                UnitFactory.SpawnUnitWithCallback(unit, spawnPosition, (e) =>
                {

                });
            }
            sender.ReceiveMessage($"Spawned {_prefab.LookupName()}!".Success());
        }

        [Command("spawn-blood", description: "Used for debugging", adminOnly: true, aliases: ["sb", "spawnblood"])]
        public static void SpawnBloodCommand(Player sender, PrefabGUID unitType, BloodPrefabData bloodType, float quality = 100)
        {
            var spawnPosition = sender.User.Read<EntityInput>().AimPosition;
            PrefabSpawnerService.SpawnWithCallback(unitType, spawnPosition, (e) =>
            {
                e.Write(new BloodConsumeSource
                {
                    UnitBloodType = new ModifiablePrefabGUID(bloodType.PrefabGUID),
                    BloodQuality = quality
                });
            });

            sender.ReceiveMessage($"Spawned {bloodType.OverrideName} blood!".Success());
        }

        [Command("spawn-prefab", description: "Used for debugging", adminOnly: true, aliases: ["s", "spawn"])]
        public static void SpawnPrefabCommand(Player sender, PrefabGUID _prefab, bool snap = false, int spawnSnapMode = 5, int rotationMode = 1)
        {
            float3 spawnPosition;
            if (snap)
            {
                spawnPosition = Helper.GetSnappedHoverPosition(sender, (SnapMode)spawnSnapMode);
            }
            else
            {
                spawnPosition = sender.User.Read<EntityInput>().AimPosition;
            }
            PrefabSpawnerService.SpawnWithCallback(_prefab, spawnPosition, (e) =>
            {
                e.LogComponentTypes();
                sender.ReceiveMessage("Spawned prefab!".Success());
            }, rotationMode);
        }

        [Command("destroy-prefab", description: "Used for debugging", adminOnly: true, aliases: ["dp", "destroy"])]
        public static void DestroyPrefabCommand(Player sender, PrefabGUID prefab = default)
        {
            if (prefab != default)
            {
                var entities = Helper.GetHoveredEntitiesByPrefabGUID(sender, prefab);
                bool destroyed = false;
                foreach (var entity in entities)
                {
                    if (entity.Read<PrefabGUID>() == prefab)
                    {
                        Helper.DestroyEntity(entity);
                        sender.ReceiveMessage($"Killed entity: {entity.Read<PrefabGUID>().LookupName()}".Success());
                        destroyed = true;
                        break;
                    }
                }
                if (!destroyed) 
                {
                    sender.ReceiveMessage("Could not find an entity matching that PrefabGUID to destroy");
                }
            }
            else
            {
                Entity entity = Helper.GetHoveredTileModel(sender.User);
                
                if (!entity.Has<TileModel>())
                {
                    sender.ReceiveMessage($"No valid targeted entity".Error());
                    return;
                }
                
                if (entity.Has<User>())
                {
                    sender.ReceiveMessage($"Cannot destroy users".Error());
                    return;
                }
                
                if (entity.Has<PlayerCharacter>()) 
                {
                    sender.ReceiveMessage($"No valid targeted entity".Error());
                    return;
                }
                
                if (entity.Has<SpawnChainChild>())
                {
                    var spawnChainChild = entity.Read<SpawnChainChild>();
                    var spawnChain = spawnChainChild.SpawnChain;
                    Helper.DestroyEntity(spawnChain);
                }
                else
                {
                    Helper.DestroyEntity(entity);
                }
                sender.ReceiveMessage($"Killed entity: {entity.Read<PrefabGUID>().LookupName()}".Success());
            }
        }


        [Command("cast", description: "Used for debugging", adminOnly: true, aliases: ["c"])]
        public static void CastCommand(Player sender, PrefabGUID prefabGuid)
        {
            var fromCharacter = sender.ToFromCharacter();
            var clientEvent = new CastAbilityServerDebugEvent
            {
                AbilityGroup = prefabGuid,
                AimPosition = new Nullable_Unboxed<float3>(sender.User.Read<EntityInput>().AimPosition),
                Who = sender.Character.Read<NetworkId>()
            };
            Core.debugEventsSystem.CastAbilityServerDebugEvent(sender.User.Read<User>().Index, ref clientEvent, ref fromCharacter);
            sender.ReceiveMessage($"Cast {prefabGuid.LookupName()}");
        }

        [Command("renameclan", description: "Used for debugging", adminOnly: true, aliases: ["rename-clan"])]
        public static void RenameClanCommand(Player sender, string name)
        {
            var clanEntity = sender.Clan;
            if (clanEntity.Exists())
            {
                var clanTeam = clanEntity.Read<ClanTeam>();
                clanTeam.Name = new FixedString64Bytes(name);
                clanEntity.Write(clanTeam);
                var clanMembers = sender.GetClanMembers();
                foreach (var clanMember in clanMembers)
                {
                    ClanUtility.SetCharacterClanName(VWorld.Server.EntityManager, clanMember.User, clanTeam.Name);
                }
                sender.ReceiveMessage("You have renamed your clan to " + name.Colorify(ExtendedColor.ClanNameColor) + ".");
            }
            else
            {
                sender.ReceiveMessage("You aren't in a clan.".Error());
            }
        }
    }
}

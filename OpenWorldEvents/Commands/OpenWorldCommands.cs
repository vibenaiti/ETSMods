using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using ModCore.Factories;
using ModCore;
using ProjectM;
using ModCore.Data;
using ProjectM.Gameplay.Systems;
using System;
using Unity.Entities;
using static ModCore.Helpers.Helper;
using Il2CppSystem;
using ProjectM.CastleBuilding;
using ProjectM.Tiles;
using ProjectM.Roofs;
using Unity.Collections;
using System.Diagnostics;
using static ProjectM.Tiles.TileMapCollisionMath;
using Unity.Physics;
using ProjectM.Network;
using ProjectM.Terrain;
using OpenWorldEvents.Managers;
using UnityEngine.Jobs;
using DateTime = System.DateTime;
using System.Drawing;
using ModCore.Events;
using Unity.Transforms;
using Stunlock.Core;
using Unity.Mathematics;
using OpenWorldEvents.Models;

namespace OpenWorldEvents.Commands
{
	public class OpenWorldCommands
	{

        [Command("unequip", adminOnly: true)]
        public unsafe static void UnequipCommand(Player sender)
        {
			Helper.UnequipAllItems(sender);
        }

        [Command("startfog", adminOnly: true, aliases: ["startcursedmoon"])]
        public unsafe static void StartFogCommand(Player sender)
        {
            CursedFogManager.StartFog();
            sender.ReceiveMessage("Fog started!");
        }

        [Command("endfog", adminOnly: true, aliases: ["endcursedmoon"])]
        public unsafe static void EndFogCommand(Player sender)
        {
            CursedFogManager.EndFog();
            sender.ReceiveMessage("Fog ended!");
        }

        [Command("starthungergames", adminOnly: true)]
        public unsafe static void StartHungerGamesCommand(Player sender)
        {
            if (HungerGamesManager.HasInitialized)
            {
                sender.ReceiveMessage("Hunger games are already going on. Wait for them to end or do .endhungergames in order to start a new one");
            }
            else
            {
                HungerGamesManager.Initialize();
            }
        }

        [Command("endhungergames", adminOnly: true)]
        public unsafe static void EndHungerGamesCommand(Player sender)
        {
            HungerGamesManager.Dispose();
            sender.ReceiveMessage("Ended Hunger Games match");
        }

        [Command("startflashduels", adminOnly: true)]
        public unsafe static void StartFlashDuelsCommand(Player sender, bool isFullLoot = false)
        {
			if (FlashDuelsManager.HasInitialized)
			{
				sender.ReceiveMessage("Flash duels are already going on. Wait for them to end or do .endflashduels in order to start new ones");
			}
			else
			{
				FlashDuelsManager.Initialize(isFullLoot);
            }
        }

        [Command("startfleshduels", adminOnly: true, aliases: ["startfleshduel"])]
        public unsafe static void StartFlashDuelsCommand(Player sender)
        {
            if (FlashDuelsManager.HasInitialized)
            {
                sender.ReceiveMessage("Duels are already going on. Wait for them to end or do .endflashduels in order to start new ones");
            }
            else
            {
                FlashDuelsManager.Initialize(true);
            }
        }

        [Command("endflashduels", adminOnly: true, aliases: ["endfleshduels"])]
        public unsafe static void EndFlashDuelsCommand(Player sender)
        {
            FlashDuelsManager.Dispose();
			sender.ReceiveMessage("Ended all duels");
        }

        [Command("join", description: "Sign up for the announced event", adminOnly: false, includeInHelp: false, aliases: ["fullloot", "tribute"])]
        public unsafe static void JoinCommand(Player sender)
        {
			GameEvents.OnPlayerSignedUp?.Invoke(sender);
        }

        [Command("leave", description: "Leave an event", adminOnly: false)]
        public unsafe static void LeaveCommand(Player sender)
        {
            GameEvents.OnPlayerRequestedLeave?.Invoke(sender);
        }

        [Command("dominion", description: "Tells how many points each team has", adminOnly: false, includeInHelp: false)]
        public unsafe static void DominionCommand(Player sender)
        {
            if (DominionManager.MatchActive)
            {
                GameEvents.OnPlayerSpecialChat?.Invoke(sender, "dominion");
            }
            else
            {
                sender.ReceiveMessage("There is no current dominion event going on right now".Error());
            }
        }

        [Command("makemaze", description: "test", adminOnly: true)]
        public unsafe static void MakeMazeCommand(Player sender)
        {
            var width = 30; // Maze width (number of cells)
            var height = 30; // Maze height (number of cells)

            var generator = new MazeGenerator(width, height);
            generator.GenerateMaze();
            generator.CreateLoops(width / 2);
            var leftHand = generator.SimulateHandRule(new Point(0, 0), true);
            var rightHand = generator.SimulateHandRule(new Point(0, 0), false);
            generator.exit = generator.ChooseStairsLocation(leftHand, rightHand);
            generator.SaveMazeImage("maze.png");
        }

        [Command("reloadopenworldconfig", description: "Used for debugging", adminOnly: true)]
		public void ReloadOpenWorldConfigCommand (Player sender)
		{
			try
			{
				OpenWorldEventsConfig.Initialize();
				sender.ReceiveMessage("Reloaded open world config!".Success());
			}
			catch (System.Exception e)
			{
				sender.ReceiveMessage(e.ToString().Error());
			}
		}

		[Command("workermode", description: "Used for debugging", adminOnly: true)]
		public void WorkerModeCommand (Player sender)
		{
			if (WorkerModeManager.IsPlayerInWorkerMode(sender))
			{
				WorkerModeManager.DisableWorkerMode(sender);
			}
			else
			{
				WorkerModeManager.EnableWorkerMode(sender);
			}
		}

        [Command("startbosscontest", description: "Used for debugging", adminOnly: true)]
        public void StartBossContestCommand(Player sender, VBloodPrefabData vBlood = null)
        {
            if (BossContestManager.HasInitialized)
            {
                sender.ReceiveMessage("Boss contests are already going on. Wait for them to end or do .endbosscontest in order to start new ones");
            }
            else
            {
				if (vBlood != null)
				{
                    BossContestManager.Initialize(vBlood.PrefabGUID);
                }
				else
				{
                    BossContestManager.Initialize(PrefabGUID.Empty);
                }
            }
        }

        [Command("endbosscontest", description: "Used for debugging", adminOnly: true)]
        public void EndBossContestCommand(Player sender)
        {
            sender.ReceiveMessage("Ended boss contest");
            BossContestManager.Dispose();
        }

        [Command("spawn-event-horse", description: "Spawns an event horse at aim position", adminOnly: true, aliases: ["seh"])]
		public void SpawnHorseCommand (Player sender)
		{
			DonkeyManager.SpawnHorse(sender.User.Read<EntityInput>().AimPosition);
		}

        [Command("startdominion", description: "Used for debugging", adminOnly: true)]
        public void StartDominionCommand(Player sender)
        {
            DominionManager.SpawnRing(sender.AimPosition);
            sender.ReceiveMessage("Started a dominion match!");
        }


        [Command("enddominion", description: "Used for debugging", adminOnly: true)]
        public void EndDominionCommand(Player sender)
        {
            sender.ReceiveMessage("Ended dominion");
            DominionManager.Dispose();
        }

        [Command("spawnnormalhorse", description: "Used for debugging", adminOnly: true)]
        public void SpawnNormalHorseCommand(Player sender, int speed, int acceleration = 7, int rotation = 14)
        { 
            var horse = new Horse
            {
                Speed = speed,
                Acceleration = acceleration,
                Rotation = rotation
            };
            UnitFactory.SpawnUnitWithCallback(horse, sender.Position, (e) => { });
        }

        [Command("killhorse", description: "Used for debugging", adminOnly: true)]
		public void KillHorseCommand (Player sender)
		{
			var entity = Helper.GetHoveredTileModel<Mountable>(sender.User);
			DonkeyManager.KillHorseAndEndEvent(entity);
		}

        [Command("listchests", description: "Used for debugging", adminOnly: false)]
        public void ListChestsCommand(Player sender)
        {
			foreach (var kvp in ScavengerHuntChestManager.ChestNameToChests)
			{
				var chestCategory = kvp.Key;
				var chest = kvp.Value;
				if (ScavengerHuntChestConfig.Config.ScavengerHuntChestLoot.TryGetValue(chestCategory, out var chestData))
				{
					var worldRegion = Helper.GetWorldRegionFromPosition(chest.Read<Translation>().Value);
                    if (WorldRegionData.WorldRegionToString.TryGetValue(worldRegion, out var name))
					{
                        sender.ReceiveMessage($"{chestData.ChestName} - {name}");
                    }
					else
					{
                        sender.ReceiveMessage($"{chestData.ChestName}");
                    }
				}
			}
        }

        [Command("checkchest", description: "Used for debugging", adminOnly: true)]
        public void CheckChestCommand(Player sender, string chestName)
        {
            if (ScavengerHuntChestManager.TryFindChestByName(chestName, out var chest))
            {
                sender.ReceiveMessage($"This chest has not been looted yet!");
            }
			else
			{
                sender.ReceiveMessage($"This chest has been looted!");
            }
        }

        [Command("spawnchest", description: "Used for debugging", adminOnly: true)]
        public void SpawnChestCommand(Player sender, string chestName)
        {
			if (ScavengerHuntChestManager.TryFindChestByName(chestName, out var chest))
			{
				sender.ReceiveMessage("Cannot spawn that chest because one with its name already exists!".Error());
				return;
			}

            if (ScavengerHuntChestManager.SpawnChest(chestName, sender)) 
			{
				sender.ReceiveMessage("Chest successfully spawned!".Success());
			}
			else
			{
                sender.ReceiveMessage("Failed to spawn chest! Double-check the name".Error());
            }
        }

        [Command("movechest", description: "Used for debugging", adminOnly: true)]
        public void MoveChestCommand(Player sender, string chestName)
        {
            if (ScavengerHuntChestManager.TryFindChestByName(chestName, out var chest))
            {
				Helper.DestroyEntity(chest);
				ScavengerHuntChestManager.SpawnChest(chestName, sender);
                sender.ReceiveMessage("Chest successfully moved!".Success());
            }
            else
            {
                sender.ReceiveMessage("Could not find a chest with that name".Error());
            }
        }

		[Command("deletechest", description: "Used for debugging", adminOnly: true, aliases: ["destroychest", "removechest"])]
		public void DeleteChestCommand(Player sender, string chestName)
        {
            if (ScavengerHuntChestManager.TryFindChestByName(chestName, out var chest))
            {
				Helper.DestroyEntity(chest);
                ScavengerHuntChestManager.ChestNameToChests.Remove(chestName);
                sender.ReceiveMessage("Chest successfully destroyed!".Success());
            }
            else
            {
                sender.ReceiveMessage("Could not find a chest with that name".Error());
            }
        }

        [Command("startchest", description: "Used for debugging", adminOnly: true)]
		public void StartChestCommand(Player sender)
		{
			Unit unit = new Unit(Prefabs.TM_WorldChest_Epic_01_Full);
			UnitFactory.SpawnUnitWithCallback(unit, sender.Position, (e) =>
			{
				e.Remove<DropInInventoryOnSpawn>();
				e.Remove<DestroyWhenInventoryIsEmpty>();
				e.Remove<Interactable>();
				var destroyAfterDuration = e.Read<DestroyAfterDuration>();
				destroyAfterDuration.Duration = float.MaxValue;
				e.Write(destroyAfterDuration);

				foreach (var item in ChestConfig.Config.ChestItems)
				{
					Helper.AddItemToInventory(e, item.ItemPrefabGUID, item.Quantity, out Entity itemEntity);
				}
				e.Add<DestroyWhenInventoryIsEmpty>();
			});
		}

		[Command("spawnmapicon", description: "Used for debugging", adminOnly: true)]
		public void SpawnMapIconCommand (Player sender, PrefabGUID prefabGUID)
		{
			Helper.CreateMapIcon(prefabGUID, sender.User.Read<EntityInput>().AimPosition, (e) => { });
			sender.ReceiveMessage("Spawned map icon".White());
		}

		[Command("checktime", description: "Used for debugging", adminOnly: true)]
		public void CheckTime (Player sender)
		{
			sender.ReceiveMessage("Seconds Elapsed " + Helper.GetServerTime().ToString());
		}

		[Command("destroyallmapicons", description: "Used for debugging", adminOnly: true)]
		public void DestroyMapIconsCommand (Player sender, PrefabGUID prefabGUID)
		{
			var entities = Helper.GetEntitiesByComponentTypes<MapIconData>();
			foreach (var entity in entities)
			{
				if (entity.GetPrefabGUID() == prefabGUID)
				{
					Helper.DestroyEntity(entity);
				}
			}
		}

		[Command("spawn-boss", description: "Spawns a boss at aim position. Use ? to list bosses.", usage: ".spawn-boss <name | ?> [level=100] [hp=-1]", aliases: ["spawnboss", "sb"], adminOnly: true)]
		public static void SpawnBossCommand(Player sender, string bossName, int level = 100, int hp = -1)
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
                sender.ReceiveMessage($"Boss '{bossName}' not found. Use .spawn-boss ? to list bosses.".Error());
                return;
            }
			var spawnPosition = Helper.GetSnappedHoverPosition(sender, Helper.SnapMode.Center);
			BossManager.SpawnBoss(spawnPosition, prefabData.PrefabGUID, level, 5, hp, false);
			sender.ReceiveMessage($"Spawned {prefabData.OverrideName}!".Success());
		}

		[Command("spawn-resource-node", description: "Spawns a resource node at your location", aliases: new string[] { "spawnresourcenode" }, adminOnly: true)]
		public static void SpawnResourceNodeCommand (Player sender)
		{
            InfiniteResourceNodeManager.SpawnResourceNode(ResourceNodeConfig.Config.ResourceNodePrefab, sender.User.Read<EntityInput>().AimPosition, ResourceNodeConfig.Config.InfiniteResourceNodeDelay);

			sender.ReceiveMessage($"Spawned resource node!".Success());
		}

        [Command("end-resource-node", description: "Ends resource node", aliases: new string[] { "endresourcenode" }, adminOnly: true)]
        public static void EndResourceNodeCommand(Player sender)
        {
            InfiniteResourceNodeManager.Dispose(true);
            sender.ReceiveMessage($"Ended the resource node event".Success());
        }

        [Command("setdurability", adminOnly: true)]
		public static void EnableDurabilityCommand(Player sender, float durability)
		{
			EventHelper.SetDeathDurabilityLoss(durability);
			sender.ReceiveMessage($"Durability is now set to {durability.ToString().Emphasize()}".Success());
		}

		[Command("time", adminOnly: true)]
		public static void GetTime (Player sender)
		{
			sender.ReceiveMessage($"Time: {DateTime.Now}".White());
		}

        [Command("boss-mode", usage: ".bossmode <name | ?>", description: "Become an admin boss. Use ? to list available modes.", aliases: new string[] { "bossmode" }, adminOnly: true)]
        public static void BossModeCommand(Player sender, string bossName)
        {
            if (bossName == "?")
            {
                sender.ReceiveMessage("Available boss modes:".Colorify(ExtendedColor.LightServerColor));
                foreach (var key in AdminBossConfig.Config.AdminBosses.Keys)
                    sender.ReceiveMessage($"  {key}".White());
                return;
            }
			if (AdminBossManager.EnterAdminBossMode(sender, bossName))
			{
				sender.ReceiveMessage($"You are now {bossName.Emphasize()}. Type .startadminboss to begin the event.".White());
			}
			else
			{
                sender.ReceiveMessage($"Boss mode '{bossName}' not found. Use .boss-mode ? to see options.".Error());
            }
        }

        [Command("exit-boss", description: "Exits boss mode, removing visual and stats.", aliases: new string[] { "exitboss", "eb" }, adminOnly: true)]
        public static void ExitBossModeCommand(Player sender)
        {
            if (AdminBossManager.ExitAdminBossMode(sender))
                sender.ReceiveMessage("Exited boss mode.".Success());
            else
                sender.ReceiveMessage("You are not in boss mode.".Error());
        }

        [Command("startadminboss", usage: ".startadminboss true", description: "Starts the admin boss event", aliases: new string[] { "start-admin-boss" }, adminOnly: true)]
        public static void StartAdminBossCommand(Player sender, bool hardMode)
        {
            if (!AdminBossManager.StartEvent(sender, hardMode))
            {
				sender.ReceiveMessage("Cannot start an admin boss event while one is already active!".Error());
            }
        }

        [Command("endadminboss", usage: ".endadminboss", description: "Forcibly ends the admin boss event", aliases: new string[] { "end-admin-boss" }, adminOnly: true)]
        public static void EndAdminBossCommand(Player sender)
        {
			AdminBossManager.Dispose(true);
			sender.ReceiveMessage("Event ended");
        }

        [Command("open", description: "Opens hunger games doors", adminOnly: true)]
        public static void OpenDoorsCommand(Player sender, int floor)
        {
            HungerGamesManager.OpenDoors(floor);
            sender.ReceiveMessage("Doors opened");
        }

        [Command("close", description: "Closes hunger games doors", adminOnly: true)]
        public static void CloseDoorsCommand(Player sender, int floor)
        {
            HungerGamesManager.CloseDoors(floor);
            sender.ReceiveMessage("Doors closed");
        }
    }
}
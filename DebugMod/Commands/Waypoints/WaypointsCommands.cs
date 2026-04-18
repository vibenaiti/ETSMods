using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModCore.Models;
using ModCore;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using Unity.Entities;
using ProjectM;
using ProjectM.Network;

namespace DebugMod.Commands.Waypoints
{
    public class WaypointsCommands
    {
        [Command("tp", description: "Teleports you to a saved waypoint. Use .tp-list to see waypoints.", usage: ".tp <id | name>", aliases: new string[] { "teleport" }, adminOnly: true, category: "Teleport")]
        public void TeleportCommand(Player sender, string nameOrId)
        {
            if (!WaypointManager.TryFindWaypoint(nameOrId, out var waypoint))
            {
                sender.ReceiveMessage($"Waypoint '{nameOrId}' not found. Use .tp-list to see available waypoints.".Error());
                return;
            }

            try
            {
                var target = waypoint.Position.ToFloat3();
                var eventEntity = Helper.CreateEntityWithComponents<TeleportDebugEvent, FromCharacter>();
                eventEntity.Write(sender.ToFromCharacter());
                eventEntity.Write(new TeleportDebugEvent
                {
                    Location = TeleportDebugEvent.TeleportLocation.WorldPosition,
                    MousePosition = sender.Position,
                    Target = TeleportDebugEvent.TeleportTarget.ClosestUnitToCursor,
                    LocationPosition = target
                });
                sender.ReceiveMessage(("Teleported to waypoint: " + waypoint.Name.Emphasize()).White());
            }
            catch (Exception e)
            {
                sender.ReceiveMessage($"Teleport failed: {e.Message}".Error());
            }
        }

        [Command("tp-create", aliases: new string[] { "tp-add", "tp create", "tp add" }, adminOnly: true)]
        public void CreateWaypointCommand(Player sender, string name, bool adminOnly = true, string id = "")
        {
            Waypoint waypoint = new Waypoint(id, name, sender.Position, adminOnly);

            if (WaypointManager.AddWaypoint(waypoint))
                sender.ReceiveMessage(("Waypoint added! " + waypoint.ToString().Emphasize()).Success());
            else
                sender.ReceiveMessage(("Waypoint ID already exists! " + waypoint.ToString().Emphasize()).Error());
        }

        [Command("tp-swap", adminOnly: true)]
        public void SwapWaypointCommand(Player sender, string id1, string id2)
        {
            if (!WaypointManager.TryFindWaypoint(id1, out var waypoint1))
                sender.ReceiveMessage("Wrong waypoint1 id / name!".Error());
            if (!WaypointManager.TryFindWaypoint(id2, out var waypoint2))
                sender.ReceiveMessage("Wrong waypoint2 id / name!".Error());

            if (waypoint1 == null || waypoint2 == null)
                return;

            WaypointManager.SwapWaypoints(waypoint1, waypoint2);
            sender.ReceiveMessage(("Waypoints swapped : " + id1.Emphasize() + " <--> " + id2.Emphasize()).Success());
        }

        [Command("tp-rename", adminOnly: true)]
        public void RenameWaypointCommand(Player sender, string idOrName, string newName)
        {
            if (!WaypointManager.TryFindWaypoint(idOrName, out var waypoint1))
            {
                sender.ReceiveMessage("Wrong waypoint id / name!".Error());
                return;
            }

            if (newName == "")
            {
                sender.ReceiveMessage("New name is empty!".Error());
                return;
            }

            WaypointManager.RenameWaypoint(waypoint1, newName);
            sender.ReceiveMessage(("Waypoint " + idOrName.Emphasize() + " renamed : " + newName.Emphasize()).Success());
        }

        [Command("tp-remove", adminOnly: true)]
        public void RemoveWaypointCommand(Player sender, string nameOrId)
        {
            if (WaypointManager.TryFindWaypoint(nameOrId, out var waypoint))
            {
                WaypointManager.RemoveWaypoint(waypoint.ID);
                sender.ReceiveMessage("Waypoint removed!".Success());
            }
            else
            {
                sender.ReceiveMessage("Waypoint doesn't exist!".Error());
            }
        }

        [Command("tp-list", description: "Displays the waypoints list", adminOnly: false, aliases: new string[] { "tp list", "tplist" }, category: "Teleport")]
        public void ListWaypointsCommand(Player sender)
        {
            WaypointManager.LoadWaypoints();
            if (WaypointManager.Waypoints.Count > 0)
            {
                sender.ReceiveMessage("Waypoint List:".Colorify(ExtendedColor.LightServerColor));
                var sortedByValue = WaypointManager.Waypoints.OrderBy(pair => int.Parse(pair.Value.ID)).ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (var waypoint in sortedByValue)
                {
                    if (sender.IsAdmin || !waypoint.Value.IsAdminOnly)
                    {
                        sender.ReceiveMessage(
                            $"{waypoint.Value.ID.Colorify(ExtendedColor.ServerColor)} - {waypoint.Value.Name}{(waypoint.Value.IsAdminOnly ? " (admin-only)".Italic() : "")}".White());
                    }
                }
            }
            else
            {
                sender.ReceiveMessage("No active waypoints!".Error());
            }
        }
    }

}

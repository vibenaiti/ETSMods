using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static ModCore.Models.Request;
using ModCore.Models;
using System.Numerics;
using Unity.Mathematics;
using System.IO;
using static ModCore.Configs.ConfigDtos;

namespace DebugMod.Commands.Waypoints
{
    public class Waypoint
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public CoordinateDto Position { get; set; }
        public bool IsAdminOnly { get; set; }

        public Waypoint() { }

        public Waypoint(string id, string name, float3 position, bool isAdminOnly)
        {
            ID = id;
            Name = name;
            Position = new CoordinateDto
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            };
            IsAdminOnly = isAdminOnly;
        }

        public override string ToString()
        {
            return $"Waypoint: {Name}, ID: {ID}, Position: {Position.ToFloat3()}";
        }
    }

    public static class WaypointManager
    {
        private const string SavePath = "BepInEx/config/ETS/Config/DebugMod/waypoints.json";
        public static Dictionary<string, Waypoint> Waypoints { get; private set; } = new Dictionary<string, Waypoint>(StringComparer.OrdinalIgnoreCase);

        // Load waypoints from file
        public static void LoadWaypoints()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                string directoryPath = Path.GetDirectoryName(SavePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                Waypoints = JsonSerializer.Deserialize<Dictionary<string, Waypoint>>(json, options);
            }
        }

        // Save waypoints to file
        public static void SaveWaypoints()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            string json = JsonSerializer.Serialize(Waypoints, options);
            string directoryPath = Path.GetDirectoryName(SavePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllText(SavePath, json);
        }

        // Add a new waypoint
        public static bool AddWaypoint(Waypoint waypoint)
        {
            if (waypoint.ID == "")
                waypoint.ID = GetNextAvailableID();

            if (waypoint != null && !Waypoints.ContainsKey(waypoint.ID))
            {
                Waypoints[waypoint.ID] = waypoint;
                SaveWaypoints();
                return true;
            }

            return false;
        }

        public static void SwapWaypoints(Waypoint waypoint1, Waypoint waypoint2)
        {
            string name1 = waypoint1.Name;
            CoordinateDto pos1 = waypoint1.Position;

            string name2 = waypoint2.Name;
            CoordinateDto pos2 = waypoint2.Position;

            waypoint1.Name = name2;
            waypoint1.Position = pos2;

            waypoint2.Name = name1;
            waypoint2.Position = pos1;

            Waypoints[waypoint1.ID] = waypoint1;
            Waypoints[waypoint2.ID] = waypoint2;

            SaveWaypoints();
        }

        public static void RenameWaypoint(Waypoint waypoint1, string newName)
        {
            waypoint1.Name = newName;
            Waypoints[waypoint1.ID] = waypoint1;
            SaveWaypoints();
        }

        // Remove a waypoint by ID
        public static void RemoveWaypoint(string id)
        {
            Waypoints.Remove(id);
            SaveWaypoints();
        }

        // Get a waypoint by ID
        public static Waypoint GetWaypointByID(string id)
        {
            Waypoints.TryGetValue(id, out Waypoint waypoint);
            return waypoint;
        }

        public static bool HasID(string id)
        {
            return (Waypoints.ContainsKey(id));
        }

        public static string GetNextAvailableID()
        {
            int id = 1;  // Start at 1

            while (Waypoints.ContainsKey(id.ToString()))
            {
                id++;
            }

            return id.ToString();
        }

        public static bool TryFindWaypoint(string query, out Waypoint result)
        {
            // First, try to find by ID
            if (Waypoints.TryGetValue(query, out Waypoint waypointByID))
            {
                result = waypointByID;
                return true;
            }

            // If not found by ID, try to find by name
            foreach (var waypoint in Waypoints.Values)
            {
                if (waypoint.Name.Equals(query, StringComparison.OrdinalIgnoreCase))
                {
                    result = waypoint;
                    return true;
                }
            }

            // If not found by either ID or name, return null
            result = default;
            return false;
        }
    }

    public class TeleportRequest : Request
    {
        public new RequestType Type = RequestType.TeleportRequest;
    }

    public static class TeleportRequestManager
    {
        private static RequestManager requestManager = new RequestManager();

        public static bool AddRequest(TeleportRequest request)
        {
            return requestManager.AddRequest(request);
        }

        public static TeleportRequest GetRequest(Player recipient, Player requester)
        {
            return (TeleportRequest)requestManager.GetRequest(recipient, requester);
        }
        public static TeleportRequest GetRequest(Player recipient)
        {
            return (TeleportRequest)requestManager.GetRequest(recipient);
        }

        public static void RemoveRequest(TeleportRequest request)
        {
            requestManager.RemoveRequest(request);
        }
    }
}

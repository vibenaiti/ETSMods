using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModCore.Models;
using ModCore.Services;
using PointsMod;
using ProjectM;

public static class PointsManager
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Data/PointsMod";
	private const string ConfigFileName = "points.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static PlayerStatsRecord Data { get; private set; } = new PlayerStatsRecord();
    private static JsonSerializerOptions SerializationOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
    };
    
	static PointsManager()
	{
        Load();
	}

    public static async void Load()
    {
        try
        {
            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(FullPath));

            if (!File.Exists(FullPath))
            {
                Save(); // Create file with default values
                return;
            }

            var jsonData = await File.ReadAllTextAsync(FullPath);
            Data = JsonSerializer.Deserialize<PlayerStatsRecord>(jsonData, SerializationOptions) ?? new PlayerStatsRecord();
        }
        catch (Exception ex)
        {
            Data = new PlayerStatsRecord();
            Plugin.PluginLog.LogInfo(ex.ToString());
        }
    }

    public static async void Save()
    {
        try
        {
            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(FullPath));

            var jsonData = JsonSerializer.Serialize(Data, SerializationOptions);
            await File.WriteAllTextAsync(FullPath, jsonData);
        }
        catch (Exception ex)
        {
            Plugin.PluginLog.LogInfo(ex.ToString());
        }
    }

    public static void TryAwardDailyLoginPoints(Player player, int pointsToAward)
    {
        DateTime currentTime = DateTime.UtcNow; // Current timestamp
        DateTime currentDate = currentTime.Date; // Current date with time part set to 00:00:00
        if (Data.PlayerLastLogin.TryGetValue(player.SteamID, out var lastLoginDate))
        {
            // Check if LastLoginTimestamp is null or if the current date is after the date part of the last login timestamp
            if (currentDate > lastLoginDate)
            {
                // Logic for granting daily login points
                AddPointsToPlayer(player, PointsType.Cosmetic, pointsToAward);
                var action = () =>
                {
                    player.ReceiveMessage($"You were awarded {pointsToAward.ToString().Emphasize()} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Warning()} for your daily login.".White());
                    player.ReceiveMessage($"New total: {GetPlayerPoints(player, PointsType.Cosmetic).ToString().Warning()}".White());
                };
                ActionScheduler.RunActionOnceAfterDelay(action, 0.25f);
            }

            // Always update the LastLoginTimestamp, whether or not points are awarded
            Data.PlayerLastLogin[player.SteamID] = currentTime;
        }
        else
        {
            // Logic for granting daily login points
            AddPointsToPlayer(player, PointsType.Cosmetic, pointsToAward);
            var action = () =>
            {
                player.ReceiveMessage($"You were awarded {pointsToAward.ToString().Emphasize()} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Warning()} for your daily login.".White());
                player.ReceiveMessage($"New total: {GetPlayerPoints(player, PointsType.Cosmetic).ToString().Warning()}".White());
            };
            ActionScheduler.RunActionOnceAfterDelay(action, 0.25f);

            // Always update the LastLoginTimestamp, whether or not points are awarded
            Data.PlayerLastLogin[player.SteamID] = currentTime;
        }
    }

    public static bool TryPurchase(Player player, PointsType pointsType, int cost)
    {
        if (GetPlayerPoints(player, pointsType) >= cost)
        {
            RemovePointsFromPlayer(player, pointsType, cost);
            return true;
        }
        return false;
    }

    public static bool HasEnoughPoints(Player player, PointsType pointsType, int cost)
    {
        cost = Math.Abs(cost);
        if (GetPlayerPoints(player, pointsType) >= cost)
        {
            return true;
        }
        return false;
    }

    public static void AddPointsToPlayer(Player player, PointsType pointsType, int pointsToAdd)
    {
        if (!Data.PlayerPoints.TryGetValue(player.SteamID, out var playerPointTypes))
        {
            playerPointTypes = new Dictionary<PointsType, int>();
            Data.PlayerPoints[player.SteamID] = playerPointTypes;
        }

        if (playerPointTypes.TryGetValue(pointsType, out var currentPoints))
        {
            playerPointTypes[pointsType] = currentPoints + pointsToAdd;
        }
        else
        {
            playerPointTypes[pointsType] = pointsToAdd;
        }
    }

    public static void RemovePointsFromPlayer(Player player, PointsType pointsType, int pointsToRemove)
    {
        if (Data.PlayerPoints.TryGetValue(player.SteamID, out var playerPointTypes))
        {
            if (playerPointTypes.TryGetValue(pointsType, out var currentPoints))
            {
                int newPoints = Math.Max(currentPoints - pointsToRemove, 0); // Ensure points don't go negative
                playerPointTypes[pointsType] = newPoints;
            }
        }
    }

    public static int GetPlayerPoints(Player player, PointsType pointsType)
    {
        if (Data.PlayerPoints.TryGetValue(player.SteamID, out var playerPointTypes))
        {
            if (playerPointTypes.TryGetValue(pointsType, out var points))
            {
                return points;
            }
        }

        return 0; // Return 0 if the player or the point type doesn't exist
    }


    public static void SetPlayerPoints(Player player, PointsType pointsType, int points)
    {
        if (Data.PlayerPoints.TryGetValue(player.SteamID, out var playerPointTypes))
        {
            playerPointTypes[pointsType] = points;
        }
    }
}

public class PlayerStatsRecord
{
    public Dictionary<ulong, DateTime> PlayerLastLogin { get; set; } = new();
    public Dictionary<ulong, Dictionary<PointsType, int>> PlayerPoints { get; set; } = new();
}

public enum PointsType
{
    Main,
    Cosmetic
}

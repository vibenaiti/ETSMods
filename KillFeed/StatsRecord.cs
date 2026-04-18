using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModCore.Models;
using ModCore.Services;
using KillFeed;

public static class StatsRecord
{
	private const string ConfigDirectoryName = "Bepinex/config/ETS/Data/KillFeed";
	private const string ConfigFileName = "killfeed_stats.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static StatsRecordData Data { get; private set; } = new StatsRecordData();
    private static JsonSerializerOptions SerializationOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
    };
    
	static StatsRecord()
	{
        SerializationOptions.Converters.Add(new PlayerStatsRecordConverter());
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
            Data = JsonSerializer.Deserialize<StatsRecordData>(jsonData, SerializationOptions) ?? new StatsRecordData();
        }
        catch (Exception ex)
        {
            Data = new StatsRecordData();
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
}

public class StatsRecordData
{
    [JsonIgnore] // Ignore this property during serialization as it will be handled manually
    public Dictionary<Player, PlayerStatsRecord> PlayerStats { get; private set; } = new();

    // Method to help with serialization
    public List<PlayerStatsRecord> GetPlayerStatsForSerialization() => PlayerStats.Values.ToList();

    public void SetPlayerStatsFromSerialization(List<PlayerStatsRecord> list, IEnumerable<Player> allPlayers)
    {
        PlayerStats = list
            .Select(statRecord => new
            {
                Player = allPlayers.FirstOrDefault(player => player.SteamID == statRecord.SteamID),
                StatsRecord = statRecord
            })
            .Where(x => x.Player != null) // Ensure that we only include entries where a player was found
            .ToDictionary(
                x => x.Player,
                x => x.StatsRecord);
    }

    public void RecordKill(Player victim, Player killer)
    {
        if (PlayerStats.TryGetValue(victim, out var victimRecord))
        {
            victimRecord.Deaths++;
            victimRecord.Streak = 0;
        }
        else
        {
            PlayerStats[victim] = new PlayerStatsRecord
            {
                SteamID = victim.SteamID,
                Deaths = 1,
                Streak = 0
            };
        }

        if (PlayerStats.TryGetValue(killer, out var killerRecord))
        {
            killerRecord.Kills++;
            killerRecord.Streak++;
        }
        else
        {
            PlayerStats[killer] = new PlayerStatsRecord
            {
                SteamID = killer.SteamID,
                Kills = 1,
                Streak = 1,
                Deaths = 0
            };
        }
    }

}

public class PlayerStatsRecordConverter : JsonConverter<StatsRecordData>
{
    public override StatsRecordData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Check if the JSON token is the start of an object and if it's empty
        if (reader.TokenType == JsonTokenType.StartObject && reader.Read() && reader.TokenType == JsonTokenType.EndObject)
        {
            return new StatsRecordData(); // Return an empty StatsRecordData instance
        }

        // If the JSON starts with a '[' (expecting a list), proceed as before
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = JsonSerializer.Deserialize<List<PlayerStatsRecord>>(ref reader, options);
            var data = new StatsRecordData();
            if (list != null)
            {
                data.SetPlayerStatsFromSerialization(list, PlayerService.CharacterCache.Values);
            }
            return data;
        }

        // If the JSON structure is unexpected, throw an exception or handle accordingly
        throw new JsonException("Unexpected JSON format for StatsRecordData.");
    }

    public override void Write(Utf8JsonWriter writer, StatsRecordData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.GetPlayerStatsForSerialization(), options);
    }
}

public class PlayerStatsRecord
{
    public ulong SteamID { get; set; } = 0;
    public int Kills { get; set; } = 0;
    public int Deaths { get; set; } = 0;
    public int Streak { get; set; } = 0;
}
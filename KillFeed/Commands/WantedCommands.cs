using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using ModCore;
using ProjectM;
using Unity.Entities;
using System.Linq;
using System;
using ModCore.Services;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KillFeed.Commands
{
    public class WantedCommands
    {
        [Command("eventactive", description: "", adminOnly: true)]
        public static void EventActiveCommand(Player sender)
        {
            sender.ReceiveMessage($"{PointsMod.Globals.EventActive} {PointsMod.Globals.ActiveEvents.Count}");
        }

        [Command("testcolor", description: "", adminOnly: true)]
        public static void TestColorCommand(Player sender, string colorName)
        {
            var colorDictionary = new Dictionary<string, Color>();

            // Use reflection to populate the dictionary with fields
            foreach (FieldInfo field in typeof(ExtendedColor).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (field.FieldType == typeof(Color))
                {
                    Color colorValue = (Color)field.GetValue(null);
                    Plugin.PluginLog.LogInfo(field.Name);
                    colorDictionary.Add(field.Name, colorValue);
                }
            }

            if (colorDictionary.TryGetValue(colorName, out Color color))
            {
                Helper.SendSystemMessageToAllClients($"This is a test message {colorName}".Colorify(color));
            }
            else
            {
                sender.ReceiveMessage($"Invalid color name");
            }
        }


        [Command("simulatekill", description: "", adminOnly: true)]
        public static void SimulateKillCommand(Player sender, Player target, Player target2 = null)
        {
            if (target2 == null)
            {
                target2 = target;
                target = sender;

            }
            DeathHandler.VictimToKiller[target2] = target;
            DeathHandler.HandleOnPlayerDeath(target2, new DeathEvent
            {
                Died = target2.Character,
                Killer = target.Character
            });
        }

        [Command("simulatekills", description: "", adminOnly: true)]
        public static void SimulateKillsCommand(Player sender)
        {
            var players = new List<Player>();
            foreach (var player in PlayerService.CharacterCache.Values)
            {
                if (player.Character.Read<Equipment>().GetFullLevel() >= 83)
                {
                    players.Add(player);
                }
                if (players.Count > 25)
                {
                    break;
                }
            }

            sender.ReceiveMessage(players.Count.ToString());

            foreach (var player in players)
            {
                foreach (var player2 in players)
                {
                    if (player != player2)
                    {
                        SimulateKillCommand(player, player2);
                    }
                }
            }
        }

        [Command("lb", description: "Displays the leaderboard sorted by kills. You can specify a page number or a player name.", aliases: new string[] { "kf" }, adminOnly: false, includeInHelp: true, category: "KillFeed")]
        public static void ShowKillsLeaderboardCommand(Player sender, string pageOrPlayerName = "")
        {
            if (pageOrPlayerName == "")
            {
                pageOrPlayerName = "1";
            }
            const int playersPerPage = 10; // Number of players to display per page
            int pageNumber = 1; // Default page number

            var orderedPlayers = StatsRecord.Data.PlayerStats
                                    .Select(kvp => new {
                                        SteamID = kvp.Key.SteamID,
                                        Name = kvp.Key.Name,
                                        Kills = kvp.Value.Kills,
                                        Deaths = kvp.Value.Deaths,
                                        Streak = kvp.Value.Streak
                                    })
                                    .OrderByDescending(player => player.Kills)
                                    .ToList();

            // Attempt to parse the page number or treat as player name
            if (!int.TryParse(pageOrPlayerName, out pageNumber))
            {
                // If not a page number, search for the player name in the sorted list
                var playerIndex = orderedPlayers
                                    .Select((value, index) => new { value.Name, Index = index })
                                    .FirstOrDefault(p => p.Name.Equals(pageOrPlayerName, StringComparison.OrdinalIgnoreCase))?.Index;

                if (playerIndex == null)
                {
                    sender.ReceiveMessage($"Player '{pageOrPlayerName}' not found.".Error());
                    return;
                }

                // Calculate the page number that contains the player
                pageNumber = (playerIndex.Value / playersPerPage) + 1;
            }

            // Ensure the page number is positive
            if (pageNumber < 1)
            {
                sender.ReceiveMessage("Page number must be positive.".Error());
                return;
            }

            // Calculate start and end indices for the current page
            int startIndex = (pageNumber - 1) * playersPerPage;
            int endIndex = Math.Min(startIndex + playersPerPage, orderedPlayers.Count);

            if (orderedPlayers.Count == 0)
            {
                sender.ReceiveMessage("No players available on the leaderboard".Error());
                return;
            }

            if (startIndex >= orderedPlayers.Count)
            {
                sender.ReceiveMessage("The page number is too high".Error());
                return;
            }

            sender.ReceiveMessage($"Leaderboard:".Colorify(ExtendedColor.LightServerColor) + $" Page {pageNumber}".White());

            ulong currentPlayerSteam = sender.SteamID; // Assuming sender has a SteamID property

            for (int i = startIndex; i < endIndex; i++)
            {
                var player = orderedPlayers[i];
                string playerInfo = FormatPlayerRankInfo(player, i + 1, currentPlayerSteam);
                sender.ReceiveMessage(playerInfo);
            }

            // If current player was not shown in the displayed page, show them at the end
            var currentPlayerIndex = orderedPlayers.FindIndex(p => p.SteamID == currentPlayerSteam);
            if (currentPlayerIndex < startIndex || currentPlayerIndex >= endIndex)
            {
                if (currentPlayerIndex != -1) // If the player is in the list
                {
                    sender.ReceiveMessage($"................................................................".White());
                    var currentPlayer = orderedPlayers[currentPlayerIndex];
                    string playerRankInfo = FormatPlayerRankInfo(currentPlayer, currentPlayerIndex + 1, currentPlayerSteam, true); // Highlight the current player
                    sender.ReceiveMessage(playerRankInfo);
                }
            }
        }

        private static string FormatPlayerRankInfo(dynamic player, int rank, ulong currentPlayerSteam, bool bold = false)
        {
            string rankColor = "#FFFFFF"; // Default rank color
            string boldStart = bold ? "<b>" : "";
            string boldEnd = bold ? "</b>" : "";
            string highlightStart = player.SteamID == currentPlayerSteam ? "<color=#FFD700>" : ""; // Highlight color if it's the current player
            string highlightEnd = player.SteamID == currentPlayerSteam ? "</color>" : "";
            return $"{boldStart}" + $"{rank}" + " - ".White() + $"{highlightStart}{player.Name}{highlightEnd}{boldEnd}".Colorify(ExtendedColor.ClanNameColor) + $": {StringExtensions.Success(player.Kills.ToString())} / {StringExtensions.Error(player.Deaths.ToString())} - K/S: " + $"{StringExtensions.Colorify(player.Streak.ToString(), ExtendedColor.DodgerBlue)}".White();
        }
    }
}

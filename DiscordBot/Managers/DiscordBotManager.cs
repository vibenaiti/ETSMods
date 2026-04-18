using ModCore.Frameworks.CommandFramework;
using ModCore.Models;
using ModCore.Services;
using ModCore;
using ProjectM.Scripting;
using System.Threading.Tasks;
using Discord;
using System;
using Discord.WebSocket;

namespace DiscordBot
{
    public static partial class DiscordBotManager
    {
        public static DiscordSocketClient _client;
        public static SocketGuild _socketGuild;
        private static DateTime _lastUpdate = DateTime.MinValue;
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

        public static async Task InitializeAsync()
        {
            try
            {
                _client = new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
                });
                await _client.LoginAsync(TokenType.Bot, DiscordBotConfig.Config.Token);
                await _client.StartAsync();
                _client.Ready += OnReadyAsync;

                PlayerService.OnOnlinePlayerAmountChanged += HandlePlayerAmountChanged;
                await UpdatePlayerCountStatusAsync();
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.LogInfo(ex.ToString());
            }
        }

        private static Task OnReadyAsync()
        {
            _socketGuild = _client.GetGuild(DiscordBotConfig.Config.GuildID);
            return Task.CompletedTask;
        }

        public static void Dispose()
        {
            if (_client != null)
            {
                PlayerService.OnOnlinePlayerAmountChanged -= HandlePlayerAmountChanged;
                _client.LogoutAsync().GetAwaiter().GetResult();
                _client.StopAsync().GetAwaiter().GetResult();
                _client.Dispose();
                _client = null;
            }
        }

        private static void HandlePlayerAmountChanged()
        {
            if (DateTime.UtcNow - _lastUpdate >= UpdateInterval)
            {
                _lastUpdate = DateTime.UtcNow;
                Task.Run(UpdatePlayerCountStatusAsync);
            }
        }

        public static async Task UpdatePlayerCountStatusAsync()
        {
            if (_client == null) return;

            try
            {
                var playerCount = PlayerService.OnlinePlayersWithUsers.Count;
                await _client.SetActivityAsync(new Discord.Game($"Online: {playerCount}", ActivityType.Watching));
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.LogInfo(ex.ToString());
            }
        }

        public static async Task SendEmbedAsync(ulong discordChannel, Embed _embed)
        {
            var channel = _client.GetChannel(discordChannel) as IMessageChannel;
            if (channel != null)
            {
                await channel.SendMessageAsync(embed: _embed);
            }
        }

        public static async Task SendMessageAsync(ulong channelId, string message)
        {
            var channel = _client.GetChannel(channelId) as IMessageChannel;
            if (channel != null)
            {
                await channel.SendMessageAsync(message);
            }
        }
    }
}

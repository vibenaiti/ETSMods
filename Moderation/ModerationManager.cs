using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModCore.Events;
using ModCore.Models;
using Unity.Entities;
using ProjectM.Network;
using ModCore;
using ModCore.Helpers;

namespace Moderation
{
    public static class ModerationManager
    {
        public static void Initialize()
        {
            GameEvents.OnPlayerChatMessage += HandleOnPlayerChatMessage;
        }

        public static void Dispose()
        {
            GameEvents.OnPlayerChatMessage -= HandleOnPlayerChatMessage;
        }

        public static void HandleOnPlayerChatMessage(Player player, Entity eventEntity, ChatMessageEvent chatMessageEvent)
        {
            if (eventEntity.Exists() && (chatMessageEvent.MessageType == ChatMessageType.Global || chatMessageEvent.MessageType == ChatMessageType.Local) && ModerationModDataStorage.Data.MutedPlayers.Contains(player.SteamID)) 
            {
                eventEntity.Destroy();
                player.ReceiveMessage("You are muted from global chat.".Error());
            }
        }

        public static bool MutePlayer(Player player)
        {
            if (!ModerationModDataStorage.Data.MutedPlayers.Contains(player.SteamID))
            {
                ModerationModDataStorage.Data.MutedPlayers.Add(player.SteamID);
                ModerationModDataStorage.Save();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool UnmutePlayer(Player player)
        {
            if (ModerationModDataStorage.Data.MutedPlayers.Contains(player.SteamID))
            {
                ModerationModDataStorage.Data.MutedPlayers.Remove(player.SteamID);
                ModerationModDataStorage.Save();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

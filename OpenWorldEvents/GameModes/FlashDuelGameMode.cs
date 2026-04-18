using ModCore;
using ModCore.Data;
using ModCore.Events;
using ModCore.Helpers;
using ModCore.Models;
using ModCore.Services;
using OpenWorldEvents.Managers;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace OpenWorldEvents.GameModes
{
    public class FlashDuelGameMode
    {
        public string UnitGameModeType => $"Ranked{ArenaNumber}";
        public int ArenaNumber = 0;
        public bool MatchActive = false;
        public Player Player1 = null;
        public Player Player2 = null;
        public float3 Location1 = new();
        public float3 Location2 = new();
        public List<Timer> Timers = new List<Timer>();
        public bool IsOccupied { get; set; }
        public Stopwatch Stopwatch { get; set; } = new Stopwatch();
        public bool IsFullLoot { get; set; } = false;

        public static Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
        {
            ResetCooldowns = true,
            RemoveConsumables = false,
            RemoveShapeshifts = true
        };

        private static HashSet<string> AllowedCommands = new HashSet<string>
        {
            "ping",
            "help",
            "legendary",
            "jewel",
            "forfeit",
            "points",
            "lb ranked",
        };

        public FlashDuelGameMode(bool isFullLoot)
        {
            IsFullLoot = isFullLoot;
            ResetOptions = new Helper.ResetOptions();
        }

        private Dictionary<PrefabGUID, Action<Player, Entity>> _buffHandlers;

        public void Initialize(Player player1, Player player2)
        {
            IsOccupied = true;

            Player1 = player1;
            Player2 = player2;

            Stopwatch.Reset();
            Stopwatch.Start();

            Initialize();
        }

        public void Initialize()
        {
            MatchActive = true;
            GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
            GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
            GameEvents.OnPlayerRespondedToClanInvite += HandleOnPlayerAcceptedClanInvite;
            GameEvents.OnPlayerInvitedToClan += HandleOnPlayerInvitedToClan;
            GameEvents.OnPlayerKickedFromClan += HandleOnPlayerKickedFromClan;
            GameEvents.OnPlayerLeftClan += HandleOnPlayerLeftClan;

            var action = () => 
            {
                FlashDuelsManager.EndMatch(ArenaNumber, Player1, Player2, false);
            };
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, DuelsConfig.Config.MaxDuelTime));
        }

        public void Dispose()
        {
            IsOccupied = false;

            Player1 = null;
            Player2 = null;

            Stopwatch.Reset();

            MatchActive = false;

            GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
            GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
            GameEvents.OnPlayerRespondedToClanInvite -= HandleOnPlayerAcceptedClanInvite;
            GameEvents.OnPlayerInvitedToClan -= HandleOnPlayerInvitedToClan;
            GameEvents.OnPlayerKickedFromClan -= HandleOnPlayerKickedFromClan;
            GameEvents.OnPlayerLeftClan -= HandleOnPlayerLeftClan;

            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();
        }

        public void HandleOnPlayerDowned(Player player, Entity killer)
        {
            if (player != Player1 && player != Player2) return;

            if (player == Player1)
            {
                Player2.Heal();
            }
            else
            {
                Player1.Heal();
            }
        }

        public void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
        {
            if (player != Player1 && player != Player2) return;
            
            if (player == Player1)
            {
                FlashDuelsManager.EndMatch(ArenaNumber, Player2, Player1);
            }
            else
            {
                FlashDuelsManager.EndMatch(ArenaNumber, Player1, Player2);
            }

            if (IsFullLoot)
            {
                Helper.UnequipAllItems(player);
                var action = () =>
                {
                    var eventEntity = Helper.CreateEntityWithComponents<FromCharacter, DropEntireInventoryEvent>();
                    eventEntity.Write(player.ToFromCharacter());
                    eventEntity.Write(new DropEntireInventoryEvent
                    {
                        Setting = DropEntireInventorySetting.All
                    });
                };
                ActionScheduler.RunActionOnceAfterFrames(action, 3);
            }
        }

        public static void TransferAllItemsToTargetInventory(Entity sourceInventory, Entity targetInventory)
        {
            if (sourceInventory.Has<InventoryInstanceElement>())
            {
                sourceInventory = sourceInventory.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
            }
            if (targetInventory.Has<InventoryInstanceElement>())
            {
                targetInventory = targetInventory.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
            }

            var inventoryBufferSource = sourceInventory.ReadBuffer<InventoryBuffer>();
            var inventoryBufferTarget = targetInventory.ReadBuffer<InventoryBuffer>();

            var emptySlots = new List<int>();
            for (int i = 0; i < inventoryBufferTarget.Length; i++)
            {
                if (inventoryBufferTarget[i].ItemType == PrefabGUID.Empty)
                {
                    emptySlots.Add(i);
                }
            }

            foreach (var item in inventoryBufferSource)
            {
                if (item.ItemType != PrefabGUID.Empty)
                {
                    if (emptySlots.Count > 0)
                    {
                        // Fill into the first empty slot
                        inventoryBufferTarget[emptySlots[0]] = item;
                        emptySlots.RemoveAt(0); // Remove the filled slot
                    }
                    else
                    {
                        // No empty slot, add to the end
                        inventoryBufferTarget.Add(item);
                    }
                }
            }

            // Clear the source inventory
            InventoryUtilitiesServer.ClearInventory(VWorld.Server.EntityManager, sourceInventory);
        }

        public void HandleOnPlayerInvitedToClan(Player player, Entity eventEntity)
        {
            if (player != Player1 && player != Player2) return;

            VWorld.Server.EntityManager.DestroyEntity(eventEntity);
            player.ReceiveMessage("You can't invite people in your clan in this game mode.".Error());
        }

        public void HandleOnPlayerKickedFromClan(Player player, Entity eventEntity, ClanEvents_Client.Kick_Request clanKickEvent)
        {
            if (player != Player1 && player != Player2) return;

            VWorld.Server.EntityManager.DestroyEntity(eventEntity);
            player.ReceiveMessage("You can't kick people in your clan in this game mode.".Error());
        }

        public void HandleOnPlayerLeftClan(Player player, Entity eventEntity, ClanEvents_Client.LeaveClan leaveClanEvent)
        {
            if (player != Player1 && player != Player2) return;

            VWorld.Server.EntityManager.DestroyEntity(eventEntity);
            player.ReceiveMessage("You can't leave your clan in this game mode.".Error());
        }

        public void HandleOnPlayerAcceptedClanInvite(Player player, Entity eventEntity, ClanEvents_Client.ClanInviteResponse clanInviteResponse)
        {
            if (player != Player1 && player != Player2) return;

            if (clanInviteResponse.Response == InviteRequestResponse.Accept)
            {
                VWorld.Server.EntityManager.DestroyEntity(eventEntity);
                player.ReceiveMessage("You can't accept clan invites in this game mode.".Error());
            }
        }

        public static new HashSet<string> GetAllowedCommands()
        {
            return AllowedCommands;
        }
    }
}

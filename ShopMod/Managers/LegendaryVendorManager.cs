using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModCore.Data;
using ModCore.Events;
using ModCore.Helpers;
using ModCore.Models;
using ModCore.Services;
using ModCore;
using Unity.Entities;
using ProjectM.Network;
using Unity.Transforms;
using static ProjectM.SpawnBuffsAuthoring.SpawnBuffElement_Editor;
using Stunlock.Core;
using ModCore.Factories;
using ProjectM.Gameplay;
using Unity.Collections;

namespace ShopMod.Managers
{
    public static class LegendaryVendorManager
    {
        private static List<Timer> Timers = new();
        private static Dictionary<PrefabGUID, List<PrefabGUID>> LegendaryWeapons = new()
        {
            { Prefabs.Item_Weapon_Slashers_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_Slashers_Unique_T08_Variation01_Shattered, Prefabs.Item_Weapon_Slashers_Unique_T08_Variation02_Shattered } },
            { Prefabs.Item_Weapon_Spear_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_Spear_Unique_T08_Variation01_Shattered } },
            { Prefabs.Item_Weapon_Axe_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_Axe_Unique_T08_Variation01_Shattered } },
            { Prefabs.Item_Weapon_GreatSword_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_GreatSword_Unique_T08_Variation01_Shattered } },
            { Prefabs.Item_Weapon_Crossbow_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_Crossbow_Unique_T08_Variation01_Shattered } },
            { Prefabs.Item_Weapon_Pistols_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_Pistols_Unique_T08_Variation01_Shattered } },
            { Prefabs.Item_Weapon_Mace_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_Mace_Unique_T08_Variation01_Shattered } },
            { Prefabs.Item_Weapon_Sword_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_Sword_Unique_T08_Variation01_Shattered } },
            { Prefabs.Item_Weapon_Reaper_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_Reaper_Unique_T08_Variation01_Shattered } },
            { Prefabs.Item_Weapon_Whip_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_Whip_Unique_T08_Variation01_Shattered } },
            { Prefabs.Item_Weapon_Longbow_Legendary_T08_Shattered, new() { Prefabs.Item_Weapon_Longbow_Unique_T08_Variation01_Shattered } },
        };

        private static Random Random = new Random();
        public static void Initialize()
        {
            GameEvents.OnPlayerPurchasedItem += HandleOnPlayerPurchasedItem;
        }

        public static void Dispose()
        {
            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();
            GameEvents.OnPlayerPurchasedItem -= HandleOnPlayerPurchasedItem;
        }

        private static void HandleOnPlayerPurchasedItem(Player player, Entity eventEntity, TraderPurchaseEvent traderPurchaseEvent)
        {
            try
            {
                if (eventEntity.Exists())
                {
                    if (Helper.TryGetEntityFromNetworkId(traderPurchaseEvent.Trader, out var trader))
                    {
                        var outputBuffer = trader.ReadBuffer<TradeOutput>();
                        var costBuffer = trader.ReadBuffer<TradeCost>();

                        var purchasedItem = outputBuffer[traderPurchaseEvent.ItemIndex].Item;
                        if (LegendaryWeapons.TryGetValue(purchasedItem, out var possibleItems))
                        {
                            if (Random.NextDouble() <= ShopModConfig.Config.ChanceOfNamedArtifactFromCustomVendor)
                            {
                                var index = Random.Next(possibleItems.Count);
                                var emptyIndices = new HashSet<int>();
                                var inventory = player.Inventory.ReadBuffer<InventoryBuffer>();
                                var i = 0;
                                foreach (var item in inventory)
                                {
                                    if (item.ItemType == PrefabGUID.Empty)
                                    {
                                        emptyIndices.Add(i);
                                    }
                                    i++;
                                }
                                var action = () =>
                                {
                                    var buffer = player.Inventory.ReadBuffer<InventoryBuffer>();
                                    var j = 0;
                                    foreach (var item in buffer)
                                    {
                                        if (item.ItemType == purchasedItem && emptyIndices.Contains(j))
                                        {
                                            Helper.RemoveItemAtSlot(player, purchasedItem, j);
                                            Helper.AddItemToInventory(player, possibleItems[index], 1, out var itemEntity);
                                        }
                                        j++;
                                    }
                                };
                                ActionScheduler.RunActionOnceAfterFrames(action, 2);
                            }
                        }
                        var outputAmount = outputBuffer[traderPurchaseEvent.ItemIndex].Amount;
                        var costAmount = costBuffer[traderPurchaseEvent.ItemIndex].Amount;
                        var costItem = costBuffer[traderPurchaseEvent.ItemIndex].Item;

                        
                        RefillStock(traderPurchaseEvent, trader);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private static void RefillStock(TraderPurchaseEvent purchaseEvent, Entity trader)
        {
            var _entryBuffer = trader.ReadBuffer<TraderEntry>();

            for (int i = 0; i < _entryBuffer.Length; i++)
            {
                TraderEntry _newEntry = _entryBuffer[i];
                if (purchaseEvent.ItemIndex == _newEntry.OutputStartIndex)
                {
                    _newEntry.StockAmount = (ushort)(_newEntry.StockAmount + 1);
                    _entryBuffer[i] = _newEntry;
                }
            }
        }
    }
}

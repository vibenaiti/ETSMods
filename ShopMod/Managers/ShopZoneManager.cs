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
using ProjectM.Physics;

namespace ShopMod.Managers
{
    public static class ShopZoneManager
    {
        private static List<Timer> Timers = new();
        public static CompositeZone ShopZone;
        public static HashSet<Player> PlayersInShop = new();
        public static HashSet<Entity> PrisonersInShop = new();
        public static void Initialize()
        {
            ShopZone = new CompositeZone();
            foreach (var zone in ShopModConfig.Config.ShopRectangleZones)
            {
                ShopZone.AddZone(zone.ToRectangleZone());
            }

            var action = () => CheckAndHandlePlayerEnteredShopArea();
            var timer = ActionScheduler.RunActionEveryInterval(action, 1f);
            Timers.Add(timer);
            CleanUpProtectionBuffs();
            GameEvents.OnPlayerPurchasedItem += HandleOnPlayerPurchasedItem;
            GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
            GameEvents.OnPlayerBuffRemoved += HandleOnPlayerBuffRemoved;
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
            GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
            GameEvents.OnPlayerBuffRemoved -= HandleOnPlayerBuffRemoved;
        }

        public static void CheckAndHandlePlayerEnteredShopArea()
        {
            var entityToTranslation = Core.adminAuthSystem.GetComponentDataFromEntity<Translation>();
            foreach (var player in PlayerService.CharacterCache.Values)
            {
                if (ShopZone.Contains(entityToTranslation[player.Character].Value))
                {
                    PlayersInShop.Add(player);
                    if (Helper.TryGetBuff(player, Prefabs.AB_Charm_Owner_HasCharmedTarget_Buff, out var buffEntity))
                    {
                        var buffer = player.Character.ReadBuffer<FollowerBuffer>();
                        foreach (var follower in buffer)
                        {
                            var followerEntity = follower.Entity._Entity;
                            if (followerEntity.Exists())
                            {
                                PrisonersInShop.Add(followerEntity);
                            }
                        }
                    }
                    if (player.IsOnline)
                    {
                        if (Helper.BuffPlayer(player, Prefabs.Buff_General_PvPProtected, out var buffEntity2, Helper.NO_DURATION))
                        {
                            Helper.ModifyBuff(buffEntity2, BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.ImmuneToSun | BuffModificationTypes.DisableDynamicCollision);
                        }
                    }
                    else
                    {
                        Helper.RemoveBuff(player, Prefabs.Buff_General_PvPProtected);
                    }
                }
                else
                {
                    if (PlayersInShop.Contains(player))
                    {
                        Helper.RemoveBuff(player, Prefabs.Buff_General_PvPProtected);
                        PlayersInShop.Remove(player);
                    }
                }
                var prisonersToRemove = new List<Entity>();
                foreach (var prisoner in PrisonersInShop)
                {
                    if (prisoner.Exists())
                    {
                        var prisonerInShop = ShopZone.Contains(prisoner);
                        var prisonerHasProtectionBuff = Helper.HasBuff(prisoner, Helper.CustomBuff1);
                        if (!prisonerInShop && prisonerHasProtectionBuff)
                        {
                            prisonersToRemove.Add(prisoner);
                            Helper.RemoveBuff(prisoner, Helper.CustomBuff1);
                            player.ReceiveMessage("Your prisoner is no longer protected!".Warning());
                        }
                    }
                    else
                    {
                        prisonersToRemove.Add(prisoner);
                    }
                }
                foreach (var prisonerToRemove in prisonersToRemove)
                {
                    PrisonersInShop.Remove(prisonerToRemove);
                }
            }
        }

        private static void CleanUpProtectionBuffs()
        {
            foreach (var player in PlayerService.CharacterCache.Values)
            {
                if (Helper.TryGetBuff(player, Prefabs.Buff_General_PvPProtected, out var buffEntity))
                {
                    if (buffEntity.Has<LifeTime>())
                    {
                        var lifeTime = buffEntity.Read<LifeTime>();
                        if (lifeTime.Duration == Helper.NO_DURATION)
                        {
                            Helper.DestroyBuff(buffEntity);
                        }
                    }
                }
            }
        }

        private static void HandleOnPlayerBuffed(Player player, Entity buffEntity, PrefabGUID prefabGuid)
        {
            if (prefabGuid == Prefabs.AB_Interact_Trade)
            {
                var spellTarget = buffEntity.Read<SpellTarget>();
                var trader = spellTarget.Target._Entity;
                if (trader.Exists())
                {
                    var category = UnitFactory.GetCategory(trader);
                    if (ShopModConfig.Config.SpecialTraderNames.Contains(category))
                    {
                        if (InventoryUtilities.GetFreeSlotsCount(VWorld.Server.EntityManager, player.Character) == 0)
                        {
                            Helper.DestroyBuff(buffEntity);
                            player.ReceiveMessage("You must have inventory space to interact with this trader!".Error());
                            return;
                        }
                        var points = PointsManager.GetPlayerPoints(player, PointsType.Cosmetic);
                        Helper.AddItemToInventory(player, ShopModConfig.Config.SpecialPhysicalCurrency, points, out var itemEntity);
                    }
                }
            }
        }

        private static void HandleOnPlayerBuffRemoved(Player player, Entity buffEntity, PrefabGUID prefabGuid)
        {
            if (prefabGuid == Prefabs.AB_Interact_Trade)
            {
                var spellTarget = buffEntity.Read<SpellTarget>();
                var trader = spellTarget.Target._Entity;
                if (trader.Exists())
                {
                    var category = UnitFactory.GetCategory(trader);
                    if (ShopModConfig.Config.SpecialTraderNames.Contains(category))
                    {
                        Helper.CompletelyRemoveItemFromInventory(player, ShopModConfig.Config.SpecialPhysicalCurrency);
                    }
                }
            }
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

                        var outputItem = Helper.GetPrefabEntityByPrefabGUID(outputBuffer[traderPurchaseEvent.ItemIndex].Item);
                        if (outputItem.Has<EquippableData>() && costBuffer[traderPurchaseEvent.ItemIndex].Item != ShopModConfig.Config.SpecialPhysicalCurrency)
                        {
                            var equippableData = outputItem.Read<EquippableData>();
                            if (equippableData.EquipmentType == EquipmentType.Headgear || equippableData.EquipmentType == EquipmentType.Cloak)
                            {
                                eventEntity.Destroy();
                                player.ReceiveMessage("You aren't allowed to purchase cosmetics here".Error());
                                Helper.RemoveBuff(player, Prefabs.AB_Interact_Trade);
                                return;
                            }
                        }
                        var outputAmount = outputBuffer[traderPurchaseEvent.ItemIndex].Amount;
                        var costAmount = costBuffer[traderPurchaseEvent.ItemIndex].Amount;
                        var costItem = costBuffer[traderPurchaseEvent.ItemIndex].Item;
                        if (costItem == ShopModConfig.Config.SpecialPhysicalCurrency)
                        {
                            if (PointsManager.HasEnoughPoints(player, PointsType.Cosmetic, Math.Abs(costAmount)))
                            {
                                PointsManager.RemovePointsFromPlayer(player, PointsType.Cosmetic, Math.Abs(costAmount));
                                //player.ReceiveMessage($"You have spent {Math.Abs(costAmount)} {PointsModConfig.Config.CosmeticVirtualCurrencyName.Emphasize()}. New total: {PointsManager.GetPlayerPoints(player, PointsType.Cosmetic)}".White());
                            }
                            else
                            {
                                eventEntity.Destroy();
                                Helper.RemoveBuff(player, Prefabs.AB_Interact_Trade);
                                //player.ReceiveMessage($"You don't have enough points to purchase that! {PointsManager.GetPlayerPoints(player, PointsType.Cosmetic).ToString().Warning()} / {Math.Abs(costAmount).ToString().Warning()}".Error());
                                return;
                            }
                        }

                        if (ShopZone.Contains(trader) || trader.Has<CanFly>())
                        {
                            RefillStock(traderPurchaseEvent, trader);
                        }
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

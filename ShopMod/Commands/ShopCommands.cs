using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using static ModCore.Helpers.Helper;
using ModCore.Factories;
using System.Linq;
using ModCore.Data;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using ModCore;
using ProjectM;
using ProjectM.Shared;
using Unity.Entities;
using ShopMod.Managers;
using ProjectM.Network;
using Unity.Collections;
using ModCore.Services;
using Stunlock.Core;

namespace ShopMod.Commands
{
    public class ShopCommands
    {
        [Command("spawn-prisoner", description: "Spawns a prisoner in a targeted Prison Cell. Use ? to list types.", usage: ".spawn-prisoner <blood type | ?>", aliases: new string[] { "spawnprisoner", "sp" }, adminOnly: true)]
        public void SpawnPrisonerCommand(Player sender, string bloodTypeName)
        {
            var supported = new[] { "brute", "worker", "scholar", "warrior", "rogue", "creature" };

            if (bloodTypeName == "?")
            {
                sender.ReceiveMessage("Hover over a Prison Cell (F = 'Prison Cell'), then:".Colorify(ExtendedColor.LightServerColor));
                sender.ReceiveMessage($"  Supported types: {string.Join(" | ", supported)}".White());
                return;
            }

            if (!Helper.TryGetPrefabDataFromString(bloodTypeName, BloodData.BloodPrefabData, out var bloodData))
            {
                sender.ReceiveMessage($"Unknown blood type. Use .spawn-prisoner ? to see options.".Error());
                return;
            }

            if (!supported.Contains(bloodData.OverrideName.ToLower()))
            {
                sender.ReceiveMessage($"{bloodData.OverrideName} is not supported for prisoners. Supported: {string.Join(", ", supported)}".Error());
                return;
            }

            Entity target;
            try { target = sender.Character.Read<Interactor>().Target; }
            catch { sender.ReceiveMessage("Could not read target. Hover over a Prison Cell (F = 'Prison Cell').".Error()); return; }

            if (!target.Exists() || !target.Has<PrisonCell>())
            {
                sender.ReceiveMessage("You must hover over a Prison Cell (F button shows 'Prison Cell').".Error());
                return;
            }

            ShopHelper.SpawnPrisoner(target, bloodData.PrefabGUID);
            sender.ReceiveMessage($"Spawned {bloodData.OverrideName} prisoner!".Success());
        }

        [Command("daypassed", description: "Used for debugging", adminOnly: true)]
        public void DayPassedCommand(Player sender)
        {
            var daysPassed = Helper.GetServerTime() / 60 / 60 / 24;
            sender.ReceiveMessage("Day Passed:" + daysPassed);
        }

        [Command("resetkit", description: "Used for debugging", adminOnly: true, category: "QoL")]
        public void ResetKitCommand(Player sender, Player target = null)
        {
            if (target == null)
            {
                target = sender;
            }

            DataStorage.Data.PlayersWhoUsedKits.Remove(target);
            DataStorage.Save();

            if (target != sender)
            {
                target.ReceiveMessage($"Your kit has been reset");
            }
            sender.ReceiveMessage($"{target.FullNameColored}" + "'s".Colorify(ExtendedColor.ClanNameColor) + " kit has been reset".Success());
        }

        [Command("kit", description: "Gives you a one-time kit", includeInHelp: true, adminOnly: false, category: "QoL")]
        public void KitCommand(Player sender)
        {
            if (DataStorage.Data.PlayersWhoUsedKits.Contains(sender))
            {
                sender.ReceiveMessage("You have already used your kit".Error());
                return;
            }

            var daysPassed = Helper.GetServerTimeAdjusted() / 60 / 60 / 24;
            if (daysPassed < 1)
            {
                foreach (var item in ShopModConfig.Config.Day1KitItems)
                {
                    Helper.AddItemToInventory(sender, item.ItemPrefabGUID, item.Quantity, out var itemEntity);
                }
                sender.ReceiveMessage("Day 1 kit received!".Success());
            }
            else if (daysPassed < 2)
            {
                foreach (var item in ShopModConfig.Config.Day2KitItems)
                {
                    Helper.AddItemToInventory(sender, item.ItemPrefabGUID, item.Quantity, out var itemEntity);
                }
                sender.ReceiveMessage("Day 2 kit received!".Success());
            }
            else if (daysPassed < 3)
            {
                foreach (var item in ShopModConfig.Config.Day3KitItems)
                {
                    Helper.AddItemToInventory(sender, item.ItemPrefabGUID, item.Quantity, out var itemEntity);
                }
                sender.ReceiveMessage("Day 3 kit received!".Success());
            }
            else
            {
                foreach (var item in ShopModConfig.Config.Day4AndBeyondKitItems)
                {
                    Helper.AddItemToInventory(sender, item.ItemPrefabGUID, item.Quantity, out var itemEntity);
                }
                sender.ReceiveMessage("Day 4+ kit received!".Success());
            }
            DataStorage.Data.PlayersWhoUsedKits.Add(sender);
            DataStorage.Save();
        }

        //[Command("restock", description: "Used for debugging", aliases: ["traders", "merchants"], includeInHelp: true, adminOnly: false, category: "QoL")]
        public void MerchantRestockCommand(Player sender, string region)
        {
            var entities = Helper.GetEntitiesByComponentTypes<TraderSpawnData>(EntityQueryOptions.IncludeDisabledEntities);
            foreach (var entity in entities)
            {
                var activeUnitBuffer = entity.ReadBuffer<UnitCompositionActiveUnit>();
                var traderSpawnData = entity.Read<TraderSpawnData>();
                if (activeUnitBuffer.Length > 0 &&
                    activeUnitBuffer[0].UnitPrefab.LookupName().ToLower().Contains(region.ToLower()))
                {
                    sender.ReceiveMessage($"{activeUnitBuffer[0].UnitPrefab.LookupName().Replace("CHAR_Trader_", "")} {Helper.FormatTime(traderSpawnData.NextRestockTime - Helper.GetServerTime()).Warning()}".White());
                }
            }
            //sender.ReceiveMessage($"The test worked: {ShopModConfig.Config.TestField}");
        }

        [Command("spawn-merchant", usage: ".spawn-merchant Cloaks", adminOnly: true, aliases: ["spawn-trader", "spawnmerchant", "spawntrader"])]
        public static void SpawnMerchantCommand(Player sender, string merchantName, int spawnSnapMode = 5)
        {
            var spawnPosition = Helper.GetSnappedHoverPosition(sender, (SnapMode)spawnSnapMode);
            SpawnMerchant(merchantName, spawnPosition);
        }

        private static void SpawnMerchant(string merchantName, float3 spawnPosition)
        {
            foreach (var trader in ShopModConfig.Config.Traders)
            {
                if (trader.UnitSpawn.Description.ToLower() == merchantName.ToLower())
                {
                    var unit = new ModCore.Factories.Trader(trader.UnitSpawn.PrefabGUID, trader.TraderItems);
                    unit.IsImmaterial = true;
                    unit.IsInvulnerable = true;
                    unit.IsRooted = true;
                    unit.IsTargetable = false;
                    unit.DrawsAggro = false;
                    unit.KnockbackResistance = true;
                    unit.MaxHealth = 10000;
                    unit.Name = merchantName.ToLower();
                    unit.Category = merchantName.ToLower();
                    UnitFactory.SpawnUnitWithCallback(unit, spawnPosition, (e) =>
                    {
                    });
                    /*UnitFactory.SpawnUnit(unit, trader.UnitSpawn.Location.ToFloat3());*/
                }
            }
        }

        [Command("jewel", description: "Creates a jewel with the mods of your choice. Do .j phantomaegis ? to see the options", usage: ".j phantomaegis 123", aliases: ["j", "je", "jew", "jewe"], adminOnly: true, includeInHelp: true, category: "Shop")]
        public static void JewelCommand(Player sender, string input1, string input2 = "", string input3 = "", string input4 = "", string input5 = "", string input6 = "", string input7 = "", string input8 = "")
        {
            /*if (!ShopZoneManager.ShopZone.Contains(sender))
			{
			    sender.ReceiveMessage("You cannot create jewels outside of the shop".Error());
			    return;
			}*/
/*
            if (PointsModConfig.Config.UsePhysicalCurrency)
            {
                if (!Helper.PlayerHasEnoughItemsInInventory(sender, ShopModConfig.Config.LegendaryCost.ItemPrefabGUID, ShopModConfig.Config.LegendaryCost.Quantity))
                {
                    sender.ReceiveMessage($"You need {ShopModConfig.Config.LegendaryCost.Quantity.ToString().Warning()} {"Vesper".Warning()} to make a legendary".Error());
                    return;
                }
            }
            else
            {
                if (!PointsManager.HasEnoughPoints(sender, PointsType.Main, ShopModConfig.Config.JewelCost.Quantity))
                {
                    sender.ReceiveMessage($"You need {ShopModConfig.Config.JewelCost.Quantity.ToString().Warning()} {PointsModConfig.Config.MainVirtualCurrencyName.Warning()} to make a jewel".Error());
                    return;
                }
            }*/

            string spellName, mods;
            float power;
            ParseInputs(input1, input2, input3, input4, input5, input6, input7, input8, out spellName, out mods, out power);
            if (input1 == "?")
            {
                sender.ReceiveMessage(("Jewel example:".Colorify(ExtendedColor.ServerColor) + " .j bloodrite 1234").Emphasize());
                sender.ReceiveMessage(("To display the list of mods use " + ".j spellName ?".Colorify(ExtendedColor.LightServerColor)).White());
                return;
            }

            if (!Helper.TryGetJewelPrefabDataFromString(spellName.ToLower(), out PrefabData item))
            {
                sender.ReceiveMessage("Couldn't find the ability name.".Error());
                return;
            }

            var properName = item.GetName();
            var condensedName = item.GetName().Replace(" ", "").ToLower();
            if (!Regex.IsMatch(mods, @"^[0-9a-fA-F]{4}$") && mods != "?")
            {
                sender.ReceiveMessage("Mods should be four numbers or A, B, i.e. 1234.".Error());
                return;
            }

            if (mods.GroupBy(c => c).Any(g => g.Count() > 1))
            {
                sender.ReceiveMessage("No duplicate mods allowed.".Error());
                return;
            }

            SchoolData jewelSchoolData = JewelData.abilityToSchoolDictionary[condensedName];
            ;

            if (mods == "?")
            {
                sender.ReceiveMessage($"Mods for {properName.Colorify(jewelSchoolData.lightColor)}".Colorify(jewelSchoolData.color));
                int i = 1;
                foreach (var modPrefab in JewelData.AbilityToSpellMods[item.PrefabGUID])
                {
                    var hexValue = i.ToString("X");
                    sender.ReceiveMessage($"{hexValue.Colorify(jewelSchoolData.color)} - {JewelData.SpellModDescriptions[modPrefab]}".White());
                    i++;
                }
            }
            else
            {
                var mod1 = Convert.ToInt32(mods[0].ToString(), 16) - 1;
                var mod2 = Convert.ToInt32(mods[1].ToString(), 16) - 1;
                var mod3 = Convert.ToInt32(mods[2].ToString(), 16) - 1;
                if (mod1 < 0 || mod1 >= JewelData.AbilityToSpellMods[item.PrefabGUID].Count)
                {
                    sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
                    return;
                }

                if (mod2 < 0 || mod2 >= JewelData.AbilityToSpellMods[item.PrefabGUID].Count)
                {
                    sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
                    return;
                }

                if (mod3 < 0 || mod3 >= JewelData.AbilityToSpellMods[item.PrefabGUID].Count)
                {
                    sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
                    return;
                }

/*                if (PointsModConfig.Config.UsePhysicalCurrency)
                {
                    Helper.RemoveItemFromInventory(sender, ShopModConfig.Config.JewelCost.ItemPrefabGUID, ShopModConfig.Config.JewelCost.Quantity);
                    sender.ReceiveMessage($"You used {ShopModConfig.Config.JewelCost.Quantity.ToString().Warning()} {Helper.GetItemName(ShopModConfig.Config.JewelCost.ItemPrefabGUID).Warning()} to create a jewel".Success());
                }
                else
                {
                    PointsManager.RemovePointsFromPlayer(sender, PointsType.Main, ShopModConfig.Config.JewelCost.Quantity);
                    sender.ReceiveMessage($"You used {ShopModConfig.Config.JewelCost.Quantity.ToString().Warning()} {PointsModConfig.Config.MainVirtualCurrencyName.Warning()} to create a jewel".Success());
                }*/
                
                Helper.GenerateJewelViaEvent(sender, condensedName, mods, power);
            }
        }

        private static void ParseInputs(string input1, string input2, string input3, string input4, string input5, string input6, string input7, string input8, out string spellName, out string mods, out float power)
        {
            // Initialize variables
            spellName = "";
            mods = "";
            power = 1;

            // Consolidate and preprocess inputs
            List<string> inputs = new List<string> { input1, input2, input3, input4, input5, input6, input7, input8 };
            inputs = inputs.Select(input => input.Replace(",", "")).ToList(); // Remove commas
            inputs.RemoveAll(string.IsNullOrEmpty);

            // Identify spellName, mods, and power
            bool modsIdentified = false;
            foreach (var input in inputs)
            {
                if (input == "?" || input.EndsWith("?"))
                {
                    mods = "?";
                    modsIdentified = true;
                    spellName += input.TrimEnd('?'); // Add to spellName but remove trailing '?'
                    break; // No need to look for numbers if mods is "?"
                }
                else if (int.TryParse(input, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out int num))
                {
                    mods += num.ToString("X"); // Convert decimal to hexadecimal
                    if (mods.Length >= 4)
                    {
                        mods = mods.Substring(0, 4); // Ensure mods is exactly 3 characters
                        modsIdentified = true;
                    }
                }
                else if (!modsIdentified)
                {
                    spellName += input;
                }
                else if (float.TryParse(input, out float numPower))
                {
                    power = numPower;
                    break; // Assuming only one power parameter is expected
                }
            }

            // Remove all spaces from spellName
            spellName = spellName.Replace(" ", "");
        }

        //[Command("legendary", description: "Gives you a custom legendary. Do .lw ? to see the mod options", usage: ".lw spear storm 123", aliases: ["lw", "leg", "lego", "l"], adminOnly: false, includeInHelp: true, category: "Shop")]
        public static void LegendaryCommand(Player sender, string weaponName, string infusion = "", string mods = "", float power = 1)
        {
            if (!Helper.PlayerHasEnoughItemsInInventory(sender, ShopModConfig.Config.LegendaryCost.ItemPrefabGUID, ShopModConfig.Config.LegendaryCost.Quantity))
            {
                sender.ReceiveMessage($"You need {ShopModConfig.Config.LegendaryCost.Quantity.ToString().Warning()} {"Vesper".Warning()} to make a legendary".Error());
                return;
            }

            if (weaponName == "?")
            {
                sender.ReceiveMessage(("Legendary example:".Colorify(ExtendedColor.ServerColor) + " .lw spear storm 123").Emphasize());
                sender.ReceiveMessage(("List of mods:".Colorify(ExtendedColor.LightServerColor)).Emphasize());
                var i = 1;
                foreach (var description in LegendaryData.statModDescriptions)
                {
                    // Convert the index to a hexadecimal string
                    var hexValue = i.ToString("X");
                    sender.ReceiveMessage($"{hexValue.Colorify(ExtendedColor.ServerColor)} - {description}".White());
                    i++;
                }
            }
            else
            {
                if (!LegendaryData.weaponToShatteredPrefabDictionary.TryGetValue(weaponName.ToLower(), out var weaponPrefabGUID))
                {
                    sender.ReceiveMessage("Invalid weapon name.".Error());
                    return;
                }

                if (!LegendaryData.infusionToPrefabDictionary.TryGetValue(infusion.ToLower(), out var infusionPrefabGUID))
                {
                    sender.ReceiveMessage("Invalid infusion name.".Error());
                    return;
                }

                // Updated regex to match hexadecimal values
                if (!Regex.IsMatch(mods, @"^[0-9a-fA-F]{3}$") && mods != "?")
                {
                    sender.ReceiveMessage("Invalid mods - should be three characters, i.e. 12A.".Error());
                    return;
                }

                if (mods.GroupBy(c => c).Any(g => g.Count() > 1))
                {
                    sender.ReceiveMessage("Invalid mods - no duplicate mods allowed!".Error());
                    return;
                }

                var mod1 = Convert.ToInt32(mods[0].ToString(), 16) - 1;
                var mod2 = Convert.ToInt32(mods[1].ToString(), 16) - 1;
                var mod3 = Convert.ToInt32(mods[2].ToString(), 16) - 1;
                if (mod1 < 0 || mod1 > LegendaryData.statMods.Count)
                {
                    sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
                    return;
                }

                if (mod2 < 0 || mod2 > LegendaryData.statMods.Count)
                {
                    sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
                    return;
                }

                if (mod3 < 0 || mod3 > LegendaryData.statMods.Count)
                {
                    sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
                    return;
                }

                if (PointsModConfig.Config.UsePhysicalCurrency)
                {
                    Helper.RemoveItemFromInventory(sender, ShopModConfig.Config.LegendaryCost.ItemPrefabGUID, ShopModConfig.Config.LegendaryCost.Quantity);
                    sender.ReceiveMessage($"You used {ShopModConfig.Config.LegendaryCost.Quantity.ToString().Warning()} {Helper.GetItemName(ShopModConfig.Config.LegendaryCost.ItemPrefabGUID).Warning()} to create a legendary".Success());
                }
                else
                {
                    PointsManager.RemovePointsFromPlayer(sender, PointsType.Main, ShopModConfig.Config.LegendaryCost.Quantity);
                    sender.ReceiveMessage($"You used {ShopModConfig.Config.LegendaryCost.Quantity.ToString().Warning()} {PointsModConfig.Config.MainVirtualCurrencyName.Warning()} to create a legendary".Success());
                }

                Helper.GenerateLegendaryViaEvent(sender, weaponName.ToLower(), infusion.ToLower(), mods, power, false);
            }
        }

        //[Command("upgradehorse", description: "Makes your vampire horse's stats perfect", aliases: new string[] { "upgrade-horse" }, includeInHelp: true, adminOnly: false, category: "Upgrades")]
        public void UpgradeHorseCommand(Player sender)
        {
            var target = sender.Character.Read<Interactor>().Target;
            if (!target.Has<Mountable>())
            {
                sender.ReceiveMessage("You must be hovering your vampire horse!".Error());
                return;
            }

            var attachedBuffer = target.ReadBuffer<AttachedBuffer>();
            foreach (var attached in attachedBuffer)
            {
                if (attached.Entity.GetPrefabGUID() == Prefabs.AB_Subdue_Active_Capture_Buff)
                {
                    if (attached.Entity.Read<EntityOwner>().Owner == sender.Character)
                    {
                        if (PointsManager.HasEnoughPoints(sender, PointsType.Main, ShopModConfig.Config.HorseUpgradeCost.Quantity))
                        {
                            PointsManager.RemovePointsFromPlayer(sender, PointsType.Main, ShopModConfig.Config.HorseUpgradeCost.Quantity);

                            var mountable = target.Read<Mountable>();
                            mountable.MaxSpeed = 11;
                            mountable.Acceleration = 7;
                            mountable.RotationSpeed = 140;
                            target.Write(mountable);
                            sender.ReceiveMessage($"You used {ShopModConfig.Config.HorseUpgradeCost.Quantity.ToString().Warning()} {PointsModConfig.Config.MainVirtualCurrencyName.Warning()} to upgrade your horse".Success());
                            return;
                        }
                        else
                        {
                            sender.ReceiveMessage($"You need {ShopModConfig.Config.HorseUpgradeCost.Quantity.ToString().Warning()} {PointsModConfig.Config.MainVirtualCurrencyName.Warning()} to upgrade your vampire horse!".Error());
                            return;
                        }
                    }
                }
            }

            sender.ReceiveMessage("You must be interacting with your vampire horse!".Error());
        }

        /*
		[Command("upgradeservant", description: "Upgrades the quality of your servant by 4% up to 44% expertise",
			aliases: new string[] { "upgrade-servant" }, includeInHelp: true, adminOnly: false, category: "Upgrades")]
		public void UpgradeServantCommand (Player sender)
		{
			var maxProficiency = .44f;
			var coffin = sender.Character.Read<Interactor>().Target;
			if (!(coffin.Exists() && coffin.Has<ServantCoffinstation>()))
			{
				sender.ReceiveMessage("You must be interacting with a coffin to use this command!".Error());
				return;
			}

			if (!Team.IsAllies(sender.Team, coffin.Read<Team>()))
			{
				sender.ReceiveMessage("Cannot upgrade an enemy servant!".Error());
				return;
			}

			if (!Helper.PlayerHasEnoughItemsInInventory(sender, ShopModConfig.Config.ServantUpgradeCost.ItemPrefabGUID,
				    ShopModConfig.Config.ServantUpgradeCost.Quantity))
			{
				sender.ReceiveMessage(
					$"You need {ShopModConfig.Config.ServantUpgradeCost.Quantity.ToString().Warning()} {"Vesper".Warning()} to use this command!"
						.Error());
				return;
			}

			var servantCoffinStation = coffin.Read<ServantCoffinstation>();
			if (servantCoffinStation.ServantProficiency >= .44f)
			{
				sender.ReceiveMessage("This servant is already fully empowered!".Error());
				return;
			}

			var amountToIncrease = ShopModConfig.Config.ServantUpgradeAmount * maxProficiency;
			servantCoffinStation.BloodQuality =
				Math.Min(servantCoffinStation.BloodQuality + ShopModConfig.Config.ServantUpgradeAmount * 100, 100);
			servantCoffinStation.ServantProficiency =
				Math.Min(servantCoffinStation.ServantProficiency + amountToIncrease, maxProficiency);
			coffin.Write(servantCoffinStation);
			Helper.RemoveItemFromInventory(sender, ShopModConfig.Config.ServantUpgradeCost.ItemPrefabGUID,
				ShopModConfig.Config.ServantUpgradeCost.Quantity);
			sender.ReceiveMessage(
				$"You used {ShopModConfig.Config.ServantUpgradeCost.Quantity.ToString().Warning()} {"Vesper".Warning()} to upgrade your servant"
					.Success());
		}
		*/


        [Command("remove-item", adminOnly: true)]
        public static void RemovePlayerItemFromInventory(Player sender, Player foundPlayer,
            PrefabGUID prefabGuid)
        {
            var inventoryResponse = InventoryUtilitiesServer.TryRemoveItemFromInventories(VWorld.Server.EntityManager, foundPlayer.Character, prefabGuid, 1);
            if (inventoryResponse)
                sender.ReceiveMessage(("Item " + prefabGuid.ToString().Emphasize() + " removed from " + foundPlayer.Name.Emphasize() +"'s inventory.").Success());
            else
                sender.ReceiveMessage(("Item " + prefabGuid.ToString().Emphasize() + " not removed from " + foundPlayer.Name.Emphasize() + "'s inventory.").Error());
        }

        [Command("remove-item-from-everyone", adminOnly: true)]
        public static void RemoveItemFromAllInventories(Player sender, PrefabGUID prefabGuid)
        {
            bool inventoryResponse = false;
            foreach (var Player in PlayerService.CharacterCache.Values)
            {
                if (!Player.IsAdmin)
                {
                    if (InventoryUtilitiesServer.TryRemoveItemFromInventories(VWorld.Server.EntityManager, Player.Character, prefabGuid, 10))
                    {
                        inventoryResponse = true;
                    }
                }
            }

            if (inventoryResponse)
                sender.ReceiveMessage(("Item " + prefabGuid.ToString().Emphasize() + " removed from all inventories!").Success());
            else
                sender.ReceiveMessage(("Item " + prefabGuid.ToString().Emphasize() + " not removed from all inventories!").Error());
        }

        [Command("list-people-with-item", adminOnly: true)]
        public static void ListItemFromAllInventories(Player sender, PrefabGUID prefabGuid)
        {
            bool inventoryResponse = false;
            foreach (var Player in PlayerService.CharacterCache.Values)
            {
                if (Helper.PlayerHasItemInInventories(Player, prefabGuid))
                {
                    sender.ReceiveMessage((Player.Name.Emphasize() + " has the item!").White());
                }
            }
        }

        [Command("list-items", adminOnly: true)]
        public static void LogItems(Player sender, Player player)
        {
            ListItemsFromInventory(sender, player.Character);
        }

        public static void ListItemsFromInventory(Player sender, Entity _recipient)
        {
            List<string> itemNames = new List<string>();

            //I think this buffer contains ALL player inventories
            var buffer = _recipient.ReadBuffer<InventoryInstanceElement>();
            for (var i = 0; i < buffer.Length; i++)
            {
                var inventoryInstanceElement = buffer[i];
                if (inventoryInstanceElement.ExternalInventoryEntity._Entity.Exists())
                {
                    //confirm the inventory belongs to the person
                    if (inventoryInstanceElement.ExternalInventoryEntity._Entity.Read<InventoryConnection>().InventoryOwner == _recipient)
                    {
                        var inventoryBuffer = inventoryInstanceElement.ExternalInventoryEntity._Entity.ReadBuffer<InventoryBuffer>();
                        foreach (var item in inventoryBuffer)
                        {
                            if (item.ItemEntity._Entity.Exists())
                            {
                                itemNames.Add(item.ItemEntity._Entity.LookupName().Split(" ")[0]);
                            }
                        }
                    }
                }
            }

            var equipment = _recipient.Read<Equipment>();
            NativeList<Entity> equipmentEntities = new NativeList<Entity>(Allocator.Temp);
            equipment.GetAllEquipmentEntities(equipmentEntities);
            foreach (var equipmentEntity in equipmentEntities)
            {
                if (equipmentEntity.Exists())
                {
                    if (equipmentEntity.Read<EquippableData>().EquipmentType != EquipmentType.Weapon)
                    {
                        itemNames.Add(equipmentEntity.LookupName().Split(" ")[0]);
                    }
                }
            }

            equipmentEntities.Dispose();
            itemNames.Sort();
            foreach (var itemName in itemNames)
            {
                sender.ReceiveMessage(itemName.White());
            }
        }
    }
}
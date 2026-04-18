using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Helpers;
using ModCore.Data;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ModCore;
using ProjectM.Network;
using Unity.Collections;
using ProjectM.CastleBuilding;
using ProjectM.Terrain;
using ProjectM;
using Unity.Entities;
using ModCore.Services;
using static ProjectM.SpawnBuffsAuthoring.SpawnBuffElement_Editor;
using ChipSaMod.Managers;
using Il2CppSystem;
using Stunlock.Core;
using ProjectM.Tiles;
using static Unity.Entities.ComponentSystemSorter;
using Unity.Mathematics;
using Unity.Transforms;
using ModCore.Factories;

namespace ChipSaMod.Commands
{
    public class ChipSaCommands
    {
        [Command("test", description: "test", adminOnly: false)]
        public unsafe static void TestCommand(Player sender)
        {
/*            var entities = Helper.GetPrefabEntitiesByComponentTypes<IsMinion>();
            foreach (var entity in entities)
            {
                entity.LogPrefabName();
            }*/
        }


        [Command("tpp", description: "Teleport to a player", adminOnly: false, includeInHelp: true, aliases: ["tp-player", "teleport-player"])]
        public unsafe static void TeleportCommand(Player sender, Player target)
        {
            sender.Teleport(target.Position);
            sender.ReceiveMessage("Teleported!".Success());
        }

        [Command("gearkit", description: "Gives gear kit by class (brute/scholar/warrior/...)", adminOnly: false, includeInHelp: true, aliases: new[] { "gk" })]
        public static void KitCommand(Player sender, string kitName)
        {
            if (kitName == "brute")
            {
                foreach (var item in Kits.StartingGearBrute)
                {
                    Helper.AddItemToInventory(sender, item, 1, out var itemEntity);
                }
            }
            else if (kitName == "scholar")
            {
                foreach (var item in Kits.StartingGearScholar)
                {
                    Helper.AddItemToInventory(sender, item, 1, out var itemEntity);
                }
            }
            else if (kitName == "warrior")
            {
                foreach (var item in Kits.StartingGearWarrior)
                {
                    Helper.AddItemToInventory(sender, item, 1, out var itemEntity);
                }
            }
            else if (kitName == "rogue")
            {
                foreach (var item in Kits.StartingGearRogue)
                {
                    Helper.AddItemToInventory(sender, item, 1, out var itemEntity);
                }
            }
            else if (kitName == "necks" || kitName == "neck" || kitName == "necklaces")
            {
                foreach (var item in Kits.Necks)
                {
                    Helper.AddItemToInventory(sender, item, 1, out var itemEntity);
                }
            }
            else if (kitName == "shard" || kitName == "shards")
            {
                foreach (var item in Kits.ShardsNecks)
                {
                    Helper.AddItemToInventory(sender, item, 1, out var itemEntity);
                }
            }
            sender.ReceiveMessage("Gave kit".Success());
        }

        [Command("clearinventory", aliases: new string[] { "ci", "clear-inventory" }, description: "Deletes all items in your inventory (excluding 1st row)", adminOnly: false)]
        public static void CleanInventoryCommand(Player sender, bool all = false)
        {
            Helper.ClearPlayerInventory(sender, all);
            sender.ReceiveMessage("Cleared inventory".Success());
        }

        [Command("spectate", description: "Spectate a player", adminOnly: false)]
        public static void SpectateCommand(Player sender, Player target = null)
        {
            if (target != null)
            {
                SpectatingManager.AddSpectator(sender, target);
            }
            else
            {
                SpectatingManager.RemoveSpectator(sender);
            }
        }

        [Command("shuffle-teams", description: "Makes everyone in an area join a clan and teleports them into two parallel lines", aliases: new string[] { "st", "shuffle teams" }, adminOnly: true)]
        public void ShuffleTeamsCommand(Player sender, int distance = 25)
        {
            var playersToDivide = new List<Player>();
            foreach (var player in PlayerService.OnlinePlayersWithCharacters)
            {
                if (math.distance(player.Position, sender.Position) <= distance)
                {
                    if (!SpectatingManager.SpectatorToSpectatedPlayer.ContainsKey(player))
                    {
                        playersToDivide.Add(player);
                    }
                }
            }

            if (playersToDivide.Count >= 2) // Ensure there are at least 2 players to form two clans
            {
                // Set the first two players as clan leaders and remove them from the list
                var clanLeader1 = playersToDivide[0];
                var clanLeader2 = playersToDivide[1];
                playersToDivide.RemoveAt(1); // Remove second leader first to maintain index integrity
                playersToDivide.RemoveAt(0); // Remove first leader

                // Shuffle the remaining players
                System.Random rng = new System.Random();
                int n = playersToDivide.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    Player value = playersToDivide[k];
                    playersToDivide[k] = playersToDivide[n];
                    playersToDivide[n] = value;
                }

                // Create clans and add the leaders
                Helper.RemoveFromClan(clanLeader1);
                Helper.CreateClanForPlayer(clanLeader1);
                Helper.RemoveFromClan(clanLeader2);
                Helper.CreateClanForPlayer(clanLeader2);

                // Determine starting positions for two parallel lines
                float3 line1StartPos = sender.Position + new float3(5, 0, 0); // 5 units to the right of the sender on the x-axis
                float3 line2StartPos = sender.Position - new float3(5, 0, 0); // 5 units to the left of the sender on the x-axis
                float spacing = 2; // Space between players in a line on the z-axis

                // Teleport clan leaders to the start of each line
                if (clanLeader1 != sender)
                {
                    clanLeader1.Teleport(line1StartPos);
                }
                if (clanLeader2 != sender)
                {
                    clanLeader2.Teleport(line2StartPos);
                }

                // Assign the rest of the players to clans and teleport them to form two parallel lines
                for (int i = 0; i < playersToDivide.Count; i++)
                {
                    float3 targetPosition;
                    if (i % 2 == 0)
                    {
                        // Add player to Clan 1 and set position in line 1
                        Helper.AddPlayerToPlayerClanForce(playersToDivide[i], clanLeader1);
                        targetPosition = line1StartPos + new float3(0, 0, ((i / 2 + 1) * spacing)); // +1 because leader is at the start
                    }
                    else
                    {
                        // Add player to Clan 2 and set position in line 2
                        Helper.AddPlayerToPlayerClanForce(playersToDivide[i], clanLeader2);
                        targetPosition = line2StartPos + new float3(0, 0, ((i / 2 + 1) * spacing)); // +1 because leader is at the start
                    }
                    if (playersToDivide[i] != sender)
                    {
                        playersToDivide[i].Teleport(targetPosition);
                    }
                }
            }
            sender.ReceiveMessage("Shuffled.".White());
        }

        [Command("spawnhorse", adminOnly: false, includeInHelp: true)]
        public static void SpawnHorseCommand(Player sender, int speed = 11, int acceleration = 7, int rotation = 14)
        {
            var horse = new Horse
            {
                Speed = speed,
                Acceleration = acceleration,
                Rotation = rotation
            };
            UnitFactory.SpawnUnitWithCallback(horse, sender.Position, (e) => { });
            Helper.AddItemToInventory(sender, Prefabs.Item_Saddle_Basic, 1, out var itemEntity);
        }

        [Command("destroycoffins", adminOnly: false)]
        public static void DestroyCoffinsCommand(Player sender, bool all = false)
        {
            if (sender.IsInBase(out var territory, out var territoryAlignment) && territoryAlignment == Helper.TerritoryAlignment.Friendly)
            {

            }
            sender.ReceiveMessage("Cleared inventory".Success());
        }

        [Command("clearplot", description: "Clears base territory", adminOnly: false, includeInHelp: true, category: "Clear Base")]
        public unsafe static void ClearBaseCommand(Player sender)
        {
            if (Helper.TryGetCurrentCastleTerritory(sender, out var territoryEntity))
            {
                var buffer = territoryEntity.ReadBuffer<CastleTerritoryBlocks>();
                foreach (var block in buffer)
                {
                    var bounds = new BoundsMinMax((block.BlockCoordinate - 2) * 10, (block.BlockCoordinate + 2) * 10);
                    var entities = Helper.GetEntitiesInArea(bounds, TileType.All);
                    foreach (var entity in entities)
                    {
                        if ((entity.Has<Health>() || entity.Has<ItemPickup>()) && !entity.Has<PlayerCharacter>() && !entity.Has<CastleHeartConnection>() && !entity.Has<ServantConnectedCoffin>() && !entity.Has<IsMinion>())
                        {
                            Helper.DestroyEntity(entity);
                        }
                    }
                }

                var droppedItems = Helper.GetEntitiesByComponentTypes<ItemPickup>();
                foreach (var entity in droppedItems)
                {
                    if (Helper.TryGetCurrentCastleTerritory(entity, out var territoryEntity2) && territoryEntity == territoryEntity2)
                    {
                        Helper.DestroyEntity(entity);
                    }
                }
            }
        }

        [Command("clearall", description: "Clears base territory", adminOnly: true, includeInHelp: false, category: "Clear Base")]
        public unsafe static void ClearAllCommand(Player sender)
        {
            if (Helper.TryGetCurrentCastleTerritory(sender, out var territoryEntity))
            {
                var buffer = territoryEntity.ReadBuffer<CastleTerritoryBlocks>();
                foreach (var block in buffer)
                {
                    var bounds = new BoundsMinMax((block.BlockCoordinate - 2) * 10, (block.BlockCoordinate + 2) * 10);
                    var entities = Helper.GetEntitiesInArea(bounds, TileType.All);
                    foreach (var entity in entities)
                    {
                        if ((entity.Has<Health>() || entity.Has<ItemPickup>() || entity.Has<CastleHeartConnection>()) && !entity.Has<PlayerCharacter>())
                        {
                            Helper.DestroyEntity(entity);
                        }
                    }
                }
            }
            var droppedItems = Helper.GetEntitiesByComponentTypes<ItemPickup>();
            foreach (var entity in droppedItems)
            {
                Helper.DestroyEntity(entity);
            }
        }

        [Command("jewel", description: "Creates a jewel with the mods of your choice. Do .j phantomaegis ? to see the options", usage: ".j phantomaegis 123", aliases: ["j", "je", "jew", "jewe"], adminOnly: false, includeInHelp: true, category: "Items")]
        public static void JewelCommand(Player sender, string input1, string input2 = "", string input3 = "", string input4 = "", string input5 = "", string input6 = "", string input7 = "", string input8 = "")
        {
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
                var mod1 = System.Convert.ToInt32(mods[0].ToString(), 16) - 1;
                var mod2 = System.Convert.ToInt32(mods[1].ToString(), 16) - 1;
                var mod3 = System.Convert.ToInt32(mods[2].ToString(), 16) - 1;
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

                sender.ReceiveMessage("Jewel created!".Success());
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

        [Command("legendary", description: "Gives you a custom legendary. Do .lw ? to see the mod options", usage: ".lw spear storm 123", aliases: ["lw", "leg", "lego", "l"], adminOnly: false, includeInHelp: true, category: "Items")]
        public static void LegendaryCommand(Player sender, string weaponName, string infusion = "", string mods = "", float power = 1)
        {
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

                var mod1 = System.Convert.ToInt32(mods[0].ToString(), 16) - 1;
                var mod2 = System.Convert.ToInt32(mods[1].ToString(), 16) - 1;
                var mod3 = System.Convert.ToInt32(mods[2].ToString(), 16) - 1;
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

                Helper.GenerateLegendaryViaEvent(sender, weaponName.ToLower(), infusion.ToLower(), mods, power, false);

                sender.ReceiveMessage("Legendary created!".Success());
            }
        }

        [Command("artifact", description: "Gives you a named artifact. Do .art sword", usage: ".art spear slashers1 sword", aliases: new string[] { "art", "aw", "artifact" }, adminOnly: false, includeInHelp: true, category: "Legendaries")]
        public static void ArtifactCommand(Player sender, string weaponName)
        {
            if (LegendaryData.weaponToArtifactPrefabDictionary.TryGetValue(weaponName, out var artifacts))
            {
                foreach (var artifact in artifacts)
                {
                    Helper.AddItemToInventory(sender.Character, artifact, 1, out var itemEntity);
                }
            }
            sender.ReceiveMessage("Artifact created!".Success());
        }

        [Command("setservants", description: "Overwrites servants with units of your choice, do .setservants ? to see options", adminOnly: false, includeInHelp: true, category: "Misc")]
        public void SetServantsCommand(Player sender, string servantTypes, float quality = 1f, bool includeGear = true, float truePower = -1, Player targetPlayer = null)
        {
            if (servantTypes == "?")
            {
                for (var i = 0; i < ModCore.Data.ServantData.ServantTypes.Count; i++)
                {
                    sender.ReceiveMessage($"{Base36Encode(i)}: {ModCore.Data.ServantData.ServantTypes[i]}".White());
                }

                return;
            }

            if (!Helper.IsInBase(sender, out var territoryEntity, out var territoryAlignment) || territoryAlignment != Helper.TerritoryAlignment.Friendly)
            {
                sender.ReceiveMessage("You must be in your territory to run this command!".Error());
                return;
            }

            var heart = territoryEntity.Read<CastleTerritory>().CastleHeart;
            if (!heart.Exists())
            {
                sender.ReceiveMessage("This territory has no base!".Error()); //should be impossible
            }

            var servantCoffins = Helper.GetEntitiesByComponentTypes<ServantCoffinstation>();

            var clanMembers = sender.GetClanMembers();
            var ClanIndex = 0;
            var index = 0;
            if (servantCoffins.Length > 0)
            {
                foreach (var servantCoffin in servantCoffins)
                {
                    if (servantCoffin.Read<CastleHeartConnection>().CastleHeartEntity._Entity != heart) continue;

                    var coffinTeam = servantCoffin.Read<Team>();
                    if (Team.IsAllies(coffinTeam, sender.Team))
                    {
                        // Get the current character from the servantTypes string and convert it to a Base36 number
                        var base36Char = servantTypes[index % servantTypes.Length];
                        var base36Index = Base36Decode(base36Char.ToString());

                        var servantType = ModCore.Data.ServantData.UnitToServantList[base36Index];
                        index++;

                        var servantCoffinStation = servantCoffin.Read<ServantCoffinstation>();
                        var servant = servantCoffinStation.ConnectedServant._Entity;
                        if (servant.Exists())
                        {
                            servant.Write(new ServantEquipment());
                            InventoryUtilitiesServer.ClearInventory(VWorld.Server.EntityManager, servant);
                            StatChangeUtility.KillEntity(VWorld.Server.EntityManager, servant, Entity.Null, 0, StatChangeReason.Any, true);
                        }
                        
                        servantCoffinStation.ServantName = clanMembers[ClanIndex % clanMembers.Count].Name;
                        ClanIndex++;
                        servantCoffinStation.State = ServantCoffinState.Reviving;
                        servantCoffinStation.ConvertionProgress = 600;
                        servantCoffinStation.ConvertFromUnit = servantType.Key;
                        servantCoffinStation.ConvertToUnit = servantType.Value;
                        servantCoffinStation.BloodQuality = quality * 100;
                        servantCoffinStation.ServantProficiency = quality * .44f;

                        servantCoffin.Write(servantCoffinStation);
                    }

                    if (includeGear)
                    {
                        var action = () => Helper.CreateAndEquipServantGear(servantCoffin);
                        ActionScheduler.RunActionOnceAfterFrames(action, 5);
                    }
                }

                sender.ReceiveMessage("Replaced all servants with the ones specified".White());
            }
            else
            {
                sender.ReceiveMessage("No servant coffins found".White());
            }
        }

    // Helper method to decode Base36 characters
    private int Base36Decode(string input)
    {
        const string base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return base36Chars.IndexOf(input.ToUpper());
    }

    // Helper method to encode an integer to Base36
    private string Base36Encode(int value)
    {
        const string base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        var result = string.Empty;
        do
        {
            result = base36Chars[value % 36] + result;
            value /= 36;
        } while (value > 0);

        return result;
    }


    [Command("reviveservants", description: "Revives all servants", includeInHelp: true, adminOnly: false, category: "Misc")]
        public void ReviveServantsCommand(Player sender)
        {
            var servantCoffins = Helper.GetEntitiesByComponentTypes<ServantCoffinstation>();
            foreach (var servantCoffin in servantCoffins)
            {
                var coffinTeam = servantCoffin.Read<Team>();
                if (Team.IsAllies(coffinTeam, sender.Team))
                {
                    var coffin = servantCoffin.Read<ServantCoffinstation>();
                    if (coffin.State == ServantCoffinState.Converting || coffin.State == ServantCoffinState.Reviving ||
                        coffin.State == ServantCoffinState.ServantRevivable)
                    {
                        if (coffin.State == ServantCoffinState.ServantRevivable)
                        {
                            coffin.State = ServantCoffinState.Reviving;
                        }

                        coffin.ConvertionProgress = 600;
                    }

                    servantCoffin.Write(coffin);
                }
            }

            sender.ReceiveMessage($"Resurrected servants for {sender.Name.Colorify(ExtendedColor.ClanNameColor) ?? "you"}.".White());
        }

        [Command("bp", usage: ".bp warrior", description: "Sets your blood", aliases: ["blood", "b", "blod", "bloodpotion"], adminOnly: false, includeInHelp: true, category: "Potions")]
        public static void SetBloodCommand(Player sender, BloodPrefabData bloodType, float quality = 100f)
        {
            Helper.SetPlayerBlood(sender, bloodType.PrefabGUID, quality);
        }

        [Command("buffs", usage: ".buffs", description: "Toggles buffs", aliases: ["buff"], adminOnly: false, includeInHelp: true, category: "Potions")]
        public static void BuffsCommand(Player sender)
        {
            if (!Helper.HasBuff(sender, Prefabs.AB_Consumable_PhysicalPowerPotion_T02_Buff))
            {
                Helper.BuffPlayer(sender, Prefabs.AB_Consumable_PhysicalPowerPotion_T02_Buff, out var buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(sender, Prefabs.AB_Consumable_SpellPowerPotion_T02_Buff, out buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(sender, Prefabs.AB_Consumable_SpellLeechPotion_T01_Buff, out buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(sender, Prefabs.AB_Consumable_FireResistancePotion_T01_Buff, out buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(sender, Prefabs.AB_Consumable_HolyResistancePotion_T02_Buff, out buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(sender, Prefabs.AB_Consumable_WranglerPotion_T01_Buff, out buffEntity, Helper.NO_DURATION, true);
                Helper.BuffPlayer(sender, Prefabs.AB_Consumable_SunResistancePotion_T01_Buff, out buffEntity, Helper.NO_DURATION, true);
            }
            else
            {
                Helper.RemoveBuff(sender, Prefabs.AB_Consumable_PhysicalPowerPotion_T02_Buff);
                Helper.RemoveBuff(sender, Prefabs.AB_Consumable_SpellPowerPotion_T02_Buff);
                Helper.RemoveBuff(sender, Prefabs.AB_Consumable_SpellLeechPotion_T01_Buff);
                Helper.RemoveBuff(sender, Prefabs.AB_Consumable_FireResistancePotion_T01_Buff);
                Helper.RemoveBuff(sender, Prefabs.AB_Consumable_HolyResistancePotion_T02_Buff);
                Helper.RemoveBuff(sender, Prefabs.AB_Consumable_WranglerPotion_T01_Buff);
                Helper.RemoveBuff(sender, Prefabs.AB_Consumable_SunResistancePotion_T01_Buff);
            }
        }

        [Command("heals", usage: ".heals", description: "Gives you healing potions", aliases: ["h"], adminOnly: false, includeInHelp: true, category: "Potions")]
        public static void HealsCommand(Player sender)
        {
            Helper.AddItemToInventory(sender, Prefabs.Item_Consumable_HealingPotion_T01, 1, out var itemEntity);
            Helper.AddItemToInventory(sender, Prefabs.Item_Consumable_HealingPotion_T02, 1, out itemEntity);
        }

        [Command(name: "r", description: "Reset cooldown and hp for the player.", usage: ".r", aliases: ["reset", "res", "r"], adminOnly: false, includeInHelp: true, category: "Misc")]
        public static void ResetCommand(Player sender, Player target = null)
        {
            if (target == null)
            {
                target = sender;
            }

            target.Reset(Helper.ResetOptions.Default);
            if (!target.IsAlive)
            {
                Helper.RevivePlayer(target);
            }

            sender.ReceiveMessage($"Player {target.Name.Emphasize()} reset.".Success());
        }

        [Command("golem", description: "Become a golem", adminOnly: false, category: "Misc")]
        public void GolemCommand(Player sender, Player target = null)
        {
            if (target == null)
            {
                target = sender;
            }
            Helper.BuffPlayer(target, Prefabs.AB_Shapeshift_Golem_T02_Buff, out var buffEntity, Helper.NO_DURATION);
        }


        [Command("placetiles", description: "Places tiles on current floor", adminOnly: false, category: "Misc")]
        public void PlaceTilesCommand(Player sender)
        {
            if (Helper.TryGetCurrentCastleTerritory(sender, out var territoryEntity))
            {
                var blockBuffer = territoryEntity.ReadBuffer<CastleTerritoryBlocks>();
                var heartPosition = territoryEntity.Read<CastleTerritory>().CastleHeart.Read<Translation>().Value;
                heartPosition.y = (float)System.Math.Floor(sender.Position.y) + 0.1f;
                
                Queue<float3> toPlace = new Queue<float3>();
                HashSet<float3> placedPositions = new HashSet<float3>();

                toPlace.Enqueue(heartPosition);
                placedPositions.Add(heartPosition);

                var directions = new float3[]
                {
                                new float3(5, 0, 0),
                                new float3(-5, 0, 0),
                                new float3(0, 0, 5),
                                new float3(0, 0, -5),
                };

                int placedCount = 0;

                while (toPlace.Count > 0 && placedCount < blockBuffer.Length * 5)
                {
                    var position = toPlace.Dequeue();
                    if (placedCount < blockBuffer.Length * 5)
                    {
                        var action = () =>
                        {
                            var buildEventEntity = Helper.CreateEntityWithComponents<FromCharacter, BuildTileModelEvent>();
                            buildEventEntity.Write(sender.ToFromCharacter());
                            buildEventEntity.Write(new BuildTileModelEvent
                            {
                                PrefabGuid = Prefabs.TM_Castle_Floor_Foundation_Stone04,
                                SpawnTranslation = new Translation
                                {
                                    Value = position
                                }
                            });
                        };
                        ActionScheduler.RunActionOnceAfterFrames(action, placedCount + 1);


                        placedCount++;

                        foreach (var direction in directions)
                        {
                            var newPosition = position + direction;

                            if (!placedPositions.Contains(newPosition))
                            {
                                toPlace.Enqueue(newPosition);
                                placedPositions.Add(newPosition);
                            }
                        }
                    }
                }
            }
           
        }

        [Command("raid", description: "Gives raid mats", adminOnly: false, includeInHelp: true, category: "Items")]
        public void RaidCommand(Player sender)
        {
            Helper.AddItemToInventory(sender, Prefabs.Item_Building_Siege_Golem_T02, 20, out var itemEntity);
            Helper.AddItemToInventory(sender, Prefabs.Item_Building_Explosives_T02, 50, out itemEntity);
        }

        [Command(name: "give", description: "Gives the specified item to the player", usage: ".give", aliases: ["g"], adminOnly: false, includeInHelp: true, category: "Items")]
        public static void GiveItem(Player sender, ItemPrefabData item, int quantity = 1, Player player = null)
        {
            Player Player = player ?? sender;

            if (Helper.AddItemToInventory(Player.Character, item.PrefabGUID, quantity, out var entity))
            {
                var itemName = item.PrefabGUID.LookupName();
                sender.ReceiveMessage($"Gave : {quantity.ToString().Emphasize()} {itemName.White()} to {Player.Name.Emphasize()}".Success());
            }
        }

        [Command("force-clan", description: "Forces a player to join the clan of another", usage: ".force-clan", aliases: ["fc", "forceclan", "force clan"], adminOnly: true, includeInHelp: true, category: "Misc")]
        public static void ForceClanCommand(Player sender, Player player1, Player player2)
        {
            Helper.AddPlayerToPlayerClanForce(player1, player2);
            sender.ReceiveMessage($"{player1.Name.Colorify(ExtendedColor.ClanNameColor)} has joined {player2.Name.Colorify(ExtendedColor.ClanNameColor)}'s clan".White());
        }

        [Command("unlock", description: "Used for debugging", adminOnly: true)]
        public void UnlockCommand(Player sender, Player target = null, bool unlockContent = true)
        {
            if (target == null)
            {
                target = sender;
            }
            Helper.Unlock(target, unlockContent);
        }

        [Command("ping", description: "Shows your latency", usage: ".ping", aliases: ["p"], adminOnly: false, includeInHelp: true, category: "Misc")]
        public static void PingCommand(Player sender, string mode = "")
        {
            var ping = (int)(sender.Character.Read<Latency>().Value * 1000);
            sender.ReceiveMessage($"Your latency is {ping.ToString().Emphasize()}ms.".White());
        }

        [Command("respawntombs", description: "Respawns tomb minions", usage: ".respawntombs", adminOnly: false, includeInHelp: true, category: "Misc")]
        public static void RespawnTombsCommand(Player sender, string mode = "")
        {
            if (Helper.IsInBase(sender, out var territoryEntity, out var territoryAlignment) && territoryAlignment == Helper.TerritoryAlignment.Friendly)
            {
                var entities = Helper.GetEntitiesByComponentTypes<UnitSpawnerstation>(EntityQueryOptions.IncludeDisabledEntities);

                var heart = territoryEntity.Read<CastleTerritory>().CastleHeart;
                if (!heart.Exists())
                {
                    sender.ReceiveMessage("This territory has no unit spawning stations!".Error());
                    return;
                }

                foreach (var entity in entities)
                {
                    if (entity.Read<CastleHeartConnection>().CastleHeartEntity._Entity == heart)
                    {
                        var buffer = entity.ReadBuffer<RefinementstationRecipesBuffer>();
                        for (var i = 0; i < buffer.Length; i++)
                        {
                            var recipe = buffer[i];
                            recipe.Unlocked = true;
                            recipe.Disabled = false;
                            buffer[i] = recipe;
                        }
                        var station = entity.Read<UnitSpawnerstation>();
                        var prefabGuid = entity.GetPrefabGUID();
                        if (prefabGuid == Prefabs.TM_UnitStation_Tomb)
                        {
                            Helper.AddItemToInventory(entity, Prefabs.Item_Ingredient_Plant_Lotus, 1000, out var itemEntity, false, 0, false);
                            Helper.AddItemToInventory(entity, Prefabs.Item_Ingredient_Gemdust, 1000, out itemEntity, false, 0, false);
                            station.Progress = 1200;
                            station.CurrentRecipeGuid = Prefabs.Recipe_UnitSpawn_Banshee;
                            station.IsWorking = true;
                            entity.Write(station);
                        }
                        else if (prefabGuid == Prefabs.TM_UnitStation_NetherGate)
                        {
                            Helper.AddItemToInventory(entity, Prefabs.Item_NetherShard_T02, 1000, out var itemEntity, false, 0, false);
                            Helper.AddItemToInventory(entity, Prefabs.Item_Ingredient_Gemdust, 1000, out itemEntity, false, 0, false);
                            station.Progress = 1200;
                            station.CurrentRecipeGuid = Prefabs.Recipe_UnitSpawn_NetherDemon_T02_Minerals;
                            station.IsWorking = true;
                            entity.Write(station);
                        }
                        else if (prefabGuid == Prefabs.TM_UnitStation_VerminNest) 
                        {
                            Helper.AddItemToInventory(entity, Prefabs.Item_Consumable_Heart_T03_Unsullied, 25, out var itemEntity, false, 0, false);
                            Helper.AddItemToInventory(entity, Prefabs.Item_Ingredient_Gravedust, 25, out itemEntity, false, 0, false);
                            station.Progress = 1200;
                            station.CurrentRecipeGuid = Prefabs.Recipe_UnitSpawn_PutridRat;
                            station.IsWorking = true;
                            entity.Write(station);
                        }
                    }
                }
            }
            else
            {
                sender.ReceiveMessage("You must be in your territory to use this command!".Error());
            }
        }

    }
}

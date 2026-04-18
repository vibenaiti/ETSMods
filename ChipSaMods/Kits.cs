using ModCore.Data;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChipSaMod
{
    public static class Kits
    {
        public static List<PrefabGUID> StartingGear = new List<PrefabGUID>()
        {
            Prefabs.Item_Boots_T09_Dracula_Brute,
            Prefabs.Item_Chest_T09_Dracula_Brute,
            Prefabs.Item_Gloves_T09_Dracula_Brute,
            Prefabs.Item_Legs_T09_Dracula_Brute,

            Prefabs.Item_Boots_T09_Dracula_Rogue,
            Prefabs.Item_Chest_T09_Dracula_Rogue,
            Prefabs.Item_Gloves_T09_Dracula_Rogue,
            Prefabs.Item_Legs_T09_Dracula_Rogue,

            Prefabs.Item_Boots_T09_Dracula_Scholar,
            Prefabs.Item_Chest_T09_Dracula_Scholar,
            Prefabs.Item_Gloves_T09_Dracula_Scholar,
            Prefabs.Item_Legs_T09_Dracula_Scholar,

            Prefabs.Item_Boots_T09_Dracula_Warrior,
            Prefabs.Item_Chest_T09_Dracula_Warrior,
            Prefabs.Item_Gloves_T09_Dracula_Warrior,
            Prefabs.Item_Legs_T09_Dracula_Warrior,

            Prefabs.Item_Cloak_Main_T03_Phantom,
        };

        public static List<PrefabGUID> StartingGearBrute = new List<PrefabGUID>
        {
            Prefabs.Item_Boots_T09_Dracula_Brute,
            Prefabs.Item_Chest_T09_Dracula_Brute,
            Prefabs.Item_Gloves_T09_Dracula_Brute,
            Prefabs.Item_Legs_T09_Dracula_Brute,
            Prefabs.Item_Cloak_Main_T03_Phantom,
        };

        public static List<PrefabGUID> StartingGearRogue = new List<PrefabGUID>
        {
            Prefabs.Item_Boots_T09_Dracula_Rogue,
            Prefabs.Item_Chest_T09_Dracula_Rogue,
            Prefabs.Item_Gloves_T09_Dracula_Rogue,
            Prefabs.Item_Legs_T09_Dracula_Rogue,
        };

        public static List<PrefabGUID> StartingGearScholar = new List<PrefabGUID>
        {
            Prefabs.Item_Boots_T09_Dracula_Scholar,
            Prefabs.Item_Chest_T09_Dracula_Scholar,
            Prefabs.Item_Gloves_T09_Dracula_Scholar,
            Prefabs.Item_Legs_T09_Dracula_Scholar,
        };

        public static List<PrefabGUID> StartingGearWarrior = new List<PrefabGUID>
        {
            Prefabs.Item_Boots_T09_Dracula_Warrior,
            Prefabs.Item_Chest_T09_Dracula_Warrior,
            Prefabs.Item_Gloves_T09_Dracula_Warrior,
            Prefabs.Item_Legs_T09_Dracula_Warrior,
        };

        public static List<PrefabGUID> Necks = new List<PrefabGUID>
        {
            Prefabs.Item_MagicSource_General_T08_Illusion,
            Prefabs.Item_MagicSource_General_T08_Frost,
            Prefabs.Item_MagicSource_General_T08_Storm,
            Prefabs.Item_MagicSource_General_T08_Blood,
            Prefabs.Item_MagicSource_General_T08_Unholy,
            Prefabs.Item_MagicSource_General_T08_Chaos,
        };

        public static List<PrefabGUID> ShardsNecks = new List<PrefabGUID>
        {
            Prefabs.Item_MagicSource_SoulShard_Dracula,
            Prefabs.Item_MagicSource_SoulShard_Solarus,
            Prefabs.Item_MagicSource_SoulShard_Manticore,
            Prefabs.Item_MagicSource_SoulShard_Monster,
        };

        public static List<PrefabGUID> SanguineWeapons = new List<PrefabGUID>
        {
            Prefabs.Item_Weapon_Slashers_T08_Sanguine,
            Prefabs.Item_Weapon_Spear_T08_Sanguine,
            Prefabs.Item_Weapon_Axe_T08_Sanguine,
            Prefabs.Item_Weapon_GreatSword_T08_Sanguine,
            Prefabs.Item_Weapon_Crossbow_T08_Sanguine,
            Prefabs.Item_Weapon_Pistols_T08_Sanguine,
            Prefabs.Item_Weapon_Reaper_T08_Sanguine,
            Prefabs.Item_Weapon_Sword_T08_Sanguine,
            Prefabs.Item_Weapon_Mace_T08_Sanguine,
        };


        public static List<PrefabGUID> StartingArtifactWeapons = new List<PrefabGUID>
        {
            Prefabs.Item_Weapon_Whip_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Pistols_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Axe_Unique_T08_Variation01,
        };


        public static List<PrefabGUID> ArtifactWeapons = new List<PrefabGUID>
        {
            Prefabs.Item_Weapon_Slashers_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Slashers_Unique_T08_Variation02,
            Prefabs.Item_Weapon_Spear_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Axe_Unique_T08_Variation01,
            Prefabs.Item_Weapon_GreatSword_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Crossbow_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Pistols_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Reaper_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Sword_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Mace_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Whip_Unique_T08_Variation01,
            Prefabs.Item_Weapon_Longbow_Unique_T08_Variation01,
        };
    }  
}

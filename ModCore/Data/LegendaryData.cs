using System.Collections.Generic;
using System.Linq;
using ProjectM;
using Stunlock.Core;

namespace ModCore.Data;

public static class LegendaryData
{
	public static readonly Dictionary<PrefabGUID, string> statModGuidToIndex = new Dictionary<PrefabGUID, string>();
	static LegendaryData()
	{
		statModGuidToIndex = statMods.Select((guid, index) => new { guid, index })
							 .ToDictionary(x => x.guid, x => (x.index + 1).ToString("X"));
	}
	public static readonly Dictionary<PrefabGUID, string> prefabToWeaponDictionary = new Dictionary<PrefabGUID, string>
	{
		{ Prefabs.Item_Weapon_Slashers_Legendary_T08, "slasher"  },
		{ Prefabs.Item_Weapon_Spear_Legendary_T08, "spear"  },
		{ Prefabs.Item_Weapon_Axe_Legendary_T08, "axe"  },
		{ Prefabs.Item_Weapon_GreatSword_Legendary_T08, "greatsword"  },
		{ Prefabs.Item_Weapon_Crossbow_Legendary_T08, "crossbow"  },
		{ Prefabs.Item_Weapon_Pistols_Legendary_T08, "pistol"  },
		{ Prefabs.Item_Weapon_Reaper_Legendary_T08, "reaper"  },
		{ Prefabs.Item_Weapon_Sword_Legendary_T08, "sword"  },
		{ Prefabs.Item_Weapon_Mace_Legendary_T08, "mace"  },
		{ Prefabs.Item_Weapon_Longbow_Legendary_T08, "longbow" },
		{ Prefabs.Item_Weapon_Whip_Legendary_T08, "whip" },
	};

	public static readonly Dictionary<string, PrefabGUID> weaponToPrefabDictionary = new Dictionary<string, PrefabGUID>
	{
		{ "slasher", Prefabs.Item_Weapon_Slashers_Legendary_T08 },
		{ "slashers", Prefabs.Item_Weapon_Slashers_Legendary_T08 },
		{ "spear", Prefabs.Item_Weapon_Spear_Legendary_T08 },
		{ "axe", Prefabs.Item_Weapon_Axe_Legendary_T08 },
		{ "axes", Prefabs.Item_Weapon_Axe_Legendary_T08 },
		{ "greatsword", Prefabs.Item_Weapon_GreatSword_Legendary_T08 },
		{ "crossbow", Prefabs.Item_Weapon_Crossbow_Legendary_T08 },
		{ "pistol", Prefabs.Item_Weapon_Pistols_Legendary_T08 },
		{ "pistols", Prefabs.Item_Weapon_Pistols_Legendary_T08 },
		{ "reaper", Prefabs.Item_Weapon_Reaper_Legendary_T08 },
		{ "sword", Prefabs.Item_Weapon_Sword_Legendary_T08 },
		{ "mace", Prefabs.Item_Weapon_Mace_Legendary_T08 },
		{ "longbow", Prefabs.Item_Weapon_Longbow_Legendary_T08 },
		{ "whip", Prefabs.Item_Weapon_Whip_Legendary_T08 },
	};

	public static readonly Dictionary<string, PrefabGUID> weaponToShatteredPrefabDictionary = new Dictionary<string, PrefabGUID>
	{
		{ "slasher", Prefabs.Item_Weapon_Slashers_Legendary_T08_Shattered },
		{ "slashers", Prefabs.Item_Weapon_Slashers_Legendary_T08_Shattered },
		{ "spear", Prefabs.Item_Weapon_Spear_Legendary_T08_Shattered },
		{ "axe", Prefabs.Item_Weapon_Axe_Legendary_T08_Shattered },
		{ "axes", Prefabs.Item_Weapon_Axe_Legendary_T08_Shattered },
		{ "greatsword", Prefabs.Item_Weapon_GreatSword_Legendary_T08_Shattered },
		{ "crossbow", Prefabs.Item_Weapon_Crossbow_Legendary_T08_Shattered },
		{ "pistol", Prefabs.Item_Weapon_Pistols_Legendary_T08_Shattered },
		{ "pistols", Prefabs.Item_Weapon_Pistols_Legendary_T08_Shattered },
		{ "reaper", Prefabs.Item_Weapon_Reaper_Legendary_T08_Shattered },
		{ "sword", Prefabs.Item_Weapon_Sword_Legendary_T08_Shattered },
		{ "mace", Prefabs.Item_Weapon_Mace_Legendary_T08_Shattered },
		{ "whip", Prefabs.Item_Weapon_Whip_Legendary_T08_Shattered },
		{ "longbow", Prefabs.Item_Weapon_Longbow_Legendary_T08_Shattered },
	};

	public static readonly Dictionary<string, List<PrefabGUID>> weaponToArtifactPrefabDictionary = new Dictionary<string, List<PrefabGUID>>
	{
		{ "slasher", new() { Prefabs.Item_Weapon_Slashers_Unique_T08_Variation01}},// Prefabs.Item_Weapon_Slashers_Unique_T08_Variation02 } },
		{ "slasher1", new() { Prefabs.Item_Weapon_Slashers_Unique_T08_Variation01}},
		{ "slasher2", new() { Prefabs.Item_Weapon_Slashers_Unique_T08_Variation02}},
		{ "slashers", new() { Prefabs.Item_Weapon_Slashers_Unique_T08_Variation01}},// Prefabs.Item_Weapon_Slashers_Unique_T08_Variation02 } },
		{ "slashers1", new() { Prefabs.Item_Weapon_Slashers_Unique_T08_Variation01}},
		{ "slashers2", new() { Prefabs.Item_Weapon_Slashers_Unique_T08_Variation02}},
		{ "spear", new() { Prefabs.Item_Weapon_Spear_Unique_T08_Variation01 } },
		{ "axe", new() { Prefabs.Item_Weapon_Axe_Unique_T08_Variation01 } },
		{ "axes", new() { Prefabs.Item_Weapon_Axe_Unique_T08_Variation01 } },
		{ "greatsword", new() { Prefabs.Item_Weapon_GreatSword_Unique_T08_Variation01 } },
		{ "crossbow", new() { Prefabs.Item_Weapon_Crossbow_Unique_T08_Variation01 } },
		{ "pistol", new() { Prefabs.Item_Weapon_Pistols_Unique_T08_Variation01 } },
		{ "pistols", new() { Prefabs.Item_Weapon_Pistols_Unique_T08_Variation01 } },
		{ "reaper", new() { Prefabs.Item_Weapon_Reaper_Unique_T08_Variation01 } },
		{ "sword", new() { Prefabs.Item_Weapon_Sword_Unique_T08_Variation01 } },
		{ "mace", new() { Prefabs.Item_Weapon_Mace_Unique_T08_Variation01 } },
		{ "longbow", new() { Prefabs.Item_Weapon_Longbow_Unique_T08_Variation01 } },
		{ "whip", new() { Prefabs.Item_Weapon_Whip_Unique_T08_Variation01 } },
	};

	public static readonly Dictionary<string, PrefabGUID> infusionToPrefabDictionary =
		new Dictionary<string, PrefabGUID>
		{
			{ "blood", Prefabs.SpellMod_Weapon_BloodInfused },
			{ "chaos", Prefabs.SpellMod_Weapon_ChaosInfused },
			{ "frost", Prefabs.SpellMod_Weapon_FrostInfused },
			{ "illusion", Prefabs.SpellMod_Weapon_IllusionInfused },
			{ "storm", Prefabs.SpellMod_Weapon_StormInfused },
			{ "unholy", Prefabs.SpellMod_Weapon_UndeadInfused },
			{ "leech", Prefabs.SpellMod_Weapon_BloodInfused },
			{ "ignite", Prefabs.SpellMod_Weapon_ChaosInfused },
			{ "chill", Prefabs.SpellMod_Weapon_FrostInfused },
			{ "weaken", Prefabs.SpellMod_Weapon_IllusionInfused },
			{ "static", Prefabs.SpellMod_Weapon_StormInfused },
			{ "condemn", Prefabs.SpellMod_Weapon_UndeadInfused },
		};

	public static readonly Dictionary<string, SchoolData> infusionToSchoolDictionary =
		new Dictionary<string, SchoolData>
		{
			{ "blood", SchoolData.Blood },
			{ "chaos", SchoolData.Chaos },
			{ "frost", SchoolData.Frost },
			{ "illusion", SchoolData.Illusion },
			{ "storm", SchoolData.Storm },
			{ "unholy", SchoolData.Unholy },
			{ "leech", SchoolData.Blood },
			{ "ignite", SchoolData.Chaos },
			{ "chill", SchoolData.Frost },
			{ "weaken", SchoolData.Illusion },
			{ "static", SchoolData.Storm },
			{ "condemn", SchoolData.Unholy },
		};

	public static Dictionary<PrefabGUID, string> prefabToInfusionDictionary = new Dictionary<PrefabGUID, string>
	{
			{ Prefabs.SpellMod_Weapon_BloodInfused, "blood"  },
			{ Prefabs.SpellMod_Weapon_ChaosInfused, "chaos"  },
			{ Prefabs.SpellMod_Weapon_FrostInfused, "frost"  },
			{ Prefabs.SpellMod_Weapon_IllusionInfused, "illusion"  },
			{ Prefabs.SpellMod_Weapon_StormInfused, "storm"  },
			{ Prefabs.SpellMod_Weapon_UndeadInfused, "unholy"  }
	};


	public static List<PrefabGUID> statMods = new List<PrefabGUID>()
	{
		Prefabs.StatMod_AttackSpeed,
		Prefabs.StatMod_DamageReduction,
		Prefabs.StatMod_MaxHealth,
		Prefabs.StatMod_MovementSpeed,
		Prefabs.StatMod_CriticalStrikePhysical,
		Prefabs.StatMod_CriticalStrikePhysicalPower,
		Prefabs.StatMod_Unique_WeaponCooldown_Mid,
		Prefabs.StatMod_CriticalStrikeSpells,
		Prefabs.StatMod_CriticalStrikeSpellPower,
		Prefabs.StatMod_Unique_SpellCooldown_Mid,
		Prefabs.StatMod_SpellPower,
		Prefabs.StatMod_SpellLeech,
		Prefabs.StatMod_ResourceYield,
	};

	public static List<string> statModDescriptions = new List<string>()
	{
		"Attack Speed",
		"Damage Reduction",
		"Max Health",
		"Movement Speed",
		"Physical Critical Strike Chance",
		"Physical Critical Strike Damage",
		"Weapon Skill Cooldown Reduction",
		"Spell Critical Strike Chance",
		"Spell Critical Strike Damage",
		"Spell Cooldown Reduction",
		"Spell Power",
		"Spell Life Leech",
		"Resource Yield",
	};
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using Stunlock.Core;
using UnityEngine;

namespace ModCore.Data;
public static class ShardData
{
	public static HashSet<PrefabGUID> ShardNecklaces = new()
	{
		Prefabs.Item_MagicSource_SoulShard_Dracula,
		Prefabs.Item_MagicSource_SoulShard_Manticore,
		Prefabs.Item_MagicSource_SoulShard_Monster,
		Prefabs.Item_MagicSource_SoulShard_Solarus,
	};

	public static HashSet<PrefabGUID> ShardMapIcons = new()
	{
		Prefabs.MapIcon_Relic_Standard_Dracula,
		Prefabs.MapIcon_Relic_Standard_WingedHorror,
		Prefabs.MapIcon_Relic_Standard_TheMonster,
		Prefabs.MapIcon_Relic_Standard_Solarus,
	};

	public static HashSet<PrefabGUID> ShardBuffs = new()
	{
		Prefabs.Item_EquipBuff_MagicSource_Soulshard_Dracula,
		Prefabs.Item_EquipBuff_MagicSource_Soulshard_Manticore,
		Prefabs.Item_EquipBuff_MagicSource_Soulshard_TheMonster,
		Prefabs.Item_EquipBuff_MagicSource_Soulshard_Solarus,
	};

	public static Dictionary<PrefabGUID, Color> ShardsToTextColor = new()
	{
		{ Prefabs.Item_MagicSource_SoulShard_Dracula, ExtendedColor.FireBrick },
		{ Prefabs.Item_MagicSource_SoulShard_Manticore, ExtendedColor.DarkViolet },
		{ Prefabs.Item_MagicSource_SoulShard_Monster, ExtendedColor.Yellow },
		{ Prefabs.Item_MagicSource_SoulShard_Solarus, ExtendedColor.Chartreuse },
	};

	public static Dictionary<PrefabGUID, Color> SpawnShardsToTextColor = new()
	{
		{ Prefabs.MapIcon_Relic_Spawn_Dracula, ExtendedColor.FireBrick },
		{ Prefabs.MapIcon_Relic_Spawn_WingedHorror, ExtendedColor.DarkViolet },
		{ Prefabs.MapIcon_Relic_Spawn_TheMonster, ExtendedColor.Yellow },
		{ Prefabs.MapIcon_Relic_Spawn_Solarus, ExtendedColor.Chartreuse },
	};
}


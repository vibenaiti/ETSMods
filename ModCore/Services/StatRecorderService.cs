using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProjectM;
using ModCore;
using ModCore.Data;
using ModCore.Factories;
using ModCore.Events;
using ModCore.Models;
using ModCore.Services;
using Unity.Entities;
using UnityEngine;
using static ModCore.Models.Player;
using Stunlock.Core;

public class DamageRecord
{
	public Player Player { get; set; }
	public DateTime DamageTime { get; set; }
}
public static class StatRecorderService
{
	public class DamageInfo
	{ 
		public float TotalDamage { get; set; }
		public float CritDamage { get; set; }
		public float DamageAbsorbed { get; set; }

		public static DamageInfo operator +(DamageInfo a, DamageInfo b)
		{
			return new DamageInfo
			{
				TotalDamage = a.TotalDamage + b.TotalDamage,
				CritDamage = a.CritDamage + b.CritDamage,
				DamageAbsorbed = a.DamageAbsorbed + b.DamageAbsorbed
			};
		}
	}

	public static void Initialize()
	{
		//GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		//GameEvents.OnPlayerHealthChanged += HandleOnPlayerHealthChanged;
		//GameEvents.OnUnitHealthChanged += HandleOnUnitHealthChanged;
	}
	public static void Dispose()
	{
		//GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		//GameEvents.OnPlayerHealthChanged -= HandleOnPlayerHealthChanged;
		//GameEvents.OnUnitHealthChanged -= HandleOnUnitHealthChanged;
	}

	private static Dictionary<Player, Dictionary<PrefabGUID, DamageInfo>> _playerDamageDealtAbilityBreakdown = new Dictionary<Player, Dictionary<PrefabGUID, DamageInfo>>();
	private static Dictionary<Player, Dictionary<PrefabGUID, DamageInfo>> _playerDamageReceivedAbilityBreakdown = new Dictionary<Player, Dictionary<PrefabGUID, DamageInfo>>();
	private static Dictionary<Player, int> _playerKills = new();
	private static Dictionary<Player, int> _playerDeaths = new();
	private static Dictionary<Player, float> _playerDamageDealt = new();
	private static Dictionary<Player, float> _playerDamageReceived = new();

	public static void RecordDamageDone(Player player, PrefabGUID ability, DamageInfo damageInfo)
	{
		if (!_playerDamageDealtAbilityBreakdown.ContainsKey(player))
		{
			_playerDamageDealtAbilityBreakdown[player] = new Dictionary<PrefabGUID, DamageInfo>();
		}

		if (!_playerDamageDealtAbilityBreakdown[player].ContainsKey(ability))
		{
			_playerDamageDealtAbilityBreakdown[player][ability] = new DamageInfo();
		}

		_playerDamageDealtAbilityBreakdown[player][ability] += damageInfo;
	}

	public static void RecordDamageReceived(Player player, PrefabGUID ability, DamageInfo damageInfo)
	{
		if (!_playerDamageReceivedAbilityBreakdown.ContainsKey(player))
		{
			_playerDamageReceivedAbilityBreakdown[player] = new Dictionary<PrefabGUID, DamageInfo>();
		}

		if (!_playerDamageReceivedAbilityBreakdown[player].ContainsKey(ability))
		{
			_playerDamageReceivedAbilityBreakdown[player][ability] = new DamageInfo();
		}

		_playerDamageReceivedAbilityBreakdown[player][ability] += damageInfo;
	}

	public static void ReportDamageResults(Player player)
	{
		if (_playerDamageDealtAbilityBreakdown.TryGetValue(player, out var abilityDamage))
		{
			float totalDamage = abilityDamage.Sum(kvp => kvp.Value.TotalDamage);
			var groupedDamage = GroupRelatedAbilities(abilityDamage);
			var sortedGroupedDamage = groupedDamage.OrderByDescending(kvp => kvp.Value.Sum(val => val.Value.TotalDamage));

			foreach (var group in sortedGroupedDamage)
			{
				float groupTotalDamage = group.Value.Sum(val => val.Value.TotalDamage);
				float percentage = (float)Math.Round((groupTotalDamage / totalDamage) * 100);

				// Get the group name and color
				Color32 groupColor = ExtendedColor.ServerColor; // Default color

				if (AbilityData.AbilityNameToColor.TryGetValue(group.Key, out var color))
				{
					groupColor = color;
				}

				float roundedGroupTotalDamage = (float)Math.Round(groupTotalDamage);

				if (roundedGroupTotalDamage > 0)
				{
					player.ReceiveMessage($"{group.Key.Colorify(groupColor)} - {roundedGroupTotalDamage} ({percentage}%)".White());
				}
			}

			_playerDamageDealtAbilityBreakdown[player].Clear();
		}
		else
		{
			player.ReceiveMessage("There is no damage to report".White());
		}
	}

	public static float GetTotalDamageDealt(Player player)
	{
		if (_playerDamageDealt.TryGetValue(player, out var damageDealt))
		{
			return damageDealt;
		}
		return 0;
	}

	public static void ClearRecord(Player player)
	{
		if (_playerDamageDealtAbilityBreakdown.ContainsKey(player))
		{
			_playerDamageDealtAbilityBreakdown[player].Clear();
		}
		if (_playerDamageReceivedAbilityBreakdown.ContainsKey(player))
		{
			_playerDamageReceivedAbilityBreakdown[player].Clear();
		}
		if (_playerDeaths.ContainsKey(player))
		{
			_playerDeaths[player] = 0;
		}
		if (_playerKills.ContainsKey(player))
		{
			_playerKills[player] = 0;
		}
		if (_playerDamageDealt.ContainsKey(player))
		{
			_playerDamageDealt[player] = 0;
		}
		if (_playerDamageReceived.ContainsKey(player))
		{
			_playerDamageReceived[player] = 0;
		}
	}

	private static Dictionary<string, Dictionary<PrefabGUID, DamageInfo>> GroupRelatedAbilities(Dictionary<PrefabGUID, DamageInfo> abilityDamage)
	{
		var groupedAbilities = new Dictionary<string, Dictionary<PrefabGUID, DamageInfo>>();
		foreach (var kvp in abilityDamage)
		{
			string groupName = GetGroupName(kvp.Key);
			if (!groupedAbilities.ContainsKey(groupName))
			{
				groupedAbilities[groupName] = new Dictionary<PrefabGUID, DamageInfo>();
			}
			groupedAbilities[groupName].Add(kvp.Key, kvp.Value);
		}
		return groupedAbilities;
	}

	private static string GetGroupName(PrefabGUID prefabGUID)
	{
		if (AbilityData.AbilityPrefabToName.TryGetValue(prefabGUID, out var name))
		{
			return name;
		}
		name = prefabGUID.LookupName();
		return name;
	}

	private static void HandleOnPlayerHealthChanged(Entity source, Entity eventEntity, Entity target, PrefabGUID ability)
	{
		if (!target.Has<PlayerCharacter>() || !source.Has<PlayerCharacter>()) return;

		if (ability == Prefabs.SunDamageDebuff) return;
		if (source == target) return;

		var sourcePlayer = PlayerService.GetPlayerFromCharacter(source);
		var targetPlayer = PlayerService.GetPlayerFromCharacter(target);
		
		var statChangeEvent = eventEntity.Read<StatChangeEvent>();
		var totalDamage = statChangeEvent.Change;
		float damageShielded = 0;
		float critDamage = 0;
		if (Math.Abs(statChangeEvent.OriginalChange) > Math.Abs(statChangeEvent.Change))
		{
			totalDamage = statChangeEvent.OriginalChange; //include shielded damage
			damageShielded = Math.Abs(statChangeEvent.OriginalChange) - Math.Abs(statChangeEvent.Change);
		}
		else if ((statChangeEvent.StatChangeFlags & (int)StatChangeFlag.IsCritical) != 0)
		{
			critDamage = Math.Abs(statChangeEvent.Change) - Math.Abs(statChangeEvent.OriginalChange);
		}
		if (totalDamage > 0) return;
		
		var damageInfo = new DamageInfo
		{
			TotalDamage = Math.Abs(totalDamage),
			CritDamage = Math.Abs(critDamage),
			DamageAbsorbed = Math.Abs(damageShielded)
		};
		RecordDamageDone(sourcePlayer, ability, damageInfo);
		if (!_playerDamageDealt.ContainsKey(sourcePlayer))
		{
			_playerDamageDealt[sourcePlayer] = 0;
		}
		if (!_playerDamageReceived.ContainsKey(targetPlayer))
		{
			_playerDamageReceived[targetPlayer] = 0;
		}
		
		_playerDamageDealt[sourcePlayer] += damageInfo.TotalDamage;
		_playerDamageReceived[targetPlayer] += damageInfo.TotalDamage;
	}

	private static void HandleOnUnitHealthChanged(Entity source, Entity eventEntity, Entity target, PrefabGUID ability)
	{
		if (!source.Has<PlayerCharacter>() || UnitFactory.HasCategory(target, "dummy")) return;

		var sourcePlayer = PlayerService.GetPlayerFromCharacter(source);
		var statChangeEvent = eventEntity.Read<StatChangeEvent>();
		var totalDamage = statChangeEvent.Change;
		float damageShielded = 0;
		float critDamage = 0;
		if (Math.Abs(statChangeEvent.OriginalChange) > Math.Abs(statChangeEvent.Change))
		{
			totalDamage = statChangeEvent.OriginalChange; //include shielded damage
			damageShielded = Math.Abs(statChangeEvent.OriginalChange) - Math.Abs(statChangeEvent.Change);
		}
		else if ((statChangeEvent.StatChangeFlags & (int)StatChangeFlag.IsCritical) != 0)
		{
			critDamage = Math.Abs(statChangeEvent.Change) - Math.Abs(statChangeEvent.OriginalChange);
		}
		if (totalDamage > 0) return;
		var damageInfo = new DamageInfo
		{
			TotalDamage = Math.Abs(totalDamage),
			CritDamage = Math.Abs(critDamage),
			DamageAbsorbed = Math.Abs(damageShielded)
		};
		RecordDamageDone(sourcePlayer, ability, damageInfo);
	}

	private static void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (killer.Exists())
		{
			if (killer.Has<PlayerCharacter>())
			{
				var killerPlayer = PlayerService.GetPlayerFromCharacter(killer);

				if (killer != player.Character)
				{
					if (_playerKills.ContainsKey(killerPlayer))
					{
						_playerKills[killerPlayer]++;
					}
					else
					{
						_playerKills[killerPlayer] = 1;
					}
				}
			}
		}

		if (_playerDeaths.ContainsKey(player))
		{
			_playerDeaths[player]++;
		}
		else
		{
			_playerDeaths[player] = 1;
		}
	}
}

public class DamageEvent
{
	public PrefabGUID Ability;
	public PrefabGUID DamageType;
	public float DamageAmount;
}

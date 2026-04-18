using ProjectM;
using Unity.Entities;
using System.Collections.Generic;
using ModCore.Data;
using ModCore.Events;
using ModCore.Helpers;
using ModCore.Models;
using Stunlock.Core;

namespace ModCore.Services;


public static class PlayerSpawnHandler
{
	public static Dictionary<Player, bool> PlayerFirstTimeSpawn = new Dictionary<Player, bool>();
	public static Dictionary<Player, bool> PlayerIsDead = new Dictionary<Player, bool>();

	public static void Initialize()
	{
		GameEvents.OnPlayerBuffRemoved += HandleOnPlayerUnbuffed;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
	}

	public static void Dispose()
	{
		GameEvents.OnPlayerBuffRemoved -= HandleOnPlayerUnbuffed;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
	}

	private static void HandleOnPlayerUnbuffed(Player player, Entity buffEntity, PrefabGUID prefabGUID)
	{
		if (prefabGUID == Prefabs.HideCharacterBuff)
		{
			if (PlayerFirstTimeSpawn.TryGetValue(player, out var firstTimeSpawn) && firstTimeSpawn)
			{
				HandleOnFirstSpawn(player);
				PlayerFirstTimeSpawn.Remove(player);
			}
			else if (PlayerIsDead.TryGetValue(player, out var playerIsDead) && playerIsDead)
			{
				GameEvents.OnPlayerRespawn?.Invoke(player);
				PlayerIsDead[player] = false;
			}
		}
	}

	private static void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
	{
		PlayerIsDead[player] = true;
	}

	private static void HandleOnFirstSpawn(Player player)
	{
		GameEvents.OnPlayerFirstSpawn?.Invoke(player);
		PlayerService.OnlinePlayersWithCharacters.Add(player);
	}
}


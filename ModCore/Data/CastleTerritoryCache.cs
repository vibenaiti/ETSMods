using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Tiles;
using Unity.Entities;
using Unity.Mathematics;
using ModCore.Helpers;
using ModCore.Models;
using Unity.Transforms;

namespace ModCore.Data;
public static class CastleTerritoryCache
{
	private static Dictionary<int2, Entity> BlockTileToTerritory = new();
	private static int TileToBlockDivisor = 10;
	public static void Initialize()
	{
		var entities = Helper.GetEntitiesByComponentTypes<CastleTerritoryBlocks>();
		foreach (var entity in entities)
		{
			var buffer = entity.ReadBuffer<CastleTerritoryBlocks>();
			foreach (var block in buffer)
			{
				BlockTileToTerritory[block.BlockCoordinate] = entity;
			}
		}
	}
	public static bool TryGetCastleTerritory(Player player, out Entity territoryEntity)
	{
		return TryGetCastleTerritory(player.Character, out territoryEntity);
	}

	public static bool TryGetCastleTerritory(Entity entity, out Entity territoryEntity)
	{
		if (entity.Has<Translation>())
		{
			var position = entity.Read<Translation>().Value;
			var tilePosition = Helper.GetTilePositionFromWorldPosition(position);
			return BlockTileToTerritory.TryGetValue(tilePosition / TileToBlockDivisor, out territoryEntity);
		}
		territoryEntity = default;
		return false;
	}

	public static bool TryGetCastleTerritory(ComponentLookup<TilePosition> tilePositionLookup, Entity entity, out Entity territoryEntity)
	{
		if (tilePositionLookup.HasComponent(entity))
		{
			var tilePosition = tilePositionLookup[entity];
			return BlockTileToTerritory.TryGetValue(tilePosition.Tile / TileToBlockDivisor, out territoryEntity);
		}

		territoryEntity = default;
		return false;
	}
}

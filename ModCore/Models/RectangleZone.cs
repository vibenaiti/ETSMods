using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModCore.Helpers;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ModCore.Models;
public class RectangleZone
{
	public float Left { get; set; }
	public float Top { get; set; }
	public float Right { get; set; }
	public float Bottom { get; set; }
	public int Height { get; set; } = -1;
	const int TileSize = 5;
	
	public static RectangleZone FromEntity(Entity entity, int x, int y)
	{
		var tileBounds = entity.Read<TileBounds>();
		var position = entity.Read<LocalToWorld>().Position;

		float halfWidth = x / 2;
		float halfHeight = y / 2;

		float left = position.x - halfWidth;
		float right = position.x + halfWidth;
		float top = position.z + halfHeight;
		float bottom = position.z - halfHeight;


		return new RectangleZone(left, top, right, bottom);
	}

	public RectangleZone(float left, float top, float right, float bottom, int height = -1)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
		Height = height;
	}

	public bool Contains(Player player)
	{
		return Contains(player.Character);
	}

	public List<Player> GetPlayersInsideZone(List<Player> players)
	{
		List<Player> results = new();
		var entityToTranslation = VWorld.Server.EntityManager.GetComponentLookup<Translation>();
		var entityToTilePosition = VWorld.Server.EntityManager.GetComponentLookup<TilePosition>();
		foreach (var player in players)
		{
			var translation = entityToTranslation[player.Character].Value;
			var tilePosition = entityToTilePosition[player.Character];
			if (Height != -1)
			{
				if (tilePosition.HeightLevel != Height) continue;
			}
			if (translation.x >= Left && translation.x <= Right && translation.z >= Bottom && translation.z <= Top)
			{
				results.Add(player);
			}
		}
		return results;
	}

	public bool Contains(float3 position)
	{
		var x = position.x;
		var z = position.z;
		return x >= Left && x <= Right && z >= Bottom && z <= Top;
	}

	public bool Contains(Entity entity, bool enforceHeight = true)
	{
		if (entity.Has<LocalToWorld>())
		{
			var localToWorld = entity.Read<LocalToWorld>();
			var x = localToWorld.Position.x;
			var z = localToWorld.Position.z;
			if (Height != -1)
			{
				if (entity.Has<TilePosition>() && entity.Read<TilePosition>().HeightLevel != Height) return false;
			}
			return x >= Left && x <= Right && z >= Bottom && z <= Top;
		}
		return false;
	}


	// Overloaded Contains method for Player with an expansion parameter
	public bool Contains(Player player, float expansion)
	{
		return Contains(player.Position, expansion);
	}

	// Overloaded Contains method for float3 position with an expansion parameter
	public bool Contains(float3 position, float expansion)
	{
		var x = position.x;
		var z = position.z;
		return x >= Left - expansion && x <= Right + expansion && z >= Bottom - expansion && z <= Top + expansion;
	}

	// Overloaded Contains method for Entity with an expansion parameter
	public bool Contains(Entity entity, float expansion)
	{
		if (entity.Has<LocalToWorld>())
		{
			var localToWorld = entity.Read<LocalToWorld>();
			return Contains(localToWorld.Position, expansion);
		}
		return false;
	}

	public float2 GetCenter()
	{
		var x = (Left + Right) / 2;
		var z = (Top + Bottom) / 2;
		return new float2(x, z);
	}


	//assumes you are facing north and that are you are standing in the bottom-left square
	public static RectangleZone GetZoneByCurrentCoordinates(Player player, int tilesRight, int tilesUp)
	{
		// Assuming player.Position.x and player.Position.z give the X and Z coordinates
		var playerX = player.Position.x;
		var playerZ = player.Position.z;

		// Calculate bottom-left corner
		// Floor to the nearest multiple of 5 (tile size)
		float bottomLeftX = (float)Math.Floor(playerX / TileSize) * TileSize;
		float bottomLeftZ = (float)Math.Floor(playerZ / TileSize) * TileSize;

		// Calculate top-right corner
		float topRightX = bottomLeftX + tilesRight * 5; // Adjust by tile count * tile size
		float topRightZ = bottomLeftZ + tilesUp * 5; // Adjust by tile count * tile size

		// Create a new RectangleZone
		return new RectangleZone(bottomLeftX, topRightZ, topRightX, bottomLeftZ, player.Character.Read<TilePosition>().HeightLevel);
	}

	public static RectangleZone FromCenterWithDimensions(float3 center1, float3 center2, float xDimension, float yDimension)
	{
		// Calculate the midpoint between center1 and center2
		float centerX = (center1.x + center2.x) / 2;
		float centerY = (center1.y + center2.y) / 2; // Assuming you meant the y component to be height/altitude
		float centerZ = (center1.z + center2.z) / 2;

		// Calculate half dimensions
		float halfXDimension = xDimension / 2;
		float halfYDimension = yDimension / 2; // Assuming yDimension is depth, which correlates to z in a 3D space

		// Calculate the corners of the rectangle based on the dimensions
		float left = centerX - halfXDimension;
		float right = centerX + halfXDimension;
		float top = centerZ + halfYDimension; // Use centerZ for calculating top and bottom, assuming z is up/down
		float bottom = centerZ - halfYDimension;

		// Create and return the RectangleZone
		return new RectangleZone(left, top, right, bottom);
	}

	public BoundsMinMax ToBoundsMinMax()
	{
		int2 min = new int2((int)Left, (int)Bottom);
		int2 max = new int2((int)Right, (int)Top);

		var tileMin = Helper.GetTilePositionFromWorldPosition(min);
		var tileMax = Helper.GetTilePositionFromWorldPosition(max);
		return new BoundsMinMax(tileMin, tileMax);
	}

	public override string ToString()
	{
		return $"\"Left\": {Left:F2},\n\"Top\": {Top:F2},\n\"Right\": {Right:F2},\n\"Bottom\": {Bottom:F2},\n\"Height\": {Height}";
	}
}


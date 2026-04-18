using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectM.Gameplay.Clan;
using static ProjectM.Network.ClanEvents_Client;
using ModCore.Services;
using ModCore.Models;
using ModCore.Data;
using ModCore.Configs;
using Il2CppSystem;
using Unity.Jobs;
using UnityEngine.Jobs;
using ProjectM.Tiles;
using Stunlock.Core;
using Unity.Entities.UniversalDelegates;
using static ProjectM.Metrics;
using ProjectM.Behaviours;

namespace ModCore.Helpers;

public static partial class Helper
{
	public static Entity CreateEntityWithComponents<T1>()
	{
		return VWorld.Server.EntityManager.CreateEntity(
			new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite)
		);
	}

	public static Entity CreateEntityWithComponents<T1, T2>()
	{
		return VWorld.Server.EntityManager.CreateEntity(
			new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite)
		);
	}

	public static Entity CreateEntityWithComponents<T1, T2, T3>()
	{
		return VWorld.Server.EntityManager.CreateEntity(
			new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite)
		);
	}

	public static Entity CreateEntityWithComponents<T1, T2, T3, T4>()
	{
		return VWorld.Server.EntityManager.CreateEntity(
			new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T4>(), ComponentType.AccessMode.ReadWrite)
		);
	}

	public static Entity CreateEntityWithComponents<T1, T2, T3, T4, T5>()
	{
		return VWorld.Server.EntityManager.CreateEntity(
			new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T4>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T5>(), ComponentType.AccessMode.ReadWrite)
		);
	}

	public static bool TryGetPrefabEntityByPrefabGUID(PrefabGUID prefabGUID, out Entity prefabEntity)
	{
		return Core.prefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(prefabGUID, out prefabEntity);
	}

	public static Entity GetPrefabEntityByPrefabGUID(PrefabGUID prefabGUID)
	{
		return Core.prefabCollectionSystem._PrefabGuidToEntityMap[prefabGUID];
	}

	public static Entity GetHoveredTileModel(Player player)
	{
		return GetHoveredTileModel(player.User);

	}
	public static Entity GetHoveredTileModel(Entity User)
	{
		var input = User.Read<EntityInput>();
		var position = input.AimPosition;
		var aimTilePosition = Helper.GetTilePositionFromWorldPosition(position);
		if (input.HoveredEntity.Exists())
		{
			return input.HoveredEntity;
		}
		else
		{
			var area = new BoundsMinMax
			{
				Min = new int2((int)aimTilePosition.x - 1, (int)aimTilePosition.y - 1),
				Max = new int2((int)aimTilePosition.x + 1, (int)aimTilePosition.y + 1)
			};
			var entities = Helper.GetEntitiesInArea(area, TileType.All);
			if (entities.Length > 0)
			{
				SortEntitiesByDistance(entities, position);
				return entities[0];
			}
		}
		return Entity.Null;
	}

	public static Entity GetHoveredTileModel<T>(Entity User)
	{
		var input = User.Read<EntityInput>();
		var position = input.AimPosition;
		var aimTilePosition = Helper.GetTilePositionFromWorldPosition(position);
		if (input.HoveredEntity.Exists() && input.HoveredEntity.Has<T>())
		{
			return input.HoveredEntity;
		}
		else
		{
			var area = new BoundsMinMax
			{
				Min = new int2((int)aimTilePosition.x - 1, (int)aimTilePosition.y - 1),
				Max = new int2((int)aimTilePosition.x + 1, (int)aimTilePosition.y + 1)
			};
			var entities = Helper.GetEntitiesInArea(area, TileType.All);
			if (entities.Length > 0)
			{
				SortEntitiesByDistance(entities, position);
				foreach (var entity in entities)
				{
					if (entity.Has<T>())
					{
						return entity;
					}
				}
			}
		}
		return Entity.Null;
	}

	public static Entity GetHoveredEntity(Entity User)
	{
		var input = User.Read<EntityInput>();
		var position = input.AimPosition;
		if (input.HoveredEntity.Exists())
		{
			return input.HoveredEntity;
		}
		else
		{
			var entities = GetEntitiesByComponentTypes<PrefabGUID, Translation>();
			if (entities.Length > 0)
			{
				SortEntitiesByDistance(entities, position);
				foreach (var entity in entities)
				{
					if (entity.GetPrefabGUID() != Prefabs.Block_Ground)
					{
						return entity;
					}
				}
			}
		}
		return Entity.Null;
	}

	public static Entity GetHoveredEntity<T>(Entity User)
	{
		var input = User.Read<EntityInput>();
		var position = input.AimPosition;
		if (input.HoveredEntity.Exists())
		{
			return input.HoveredEntity;
		}
		else
		{
			var entities = GetEntitiesByComponentTypes<PrefabGUID, T>();
			if (entities.Length > 0)
			{
				SortEntitiesByDistance(entities, position);
				foreach (var entity in entities)
				{
					if (entity.Has<T>())
					{
						return entity;
					}
				}
			}
		}
		return Entity.Null;
	}

	public static Entity GetHoveredEntity<T, T2>(Entity User)
	{
		var input = User.Read<EntityInput>();
		var position = input.AimPosition;
		if (input.HoveredEntity.Exists())
		{
			return input.HoveredEntity;
		}
		else
		{
			var entities = GetEntitiesByComponentTypes<PrefabGUID, T, T2>();
			if (entities.Length > 0)
			{
				SortEntitiesByDistance(entities, position);
				foreach (var entity in entities)
				{
					if (entity.Has<T>() && entity.Has<T2>())
					{
						return entity;
					}
				}
			}
		}
		return Entity.Null;
	}

	public static List<Entity> GetEntitiesNearPosition(Player player, int amount = 5)
	{
		List<Entity> entityList = new List<Entity>();
		var input = player.User.Read<EntityInput>();
		var position = input.AimPosition;
		var aimTilePosition = Helper.GetTilePositionFromWorldPosition(position);
		var area = new BoundsMinMax
		{
			Min = new int2((int)aimTilePosition.x - 10, (int)aimTilePosition.y - 10),
			Max = new int2((int)aimTilePosition.x + 10, (int)aimTilePosition.y + 10)
		};
		var entities = Helper.GetEntitiesInArea(area, TileType.All);
		if (entities.Length > 0)
		{
			SortEntitiesByDistance(entities, position);
		}
		for (var i = 0; i < amount && i < entities.Length; i++)
		{
			entityList.Add(entities[i]);
		}
		return entityList;
	}

	public static List<Entity> GetEntitiesByPrefabGuid(PrefabGUID prefabGUID)
	{
		// Get all component types of the given entity
		var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(prefabGUID);
		var componentTypes = VWorld.Server.EntityManager.GetComponentTypes(prefabEntity);

		// Create an entity query based on these component types
		var queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[componentTypes.Length]
		};

		for (int i = 0; i < componentTypes.Length; i++)
		{
			queryDesc.All[i] = componentTypes[i];
		}

		var entityQuery = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);

		// Query all entities matching the prefab's components
		var entities = entityQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
		List<Entity> results = new List<Entity>();
		foreach (var entity in entities)
		{
			if (entity.GetPrefabGUID() == prefabGUID)
			{
				results.Add(entity);
			}
		}
		return results;
	}

	public static List<Entity> GetHoveredEntitiesByPrefabGUID(Player player, PrefabGUID prefabGUID)
	{
		var input = player.User.Read<EntityInput>();
		var position = input.AimPosition;

		var entities = GetEntitiesByPrefabGuid(prefabGUID);
		if (entities.Count > 0)
		{
			SortEntitiesByDistance(entities, position);
		}
		return entities;
	}

	public static List<Entity> GetHoveredEntities(Entity user, int amount)
	{
		var input = user.Read<EntityInput>();
		var position = input.AimPosition;
		List<Entity> entityList = new List<Entity>();

		var entities = GetEntitiesByComponentTypes<PrefabGUID>();
		if (entities.Length > 0)
		{
			SortEntitiesByDistance(entities, position);
		}
		for (var i = 0; i < amount && i < entities.Length; i++)
		{
			entityList.Add(entities[i]);
		}
		return entityList;
	}

	public static List<Entity> SortEntitiesByDistance(List<Entity> entities, float3 position)
	{
		// Create a temporary array to hold entities and their distances
		(Entity entity, float distance)[] tempArray = new (Entity, float)[entities.Count];

		// Populate the temporary array
		for (int i = 0; i < entities.Count; i++)
		{
			float distance = float.MaxValue;
			if (entities[i].Has<LocalToWorld>())
			{
				LocalToWorld ltw = entities[i].Read<LocalToWorld>();
				distance = math.distance(position, ltw.Position);
			}

			tempArray[i] = (entities[i], distance);
		}

		// Sort the temporary array based on distance
		System.Array.Sort(tempArray, (a, b) => a.distance.CompareTo(b.distance));

		// Extract the sorted entities back into the NativeArray
		for (int i = 0; i < entities.Count; i++)
		{
			entities[i] = tempArray[i].entity;
		}

		return entities;
	}

	public static NativeArray<Entity> SortEntitiesByDistance(NativeArray<Entity> entities, float3 position)
	{
		// Create a temporary array to hold entities and their distances
		(Entity entity, float distance)[] tempArray = new (Entity, float)[entities.Length];

		// Populate the temporary array
		for (int i = 0; i < entities.Length; i++)
		{
			float distance = float.MaxValue;
			if (entities[i].Has<LocalToWorld>())
			{
				LocalToWorld ltw = entities[i].Read<LocalToWorld>();
				distance = math.distance(position, ltw.Position);
			}

			tempArray[i] = (entities[i], distance);
		}

		// Sort the temporary array based on distance
		System.Array.Sort(tempArray, (a, b) => a.distance.CompareTo(b.distance));

		// Extract the sorted entities back into the NativeArray
		for (int i = 0; i < entities.Length; i++)
		{
			entities[i] = tempArray[i].entity;
		}

		return entities;
	}

	public static NativeArray<Entity> GetAllPrefabEntities()
	{
		EntityQueryOptions options = EntityQueryOptions.IncludePrefab;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<Prefab>(), ComponentType.AccessMode.ReadWrite),
			},
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
		var entities = query.ToEntityArray(Allocator.Temp);
		query.Dispose();
		return entities;
	}

	public static NativeArray<Entity> GetPrefabEntitiesByComponentTypes<T1>()
	{
		EntityQueryOptions options = EntityQueryOptions.IncludePrefab;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<Prefab>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
		var entities = query.ToEntityArray(Allocator.Temp);
		query.Dispose();
		return entities;
	}

	public static NativeArray<Entity> GetPrefabEntitiesByComponentTypes<T1, T2>()
	{
		EntityQueryOptions options = EntityQueryOptions.IncludePrefab;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<Prefab>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
		var entities = query.ToEntityArray(Allocator.Temp);
		query.Dispose();
		return entities;
	}

	public static NativeArray<Entity> GetPrefabEntitiesByComponentTypes<T1, T2, T3>()
	{
		EntityQueryOptions options = EntityQueryOptions.IncludePrefab;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<Prefab>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		query.Dispose();
		return entities;
	}

	public static NativeArray<Entity> GetPrefabEntitiesByComponentTypes<T1, T2, T3, T4>()
	{
		EntityQueryOptions options = EntityQueryOptions.IncludePrefab;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<Prefab>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T4>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		query.Dispose();
		return entities;
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1>(EntityQueryOptions queryOptions = default)
	{
		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[] { new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite) },
			Options = queryOptions
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
		var entities = query.ToEntityArray(Allocator.Temp);
		query.Dispose();
		return entities;
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(EntityQueryOptions queryOptions = default)
	{
		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = queryOptions
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		query.Dispose();
		return entities;
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2, T3>(EntityQueryOptions queryOptions = default)
	{
		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = queryOptions
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
		var entities = query.ToEntityArray(Allocator.Temp);
		query.Dispose();
		return entities;
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2, T3, T4>(EntityQueryOptions queryOptions = default)
	{
		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T4>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = queryOptions
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
		var entities = query.ToEntityArray(Allocator.Temp);
		query.Dispose();
		return entities;
	}

	public static NativeArray<Entity> GetNonPlayerSpawnedEntities(bool includeDisabled = false)
	{
		EntityQueryOptions options = includeDisabled ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[] { new ComponentType(Il2CppType.Of<CanFly>(), ComponentType.AccessMode.ReadWrite) },
			None = new ComponentType[] { new ComponentType(Il2CppType.Of<PlayerCharacter>(), ComponentType.AccessMode.ReadWrite) },
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
		var entities = query.ToEntityArray(Allocator.Temp);
		query.Dispose();
		return entities;
	}

	public unsafe static NativeArray<Entity> GetEntitiesInArea(BoundsMinMax area, TileType tileType)
	{
		var systemState = *Core.tileModelSpatialLookupSystemEntity.Read<SystemInstance>().state;
		var tileModelSpatialLookupSystemData = TileModelSpatialLookupSystemData.Create(ref systemState);
		var spatialLookup = tileModelSpatialLookupSystemData.GetSpatialLookupAndComplete(ref systemState);
		spatialLookup.GetEntities(ref area, tileType);
		return spatialLookup.Results.AsArray(); // Results is NativeList<Entity> in v1.1.11+
	}
}

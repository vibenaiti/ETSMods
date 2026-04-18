using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ModCore;

#pragma warning disable CS8500
public static class ECSExtensions
{
	private static Dictionary<Type, ComponentType> componentTypeCache = new Dictionary<Type, ComponentType>();

	public unsafe static void Write<T>(this Entity entity, T componentData) where T : struct
	{
		VWorld.Server.EntityManager.SetComponentData<T>(entity, componentData);
	}

	public unsafe static T Read<T>(this Entity entity) where T : struct
	{
		return VWorld.Server.EntityManager.GetComponentData<T>(entity);
	}

	public static DynamicBuffer<T> ReadBuffer<T>(this Entity entity) where T : struct
	{
		return VWorld.Server.EntityManager.GetBuffer<T>(entity);
	}

	public static DynamicBuffer<T> AddBuffer<T>(this Entity entity) where T : struct
	{
		return VWorld.Server.EntityManager.AddBuffer<T>(entity);
	}

	public static void Add<T>(this Entity entity)
	{
		VWorld.Server.EntityManager.AddComponent<T>(entity);
	}

	public static void Remove<T>(this Entity entity)
	{
		VWorld.Server.EntityManager.RemoveComponent<T>(entity);
	}

	public static bool Has<T>(this Entity entity)
	{
		return VWorld.Server.EntityManager.HasComponent<T>(entity);
	}

	public static NativeArray<ComponentType> GetComponentTypes(this Entity entity)
	{
		return VWorld.Server.EntityManager.GetComponentTypes(entity);
	}

	public static void LogComponentTypes(this Entity entity)
    {
        var comps = VWorld.Server.EntityManager.GetComponentTypes(entity);
		if (entity.Has<PrefabGUID>())
		{
			entity.LogPrefabName();
		}
        foreach (var comp in comps)
        {
            Plugin.PluginLog.LogInfo($"{comp}");
        }
		Plugin.PluginLog.LogInfo("===");
    }

    public static void LogComponentTypes(this EntityQuery entityQuery)
    {
        var types = entityQuery.GetQueryTypes();
        foreach (var t in types)
        {
			Plugin.PluginLog.LogInfo($"Query Component Type: {t}");
        }
		Plugin.PluginLog.LogInfo($"===");
    }

	public static PrefabGUID GetPrefabGUID(this Entity entity)
	{
		if (entity.Exists() && entity.Has<PrefabGUID>())
		{
			return entity.Read<PrefabGUID>();
		}
		return PrefabGUID.Empty;
	}

	public static bool TryGetInventoryBuffer(this Entity entity, out DynamicBuffer<InventoryBuffer> buffer)
	{
		if (entity.Has<InventoryBuffer>())
		{
			buffer = entity.ReadBuffer<InventoryBuffer>();
			return true;
		}
		else if (entity.Has<InventoryInstanceElement>())
		{
			buffer = entity.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity.ReadBuffer<InventoryBuffer>();
			return true;
		}
		buffer = default;
		return false;
	}

	public static void LogPrefabName(this Entity entity)
	{
		if (entity.Has<PrefabGUID>())
		{
			Plugin.PluginLog.LogInfo(entity.Read<PrefabGUID>().LookupName());
		}
		else
		{
			Plugin.PluginLog.LogInfo("GUID Not Found");
		}
	}

	public static string LookupName(this Entity entity)
	{
		if (entity.Exists())
		{
			return entity.Read<PrefabGUID>().LookupName();
		}
		else
		{
			return "Invalid Entity";
		}
	}

	public static string LookupName(this PrefabGUID prefabGuid)
	{
		// PrefabGuidToNameDictionary was removed in v1.1.11+. Reverse-search SpawnableNameToPrefabGuidDictionary.
		var prefabCollectionSystem = VWorld.Server?.GetExistingSystemManaged<PrefabCollectionSystem>();
		if (prefabCollectionSystem == null) return $"GUID_{prefabGuid.GuidHash}";
		foreach (var kvp in prefabCollectionSystem.SpawnableNameToPrefabGuidDictionary)
		{
			if (kvp.Value == prefabGuid)
				return kvp.Key;
		}
		return $"GUID_{prefabGuid.GuidHash}";
	}

	public static void LogPrefabName(this PrefabGUID prefabGuid)
	{
		Plugin.PluginLog.LogInfo(prefabGuid.LookupName());
	}

	public static void Destroy(this Entity entity)
	{
		VWorld.Server.EntityManager.DestroyEntity(entity);
	}

	public static bool Exists(this Entity entity)
	{
		return entity.Index > 0 && VWorld.Server.EntityManager.Exists(entity);
	}

	public static bool HasBuffModification(this Entity entity, BuffModificationTypes modificationType)
	{
		if (!entity.Has<BuffModificationFlagData>()) return false;

		var buffModificationFlagData = entity.Read<BuffModificationFlagData>();
		return buffModificationFlagData.ModificationTypes == (long)BuffModificationTypes.All;
	}

	public static string ToString(this Entity entity)
	{
		return entity.LookupName();
	}

	public static string ToString(this PrefabGUID prefabGUID)
	{
		return prefabGUID.LookupName();
	}

	public static void LogBuffFlags(this Entity buffEntity)
	{
		if (buffEntity.Has<Buff>() && buffEntity.Has<BuffModificationFlagData>())
		{
			var flagData = buffEntity.Read<BuffModificationFlagData>();
			if (flagData.ModificationTypes == (long)BuffModificationTypes.All)
			{
				Plugin.PluginLog.LogInfo(BuffModificationTypes.All.ToString());
				return;
			}

			foreach (BuffModificationTypes type in Enum.GetValues(typeof(BuffModificationTypes)))
			{
				if (type != BuffModificationTypes.None && type != BuffModificationTypes.All && (flagData.ModificationTypes & (long)type) != 0)
				{
					Plugin.PluginLog.LogInfo(type.ToString());
				}
			}
		}
		else
		{
			Plugin.PluginLog.LogInfo("Tried to log buff modification flags for an entity that has no buff modifications");
		}
	}

	public static void LogItemCategories(this Entity itemEntity)
	{
		if (itemEntity.Has<ItemData>())
		{
			var flagData = itemEntity.Read<ItemData>();
			if (flagData.ItemCategory == ItemCategory.ALL)
			{
				Plugin.PluginLog.LogInfo(ItemCategory.ALL.ToString());
				return;
			}

			foreach (ItemCategory type in Enum.GetValues(typeof(ItemCategory)))
			{
				if (type != ItemCategory.NONE && type != ItemCategory.ALL && (flagData.ItemCategory & type) != 0)
				{
					Plugin.PluginLog.LogInfo(type.ToString());
				}
			}
		}
		else
		{
			Plugin.PluginLog.LogInfo("Tried to log item categories for an entity that has no item data");
		}
	}
}
#pragma warning restore CS8500

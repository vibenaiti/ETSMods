using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using ProjectM.Network;
using ModCore.Events;
using ModCore.Services;
using Unity.Entities;
using Unity.Collections;
using Il2CppInterop.Runtime;
using ModCore.Helpers;
using Stunlock.Core;

namespace ModCore.Listeners;

public static class FromCharacterListener
{
	private static EntityQueryOptions Options = EntityQueryOptions.Default;
	private static EntityQueryDesc QueryDesc;
	private static EntityQuery Query;
	private static bool Initialized = false;

	public static void Initialize()
	{
		if (Initialized) return;
		QueryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<FromCharacter>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = Options
		};
		Query = VWorld.Server.EntityManager.CreateEntityQuery(QueryDesc);

		GameEvents.OnGameFrameUpdate += OnUpdate;
		Initialized = true;
	}

	public static void Dispose()
	{
		GameEvents.OnGameFrameUpdate -= OnUpdate;
		Initialized = false;
	}

	private static void OnUpdate()
	{
		var entities = Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (entity.Exists())
			{
				var user = entity.Read<FromCharacter>().User;
				if (user.Exists())
				{
					entity.LogComponentTypes();
				}
			}
		}

		entities.Dispose();
	}
}

public static class AbilityCastEndedEventListener
{
	private static EntityQueryOptions Options = EntityQueryOptions.Default;
	private static EntityQueryDesc QueryDesc;
	private static EntityQuery Query;
	private static bool Initialized = false;

	public static void Initialize()
	{
		if (Initialized) return;
		QueryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
					{
				new ComponentType(Il2CppType.Of<AbilityPostCastFinishedEvent>(), ComponentType.AccessMode.ReadWrite)
					},
			Options = Options
		};
		Query = VWorld.Server.EntityManager.CreateEntityQuery(QueryDesc);

		GameEvents.OnGameFrameUpdate += OnUpdate;
		Initialized = true;
	}

	public static void Dispose()
	{
		GameEvents.OnGameFrameUpdate -= OnUpdate;
		Initialized = false;
	}

	private static void OnUpdate()
	{
		if (GameEvents.OnPlayerFinishedCasting == null) return;

		var entities = Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (entity.Exists())
			{
				var abilityPostCastFinishedEvent = entity.Read<AbilityPostCastFinishedEvent>();
				if (abilityPostCastFinishedEvent.Character.Has<PlayerCharacter>())
				{
					var player = PlayerService.GetPlayerFromCharacter(abilityPostCastFinishedEvent.Character);
					GameEvents.OnPlayerFinishedCasting?.Invoke(player, abilityPostCastFinishedEvent);
				}
			}
		}

		entities.Dispose();
	}
}

public static class StatChangeListener
{
	private static EntityQueryOptions Options = EntityQueryOptions.Default;
	private static EntityQueryDesc QueryDesc;
	private static EntityQuery Query;
	private static bool Initialized = false;
	public static Dictionary<Entity, Entity> SummonToGrandparentPlayerCharacter = new();

	public static void Initialize()
	{
		if (Initialized) return;
		QueryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<StatChangeEvent>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = Options
		};
		Query = VWorld.Server.EntityManager.CreateEntityQuery(QueryDesc);

		GameEvents.OnGameFrameUpdate += OnUpdate;
		Initialized = true;
	}

	public static void Dispose()
	{
		GameEvents.OnGameFrameUpdate -= OnUpdate;
		Initialized = false;
	}

	private static void OnUpdate()
	{
		if (GameEvents.OnPlayerHealthChanged == null && GameEvents.OnUnitHealthChanged == null) return;

		var entities = Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var statChangeEvent = entity.Read<StatChangeEvent>();
				var source = statChangeEvent.Source;
				if (source.Exists() && source.Has<EntityOwner>())
				{
					if (statChangeEvent.StatType == StatType.Health)
					{
						var damageDealerEntity = source.Read<EntityOwner>().Owner;
						if (damageDealerEntity.Exists())
						{
							if (damageDealerEntity.Has<EntityOwner>())
							{
								var owner = damageDealerEntity.Read<EntityOwner>().Owner;
								if (owner.Exists()) //if it has a parent
								{
									damageDealerEntity = owner;
								}
								else if (SummonToGrandparentPlayerCharacter.TryGetValue(damageDealerEntity, out owner))
								{
									damageDealerEntity = owner;
								}
							}
							var targetEntity = statChangeEvent.Entity;
							if (targetEntity.Exists())
							{
								if (targetEntity.Has<PlayerCharacter>())
								{
									var targetPlayer = PlayerService.GetPlayerFromCharacter(targetEntity);
									var totalChange = (statChangeEvent.Change == 0 ? Math.Abs(statChangeEvent.OriginalChange) : Math.Abs(statChangeEvent.Change));

									if (totalChange > 0)
									{
										GameEvents.OnPlayerHealthChanged?.Invoke(damageDealerEntity, entity, statChangeEvent, targetPlayer, source.Read<PrefabGUID>());
									}
								}
								else
								{
									GameEvents.OnUnitHealthChanged?.Invoke(damageDealerEntity, entity, statChangeEvent, targetEntity, source.Read<PrefabGUID>());
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
				continue;
			}
		}
		entities.Dispose();
	}
}


public static class AutoChainListener
{
	private static EntityQueryOptions Options = EntityQueryOptions.Default;
	private static EntityQueryDesc QueryDesc;
	private static EntityQuery Query;
	private static bool Initialized = false;

	public static void Initialize()
	{
		if (Initialized) return;
		QueryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
					{
				new ComponentType(Il2CppType.Of<SpawnChainTransitionEvent>(), ComponentType.AccessMode.ReadWrite)
					},
			Options = Options
		};
		Query = VWorld.Server.EntityManager.CreateEntityQuery(QueryDesc);

		GameEvents.OnGameFrameUpdate += OnUpdate;
		Initialized = true;
	}

	public static void Dispose()
	{
		GameEvents.OnGameFrameUpdate -= OnUpdate;
		Initialized = false;
	}

	private static void OnUpdate()
	{
		if (GameEvents.OnPlayerFinishedCasting == null) return;

		var entities = Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (entity.Exists())
			{
				entity.LogComponentTypes();
			}
		}

		entities.Dispose();
	}
}

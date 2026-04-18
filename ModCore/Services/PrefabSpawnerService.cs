using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ModCore.Data;
using ModCore.Factories;
using ModCore.Helpers;
using ModCore.Listeners;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Il2CppInterop.Runtime;
using ModCore.Events;
using Stunlock.Core;

namespace ModCore
{
	public class PrefabSpawnerService
	{
		private static UnitSpawnerUpdateSystem usus = VWorld.Server.GetExistingSystemManaged<UnitSpawnerUpdateSystem>();

		internal const int DEFAULT_MINRANGE = 0;
		internal const int DEFAULT_MAXRANGE = 0;

		//components:


		private static Action<Entity> WrapCallback(Action<Entity> originalAction, int rotationMode, string category = "")
		{
			return entity =>
			{
				entity.Remove<ModifyMovementSpeedBuff>();
				var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(entity.GetPrefabGUID());
				prefabEntity.Remove<CanFly>();
				prefabEntity.Remove<ModifyMovementSpeedBuff>();
				if (category != "") //we store the category on the unit's useless stats in case we need to find them after a server restart
				{
					entity.Add<ResistanceData>();
					entity.Write(new ResistanceData
					{
						GarlicResistance_IncreasedExposureFactorPerRating = UnitFactory.StringToFloatHash(category)
					});
					entity.Add<NameableInteractable>();
					entity.Write(new NameableInteractable
					{
						Name = category
					});
				}

				if (Rotations.RotationModes.ContainsKey(rotationMode)) 
				{
					var rotation = entity.Read<Rotation>();
					rotation.Value = Rotations.RotationModes[rotationMode];
					entity.Write(rotation);
				}
				

				// Call the original action
				originalAction(entity);
			};
		}

		public static void SpawnWithCallback(PrefabGUID unit, float3 position, Action<Entity> postActions, int rotation = 0, float duration = -1, string category = "")
		{
			var buildingKey = $"{position.xz}_{unit.GuidHash}";
			var durationKey = NextKey();
			if (Helper.TryGetPrefabEntityByPrefabGUID(unit, out var prefabEntity))
			{
				//the idea here is to add components to the prefab entity temporarily so that the spawned entity will spawn with those components, then remove those components from the prefab once we found the spawned unit
				prefabEntity.Add<CanFly>(); //permanently tag as custom spawned
				prefabEntity.Add<ModifyMovementSpeedBuff>(); //this is a fake spawn tag to be removed once we hit the callback


				if (prefabEntity.Has<CastleHeartConnection>())
				{
					durationKey = (long)duration;
				}
			}

			// Spawn the entity1
			usus.SpawnUnit(Entity.Null, unit, position, 1, DEFAULT_MINRANGE, DEFAULT_MAXRANGE, durationKey);

			// Associate the unique key with the post-action callback
			var wrappedCallback = WrapCallback(postActions, rotation, category);

			if (prefabEntity.Has<CastleHeartConnection>())
			{
				if (!BuildingPostActions.ContainsKey(buildingKey))
				{
					BuildingPostActions.Add(buildingKey, wrappedCallback);
					if (BuildingPostActions.Count > 0)
					{
						ManuallySpawnedPrefabListener.Initialize();
					}
				}
			}
			else
			{
				if (!UnitPostActions.ContainsKey(durationKey))
				{
					UnitPostActions.Add(durationKey, (duration, wrappedCallback));
				}
			}
		}
		static internal long NextKey()
		{
			System.Random r = new();
			long key;
			int breaker = 5;
			do
			{
				key = r.NextInt64(10000) * 3;
				breaker--;
				if (breaker < 0)
				{
					throw new Exception($"Failed to generate a unique key for UnitSpawnerService");
				}
			} while (UnitPostActions.ContainsKey(key));
			return key;
		}

		static internal Dictionary<string, Action<Entity>> BuildingPostActions = new Dictionary<string, Action<Entity>>();
		static internal Dictionary<long, (float actualDuration, Action<Entity>)> UnitPostActions = new Dictionary<long, (float actualDuration, Action<Entity>)>();


		[HarmonyPatch(typeof(UnitSpawnerReactSystem), nameof(UnitSpawnerReactSystem.OnUpdate))]
		private static class UnitSpawnerReactSystem_Patch
		{
			public static void Prefix(UnitSpawnerReactSystem __instance)
			{
				var entities = __instance._Query.ToEntityArray(Allocator.Temp);
				foreach (var entity in entities)
				{
					if (!entity.Has<LifeTime>()) continue;
					
					var lifetimeComp = entity.Read<LifeTime>();
					var durationKey = (long)Mathf.Round(lifetimeComp.Duration);
					if (UnitPostActions.TryGetValue(durationKey, out var unitData))
					{
						var (actualDuration, actions) = unitData;
						UnitPostActions.Remove(durationKey);

						var endAction = actualDuration < 0 ? LifeTimeEndAction.None : LifeTimeEndAction.Destroy;
                        actualDuration = 0;
						var newLifeTime = new LifeTime()
						{
							Duration = actualDuration,
							EndAction = endAction
						};

						entity.Write(newLifeTime);
						actions(entity);
					}
				}
				entities.Dispose();
			}
		}

		public static class ManuallySpawnedPrefabListener
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
				new ComponentType(Il2CppType.Of<CanFly>(), ComponentType.AccessMode.ReadWrite), //this is going to be a permanent "custom spawned"
				new ComponentType(Il2CppType.Of<ModifyMovementSpeedBuff>(), ComponentType.AccessMode.ReadWrite), //this is going to be a "custom spawned" tag that we immediately remove in order to distinguish between fresh spawns vs old stuff
},
					None = new ComponentType[]
{
				new ComponentType(Il2CppType.Of<PlayerCharacter>(), ComponentType.AccessMode.ReadWrite)
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
				Query.Dispose();
			}
			
			private static void OnUpdate()
			{
				var entities = Query.ToEntityArray(Allocator.Temp);
				foreach (var entity in entities)
				{
					// entity.LogComponentTypes();
					if (entity.Exists() && entity.Has<LocalToWorld>() && !entity.Has<UnitSpawnData>())
					{
						var localToWorld = entity.Read<LocalToWorld>();
						var key = $"{localToWorld.Position.xz}_{entity.Read<PrefabGUID>().GuidHash}";
						if (BuildingPostActions.ContainsKey(key))
						{
							BuildingPostActions[key](entity);
							BuildingPostActions.Remove(key);
							if (BuildingPostActions.Count == 0)
							{
								Dispose();
								break;
							}
						}
					}
				}

				entities.Dispose();
			}
		}
	}
}



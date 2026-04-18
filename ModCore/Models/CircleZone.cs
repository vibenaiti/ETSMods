using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjectM;
using ModCore.Data;
using ModCore.Helpers;
using ModCore.Services;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static ProjectM.HitColliderCast;
using Stunlock.Core;

namespace ModCore.Models
{
	public class CircleZone
	{
		public float3 Center { get; set; }
		public float Radius { get; set; }
		public int Height { get; set; } = -1;

		public CircleZone(float3 center, float radius, int height = -1)
		{
			Center = center;
			Radius = radius;
			Height = height;
		}

		public static CircleZone FromEntity(Entity entity, float radius)
		{
			var position = entity.Read<LocalToWorld>().Position;

			return new CircleZone(position, radius);
		}

		public bool Contains(Player player)
		{
			return Contains(player.Character);
		}

		public bool Contains(Entity entity)
		{
			if (entity.Has<LocalToWorld>())
			{
				var localToWorld = entity.Read<LocalToWorld>();
				var x = localToWorld.Position.x;
				var z = localToWorld.Position.z;
				if (Height != -1)
				{
					if (entity.Read<TilePosition>().HeightLevel != Height) return false;
				}
				float dx = x - Center.x;
				float dz = z - Center.z;
				return (dx * dx + dz * dz) <= (Radius * Radius);
			}
			return false;
		}

		public bool Contains(float3 position)
		{
			float dx = position.x - Center.x;
			float dz = position.z - Center.z;
			return (dx * dx + dz * dz) <= (Radius * Radius);
		}

		// Additional functionality as needed

		public override string ToString()
		{
			return $"\"Center\": ({Center.x:F2}, {Center.y:F2}, {Center.z:F2}),\n\"Radius\": {Radius:F2}";
		}
	}

	public class ShrinkingCircleZone : CircleZone
	{
		public float StartRadius { get; private set; }
		public float EndRadius { get; private set; }
		public float TotalShrinkTime { get; private set; }
		public int UpdateIntervalInSeconds { get; private set; }
		public PrefabGUID BorderPrefab { get; set; }
		public float PrefabRadius { get; set; }
		private float elapsedTime;
		private Timer timer;
		private List<Entity> borderEntities = new List<Entity>();

		public ShrinkingCircleZone(float3 center, float startRadius, float endRadius, float totalShrinkTime, int updateIntervalInSeconds = 5, float prefabRadius = 0)
	: base(center, startRadius)
		{
			StartRadius = startRadius;
			EndRadius = endRadius;
			TotalShrinkTime = totalShrinkTime;
			UpdateIntervalInSeconds = updateIntervalInSeconds;
			BorderPrefab = Prefabs.TM_ChurchOfLight_Overseer_IceRink_Icicle_02;
			elapsedTime = 0;
			timer = ActionScheduler.RunActionEveryInterval(ShrinkZone, UpdateIntervalInSeconds);
			PrefabRadius = prefabRadius;
		}

		public ShrinkingCircleZone(float3 center, float startRadius, float endRadius, float totalShrinkTime, PrefabGUID borderPrefab, int updateIntervalInSeconds = 5, float prefabRadius = 0)
			: base(center, startRadius)
		{
			StartRadius = startRadius;
			EndRadius = endRadius;
			TotalShrinkTime = totalShrinkTime;
			UpdateIntervalInSeconds = updateIntervalInSeconds;
			BorderPrefab = borderPrefab;
			elapsedTime = 0;
			timer = ActionScheduler.RunActionEveryInterval(ShrinkZone, UpdateIntervalInSeconds);
			PrefabRadius = prefabRadius;
		}

		public void ShrinkZone()
		{
			if (Radius == EndRadius)
			{
				return;
			}
			elapsedTime += UpdateIntervalInSeconds;
			if (elapsedTime < TotalShrinkTime)
			{
				// Calculate the new radius based on the elapsed time
				Radius = StartRadius - (StartRadius - EndRadius) * (elapsedTime / TotalShrinkTime);
			}
			else
			{
				// Once the total time has elapsed, set the radius to the end radius
				Radius = EndRadius;
			}
			UpdateBorder();
		}

		private void UpdateBorder()
		{
			// Despawn old border entities
			foreach (var entity in borderEntities)
			{
				Helper.DestroyEntity(entity);
			}
			borderEntities.Clear();

			// Calculate new border positions and spawn new entities
			int numEntities = CalculateNumberOfEntitiesForBorder();
			float angleStep = 360.0f / numEntities;
			for (int i = 0; i < numEntities; i++)
			{
				float angle = math.radians(angleStep * i);
				float3 position = new float3(
					Center.x + (Radius + PrefabRadius) * math.cos(angle),
					Center.y,
					Center.z + (Radius + PrefabRadius) * math.sin(angle)
				);
				PrefabSpawnerService.SpawnWithCallback(BorderPrefab, position, (e) => 
				{ 
					borderEntities.Add(e);
					e.Remove<Health>();
					e.Remove<HitColliderCast>();
					e.Remove<HitTrigger>();
					e.Remove<ApplyBuffOnGameplayEvent>();
					e.Remove<GameplayEventListeners>();
					e.Remove<CollisionCastOnUpdate>();
					e.Remove<CreateGameplayEventsOnTick>();
					e.Remove<CreateGameplayEventsOnHit>();
				});
			}
		}

		private int CalculateNumberOfEntitiesForBorder()
		{
			// Logic to determine how many entities should be placed based on the radius
			// For example, one entity per unit of circumference
			return (int)(.5 * Math.PI * Radius);
		}

		// Override ToString() if necessary
		public override string ToString()
		{
			return $"\"Center\": ({Center.x:F2}, {Center.y:F2}, {Center.z:F2}),\n\"StartRadius\": {StartRadius:F2},\n\"EndRadius\": {EndRadius:F2},\n\"CurrentRadius\": {Radius:F2},\n\"TotalShrinkTime\": {TotalShrinkTime:F2},\n\"ElapsedTime\": {elapsedTime:F2}";
		}

		public void Dispose()
		{
			if (timer != null)
			{
				timer.Dispose();
				timer = null;
			}
			foreach (var entity in borderEntities)
			{
				Helper.DestroyEntity(entity);
			}
			borderEntities.Clear();
		}
	}

}



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

public static class RemovePvpProtectionListener
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
				new ComponentType(Il2CppType.Of<FromCharacter>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<RemovePvPProtectionEvent>(), ComponentType.AccessMode.ReadWrite)
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
		if (GameEvents.OnPlayerRemovedPvpProtection == null) return;

		var entities = Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (entity.Exists())
			{
				var player = PlayerService.GetPlayerFromUser(entity.Read<FromCharacter>().User);
				GameEvents.OnPlayerRemovedPvpProtection?.Invoke(player, entity);
			}
		}

		entities.Dispose();
	}
}

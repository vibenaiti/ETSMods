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
using ModCore.Events;
using ModCore.Services;
using ModCore.Models;
using ModCore.Data;
using ModCore.Factories;
using static ModCore.Factories.UnitFactory;
using UnityEngine;
using ProjectM.CastleBuilding;
using Il2CppSystem;
using Stunlock.Network;
using ProjectM.Terrain;
using Stunlock.Core;

namespace ModCore.Helpers;

//this is horrible god help us all
public static partial class Helper
{
	public const int RANDOM_POWER = -1;

	public static float3 NETHER_POSITION_1 = new(-1973.5f, 5f, -3169f);

	public static NativeHashSet<PrefabGUID> prefabGUIDs;

	public static System.Random random = new System.Random();

	public static void RevivePlayer(Player player, float3? pos = null)
	{
		var sbs = VWorld.Server.GetExistingSystemManaged<ServerBootstrapSystem>();
		var bufferSystem = VWorld.Server.GetExistingSystemManaged<EntityCommandBufferSystem>();
		var buffer = bufferSystem.CreateCommandBuffer();

		Nullable_Unboxed<float3> spawnLoc = new();
		if (pos.HasValue)
		{
			spawnLoc.value = pos.Value;
		}
		else
		{
			spawnLoc.value = player.Position;
		}

		var health = player.Character.Read<Health>();
		if (Helper.HasBuff(player, Prefabs.Buff_General_Vampire_Wounded_Buff))
		{
			Helper.RemoveBuff(player, Prefabs.Buff_General_Vampire_Wounded_Buff);

			health.Value = health.MaxHealth;
			health.MaxRecoveryHealth = health.MaxHealth;
			player.Character.Write(health);
		}

		if (health.IsDead)
		{
			sbs.RespawnCharacter(buffer, player.User,
				customSpawnLocation: spawnLoc,
				previousCharacter: player.Character);
			player.Teleport(spawnLoc.value);
			PollForCoffinBuff(player);
		}
	}

	private static void PollForCoffinBuff(Player player)
	{
		if (Helper.TryGetBuff(player, Prefabs.AB_Interact_TombCoffinSpawn_Travel_Delay, out var buffEntity))
		{
			buffEntity.Remove<GameplayEventListeners>();
			Helper.RemoveBuff(player, Prefabs.AB_Interact_TombCoffinSpawn_Travel_Delay);
		}
		else
		{
			var action = () =>
			{
				PollForCoffinBuff(player);
			};
			ActionScheduler.RunActionOnceAfterFrames(action, 5);
		}
	}

	public static List<Player> GetPlayersNearPlayer(Player player, int distance = 15, bool includeSpectators = false)
	{
		List<Player> nearbyPlayers = new();
		foreach (var onlinePlayer in PlayerService.OnlinePlayersWithUsers) 
		{
			if (math.distance(onlinePlayer.Position, player.Position) <= distance && onlinePlayer != player)
			{
				if (!includeSpectators && Helper.HasBuff(onlinePlayer, Prefabs.Admin_Observe_Invisible_Buff))
				{
					continue;
				}
				nearbyPlayers.Add(onlinePlayer);
			}
		}
		return nearbyPlayers;
	}

	public static void KickPlayer(ulong steamId, ConnectionStatusChangeReason kickReason = Stunlock.Network.ConnectionStatusChangeReason.Kicked)
	{
		Core.serverBootstrapSystem.Kick(steamId, kickReason, false);
	}

	public static void KickPlayer(Player player, ConnectionStatusChangeReason kickReason = Stunlock.Network.ConnectionStatusChangeReason.Kicked)
	{
		Core.serverBootstrapSystem.Kick(player.SteamID, kickReason, false);
	}

	public static float Clamp(float value, float min, float max)
	{
		return System.Math.Max(min, System.Math.Min(value, max));
	}

	public static void Unlock(Player player, bool unlockContent = false)
	{
		var fromCharacter = player.ToFromCharacter();
		Core.debugEventsSystem.UnlockAllResearch(fromCharacter);
		Core.debugEventsSystem.UnlockAllVBloods(fromCharacter);
		Core.debugEventsSystem.CompleteAllAchievements(fromCharacter);
		UnlockAllWaypoints(player);
		UnlockAllAbilities(player);
		UnlockAllPassives(player);
        
		if (unlockContent)
			UnlockAllContent(fromCharacter);
	}

	public static void UnlockAllAbilities(Player player)
	{
		var entities = Helper.GetPrefabEntitiesByComponentTypes<AbilitySpellSchool>();
		var prefabLookupMap = Core.prefabCollectionSystem._PrefabLookupMap;
		foreach (var entity in entities)
		{
			SpellSchoolProgressionUtility_Server.TryUnlockAbility(VWorld.Server.EntityManager, prefabLookupMap, player.User, entity.GetPrefabGUID(), true);
		}
	}

	public static void UnlockAllPassives(Player player)
	{
		// DebugUnlockAllSpellSchoolPassives was removed in v1.1.11+.
		// Passives must now be unlocked individually via SpellSchoolProgressionUtility_Server.TryUnlockAbility.
		Plugin.PluginLog.LogWarning("UnlockAllPassives: DebugUnlockAllSpellSchoolPassives is not available in this game version.");
	}

	public static void UnlockAllContent(FromCharacter fromCharacter)
	{
		SetUserContentDebugEvent setUserContentDebugEvent = new SetUserContentDebugEvent
		{
			Value = UserContentFlags.EarlyAccess | UserContentFlags.DLC_DraculasRelics_EA |
					UserContentFlags.GiveAway_Razer01 | UserContentFlags.DLC_FoundersPack_EA |
					UserContentFlags.Halloween2022 | UserContentFlags.DLC_Gloomrot | UserContentFlags.DLC_ProjectK
		};
		Core.debugEventsSystem.SetUserContentDebugEvent(fromCharacter.User.Read<User>().Index, ref setUserContentDebugEvent,
			ref fromCharacter);
	}

	public static void UnlockAllWaypoints(Player player)
	{
		var buffer = VWorld.Server.EntityManager.AddBuffer<UnlockedWaypointElement>(player.User);
		var waypointComponentType =
			new ComponentType(Il2CppType.Of<ChunkWaypoint>(), ComponentType.AccessMode.ReadWrite);
		var query = VWorld.Server.EntityManager.CreateEntityQuery(waypointComponentType);
		var waypoints = query.ToEntityArray(Allocator.Temp);
		buffer.Clear();
		foreach (var waypoint in waypoints)
		{
			var unlockedWaypoint = new UnlockedWaypointElement();
			unlockedWaypoint.Waypoint = waypoint.Read<NetworkId>();
			buffer.Add(unlockedWaypoint);
		}

		waypoints.Dispose();
	}

    public static void RevealMapForPlayer(Player player)
    {
        var attachedBuffer = player.User.ReadBuffer<AttachedBuffer>();
        foreach (var attached in attachedBuffer)
        {
            if (attached.Entity.GetPrefabGUID() == Prefabs.UserMapZoneState)
            {
                var buffer = attached.Entity.ReadBuffer<UserMapZonePackedRevealElement>();
                for (var i = 0; i < buffer.Length; i++)
                {
                    var userMapZonePackedRevealElement = buffer[i];
                    userMapZonePackedRevealElement.PackedPixel = (byte)255;
                    buffer[i] = userMapZonePackedRevealElement;
                }
            }
        }
    }

	public static void RenamePlayer(FromCharacter fromCharacter, string newName)
	{
		var nameTaken = false;
		foreach (var player in PlayerService.CharacterCache.Values)
		{
			if (player.Name.ToLower() == newName.ToLower())
			{
				nameTaken = true;
				break;
			}
		}
		if (!nameTaken)
		{
			var networkId = fromCharacter.User.Read<NetworkId>();
			var renameEvent = new RenameUserDebugEvent
			{
				NewName = newName,
				Target = networkId
			};
			Core.debugEventsSystem.RenameUser(fromCharacter, renameEvent);
		}
	}

	public static void ResetAllServants(Team playerTeam)
	{
		var servantCoffinComponentType =
			new ComponentType(Il2CppType.Of<ServantCoffinstation>(), ComponentType.AccessMode.ReadWrite);
		var query = VWorld.Server.EntityManager.CreateEntityQuery(servantCoffinComponentType);
		var servantCoffins = query.ToEntityArray(Allocator.Temp);

		foreach (var servantCoffin in servantCoffins)
		{
			try
			{
				var coffinTeam = servantCoffin.Read<Team>();
				if (coffinTeam.Value == playerTeam.Value)
				{
					var servantCoffinStation = servantCoffin.Read<ServantCoffinstation>();
					var servant = servantCoffinStation.ConnectedServant._Entity;
					servant.Write(new ServantEquipment());
					StatChangeUtility.KillEntity(VWorld.Server.EntityManager, servant, Entity.Null, 0, StatChangeReason.Any, true);
				}
			}
			catch (System.Exception e)
			{

			}
		}
	}

	public static bool UserHasAbilitiesUnlocked(Entity User)
	{
		var buffer = User.ReadBuffer<AttachedBuffer>();
		foreach (var attached in buffer)
		{
			if (attached.PrefabGuid == Prefabs.ProgressionCollection)
			{
				var progressionEntity = attached.Entity;
				var unlockedAbilityBuffer = progressionEntity.ReadBuffer<UnlockedSpellBookAbility>();
				if (unlockedAbilityBuffer.Length > 20)
				{
					return true;
				}
			}
		}

		return false;
	}

	public static void SoftKillPlayer(Player player)
	{
		Helper.BuffPlayer(player, Prefabs.Buff_General_Vampire_Wounded_Buff, out var buffEntity);
	}

    //used to kill something via damage if it has health, otherwise destroys it. It is better to destroy using DestroyEntity
	public static void KillOrDestroyEntity(Entity entity)
	{
		StatChangeUtility.KillOrDestroyEntity(VWorld.Server.EntityManager, entity, entity, entity, 0, StatChangeReason.Any, true);
	}

    //used to remove things from existence -- doesn't trigger on-death logic (but will trigger on-destroy logic)
	public static void DestroyEntity(Entity entity)
	{
		DestroyUtility.CreateDestroyEvent(VWorld.Server.EntityManager, entity, DestroyReason.Default, DestroyDebugReason.None);
	}

	public static void RespawnPlayer(Player player, float3 pos)
	{
		if (!player.IsAlive)
		{
			var buffer = Core.entityCommandBufferSystem.CreateCommandBuffer();

			Nullable_Unboxed<float3> spawnLoc = new();
			spawnLoc = new();
			spawnLoc.value = pos;

			Core.serverBootstrapSystem.RespawnCharacter(buffer, player.User,
			customSpawnLocation: spawnLoc,
				previousCharacter: player.Character, spawnLocationIndex: 0);
		}
	}

/*	var entity = VWorld.Server.EntityManager.CreateEntity(
	ComponentType.ReadWrite<FromCharacter>(),
	ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
);
	entity.Write(player.ToFromCharacter());
		entity.Write<PlayerTeleportDebugEvent>(new ()
		{
			Position = targetPosition,
			Target = PlayerTeleportDebugEvent.TeleportTarget.Self
});*/

	public static void TeleportViaEvent(this Entity unit, float3 targetPosition)
	{
		var anyPlayer = PlayerService.GetAnyPlayer();
		var eventEntity = Helper.CreateEntityWithComponents<TeleportDebugEvent, FromCharacter>();
		eventEntity.Write(anyPlayer.ToFromCharacter());
		eventEntity.Write(new TeleportDebugEvent
		{
			Location = TeleportDebugEvent.TeleportLocation.WorldPosition,
			MousePosition = unit.Read<LocalToWorld>().Position,
			Target = TeleportDebugEvent.TeleportTarget.ClosestUnitToCursor,
			LocationPosition = targetPosition
		});
	}

	public static void Teleport(this Entity unit, float3 targetPosition)
	{
		var teleportBlockingBuffs = Helper.GetTeleportBlockingBuffs(unit);
		foreach (var teleportBlockingBuff in teleportBlockingBuffs)
		{
			Helper.DestroyBuff(teleportBlockingBuff);
		}

		if (Helper.BuffEntity(unit, Helper.CustomBuff4, out var buffEntity, 0.25f))
		{
			Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair);
			buffEntity.Add<TeleportBuff>();
			buffEntity.Write(new TeleportBuff
			{
				EndPosition = targetPosition
			});
		}
	}

	public static void Teleport(this Player player, float3 targetPosition)
	{
		player.Character.Teleport(targetPosition);
	}

	public static void Teleport(this Player player, float2 targetPosition)
	{
		player.Character.Teleport(new float3(targetPosition.x, player.Position.y, targetPosition.y));
	}

	public class ResetOptions
    {
        public bool RemoveConsumables = false;
        public bool RemoveShapeshifts = false;
        public bool ResetCooldowns = true;
		public bool RemoveMinions = false;
		public bool RemoveBuffs = true;
		public bool ResetHealth = true;
		public HashSet<PrefabGUID> BuffsToIgnore = new HashSet<PrefabGUID>();

        public static ResetOptions Default => new ResetOptions();
        public static ResetOptions FreshMatch = new ResetOptions
        {
            RemoveConsumables = true,
            RemoveShapeshifts = true,
            ResetCooldowns = true,
			RemoveMinions = true,
			RemoveBuffs = true,
			ResetHealth = true,
            BuffsToIgnore = new HashSet<PrefabGUID>()
        };
    }

	private static void DestroyPlayerSummonsAndProjectiles(Player player)
	{
		var projectileEntities = Helper.GetEntitiesByComponentTypes<HitColliderCast, SpellModArithmetic>();
		foreach (var entity in projectileEntities)
		{
			var owner = entity.Read<EntityOwner>().Owner;
			if (owner == player.Character)
			{
				if (entity.Has<CreateGameplayEventsOnDestroy>())
				{
					var buffer = entity.ReadBuffer<CreateGameplayEventsOnDestroy>();
					buffer.Clear();
				}
				Helper.DestroyEntity(entity);
			}
		}
		projectileEntities.Dispose();

		var minionEntities = Helper.GetEntitiesByComponentTypes<Minion>();
		foreach (var entity in minionEntities)
		{
			var owner = entity.Read<EntityOwner>().Owner;
			if (owner == player.Character)
			{
				if (entity.Has<CreateGameplayEventsOnDestroy>())
				{
					var buffer = entity.ReadBuffer<CreateGameplayEventsOnDestroy>();
					buffer.Clear();
				}
				if (entity.Has<CreateGameplayEventOnDeath>())
				{
					var buffer = entity.ReadBuffer<CreateGameplayEventOnDeath>();
					buffer.Clear();
				}
				Helper.DestroyEntity(entity);
			}
		}
		minionEntities.Dispose();
	}

	public static void Reset(this Player player, ResetOptions resetOptions = null)
	{
		if (!player.Character.Exists()) return;

		resetOptions ??= ResetOptions.Default;
		
        if (resetOptions.ResetCooldowns)
		{
            ResetCooldown(player.Character);
		}
		if (resetOptions.RemoveMinions)
		{
			DestroyPlayerSummonsAndProjectiles(player);
		}
		if (resetOptions.RemoveBuffs)
		{
			ClearExtraBuffs(player.Character, resetOptions);
		}
        
        //delay so that removing gun e / heart strike doesnt dmg you
		if (resetOptions.ResetHealth)
		{
			var action = () => HealEntity(player.Character);
			ActionScheduler.RunActionOnceAfterFrames(action, 3);
		}

        GameEvents.OnPlayerReset?.Invoke(player);
    }

	public static bool IsImmaterial(this Player player)
	{
		var buffs = Helper.GetAllBuffs(player);
		foreach (var buff in buffs)
		{
			if (buff.HasBuffModification(BuffModificationTypes.Immaterial)) return true;
		}
		return false;
	}

	/*	public static void MakeSCT(Player player, PrefabGUID sctPrefab, float value = 0, float3 pos = default)
		{
			if (pos.Equals(default(float3)))
			{
				pos = player.Position;
			}
			var sctEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.ScrollingCombatTextMessage);
			ScrollingCombatTextMessage.Create(VWorld.Server.EntityManager, Core.entityCommandBufferSystem.CreateCommandBuffer(), sctEntity, value, sctPrefab, pos, player.Character, player.Character);
		}*/

	public static void ResetCooldown(Entity character)
	{
		if (character.Exists())
		{
			var buffer = character.ReadBuffer<AbilityGroupSlotBuffer>();
			foreach (var ability in buffer)
			{
				var abilityGroupSlotEntity = ability.GroupSlotEntity._Entity;
				if (abilityGroupSlotEntity.Exists())
				{
					var abilityGroupSlotData = abilityGroupSlotEntity.Read<AbilityGroupSlot>();
					var abilityGroupSlotStateEntity = abilityGroupSlotData.StateEntity._Entity;
					if (abilityGroupSlotStateEntity.Exists())
					{
						if (abilityGroupSlotStateEntity.Has<AbilityChargesState>())
						{
							var abilityChargesState = abilityGroupSlotStateEntity.Read<AbilityChargesState>();
							var abilityChargesData = abilityGroupSlotStateEntity.Read<AbilityChargesData>();
							abilityChargesState.CurrentCharges = abilityChargesData.MaxCharges;
							abilityChargesState.ChargeTime = 0;
							abilityGroupSlotStateEntity.Write(abilityChargesState);
						}

						var abilityStateBuffer = abilityGroupSlotStateEntity.ReadBuffer<AbilityStateBuffer>();

						foreach (var state in abilityStateBuffer)
						{
							var abilityState = state.StateEntity._Entity;
							if (abilityState.Exists())
							{
								var abilityCooldownState = abilityState.Read<AbilityCooldownState>();
								abilityCooldownState.CooldownEndTime = 0;
								abilityState.Write(abilityCooldownState);
							}
						}
					}
				}
			}
		}
	}

	public static float GetAbilityCooldown(Player player, PrefabGUID abilityCastGuid)
	{
		var abilityGroupSlotBuffer = player.Character.ReadBuffer<AbilityGroupSlotBuffer>();
		foreach (var abilityGroupSlotNetworkedEntity in abilityGroupSlotBuffer)
		{
			var abilityGroupSlotEntity = abilityGroupSlotNetworkedEntity.GroupSlotEntity._Entity;
			if (abilityGroupSlotEntity.Exists())
			{
				var abilityGroupSlotData = abilityGroupSlotEntity.Read<AbilityGroupSlot>();
				var abilityGroupSlotStateEntity = abilityGroupSlotData.StateEntity._Entity;
				if (abilityGroupSlotStateEntity.Exists())
				{
					var abilityStateBuffer = abilityGroupSlotStateEntity.ReadBuffer<AbilityStateBuffer>();
					foreach (var abilityStateNetworkedEntity in abilityStateBuffer)
					{
						var abilityStateEntity = abilityStateNetworkedEntity.StateEntity._Entity;
						if (abilityStateEntity.Exists() && abilityStateEntity.GetPrefabGUID() == abilityCastGuid)
						{
							var abilityCooldownState = abilityStateEntity.Read<AbilityCooldownState>();
							return abilityCooldownState.GetCurrentCooldown(Helper.GetServerTime());
						}
					}
				}
			}
		}
		return -1;
	}

	public static void Heal(this Player player)
	{
		Health health = player.Character.Read<Health>();
		health.Value = health.MaxHealth;
		health.MaxRecoveryHealth = health.MaxHealth;
		player.Character.Write(health);
	}

	public static void HealEntity(Entity entity)
	{
		Health health = entity.Read<Health>();
		health.Value = health.MaxHealth;
		health.MaxRecoveryHealth = health.MaxHealth;
		entity.Write(health);
	}

	public static int HealEntity(Entity entity, int amount)
	{
		Health health = entity.Read<Health>();
		var originalHealth = health.Value;
		if (health.Value + amount > health.MaxHealth)
		{
			health.Value = health.MaxHealth;
		}
		else
		{
			health.Value += amount;
		}
		
		if (health.MaxRecoveryHealth + amount > health.MaxHealth)
		{
			health.MaxRecoveryHealth = health.MaxHealth;
		}
		else
		{
			health.MaxRecoveryHealth += amount;
		}

		entity.Write(health);
		return (int)(health.Value - originalHealth);
	}

	public static void HealPlayer(Player player, int amount)
	{
		var amountHealed = HealEntity(player.Character, amount);
/*		if (amountHealed > 0)
		{
			if (amountHealed < amount)
			{
				MakeSCT(player, Prefabs.SCT_Type_MAX, amountHealed);
			}
			else
			{
				MakeSCT(player, Prefabs.SCT_Type_Healing, amountHealed);
			}
		}*/
	}

	public static void ToggleBloodOnPlayer(Player player)
	{
		if (!Helper.HasBuff(player, Prefabs.AB_BloodBuff_Warrior_WeaponCooldown))
			SetPlayerBlood(player, Prefabs.BloodType_Warrior, 100f);
		else
			SetPlayerBlood(player, Prefabs.BloodType_None, 100f);
	}

	public static double GetServerTime()
	{
		return Core.serverGameManager.ServerTime;
	}

	public static double GetServerTimeAdjusted()
	{
		return GetServerTime() - Globals.ServerStartTime;
	}

	public static int GetDayOfWipe()
	{
		// Get the server start time and current time
		System.DateTime wipeStartTime = Globals.ServerStartDateTime;
		double secondsSinceWipeStart = Helper.GetServerTimeAdjusted();

		// Define constants
		const int secondsInDay = 86400; // 24 hours * 60 minutes * 60 seconds

		// Calculate the end of the initial "day 1" period (wipe start time until 12 pm the next day)
		System.DateTime endOfDay1 = wipeStartTime.Date.AddDays(1).AddHours(12);
		long secondsUntilEndOfDay1 = (long)(endOfDay1 - wipeStartTime).TotalSeconds;

		// Calculate the day of the wipe
		if (secondsSinceWipeStart <= secondsUntilEndOfDay1)
		{
			// If the seconds since wipe start are less than or equal to the seconds until end of "day 1"
			return 1;
		}
		else
		{
			// Subtract the initial "day 1" period
			double remainingSeconds = secondsSinceWipeStart - secondsUntilEndOfDay1;

			// Calculate the number of full days that have passed since the end of "day 1"
			int fullDays = (int)(remainingSeconds / secondsInDay);

			// Add 2 to account for the initial "day 1"
			return fullDays + 2; // +2 because we count from day 1
		}
	}

	public static void KillPreviousEntities(string category)
	{
		var entities = Helper.GetNonPlayerSpawnedEntities(true);
		foreach (var entity in entities)
		{
			if (!entity.Has<PlayerCharacter>())
			{
				if (UnitFactory.TryGetSpawnedUnitFromEntity(entity, out SpawnedUnit spawnedUnit))
				{
					if (spawnedUnit.Unit.Category == category)
					{
						Helper.DestroyEntity(entity);
					}
				}
				else
				{
					if (entity.Has<ResistanceData>() && entity.Read<ResistanceData>().GarlicResistance_IncreasedExposureFactorPerRating == StringToFloatHash(category))
					{
						Helper.DestroyEntity(entity);
					}
				}
			}
		}
	}

    public static void ChangeBuffResistances(Entity entity, PrefabGUID prefabGuid)
	{
        entity.Add<BuffResistances>();
        var prefabEntity = GetPrefabEntityByPrefabGUID(prefabGuid);
        entity.Write(new BuffResistances
        {
            InitialSettingGuid = prefabGuid,
			SettingsEntity = new ModifiableEntity(prefabEntity)
		});
    }

    public static void MakePlayerCcImmune(Player player)
    {
        player.Character.Add<BuffResistances>();
        player.Character.Write(new BuffResistances
        {
			SettingsEntity = new ModifiableEntity(Helper.GetPrefabEntityByPrefabGUID(Prefabs.BuffResistance_UberMob)),
			InitialSettingGuid = Prefabs.BuffResistance_UberMobNoKnockbackOrGrab
		});
    }

	public static void ApplyBuildImpairBuffToPlayer(Player player)
	{
		if (!player.IsAdmin)
		{
			if (Helper.BuffPlayer(player, Prefabs.Buff_Gloomrot_SentryOfficer_TurretCooldown, out var buffEntity2, Helper.NO_DURATION, true))
			{
				Helper.ModifyBuff(buffEntity2, BuffModificationTypes.BuildMenuImpair);
			}
		}
		else
		{
			Helper.RemoveBuff(player, Prefabs.Buff_Gloomrot_SentryOfficer_TurretCooldown);
		}
	}

	public static void RemoveBuildImpairBuffFromPlayer(Player player)
	{
		Helper.RemoveBuff(player, Prefabs.Buff_Gloomrot_SentryOfficer_TurretCooldown);
	}

	public static void ControlUnit(Player player, Entity unit)
	{
		Helper.BuffPlayer(player, Prefabs.Admin_Observe_Invisible_Buff, out var buffEntity);
		var action = () =>
		{
			var controlDebugEvent = new ControlDebugEvent
			{
				EntityTarget = unit,
				Target = unit.Read<NetworkId>()
			};

			if (unit.Has<AggroConsumer>())
			{
				var aggroConsumer = unit.Read<AggroConsumer>();
				aggroConsumer.Active._Value = false;
				unit.Write(aggroConsumer);
			}

			Core.debugEventsSystem.ControlUnit(player.ToFromCharacter(), controlDebugEvent);
		};
		ActionScheduler.RunActionOnceAfterDelay(action, 0.05f);
	}

	public static void ControlOriginalCharacter(Player player)
	{
		var controlledEntity = player.ControlledEntity;
		float3 position = player.Position;
		if (controlledEntity.Exists())
		{
			if (controlledEntity.Has<LocalToWorld>())
			{
				position = controlledEntity.Read<LocalToWorld>().Position;
			}
			Helper.KillOrDestroyEntity(controlledEntity);
		}
		
		ControlUnit(player, player.Character);
		var action = () => {
			player.Teleport(position);
			Helper.RemoveBuff(player, Prefabs.Admin_Observe_Invisible_Buff);
		};
		ActionScheduler.RunActionOnceAfterDelay(action, 0.05f);
	}

	public static void MakePlayerCcDefault(Player player)
    {
        player.Character.Add<BuffResistances>();
        player.Character.Write(new BuffResistances
        {
			SettingsEntity = new ModifiableEntity(Helper.GetPrefabEntityByPrefabGUID(Prefabs.BuffResistance_Vampire)),
			InitialSettingGuid = Prefabs.BuffResistance_Vampire
        });
    }

    public static void AnnounceSiegeWeapon()
	{
		CreateEntityWithComponents<AnnounceSiegeWeapon, SpawnTag, DestroyOnSpawn, PrefabGUID>();
	}

	public enum SnapMode
    {
        NorthWest = 1, North, NorthEast, West, Center, East, SouthWest, South, SouthEast
    }

    public static float3 GetSnappedHoverPosition(Player player, SnapMode mode)
    {
        float3 originalPosition = player.Character.Read<EntityInput>().AimPosition;
        // Calculate the bottom-left corner of the tile
        float tileX = Mathf.Floor(originalPosition.x / 5) * 5;
        float tileZ = Mathf.Floor(originalPosition.z / 5) * 5;

        // Adjust position based on the snap mode
        switch (mode)
        {
            case SnapMode.NorthWest:
                return new float3(tileX, originalPosition.y, tileZ + 5);
            case SnapMode.North:
                return new float3(tileX + 2.5f, originalPosition.y, tileZ + 5);
            case SnapMode.NorthEast:
                return new float3(tileX + 5, originalPosition.y, tileZ + 5);
            case SnapMode.West:
                return new float3(tileX, originalPosition.y, tileZ + 2.5f);
            case SnapMode.Center:
                return new float3(tileX + 2.5f, originalPosition.y, tileZ + 2.5f);
            case SnapMode.East:
                return new float3(tileX + 5, originalPosition.y, tileZ + 2.5f);
            case SnapMode.SouthWest:
                return new float3(tileX, originalPosition.y, tileZ);
            case SnapMode.South:
                return new float3(tileX + 2.5f, originalPosition.y, tileZ);
            case SnapMode.SouthEast:
                return new float3(tileX + 5, originalPosition.y, tileZ);
            default:
                return originalPosition; // Default case to handle unexpected mode
        }
    }

	public static void BecomeEntity(Player player, PrefabGUID prefabGuid)
	{
		Helper.BuffPlayer(player, Prefabs.Admin_Observe_Invisible_Buff, out var buffEntity, Helper.NO_DURATION);
		PrefabSpawnerService.SpawnWithCallback(prefabGuid, player.Position, (e) =>
		{
			Helper.ControlUnit(player, e);
			if (e.Has<UnitLevel>())
			{
				var level = e.Read<UnitLevel>();
				level.Level._Value = 200;
				e.Write(level);
				e.Write(player.Character.Read<Team>());
				e.Write(player.Character.Read<TeamReference>());
				Helper.BuffEntity(e, Helper.CustomBuff2, out var buffEntity1, Helper.NO_DURATION);
				var aggroConsumer = e.Read<AggroConsumer>();
				aggroConsumer.Active._Value = false;
				e.Write(aggroConsumer);
			}
		});
	}

	//in spite of the settings below, only certain icon prefabs are globally visible
	public static void AttachMapIconToPlayer(Player player, PrefabGUID mapIcon, System.Action<Entity> action)
	{
		PrefabSpawnerService.SpawnWithCallback(mapIcon, player.Position, (e) =>
		{
            e.Add<MapIconData>();
			var mapIconData = e.Read<MapIconData>();
			mapIconData.EnemySetting = MapIconShowSettings.Global;
			mapIconData.AllySetting = MapIconShowSettings.Global;
			mapIconData.RequiresReveal = false;
			mapIconData.ShowOutsideVision = true;
			mapIconData.ShowOnMinimap = true;
			mapIconData.TargetUser = player.User;
			e.Write(mapIconData);

            e.Add<MapIconTargetEntity>();
            var mapIconTargetEntity = e.Read<MapIconTargetEntity>();
			mapIconTargetEntity.TargetEntity = NetworkedEntity.ServerEntity(player.User);
			mapIconTargetEntity.TargetNetworkId = player.User.Read<NetworkId>();
			e.Write(mapIconTargetEntity);
			action.Invoke(e);
		});
	}

	public static void CreateMapIcon(PrefabGUID mapIcon, float3 pos, System.Action<Entity> action)
	{
		PrefabSpawnerService.SpawnWithCallback(mapIcon, pos, (e) =>
			{
				e.Add<MapIconData>();
				var mapIconData = e.Read<MapIconData>();
				mapIconData.EnemySetting = MapIconShowSettings.Global;
				mapIconData.AllySetting = MapIconShowSettings.Global;
				mapIconData.RequiresReveal = false;
				mapIconData.ShowOutsideVision = true;
				mapIconData.ShowOnMinimap = true;
				e.Write(mapIconData);
				action.Invoke(e);
			});
	}

	public static void AttachMapIconToEntity(Entity entity, Entity mapIcon)
	{
		mapIcon.Add<MapIconTargetEntity>();
		var mapIconTargetEntity = mapIcon.Read<MapIconTargetEntity>();
		mapIconTargetEntity.TargetEntity = NetworkedEntity.ServerEntity(entity);
		mapIconTargetEntity.TargetNetworkId = entity.Read<NetworkId>();
		mapIcon.Write(mapIconTargetEntity);
	}

	public static void CreateAndAttachMapIconToEntity(Entity entity, PrefabGUID mapIcon, System.Action<Entity> action)
	{
		PrefabSpawnerService.SpawnWithCallback(mapIcon, new float3(0, 0, 0), (e) =>
		{
			e.Add<MapIconData>();
			var mapIconData = e.Read<MapIconData>();
			mapIconData.EnemySetting = MapIconShowSettings.Global;
			mapIconData.AllySetting = MapIconShowSettings.Global;
			mapIconData.RequiresReveal = false;
			mapIconData.ShowOutsideVision = true;
			mapIconData.ShowOnMinimap = true;
			e.Write(mapIconData);

			e.Add<MapIconTargetEntity>();
			var mapIconTargetEntity = e.Read<MapIconTargetEntity>();
			mapIconTargetEntity.TargetEntity = NetworkedEntity.ServerEntity(entity);
			mapIconTargetEntity.TargetNetworkId = entity.Read<NetworkId>();
			e.Write(mapIconTargetEntity);
			action.Invoke(e);
		});
	}

	public static NativeList<Entity> GetStructuresFromCastleHeart(Entity castleHeartEntity)
	{
		return CastleHeartHelpers.GetConnectedTileModels(castleHeartEntity, VWorld.Server.EntityManager, Allocator.Temp);
	}

	public static bool TryGetCurrentCastleTerritory(Entity entity, out Entity territoryEntity)
	{
		return CastleTerritoryCache.TryGetCastleTerritory(entity, out territoryEntity);
	}

	public static bool TryGetCurrentCastleTerritory(Player player, out Entity territoryEntity)
	{
		return TryGetCurrentCastleTerritory(player.Character, out territoryEntity);
	}

	public static bool TryGetFloorEntityBelowEntity(Entity entity, out Entity floorEntity)
	{
		floorEntity = default;
		var entityTilePosition = entity.Read<TilePosition>();
		
		var entitiesInArea = Helper.GetEntitiesInArea(entity.Read<TileBounds>().Value, TileType.Pathfinding);
		foreach (var entityInArea in entitiesInArea)
		{
			var floorTilePosition = entityInArea.Read<TilePosition>();
			if (floorTilePosition.HeightLevel != entityTilePosition.HeightLevel)
			{
				continue;
			}
			if (entityInArea.Has<CastleFloor>() && !entityInArea.Has<CastleStairs>()) //ignoring hearts and stairs
			{
				if (entityInArea.Read<TileBounds>().Value.Contains(entityTilePosition.Tile))
				{
					floorEntity = entityInArea;
					return true;
				}
			}
		}
		return false;
	}

	public static bool TryGetFloorEntityBelowPlayer(Player player, out Entity floorEntity)
	{
		return TryGetFloorEntityBelowEntity(player.Character, out floorEntity);
	}

	public enum TerritoryAlignment
	{
		Friendly,
		Enemy,
		Neutral,
		None
	}

	public static bool IsInBase(this Player player, out Entity territory, out TerritoryAlignment territoryAlignment, bool requireRoom = false)
	{
		return IsInBase(player.Character, out territory, out territoryAlignment, requireRoom);
	}

	public static bool IsInBase(Entity entity, out Entity territory, out TerritoryAlignment territoryAlignment, bool requireRoom = false)
	{
		territoryAlignment = TerritoryAlignment.None;
		if (TryGetCurrentCastleTerritory(entity, out territory))
		{
			var heart = territory.Read<CastleTerritory>().CastleHeart;
			if (heart.Exists())
			{
				if (Team.IsAllies(heart.Read<Team>(), entity.Read<Team>()))
				{
					territoryAlignment = TerritoryAlignment.Friendly;
				}
				else
				{
					territoryAlignment = TerritoryAlignment.Enemy;
				}
			}
			else
			{
				territoryAlignment = TerritoryAlignment.Neutral;
			}
			
			if (!requireRoom)
			{
				return true;
			}
			else
			{
				if (TryGetFloorEntityBelowEntity(entity, out var floorEntity) && floorEntity.Has<CastleRoomConnection>())
				{
					return floorEntity.Read<CastleRoomConnection>().RoomEntity._Entity.Read<CastleRoom>().IsEnclosedRoom;
				}
				return false;
			}
		}

		return false;
	}

	public static bool TryGetEntityFromNetworkId(NetworkId networkId, out Entity entity)
	{
		var singleton = Core.networkIdLookupEntity.Read<NetworkIdSystem.Singleton>();
		return singleton.GetNetworkIdLookupRW().TryGetValue(networkId, out entity);
	}

	public static string FormatTime(double totalSeconds)
	{
		int hours = (int)(totalSeconds / 3600);
		int minutes = (int)((totalSeconds % 3600) / 60);
		int seconds = (int)totalSeconds % 60;

		string formattedTime = "";

		if (hours > 0)
		{
			formattedTime += $"{hours}h";
		}

		if (minutes > 0)
		{
			formattedTime += $"{minutes}m";
		}

		// Seconds are always included
		formattedTime += $"{seconds}s";

		return formattedTime.Trim(); // Remove any trailing space
	}


	public static void CreateAndEquipServantGear(Entity servantCoffin)
	{
		var servantGear = new List<PrefabGUID>()
		{
			Prefabs.Item_Weapon_Spear_Legendary_T08,
			Prefabs.Item_Boots_T09_Dracula_Warrior,
			Prefabs.Item_Chest_T09_Dracula_Warrior,
			Prefabs.Item_Gloves_T09_Dracula_Warrior,
			Prefabs.Item_Legs_T09_Dracula_Warrior,
			Prefabs.Item_MagicSource_General_T08_Illusion,
		};
		var servantCoffinStation = servantCoffin.Read<ServantCoffinstation>();
		var coffinTeam = servantCoffin.Read<Team>();
		var servant = servantCoffinStation.ConnectedServant._Entity;
		AddItemToInventory(servant, servantGear[0], 1, out Entity item);
		AddItemToInventory(servant, servantGear[1], 1, out item);
		AddItemToInventory(servant, servantGear[2], 1, out item);
		AddItemToInventory(servant, servantGear[3], 1, out item);
		AddItemToInventory(servant, servantGear[4], 1, out item);
		AddItemToInventory(servant, servantGear[5], 1, out item);
	}

	public static unsafe bool IsRaidHour()
	{
		var sys = Core.shapeshiftSystem;
		var state = *sys.SystemState._SystemHandle.m_Entity.Read<SystemInstance>().state;
		var conditionChecker = sys._ConditionCheckerFactory.Build(ref state);
		var condition = new TargetBoolCondition();
		var result = conditionChecker.IsDuringCastlePvPTime(ref condition);
		return result || Core.serverGameSettingsSystem._Settings.CastleDamageMode == CastleDamageMode.Always;
	}


	public static unsafe bool PlayerHasSoulShard(Player player)
	{
		var equippedNeck = player.Character.Read<Equipment>().GrimoireSlot.SlotEntity._Entity;
		if (equippedNeck.Exists() && ShardData.ShardNecklaces.Contains(equippedNeck.GetPrefabGUID()))
		{
			return true;
		}
		var buffer = player.Inventory.ReadBuffer<InventoryBuffer>();
		foreach (var item in buffer)
		{
			if (ShardData.ShardNecklaces.Contains(item.ItemType))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsCastleBeingRaided(Entity heart)
	{
		if (heart.Exists())
		{
			var pylonStation = heart.Read<CastleHeart>();
			if (pylonStation.ActiveEvent == CastleHeartEvent.Attacked || pylonStation.ActiveEvent == CastleHeartEvent.Breached || pylonStation.ActiveEvent == CastleHeartEvent.Raided)
			{
				return true;
			}
		}
		
		return false;
	}

	public static bool IsCastleBeingRaided(ComponentLookup<CastleHeart> componentLookup, Entity heart)
	{
		if (heart.Exists())
		{
			var castleHeart = componentLookup[heart];
			
			if (castleHeart.ActiveEvent == CastleHeartEvent.Attacked || castleHeart.ActiveEvent == CastleHeartEvent.Breached || castleHeart.ActiveEvent == CastleHeartEvent.Raided)
			{
				return true;
			}
		}

		return false;
	}

	public static void BanPlayer(Player player)
	{
		var sys = VWorld.Server.GetExistingSystemManaged<KickBanSystem_Server>();
		sys._LocalBanList.RefreshLocal(true);
		sys._LocalBanList.Add(player.SteamID);
		sys._LocalBanList.Save();
		KickPlayer(player, ConnectionStatusChangeReason.Banned);
	}

	public static void UnbanPlayer(Player player)
	{
		var sys = VWorld.Server.GetExistingSystemManaged<KickBanSystem_Server>();
		sys._LocalBanList.RefreshLocal(true);
		sys._LocalBanList.Remove(player.SteamID);
		sys._LocalBanList.Save();
	}

	public static WorldRegionType GetWorldRegionFromPosition(float3 position)
	{
		// UpdateUserWorldRegionSystem is now a struct ISystem — use GetExistingSystem + GetUnsafeSystemRef
		var handle = VWorld.Server.GetExistingSystem<UpdateUserWorldRegionSystem>();
		if (handle == SystemHandle.Null) return WorldRegionType.None;
		ref var sys = ref VWorld.Server.Unmanaged.GetUnsafeSystemRef<UpdateUserWorldRegionSystem>(handle);
		var mapRegions = sys._MapRegionCache;
		foreach (var mapRegion in mapRegions)
		{
			if (!mapRegion.PolygonData.PolygonBounds.Contains(position))
			{
				continue;
			}
			else
			{
				var buffer = mapRegion.Entity.ReadBuffer<WorldRegionPolygonVertex>();
				if (IsPointInPolygon(position.xz, buffer))
				{
					return mapRegion.PolygonData.WorldRegion;
				}
			}
		}
		return WorldRegionType.None;
	}

	private static bool IsPointInPolygon(float2 point, DynamicBuffer<WorldRegionPolygonVertex> vertices)
	{
		bool inside = false;
		for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
		{
			if ((vertices[i].VertexPos.y > point.y) != (vertices[j].VertexPos.y > point.y) && point.x < (vertices[j].VertexPos.x - vertices[i].VertexPos.x) * (point.y - vertices[i].VertexPos.y) / (vertices[j].VertexPos.y - vertices[i].VertexPos.y) + vertices[i].VertexPos.x)
			{
				inside = !inside;
			}
		}

		return inside;
	}

	public static unsafe Entity FindSystemEntityByName(string name)
	{
		var systems = Helper.GetEntitiesByComponentTypes<SystemInstance>(EntityQueryOptions.IncludeSystems);
		foreach (var sys in systems)
		{
			var sysInstance = sys.Read<SystemInstance>();
			var state = *sysInstance.state;
			if (state.DebugName.ToString() == "ProjectM.Gameplay.Scripting.RadialZoneSystem_Curse_Server")
			{
				return sys;
			}
		}
		return Entity.Null;
	}

	public static bool IsDayTime()
	{
		if (Core.dayNightCycleEntity.Exists())
		{
			var dayNightCycle = Core.dayNightCycleEntity.Read<DayNightCycle>();
			return dayNightCycle.TimeOfDay == TimeOfDay.Day;
		}
		return false;
	}

	public static int GetTilePositionFromWorldPosition(float worldPosition)
	{
		int offset = 6400;
		int tilePosition = Mathf.RoundToInt((worldPosition * 2) + offset);

		return tilePosition;
	}

	public static int2 GetTilePositionFromWorldPosition(float2 worldPosition)
	{
		int offsetX = 6400;
		int offsetY = 6400;
		int tileX = Mathf.RoundToInt((worldPosition.x * 2) + offsetX);
		int tileZ = Mathf.RoundToInt((worldPosition.y * 2) + offsetY);

		return new int2(tileX, tileZ);
	}

	public static int2 GetTilePositionFromWorldPosition(float3 worldPosition)
	{
		int offsetX = 6400;
		int offsetY = 6400;
		int tileX = Mathf.RoundToInt((worldPosition.x * 2) + offsetX);
		int tileZ = Mathf.RoundToInt((worldPosition.z * 2) + offsetY);

		return new int2(tileX, tileZ);
	}

	//this seems buggy
	public static float3 GetWorldPositionFromTilePosition(int2 tilePosition)
	{
		int offsetX = 6400;
		int offsetY = 6400;

		// Calculate the world position by reversing the operations
		float worldX = (tilePosition.x - offsetX) / 2.0f;
		float worldZ = (tilePosition.y - offsetY) / 2.0f;

		return new float3(worldX, 0, worldZ); // Assuming y remains 0
	}

	public static int2 GetChunkFromWorldPosition(float3 worldPosition)
	{
		int chunkSize = 320;
		// Calculate chunk position by dividing world position by chunk size
		int chunkX = Mathf.FloorToInt(worldPosition.x * 2 / chunkSize) + 20;
		int chunkZ = Mathf.FloorToInt(worldPosition.z * 2 / chunkSize) + 20;

		return new int2(chunkX, chunkZ);
	}

	public static void PlaySequenceOnPosition(float3 pos, SequenceGUID guid)
	{
		Core.serverGameManager.PlaySequenceOnPosition(pos, new quaternion(), guid);
	}
}

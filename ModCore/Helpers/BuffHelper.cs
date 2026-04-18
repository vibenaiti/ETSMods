using System.Collections.Generic;
using System.Threading;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using ModCore.Data;
using ModCore.Models;
using ModCore.Services;
using Unity.Entities;
using ModCore.Factories;
using Stunlock.Core;

namespace ModCore.Helpers;

public static partial class Helper
{
	public const float NO_DURATION = 0;
	public const float DEFAULT_DURATION = -1;

	public static PrefabGUID CustomBuff1 = Prefabs.Buff_BloodQuality_T01_OLD;
	public static PrefabGUID CustomBuff2 = Prefabs.Buff_BloodQuality_T02_OLD;
	public static PrefabGUID CustomBuff3 = Prefabs.Buff_BloodQuality_T03_OLD;
	public static PrefabGUID CustomBuff4 = Prefabs.Buff_BloodQuality_T04_OLD;
	public static PrefabGUID CustomBuff5 = Prefabs.Buff_BloodQuality_T05_OLD;

	public static void ModifyBuff(Entity buffEntity, BuffModificationTypes buffModificationTypes, bool overwrite = false)
	{
		buffEntity.Add<BuffModificationFlagData>();
		var buffModificationFlagData = buffEntity.Read<BuffModificationFlagData>();
		if (overwrite)
		{
			buffModificationFlagData.ModificationTypes = (long)BuffModificationTypes.None;
		}
		buffModificationFlagData.ModificationTypes |= (long)buffModificationTypes;
		buffEntity.Write(buffModificationFlagData);
	}

	public static void ModifyBuffAggro(Entity buffEntity, DisableAggroBuffMode mode)
	{
		buffEntity.Add<DisableAggroBuff>();
		buffEntity.Write(new DisableAggroBuff
		{
			Mode = mode
		});
	}

	public static void RemoveBuffModifications(Entity buffEntity, BuffModificationTypes buffModificationTypes)
	{
		buffEntity.Add<BuffModificationFlagData>();
		var buffModificationFlagData = buffEntity.Read<BuffModificationFlagData>();
		buffModificationFlagData.ModificationTypes &= ~(long)buffModificationTypes;
		buffEntity.Write(buffModificationFlagData);
	}

	public static void ApplyStatModifier(Entity buffEntity, ModifyUnitStatBuff_DOTS statMod, bool clearOld = true)
	{
		var buffer = VWorld.Server.EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
		if (clearOld)
		{
			buffer.Clear();
		}
		buffer.Add(statMod);
	}

	public static void SetStatThroughBuff(Entity buffEntity, UnitStatType unitStatType, float value)
	{
		Helper.ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
		{
			Id = ModificationIdFactory.NewId(),
			ModificationType = ModificationType.Set,
			Priority = 100,
			Value = value,
			Modifier = 1,
			StatType = unitStatType
		}, false);
	}

	public static List<Entity> GetTeleportBlockingBuffs(Player player)
	{
		return GetTeleportBlockingBuffs(player.Character);
	}

	public static List<Entity> GetTeleportBlockingBuffs(Entity entity)
	{
		var buffs = Helper.GetAllBuffs(entity);
		List<Entity> teleportBlockingBuffs = new();
		foreach (var buff in buffs)
		{
			if (buff.Has<BuffModificationFlagData>())
			{
				var buffModificationFlagData = buff.Read<BuffModificationFlagData>();
				if ((buffModificationFlagData.ModificationTypes & (long)BuffModificationTypes.DisableHeightCorrection) != 0)
				{
					teleportBlockingBuffs.Add(buff);
				}
			}
		}
		return teleportBlockingBuffs;
	}

	public static List<Entity> GetAllBuffs(Player player)
	{
		return GetAllBuffs(player.Character);
	}

	public static List<Entity> GetAllBuffs(Entity entity)
	{
		List<Entity> entityBuffs = new List<Entity>();
		if (entity.Has<BuffBuffer>())
		{
			var buffs = entity.ReadBuffer<BuffBuffer>();
			foreach (var buff in buffs)
			{
				if (buff.Entity.Read<EntityOwner>().Owner == entity)
				{
					entityBuffs.Add(buff.Entity);
				}
			}
		}

/*		var buffEntities = Helper.GetEntitiesByComponentTypes<Buff, PrefabGUID>();
		foreach (var buffEntity in buffEntities)
		{
			if (buffEntity.Read<EntityOwner>().Owner == entity)
			{
				entityBuffs.Add(buffEntity);
			}
		}*/
		return entityBuffs;
	}

	public static bool HasBuff(Entity entity, PrefabGUID buff)
	{
		return BuffUtility.HasBuff(VWorld.Server.EntityManager, entity, buff.ToIdentifier());
	}

	public static bool HasBuff(this Player player, PrefabGUID buff)
	{
		return HasBuff(player.Character, buff);
	}

	public static bool TryGetBuff(Entity entity, PrefabGUID buff, out Entity buffEntity)
	{
		return BuffUtility.TryGetBuff(VWorld.Server.EntityManager, entity, buff.ToIdentifier(), out buffEntity);
	}

	public static bool TryGetBuff(Player player, PrefabGUID buff, out Entity buffEntity)
	{
		return TryGetBuff(player.Character, buff, out buffEntity);
	}

	public static void SetBuffDuration(Entity buffEntity, float duration)
	{
		var lifetime = buffEntity.Read<LifeTime>();
		var age = buffEntity.Read<Age>();
		var originalBuffDuration = Helper.GetPrefabEntityByPrefabGUID(buffEntity.GetPrefabGUID()).Read<LifeTime>().Duration;
		if (duration < originalBuffDuration)
		{
			var delta = originalBuffDuration - duration;
			age.Value += delta;
			buffEntity.Write(age);
		}
		
		lifetime.Duration = duration;
		buffEntity.Write(lifetime);
	}

	public static void DestroyBuff(Entity buff)
	{
		DestroyUtility.Destroy(VWorld.Server.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);
	}

	public static void RemoveBuff(Entity unit, PrefabGUID buff)
	{
		if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, unit, buff.ToIdentifier(), out var buffEntity))
		{
			DestroyBuff(buffEntity);
		}
	}

	public static void RemoveBuff(Player player, PrefabGUID buff)
	{
		RemoveBuff(player.Character, buff);
	}

	public static void RemoveAllShieldBuffs(Player player)
	{
		var buffer = player.Character.ReadBuffer<BuffBuffer>();
		foreach (var buff in buffer)
		{
			if (buff.Entity.Has<AbsorbBuff>())
			{
				DestroyBuff(buff.Entity);
			}
		}
	}

	public static void CompletelyRemoveAbilityBarFromBuff(Entity buffEntity)
	{
		var buffer = VWorld.Server.EntityManager.AddBuffer<ReplaceAbilityOnSlotBuff>(buffEntity);
		for (var i = 0; i < 8; i++)
		{
			buffer.Add(new ReplaceAbilityOnSlotBuff
			{
				Slot = i,
				CastBlockType = GroupSlotModificationCastBlockType.WholeCast,
				NewGroupId = PrefabGUID.Empty,
				ReplaceGroupId = PrefabGUID.Empty,
				Priority = 100,
				Target = ReplaceAbilityTarget.BuffOwner
			});
		}
		buffEntity.Add<ReplaceAbilityOnSlotData>();
	}

	public static void RemoveNewAbilitiesFromBuff(Entity buffEntity)
	{
		var buffer = buffEntity.ReadBuffer<ReplaceAbilityOnSlotBuff>();
		buffer.Clear();
	}

	public static void FixIconForShapeshiftBuff(Player player, Entity buffEntity, PrefabGUID abilityGroupPrefab)
	{
		var abilityOwner = buffEntity.Read<AbilityOwner>();
		var abilityBarShared = player.Character.Read<AbilityBar_Shared>();
		AbilityUtilitiesServer.ValidateAbilityExists(VWorld.Server.EntityManager, ref Core.prefabCollectionSystem.PrefabLookupMap, player.Character, abilityGroupPrefab, out Entity abilityGroupEntity);
		abilityOwner.Ability = abilityBarShared.CastAbility;
		abilityOwner.AbilityGroup = abilityGroupEntity;
		buffEntity.Write(abilityOwner); //shapeshift buffs forcibly applied without casting have an annoying white icon unless you set this
	}

	public static bool BuffPlayer(Player player, PrefabGUID buff, out Entity buffEntity, float duration = DEFAULT_DURATION, bool attemptToPersistThroughDeath = false)
	{
		return BuffEntity(player.Character, buff, out buffEntity, duration, attemptToPersistThroughDeath);
	}

	public static void RemoveBuffEffects(Entity buffEntity, HashSet<GameplayEventTypeEnum> eventTypes, bool keep)
	{
		var gameplayEventListenerBuffer = buffEntity.ReadBuffer<GameplayEventListeners>();

		for (int i = gameplayEventListenerBuffer.Length - 1; i >= 0; i--)
		{
			var gameplayEventListener = gameplayEventListenerBuffer[i];
			bool typeFound = eventTypes.Contains(gameplayEventListener.GameplayEventType);

			if ((keep && !typeFound) || (!keep && typeFound))
			{
				gameplayEventListenerBuffer.RemoveAt(i);
			}
		}
	}

	public static bool BuffEntity(Entity entity, PrefabGUID buff, out Entity buffEntity, float duration = DEFAULT_DURATION, bool attemptToPersistThroughDeath = false)
	{
		var buffEvent = new ApplyBuffDebugEvent()
		{
			BuffPrefabGUID = buff
		};
		var fromCharacter = new FromCharacter()
		{
			User = PlayerService.GetAnyPlayer().User,
			Character = entity
		};
		
		if (!HasBuff(entity, buff))
		{
			Core.debugEventsSystem.ApplyBuff(fromCharacter, buffEvent);
		}
		
		if (TryGetBuff(entity, buff, out buffEntity))
		{
			if (attemptToPersistThroughDeath || duration == Helper.NO_DURATION)
			{
				buffEntity.Add<Buff_Persists_Through_Death>();
				buffEntity.Remove<Buff_Destroy_On_Owner_Death>();

				if (buffEntity.Has<RemoveBuffOnGameplayEventEntry>())
				{
					var buffer = buffEntity.ReadBuffer<RemoveBuffOnGameplayEventEntry>();
					buffer.Clear();
				}
			}
			else if (!attemptToPersistThroughDeath)
			{
				buffEntity.Remove<Buff_Persists_Through_Death>();
				buffEntity.Add<Buff_Destroy_On_Owner_Death>();
			}

			if (duration == NO_DURATION)
			{
				if (buffEntity.Has<LifeTime>())
				{
					var lifetime = buffEntity.Read<LifeTime>();
					buffEntity.Remove<Age>();
					lifetime.Duration = 0;
					lifetime.EndAction = LifeTimeEndAction.None;
					buffEntity.Write(lifetime);
				}
			} 
			else if (duration > 0)
			{
				if (buffEntity.Has<Age>()) //if we try to buff with a buff they already have, reset the age
				{
					var age = buffEntity.Read<Age>();
					age.Value = 0;
					buffEntity.Write(age);
				}
				
				if (!buffEntity.Has<LifeTime>())
				{
					buffEntity.Add<LifeTime>();
				}
				buffEntity.Write(new LifeTime
				{
					EndAction = LifeTimeEndAction.Destroy,
					Duration = duration
				});
			}
			return true;
		}
		return false;
	}

	public static void ClearDelayedBuffs(Entity unit, HashSet<PrefabGUID> buffsToRemove)
	{
		if (unit.Has<BuffBuffer>())
		{
			var buffs = unit.ReadBuffer<BuffBuffer>();

			foreach (var buff in buffs)
			{
				if (buffsToRemove.Contains(buff.PrefabGuid))
				{
					Helper.DestroyBuff(buff.Entity);
				}
			}
		}
	}

	public static void ClearExtraBuffs(Entity unit, ResetOptions resetOptions = default)
	{
		if (unit.Has<BuffBuffer>())
		{
			var buffs = unit.ReadBuffer<BuffBuffer>();

			foreach (var buff in buffs)
			{
				if (ShouldDestroyBuff(buff.PrefabGuid, resetOptions))
				{
					Helper.DestroyBuff(buff.Entity);
				}
			}

			var action = () => ClearDelayedBuffs(unit, ResetBuffPrefabs.DelayedBuffs);
			ActionScheduler.RunActionOnceAfterDelay(action, 0.05f);
		}
	}

	private static bool ShouldDestroyBuff(PrefabGUID buff, ResetOptions resetOptions)
	{
		if (ResetBuffPrefabs.BuffsToKeep.Contains(buff))
		{
			return false;
		}

		if (resetOptions.BuffsToIgnore.Contains(buff))
		{
			return false;
		}

		if (ResetBuffPrefabs.ConsumableBuffs.Contains(buff))
		{
			return resetOptions.RemoveConsumables;
		}

		if (ResetBuffPrefabs.ShapeshiftBuffs.Contains(buff))
		{
			return resetOptions.RemoveShapeshifts;
		}

		return true;
	}

	public static void ClearConsumablesAndShards(Entity player)
	{
		ClearConsumables(player);
		ClearShards(player);
	}

	public static void ClearConsumables(Entity player)
	{
		var buffs = VWorld.Server.EntityManager.GetBuffer<BuffBuffer>(player);
		var stringsToRemove = new List<string>
		{
			"Consumable",
		};

		foreach (var buff in buffs)
		{
			bool shouldRemove = false;
			foreach (string word in stringsToRemove)
			{
				if (buff.PrefabGuid.LookupName().Contains(word))
				{
					shouldRemove = true;
					break;
				}
			}

			if (shouldRemove)
			{
				DestroyUtility.Destroy(VWorld.Server.EntityManager, buff.Entity, DestroyDebugReason.TryRemoveBuff);
			}
		}
	}

	public static void ClearShards(Entity player)
	{
		var buffs = VWorld.Server.EntityManager.GetBuffer<BuffBuffer>(player);
		var stringsToRemove = new List<string>
		{
			"UseRelic",
		};

		foreach (var buff in buffs)
		{
			bool shouldRemove = false;
			foreach (string word in stringsToRemove)
			{
				if (buff.PrefabGuid.LookupName().Contains(word))
				{
					shouldRemove = true;
					break;
				}
			}

			if (shouldRemove)
			{
				DestroyUtility.Destroy(VWorld.Server.EntityManager, buff.Entity, DestroyDebugReason.TryRemoveBuff);
			}
		}
	}

	//to be called when we detect the buff being removed
	public static void PreventBuffDestruction(Entity buffEntity)
	{
		buffEntity.Remove<DestroyTag>();

		var destroyData = buffEntity.Read<DestroyData>();
		destroyData.DestroyReason = DestroyReason.Default;
		buffEntity.Write(destroyData);

		var destroyState = buffEntity.Read<DestroyState>();
		destroyState.Value = DestroyStateEnum.NotDestroyed;
		buffEntity.Write(destroyState);
	}

	public static void RemoveBloodDrain(Entity buffEntity)
	{
		buffEntity.Add<ModifyBloodDrainBuff>();
		var modifyBloodDrainBuff = new ModifyBloodDrainBuff()
		{
			AffectBloodValue = true,
			AffectIdleBloodValue = true,
			BloodValue = 0,
			BloodIdleValue = 0,

			ModificationPriority = 100,
			ModificationIdlePriority = 100,

			ModificationType = ModificationType.Set,
			ModificationIdleType = ModificationType.Set,

			IgnoreIdleDrainWhileActive = true,
		};
		buffEntity.Write(modifyBloodDrainBuff);
	}
}

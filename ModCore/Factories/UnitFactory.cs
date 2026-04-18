using System;
using System.Collections.Generic;
using System.Threading;
using ProjectM;
using ProjectM.CastleBuilding;
using ModCore.Data;
using ModCore.Events;
using ModCore.Helpers;
using ModCore.Models;
using ModCore.Services;
using Unity.Entities;
using Unity.Mathematics;
using static ModCore.Configs.ConfigDtos;
using static ModCore.Factories.UnitFactory;
using Stunlock.Core;

namespace ModCore.Factories;
public static class UnitFactory
{
	public class SpawnedUnit
	{
		public Unit Unit;
		public float3 SpawnPosition;
		public Player Player;
		public SpawnedUnit(Unit unit, float3 spawnPosition, Player player = null)
		{
			Unit = unit;
			SpawnPosition = spawnPosition;
			Player = player;
		}
	}

	public static Dictionary<int, SpawnedUnit> HashToUnit = new Dictionary<int, SpawnedUnit>();
	public static Dictionary<string, List<Timer>> timersByCategory = new Dictionary<string, List<Timer>>();
	public static Dictionary<Unit, Entity> UnitToEntity = new Dictionary<Unit, Entity>();

	static UnitFactory()
	{
		GameEvents.OnUnitDeath += HandleOnUnitDeath;
	}

	public static void HandleOnUnitDeath(Entity unitEntity, DeathEvent deathEvent)
	{
		RespawnUnitIfEligible(unitEntity);
	}

	private static void RespawnUnitIfEligible(Entity unitEntity)
	{
		if (TryGetSpawnedUnitFromEntity(unitEntity, out SpawnedUnit spawnedUnit))
		{
			if (spawnedUnit.Unit.RespawnTime != -1)
			{
				spawnedUnit.Unit.SpawnDelay = (int)spawnedUnit.Unit.RespawnTime;
				SpawnUnit(spawnedUnit.Unit, spawnedUnit.SpawnPosition, spawnedUnit.Player);
			}
		}
	}


	public static bool TryGetSpawnedUnitFromEntity(Entity unitEntity, out SpawnedUnit unit)
	{
		if (unitEntity.Has<ResistanceData>() && unitEntity.Has<CanFly>())
		{
			var resistanceData = unitEntity.Read<ResistanceData>();
			var hash = (int)(resistanceData.FireResistance_RedcuedIgiteChancePerRating);
			return HashToUnit.TryGetValue(hash, out unit);
		}
		unit = default;
		return false;
	}

	public static void SpawnUnit(Unit unit, float3 position, Player player = null)
	{;
		Action spawnAction = () =>
		{
			SpawnedUnit spawnedUnit = new SpawnedUnit(unit, position, player);

			PrefabSpawnerService.SpawnWithCallback(unit.PrefabGuid, position, (Action<Entity>)(e =>
			{
				var hash = e.GetHashCode();
				HashToUnit[hash] = spawnedUnit;
				StoreMetaDataOnUnit(unit, e, position, player);
				SetHealth(unit, e);
				if (Helper.BuffEntity(e, Helper.CustomBuff1, out Entity buffEntity, (float)Helper.NO_DURATION, true))
				{
					if (unit.Level != -1 && e.Has<UnitLevel>())
					{
						buffEntity.Add<ModifyUnitLevelBuff>();
						buffEntity.Write(new ModifyUnitLevelBuff
						{
							UnitLevel = unit.Level,
							Priority = 100
						});
					}

					if (unit.AggroRadius != -1)
					{
						ModifyAggroRadius(unit, buffEntity); //this increases the boss range, but keeps players in combat :(
					}
					AddBuffModifications(unit, buffEntity);
					if (unit.KnockbackResistance)
					{
						GiveKnockbackResistance(unit, e, buffEntity);
					}
					if (!unit.DrawsAggro)
					{
						DisableAggro(buffEntity);
					}
					unit.Modify(e, buffEntity);

					if (e.Has<BloodConsumeSource>() && !e.Has<VBloodUnit>())
					{
						var bloodConsumeSource = e.Read<BloodConsumeSource>();
						bloodConsumeSource.CanBeConsumed = false;
						e.Write(bloodConsumeSource);
					}

					if (unit.SoftSpawn && unit.SpawnDelay > 0)
					{
						Helper.BuffEntity(e, Prefabs.Buff_General_VampireMount_Dead, out var softSpawnBuff, unit.SpawnDelay);
						Helper.ModifyBuff(softSpawnBuff, BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.TargetSpellImpaired | BuffModificationTypes.MovementImpair | BuffModificationTypes.RelocateImpair | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.BehaviourImpair);
					}
				}
				else
				{
					unit.Modify(e);
				}
				UnitToEntity[unit] = e;
				if (!unit.SoftSpawn)
				{
					GameEvents.OnUnitSpawn?.Invoke(unit);
				}
				else if (unit.SpawnDelay > 0)
				{
					var action = () => GameEvents.OnUnitSpawn?.Invoke(unit);
					ActionScheduler.RunActionOnceAfterDelay(action, unit.SpawnDelay);
				}
				
			}), 0, -1);
		};

		if (unit.SpawnDelay >= 0)
		{
			GameEvents.OnDelayedUnitSpawn?.Invoke(unit);
		}

		if (unit.SpawnDelay >= 0 && !unit.SoftSpawn)
		{	
			// Schedule the spawn action after the specified delay
			var timer = ActionScheduler.RunActionOnceAfterDelay(spawnAction, unit.SpawnDelay);
			AddTimerToCategory(timer, unit.Category);
		}
		else
		{
			// Execute immediately if no delay is specified
			spawnAction.Invoke();
		}
	}

	public static void SpawnUnitWithCallback(Unit unit, float3 position, Action<Entity> postActions, Player player = null)
	{
		Action spawnAction = () =>
		{
			SpawnedUnit spawnedUnit = new SpawnedUnit(unit, position, player);

			PrefabSpawnerService.SpawnWithCallback(unit.PrefabGuid, position, (Action<Entity>)(e =>
			{
				var hash = e.GetHashCode();
				HashToUnit[hash] = spawnedUnit;
				StoreMetaDataOnUnit(unit, e, position, player);
				SetHealth(unit, e);
				if (unit.MaxDistanceFromPreCombatPosition != -1)
				{
					var aggro = e.Read<AggroConsumer>();
					aggro.MaxDistanceFromPreCombatPosition = unit.MaxDistanceFromPreCombatPosition;
					e.Write(aggro);
				}
				if (Helper.BuffEntity(e, Helper.CustomBuff1, out Entity buffEntity, (float)Helper.NO_DURATION, true))
				{
					if (unit.Level != -1 && e.Has<UnitLevel>())
					{
						buffEntity.Add<ModifyUnitLevelBuff>();
						buffEntity.Write(new ModifyUnitLevelBuff
						{
							UnitLevel = unit.Level,
							Priority = 100
						});
					}

					if (unit.AggroRadius != -1)
					{
						ModifyAggroRadius(unit, buffEntity); //this increases the boss range, but keeps players in combat :(
					}
					AddBuffModifications(unit, buffEntity);
					if (unit.KnockbackResistance)
					{
						GiveKnockbackResistance(unit, e, buffEntity);
					}
					if (!unit.DrawsAggro)
					{
						DisableAggro(buffEntity);
					}
					unit.Modify(e, buffEntity);

					if (e.Has<BloodConsumeSource>() && !e.Has<VBloodUnit>())
					{
						var bloodConsumeSource = e.Read<BloodConsumeSource>();
						bloodConsumeSource.CanBeConsumed = false;
						e.Write(bloodConsumeSource);
					}

					if (unit.SoftSpawn && unit.SpawnDelay > 0)
					{
						Helper.BuffEntity(e, Prefabs.Buff_General_VampireMount_Dead, out var softSpawnBuff, unit.SpawnDelay);
						Helper.ModifyBuff(softSpawnBuff, BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.TargetSpellImpaired | BuffModificationTypes.MovementImpair | BuffModificationTypes.RelocateImpair | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.BehaviourImpair);
					}
				}
				else
				{
					unit.Modify(e);
				}
				UnitToEntity[unit] = e;
				if (!unit.SoftSpawn)
				{
					GameEvents.OnUnitSpawn?.Invoke(unit);
				}
				else if (unit.SpawnDelay > 0)
				{
					var action = () => GameEvents.OnUnitSpawn?.Invoke(unit);
					ActionScheduler.RunActionOnceAfterDelay(action, unit.SpawnDelay);
				}
				postActions(e);
			}), 0, -1);
		};

		if (unit.SpawnDelay >= 0)
		{
			GameEvents.OnDelayedUnitSpawn?.Invoke(unit);
		}

		if (unit.SpawnDelay >= 0 && !unit.SoftSpawn)
		{
			// Schedule the spawn action after the specified delay
			var timer = ActionScheduler.RunActionOnceAfterDelay(spawnAction, unit.SpawnDelay);
			AddTimerToCategory(timer, unit.Category);
		}
		else
		{
			// Execute immediately if no delay is specified
			spawnAction.Invoke();
		}
	}

	private static void AddTimerToCategory(Timer timer, string category)
	{
		if (!timersByCategory.ContainsKey(category))
		{
			timersByCategory[category] = new List<Timer>();
		}
		timersByCategory[category].Add(timer);
	}

	public static void DisposeTimers(string category)
	{
		if (timersByCategory.TryGetValue(category, out List<Timer> timers))
		{
			foreach (var timer in timers)
			{
				if (timer != null)
				{
					timer.Dispose();
				}
			}
			timersByCategory[category].Clear();
		}
	}

	private static void ModifyAggroRadius(Unit unit, Entity buffEntity)
	{
		buffEntity.Add<ModifyAggroRangesBuff>();
		buffEntity.Write(new ModifyAggroRangesBuff
		{
			AggroCircleRadiusFactor = unit.AggroRadius,
			AggroConeRadiusFactor = unit.AggroRadius,
			AlertCircleRadiusFactor = unit.AggroRadius,
			AlertConeRadiusFactor = unit.AggroRadius
		});
	}

	public static float StringToFloatHash(string input)
	{
		float hash = 0;
		float charMultiplier = 31; // Prime number as multiplier

		foreach (char c in input)
		{
			hash = hash * charMultiplier + (int)c;
		}

		return hash;
	}

    public static float3 GetSpawnPositionOfEntity(Entity entity)
    {
        if (entity.Has<ResistanceData>())
        {
            var resistanceData = entity.Read<ResistanceData>();
            return new float3(resistanceData.HolyResistance_DamageAbsorbPerRating, resistanceData.HolyResistance_DamageReductionPerRating, resistanceData.SilverResistance_DamageReductionPerRating);
        }
        else
        {
            return float3.zero;
        }
    }


	private static void StoreMetaDataOnUnit(Unit unit, Entity e, float3 position, Player player = null)
	{
		e.Add<NameableInteractable>();
		e.Write(new NameableInteractable
		{
			Name = unit.Category
		});
		e.Add<ResistanceData>();
		var resistanceData = e.Read<ResistanceData>();
		resistanceData.FireResistance_DamageReductionPerRating = unit.Team;
		resistanceData.FireResistance_RedcuedIgiteChancePerRating = e.GetHashCode(); //going to use position to identify spawn point 
		resistanceData.GarlicResistance_IncreasedExposureFactorPerRating = StringToFloatHash(unit.Category);
        resistanceData.HolyResistance_DamageAbsorbPerRating = position.x;
        resistanceData.HolyResistance_DamageReductionPerRating = position.y;
        resistanceData.SilverResistance_DamageReductionPerRating = position.z;
		e.Write(resistanceData);
		if (player != null)
		{
			e.Write(player.Character.Read<TeamReference>());
			e.Write(player.Character.Read<Team>());
		}
	}

	private static void DisableAggro(Entity buffEntity)
	{
		buffEntity.Add<DisableAggroBuff>();
		buffEntity.Write(new DisableAggroBuff
		{
			Mode = DisableAggroBuffMode.OthersDontAttackTarget
		});
	}

	private static void GiveKnockbackResistance(Unit unit, Entity e, Entity buffEntity)
	{
		e.Add<BuffResistances>();
		if (unit.IsRooted)
		{
			e.Write(new BuffResistances
			{
				SettingsEntity = new ModifiableEntity(Helper.GetPrefabEntityByPrefabGUID(Prefabs.BuffResistance_Golem)),
				InitialSettingGuid = Prefabs.BuffResistance_Golem
			});
		}
		else
		{
			e.Write(new BuffResistances
			{
				SettingsEntity = new ModifiableEntity(Helper.GetPrefabEntityByPrefabGUID(Prefabs.BuffResistance_UberMobNoKnockbackOrGrab)),
				InitialSettingGuid = Prefabs.BuffResistance_UberMobNoKnockbackOrGrab
			});
		}
	}

	private static void AddBuffModifications(Unit unit, Entity buffEntity)
	{
		buffEntity.Add<BuffModificationFlagData>();
		BuffModificationTypes modificationTypes = BuffModificationTypes.None;
		if (!unit.DynamicCollision)
		{
			modificationTypes |= BuffModificationTypes.DisableDynamicCollision;
		}
		if (!unit.MapCollision)
		{
			modificationTypes |= BuffModificationTypes.DisableMapCollision;
		}
		if (unit.IsRooted)
		{
			modificationTypes |= BuffModificationTypes.MovementImpair | BuffModificationTypes.RelocateImpair;
		}
		if (unit.IsImmaterial)
		{
			modificationTypes |= BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable;
		}
		if (unit.IsInvulnerable)
		{
			modificationTypes |= BuffModificationTypes.Invulnerable;
		}
		if (!unit.IsTargetable)
		{
			modificationTypes |= BuffModificationTypes.TargetSpellImpaired;
		}
		if (unit.AbilityCastImpair)
		{
			modificationTypes |= BuffModificationTypes.AbilityCastImpair;
		}
		Helper.ModifyBuff(buffEntity, modificationTypes, true);
	}

	private static void SetHealth(Unit unit, Entity e)
	{
		if (e.Has<Health>())
		{
			var health = e.Read<Health>();
			if (unit.MaxHealth != -1)
			{
				health.MaxHealth._Value = unit.MaxHealth;
				health.MaxRecoveryHealth = unit.MaxHealth;
				health.Value = unit.MaxHealth;
			}
			e.Write(health);
		}
	}

	public static string GetGameMode(Entity entity)
	{
		if (entity.Has<CanFly>() && entity.Has<NameableInteractable>())
		{
			var nameableInteractable = entity.Read<NameableInteractable>();
			return nameableInteractable.Name.ToString();
		}
		return "";
	}

	//this is deprecated but may have a use in the future if we need to spawn in things but can't use the nameable interactable to store their game mode
	public static bool HasCategoryOld(Entity entity, string category)
	{
		if (entity.Has<CanFly>() && entity.Has<ResistanceData>())
		{ 
			var resistances = entity.Read<ResistanceData>();
			if (StringToFloatHash(category) == resistances.GarlicResistance_IncreasedExposureFactorPerRating)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasCategory(Entity entity, string category)
	{
		if (entity.Has<CanFly>() && entity.Has<NameableInteractable>())
		{
			var nameableInteractable = entity.Read<NameableInteractable>();
			if (nameableInteractable.Name.ToString() == category)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetCategory(Entity entity)
	{
		if (entity.Has<CanFly>() && entity.Has<NameableInteractable>())
		{
			var nameableInteractable = entity.Read<NameableInteractable>();
			return nameableInteractable.Name.ToString();
		}
		return "";
	}

	public static void AddCategory(Entity entity, string category)
	{
		entity.Add<CanFly>();
		entity.Add<NameableInteractable>();
		entity.Write(new NameableInteractable
		{
			Name = category
		});
	}
}

public class Unit
{
	protected PrefabGUID prefabGuid;
	protected int team = 10;
	protected int level = -1;
	protected bool isImmaterial = false;
	protected float maxHealth = -1;
	protected float aggroRadius = -1;
	protected bool knockbackResistance = false;
	protected bool isRooted = false;
	protected float respawnTime = -1;
	protected bool drawsAggro = true;
	protected bool isTargetable = true;
	protected bool isInvisible = false;
	protected bool isInvulnerable = false;
	protected bool abilityCastImpair = false;
	protected bool dynamicCollision = false;
	protected float maxDistanceFromPreCombatPosition = -1;
	protected bool mapCollision = true;
	protected string gameMode = "";
	protected int spawnDelay = -1;
	protected bool softSpawn = false;
	protected bool announceSpawn = false;
	protected string name = "";

	public PrefabGUID PrefabGuid { get => prefabGuid; set => prefabGuid = value; }
	public int Team { get => team; set => team = value; }
	public int Level { get => level; set => level = value; }
	public bool IsImmaterial { get => isImmaterial; set => isImmaterial = value; }
	public bool IsInvulnerable { get => isInvulnerable; set => isInvulnerable = value; }
	public bool AbilityCastImpair { get => abilityCastImpair; set => abilityCastImpair = value; }
	public float MaxHealth { get => maxHealth; set => maxHealth = value; }
	public float AggroRadius { get => aggroRadius; set => aggroRadius = value; }
	public bool KnockbackResistance { get => knockbackResistance; set => knockbackResistance = value; }
	public bool IsRooted { get => isRooted; set => isRooted = value; }
	public float RespawnTime { get => respawnTime; set => respawnTime = value; }
	public bool DrawsAggro { get => drawsAggro; set => drawsAggro = value; }
	public bool DynamicCollision { get => dynamicCollision; set => dynamicCollision = value; }
	public float MaxDistanceFromPreCombatPosition { get => maxDistanceFromPreCombatPosition; set => maxDistanceFromPreCombatPosition = value; }
	public bool MapCollision { get => mapCollision; set => mapCollision = value; }
	public bool IsTargetable { get => isTargetable; set => isTargetable = value; }
	public string Category { get => gameMode; set => gameMode = value; }
	public bool AnnounceSpawn { get => announceSpawn; set => announceSpawn = value; }
	public int SpawnDelay { get => spawnDelay; set => spawnDelay = value; }
	public bool SoftSpawn { get => softSpawn; set => softSpawn = value; }
	public string Name { get => name; set => name = value; }


	public Unit(PrefabGUID prefabGuid, int team = 10, int level = -1)
	{
		this.prefabGuid = prefabGuid;
		this.team = team;
		this.level = level;
	}

	public virtual void Modify(Entity e, Entity buffEntity)
	{
		Modify(e);
	}

	public virtual void Modify(Entity e)
	{

	}
}

public class HardBoss : Boss
{
	public HardBoss(PrefabGUID prefabGuid, int team = 10) : base(prefabGuid, team)
	{
		level = 120;
		maxHealth = 1500;
	}

	public override void Modify(Entity e, Entity buffEntity)
	{

		base.Modify(e, buffEntity);
	}
}

public class Boss : Unit
{
	public Boss(PrefabGUID prefabGuid, int team = 10, int level = -1) : base(prefabGuid, team, level)
	{
		isImmaterial = false;
		aggroRadius = -1;
		knockbackResistance = true;
		isRooted = false;
		drawsAggro = false;
		isTargetable = false;
		softSpawn = true;
	}

	public override void Modify(Entity e, Entity buffEntity)
	{
		//e.Remove<DynamicallyWeakenAttackers>();
		base.Modify(e, buffEntity);
	}
}

public class LightningBoss : Boss
{
	public LightningBoss(string gameMode, int team = 10, int level = -1) : base(Prefabs.CHAR_Gloomrot_SpiderTank_LightningRod, team, level)
	{
		isRooted = true;
	}

	public override void Modify(Entity e, Entity buffEntity)
	{
		Action action = () => Helper.BuffEntity(e, Prefabs.AB_LightningStrike_RodHit_EmpowerTankBuff, out var lightningBuffEntity, Helper.NO_DURATION);
		var timer = ActionScheduler.RunActionEveryInterval(action, 3);
		timersByCategory[gameMode].Add(timer);
		base.Modify(e, buffEntity);
	}
}

public class AngramBoss : Boss
{
	public AngramBoss(int team = 10, int level = -1) : base(Prefabs.CHAR_Gloomrot_Purifier_VBlood, team, level)
	{
		name = "Angram";
		softSpawn = true;
		isRooted = true;
	}
}

public class Dummy : Unit
{
    public static readonly int ResetTime = 5;
    public static readonly PrefabGUID PrefabGUID = Prefabs.CHAR_TargetDummy_Footman;
	private bool Aggro = false;
	public Dummy(PrefabGUID prefabGuid, bool aggro = true) : base(PrefabGUID)
	{
		level = 84;
		isInvulnerable = false;
		maxHealth = 653;
		drawsAggro = true;
		isRooted = true;
		dynamicCollision = true;
		knockbackResistance = false;
        gameMode = "dummy";
        PrefabGuid = prefabGuid;
		Aggro = aggro;
	}

    public Dummy() : base(PrefabGUID)
    {
        level = 84;
        isInvulnerable = false;
		maxHealth = 653;
        drawsAggro = true;
        isRooted = true;
		dynamicCollision = true;
		knockbackResistance = false;
        gameMode = "dummy";
    }

    public override void Modify(Entity e, Entity buffEntity)
	{
		base.Modify(e);

		if (!Aggro)
		{
			var aggroConsumer = e.Read<AggroConsumer>();
			aggroConsumer.Active._Value = false;
			e.Write(aggroConsumer);
		}

        var woundedConstants = e.Read<WoundedConstants>();
        woundedConstants.HealthFactor = 0;
        woundedConstants.TriggerKnockbackOnWounded = false;
        e.Write(woundedConstants);

		var blood = e.Read<BloodConsumeSource>();
		blood.BloodQuality = 0;
		blood.CanBeConsumed = false;
		e.Write(blood);

		Helper.BuffEntity(e, Prefabs.Buff_BloodMoon, out var bloodAuraBuffEntity, Helper.NO_DURATION);
        Helper.ChangeBuffResistances(e, Prefabs.BuffResistance_Vampire);
	}
}

public class Turret : Unit
{
	public Turret(PrefabGUID prefabGuid, int team = 10, int level = -1) : base(prefabGuid, team, level)
	{
		isImmaterial = true;
		knockbackResistance = false;
		isRooted = true;
		drawsAggro = false;
		isTargetable = false;
	}
}

public class BaseTurret : Unit
{
	public static PrefabGUID PrefabGUID = Prefabs.CHAR_Gloomrot_SentryTurret;
	public BaseTurret(PrefabGUID prefabGuid, int team = 10, int level = -1) : base(prefabGuid, team, level)
	{
		isImmaterial = false;
		knockbackResistance = true;
		isRooted = true;
		drawsAggro = true;
		isTargetable = false;
		aggroRadius = 3f;
		maxDistanceFromPreCombatPosition = 20;
	}

	public override void Modify(Entity e)
	{
		base.Modify(e);
		Helper.BuffEntity(e, Prefabs.AB_Gloomrot_SentryTurret_BunkerDown_Buff, out var buffEntity, Helper.NO_DURATION);
		Helper.ModifyBuff(buffEntity, BuffModificationTypes.None, true);
	}
}

public class DyeableStructure : Unit
{
	protected int color = 0;
	public int Color { get => color; set => color = value; }
	public DyeableStructure(PrefabGUID prefabGuid, int color) : base(prefabGuid)
	{
		Color = color;
	}

	public override void Modify(Entity e)
	{
		if (e.Has<DyeableCastleObject>())
		{
			var dyeable = e.Read<DyeableCastleObject>();
			dyeable.ActiveColorIndex = (byte)color;
			e.Write(dyeable);
		}
	}


	public override void Modify(Entity e, Entity buffEntity)
	{
		Modify(e);
	}
}

public class HealingOrb : Unit
{
	public HealingOrb() : base(Prefabs.AB_General_HealingOrb_Object)
	{

	}

	public override void Modify(Entity e)
	{
		var lifetime = e.Read<LifeTime>();
		lifetime.EndAction = LifeTimeEndAction.None;
		lifetime.Duration = -1;
		var buffer = e.ReadBuffer<CreateGameplayEventsOnTimePassed>();
		buffer.Clear();
	}
}

public class UltimateOrb : Unit
{
	public UltimateOrb() : base(Prefabs.CHAR_Militia_ConstrainingPole)
	{
		isRooted = true;
		knockbackResistance = true;
		drawsAggro = false;
		isTargetable = false;
		name = "Ultimate Orb";
		announceSpawn = true;
	}

	public override void Modify(Entity e)
	{
		var lifetime = e.Read<LifeTime>();
		lifetime.EndAction = LifeTimeEndAction.None;
		lifetime.Duration = -1;
		if (Helper.BuffEntity(e, Helper.CustomBuff1, out var buffEntity, Helper.NO_DURATION))
		{
			Helper.ModifyBuff(buffEntity, BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.MovementImpair);
		}
	}
}

public class Trader : Unit
{
	List<TraderItemDto> TraderItemDtos;
	public Trader(PrefabGUID prefabGuid, List<TraderItemDto> traderItemDtos) : base(prefabGuid)
	{
		TraderItemDtos = traderItemDtos;
	}

	public override void Modify(Entity e)
	{
		var aggroConsumer = e.Read<AggroConsumer>();
		aggroConsumer.Active._Value = false;
		e.Write(aggroConsumer);

		var action = () =>
		{
			var _tradeOutputBuffer = e.ReadBuffer<TradeOutput>();
			var _traderEntryBuffer = e.ReadBuffer<TraderEntry>();
			var _tradeCostBuffer = e.ReadBuffer<TradeCost>();
			_tradeOutputBuffer.Clear();
			_traderEntryBuffer.Clear();
			_tradeCostBuffer.Clear();

			var i = 0;
			foreach (var item in TraderItemDtos)
			{
				if (i > 25)
				{
					break;
				}
				_tradeOutputBuffer.Add(new TradeOutput
				{
					Amount = (ushort)item.OutputAmount,
					Item = item.OutputItem,
				});

				_tradeCostBuffer.Add(new TradeCost
				{
					Amount = (ushort)item.InputAmount,
					Item = item.InputItem,
				});

				_traderEntryBuffer.Add(new TraderEntry
				{
					RechargeInterval = -1,
					CostCount = 1,
					CostStartIndex = (byte)i,
					FullRechargeTime = -1,
					OutputCount = 1,
					OutputStartIndex = (byte)i,
					StockAmount = (ushort)item.StockAmount,
				});
				i++;
			}
		};
		ActionScheduler.RunActionOnceAfterFrames(action, 3); //delay clearing buffer until after it is populated
	}
}

public class Horse : Unit
{
	protected static readonly new PrefabGUID prefabGuid = Prefabs.CHAR_Mount_Horse;
	protected float speed = 11;
	protected float acceleration = 7;
	protected float rotation = 14;
	protected string name = "";

	public float Speed { get => speed; set => speed = value; }
	public float Acceleration { get => acceleration; set => acceleration = value; }
	public float Rotation { get => rotation; set => rotation = value; }
	public string Name { get => name; set => name = value; }

	public Horse(int team = 10) : base(prefabGuid, team)
	{

	}

	public override void Modify(Entity e, Entity buffEntity)
	{
		if (!string.IsNullOrEmpty(name))
		{
			var nameableInteractable = e.Read<NameableInteractable>();
			nameableInteractable.Name = name;
			e.Write(nameableInteractable);
		}

		var mountable = e.Read<Mountable>();
		mountable.MaxSpeed = Speed;
		mountable.Acceleration = Acceleration;
		mountable.RotationSpeed = Rotation * 10;
		e.Write(mountable);
	}
}

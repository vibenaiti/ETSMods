using System.Collections.Generic;
using System.Linq;
using ProjectM;
using ModCore.Models;
using Stunlock.Core;

namespace ModCore.Data;

public static class JewelData
{
    static JewelData()
    {
        prefabToAbilityNameDictionary = abilityToPrefabDictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
    }

    public static float RANDOM_POWER = -1;

    public static readonly Dictionary<PrefabGUID, List<PrefabGUID>> AbilityToSpellMods = new()
    {
        {
            Prefabs.AB_Vampire_VeilOfStorm_Group, new List<PrefabGUID>
            {
                Prefabs.SpellMod_VeilOfStorm_SparklingIllusion,
                Prefabs.SpellMod_VeilOfStorm_DashInflictStatic,
                Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
                Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun,
                Prefabs.SpellMod_VeilOfStorm_AttackInflictFadingSnare,
                Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
            }
        },
        {
            Prefabs.AB_Storm_Discharge_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Discharge_RecastDetonate,
                Prefabs.SpellMod_Discharge_SpellLeech,
                Prefabs.SpellMod_Discharge_Immaterial,
                Prefabs.SpellMod_Shared_Storm_GrantWeaponCharge,
                Prefabs.SpellMod_Discharge_BonusDamage,
                Prefabs.SpellMod_Discharge_IncreaseStunDuration,
                Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High,
            }
        },
        {
            Prefabs.AB_Vampire_VeilOfBones_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_VeilOfBones_DashInflictCondemn,
                Prefabs.SpellMod_VeilOfBones_DashHealMinions,
                Prefabs.SpellMod_VeilOfBones_SpawnSkeletonMage,
                Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
                Prefabs.SpellMod_VeilOfBones_BonusDamageBelowTreshhold,
                Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
            }
        },
        {
            Prefabs.AB_Unholy_WardOfTheDamned_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_WardOfTheDamned_MightSpawnMageSkeleton,
                Prefabs.SpellMod_WardOfTheDamned_DamageMeleeAttackers,
                Prefabs.SpellMod_WardOfTheDamned_HealOnAbsorbProjectile,
                Prefabs.SpellMod_WardOfTheDamned_KnockbackOnRecast,
                Prefabs.SpellMod_WardOfTheDamned_EmpowerSkeletonsOnRecast,
                Prefabs.SpellMod_WardOfTheDamned_ShieldSkeletonsOnRecast,
                Prefabs.SpellMod_WardOfTheDamned_BonusDamageOnRecast,
                Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_Low,
            }
        },
        {
            Prefabs.AB_Chaos_PowerSurge_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_PowerSurge_RecastDestonate,
                Prefabs.SpellMod_Shared_DispellDebuffs,
                Prefabs.SpellMod_PowerSurge_Shield,
                Prefabs.SpellMod_PowerSurge_IncreaseDurationOnKill,
                Prefabs.SpellMod_PowerSurge_AttackSpeed,
                Prefabs.SpellMod_PowerSurge_Haste,
                Prefabs.SpellMod_PowerSurge_Lifetime,
                Prefabs.SpellMod_PowerSurge_EmpowerPhysical,
            }
        },
        {
            Prefabs.AB_Chaos_Barrier_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Chaos_Barrier_LesserPowerSurge,
                Prefabs.SpellMod_Chaos_Barrier_StunOnHit,
                Prefabs.SpellMod_Chaos_Barrier_ExplodeOnHit,
                Prefabs.SpellMod_Chaos_Barrier_BonusDamage,
                Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_Low,
                Prefabs.SpellMod_Chaos_Barrier_ConsumeAttackReduceCooldownXTimes,
            }
        },
        {
            Prefabs.AB_Vampire_VeilOfChaos_Group, new List<PrefabGUID>
            {
                Prefabs.SpellMod_VeilOfChaos_BonusIllusion,
                Prefabs.SpellMod_VeilOfChaos_ApplySnareOnExplode,
                Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteAgonizingFlames_OnAttack,
                Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
                Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
                Prefabs.SpellMod_VeilOfChaos_BonusDamageOnExplode,
            }
        },
        {
            Prefabs.AB_FrostBarrier_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_FrostBarrier_ConsumeAttackReduceCooldownXTimes,
                Prefabs.SpellMod_FrostBarrier_BonusSpellPowerOnAbsorb,
                Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Recast,
                Prefabs.SpellMod_FrostBarrier_KnockbackOnRecast,
                Prefabs.SpellMod_FrostBarrier_ShieldOnFrostyRecast,
                Prefabs.SpellMod_FrostBarrier_BonusDamage,
                Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_Low,
            }
        },
        {
            Prefabs.AB_Frost_ColdSnap_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_ColdSnap_HasteWhileShielded,
                Prefabs.SpellMod_ColdSnap_Immaterial,
                Prefabs.SpellMod_Shared_FrostWeapon,
                Prefabs.SpellMod_Shared_Frost_IncreaseFreezeWhenChill,
                Prefabs.SpellMod_ColdSnap_BonusDamage,
                Prefabs.SpellMod_ColdSnap_BonusAbsorb,
                Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High,
            }
        },
        {
            Prefabs.AB_Vampire_VeilOfFrost_Group, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack,
                Prefabs.SpellMod_VeilOfFrost_FrostNova,
                Prefabs.SpellMod_VeilOfFrost_IllusionFrostBlast,
                Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
                Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
                Prefabs.SpellMod_VeilOfFrost_ShieldBonus,
            }
        },
        {
            Prefabs.AB_Illusion_PhantomAegis_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_PhantomAegis_ConsumeShieldAndPullAlly,
                Prefabs.SpellMod_Shared_DispellDebuffs,
                Prefabs.SpellMod_Shared_KnockbackOnHit_Medium,
                Prefabs.SpellMod_PhantomAegis_ExplodeOnDestroy,
                Prefabs.SpellMod_PhantomAegis_IncreaseLifetime,
                Prefabs.SpellMod_Shared_MovementSpeed_Normal,
                Prefabs.SpellMod_PhantomAegis_IncreaseSpellPower,
            }
        },
        {
            Prefabs.AB_Illusion_MistTrance_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_MistTrance_ReduceSecondaryWeaponCD,
                Prefabs.SpellMod_MistTrance_PhantasmOnTrigger,
                Prefabs.SpellMod_MistTrance_HasteOnTrigger,
                Prefabs.SpellMod_MIstTrance_DamageOnAttack,
                Prefabs.SpellMod_MistTrance_FearOnTrigger,
                Prefabs.SpellMod_Shared_KnockbackOnHit_Medium,
                Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High,
                Prefabs.SpellMod_Shared_TravelBuff_IncreaseRange_Medium,
            }
        },
        {
            Prefabs.AB_Vampire_VeilOfIllusion_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_VeilOfIllusion_RecastDetonate,
                Prefabs.SpellMod_VeilOfIllusion_IllusionProjectileDamage,
                Prefabs.SpellMod_VeilOfIllusion_PhantasmOnHit,
                Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
                Prefabs.SpellMod_VeilOfIllusion_AttackInflictFadingSnare,
                Prefabs.SpellMod_Shared_Illusion_WeakenShield_OnAttack,
                Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
            }
        },
        {
            Prefabs.AB_Blood_BloodRage_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_BloodRage_HealOnKill,
                Prefabs.SpellMod_Shared_DispellDebuffs,
                Prefabs.SpellMod_BloodRage_Shield,
                Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium,
                Prefabs.SpellMod_BloodRage_DamageBoost,
                Prefabs.SpellMod_BloodRage_IncreaseLifetime,
                Prefabs.SpellMod_BloodRage_IncreaseMoveSpeed,
            }
        },
        {
            Prefabs.AB_Vampire_VeilOfBlood_Group, new List<PrefabGUID>
            {
                Prefabs.SpellMod_VeilOfBlood_DashInflictLeech,
                Prefabs.SpellMod_VeilOfBlood_Empower,
                Prefabs.SpellMod_VeilOfBlood_AttackInflictFadingSnare,
                Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
                Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
                Prefabs.SpellMod_VeilOfBlood_SelfHealing,
            }
        },
        {
            Prefabs.AB_Storm_BallLightning_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_BallLightning_DetonateOnRecast,
                Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun_Explode,
                Prefabs.SpellMod_BallLightning_KnockbackOnExplode,
                Prefabs.SpellMod_BallLightning_Haste,
                Prefabs.SpellMod_BallLightning_BonusDamage,
                Prefabs.SpellMod_Shared_Projectile_IncreaseRange_Medium,
            }
        },
        {
            Prefabs.AB_Storm_LightningWall_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_LightningWall_FadingSnare,
                Prefabs.SpellMod_LightningWall_ApplyShield,
                Prefabs.SpellMod_LightningWall_ConsumeProjectileWeaponCharge,
                Prefabs.SpellMod_LightningWall_BonusDamage,
                Prefabs.SpellMod_LightningWall_IncreaseMovementSpeed,
            }
        },
        {
            Prefabs.AB_Storm_Cyclone_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun,
                Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoWeaponCharge,
                Prefabs.SpellMod_Cyclone_BonusDamage,
                Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
                Prefabs.SpellMod_Cyclone_IncreaseLifetime,
            }
        },
        {
            Prefabs.AB_Unholy_Soulburn_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Shared_DispellDebuffs_Self,
                Prefabs.SpellMod_Soulburn_ConsumeSkeletonEmpower,
                Prefabs.SpellMod_Soulburn_ConsumeSkeletonHeal,
                Prefabs.SpellMod_Soulburn_IncreaseTriggerCount,
                Prefabs.SpellMod_Soulburn_IncreasedSilenceDuration,
                Prefabs.SpellMod_Soulburn_BonusDamage,
                Prefabs.SpellMod_Soulburn_BonusLifeDrain,
                Prefabs.SpellMod_Soulburn_ReduceCooldownOnSilence,
            }
        },
        {
            Prefabs.AB_Unholy_DeathKnight_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_DeathKnight_SkeletonMageOnLifetimeEnded,
                Prefabs.SpellMod_DeathKnight_LifeLeech,
                Prefabs.SpellMod_DeathKnight_SnareEnemiesOnSummon,
                Prefabs.SpellMod_DeathKnight_BonusDamage,
                Prefabs.SpellMod_DeathKnight_IncreaseLifetime,
                Prefabs.SpellMod_DeathKnight_MaxHealth,
            }
        },
        {
            Prefabs.AB_Unholy_CorpseExplosion_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_CorpseExplosion_KillingBlow,
                Prefabs.SpellMod_CorpseExplosion_SkullNova,
                Prefabs.SpellMod_CorpseExplosion_DoubleImpact,
                Prefabs.SpellMod_CorpseExplosion_HealMinions,
                Prefabs.SpellMod_CorpseExplosion_SnareBonus,
                Prefabs.SpellMod_CorpseExplosion_BonusDamage,
                Prefabs.SpellMod_Shared_TargetAoE_IncreaseRange_Medium,
                Prefabs.SpellMod_Shared_Cooldown_Medium,
            }
        },
        {
            Prefabs.AB_Chaos_Aftershock_Group, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Chaos_Aftershock_KnockbackArea,
                Prefabs.SpellMod_Chaos_Aftershock_InflictSlowOnProjectile,
                Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteAgonizingFlames,
                Prefabs.SpellMod_Chaos_Aftershock_BonusDamage,
                Prefabs.SpellMod_Shared_Projectile_IncreaseRange_Medium,
                Prefabs.SpellMod_Shared_Cooldown_Medium,
            }
        },
        {
            Prefabs.AB_Frost_IceNova_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_IceNova_RecastLesserNova,
                Prefabs.SpellMod_IceNova_ApplyShield,
                Prefabs.SpellMod_Shared_TargetAoE_IncreaseRange_Medium,
                Prefabs.SpellMod_IceNova_BonusDamageToFrosty,
                Prefabs.SpellMod_Shared_Cooldown_Medium,
            }
        },
        {
            Prefabs.AB_Frost_CrystalLance_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_CrystalLance_PierceEnemies,
                Prefabs.SpellMod_Shared_Frost_SplinterNovaOnFrosty,
                Prefabs.SpellMod_CrystalLance_BonusDamageToFrosty,
                Prefabs.SpellMod_Shared_Frost_IncreaseFreezeWhenChill,
                Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
                Prefabs.SpellMod_Shared_CastRate,
            }
        },
        {
            Prefabs.AB_Illusion_WraithSpear_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenSpawnWisp,
                Prefabs.SpellMod_Shared_Illusion_WeakenShield,
                Prefabs.SpellMod_WraithSpear_ShieldAlly,
                Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium,
                Prefabs.SpellMod_WraithSpear_BonusDamage,
                Prefabs.SpellMod_Shared_Projectile_IncreaseRange_Medium,
                Prefabs.SpellMod_WraithSpear_ReducedDamageReduction,
            }
        },
        {
            Prefabs.AB_Illusion_SpectralWolf_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_SpectralWolf_FirstBounceInflictFadingSnare,
                Prefabs.SpellMod_SpectralWolf_WeakenApplyXPhantasm,
                Prefabs.SpellMod_Shared_Illusion_WeakenShield,
                Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenSpawnWisp,
                Prefabs.SpellMod_SpectralWolf_ReturnToOwner,
                Prefabs.SpellMod_SpectralWolf_AddBounces,
                Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
                Prefabs.SpellMod_SpectralWolf_DecreaseBounceDamageReduction,
            }
        },
        {
            Prefabs.AB_Illusion_Mosquito_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Mosquito_ShieldOnSpawn,
                Prefabs.SpellMod_Mosquito_WispsOnDestroy,
                Prefabs.SpellMod_Mosquito_BonusDamage,
                Prefabs.SpellMod_Mosquito_BonusFearDuration,
                Prefabs.SpellMod_Mosquito_BonusHealthAndSpeed,
            }
        },
        {
            Prefabs.AB_Blood_Shadowbolt_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Shadowbolt_LeechBonusDamage,
                Prefabs.SpellMod_Shared_Blood_ConsumeLeechSelfHeal_Big,
                Prefabs.SpellMod_Shadowbolt_ExplodeOnHit,
                Prefabs.SpellMod_Shadowbolt_VampiricCurse,
                Prefabs.SpellMod_Shared_KnockbackOnHit_Medium,
                Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
                Prefabs.SpellMod_Shared_Cooldown_Medium,
                Prefabs.SpellMod_Shared_CastRate,
            }
        },
        {
            Prefabs.AB_Blood_BloodRite_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_BloodRite_Stealth,
                Prefabs.SpellMod_BloodRite_HealOnTrigger,
                Prefabs.SpellMod_BloodRite_ApplyFadingSnare,
                Prefabs.SpellMod_BloodRite_DamageOnAttack,
                Prefabs.SpellMod_BloodRite_TossDaggers,
                Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High,
                Prefabs.SpellMod_BloodRite_IncreaseLifetime,
                Prefabs.SpellMod_BloodRite_BonusDamage,
            }
        },
        {
            Prefabs.AB_Blood_BloodFountain_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_BloodFountain_RecastLesser,
                Prefabs.SpellMod_BloodFountain_FirstImpactApplyLeech,
                Prefabs.SpellMod_BloodFountain_FirstImpactFadingSnare,
                Prefabs.SpellMod_BloodFountain_FirstImpactDispell,
                Prefabs.SpellMod_BloodFountain_SecondImpactKnockback,
                Prefabs.SpellMod_BloodFountain_SecondImpactSpeedBuff,
                Prefabs.SpellMod_BloodFountain_FirstImpactHealIncrease,
                Prefabs.SpellMod_BloodFountain_SecondImpactDamageIncrease,
                Prefabs.SpellMod_BloodFountain_SecondImpactHealIncrease,
            }
        },
        {
            Prefabs.AB_Storm_PolarityShift_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Storm_PolarityShift_AreaImpactOrigin,
                Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoWeaponCharge,
                Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium,
                Prefabs.SpellMod_Storm_PolarityShift_AreaImpactDestination,
                Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
            }
        },
        {
            Prefabs.AB_Chaos_Void_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteAgonizingFlames,
                Prefabs.SpellMod_Chaos_Void_FragBomb,
                Prefabs.SpellMod_Chaos_Void_BurnArea,
                Prefabs.SpellMod_Chaos_Void_BonusDamage,
                Prefabs.SpellMod_Shared_TargetAoE_IncreaseRange_Medium,
                Prefabs.SpellMod_Chaos_Void_ReduceChargeCD,
            }
        },
        {
            Prefabs.AB_Unholy_CorruptedSkull_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_CorruptedSkull_LesserProjectiles,
                Prefabs.SpellMod_CorruptedSkull_DetonateSkeleton,
                Prefabs.SpellMod_Shared_KnockbackOnHit_Medium,
                Prefabs.SpellMod_CorruptedSkull_BoneSpirit,
                Prefabs.SpellMod_CorruptedSkull_BonusDamage,
                Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
            }
        },
        {
            Prefabs.AB_Chaos_Volley_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Chaos_Volley_SecondProjectileBonusDamage,
                Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteAgonizingFlames,
                Prefabs.SpellMod_Shared_KnockbackOnHit_Light,
                Prefabs.SpellMod_Chaos_Volley_BonusDamage,
                Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
                Prefabs.SpellMod_Shared_Cooldown_Medium,
            }
        },
        {
            Prefabs.AB_Frost_FrostBat_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_Shared_Frost_SplinterNovaOnFrosty,
                Prefabs.SpellMod_Shared_Frost_ShieldOnFrosty,
                Prefabs.SpellMod_FrostBat_AreaDamage,
                Prefabs.SpellMod_FrostBat_BonusDamageToFrosty,
                Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
                Prefabs.SpellMod_Shared_CastRate,
            }
        },
        {
            Prefabs.AB_Blood_SanguineCoil_AbilityGroup, new List<PrefabGUID>
            {
                Prefabs.SpellMod_SanguineCoil_KillRecharge,
                Prefabs.SpellMod_SanguineCoil_AddBounces,
                Prefabs.SpellMod_SanguineCoil_LeechBonusDamage,
                Prefabs.SpellMod_Shared_AddCharges,
                Prefabs.SpellMod_SanguineCoil_BonusDamage,
                Prefabs.SpellMod_SanguineCoil_BonusLifeLeech,
                Prefabs.SpellMod_SanguineCoil_BonusHealing,
                Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
            }
        }
    };

    public static readonly Dictionary<PrefabGUID, string> SpellModDescriptions = new()
    {
        {
            Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun,
            $"Consumes {"Static".Colorify(ExtendedColor.Storm)} to {"Stun".Colorify(ExtendedColor.Storm)} the target"
        },
        {
            Prefabs.SpellMod_VeilOfStorm_DashInflictStatic,
            $"Dashing through an enemy inflicts {"Static".Colorify(ExtendedColor.Storm)}"
        },
        {
            Prefabs.SpellMod_Shared_Storm_GrantWeaponCharge,
            $"{"Charge".Colorify(ExtendedColor.Storm)} weapon when triggered"
        },
        { Prefabs.SpellMod_VeilOfBones_BonusDamageBelowTreshhold, $"Deal more {"damage".Emphasize()} to low health targets" },
        { Prefabs.SpellMod_VeilOfBones_SpawnSkeletonMage, $"Summon a {"Mage".Colorify(ExtendedColor.Unholy)} on hit instead of a {"Warrior".Colorify(ExtendedColor.Unholy)}" },
        { Prefabs.SpellMod_Chaos_Barrier_StunOnHit, $"Projectile recast applies {"stun".Emphasize()} to the target" },
        { Prefabs.SpellMod_Chaos_Barrier_ExplodeOnHit, $"Projectile recast conjurs an {"AoE".Emphasize()}" },
        { Prefabs.SpellMod_Chaos_Barrier_LesserPowerSurge, $"Gain a lesser {"Power Surge".Colorify(ExtendedColor.LightChaos)} when {"fully charged".Emphasize()}" },
        {
            Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteAgonizingFlames_OnAttack,
            $"Applies {"Agonizing Flames".Colorify(ExtendedColor.Chaos)} to {"Ignited".Colorify(ExtendedColor.Chaos)} targets"
        },
        { Prefabs.SpellMod_Shared_Frost_IncreaseFreezeWhenChill, $"Increase {"Freeze".Colorify(ExtendedColor.Frost)} duration on {"Chilled".Colorify(ExtendedColor.Frost)} targets" },
        { Prefabs.SpellMod_Shared_FrostWeapon, $"Next {"attack".Emphasize()} applies {"Chill".Colorify(ExtendedColor.Frost)} and does {"damage".Emphasize()}" },
        { Prefabs.SpellMod_ColdSnap_HasteWhileShielded, $"Increase {"movement speed".Emphasize()} during {"shield".Emphasize()} uptime" },
        { Prefabs.SpellMod_ColdSnap_BonusAbsorb, $"Increase {"shield strength".Emphasize()}" },
        { Prefabs.SpellMod_ColdSnap_BonusDamage, $"Increase {"damage".Emphasize()}" },
        { Prefabs.SpellMod_ColdSnap_Immaterial, $"Turn {"immaterial".Emphasize()} when triggered" },
        { Prefabs.SpellMod_VeilOfFrost_FrostNova, $"Next {"attack".Emphasize()} triggers an {"AoE".Emphasize()} that inflicts {"Chill".Colorify(ExtendedColor.Frost)}"},
        { Prefabs.SpellMod_VeilOfFrost_ShieldBonus, "Increase "+"shield strength".Emphasize() },
        {
            Prefabs.SpellMod_Shared_Illusion_WeakenShield_OnAttack,
            $"Next {"attack".Emphasize()} on a {"Weakened".Colorify(ExtendedColor.Illusion)} grants a {"shield".Emphasize()}"
        },
        {
            Prefabs.SpellMod_DeathKnight_SkeletonMageOnLifetimeEnded,
            "Summon a "+"Skeleton Mage".Colorify(ExtendedColor.Unholy)+" after expiration"
        },
        { Prefabs.SpellMod_DeathKnight_LifeLeech, "Heal for % of "+"damage".Emphasize()+" done by "+"Death Knight".Colorify(ExtendedColor.LightUnholy) },
        { Prefabs.SpellMod_DeathKnight_IncreaseLifetime, "Increase " +"Death Knight".Colorify(ExtendedColor.LightUnholy)+" uptime".Emphasize() },
        { Prefabs.SpellMod_DeathKnight_MaxHealth, "Increase " +"Death Knight".Colorify(ExtendedColor.LightUnholy)+" health".Emphasize() },
        { Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteAgonizingFlames, $"Applies {"Agonizing Flames".Colorify(ExtendedColor.Chaos)} to {"Ignited".Colorify(ExtendedColor.Chaos)} targets" },
        { Prefabs.SpellMod_Shared_Illusion_WeakenShield, $"Hitting a {"Weakened".Colorify(ExtendedColor.Illusion)} target grants a {"shield".Emphasize()}" },
        { Prefabs.SpellMod_SpectralWolf_WeakenApplyXPhantasm, $"Hitting a {"Weakened".Colorify(ExtendedColor.Illusion)} target grants {"Phantasm".Colorify(ExtendedColor.Illusion)}" },
        { Prefabs.SpellMod_Shadowbolt_LeechBonusDamage, "Deal "+"bonus damage".Emphasize()+" to "+"Leeched".Colorify(ExtendedColor.Blood)+" targets" },
        { Prefabs.SpellMod_Shared_Blood_ConsumeLeechSelfHeal_Big, "Heal "+"bonus max HP".Emphasize()+" on "+"Leeched".Colorify(ExtendedColor.Blood)+" targets" },
        { Prefabs.SpellMod_Shadowbolt_VampiricCurse, "Hit applies "+ "Vampiric Curse".Colorify(ExtendedColor.LightBlood) },
        { Prefabs.SpellMod_BloodRite_ApplyFadingSnare, "Trigger applies a "+"fading snare".Emphasize() + " to enemies" },
        { Prefabs.SpellMod_BloodRite_HealOnTrigger, "Trigger "+"heals".Emphasize()+" the caster" },
        { Prefabs.SpellMod_BloodRite_TossDaggers, "Trigger throws "+"daggers".Emphasize()+" to enemies" },
        { Prefabs.SpellMod_SanguineCoil_LeechBonusDamage, "Deal "+"bonus damage".Emphasize()+" to "+"Leeched".Colorify(ExtendedColor.Blood)+" targets"},
        { Prefabs.SpellMod_SanguineCoil_BonusLifeLeech, "Increase "+"life drain".Emphasize() },
        {
            Prefabs.SpellMod_BloodFountain_RecastLesser,
            "Recast a smaller " + "Blood Fountain".Colorify(ExtendedColor.LightBlood)
        },
        {
            Prefabs.SpellMod_BloodFountain_FirstImpactApplyLeech, "Hit applies " + "Leech".Colorify(ExtendedColor.Blood)
        },
        { Prefabs.SpellMod_BloodFountain_FirstImpactFadingSnare, "Hit applies " + "fading snare".Emphasize() },
        {
            Prefabs.SpellMod_BloodFountain_FirstImpactDispell,
            "Hit removes " + "negative effects".Emphasize() + " from allies"
        },
        {
            Prefabs.SpellMod_BloodFountain_SecondImpactKnockback,
            "Explosion " + "pushes".Emphasize() + " enemies "+"back".Emphasize()
        },
        { Prefabs.SpellMod_BloodFountain_SecondImpactSpeedBuff, "Explosion increases ally " + "MS".Emphasize() },
        { Prefabs.SpellMod_BloodFountain_FirstImpactHealIncrease, "Increase hit " + "healing".Emphasize() },
        { Prefabs.SpellMod_BloodFountain_SecondImpactDamageIncrease, "Increase explosion " + "damage".Emphasize() },
        { Prefabs.SpellMod_BloodFountain_SecondImpactHealIncrease, "Increase explosion " + "healing".Emphasize() },
        { Prefabs.SpellMod_BloodRage_HealOnKill, "Kill an enemy to " + "heal".Emphasize() },
        {
            Prefabs.SpellMod_Shared_DispellDebuffs,
            "Cast " + "removes".Emphasize() + " all " + "negative effects".Emphasize()
        },
        { Prefabs.SpellMod_BloodRage_Shield, "Cast grants a " + "shield".Emphasize() + " to caster and allies" },
        {
            Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium,
            "Cast applies a " + "fading snare".Emphasize() + " on enemies"
        },
        { Prefabs.SpellMod_BloodRage_DamageBoost, "Increase " + "physical power".Emphasize() },
        { Prefabs.SpellMod_BloodRage_IncreaseLifetime, "Increase effect " + "duration".Emphasize() },
        { Prefabs.SpellMod_BloodRage_IncreaseMoveSpeed, "Increase " + "MS".Emphasize() },
        { Prefabs.SpellMod_BloodRite_Stealth, "Turn "+"invisible".Emphasize() + " while " +"immaterial".Emphasize() },
        { Prefabs.SpellMod_BloodRite_DamageOnAttack, "Trigger for first "+"attack".Emphasize()+" to deal "+"bonus damage".Emphasize() },
        { Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High, "Increase "+"MS".Emphasize() + " during channel" },
        { Prefabs.SpellMod_BloodRite_IncreaseLifetime, "Increase "+"immaterial duration".Emphasize() },
        { Prefabs.SpellMod_BloodRite_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_SanguineCoil_KillRecharge, "Kill an enemy to restore a"+" charge".Emphasize() },
        {
            Prefabs.SpellMod_SanguineCoil_AddBounces, "Hit "+"bounces".Emphasize() + " to an "+"additional target".Emphasize()
        },
        { Prefabs.SpellMod_Shared_AddCharges, "Increase "+"charges".Emphasize() },
        { Prefabs.SpellMod_SanguineCoil_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_SanguineCoil_BonusHealing, "Increase "+"healing".Emphasize() },
        {
            Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
            "Increase "+"projectile range".Emphasize() + " and "+"speed".Emphasize()
        },
        { Prefabs.SpellMod_Shadowbolt_ExplodeOnHit, "Hit conjurs an "+"AoE".Emphasize() },
        { Prefabs.SpellMod_Shared_KnockbackOnHit_Medium, "Hit "+"pushes".Emphasize() + " enemies "+"back".Emphasize() },
        { Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease "+"CD".Emphasize() },
        { Prefabs.SpellMod_Shared_CastRate, "Decrease "+"cast time".Emphasize() },
        {
            Prefabs.SpellMod_VeilOfBlood_DashInflictLeech,
            "Dashing through an enemy applies "+"Leech".Colorify(ExtendedColor.Blood)
        },
        {
            Prefabs.SpellMod_VeilOfBlood_Empower,
            "Next "+"attack".Emphasize()+" consumes "+"Leech".Colorify(ExtendedColor.Blood) + " for "+"phys power".Emphasize()
        },
        {
            Prefabs.SpellMod_VeilOfBlood_AttackInflictFadingSnare,
            "Next "+"attack".Emphasize()+" applies a "+"fading snare".Emphasize()
        },
        { Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration, "Increase "+"elude duration".Emphasize() },
        {
            Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
            "Increase "+"damage".Emphasize() + " of next "+"attack".Emphasize()
        },
        { Prefabs.SpellMod_VeilOfBlood_SelfHealing, "Increase "+"healing".Emphasize()},
        { Prefabs.SpellMod_Chaos_Aftershock_KnockbackArea, "Cast "+"knocks".Emphasize() + " enemies "+"back".Emphasize() },
        { Prefabs.SpellMod_Chaos_Aftershock_InflictSlowOnProjectile, "Cast applies a "+"fading snare".Emphasize() },
        {
            Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion,
            "Explosion consumes "+"Ignite".Colorify(ExtendedColor.Chaos) + " to conjure an "+"AoE".Emphasize()
        },
        { Prefabs.SpellMod_Chaos_Aftershock_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_Shared_Projectile_IncreaseRange_Medium, "Increase "+"projectile range".Emphasize() },
        { Prefabs.SpellMod_Chaos_Barrier_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_Low, "Increase "+"MS".Emphasize() + " during channel" },
        {
            Prefabs.SpellMod_Chaos_Barrier_ConsumeAttackReduceCooldownXTimes,
            "Decrease "+"CD".Emphasize() + " on absorbed hit"
        },
        {
            Prefabs.SpellMod_Chaos_Volley_SecondProjectileBonusDamage,
            "Bonus "+"dmg".Emphasize() + " for hitting diff target with 2nd shot"
        },
        { Prefabs.SpellMod_Shared_KnockbackOnHit_Light, "Hit "+"pushes".Emphasize() + " enemies "+"back".Emphasize() },
        { Prefabs.SpellMod_Chaos_Volley_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_PowerSurge_RecastDestonate, "Recast to spawn an "+"AoE".Emphasize() + " that applies "+"Ignite".Colorify(ExtendedColor.Chaos) },
        { Prefabs.SpellMod_PowerSurge_Shield, "Apply a "+"shield".Emphasize() },
        {
            Prefabs.SpellMod_PowerSurge_IncreaseDurationOnKill, "Kill an enemy to reduce "+"CD".Emphasize()
        },
        { Prefabs.SpellMod_PowerSurge_AttackSpeed, "Increase "+"AS".Emphasize() },
        { Prefabs.SpellMod_PowerSurge_Haste, "Increase "+"MS".Emphasize() },
        { Prefabs.SpellMod_PowerSurge_Lifetime, "Increase effect "+"duration".Emphasize() },
        { Prefabs.SpellMod_PowerSurge_EmpowerPhysical, "Increase "+"physical power".Emphasize() },
        { Prefabs.SpellMod_Chaos_Void_FragBomb, "Explosion conjurs "+"3 AoEs".Emphasize() + " that explode" },
        {
            Prefabs.SpellMod_Chaos_Void_BurnArea,
            "Explosion leaves an "+"AoE".Emphasize() + " that applies "+"Ignite".Colorify(ExtendedColor.Chaos)
        },
        { Prefabs.SpellMod_Chaos_Void_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_Shared_TargetAoE_IncreaseRange_Medium, "Increase "+"range".Emphasize() },
        { Prefabs.SpellMod_Chaos_Void_ReduceChargeCD, "Increase "+"recharge rate".Emphasize() },
        { Prefabs.SpellMod_VeilOfChaos_BonusIllusion, "Recast conjurs a second exploding "+"illusion".Emphasize() },
        { Prefabs.SpellMod_VeilOfChaos_ApplySnareOnExplode, "Explosion applies a "+"fading snare".Emphasize() },
        {
            Prefabs.SpellMod_VeilOfChaos_BonusDamageOnExplode,
            "Increase explosion "+"damage".Emphasize() + " of any illusion".Emphasize()
        },
        {
            Prefabs.SpellMod_CrystalLance_PierceEnemies,
            "Projectile "+"pierces".Emphasize() + " dealing "+"reduced damage".Emphasize()
        },
        {
            Prefabs.SpellMod_Shared_Frost_SplinterNovaOnFrosty,
            "Hit on a "+"Chilled/Frozen".Colorify(ExtendedColor.Frost) + " enemy throws "+"splinters".Emphasize()
        },
        {
            Prefabs.SpellMod_CrystalLance_BonusDamageToFrosty,
            "Increase damage to "+"Chilled/Frozen".Colorify(ExtendedColor.Frost) + " enemies"
        },
        { Prefabs.SpellMod_FrostBarrier_ConsumeAttackReduceCooldownXTimes, "Barrier hits decrease "+"CD".Emphasize() },
        {
            Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Recast,
            "Recast consumes "+"Chill".Colorify(ExtendedColor.Frost) + " and applies "+"Freeze".Colorify(ExtendedColor.Frost)
        },
        { Prefabs.SpellMod_FrostBarrier_KnockbackOnRecast, "Recast "+"pushes".Emphasize() + " enemies "+"back".Emphasize() },
        {
            Prefabs.SpellMod_FrostBarrier_ShieldOnFrostyRecast,
            "Recast hitting "+"Chilled".Colorify(ExtendedColor.Frost) + " enemy " + "shields".Emphasize() + " caster"
        },
        { Prefabs.SpellMod_FrostBarrier_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_FrostBarrier_BonusSpellPowerOnAbsorb, "Barrier hits increase "+"spell power".Emphasize() },
        { Prefabs.SpellMod_FrostBat_AreaDamage, "Hit conjurs an "+"AoE".Emphasize() + " that deals "+"damage".Emphasize() },
        {
            Prefabs.SpellMod_FrostBat_BonusDamageToFrosty,
            "Increase damage to "+"Chilled/Frozen".Colorify(ExtendedColor.Frost) + " enemies"
        },
        { Prefabs.SpellMod_IceNova_RecastLesserNova, "Recast to conjure an "+"AoE".Emphasize() + " that explodes" },
        {
            Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze,
            "Explosion consumes "+"Chill".Colorify(ExtendedColor.Frost) + " to apply "+"Freeze".Colorify(ExtendedColor.Frost)
        },
        { Prefabs.SpellMod_IceNova_ApplyShield, "Explosion "+"shields".Emphasize() + " caster and allies" },
        {
            Prefabs.SpellMod_IceNova_BonusDamageToFrosty,
            "Increase "+"damage".Emphasize() + " to "+"Chilled/Frozen".Colorify(ExtendedColor.Frost) + " enemies"
        },
        {
            Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack,
            "Next "+"attack".Emphasize()+" consumes "+"Chill".Colorify(ExtendedColor.Frost) +
            " and applies "+"Freeze".Colorify(ExtendedColor.Frost)
        },
        {
            Prefabs.SpellMod_Shared_Frost_ShieldOnFrosty,
            "Hit on a " + "Chilled".Colorify(ExtendedColor.Frost) + " enemy "+"shields".Emphasize() + " caster"
        },
        { Prefabs.SpellMod_VeilOfFrost_IllusionFrostBlast, "Illusion explodes in an "+"AoE".Emphasize()+" that applies "+"Chill".Colorify(ExtendedColor.Frost) },
        {
            Prefabs.SpellMod_MistTrance_ReduceSecondaryWeaponCD, "Trigger reduces secondary weapon skill "+"CD".Emphasize()
        },
        {
            Prefabs.SpellMod_MistTrance_PhantasmOnTrigger,
            "Trigger grants "+"Phantasm".Colorify(ExtendedColor.Illusion) + ""
        },
        { Prefabs.SpellMod_MistTrance_HasteOnTrigger, "Trigger increases "+"MS".Emphasize() },
        { Prefabs.SpellMod_MIstTrance_DamageOnAttack, "Trigger increases first "+"attack damage".Emphasize() },
        {
            Prefabs.SpellMod_MistTrance_FearOnTrigger,
            "Trigger applies "+"Fear".Colorify(ExtendedColor.Illusion) + " to enemies near caster"
        },
        { Prefabs.SpellMod_Shared_TravelBuff_IncreaseRange_Medium, "Increase "+"distance".Emphasize() + " travelled" },
        { Prefabs.SpellMod_Mosquito_ShieldOnSpawn, "Cast "+"shields".Emphasize() + " caster and allies" },
        {
            Prefabs.SpellMod_Mosquito_WispsOnDestroy,
            "Explosion summons "+"Wisps".Colorify(ExtendedColor.Illusion) + " that "+"heal".Emphasize()
        },
        { Prefabs.SpellMod_Mosquito_BonusDamage, "Increase "+"damage".Emphasize() },
        {
            Prefabs.SpellMod_Mosquito_BonusFearDuration,
            "Increase "+"Fear".Colorify(ExtendedColor.Illusion) + " duration".Emphasize()
        },
        { Prefabs.SpellMod_Mosquito_BonusHealthAndSpeed, "Increase summon "+"max HP".Emphasize() + " and "+"MS".Emphasize() },
        {
            Prefabs.SpellMod_PhantomAegis_ConsumeShieldAndPullAlly,
            "Recast to remove the effect and "+"pull the target".Emphasize()
        },
        { Prefabs.SpellMod_PhantomAegis_ExplodeOnDestroy, "Expiration conjurs an "+"AoE".Emphasize() +" that applies "+"Fear".Colorify(ExtendedColor.Illusion) },
        { Prefabs.SpellMod_PhantomAegis_IncreaseLifetime, "Increase "+"duration".Emphasize() },
        { Prefabs.SpellMod_Shared_MovementSpeed_Normal, "Increase "+"MS".Emphasize() },
        { Prefabs.SpellMod_PhantomAegis_IncreaseSpellPower, "Increase "+"spell power".Emphasize() },
        { Prefabs.SpellMod_SpectralWolf_FirstBounceInflictFadingSnare, "First hit applies a "+"fading snare".Emphasize() },
        {
            Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenSpawnWisp,
            "Hit consumes "+"Weaken".Colorify(ExtendedColor.Illusion) + " to spawn a "+"Wisp".Colorify(ExtendedColor.Illusion)
        },
        { Prefabs.SpellMod_SpectralWolf_ReturnToOwner, "Hit returns to caster on last bounce to "+"heal".Emphasize() },
        { Prefabs.SpellMod_SpectralWolf_AddBounces, "Increase max "+"bounces".Emphasize() + " by "+"1".Emphasize() },
        {
            Prefabs.SpellMod_SpectralWolf_DecreaseBounceDamageReduction,
            "Decrease "+"damage penalty".Emphasize() + " per bounce"
        },
        { Prefabs.SpellMod_WraithSpear_ShieldAlly, "Hit grants allies a "+"shield".Emphasize() },
        { Prefabs.SpellMod_WraithSpear_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_WraithSpear_ReducedDamageReduction, "Decrease "+"damage penalty".Emphasize() + " per hit" },
        {
            Prefabs.SpellMod_VeilOfIllusion_RecastDetonate,
            "Recast to "+"explode".Emphasize() + " illusion and apply" + " Weaken".Colorify(ExtendedColor.Illusion)
        },
        { Prefabs.SpellMod_VeilOfIllusion_IllusionProjectileDamage, "Illusion projectiles deal "+"damage".Emphasize() },
        {
            Prefabs.SpellMod_VeilOfIllusion_PhantasmOnHit,
            "Next "+"attack".Emphasize()+" grants "+"Phantasm".Colorify(ExtendedColor.Illusion)
        },
        {
            Prefabs.SpellMod_VeilOfIllusion_AttackInflictFadingSnare,
            "Next "+"attack".Emphasize()+" applies a "+"fading snare".Emphasize()
        },
        {
            Prefabs.SpellMod_BallLightning_DetonateOnRecast,
            "Recast "+"detonates".Emphasize() + " the ball to deal "+"damage".Emphasize()
        },
        {
            Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun_Explode,
            "Explosion consumes "+"Static".Colorify(ExtendedColor.Storm) + " to apply "+"Stun".Colorify(ExtendedColor.Storm)
        },
        {
            Prefabs.SpellMod_BallLightning_KnockbackOnExplode,
            "Explosion "+"pushes".Emphasize() + " enemies "+"back".Emphasize()
        },
        { Prefabs.SpellMod_BallLightning_Haste, "Explosion increases caster and ally "+"MS".Emphasize() },
        { Prefabs.SpellMod_BallLightning_BonusDamage, "Increase tick "+"damage".Emphasize() },
        {
            Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoWeaponCharge,
            "Hit consumes "+"Static".Colorify(ExtendedColor.Storm) + " to "+"Charge".Colorify(ExtendedColor.Storm)+ " weapon"
        },
        { Prefabs.SpellMod_Cyclone_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_Cyclone_IncreaseLifetime, "Increase projectile "+"duration".Emphasize() },
        { Prefabs.SpellMod_Discharge_BonusDamage, "Increase "+ "Storm Shield".Colorify(ExtendedColor.Storm)+" damage".Emphasize() },
        { Prefabs.SpellMod_Discharge_Immaterial, "Turn "+"immaterial".Emphasize() + " if 3 "+"Storm Shield".Colorify(ExtendedColor.Storm) + " are active" },
        { Prefabs.SpellMod_Discharge_IncreaseStunDuration, "Increase the " +"Stun".Colorify(ExtendedColor.Storm)+" duration" },
        { Prefabs.SpellMod_Discharge_RecastDetonate, "Recast to conjur an "+"AoE".Emphasize() + " that "+"knocks back".Emphasize()},
        { Prefabs.SpellMod_Discharge_SpellLeech, "Grants "+"spell life leech ".Emphasize() + "per "+"Storm Shield".Colorify(ExtendedColor.Storm) },
        { Prefabs.SpellMod_LightningWall_FadingSnare, "Hit applies a "+"fading snare".Emphasize() },
        { Prefabs.SpellMod_LightningWall_ApplyShield, "Hit on caster or ally grants a "+"shield".Emphasize() },
        {
            Prefabs.SpellMod_LightningWall_ConsumeProjectileWeaponCharge,
            "Block projectiles to "+ "Charge".Colorify(ExtendedColor.Storm)+ " weapon"
        },
        { Prefabs.SpellMod_LightningWall_BonusDamage, "Increase tick "+"damage".Emphasize() },
        { Prefabs.SpellMod_LightningWall_IncreaseMovementSpeed, "Increase " + "MS".Emphasize() },
        {
            Prefabs.SpellMod_Storm_PolarityShift_AreaImpactOrigin,
            "Hit conjurs an "+"AoE".Emphasize() + " at the caster's location"
        },
        {
            Prefabs.SpellMod_Storm_PolarityShift_AreaImpactDestination,
            "Teleport conjurs an " + "AoE".Emphasize() + " at the target's location"
        },
        { Prefabs.SpellMod_VeilOfStorm_SparklingIllusion, "Illusion conjurs an " + "AoE".Emphasize() + " that applies " + "Static".Colorify(ExtendedColor.Storm) },
        {
            Prefabs.SpellMod_VeilOfStorm_AttackInflictFadingSnare,
            "Next "+"attack".Emphasize() +" applies a " + "fading snare".Emphasize()
        },
        {
            Prefabs.SpellMod_Shared_Unholy_ApplyAgony,
            "Hit on "+"Condemned".Colorify(ExtendedColor.Unholy) + " enemy applies "+"Agony".Colorify(ExtendedColor.Unholy)
        },
        { Prefabs.SpellMod_CorpseExplosion_KillingBlow, "Hit on low HP enemy deals bonus "+"damage".Emphasize() },
        { Prefabs.SpellMod_CorpseExplosion_SkullNova, "Explosion conjurs "+"projectiles".Emphasize() +" applying " + "Condemn".Colorify(ExtendedColor.Unholy)},
        { Prefabs.SpellMod_CorpseExplosion_DoubleImpact, "Explosion conjurs an "+"AoE".Emphasize() +" applying " + "Condemn".Colorify(ExtendedColor.Unholy)},
        {
            Prefabs.SpellMod_CorpseExplosion_HealMinions,
            "Explosion heal and reset "+"Skeletons".Colorify(ExtendedColor.Unholy) + " uptime"
        },
        { Prefabs.SpellMod_CorpseExplosion_SnareBonus, "Explosion applies a "+"fading snare".Emphasize() },
        { Prefabs.SpellMod_CorpseExplosion_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_CorruptedSkull_LesserProjectiles, "Launch 2 "+"projectiles".Emphasize() + " that deal "+"less damage".Emphasize() },
        { Prefabs.SpellMod_CorruptedSkull_DetonateSkeleton, "Hit on allied "+"Skeleton".Colorify(ExtendedColor.Unholy)+" causes it to "+"explode".Emphasize() },
        { Prefabs.SpellMod_CorruptedSkull_BoneSpirit, "Hit conjurs a "+"projectile".Emphasize() + " that circles around target"},
        { Prefabs.SpellMod_CorruptedSkull_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_DeathKnight_SnareEnemiesOnSummon, "Cast applies a "+"fading snare".Emphasize() },
        { Prefabs.SpellMod_DeathKnight_BonusDamage, "Increase "+"damage".Emphasize() },
        {
            Prefabs.SpellMod_Shared_DispellDebuffs_Self,
            "Cast "+"removes".Emphasize()+" all "+"negative effects".Emphasize() + " from caster"
        },
        { Prefabs.SpellMod_Soulburn_ConsumeSkeletonEmpower, "Cast consumes skeletons to increase "+"power".Emphasize() },
        {
            Prefabs.SpellMod_Soulburn_ConsumeSkeletonHeal,
            "Cast consumes skeletons to "+"heal".Emphasize() + " per skeleton"
        },
        { Prefabs.SpellMod_Soulburn_IncreaseTriggerCount, "Increase targets hit by "+"1".Emphasize() },
        {
            Prefabs.SpellMod_Soulburn_IncreasedSilenceDuration,
            "Increase "+"Silence".Colorify(ExtendedColor.Unholy) + " duration"
        },
        { Prefabs.SpellMod_Soulburn_BonusDamage, "Increase "+"damage".Emphasize() },
        { Prefabs.SpellMod_Soulburn_BonusLifeDrain, "Increase "+"life drain".Emphasize() },
        {
            Prefabs.SpellMod_Soulburn_ReduceCooldownOnSilence,
            "Decrease "+"CD".Emphasize() + " for each "+"Silenced".Colorify(ExtendedColor.Unholy) + " enemy"
        },
        {
            Prefabs.SpellMod_WardOfTheDamned_MightSpawnMageSkeleton,
            "Barrier hit can summon a "+"Skeleton Mage".Colorify(ExtendedColor.Unholy)
        },
        {
            Prefabs.SpellMod_WardOfTheDamned_DamageMeleeAttackers,
            "Melee barrier hits deal "+"damage".Emphasize() + " to attacker"
        },
        {
            Prefabs.SpellMod_WardOfTheDamned_HealOnAbsorbProjectile, "Projectile barrier hits "+"heal".Emphasize() + " you"
        },
        {
            Prefabs.SpellMod_WardOfTheDamned_KnockbackOnRecast,
            "Recast "+"pushes".Emphasize() + " enemies "+"back".Emphasize()
        },
        {
            Prefabs.SpellMod_WardOfTheDamned_EmpowerSkeletonsOnRecast,
            "Recast increases allied "+"Skeleton".Colorify(ExtendedColor.Unholy)+" damage".Emphasize()
        },
        {
            Prefabs.SpellMod_WardOfTheDamned_ShieldSkeletonsOnRecast, "Recast "+"shields".Emphasize() + " allied skeletons"
        },
        { Prefabs.SpellMod_WardOfTheDamned_BonusDamageOnRecast, "Increase recast "+"damage".Emphasize() },
        {
            Prefabs.SpellMod_VeilOfBones_DashInflictCondemn,
            "Dashing through an enemy applies "+"Condemn".Colorify(ExtendedColor.Unholy)
        },
        { Prefabs.SpellMod_VeilOfBones_DashHealMinions, "Dashing through "+"Skeletons".Colorify(ExtendedColor.Unholy) +" resets & heal them" },
    };


    public static readonly Dictionary<string, PrefabGUID> abilityToPrefabDictionary = new Dictionary<string, PrefabGUID>
    {
        { "bloodfountain", Prefabs.AB_Blood_BloodFountain_AbilityGroup },
        { "bloodrage", Prefabs.AB_Blood_BloodRage_AbilityGroup },
        { "bloodrite", Prefabs.AB_Blood_BloodRite_AbilityGroup },
        { "sanguinecoil", Prefabs.AB_Blood_SanguineCoil_AbilityGroup },
        { "shadowbolt", Prefabs.AB_Blood_Shadowbolt_AbilityGroup },
        { "aftershock", Prefabs.AB_Chaos_Aftershock_Group },
        { "chaosbarrier", Prefabs.AB_Chaos_Barrier_AbilityGroup },
        { "powersurge", Prefabs.AB_Chaos_PowerSurge_AbilityGroup },
        { "void", Prefabs.AB_Chaos_Void_AbilityGroup },
        { "chaosvolley", Prefabs.AB_Chaos_Volley_AbilityGroup },
        { "crystallance", Prefabs.AB_Frost_CrystalLance_AbilityGroup },
        { "frostbat", Prefabs.AB_Frost_FrostBat_AbilityGroup },
        { "icenova", Prefabs.AB_Frost_IceNova_AbilityGroup },
        { "frostbarrier", Prefabs.AB_FrostBarrier_AbilityGroup },
        { "coldsnap", Prefabs.AB_Frost_ColdSnap_AbilityGroup },
        { "misttrance", Prefabs.AB_Illusion_MistTrance_AbilityGroup },
        { "mosquito", Prefabs.AB_Illusion_Mosquito_AbilityGroup },
        { "phantomaegis", Prefabs.AB_Illusion_PhantomAegis_AbilityGroup },
        { "spectralwolf", Prefabs.AB_Illusion_SpectralWolf_AbilityGroup },
        { "wraithspear", Prefabs.AB_Illusion_WraithSpear_AbilityGroup },
        { "balllightning", Prefabs.AB_Storm_BallLightning_AbilityGroup },
        { "cyclone", Prefabs.AB_Storm_Cyclone_AbilityGroup },
        { "discharge", Prefabs.AB_Storm_Discharge_AbilityGroup },
        { "lightningcurtain", Prefabs.AB_Storm_LightningWall_AbilityGroup },
        { "polarityshift", Prefabs.AB_Storm_PolarityShift_AbilityGroup },
        { "boneexplosion", Prefabs.AB_Unholy_CorpseExplosion_AbilityGroup },
        { "corruptedskull", Prefabs.AB_Unholy_CorruptedSkull_AbilityGroup },
        { "deathknight", Prefabs.AB_Unholy_DeathKnight_AbilityGroup },
        { "soulburn", Prefabs.AB_Unholy_Soulburn_AbilityGroup },
        { "wardofthedamned", Prefabs.AB_Unholy_WardOfTheDamned_AbilityGroup },
        { "veilofblood", Prefabs.AB_Vampire_VeilOfBlood_Group },
        { "veilofbones", Prefabs.AB_Vampire_VeilOfBones_AbilityGroup },
        { "veilofchaos", Prefabs.AB_Vampire_VeilOfChaos_Group },
        { "veiloffrost", Prefabs.AB_Vampire_VeilOfFrost_Group },
        { "veilofillusion", Prefabs.AB_Vampire_VeilOfIllusion_AbilityGroup },
        { "veilofstorm", Prefabs.AB_Vampire_VeilOfStorm_Group }
    };

    public static readonly Dictionary<PrefabGUID, string> prefabToAbilityNameDictionary =
        new Dictionary<PrefabGUID, string>();

    public static List<PrefabData> JewelPrefabData = new List<PrefabData>()
    {
        new PrefabData(Prefabs.AB_Blood_BloodFountain_AbilityGroup, "AB_Blood_BloodFountain_AbilityGroup",
            "Blood Fountain"),
        new PrefabData(Prefabs.AB_Blood_BloodRage_AbilityGroup, "AB_Blood_BloodRage_AbilityGroup", "Blood Rage"),
        new PrefabData(Prefabs.AB_Blood_BloodRite_AbilityGroup, "AB_Blood_BloodRite_AbilityGroup", "Blood Rite"),
        new PrefabData(Prefabs.AB_Blood_SanguineCoil_AbilityGroup, "AB_Blood_SanguineCoil_AbilityGroup",
            "Sanguine Coil"),
        new PrefabData(Prefabs.AB_Blood_Shadowbolt_AbilityGroup, "AB_Blood_Shadowbolt_AbilityGroup", "Shadowbolt"),
        new PrefabData(Prefabs.AB_Chaos_Aftershock_Group, "AB_Chaos_Aftershock_Group", "Aftershock"),
        new PrefabData(Prefabs.AB_Chaos_Barrier_AbilityGroup, "AB_Chaos_Barrier_AbilityGroup", "Chaos Barrier"),
        new PrefabData(Prefabs.AB_Chaos_PowerSurge_AbilityGroup, "AB_Chaos_PowerSurge_AbilityGroup", "Power Surge"),
        new PrefabData(Prefabs.AB_Chaos_Void_AbilityGroup, "AB_Chaos_Void_AbilityGroup", "Void"),
        new PrefabData(Prefabs.AB_Chaos_Volley_AbilityGroup, "AB_Chaos_Volley_AbilityGroup", "Chaos Volley"),
        new PrefabData(Prefabs.AB_Frost_CrystalLance_AbilityGroup, "AB_Frost_CrystalLance_AbilityGroup",
            "Crystal Lance"),
        new PrefabData(Prefabs.AB_Frost_FrostBat_AbilityGroup, "AB_Frost_FrostBat_AbilityGroup", "Frost Bat"),
        new PrefabData(Prefabs.AB_Frost_IceNova_AbilityGroup, "AB_Frost_IceNova_AbilityGroup", "Ice Nova"),
        new PrefabData(Prefabs.AB_FrostBarrier_AbilityGroup, "AB_FrostBarrier_AbilityGroup", "Frost Barrier"),
        new PrefabData(Prefabs.AB_Frost_ColdSnap_AbilityGroup, "AB_Frost_ColdSnap_AbilityGroup", "Cold Snap"),
        new PrefabData(Prefabs.AB_Illusion_MistTrance_AbilityGroup, "AB_Illusion_MistTrance_AbilityGroup",
            "Mist Trance"),
        new PrefabData(Prefabs.AB_Illusion_Mosquito_AbilityGroup, "AB_Illusion_Mosquito_AbilityGroup", "Mosquito"),
        new PrefabData(Prefabs.AB_Illusion_PhantomAegis_AbilityGroup, "AB_Illusion_PhantomAegis_AbilityGroup",
            "Phantom Aegis"),
        new PrefabData(Prefabs.AB_Illusion_SpectralWolf_AbilityGroup, "AB_Illusion_SpectralWolf_AbilityGroup",
            "Spectral Wolf"),
        new PrefabData(Prefabs.AB_Illusion_WraithSpear_AbilityGroup, "AB_Illusion_WraithSpear_AbilityGroup",
            "Wraith Spear"),
        new PrefabData(Prefabs.AB_Storm_BallLightning_AbilityGroup, "AB_Storm_BallLightning_AbilityGroup",
            "Ball Lightning"),
        new PrefabData(Prefabs.AB_Storm_Cyclone_AbilityGroup, "AB_Storm_Cyclone_AbilityGroup", "Cyclone"),
        new PrefabData(Prefabs.AB_Storm_Discharge_AbilityGroup, "AB_Storm_Discharge_AbilityGroup", "Discharge"),
        new PrefabData(Prefabs.AB_Storm_LightningWall_AbilityGroup, "AB_Storm_LightningWall_AbilityGroup",
            "Lightning Curtain"),
        new PrefabData(Prefabs.AB_Storm_PolarityShift_AbilityGroup, "AB_Storm_PolarityShift_AbilityGroup",
            "Polarity Shift"),
        new PrefabData(Prefabs.AB_Unholy_CorpseExplosion_AbilityGroup, "AB_Unholy_CorpseExplosion_AbilityGroup",
            "Bone Explosion"),
        new PrefabData(Prefabs.AB_Unholy_CorruptedSkull_AbilityGroup, "AB_Unholy_CorruptedSkull_AbilityGroup",
            "Corrupted Skull"),
        new PrefabData(Prefabs.AB_Unholy_DeathKnight_AbilityGroup, "AB_Unholy_DeathKnight_AbilityGroup",
            "Death Knight"),
        new PrefabData(Prefabs.AB_Unholy_Soulburn_AbilityGroup, "AB_Unholy_Soulburn_AbilityGroup", "Soulburn"),
        new PrefabData(Prefabs.AB_Unholy_WardOfTheDamned_AbilityGroup, "AB_Unholy_WardOfTheDamned_AbilityGroup",
            "Ward of the Damned"),
        new PrefabData(Prefabs.AB_Vampire_VeilOfBlood_Group, "AB_Vampire_VeilOfBlood_Group", "Veil of Blood"),
        new PrefabData(Prefabs.AB_Vampire_VeilOfBones_AbilityGroup, "AB_Vampire_VeilOfBones_AbilityGroup",
            "Veil of Bones"),
        new PrefabData(Prefabs.AB_Vampire_VeilOfChaos_Group, "AB_Vampire_VeilOfChaos_Group", "Veil of Chaos"),
        new PrefabData(Prefabs.AB_Vampire_VeilOfFrost_Group, "AB_Vampire_VeilOfFrost_Group", "Veil of Frost"),
        new PrefabData(Prefabs.AB_Vampire_VeilOfIllusion_AbilityGroup, "AB_Vampire_VeilOfIllusion_AbilityGroup",
            "Veil of Illusion"),
        new PrefabData(Prefabs.AB_Vampire_VeilOfStorm_Group, "AB_Vampire_VeilOfStorm_Group", "Veil of Storm")
    };

    public static readonly Dictionary<string, SchoolData> abilityToSchoolDictionary = new Dictionary<string, SchoolData>
    {
        { "bloodfountain", SchoolData.Blood },
        { "bloodrage", SchoolData.Blood },
        { "bloodrite", SchoolData.Blood },
        { "sanguinecoil", SchoolData.Blood },
        { "shadowbolt", SchoolData.Blood },
        { "aftershock", SchoolData.Chaos },
        { "chaosbarrier", SchoolData.Chaos },
        { "powersurge", SchoolData.Chaos },
        { "void", SchoolData.Chaos },
        { "chaosvolley", SchoolData.Chaos },
        { "crystallance", SchoolData.Frost },
        { "frostbat", SchoolData.Frost },
        { "icenova", SchoolData.Frost },
        { "frostbarrier", SchoolData.Frost },
        { "coldsnap", SchoolData.Frost },
        { "misttrance", SchoolData.Illusion },
        { "mosquito", SchoolData.Illusion },
        { "phantomaegis", SchoolData.Illusion },
        { "spectralwolf", SchoolData.Illusion },
        { "wraithspear", SchoolData.Illusion },
        { "balllightning", SchoolData.Storm },
        { "cyclone", SchoolData.Storm },
        { "discharge", SchoolData.Storm },
        { "lightningcurtain", SchoolData.Storm },
        { "polarityshift", SchoolData.Storm },
        { "boneexplosion", SchoolData.Unholy },
        { "corruptedskull", SchoolData.Unholy },
        { "deathknight", SchoolData.Unholy },
        { "soulburn", SchoolData.Unholy },
        { "wardofthedamned", SchoolData.Unholy },
        { "veilofblood", SchoolData.Blood },
        { "veilofbones", SchoolData.Unholy },
        { "veilofchaos", SchoolData.Chaos },
        { "veiloffrost", SchoolData.Frost },
        { "veilofillusion", SchoolData.Illusion },
        { "veilofstorm", SchoolData.Storm },
    };
}


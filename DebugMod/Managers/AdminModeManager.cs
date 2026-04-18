using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using ModCore.Events;
using ModCore.Listeners;
using ModCore.Models;
using ModCore;
using ModCore.Helpers;
using ModCore.Data;
using ModCore.Services;
using System.Threading;
using ProjectM.Gameplay.Systems;
using static ModCore.Helpers.Helper;
using ModCore.Factories;
using ProjectM.Scripting;
using HarmonyLib;
using static ProjectM.Network.InteractEvents_Client;
using Unity.Collections;
using Stunlock.Core;

namespace DebugMod.Managers
{
    public static class AdminModeManager
    {
        public static void Initialize()
        {
            GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
            GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
        }


        public static void Dispose()
        {
            GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
            GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
        }

        public static void HandleOnPlayerDisconnected(Player player)
        {
            if (player.IsAdmin)
            {
                player.Reset();
            }
        }

        private static void HandleOnPlayerBuffed(Player player, Entity buffEntity, PrefabGUID prefabGuid)
        {
            if (prefabGuid == Prefabs.Admin_Observe_Ghost_Buff || prefabGuid == Prefabs.Admin_Observe_Invisible_Buff)
            {
                if (SpectatingManager.SpectatorToSpectatedPlayer.ContainsKey(player)) return;
                if (!player.IsAdmin) return;
                if (buffEntity.Has<CanFly>()) return; //temporary workaround to not modify .stealth 

                if (prefabGuid == Prefabs.Admin_Observe_Ghost_Buff)
                {
                    ModifyBuff(buffEntity, BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.DisableMapCollision | BuffModificationTypes.ImmuneToSun | BuffModificationTypes.ImmuneToHazards, true);
                }
                else
                {
                    ModifyBuff(buffEntity, BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.DisableMapCollision | BuffModificationTypes.ImmuneToSun | BuffModificationTypes.ImmuneToHazards | BuffModificationTypes.AbilityCastImpair, true);
                }

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    StatType = UnitStatType.CooldownRecoveryRate,
                    Priority = 100,
                    Modifier = 1,
                    Value = 100000
                }, true);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    StatType = UnitStatType.PrimaryAttackSpeed,
                    Priority = 100,
                    Modifier = 1,
                    Value = 5
                }, false);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    Priority = 100,
                    StatType = UnitStatType.MovementSpeed,
                    Modifier = 1,
                    Value = 20
                }, false);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    Priority = 100,
                    StatType = UnitStatType.PhysicalPower,
                    Modifier = 1,
                    Value = 0
                }, false);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    Priority = 100,
                    StatType = UnitStatType.ResourcePower,
                    Modifier = 1,
                    Value = 0
                }, false);

                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    Priority = 100,
                    StatType = UnitStatType.SpellPower,
                    Modifier = 1,
                    Value = 0
                }, false);


                ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
                {
                    Id = ModificationIdFactory.NewId(),
                    ModificationType = ModificationType.Set,
                    Priority = 100,
                    StatType = UnitStatType.SiegePower,
                    Modifier = 1,
                    Value = 0
                }, false);

                /*                buffEntity.Add<AmplifyBuff>();
                                buffEntity.Write(new AmplifyBuff
                                {
                                    AmplifyModifier = 1000
                                });*/

                /*                buffEntity.Add<WeakenBuff>();
                                buffEntity.Write(new WeakenBuff
                                {
                                    WeakenModifier = 10
                                });*/
            }
        }
    }
}
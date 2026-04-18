using HarmonyLib;
using ProjectM.Network;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using ModCore.Data;
using ModCore.Services;
using ModCore;
using ModCore.Helpers;
using ModCore.Events;
using Il2CppSystem.Runtime.Remoting.Channels;
using Unity.Physics;
using ModCore.Models;
using ShopMod.Managers;

namespace ShopMod.Patches
{
    [HarmonyPatch(typeof(InteractWithPrisonerSystem), nameof(InteractWithPrisonerSystem.OnUpdate))]
    public static class InteractWithPrisonerSystemPatch
    {
        public static void Prefix(InteractWithPrisonerSystem __instance)
        {
            var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                if (!entity.Exists()) continue;
                var interactWithPrisonerEvent = entity.Read<InteractWithPrisonerEvent>();
                if (Helper.TryGetEntityFromNetworkId(interactWithPrisonerEvent.Prison, out var prisonEntity))
                {
                    var player = PlayerService.GetPlayerFromUser(entity.Read<FromCharacter>().User);
                    if (ShopZoneManager.ShopZone.Contains(prisonEntity))
                    {
                        var imprisonedEntity = prisonEntity.Read<PrisonCell>().ImprisonedEntity._Entity;
                        if (!imprisonedEntity.Exists())
                        {
                            player.ReceiveMessage("You cannot interact with an empty shop prison".Error());
                            entity.Destroy();
                            continue;
                        }

                        if (interactWithPrisonerEvent.PrisonInteraction == EventHelper.PrisonInteraction.Charm)
                        {
                            if (!Helper.HasBuff(player, Prefabs.AB_Shapeshift_DominatingPresence_PsychicForm_Buff))
                            {
                                continue;
                            }
                            
                            if (Helper.HasBuff(player, Prefabs.AB_Charm_Owner_HasCharmedTarget_Buff))
                            {
                                player.ReceiveMessage("You already have an active charmed target".Error());
                                entity.Destroy();
                                continue;
                            }

                            if (!Helper.PlayerHasEnoughItemsInInventory(player, ShopModConfig.Config.PrisonerCost.ItemPrefabGUID, ShopModConfig.Config.PrisonerCost.Quantity))
                            {
                                entity.Destroy();
                                player.ReceiveMessage($"You need {ShopModConfig.Config.PrisonerCost.Quantity.ToString().Warning()} {PointsModConfig.Config.MainVirtualCurrencyName.Warning()} to purchase a prisoner".Error());
                                continue;
                            }

                            player.ReceiveMessage($"You have bought a prisoner for {ShopModConfig.Config.PrisonerCost.Quantity.ToString().Warning()} {PointsModConfig.Config.MainVirtualCurrencyName.Warning()}".Success());
                            Helper.RemoveItemFromInventory(player, ShopModConfig.Config.PrisonerCost.ItemPrefabGUID, ShopModConfig.Config.PrisonerCost.Quantity);

                            var bloodType = imprisonedEntity.Read<BloodConsumeSource>().UnitBloodType;
                            var action = () => ShopHelper.SpawnPrisoner(prisonEntity, bloodType);
                            ActionScheduler.RunActionOnceAfterFrames(action, 2);
                        }
                        else
                        {
                            player.ReceiveMessage($"You cannot kill this prisoner".Error());
                            entity.Destroy();
                        }
                    }
                }
            }
            entities.Dispose();
        }
    }

    [HarmonyPatch(typeof(StartCraftingSystem), nameof(StartCraftingSystem.OnUpdate))]
    public static class StartCraftingSystemPatch
    {
        public static void Prefix(StartCraftingSystem __instance)
        {
            var entities = __instance._StartCraftItemEventQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                if (!entity.Exists()) continue;
                var startCraftItemEvent = entity.Read<StartCraftItemEvent>();
                if (Helper.TryGetEntityFromNetworkId(startCraftItemEvent.Workstation, out var workstationEntity) && workstationEntity.Has<PrisonCell>())
                {
                    var imprisonedEntity = workstationEntity.Read<PrisonCell>().ImprisonedEntity._Entity;
                    if (Helper.HasBuff(imprisonedEntity, Helper.CustomBuff1))
                    {
                        entity.Destroy();
                        continue;
                    }
                }
            }
            entities.Dispose();
        }
    }
}

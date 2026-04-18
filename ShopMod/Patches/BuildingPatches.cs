using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Scripting;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Shared.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using ModCore;
using ModCore.Data;
using ModCore.Events;
using ModCore.Helpers;
using ModCore.Models;
using ModCore.Services;

namespace ShopMod.Patches
{
    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
    public static class PlaceTileModelSystemPatch
    {
        public static void Prefix(PlaceTileModelSystem __instance)
        {
            var entities = __instance._DismantleTileQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                try
                {
                    var fromCharacter = entity.Read<FromCharacter>();
                    Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
                    var dismantleTileModelEvent = entity.Read<DismantleTileModelEvent>();
                    if (Helper.TryGetEntityFromNetworkId(dismantleTileModelEvent.Target, out var structure))
                    {
                        if (!Team.IsAllies(structure.Read<Team>(), player.Character.Read<Team>()))
                        {
                            if (!player.IsAdmin)
                            {
                                var interactedUpon = structure.Read<InteractedUpon>();
                                interactedUpon.Interacting = false;
                                interactedUpon.BlockBuildingDisassemble = false;
                                interactedUpon.BlockBuildingMovement = false;
                                structure.Write(interactedUpon);
                                player.ReceiveMessage($"You can only dismantle structures that belong to you".Error());
                                entity.Destroy();
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

            entities = __instance._MoveTileQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                try
                {
                    var fromCharacter = entity.Read<FromCharacter>();
                    Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
                    var moveTileModelEvent = entity.Read<MoveTileModelEvent>();
                    if (Helper.TryGetEntityFromNetworkId(moveTileModelEvent.Target, out var structure))
                    {
                        if (!Team.IsAllies(structure.Read<Team>(), player.Character.Read<Team>()))
                        {
                            if (!player.IsAdmin)
                            {
                                var interactedUpon = structure.Read<InteractedUpon>();
                                interactedUpon.Interacting = false;
                                interactedUpon.BlockBuildingDisassemble = false;
                                interactedUpon.BlockBuildingMovement = false;
                                structure.Write(interactedUpon);
                                player.ReceiveMessage($"You can only move structures that belong to you".Error());
                                entity.Destroy();
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

            entities = __instance._SetVariationQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                try
                {
                    var fromCharacter = entity.Read<FromCharacter>();
                    Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
                    var setTileModelVariationEvent = entity.Read<SetTileModelVariationEvent>();
                    if (Helper.TryGetEntityFromNetworkId(setTileModelVariationEvent.Target, out var structure))
                    {
                        if (!Team.IsAllies(structure.Read<Team>(), player.Character.Read<Team>()))
                        {
                            if (!player.IsAdmin)
                            {
                                var interactedUpon = structure.Read<InteractedUpon>();
                                interactedUpon.Interacting = false;
                                interactedUpon.BlockBuildingDisassemble = false;
                                interactedUpon.BlockBuildingMovement = false;
                                structure.Write(interactedUpon);
                                player.ReceiveMessage($"You can only modify structures that belong to you".Error());
                                entity.Destroy();
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
}

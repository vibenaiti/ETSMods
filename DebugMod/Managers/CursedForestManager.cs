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
using ProjectM.Terrain;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Scripting;
using Stunlock.Core;

namespace DebugMod.Managers
{
    public static class CursedForestManager
    {
        private static List<Timer> Timers = new();

        private static HashSet<Entity> CursedForestHeartTerritories = new();
        private static HashSet<int> TerritoryIndices = new()
        {
            129,
            128,
            127,
            138
        };
        public static void Initialize()
        {
            var entities = Helper.GetEntitiesByComponentTypes<CastleTerritory>();
            foreach (var entity in entities)
            {
                var castleTerritory = entity.Read<CastleTerritory>();
                if (TerritoryIndices.Contains(castleTerritory.CastleTerritoryIndex))
                {
                    CursedForestHeartTerritories.Add(entity);
                }
            }

            GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
            GameEvents.OnPlayerSetMapMarker += HandleOnPlayerSetMapMarker;

            var action = () => RemoveCurseIfInBase();
            Timers.Add(ActionScheduler.RunActionEveryInterval(action, 1f));
        }


        public static void Dispose()
        {
            GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
            GameEvents.OnPlayerSetMapMarker -= HandleOnPlayerSetMapMarker;

            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();
        }

        public static void HandleOnPlayerBuffed(Player player, Entity buffEntity, PrefabGUID prefabGUID)
        {
            if (prefabGUID == Prefabs.Buff_General_CurseOfTheForest_Area)
            {
                var eventEntity = Helper.CreateEntityWithComponents<FromCharacter, DeleteMapMarkerEvent>();
                eventEntity.Write(player.ToFromCharacter());
            }
        }

        public static void HandleOnPlayerSetMapMarker(Player player, Entity eventEntity, SetMapMarkerEvent setMapMarkerEvent)
        {
            if (Helper.HasBuff(player, Prefabs.Buff_General_CurseOfTheForest_Area))
            {
                eventEntity.Destroy();
                player.ReceiveMessage("You cannot set a map marker while you have the Curse of the Forest!".Error());
            }
        }

        public static void RemoveCurseIfInBase()
        {
            foreach (var territory in CursedForestHeartTerritories)
            {
                var territoryInfo = territory.Read<CastleTerritory>();
                var heart = territoryInfo.CastleHeart;

                if (heart.Exists())
                {
                    var heartInfo = heart.Read<CastleHeart>();
                    var castleBreached = heartInfo.IsRaided() || heartInfo.IsSieged() || heartInfo.IsDecaying();
                    var ownerEntity = heart.Read<UserOwner>().Owner._Entity;
                    if (!ownerEntity.Exists()) continue;
                    var owner = PlayerService.GetPlayerFromUser(ownerEntity);
                    var allies = owner.GetClanMembers();
                    foreach (var player in allies)
                    {
                        if (player.IsInBase(out var currentTerritory, out var territoryAlignment))
                        {
                            if (CursedForestHeartTerritories.Contains(currentTerritory) && territoryAlignment == TerritoryAlignment.Friendly)
                            {
                                if (Helper.TryGetBuff(player, Prefabs.Buff_General_CurseOfTheForest_Area, out var buffEntity))
                                {
                                    var curseAreaDebuffServer = buffEntity.Read<Script_CursedAreaDebuff_DataServer>();
                                    curseAreaDebuffServer.ImmunityBuff = castleBreached ? PrefabGUID.Empty : Prefabs.Buff_General_CurseOfTheForest_Area;
                                    buffEntity.Write(curseAreaDebuffServer);   
                                }
                            }
                        }
                        else
                        {
                            if (Helper.TryGetBuff(player, Prefabs.Buff_General_CurseOfTheForest_Area, out var buffEntity))
                            {
                                var curseAreaDebuffServer = buffEntity.Read<Script_CursedAreaDebuff_DataServer>();
                                curseAreaDebuffServer.ImmunityBuff = PrefabGUID.Empty;
                                buffEntity.Write(curseAreaDebuffServer);
                            }
                        }
                    }
                }
            }
        }
    }
}
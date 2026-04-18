using ModCore;
using ModCore.Data;
using ModCore.Events;
using ModCore.Factories;
using ModCore.Helpers;
using ModCore.Models;
using ModCore.Services;
using OpenWorldEvents.Managers;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static ProjectM.NetherSpawnPositionMetadata;

namespace OpenWorldEvents.GameModes
{
    public class BossContestGameMode
    {
        public string UnitGameModeType => $"BossContest{ArenaNumber}";
        public int ArenaNumber = 0;
        public bool MatchActive = false;
        public Player Player1 = null;
        public Player Player2 = null;
        public Entity Boss1 = Entity.Null;
        public Entity Boss2 = Entity.Null;
        public float3 PlayerLocation1 = new();
        public float3 BossLocation1 = new();
        public float3 PlayerLocation2 = new();
        public float3 BossLocation2 = new();
        public List<Timer> Timers = new List<Timer>();
        public bool IsOccupied { get; set; }
        public Stopwatch Stopwatch { get; set; } = new Stopwatch();
        public bool IsFullLoot { get; set; } = false;

        public static Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
        {
            ResetCooldowns = true,
            RemoveConsumables = false,
            RemoveShapeshifts = true
        };

        private static HashSet<string> AllowedCommands = new HashSet<string>
        {
            "ping",
            "help",
            "legendary",
            "jewel",
            "forfeit",
            "points",
            "lb ranked",
        };

        public BossContestGameMode()
        {
            ResetOptions = new Helper.ResetOptions();
        }

        private Dictionary<PrefabGUID, Action<Player, Entity>> _buffHandlers;

        public void Initialize(Player player1, Player player2)
        {
            IsOccupied = true;

            Player1 = player1;
            Player2 = player2;
            Stopwatch.Reset();
            Stopwatch.Start();

            Initialize();
        }

        public void Initialize()
        {
            MatchActive = true;
            GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
            GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
            GameEvents.OnUnitDeath += HandleOnUnitDeath;

            var action = () => 
            {
                BossContestManager.EndMatch(ArenaNumber, Player1, Player2, false);
            };
            Timers.Add(ActionScheduler.RunActionOnceAfterDelay(action, BossContestConfig.Config.MaxBossFightTime));
        }

        public void Dispose()
        {
            IsOccupied = false;

            Player1 = null;
            Player2 = null;

            Stopwatch.Reset();

            MatchActive = false;

            GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
            GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
            GameEvents.OnUnitDeath -= HandleOnUnitDeath;

            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();

            if (Boss1.Exists())
            {
                Helper.KillOrDestroyEntity(Boss1);
            }
            if (Boss2.Exists())
            {
                Helper.KillOrDestroyEntity(Boss2);
            }
        }

        public void HandleOnUnitDeath(Entity unit, DeathEvent deathEvent)
        {
            if (unit != Boss1 && unit != Boss2) return;

            if (unit == Boss1)
            {
                BossContestManager.EndMatch(ArenaNumber, Player1, Player2);
            }
            else if (unit == Boss2)
            {
                BossContestManager.EndMatch(ArenaNumber, Player2, Player1);
            }
        }

        public void HandleOnPlayerDowned(Player player, Entity killer)
        {
            if (player != Player1 && player != Player2) return;

            if (player == Player1)
            {
                BossContestManager.EndMatch(ArenaNumber, Player2, Player1);
            }
            else
            {
                BossContestManager.EndMatch(ArenaNumber, Player1, Player2);
            }
        }

        public void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
        {
            if (player != Player1 && player != Player2) return;
            
            if (player == Player1)
            {
                BossContestManager.EndMatch(ArenaNumber, Player2, Player1);
            }
            else
            {
                BossContestManager.EndMatch(ArenaNumber, Player1, Player2);
            }
        }

        public void SpawnBoss(PrefabGUID prefab, int index, int level)
        {
            var boss = new Boss(prefab);

            boss.Category = UnitGameModeType;
            boss.Level = level;
            boss.SoftSpawn = false;
            boss.MaxDistanceFromPreCombatPosition = 40;
            boss.DrawsAggro = true;
            boss.AggroRadius = 3;
            var spawnPosition = index == 1 ? BossLocation1 : BossLocation2;
            UnitFactory.SpawnUnitWithCallback(boss, spawnPosition, (e) =>
            {
                if (index == 1)
                {
                    Boss1 = e;
                }
                else
                {
                    Boss2 = e;
                }
            });
        }
    }
}

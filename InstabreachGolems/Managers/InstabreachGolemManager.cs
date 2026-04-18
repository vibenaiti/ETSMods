using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModCore.Events;
using ModCore.Models;
using Unity.Entities;
using System.Threading;
using ModCore;
using ProjectM.CastleBuilding;
using ModCore.Services;
using ModCore.Helpers;

namespace InstabreachGolem.Managers
{
    public static class InstabreachGolemManager
    {
        private static Dictionary<Player, Entity> PlayerToBreachedHearts = new Dictionary<Player, Entity>();
        private static List<Timer> Timers = new();
        public static void Initialize()
        {
            GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
        }

        public static void Dispose()
        {
            foreach (var timer in Timers)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Timers.Clear();

            PlayerToBreachedHearts.Clear();
            GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
        }

        public static void HandleOnPlayerDamageDealt(Player player, Entity eventEntity, DealDamageEvent dealDamageEvent)
        {
            if (dealDamageEvent.MaterialModifiers.StoneStructure > 0)
            {
                if (!eventEntity.Exists()) return;

                if (!PlayerToBreachedHearts.TryGetValue(player, out var heart))
                {
                    if (!Helper.IsRaidHour()) return;
                    if (dealDamageEvent.Target.Exists() && dealDamageEvent.Target.Has<CastleHeartConnection>())
                    {
                        var targetedHeart = dealDamageEvent.Target.Read<CastleHeartConnection>().CastleHeartEntity._Entity;
                        if (targetedHeart.Exists())
                        {
                            var owner = targetedHeart.Read<UserOwner>().Owner._Entity;
                            if (owner.Exists())
                            {
                                var ownerPlayer = PlayerService.GetPlayerFromUser(owner);
/*                                if (!ownerPlayer.IsOnline)
                                {
                                    dealDamageEvent.MaterialModifiers.StoneStructure = 1;
                                    eventEntity.Write(dealDamageEvent);
                                    return;
                                }*/
                            }
                            PlayerToBreachedHearts[player] = targetedHeart;
                            var action = () =>
                            {
                                PlayerToBreachedHearts.Remove(player);
                            };
                            ActionScheduler.RunActionOnceAfterDelay(action, 420);
                            player.ReceiveMessage("You have used your insta breach. You will not be able to attack another castle until this breach is over!".Emphasize());
                        }
                    }
                }
                else
                {
                    dealDamageEvent.MaterialModifiers.StoneStructure = 0;
                    eventEntity.Write(dealDamageEvent);
                }
            }
        }
    }
}

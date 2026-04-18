using ProjectM;
using Unity.Mathematics;
using ModCore;
using ModCore.Helpers;

namespace OpenWorldEvents
{
    public static class EventHelper
    {
        public static void SetDeathDurabilityLoss(float durability)
        {
            Core.serverGameSettingsSystem._Settings.Death_DurabilityFactorLoss = durability;
            var entity = Helper.GetEntitiesByComponentTypes<ServerGameBalanceSettings>()[0];
            var serverGameBalanceSettings = entity.Read<ServerGameBalanceSettings>();
            serverGameBalanceSettings.Death_DurabilityFactorLoss = new half(durability);
            entity.Write(serverGameBalanceSettings);
        }
    }
}

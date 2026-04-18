using Il2CppInterop.Runtime;
using ModCore;
using ModCore.Events;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace DebugMod.Listeners
{
    public static class InventoryChangedListener
    {
        private static EntityQueryOptions Options = EntityQueryOptions.Default;
        private static EntityQueryDesc QueryDesc;
        private static EntityQuery Query;
        private static bool Initialized = false;

        public static void Initialize()
        {
            if (Initialized) return;
            QueryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                new ComponentType(Il2CppType.Of<InventoryChangedEvent>(), ComponentType.AccessMode.ReadWrite)
                },
                Options = Options
            };
            Query = VWorld.Server.EntityManager.CreateEntityQuery(QueryDesc);

            GameEvents.OnGameFrameUpdate += OnUpdate;
            Initialized = true;
        }

        public static void Dispose()
        {
            GameEvents.OnGameFrameUpdate -= OnUpdate;
            Initialized = false;
        }

        private static void OnUpdate()
        {
            var entities = Query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                if (entity.Exists())
                {
                    var inventoryChangedEvent = entity.Read<InventoryChangedEvent>();
                    Plugin.PluginLog.LogInfo($"{inventoryChangedEvent.InventoryEntity.LookupName()} {inventoryChangedEvent.Item.LookupName()} {inventoryChangedEvent.Amount} {inventoryChangedEvent.ChangeType}");
                }
            }

            entities.Dispose();
        }
    }

}

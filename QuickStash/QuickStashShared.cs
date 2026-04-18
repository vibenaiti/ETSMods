using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.CastleBuilding;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace QuickStash
{
    public class QuickStashShared
    {
        private static ComponentType[] _containerComponents = null;
        private static ComponentType[] _workstationComponents = null;
        private static ComponentType[] ContainerComponents
        {
            get
            {
                if (_containerComponents == null)
                {
                    _containerComponents = new[] {

                        ComponentType.ReadOnly(Il2CppType.Of<Team>()),
                        ComponentType.ReadOnly(Il2CppType.Of<CastleHeartConnection>()),
                        ComponentType.ReadOnly(Il2CppType.Of<InventoryInstanceElement>()),
                        ComponentType.ReadOnly(Il2CppType.Of<NameableInteractable>()),
                    };
                }
                return _containerComponents;
            }
        }
        private static ComponentType[] WorkstationComponents
        {
            get
            {
                if (_workstationComponents == null)
                {
                    _workstationComponents = new[] {
                        ComponentType.ReadOnly(Il2CppType.Of<CastleWorkstation>()),
                    };
                }
                return _workstationComponents;
            }
        }

        public static NativeArray<Entity> GetStashEntities(EntityManager entityManager)
        {
            var queryDesc = new EntityQueryDesc
            {
                None = WorkstationComponents,
                All = ContainerComponents
            };
            var query = entityManager.CreateEntityQuery(queryDesc);
            return query.ToEntityArray(Allocator.Temp);
        }
    }
}

using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Transforms;
using ModCore.Data;
using ModCore;
using ModCore.Factories;
using ModCore.Helpers;
using Stunlock.Core;

namespace ShopMod
{
    public static class ShopHelper
    {
        private static Dictionary<PrefabGUID, PrefabGUID> BloodToUnit = new()
        {
            { Prefabs.BloodType_Brute, Prefabs.CHAR_Militia_Light },
            { Prefabs.BloodType_Worker, Prefabs.CHAR_ChurchOfLight_Villager_Female },
            { Prefabs.BloodType_Scholar, Prefabs.CHAR_Farmlands_Villager_Female_Sister },
            { Prefabs.BloodType_Warrior, Prefabs.CHAR_Militia_Guard },
            { Prefabs.BloodType_Rogue, Prefabs.CHAR_Militia_Crossbow },
            { Prefabs.BloodType_Creature, Prefabs.CHAR_ChurchOfLight_Villager_Female },
        };
        public static void SpawnPrisoner(Entity prisonCell, PrefabGUID bloodType)
        {
            if (BloodToUnit.TryGetValue(bloodType, out var prisonerGuid))
            {
                if (prisonCell.Exists())
                {
                    var prisonCellData = prisonCell.Read<PrisonCell>();
                    var prisoner = prisonCellData.ImprisonedEntity._Entity;
                    if (!prisoner.Exists())
                    {
                        var unit = new Unit(prisonerGuid);
                        unit.IsImmaterial = true;
                        unit.IsInvulnerable = true;
                        UnitFactory.SpawnUnitWithCallback(unit, prisonCell.Read<LocalToWorld>().Position, (e) =>
                        {

                            e.Add<Imprisoned>();
                            e.Write(new Imprisoned
                            {
                                PrisonCellEntity = prisonCell
                            });

                            prisonCellData.ImprisonedEntity = e;
                            prisonCell.Write(prisonCellData);

                            Helper.BuffEntity(e, Prefabs.ImprisonedBuff, out var buffEntity, Helper.NO_DURATION);

                            var bloodConsumeSource = e.Read<BloodConsumeSource>();
                            bloodConsumeSource.BloodQuality = 100;
                            bloodConsumeSource.UnitBloodType._Value = bloodType;
                            e.Write(bloodConsumeSource);
                        });
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public static class GenTiberium
    {
        public static byte GetWindExposure(TiberiumCrystal thing)
        {
            return (byte)Mathf.Min(255f * thing.def.tiberium.topWindExposure, 255f);
        }

        public static void SetWindExposureColors(Color32[] colors, TiberiumCrystal thing)
        {
            colors[1].a = (colors[2].a = GenTiberium.GetWindExposure(thing));
            colors[0].a = (colors[3].a = 0);
        }

        public static bool CanEverGrowTo(this ThingDef crystalDef, IntVec3 c, Map map)
        {
            if (!c.InBounds(map))
            {
                return false;
            }
            List<Thing> list = map.thingGrid.ThingsListAt(c);
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (thing.def.category == ThingCategory.Building)
                {
                    return false;
                }
                if (thing.def.thingClass == typeof(TiberiumCrystal))
                {
                    return false;
                }
                if (crystalDef.passability == Traversability.Impassable && (thing.def.category == ThingCategory.Pawn || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Building))
                {
                    return false;
                }
            }
            if (crystalDef.passability == Traversability.Impassable)
            {
                for (int j = 0; j < 4; j++)
                {
                    IntVec3 c2 = c + GenAdj.CardinalDirections[j];
                    if (c2.InBounds(map))
                    {
                        Building edifice = c2.GetEdifice(map);
                        if (edifice != null && edifice.def.IsDoor)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}

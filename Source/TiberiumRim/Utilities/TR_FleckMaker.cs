using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim 
{
    public static class TR_FleckMaker
    {
        public static void ThrowTiberiumAirPuff(Vector3 loc, Map map)
        {
            if (!loc.ToIntVec3().ShouldSpawnMotesAt(map)) return;

            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc + new Vector3(Rand.Range(-0.02f, 0.02f), 0f, Rand.Range(-0.02f, 0.02f)), map, TiberiumDefOf.TiberiumAirPuff, 1.5f);
            dataStatic.rotationRate = (float)Rand.RangeInclusive(-240, 240);
            dataStatic.velocityAngle = (float)Rand.Range(-45, 45);
            dataStatic.velocitySpeed = Rand.Range(1.2f, 1.5f);
            map.flecks.CreateFleck(dataStatic);
        }

        public static void ThrowTiberiumSmoke(Vector3 loc, Map map, float size)
        {
            if (!loc.ShouldSpawnMotesAt(map)) return;

            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, TiberiumDefOf.TiberiumSmoke, Rand.Range(1.5f, 2.5f) * size);
            dataStatic.rotationRate = Rand.Range(-30f, 30f);
            dataStatic.velocityAngle = (float)Rand.Range(30, 40);
            dataStatic.velocitySpeed = Rand.Range(0.5f, 0.7f);
            map.flecks.CreateFleck(dataStatic);
        }

        public static void ThrowTiberiumLeak(Vector3 loc, Map map, Rot4 rotation, Color color)
        {
            if (!loc.ShouldSpawnMotesAt(map)) return;

            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, TiberiumDefOf.TiberiumSmoke, 0.55f);
            dataStatic.instanceColor = color;
            dataStatic.rotationRate = TRandom.RangeInclusive(-240, 240); ;
            dataStatic.spawnPosition += new Vector3(TRandom.Range(-0.02f, 0.02f), 0f, TRandom.Range(-0.02f, 0.02f));
            dataStatic.velocityAngle = rotation.AsAngle + TRandom.Range(-16, 16);
            dataStatic.velocitySpeed = TRandom.Range(1.85f, 2.5f);
            map.flecks.CreateFleck(dataStatic);
        }
    }
}

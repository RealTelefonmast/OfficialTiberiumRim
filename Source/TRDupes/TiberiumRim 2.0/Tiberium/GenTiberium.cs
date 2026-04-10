using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim.Tiberium
{
    public static class GenTiberium
    {
        public static TiberiumEntity Spawn(IntVec3 pos, Map map, TiberiumDef def)
        {
            TiberiumEntity entity = (TiberiumEntity)Activator.CreateInstance(def.workerClass);
            map.GetComponent<MapComponent_TiberiumEntityManager>().RegisterEntity(entity);
            entity.def = def;
            entity.Position = pos;
            entity.SpawnSetup(map, false);
            return entity;
        }
    }
}

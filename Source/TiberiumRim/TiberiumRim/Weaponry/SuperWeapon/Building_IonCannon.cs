using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TiberiumRim
{
    public class Building_IonCannon : TRBuilding
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            AttackSatellite_Ion asat = (AttackSatellite_Ion) WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("ASat_Ion"));
            asat.Tile = Tile;
            Find.WorldObjects.Add(asat);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("Current ASATS: " + TiberiumRimComp.AttackSatelliteNetwork.ASatsIon.Count);
            return sb.ToString().TrimEndNewlines();
        }
    }
}

using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class Building_IonCannon : TRBuilding
    {

        //FX
        public override bool? FX_ShouldDraw(FXLayerArgs args)
        {
            return args.index switch
            {
                0 => true,
                1 => CentralLight
            };
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            AttackSatellite_Ion asat = (AttackSatellite_Ion) WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("ASat_Ion"));
            asat.Tile = Tile;
            Find.WorldObjects.Add(asat);
        }

        public bool CentralLight => true;

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("Current ASATS: " + TiberiumRimComp.SatelliteInfo.AttackSatelliteNetwork.ASatsIon.Count);
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (DebugSettings.godMode)
            {

            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class VeinHub : TRBuilding
    {
        public Veinhole parent;
        public CellArea affectedArea;
        public List<IntVec3> AffectedCells = new List<IntVec3>();
        public float radius = 12.59f;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            affectedArea = new CellArea();
            AffectedCells = GenRadial.RadialCellsAround(Position, radius, false).ToList();
        }

        private bool Dying => parent.DestroyedOrNull();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref parent, "veinParent");
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            parent.RemoveHub(this);
            base.Destroy(mode);
        }

        public override void TickRare()
        {
            /*
            if (Dying)
            {
                TakeDamage(new DamageInfo(DamageDefOf.Crush, Rand.Range(3f, 13f)));
                return;
            }
            */
            foreach (var cell in AffectedCells)
            {
                var pawn = cell.GetFirstPawn(Map);
                if (pawn != null && TRandom.Chance(0.86f))
                {
                    LaunchGas(pawn);
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
            GenDraw.DrawFieldEdges(AffectedCells, Color.green);
        }

        private void LaunchGas(Pawn pawn)
        {
            VeinGasCloud gas = (VeinGasCloud)GenSpawn.Spawn(ThingDef.Named("VeinGasCloud"), Position, Map);
            gas.SetTarget(pawn);
        }
    }
}

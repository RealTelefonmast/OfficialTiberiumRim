using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Noise;

namespace TiberiumRim
{
    public class Comp_Suppression : ThingComp
    {
        public CompProperties_Suppression Props => (CompProperties_Suppression) base.props;

        public MapComponent_Suppression Suppression { get; set; }
        public CompPowerTrader PowerComp { get; set; }
        public List<IntVec3> SuppressionCells { get; set; } = new List<IntVec3>();

        public bool IsPowered => PowerComp.PowerOn;
        public bool SuppressingNow => IsPowered;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Suppression = parent.Map.GetComponent<MapComponent_Suppression>();
            PowerComp = parent.GetComp<CompPowerTrader>();

            Suppression.RegisterSuppressor(this);
        }

        public override void PostDeSpawn(Map map)
        {
            Suppression.DeregisterSuppressor(this);
            base.PostDeSpawn(map);
        }

        public bool CoversCell(IntVec3 cell)
        {
            return SuppressionCells.Contains(cell);
        }

        public bool AffectsCell(IntVec3 cell)
        {
            if (!SuppressingNow) return false;
            return SuppressionCells.Contains(cell);
        }

        public void UpdateSuppressionCells()
        {
            //Select all potentially coverable cells and define new suppression area
            bool Predicate(IntVec3 c) => !c.Roofed(parent.Map) && GenSight.LineOfSight(parent.Position, c, parent.Map);
            SuppressionCells = TRUtils.SectorCells(parent.Position, parent.Map, Props.radius, Props.angle, parent.Rotation.AsAngle, false, Predicate).ToList();
            
        }

        public override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
                case "PowerTurnedOff":
                    Suppression.Toggle(this, false);
                    break;
                case "PowerTurnedOn":
                    Suppression.Toggle(this, true);
                    break;
            }

            base.ReceiveCompSignal(signal);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (Find.Selector.IsSelected(parent))
            {
                var coveredCells = Suppression.CoveredCells.ToList();
                var suppressedCells = Suppression.SuppressedCells.ToList();
                GenDraw.DrawFieldEdges(coveredCells, Color.gray);
                GenDraw.DrawFieldEdges(suppressedCells, Color.cyan);
            }
        }
    }

    public class CompProperties_Suppression : CompProperties
    {
        public float radius;
        public float angle;

        public CompProperties_Suppression()
        {
            compClass = typeof(Comp_Suppression);
        }
    }
}

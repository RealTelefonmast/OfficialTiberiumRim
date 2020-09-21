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

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Suppression = parent.Map.GetComponent<MapComponent_Suppression>();
            PowerComp = parent.GetComp<CompPowerTrader>();
            Suppression.RegisterSuppressor(this);
            Suppression.UpdateGrid(this);
        }

        public override void PostDeSpawn(Map map)
        {
            Suppression.DeregisterSuppressor(this);
            Suppression.RemoveFromGrid(SuppressionCells);
            base.PostDeSpawn(map);
        }

        public void UpdateSuppressionCells()
        {
            Suppression.RemoveFromGrid(SuppressionCells);

            bool Predicate(IntVec3 c) => !c.Roofed(parent.Map) && GenSight.LineOfSight(parent.Position, c, parent.Map);
            SuppressionCells = TRUtils.SectorCells(parent.Position, parent.Map, Props.radius, Props.angle, parent.Rotation.AsAngle, false, Predicate).ToList();

            Suppression.AddToGrid(SuppressionCells);
        }

        public override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
                case "PowerTurnedOff":
                    Suppression.RemoveFromGrid(SuppressionCells);
                    break;
                case "PowerTurnedOn":
                    Suppression.AddToGrid(SuppressionCells);
                    break;
            }

            base.ReceiveCompSignal(signal);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (Find.Selector.IsSelected(parent))
            {
                Color color = IsPowered ? Color.cyan : Color.gray;
                GenDraw.DrawFieldEdges(SuppressionCells, color);
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

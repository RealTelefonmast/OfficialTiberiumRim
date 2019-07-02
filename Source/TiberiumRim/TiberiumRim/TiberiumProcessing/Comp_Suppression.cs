using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{

    public class Comp_Suppression : ThingComp
    {
        private List<IntVec3> cells = new List<IntVec3>();
        private MapComponent_Tiberium tiberium;
        private MapComponent_Suppression suppression;

        public CompPowerTrader PowerComp;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            PowerComp = parent.GetComp<CompPowerTrader>();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDeSpawn(Map map)
        {
            Suppression.DeregisterSuppressor(this.parent);
            base.PostDeSpawn(map);
        }

        public CompProperties_Suppression Props => (CompProperties_Suppression)base.props;

        public MapComponent_Tiberium Tiberium
        {
            get
            {
                if(tiberium == null)
                   tiberium = parent.Map.GetComponent<MapComponent_Tiberium>();
                return tiberium;
            }
        }
        public MapComponent_Suppression Suppression
        {
            get
            {
                if (suppression == null)
                    suppression = parent.Map.GetComponent<MapComponent_Suppression>();
                return suppression;
            }
        }

        public List<IntVec3> Cells
        {
            get
            {
                if (cells.NullOrEmpty())
                {
                    cells = TRUtils.SectorCells(parent.Position, parent.Map, Props.radius, Props.angle, parent.Rotation.AsAngle);
                }
                return cells;
            }
        }

        public void UpdateCells(bool off)
        {
            cells = TRUtils.SectorCells(parent.Position, parent.Map, Props.radius, Props.angle, parent.Rotation.AsAngle);
            Suppression.RegisterOrUpdateSuppressor(parent, off ? new List<IntVec3>() : cells);
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "PowerTurnedOff")
            {
                UpdateCells(true);
            }

            if (signal == "PowerTurnedOn")
            {
                UpdateCells(false);
            }
            base.ReceiveCompSignal(signal);
        }

        public bool IsPowered => PowerComp.PowerOn;

        public override void PostDraw()
        {
            if (Find.Selector.IsSelected(parent))
            {
                Color color = Color.gray;
                if (IsPowered)
                    color = Color.cyan;
                GenDraw.DrawFieldEdges(Cells, color);
            }

            if (showGrid)
            {
                
            }
            base.PostDraw();
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }

        private static bool showGrid = false;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "ye show me",
                action = delegate { showGrid = !showGrid; }
            };
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

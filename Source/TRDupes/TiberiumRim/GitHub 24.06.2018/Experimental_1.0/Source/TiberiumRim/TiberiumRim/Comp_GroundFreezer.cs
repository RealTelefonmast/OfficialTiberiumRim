using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class Comp_GroundFreezer : ThingComp
    {
        private CompPowerTrader powerComp = null;

        private MapComponent_WaterHandler waterHandler;

        private HashSet<IntVec3> affectedPos = new HashSet<IntVec3>();

        private int ticksToRadius = 0;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.waterHandler = this.parent.Map.GetComponent<MapComponent_WaterHandler>();
            this.powerComp = this.parent.TryGetComp<CompPowerTrader>();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ticksToRadius, "ticksToRadius");
            base.PostExposeData();
        }

        public override void PostDeSpawn(Map map)
        {
            CleanLists();
            base.PostDeSpawn(map);
        }

        private void CleanLists()
        {
            this.waterHandler.SetFalse(affectedPos);
            foreach (IntVec3 c in affectedPos)
            {
                waterHandler.affectedCells.Remove(c);
            }
            affectedPos = new HashSet<IntVec3>();
        }

        private CompProperties_GroundFreezer Props
        {
            get
            {
                return (CompProperties_GroundFreezer)base.props;
            }
        }

        private float CurRadius
        {
            get
            {
                return Props.radius * (ticksToRadius / (Props.daysToRadius * GenDate.TicksPerDay));
            }
        }

        public bool Working
        {
            get
            {
                if (Props != null)
                {
                    if (Props.requiresElectricity)
                    {
                        if (powerComp != null)
                        {
                            return powerComp.PowerOn;
                        }
                    }
                    else { return true; }
                }
                return false;
            }
        }

        public bool Finished
        {
            get
            {
                return ticksToRadius >= Props.daysToRadius * GenDate.TicksPerDay;
            }
        }

        private int TicksUntilRadiusInteger
        {
            get
            {
                float num = Mathf.Ceil(this.CurRadius) - this.CurRadius;
                if (num < 1E-05f)
                {
                    num = 1f;
                }
                float num2 = this.Props.radius / this.Props.daysToRadius;
                float num3 = num / num2;
                return (int)(num3 * 60000f);
            }
        }

        public override void CompTick()
        {
            int ticks = 150;
            if (Find.TickManager.TicksGame % ticks == 0)
            {
                if (Working)
                {
                    if (!Finished)
                    {
                        ticksToRadius += ticks;
                    }
                    int num = GenRadial.NumCellsInRadius(CurRadius);
                    for (int i = 0; i < num; i++)
                    {
                        IntVec3 pos = parent.Position + GenRadial.RadialPattern[i];
                        if (pos.InBounds(parent.Map) && pos.GetRoom(parent.Map) == parent.GetRoom())
                        {
                            AffectCell(pos);
                            if (!affectedPos.Contains(pos))
                            {
                                affectedPos.Add(pos);
                                waterHandler.affectedCells.Add(pos);
                            }
                        }
                    }
                }
                else { CleanLists(); }
            }
        }

        public void AffectCell(IntVec3 c)
        {
            TerrainDef terrain = c.GetTerrain(this.parent.Map);
            if (terrain != null)
            {
                if (!terrain.Removable)
                {
                    TerrainDef Postterrain = TerrainDef.Named("Ice");
                    if (!waterHandler.TotalList.Contains(c))
                    {
                        if (terrain == TerrainDefOf.WaterShallow)
                        {
                            waterHandler.AddShallow(c);
                            this.parent.Map.terrainGrid.SetTerrain(c, Postterrain);
                        }
                        if (terrain == TerrainDefOf.WaterOceanShallow)
                        {
                            waterHandler.AddOceanShallow(c);
                            this.parent.Map.terrainGrid.SetTerrain(c, Postterrain);
                        }
                        if (terrain.defName == "Marsh")
                        {
                            waterHandler.AddMarsh(c);
                            this.parent.Map.terrainGrid.SetTerrain(c, Postterrain);
                        }                        
                    }
                    else { return; }
                    this.parent.Map.snowGrid.AddDepth(c, 0.095f);
                    Plant p = c.GetPlant(this.parent.Map);
                    if (p != null && !p.def.defName.Contains("Tiberium"))
                    {
                        p.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 10));
                    }
                }
            }
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            if (this.CurRadius < this.Props.radius)
            {
                GenDraw.DrawRadiusRing(this.parent.Position, this.CurRadius);
            }
        }


        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("CurrentRadius".Translate().CapitalizeFirst() + ": " + CurRadius.ToString("F1"));
            if (GenDate.TicksToDays(this.ticksToRadius) < Props.daysToRadius && Working)
            {
                sb.AppendLine("RadiusExpandsIn".Translate().CapitalizeFirst() + ": " + this.TicksUntilRadiusInteger.ToStringTicksToPeriod(true));
            }
            return sb.ToString().TrimEndNewlines();
        }
    }

    public class CompProperties_GroundFreezer : CompProperties
    {
        public float daysToRadius;

        public float radius;

        public bool requiresElectricity;

        public CompProperties_GroundFreezer()
        {
            this.compClass = typeof(Comp_GroundFreezer);
        }
    }
}

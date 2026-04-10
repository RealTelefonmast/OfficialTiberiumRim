using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ThingComp_TiberiumRadiation : ThingComp
    {
        public List<IntVec3> cellsPlants = new List<IntVec3>();
        public List<IntVec3> cellsPawns = new List<IntVec3>();

        private bool showRadius = false;
        private int curTick = 0;

        public CompProperties_TiberiumRadiation Props => this.props as CompProperties_TiberiumRadiation;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            cellsPlants.AddRange(GenRadial.RadialCellsAround(parent.Position, Props.radius, true));
            cellsPawns.AddRange(GenRadial.RadialCellsAround(parent.Position, Props.radius * 0.5f, true));
        }

        public override void CompTick()
        {
            base.CompTick();
            if (curTick <= 0)
            {
                Irradiate();
                curTick = TRUtils.Range(Props.interval);
            }
            curTick--;
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (curTick <= 0)
            {
                Irradiate();
                curTick = TRUtils.Range(Props.interval);
            }
            curTick -= GenTicks.TickRareInterval;
        }

        private float IntensityAt(IntVec3 cell)
        {
            return parent.Position.DistanceTo(cell) / Props.radius;
        }

        private void Irradiate()
        {
            foreach (IntVec3 cell in cellsPlants)
            {
                if (!cell.InBounds(parent.Map))
                {
                    return;
                }
                Plant plant = cell.GetPlant(parent.Map);
                TiberiumCrystal crystal = cell.GetTiberium(parent.Map);
                if (plant != null)
                {
                    if (!plant.IsTiberiumPlant())
                    {
                        if (TRUtils.Chance(Props.intensity))
                        {
                            float damage = Mathf.Lerp(0f, TRUtils.Range(Props.damage), Mathf.Lerp(0, Props.intensity, IntensityAt(cell)));
                            DamageInfo dinfo = new DamageInfo(DamageDefOf.Burn, damage, 1f, -1, parent);
                            plant.TakeDamage(dinfo);
                        }
                    }
                    else
                    {
                        if (plant.HitPoints < plant.MaxHitPoints)
                        {
                            if (TRUtils.Chance(IntensityAt(cell)))
                            {
                                plant.HitPoints += 1;
                            }
                        }
                    }
                }
                if (crystal != null)
                {
                    if (!crystal.def.tiberium.dependsOnProducer && crystal.HitPoints < crystal.MaxHitPoints)
                    {
                        if (TRUtils.Chance(IntensityAt(cell)))
                        {
                            crystal.HitPoints += 1;
                        }
                    }
                }
            }
            foreach (IntVec3 cell in cellsPawns)
            {
                Pawn pawn = cell.GetFirstPawn(parent.Map);
                if (pawn != null)
                {
                    //TODO: Finish radiaion
                    HediffUtils.TryIrradiatePawn(pawn, null, 1);
                }
            }
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            if (showRadius)
            {
                GenDraw.DrawFieldEdges(cellsPawns, Color.green);
                GenDraw.DrawFieldEdges(cellsPlants, Color.cyan);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode && DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Show radius",
                    action = delegate
                    {
                        showRadius = !showRadius;
                    }
                };
            }
        }
    }

    public class CompProperties_TiberiumRadiation : CompProperties
    {
        public bool rareTick = false;
        public float radius = 1f;
        public float intensity = 1f;
        public IntRange damage = new IntRange(0, 1);
        public IntRange interval = new IntRange(1, 15);

        public CompProperties_TiberiumRadiation()
        {
            this.compClass = typeof(ThingComp_TiberiumRadiation);
        }
    }
}

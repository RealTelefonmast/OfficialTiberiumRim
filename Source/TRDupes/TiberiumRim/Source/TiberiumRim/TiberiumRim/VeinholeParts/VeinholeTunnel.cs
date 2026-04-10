using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;

namespace TiberiumRim
{
    public class VeinholeTunnel : ThingWithComps
    {
        private Sustainer sustainer;
        private int ticksToSpawn;
        private List<IntVec3> occupied;

        private void CreateSustainer()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                SoundDef tunnel = SoundDefOf.Tunnel;
                this.sustainer = tunnel.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            });
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            CreateSustainer();
            var rect = this.OccupiedRect();
            occupied = rect.Cells.Where(c => !rect.IsCorner(c)).InRandomOrder().ToList();
            //occupied.ForEach(c => FilthMaker.MakeFilth(c, Map, ThingDefOf.Filth_Dirt));
            if (!respawningAfterLoad)
            {
                ticksToSpawn = Find.TickManager.TicksGame + GenTicks.SecondsToTicks(30);
            }
        }

        public override void Tick()
        {
            sustainer.Maintain();
            if (TRUtils.Chance(0.32f))
                foreach (var cell in occupied)
                {
                    if (TRUtils.Chance(0.39f))
                        if (TRUtils.Chance(0.25f))
                            FilthMaker.TryMakeFilth(cell, Map, ThingDefOf.Filth_RubbleRock);
                        else
                            FilthMaker.TryMakeFilth(cell, Map, ThingDefOf.Filth_Dirt);

                    MoteMaker.ThrowDustPuffThick(cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), Map,
                        TRUtils.Range(1.5f, 3.5f), new Color(1f, 1f, 1f, 0.55f));
                }
            if (ticksToSpawn <= Find.TickManager.TicksGame)
            {
                GenSpawn.Spawn(TiberiumDefOf.Veinhole, this.Position, Map);
                this.DeSpawn();
            }
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}

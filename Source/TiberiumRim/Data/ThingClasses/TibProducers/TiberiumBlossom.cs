﻿using System.Collections.Generic;
using Verse;

namespace TR
{
    public class TiberiumBlossom : TiberiumProducer
    {

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            //TiberiumComp.BlossomInfo.RegisterBlossom(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            //TiberiumComp.BlossomInfo.DeregisterBlossom(this);
            base.DeSpawn(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (!DebugSettings.godMode) yield break;

            yield return new Command_Action()
            {
                defaultLabel = "Mutate Area",
                action = delegate
                {
                    foreach (var pos in GenRadial.RadialCellsAround(Position, 14.9f, false))
                    {
                        if(!GenTiberium.TryMutatePlant(pos.GetPlant(Map), TiberiumDefOf.TiberiumGreen))
                            Map.terrainGrid.SetTerrain(pos, TiberiumDefOf.TiberiumGreen.conversions.baseTerrain);
                    }
                }
            };
        }

    }
}

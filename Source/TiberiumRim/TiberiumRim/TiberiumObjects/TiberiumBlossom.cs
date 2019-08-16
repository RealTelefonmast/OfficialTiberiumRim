using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumBlossom : TiberiumProducer
    {
        public float radius = 30f;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Log.Message(TiberiumComp.StructureInfo.Blossoms + " blossoms. ");
            if (TiberiumComp.StructureInfo.Blossoms.Any() && TiberiumComp.StructureInfo.Blossoms.Any(b => b != this && b.Position.DistanceTo(Position) <= 3))
                this.DeSpawn();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            TiberiumComp.StructureInfo.Notify_BlossomGone();
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
    }
}

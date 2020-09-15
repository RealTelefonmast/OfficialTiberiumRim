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

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            TiberiumComp.BlossomInfo.RegisterBlossom(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            TiberiumComp.BlossomInfo.DeregisterBlossom(this);
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

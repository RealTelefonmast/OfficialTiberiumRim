using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class WorldComponent_Tiberium : WorldComponent
    {
        public GroundZero GroundZero;

        public WorldComponent_Tiberium(World world) : base(world)
        {

        }

        public void SetupGroundZero(TiberiumProducer producer, Map map, ref bool gz)
        {
            if (producer != null && (!producer.def.spore?.canBeGroundZero ?? true))
                return;
            if (GroundZero != null)
                return;
            GroundZero = new GroundZero();
            if (producer != null)
            {
                GroundZero.producer = producer;
                gz = true;
            }
            GroundZero.map = map;
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }
    }
}

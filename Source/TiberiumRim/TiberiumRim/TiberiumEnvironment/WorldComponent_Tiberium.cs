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
        //public GroundZero GroundZero;
        public GlobalTargetInfo GroundZero = GlobalTargetInfo.Invalid;

        public WorldComponent_Tiberium(World world) : base(world)
        {

        }

        public void SetGroundZero(TiberiumProducer producer)
        {
            Log.Message("Setting GZ");
            Log.Message("GZ Already Valid: " + GroundZero.IsValid + " Producer Invalid: " + !producer.def.canBeGroundZero);
            if (GroundZero.IsValid || !producer.def.canBeGroundZero) return;
            GroundZero = new GlobalTargetInfo(producer);
            producer.IsGroundZero = true;
            Log.Message("Made new GZ and set it to " + producer);
        }

        public void SetGroundZero(Map map)
        {
            if (GroundZero.IsValid) return;
            //TODO: Setup GZ WorldObject
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }
    }
}

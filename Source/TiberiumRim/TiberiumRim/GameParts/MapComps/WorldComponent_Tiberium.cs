﻿using System;
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_TargetInfo.Look(ref GroundZero, "groundZero");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if(GroundZero.IsValid)
                    ((TiberiumProducer) GroundZero.Thing).IsGroundZero = true;
            }
        }

        public void SetGroundZero(TiberiumProducer producer)
        {
            if (GroundZero.IsValid || !producer.def.canBeGroundZero) return;
            GroundZero = new GlobalTargetInfo(producer);
            producer.IsGroundZero = true;
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
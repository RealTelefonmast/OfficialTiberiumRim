using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TiberiumPawn : FXPawn, IPawnWithParent
    {
        public new TiberiumKindDef kindDef;
        public TiberiumProducer boundProducer;

        public bool ProducerAvailable => !boundProducer.DestroyedOrNull();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void PostMake()
        {
            kindDef = (TiberiumKindDef)base.kindDef;
            base.PostMake();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref boundProducer, "mainProducer");
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
        {
            var stuff = base.ButcherProducts(butcher, efficiency);

            return stuff;
        }

        public List<IntVec3> Field { get; }
        public ThingWithComps Parent { get; }
        public bool CanWander { get; }
    }
}

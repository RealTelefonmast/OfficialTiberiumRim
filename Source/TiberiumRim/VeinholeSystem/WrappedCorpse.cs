using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TR
{
    public class WrappedCorpse : Corpse
    {
        private Veinhole destination;
        
        private GenericPathFollower pather;
        
        /*
        private Vector3 tweenedPos;
        private Vector3 startVec;
        private Vector3 endVec;
        private IntVec3 startPos;
        private IntVec3 endPos;
        
        private float startTime;
        private int startTick;
        private int tick;
        */

        private const int travelTime = 1200;

        private Effecter _effecter;
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            _effecter = DefDatabase<EffecterDef>.GetNamed("VeinholePusher").SpawnAttached(this, Map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref pather, "pather");
        }

        //Tick wrapped corpse to move towards veinhole
        public override void Tick()
        {
            base.Tick();

            //TODO:Keep as veincorpse...
            /*
            if (Position.GetTiberium(Map) is not TiberiumVein)
            {
                innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Direct);
                DeSpawn();
            }
            */

            if (Find.TickManager.TicksGame % 250 == 0)
            {
                InnerPawn.TickRare();
            }
            
            if (destination.Spawned)
            {
	            if (!pather.Moving)
	            {
					pather.StartPath(destination, PathEndMode.Touch, TraverseMode.NoPassClosedDoorsOrWater);   
	            }
                else
                {
                    if (this.IsHashIntervalTick(4))
                    {
                        var thisPos = new TargetInfo(this);
                        _effecter.Trigger(thisPos, thisPos);
                    }
                    pather.PatherTick();
                }
            }
        }

        private Vector3 TweenedRoot
        {
            get
            {
                if (pather.Moving)
                {
                    float num = pather.MovedPercent;
                    return pather.nextCell.ToVector3Shifted() * num + Position.ToVector3Shifted() * (1f - num);
                }
                return DrawPos;
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var next = TweenedRoot;
            InnerPawn.Drawer.renderer.RenderPawnAt(new Vector3(next.x, AltitudeLayer.Pawn.AltitudeFor(), next.z));
            pather.DrawPath();
        }

        public static WrappedCorpse MakeFrom(Corpse corpse, TiberiumVein creator)
        {
            var def = corpse.GetWrappedCorpseDef();
            var pos = corpse.Position;
            var map = corpse.Map;
            WrappedCorpse wrapped = (WrappedCorpse)ThingMaker.MakeThing(def, null);
            if (corpse.innerContainer.TryTransferToContainer(corpse.InnerPawn, wrapped.innerContainer))
            {
                wrapped.destination = (Veinhole)creator.Parent;
                if (!corpse.Destroyed)
                    corpse.Destroy();
                if (corpse.Spawned)
                    corpse.DeSpawn();
                if (!corpse.Discarded)
                    corpse.Discard();

                wrapped = (WrappedCorpse) GenSpawn.Spawn(wrapped, pos, map);
                wrapped.pather = new GenericPathFollower(wrapped);
                return wrapped;
            }

            return null;
        }
    }
}

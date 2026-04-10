using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class VeinholeFood : ThingWithComps, IThingHolder, IThoughtGiver, IStrippable, IBillGiver
    {
        private Corpse corpse;
        private ThingOwner innerContainer;
        private Veinhole destination;

        private Vector3 tweenedPos;
        private Vector3 startVec;
        private Vector3 endVec;
        private IntVec3 startPos;
        private IntVec3 endPos;

        private float startTime;
        private int startTick;
        private int tick;

        private const int travelTime = 1200;

        public VeinholeFood()
        {
            this.innerContainer = new ThingOwner<Corpse>(this, true, LookMode.Reference);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public void AddCorpse(Corpse corpse, Veinhole dest)
        {
            this.corpse = corpse;
            this.destination = dest;
            startPos = Position;
            endPos = Position;
            tweenedPos = Position.ToVector3Shifted();
            startVec = Position.ToVector3Shifted();
            endVec = endPos.ToVector3Shifted();
            this.innerContainer.TryAdd(corpse.SplitOff(1), true);
        }

        public override void Tick()
        {
            base.Tick();
            if (!(Position.GetTiberium(Map) is TiberiumVein))
            {
                innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Direct);
                this.DeSpawn();
            }
            if(CanMove && destination != null)
                TryMoveTowards(destination.Position);
            if(Find.TickManager.TicksGame % 250 == 0)
                corpse.InnerPawn.TickRare();
            UpdateDrawUpdate();
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public Thought_Memory GiveObservedThought()
        {
            return corpse.GiveObservedThought();
        }

        public bool AnythingToStrip()
        {
            return corpse.AnythingToStrip();
        }

        public void Strip()
        {
            corpse.Strip();
        }

        public bool CurrentlyUsableForBills()
        {
            return corpse.CurrentlyUsableForBills();
        }

        public bool UsableForBillsAfterFueling()
        {
            return corpse.UsableForBillsAfterFueling();
        }

        public BillStack BillStack => corpse.BillStack;
        public IEnumerable<IntVec3> IngredientStackCells => corpse.IngredientStackCells;

        public void TryMoveTowards(IntVec3 pos)
        {
            if (!CanMove) return;
            Log.Message("Can move and setting to go for " + pos);
            startPos = Position;
            endPos = Position.CellsAdjacent8Way().Where(x => x.GetTiberium(Map) is TiberiumVein).MinBy(x => x.DistanceTo(pos));
            tweenedPos = Position.ToVector3Shifted();
            startVec = Position.ToVector3Shifted();
            endVec = endPos.ToVector3Shifted();
            startTime = RealTime.deltaTime;
            startTick = Find.TickManager.TicksGame;
            tick = 0;
        }

        public bool CanMove => tweenedPos == endPos.ToVector3Shifted();

        
        public override Vector3 DrawPos
        {
            get { return tweenedPos; }
        }

        private void UpdateDrawUpdate()
        {
            if (tick >= 600)
                Position = endPos;
            float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
            if (tickRateMultiplier < 5f)
            {
                Vector3 a = this.TweenedRoot() - this.tweenedPos;
                float num = 0.09f * (RealTime.deltaTime * 60f * tickRateMultiplier);
                if (RealTime.deltaTime > 0.05f)
                {
                    num = Mathf.Min(num, 1f);
                }
                this.tweenedPos += a * num;
            }
            else
            {
                this.tweenedPos = this.TweenedRoot();
            }
        }

        private Vector3 TweenedRoot()
        {
            float num = this.MovedPct();
            return endVec * num + startVec * (1 - num) + PawnCollisionTweenerUtility.PawnCollisionPosOffsetFor(corpse.InnerPawn);
        }

        private float MovedPct()
        {
            if(tick < travelTime)
                tick++;
            return Mathf.InverseLerp(0, travelTime, tick++);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            //base.DrawAt(drawLoc, flip);
            Log.Message("Actual pos: " + tweenedPos);
            corpse.InnerPawn.Drawer.renderer.RenderPawnAt(DrawPos + new Vector3(0, this.def.altitudeLayer.AltitudeFor(), 0));
        }

    }
}

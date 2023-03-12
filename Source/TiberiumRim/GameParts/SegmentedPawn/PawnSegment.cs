using Verse;

namespace TiberiumRim
{
    public class PawnSegmentAttacher
    {
        private SegmentedPawn headPawn;
        private PawnSegment segment;

        private PawnSegmentAttacher head;
        private PawnSegmentAttacher tail;

        public PawnSegment Owner => segment;
        public PawnSegmentAttacher Head => head;
        public PawnSegmentAttacher Tail => tail;

        public int GetIndex()
        {
            int index = 0;
            var current = Head;
            while (current != null)
            {
                index++;
                current = current.Head;
            }
            return index;
        }

        public bool IsHead => headPawn != null;
        public bool IsBehindHead => head == null;
        public bool IsTail => tail == null;

        public PawnSegmentAttacher(SegmentedPawn head)
        {
            headPawn = head;
        }

        public PawnSegmentAttacher(PawnSegment segment)
        {
            this.segment = segment;

        }

        public void AttachHead(PawnSegment newSegment)
        {
            head = newSegment.AttachedSegments;
        }

        public void AttachTail(PawnSegment newSegment)
        {
            if (tail == null)
            {
                if (!IsHead)
                {
                    newSegment.AttachedSegments.AttachHead(Owner);
                }
                tail = newSegment.AttachedSegments;
                Log.Message($"Attached new segment {newSegment} to {segment} at index {newSegment.AttachedSegments.GetIndex()}");
                return;
            }
            tail.AttachTail(newSegment);
        }
    }

    public class PawnSegment : ThingWithComps//, IVerbOwner, IAttackTarget, IAttackTargetSearcher, ILoadReferenceable
    {
        private SegmentedPawn parent;
        private PawnSegmentAttacher attachedSegments;

        private PawnSegmentTweener tweener;
        private PawnSegment_PawnFollower follower;

        public PawnSegmentAttacher AttachedSegments => attachedSegments;

        public override void PostMake()
        {
            base.PostMake();
            attachedSegments = new PawnSegmentAttacher(this);
            tweener = new PawnSegmentTweener();
            follower = new PawnSegment_PawnFollower();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

        }

        public override void Tick()
        {
            base.Tick();
            if (!parent.pather.MovingNow) return;
            int consumed = parent.pather.curPath.NodesConsumedCount;
            if (consumed > 0)
            {
                int index = AttachedSegments.GetIndex();
                if (index > consumed) return;
                int nodesCount = parent.pather.curPath.NodesReversed.Count;
                //Log.Message($"Index {index}");
                //Log.Message($"Path size {nodesCount}");
                if (index < nodesCount)
                    Position = parent.pather.curPath.NodesReversed[(nodesCount-1)-index];
            }
        }

        public void AttachTo(SegmentedPawn parent)
        {
            this.parent = parent;
            parent.AttachedSegments.AttachTail(this);

        }


        public override void Draw()
        {
            base.Draw();
        }
    }
}

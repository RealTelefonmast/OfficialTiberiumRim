using Verse;

namespace TR.GameParts.SegmentedPawn;

public class PawnSegmentAttacher
{
    private readonly SegmentedPawn headPawn;

    public PawnSegmentAttacher(SegmentedPawn head)
    {
        headPawn = head;
    }

    public PawnSegmentAttacher(PawnSegment segment)
    {
        this.Owner = segment;
    }

    public PawnSegment Owner { get; }

    public PawnSegmentAttacher Head { get; private set; }

    public PawnSegmentAttacher Tail { get; private set; }

    public bool IsHead => headPawn != null;
    public bool IsBehindHead => Head == null;
    public bool IsTail => Tail == null;

    public int GetIndex()
    {
        var index = 0;
        var current = Head;
        while (current != null)
        {
            index++;
            current = current.Head;
        }

        return index;
    }

    public void AttachHead(PawnSegment newSegment)
    {
        Head = newSegment.AttachedSegments;
    }

    public void AttachTail(PawnSegment newSegment)
    {
        if (Tail == null)
        {
            if (!IsHead) newSegment.AttachedSegments.AttachHead(Owner);
            Tail = newSegment.AttachedSegments;
            Log.Message(
                $"Attached new segment {newSegment} to {Owner} at index {newSegment.AttachedSegments.GetIndex()}");
            return;
        }

        Tail.AttachTail(newSegment);
    }
}

public class PawnSegment : ThingWithComps //, IVerbOwner, IAttackTarget, IAttackTargetSearcher, ILoadReferenceable
{
    private PawnSegment_PawnFollower follower;
    private SegmentedPawn parent;

    private PawnSegmentTweener tweener;

    public PawnSegmentAttacher AttachedSegments { get; private set; }

    public override void PostMake()
    {
        base.PostMake();
        AttachedSegments = new PawnSegmentAttacher(this);
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
        var consumed = parent.pather.curPath.NodesConsumedCount;
        if (consumed > 0)
        {
            var index = AttachedSegments.GetIndex();
            if (index > consumed) return;
            var nodesCount = parent.pather.curPath.NodesReversed.Count;
            //Log.Message($"Index {index}");
            //Log.Message($"Path size {nodesCount}");
            if (index < nodesCount)
                Position = parent.pather.curPath.NodesReversed[nodesCount - 1 - index];
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
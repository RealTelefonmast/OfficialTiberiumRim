using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class SegmentedPawn : Pawn
    {
        private List<PawnSegment> ownedSegments;
        private PawnSegmentAttacher attachedSegments;

        public PawnSegmentAttacher AttachedSegments => attachedSegments;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            ownedSegments = new List<PawnSegment>();
            attachedSegments = new PawnSegmentAttacher(this);
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            base.Kill(dinfo, exactCulprit);
            foreach (var segment in ownedSegments)
            {
                segment.Kill();
            }
        }

        private void SpawnSegment(PawnSegment newSegment)
        {
            var position = Position;
            if (AttachedSegments.Tail != null)
            {
                position = AttachedSegments.Tail.Owner.Position;
            }
            position -= IntVec3.North;
            GenSpawn.Spawn(newSegment, position, Map);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Add Segment",
                action = delegate
                {
                    PawnSegment segment = (PawnSegment)ThingMaker.MakeThing(ThingDef.Named("BasicPawnSegment"));
                    SpawnSegment(segment);
                    segment.AttachTo(this);
                }
            };
        }
    }
}

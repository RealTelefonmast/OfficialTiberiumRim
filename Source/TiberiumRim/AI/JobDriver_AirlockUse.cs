using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobDriver_AirlockUse : JobDriver
    {
        private Room AirLockRoom => RoomCenter.GetRoom(Map);
        private RoomComponent_AirLock AirLock => airlockInt ??= AirLockRoom.GetRoomComp<RoomComponent_AirLock>();

        //Target A: Entrance AirLock Door
        private Building_AirLock Target => TargetA.Thing as Building_AirLock;

        //Target B: AirLock Standing Cell
        private IntVec3 RoomCenter => TargetB.Cell;

        //Extra Data
        private RoomComponent_AirLock airlockInt;
        private HashSet<IntVec3> roomCellsTemp;
        private IntVec3 QueueCell;

        private bool ShouldQueue => AirLock.CurrentPawn != pawn;

        public override string GetReport()
        {
            return $"{base.GetReport()}[{AirLockRoom.ID}][{(ShouldQueue ? "Waiting" : "Direct")}]";
        }

        private bool ShouldStop
        {
            get
            {
                switch (AirLock.CurrentUsage)
                {
                    case AirLockUsage.WaitForDoors:
                        return AirLock.AllDoorsClosed;
                    case AirLockUsage.WaitForClean:
                        return AirLock.IsClean;
                }
                return true;
            }
        }

        private IntVec3 NextBestQueuePos()
        {
            GenPlace.TryFindPlaceSpotNear(TargetA.Cell, Rot4.South, Map, pawn, false, out IntVec3 result,x => x != TargetA.Cell && x.InBounds(Map) && !roomCellsTemp.Contains(x) && !AirLock.ReservedQueue.Contains(x));
            return result;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //If it is not our turn, setup queue data
            if (ShouldQueue)
            {
                roomCellsTemp = AirLockRoom.Cells.ToHashSet();
                QueueCell = NextBestQueuePos();
                AirLock.Notify_EnqueuePawnPos(QueueCell);
            }
            //SetupData
            AddFinishAction(() =>
            {
                Log.Message($"[{pawn}]Finishing toil...");
                AirLock.Notify_FinishJob(pawn);
            });
            return true;
        }

        public IntVec3 GotoTarget => ShouldQueue ? QueueCell : RoomCenter;

        public override IEnumerable<Toil> MakeNewToils()
        {
            Log.Message($"Going To {GotoTarget}");
            yield return Toils_Goto.GotoCell(GotoTarget, PathEndMode.OnCell);

            Log.Message($"Should Queue: {ShouldQueue}");
            //
            if (ShouldQueue)
            {
                Toil waitForQueue = new Toil();
                waitForQueue.initAction = delegate
                {

                };
                waitForQueue.defaultCompleteMode = ToilCompleteMode.Never;
                waitForQueue.FailOn(() => AirLock.IsReadyForUsage && AirLock.CurrentPawn == pawn);
                waitForQueue.AddFinishAction(() => AirLock.Notify_DequeuePawnPos(QueueCell));
                yield return waitForQueue;
            }

            Toil toil = new Toil();
            toil.initAction = delegate
            {
                toil.actor.pather.StopDead();
            };
            toil.tickAction = delegate
            {
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.FailOn(() => ShouldStop);
            yield return toil;
        }
    }
}

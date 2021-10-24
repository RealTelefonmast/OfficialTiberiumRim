using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobDriver_AirlockQueue : JobDriver
    {
        private RoomComponent_AirLock airlockInt;

        private Building_AirLock Target => TargetA.Thing as Building_AirLock;

        private Room AirLockRoom => TargetB.Cell.GetRoom(Map);
        private RoomComponent_AirLock AirLock => airlockInt ??= AirLockRoom.GetRoomComp<RoomComponent_AirLock>();

        private HashSet<IntVec3> roomCellsTemp;

        private LocalTargetInfo QueueCell;

        private IntVec3 NextBestQueuePos()
        {
            CellFinder.TryFindRandomCellNear(TargetB.Cell, Map, 5, x =>x.InBounds(Map) && !roomCellsTemp.Contains(x) && !AirLock.ReservedQueue.Contains(x), out IntVec3 result);
            return result;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            roomCellsTemp = AirLockRoom.Cells.ToHashSet();
            QueueCell = NextBestQueuePos();
            AirLock.Notify_EnqueuePawnPos(QueueCell.Cell);
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(QueueCell.Cell, PathEndMode.OnCell);
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                toil.actor.pather.StopDead();
            };
            toil.tickAction = delegate
            {
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.FailOn(() => AirLock.IsReadyForUsage && AirLock.CurrentPawn == pawn);
            yield return toil;
        }
    }
}

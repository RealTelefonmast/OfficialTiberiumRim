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

        private bool ShouldQueue => AirLock.ShouldWaitFor(pawn); //!(AirLock.IsReadyForUsage && AirLock.NextPawnInQueue == pawn);//AirLock.NextPawnInQueue != null && AirLock.NextPawnInQueue != pawn || !AirLock.IsReadyForUsage;

        public override string GetReport()
        {
            return $"{base.GetReport()}[{AirLockRoom.ID}][{(ShouldQueue ? "Queued" : "Direct")}]";
        }

        private IntVec3 NextBestQueuePos()
        {
            GenPlace.TryFindPlaceSpotNear(TargetA.Cell, Rot4.South, Map, pawn, false, out IntVec3 result,x => x != TargetA.Cell && x.InBounds(Map) && !roomCellsTemp.Contains(x) && !AirLock.ReservedQueue.Contains(x));
            return result;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (AirLock.AlreadyWaitingFor(pawn, out IntVec3 pos))
            {
                QueueCell = pos;
            }
            else if(ShouldQueue) //AirLock.NextPawnInQueue != null && AirLock.NextPawnInQueue != pawn
            {
                roomCellsTemp = AirLockRoom.Cells.ToHashSet();
                //If already inside airlock, wait inside airlock
                if (pawn.GetRoom() == AirLockRoom)
                {
                    QueueCell = RoomCenter;
                }
                else //If it is not our turn, setup queue data
                {
                    QueueCell = NextBestQueuePos();
                }
                AirLock.Notify_EnqueuePawnPos(pawn, QueueCell);
            }

            //SetupData
            AddFinishAction(() =>
            {
                var jobCondition = TRAIPatches.JobDriver_CleanupPatch._LastJobCondition;
                AirLock.Notify_FinishJob(pawn, jobCondition);
            });
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            //
            if (ShouldQueue)
            {
                yield return Toils_Goto.GotoCell(QueueCell, PathEndMode.OnCell);

                Toil waitForQueue = new Toil();
                waitForQueue.initAction = delegate
                {
                    //TLog.Debug($"[{pawn.NameShortColored}][{AirLock.Room.ID}]Starting to wait in Queue on {QueueCell}");
                };
                waitForQueue.tickAction = delegate
                {
                    if(!ShouldQueue) 
                        ReadyForNextToil();
                };
                waitForQueue.defaultCompleteMode = ToilCompleteMode.Never;
                waitForQueue.AddFinishAction(() => AirLock.Notify_DequeuePawnPos(pawn));
                yield return waitForQueue;
            }

            yield return Toils_Goto.GotoCell(RoomCenter, PathEndMode.OnCell);

            Toil toil = new Toil();
            toil.initAction = delegate
            {
                toil.actor.pather.StopDead();
            };
            toil.tickAction = delegate
            {
                if(AirLock.SafeToLeave)
                    EndJobWith(JobCondition.Succeeded);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }
    }
}

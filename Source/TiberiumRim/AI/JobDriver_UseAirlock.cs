using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobDriver_UseAirlock : JobDriver
    {
        private Room AirLockRoom => TargetA.Cell.GetRoom(Map);
        private RoomComponent_AirLock AirLock => AirLockRoom.GetRoomComp<RoomComponent_AirLock>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                toil.actor.pather.StopDead();
            };
            toil.tickAction = delegate
            {
                if (!AirLock.AllDoorsClosed) return;
                if (AirLock.IsClean)
                {
                    EndJobWith(JobCondition.Succeeded);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            yield return toil;
        }
    }
}

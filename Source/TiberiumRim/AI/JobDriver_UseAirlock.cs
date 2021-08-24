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


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            yield return Toils_General.Wait(600);
            yield break;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TiberiumRim.Utilities;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public static class TRAIPatches
    {
        [HarmonyPatch(typeof(Pawn_PathFollower)), HarmonyPatch("TrySetNewPath")]
        public static class TrySetNewPathPatch
        {
            //Patch the new path setter method to hook into new logic depending on the path
            public static void Postfix(Pawn_PathFollower __instance, ref bool __result, Pawn ___pawn)
            {
                //Bad path... return
                if (!__result) return;

                //If already using an airlock..
                if (___pawn.CurJobDef == TiberiumDefOf.UseAirlock) return;

                //
                var currentRoom = ___pawn.GetRoom();
                if (currentRoom.Role == TiberiumDefOf.TR_AirLock)
                {
                    //If already in airlock which is cleaned, return
                    var curAirlock = currentRoom.GetRoomComp<RoomComponent_AirLock>();
                    if (curAirlock.IsClean || !curAirlock.IsActive) return;
                }

                //Check path for airlocks
                var rooms = __instance.curPath.RoomsAlongPath(___pawn.Map);
                Room airLock = rooms?.Find(r => r.Role == TiberiumDefOf.TR_AirLock);

                //No airlocks, no job
                if (airLock == null) return;

                //If airlock is inactive,
                var airLockComp = airLock.GetRoomComp<RoomComponent_AirLock>();
                if (!airLockComp.IsActive) return;

                //Start the airlock job and set the current job to be resumed
                Job airlockJob = JobMaker.MakeJob(TiberiumDefOf.UseAirlock, airLock.GeneralCenter());
                ___pawn.jobs.StartJob(airlockJob, JobCondition.Ongoing, null, true);

                //Discard original path result
                __result = false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using TiberiumRim.Utilities;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    internal static class TRAIPatches
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
                if (___pawn.CurJobDef == TiberiumDefOf.UseAirlock /*|| ___pawn.CurJobDef == TiberiumDefOf.UseAirlock_Clean*/) return;

                //
                /*
                var currentRoom = ___pawn.GetRoom();
                if (currentRoom.Role == TiberiumDefOf.TR_AirLock)
                {
                    //If already in airlock which is cleaned, return
                    var curAirlock = currentRoom.GetRoomComp<RoomComponent_AirLock>();
                    if (!curAirlock.ShouldBeUsed(true, out _)) return;
                }
                */

                //Check path for airlocks
                var currentRoom = ___pawn.GetRoom();
                var lastPathNodes = __instance.curPath.NodesReversed.ListFullCopy();
                var rooms = __instance.curPath.RoomsAlongPath(___pawn.Map);
                var airLocks = rooms?.Where(r => r.Role == TiberiumDefOf.TR_AirLock);

                //No airlocks, no job
                if (airLocks.EnumerableNullOrEmpty()) return;

                bool desiresAirlock = false;
                foreach (var airLock in airLocks)
                {
                    //If airlock is inactive or cannot be used for any reason, skip it
                    var airLockComp = airLock.GetRoomComp<RoomComponent_AirLock>();
                    if (!airLockComp.ShouldBeUsed(currentRoom == airLockComp.Room)) continue;

                    var airlocks = airLockComp.AirLocksOnPath(lastPathNodes);

                    //Add pawn to the FCFS pawn queue
                    airLockComp.Notify_EnqueuePawn(___pawn);

                    //Start the airlock job and set the current job to be resumed
                    Log.Message($"[{___pawn}]Adding airlock job at: {airLockComp.Room.ID} via {airlocks[1]}");
                    Job theJob = JobMaker.MakeJob(TiberiumDefOf.UseAirlock, airlocks[1], airLock.GeneralCenter());
                    ___pawn.jobs.StartJob(theJob, JobCondition.Ongoing, null, true);

                    /*
                    //If current pawn in the queue is not pathing pawn, wait in queue for other to finish
                    if (airLockComp.CurrentPawn != ___pawn)
                    {
                        var airlocks = airLockComp.AirLocksOnPath(lastPathNodes);
                        
                        Log.Message($"[{___pawn}]Adding queue job at: {airlocks[0]}");
                        Job queueJob = JobMaker.MakeJob(TiberiumDefOf.QueueAtAirlock, airlocks[0], airLock.GeneralCenter());
                        ___pawn.jobs.StartJob(queueJob, JobCondition.Ongoing, null, true);
                    }
                    */
                    desiresAirlock = true;
                }

                if (desiresAirlock)
                {
                    //Discard original path result
                    __result = false;
                    return;
                }

            }
        }

        [HarmonyPatch(typeof(Pawn_PathFollower)), HarmonyPatch(nameof(Pawn_PathFollower.TryEnterNextPathCell))]
        public static class TryEnterNextPathCellPatch
        {
            private static MethodInfo helper = AccessTools.Method(typeof(Pawn_FilthTracker), nameof(Pawn_FilthTracker.Notify_EnteredNewCell));
            private static MethodInfo newNotify = AccessTools.Method(typeof(TryEnterNextPathCellPatch), nameof(Notify_PawnEnteredNewCell));
            private static FieldInfo pawnField = AccessTools.Field(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.pawn));

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var code in instructions)
                {
                    yield return code;
                    if (code.opcode == OpCodes.Callvirt && code.Calls(helper))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                        yield return new CodeInstruction(OpCodes.Call, newNotify);
                    }
                }
            }

            public static void Notify_PawnEnteredNewCell(Pawn pawn)
            {
                var curRoom = pawn.GetRoom();
                var lastRoom = pawn.pather.lastCell.GetRoom(pawn.Map);
                if (curRoom != lastRoom)
                {
                    curRoom.RoomTracker().Notify_PawnEnteredRoom(pawn);
                    lastRoom.RoomTracker().Notify_PawnLeftRoom(pawn);
                }
            }

        }
    }
}

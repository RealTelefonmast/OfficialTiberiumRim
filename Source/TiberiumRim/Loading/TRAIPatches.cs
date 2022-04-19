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
        //Pathing
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

                //Check path for airlocks
                var entryRoom = ___pawn.GetRoom();
                var lastPathNodes = __instance.curPath.NodesReversed.ListFullCopy();
                var airLockRooms = __instance.curPath.RoomsAlongPath(___pawn.Map, TiberiumDefOf.TR_AirLock);

                //TLog.Debug($"[{___pawn.NameShortColored}]RoomsAlongPath: {airLockRooms.Select(t => t.ID).ToStringSafeEnumerable()}");

                //No airlocks, no job
                if (airLockRooms.NullOrEmpty()) return;

                bool desiresAirlock = false;
                foreach (var room in airLockRooms)
                {
                    //If airlock is inactive or cannot be used for any reason, skip it
                    var airLockComp = room.GetRoomComp<RoomComponent_AirLock>();
                    //Gets an array of size 2 of the 2 airlock doors tha pawn passes through in reverse order
                    var airLockDoors = airLockComp.AirLocksOnPath(lastPathNodes, ___pawn);
                    if (!airLockComp.ShouldBeUsed(___pawn , airLockDoors, entryRoom == room)) continue;


                    //Add pawn to the FCFS pawn queue
                    airLockComp.Notify_EnqueuePawn(___pawn);

                    //Start the airlock job and set the current job to be resumed
                    //TLog.Debug($"[{___pawn.NameShortColored}][{airLockComp.Room.ID}]Adding airlock job via {airLockDoors[1]}");
                    Job theJob = JobMaker.MakeJob(TiberiumDefOf.UseAirlock, airLockDoors[1], room.GeneralCenter());
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

        /*
        [HarmonyPatch(typeof(ReachabilityCache)), HarmonyPatch(nameof(ReachabilityCache.CachedResultFor))]
        public static class ReachabilityCache_CachedResultForPatch
        {
            public static void Postfix(District A, District B, TraverseParms traverseParams, ref BoolUnknown __result)
            {
                if (__result == BoolUnknown.Unknown) return;

                if (A.Room.GetRoomComp<RoomComponent_AirLock>().LockedDown ||
                    B.Room.GetRoomComp<RoomComponent_AirLock>().LockedDown)
                {
                    __result = BoolUnknown.Unknown;
                }
            }
        }
        */

        [HarmonyPatch(typeof(JobDriver)), HarmonyPatch(nameof(JobDriver.Cleanup))]
        public static class JobDriver_CleanupPatch
        {
            public static JobCondition _LastJobCondition;

            public static bool Prefix(JobCondition condition)
            {
                _LastJobCondition = condition;
                return true;
            }
        }
    }
}

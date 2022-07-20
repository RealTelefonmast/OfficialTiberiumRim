﻿using System;
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
        #region Old Job on Path Injection
        //Pathing
        /*
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
                var airLockRooms = lastPathNodes.RoomsAlongPath(___pawn.Map, TiberiumDefOf.TR_AirLock);

                //TRLog.Debug($"[{___pawn.NameShortColored}]RoomsAlongPath: {airLockRooms.Select(t => t.ID).ToStringSafeEnumerable()}");

                //No airlocks, no job
                if (airLockRooms.NullOrEmpty()) return;

                bool desiresAirlock = false;
                foreach (var room in airLockRooms)
                {
                    TRLog.Debug($"Checking AirLockRoom: [{room.ID}]");
                    //If airlock is inactive or cannot be used for any reason, skip it
                    var airLockComp = room.GetRoomComp<RoomComponent_AirLock>();
                    //Gets an array of size 2 of the 2 airlock doors tha pawn passes through in reverse order
                    var airLockDoors = airLockComp.AirLocksOnPath(lastPathNodes, ___pawn);
                    if (!airLockComp.ShouldBeUsed(___pawn, airLockDoors, entryRoom == room)) continue;

                    //Add pawn to the FCFS pawn queue
                    airLockComp.Notify_EnqueuePawn(___pawn);

                    var carriedThing = ___pawn.carryTracker.CarriedThing;
                    var queue = new List<LocalTargetInfo>() { ___pawn.CurJob.targetB };

                    var curJob = ___pawn.CurJob;
                    if (!curJob.def.suspendable)
                    {
                        var queueA = curJob.targetQueueA?.ListFullCopy();
                        var queueB = curJob.targetQueueB?.ListFullCopy();
                        TRLog.Debug($"(Before)TargetA:{curJob.targetA} | TargetB :{curJob.targetB} | TargetC :{curJob.targetC}");
                        TRLog.Debug($"Queues Before: A:{curJob.targetQueueA?.Select(t => t.Thing).ToStringSafeEnumerable()} | B:{curJob.targetQueueB?.Select(t => t.Thing).ToStringSafeEnumerable()} | Count:{curJob.countQueue.ToStringSafeEnumerable()}");
                        TRLog.Debug($"Bill Before: {curJob.bill}");
                        
                        ___pawn.jobs.jobQueue.EnqueueLast(curJob);
                        ___pawn.jobs.CleanupCurrentJob(JobCondition.Ongoing, false, true, false);

                        TRLog.Debug($"(After)TargetA:{curJob.targetA} | TargetB :{curJob.targetB} | TargetC :{curJob.targetC}");
                        TRLog.Debug($"Queues After: A:{curJob.targetQueueA?.Select(t => t.Thing).ToStringSafeEnumerable()} | B:{curJob.targetQueueB?.Select(t => t.Thing).ToStringSafeEnumerable()}| Count:{curJob.countQueue.ToStringSafeEnumerable()}");
                        TRLog.Debug($"Bill After: {curJob.bill}");
                    }

                    //Start the airlock job and set the current job to be resumed
                    Job theJob = JobMaker.MakeJob(TiberiumDefOf.UseAirlock, airLockDoors[1], room.GeneralCenter(), carriedThing);
                    theJob.targetQueueA = queue;

                    ___pawn.jobs.StartJob(theJob, JobCondition.Ongoing, null, true);
                    //TRLog.Debug($"Queue Now: {___pawn.jobs.jobQueue.jobs.Select(t => t.job.GetReport(___pawn)).ToStringSafeEnumerable()}");

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
        */
        #endregion

        [HarmonyPatch(typeof(Pawn_PathFollower)), HarmonyPatch(nameof(Pawn_PathFollower.TrySetNewPath))]
        public static class PathFollower_TrySetNewPathPatch
        {
            //Patch the new path setter method to hook into new logic depending on the path
            public static void Postfix(Pawn_PathFollower __instance, ref bool __result, Pawn ___pawn)
            {
                //Bad path... return
                if (!__result) return;
                ___pawn.GetComp<Comp_PathFollowerExtra>().Notify_NewPath(__instance.curPath);
            }
        }

        [HarmonyPatch(typeof(Pawn_PathFollower)), HarmonyPatch(nameof(Pawn_PathFollower.StopDead))]
        public static class PathFollower_StopDeadPatch
        {
            //
            public static void Postfix(Pawn ___pawn)
            {
                ___pawn.GetComp<Comp_PathFollowerExtra>().Notify_StopDead();
            }
        }

        [HarmonyPatch(typeof(Pawn_PathFollower)), HarmonyPatch(nameof(Pawn_PathFollower.SetupMoveIntoNextCell))]
        public static class PathFollower_SetupMoveIntoNextCell
        {
            //
            public static bool Prefix(Pawn_PathFollower __instance, Pawn ___pawn, IntVec3 ___nextCell)
            {
                if (!___pawn.GetComp<Comp_PathFollowerExtra>().CanSetupMoveIntoNextCell(___nextCell))
                    return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_PathFollower)), HarmonyPatch(nameof(Pawn_PathFollower.TryEnterNextPathCell))]
        public static class TryEnterNextPathCellPatch
        {
            public static bool Prefix(Pawn_PathFollower __instance, Pawn ___pawn, IntVec3 ___nextCell)
            {
                if (!___pawn.GetComp<Comp_PathFollowerExtra>().CanEnterNextCell(___nextCell))
                {
                    return false;
                }
                return true;
            }
        }

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

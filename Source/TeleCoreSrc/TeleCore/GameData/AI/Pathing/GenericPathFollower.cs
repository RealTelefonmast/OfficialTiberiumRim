using RimWorld;
using TeleCore.Events;
using TeleCore.Events.Args;
using TeleCore.Logging;
using TeleCore.Mod.Patches;
using TeleCore.Utils;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TeleCore.GameData.AI.Pathing;

public class GenericPathFollower : IExposable
{
    private const int MaxMoveTicks = 450;
    private const int MaxCheckAheadNodes = 20;
    private const int MinCostWalk = 50;
    private static readonly float moveSpeed = 1;

    //
    public PawnPath curPath;
    private LocalTargetInfo destination;
    private IntVec3 lastCell;
    private int lastMovedTick = -999999;
    public IntVec3 lastPathedTargetPosition;

    //
    private bool moving;

    //
    public IntVec3 nextCell;

    //
    public float nextCellCostLeft;
    public float nextCellCostTotal = 1f;
    private PathEndMode peMode;
    private TraverseMode traverseMode;

    public GenericPathFollower(Thing forThing)
    {
        Thing = forThing;
    }

    //
    public Thing Thing { get; }
    public Verse.Map Map => Thing.Map;

    public LocalTargetInfo Destination => destination;

    public void ExposeData()
    {
        Scribe_Values.Look(ref moving, "moving", true);
        Scribe_Values.Look(ref nextCell, "nextCell");
        Scribe_Values.Look(ref nextCellCostLeft, "nextCellCostLeft");
        Scribe_Values.Look(ref nextCellCostTotal, "nextCellCostInitial");
        Scribe_Values.Look(ref peMode, "peMode");
        Scribe_Values.Look(ref lastMovedTick, "lastMovedTick", -999999);
        Scribe_Values.Look(ref traverseMode, "traverseMode");
        if (moving) Scribe_TargetInfo.Look(ref destination, "destination");
    }

    public event MovedEventHandler OnMoved;

    public void StartPath(LocalTargetInfo dest, PathEndMode peMode, TraverseMode traverseMode)
    {
        //
        StopDead();

        //
        this.peMode = peMode;
        this.traverseMode = traverseMode;

        if (dest.HasThing && dest.ThingDestroyed)
        {
            TLog.Error($"{Thing} pathing to destroyed thing {dest.Thing}");
            PatherFailed();
            return;
        }

        if (!ThingCanOccupy(Thing.Position) && !TryRecoverFromUnwalkablePosition()) return;

        if (moving && curPath != null && destination == dest && this.peMode == peMode) return;

        if (!Thing.Map.reachability.CanReach(Thing.Position, dest, peMode, traverseMode))
        {
            PatherFailed();
            return;
        }

        destination = dest;
        if (!nextCell.Walkable(Map) || NextCellDoorToWaitForOrManuallyOpen() != null ||
            nextCellCostLeft == nextCellCostTotal)
            ResetToCurrentPosition();

        /*TODO: Things can have reservation?
        PawnDestinationReservationManager.PawnDestinationReservation pawnDestinationReservation = this.curThing.Map.pawnDestinationReservationManager.MostRecentReservationFor(this.pawn);
        if (pawnDestinationReservation != null &&
            ((this.destination.HasThing && pawnDestinationReservation.target != this.destination.Cell) ||
             (pawnDestinationReservation.job != this.pawn.CurJob &&
              pawnDestinationReservation.target != this.destination.Cell)))
        {
            this.pawn.Map.pawnDestinationReservationManager.ObsoleteAllClaimedBy(this.pawn);
        }
        */

        if (AtDestinationPosition)
        {
            PatherArrived();
            return;
        }

        /*TODO: Things cant be downed
        if (this.pawn.Downed)
        {
            TLog.Error(this.pawn.LabelCap + " tried to path while downed. This should never happen. curJob=" +
                      this.pawn.CurJob.ToStringSafe<Job>());
            this.PatherFailed();
            return;
        }
        */

        curPath?.ReleaseToPool();

        curPath = null;
        moving = true;
        //this.pawn.jobs.posture = PawnPosture.Standing;
    }

    public void TryResumePathingAfterLoading()
    {
        if (moving)
            StartPath(destination, peMode, traverseMode);
    }

    public void PatherTick()
    {
        lastMovedTick = Find.TickManager.TicksGame;
        if (nextCellCostLeft > 0f)
        {
            nextCellCostLeft -= CostToPayThisTick();
            OnMoved?.Invoke(this, new MovedEventArgs(Thing, nextCell, nextCellCostLeft, nextCellCostTotal));
            return;
        }

        if (moving)
            TryEnterNextPathCell();
    }

    private void SetupMoveIntoNextCell()
    {
        if (curPath.NodesLeftCount <= 1)
        {
            TLog.Error($"{Thing} at {Thing.Position} ran out of path nodes while pathing to {destination}.");
            PatherFailed();
            return;
        }

        nextCell = curPath.ConsumeNextNode();
        if (!nextCell.Walkable(Map)) Log.Error($"{Thing} entering {nextCell} which is unwalkable.");
        var num = CostToMoveIntoCell(Thing, nextCell);
        nextCellCostTotal = num;
        nextCellCostLeft = num;
    }

    private void TryEnterNextPathCell()
    {
        // Check if there is a building blocking the next path cell
        var building = BuildingBlockingNextPathCell();
        if (building != null)
        {
            // If there is a building, check if it has a free passage (represented by a Building_Door object)
            var building_Door = building as Building_Door;
            if (building_Door is not { FreePassage: true })
            {
                // If the building doesn't have a free passage, mark the pather as failed and exit the method
                PatherFailed();
                return;
            }
        }

        // Check if there is a door in the next cell to wait for or manually open
        var building_Door2 = NextCellDoorToWaitForOrManuallyOpen();
        if (building_Door2 != null)
            // If there is a door, exit the method and wait for it to open
            return;

        // Save the current position as the last cell and move the thing (represented by curThing) to the next cell
        lastCell = Thing.Position;
        Thing.Position = nextCell;

        // If the thing has a base mass greater than 5f, decrease the snow depth at its new position
        if (Thing.def.BaseMass > 5f)
            Thing.Map.snowGrid.AddDepth(Thing.Position, -0.001f);

        // If a new path is needed and cannot be set, exit the method
        if (NeedNewPath() && !TrySetNewPath()) return;

        // If the thing has arrived at its destination position, mark the pather as arrived and exit the method
        if (AtDestinationPosition)
        {
            PatherArrived();
            return;
        }

        // Set up the move into the next cell and exit the method
        SetupMoveIntoNextCell();
    }

    private PawnPath GenerateNewPath()
    {
        PathFinderPatches.UsedGenericPather = this;
        lastPathedTargetPosition = destination.Cell;
        var pathResult = AIUtils.TryGetPath(Thing.Position, destination, Map, traverseMode, peMode).Path;
        PathFinderPatches.UsedGenericPather = null;
        return pathResult;
    }

    private bool TrySetNewPath()
    {
        var pawnPath = GenerateNewPath();
        if (!pawnPath.Found)
        {
            PatherFailed();
            return false;
        }

        curPath?.ReleaseToPool();
        curPath = pawnPath;
        return true;
    }

    private bool NeedNewPath()
    {
        if (!destination.IsValid || curPath is not {Found: true} || curPath.NodesLeftCount == 0) return true;

        if (destination.HasThing && destination.Thing.Map != Thing.Map) return true;

        if ((Thing.Position.InHorDistOf(curPath.LastNode, 15f) ||
             Thing.Position.InHorDistOf(destination.Cell, 15f)) &&
            !Thing.Map.reachability.CanReach(curPath.LastNode, destination, peMode, traverseMode))
            return true;

        if (curPath.UsedRegionHeuristics && curPath.NodesConsumedCount >= 75) return true;

        if (lastPathedTargetPosition != destination.Cell)
        {
            float num = (Thing.Position - destination.Cell).LengthHorizontalSquared;
            var num2 = num switch
            {
                > 900f => 10f,
                > 289f => 5f,
                > 100f => 3f,
                > 49f => 2f,
                _ => 0.5f
            };

            if ((lastPathedTargetPosition - destination.Cell).LengthHorizontalSquared > num2 * num2) return true;
        }

        var pc = Thing.Map.pathing.normal;
        var intVec = IntVec3.Invalid;
        var num3 = 0;
        while (num3 < 20 && num3 < curPath.NodesLeftCount)
        {
            var intVec2 = curPath.Peek(num3);
            if (!intVec2.Walkable(pc.map)) return true;

            var building_Door = intVec2.GetEdifice(pc.map) as Building_Door;
            if (building_Door != null)
                if (!building_Door.FreePassage)
                    return true;

            if (num3 != 0 && intVec2.AdjacentToDiagonal(intVec) &&
                (PathFinderExtended.BlocksDiagonalMovement(intVec2.x, intVec.z, pc, false) ||
                 PathFinderExtended.BlocksDiagonalMovement(intVec.x, intVec2.z, pc, false)))
                return true;

            intVec = intVec2;
            num3++;
        }

        return false;
    }

    private float CostToPayThisTick()
    {
        var num = 1f;
        if (num < nextCellCostTotal / MaxMoveTicks)
            num = nextCellCostTotal / MaxMoveTicks;
        return num;
    }

    private int TicksPerMove(bool diagonal)
    {
        var num = moveSpeed;
        var num2 = num / 60f;
        float num3;
        if (num2 == 0f)
        {
            num3 = MaxMoveTicks;
        }
        else
        {
            num3 = 1f / num2;
            if (Thing.Spawned && !Map.roofGrid.Roofed(Thing.Position))
                num3 /= Map.weatherManager.CurMoveSpeedMultiplier;
            if (diagonal) num3 *= 1.41421f;
        }

        return Mathf.Clamp(Mathf.RoundToInt(num3), 1, MaxMoveTicks);
    }

    private int CostToMoveIntoCell(Thing thing, IntVec3 c)
    {
        int num;
        if (c.x == thing.Position.x || c.z == thing.Position.z) //Cardinal
            num = TicksPerMove(false);
        else //Diagonal
            num = TicksPerMove(true);
        num += Map.pathing.normal.pathGrid.CalculatedCostAt(c, false, Thing.Position);
        if (num > MaxMoveTicks) num = MaxMoveTicks;
        return Mathf.Max(num, 1);
    }

    //TODO: Irrelevant for non-pawn pathed things
    public Building_Door NextCellDoorToWaitForOrManuallyOpen()
    {
        var building_Door = Thing.Map.thingGrid.ThingAt<Building_Door>(nextCell);
        if (building_Door is {SlowsPawns: true} &&
            (!building_Door.Open || building_Door.TicksTillFullyOpened > 0) /*&& building_Door.PawnCanOpen(this.pawn)*/
           ) return building_Door;
        return null;
    }

    public Building BuildingBlockingNextPathCell()
    {
        var edifice = nextCell.GetEdifice(Thing.Map);
        if (edifice != null && edifice.def.passability == Traversability.Impassable) return edifice;
        return null;
    }

    private bool ThingCanOccupy(IntVec3 c)
    {
        if (!c.Walkable(Map)) return false;
        var edifice = c.GetEdifice(Thing.Map);
        var building_Door = edifice as Building_Door;
        return building_Door == null || building_Door.Open; //building_Door.PawnCanOpen(this.pawn) 
    }

    public bool TryRecoverFromUnwalkablePosition(bool error = true)
    {
        var flag = false;
        var i = 0;
        while (i < GenRadial.RadialPattern.Length)
        {
            var pos = Thing.Position;
            var intVec = pos + GenRadial.RadialPattern[i];
            if (ThingCanOccupy(intVec))
            {
                if (intVec == pos) return true;
                if (error) TLog.Warning($"{Thing} on unwalkable cell {Thing.Position}. Teleporting to {intVec}");

                Thing.Position = intVec;
                //this.curThing.Notify_Teleported(true, false);
                flag = true;
                break;
            }

            i++;
        }

        if (!flag)
        {
            Thing.Destroy();
            TLog.Error(
                $"{Thing.ToStringSafe()} on unwalkable cell {Thing.Position}. Could not find walkable position nearby. Destroyed.");
        }

        return flag;
    }

    private void PatherArrived()
    {
        StopDead();
        /*TODO: No pawn no job notifier
        if (this.pawn.jobs.curJob != null)
        {
            this.pawn.jobs.curDriver.Notify_PatherArrived();
        }
        */
    }

    private void PatherFailed()
    {
        StopDead();
    }

    public void ResetToCurrentPosition()
    {
        nextCell = Thing.Position;
        nextCellCostLeft = 0f;
        nextCellCostTotal = 1f;
    }

    public void StopDead()
    {
        if (curPath != null)
            curPath.ReleaseToPool();
        curPath = null;
        moving = false;
        nextCell = Thing.Position;
    }

    //
    public void DrawPath()
    {
        if (curPath is not { Found: true }) return;

        var y = AltitudeLayer.Item.AltitudeFor();
        if (curPath.NodesLeftCount > 0)
        {
            for (var i = 0; i < curPath.NodesLeftCount - 1; i++)
            {
                var a = curPath.Peek(i).ToVector3Shifted();
                a.y = y;
                var b = curPath.Peek(i + 1).ToVector3Shifted();
                b.y = y;
                GenDraw.DrawLineBetween(a, b);
            }

            if (Thing != null)
            {
                var drawPos = Thing.DrawPos;
                drawPos.y = y;
                var b2 = curPath.Peek(0).ToVector3Shifted();
                b2.y = y;
                if ((drawPos - b2).sqrMagnitude > 0.01f) GenDraw.DrawLineBetween(drawPos, b2);
            }
        }
    }

    #region States

    public bool AtDestinationPosition =>
        TouchPathEndModeUtility.IsAdjacentOrInsideAndAllowedToTouch(Thing.Position, destination, Map.pathing.normal);

    public bool Moving => moving;

    public float MovedPercent
    {
        get
        {
            if (!Moving) return 0f;
            if (BuildingBlockingNextPathCell() != null) return 0f;
            if (NextCellDoorToWaitForOrManuallyOpen() != null) return 0f;
            return 1f - nextCellCostLeft / nextCellCostTotal;
        }
    }

    #endregion
}
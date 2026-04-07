using System.Collections.Generic;
using System.Linq;
using TR.Data.Enums;
using TR.GameParts;
using TR.Util;
using TR.VeinholeSystem;
using UnityEngine;
using Verse;

namespace TR.Data.ThingClasses;

public class VeinHub : TRBuilding
{
    public CellArea affectedArea;
    public List<IntVec3> AffectedCells = new();
    public float radius = 12.59f;
    private Environment.Veinholes.VeinholeSystem system;

    private bool Alive => system.IsAlive;

    public void Setup(Veinhole parent)
    {
        system = parent.System;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        affectedArea = new CellArea();
        AffectedCells = GenRadial.RadialCellsAround(Position, radius, false).ToList();
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        system.RemovePart(this, VeinholeSystemType.Hub);
        base.Destroy(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
    }

    public override void TickRare()
    {
        /*
        if (Dying)
        {
            TakeDamage(new DamageInfo(DamageDefOf.Crush, Rand.Range(3f, 13f)));
            return;
        }
        */
        foreach (var cell in AffectedCells)
        {
            var pawn = cell.GetFirstPawn(Map);
            if (pawn != null && TRandom.Chance(0.86f)) LaunchGas(pawn);
        }
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        GenDraw.DrawFieldEdges(AffectedCells, Color.green);
    }

    private void LaunchGas(Pawn pawn)
    {
        var gas = (VeinGasCloud)GenSpawn.Spawn(ThingDef.Named("VeinGasCloud"), Position, Map);
        gas.SetTarget(pawn);
    }
}
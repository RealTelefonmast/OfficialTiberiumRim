using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TR;

public abstract class PawnVeinOverlayDrawer : PawnOverlayDrawer
{
    public bool isCoveredInVeins;

    public PawnVeinOverlayDrawer(Pawn pawn) : base(pawn)
    {
    }

    public override void WriteCache(CacheKey key, List<DrawCall> writeTarget)
    {
        throw new NotImplementedException();
    }

    public void DrawOn(Pawn pawn)
    {
        var drawPos = pawn.DrawPos;
    }
}
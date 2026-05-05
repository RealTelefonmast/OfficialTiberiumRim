using System;
using System.Collections.Generic;
using UnityEngine.Pool;
using Verse;

namespace TeleCore.Types.Structs;

public struct HediffIterator<THediff>() : IDisposable
    where THediff : Hediff
{
    private List<THediff> _tmpList = ListPool<THediff>.Get();

    public IReadOnlyList<THediff> Hediffs => _tmpList;

    public static HediffIterator<THediff> ForPawn(Pawn pawn)
    {
        var iterator = new HediffIterator<THediff>();
        pawn.health.hediffSet.GetHediffs<THediff>(ref iterator._tmpList);
        return iterator;
    }

    public void Dispose()
    {
        ListPool<THediff>.Release(_tmpList);
    }
}
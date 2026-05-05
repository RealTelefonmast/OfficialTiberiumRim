using System;
using Verse;

namespace TeleCore.Types;

public class PawnHediffChangedEventArgs : EventArgs
{
    public PawnHediffChangedEventArgs(Hediff hediff, DamageInfo? dinfo)
    {
        Pawn = hediff.pawn;
        Hediff = hediff;
        DamageInfo = dinfo;
    }

    public Pawn Pawn { get; }
    public Hediff Hediff { get; }
    public DamageInfo? DamageInfo { get; }
}
using Verse;

namespace TeleCore.Events.Args;

public struct PawnHediffChangedEventArgs
{
    public Pawn Pawn { get; }
    public Hediff Hediff { get; }
    public DamageInfo? DamageInfo { get; }

    public PawnHediffChangedEventArgs(Hediff hediff, DamageInfo? dinfo)
    {
        Pawn = hediff.pawn;
        Hediff = hediff;
        DamageInfo = dinfo;
    }
}
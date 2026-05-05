using System.Text;
using RimWorld;
using TeleCore.Comps;
using TeleCore.Defs;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Needs;

public class Need_Respiration : Need
{
    private readonly Comp_PawnAtmosphereTracker _atmosTracker;
    private readonly RespirationExtension _cachedProps;

    private int tickTracker;

    private int tickTracker;
    private int tickTrackerCondition;
    private int tickTrackerCondition;

    public Need_Respiration(Pawn pawn) : base(pawn)
    {
        _atmosTracker = Comp_PawnAtmosphereTracker.CompFor(pawn);
        _cachedProps = pawn.kindDef.GetModExtension<RespirationExtension>();
        curLevelInt = 1f;
    }

    public Need_Respiration(Pawn pawn) : base(pawn)
    {
        _atmosTracker = Comp_PawnAtmosphereTracker.CompFor(pawn);
        _cachedProps = pawn.kindDef.GetModExtension<RespirationExtension>();
        curLevelInt = 1f;
    }

    public bool NeedsToBreathe => _cachedProps != null;

    public override bool IsFrozen => !NeedsToBreathe;

    private AtmosphericVolume Volume => _atmosTracker.RoomComp.Volume;

    public override string GetTipString()
    {
        var sb = new StringBuilder(base.GetTipString());
        if (NeedsToBreathe)
        {
            sb.AppendLine("\nDetails:");
            var inhaled = _cachedProps.inhaledGas;
            var exhaled = _cachedProps.exhaledGas;
            var inhaledLabel = ((TaggedString)inhaled?.LabelCap).Colorize(_cachedProps.inhaledGas.valueColor);
            if (inhaled != null)
            {
                var h = _cachedProps.intervalTicks / (float)GenDate.TicksPerHour;
                var inhaledPerHour = $"{_cachedProps.unitsInPerInterval * h}L".Colorize(inhaled.valueColor);

                if (exhaled != null)
                {
                    var exhaledLabel = ((TaggedString)exhaled?.LabelCap).Colorize(_cachedProps.exhaledGas.valueColor);
                    var exhaledPerHour = $"{_cachedProps.unitsOutPerInterval * h}L".Colorize(exhaled.valueColor);
                    sb.AppendLine(
                        $"This pawn will turn {inhaledLabel} into {exhaledLabel} at a rate of {inhaledPerHour} to {exhaledPerHour} per hour.");
                    return sb.ToString();
                }

                sb.AppendLine($"This pawn will consume {inhaledLabel} at a rate of {inhaledPerHour}L/h.");
                return sb.ToString();
            }

            return sb.ToString();
        }

        sb.AppendLine("\nThis pawn has no respiratory needs. Not even a vacuum could harm them!");
        return sb.ToString();
        return base.GetTipString();
    }

    public override void NeedInterval()
    {
        if (!pawn.Spawned || !NeedsToBreathe) return;
        if (_atmosTracker == null) return;

        CurLevel = _atmosTracker.RoomComp.Volume.StoredPercentOf(_cachedProps.inhaledGas);

        if (tickTrackerCondition > _cachedProps.conditionTicks)
        {
            //Custom Worker
            if (_cachedProps.worker != null) _cachedProps.worker.OnInterval(pawn, CurLevel, this);

            //Stage Effect
            if (_cachedProps.UsesStages)
            {
                var stage = _cachedProps.StageAt(CurLevel);
                if (stage.hediffGivers != null)
                    foreach (var hediffGiver in stage.hediffGivers)
                        hediffGiver.OnIntervalPassed(pawn, null);
            }

            tickTrackerCondition = 0;
        }
        else
        {
            tickTrackerCondition += 150; //Need interval is done via 150 hash interval ticking
        }

        if (tickTracker > _cachedProps.intervalTicks)
        {
            //
            if (!_atmosTracker.IsOutside)
            {
                _atmosTracker.RoomComp.Volume.TryRemove(_cachedProps.inhaledGas, _cachedProps.unitsInPerInterval);
                if (_cachedProps.exhaledGas != null)
                    _atmosTracker.RoomComp.Volume.TryAdd(_cachedProps.exhaledGas, _cachedProps.unitsOutPerInterval);
            }

            tickTracker = 0;
        }
        else
        {
            tickTracker += 150; //Need interval is done via 150 hash interval ticking
        }
    }
}
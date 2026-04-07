using System.Text;
using RimWorld;
using TeleCore.Atmosphere.Comps;
using TeleCore.Atmosphere.Rooms;
using Verse;

namespace TeleCore.Atmosphere.Health;

public class Need_Respiration : Need
{
    private Comp_PawnAtmosphereTracker _atmosTracker;
    private RespirationExtension _cachedProps;

    public bool NeedsToBreathe => _cachedProps != null;

    public override bool IsFrozen => !NeedsToBreathe;

    private AtmosphericVolume Volume => _atmosTracker.RoomComp.Volume;

    public override string GetTipString()
    {
        StringBuilder sb = new StringBuilder(base.GetTipString());
        if (NeedsToBreathe)
        {
            sb.AppendLine("\nDetails:");
            var inhaled = _cachedProps.inhaledGas;
            var exhaled = _cachedProps.exhaledGas;
            var inhaledLabel = ColoredText.Colorize((TaggedString)inhaled?.LabelCap, _cachedProps.inhaledGas.valueColor);
            if (inhaled != null)
            {
                var h = _cachedProps.intervalTicks / (float)GenDate.TicksPerHour;
                var inhaledPerHour = $"{_cachedProps.unitsInPerInterval * h}L".Colorize(inhaled.valueColor);

                if (exhaled != null)
                {
                    var exhaledLabel = ColoredText.Colorize((TaggedString)exhaled?.LabelCap, _cachedProps.exhaledGas.valueColor);
                    var exhaledPerHour = $"{_cachedProps.unitsOutPerInterval * h}L".Colorize(exhaled.valueColor);
                    sb.AppendLine($"This pawn will turn {inhaledLabel} into {exhaledLabel} at a rate of {inhaledPerHour} to {exhaledPerHour} per hour.");
                    return sb.ToString();
                }

                sb.AppendLine($"This pawn will consume {inhaledLabel} at a rate of {inhaledPerHour}L/h.");
                return sb.ToString();
            }

            return sb.ToString();
        }
        else
        {
            sb.AppendLine("\nThis pawn has no respiratory needs. Not even a vacuum could harm them!");
            return sb.ToString();
        }
        return base.GetTipString();
    }

    public Need_Respiration(Pawn pawn) : base(pawn)
    {
        _atmosTracker = Comp_PawnAtmosphereTracker.CompFor(pawn);
        _cachedProps = pawn.kindDef.GetModExtension<RespirationExtension>();
        curLevelInt = 1f;
    }

    private int tickTracker = 0;
    private int tickTrackerCondition = 0;

    public override void NeedInterval()
    {
        if (!pawn.Spawned || !NeedsToBreathe) return;
        if (_atmosTracker == null) return;

        CurLevel = _atmosTracker.RoomComp.Volume.StoredPercentOf(_cachedProps.inhaledGas);

        if (tickTrackerCondition > _cachedProps.conditionTicks)
        {
            //Custom Worker
            if (_cachedProps.worker != null)
            {
                _cachedProps.worker.OnInterval(pawn, CurLevel, this);
            }

            //Stage Effect
            if (_cachedProps.UsesStages)
            {
                var stage = _cachedProps.StageAt(CurLevel);
                if (stage.hediffGivers != null)
                {
                    foreach (var hediffGiver in stage.hediffGivers)
                    {
                        hediffGiver.OnIntervalPassed(pawn, null);
                    }
                }
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

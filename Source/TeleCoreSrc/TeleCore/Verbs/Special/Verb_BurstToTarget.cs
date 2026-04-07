using Verse;

namespace TeleCore.Verbs.Special;

public class Verb_BurstToTarget : Verb_LaunchProjectile
{
    public override bool TryCastShot()
    {
        return base.TryCastShot();
        
        //TODO: Better burst with a line from center to target, getting offsets along the line, each new shot going closer to the target
        /*if (!currentTarget.IsValid) return false;
        var from = Caster.Position;
        var to = currentTarget.Cell;
        var distance = from.DistanceTo(to);
        var minDist = this.verbProps.minRange;
        var travelDistance = distance - minDist;
        
        //Adjust From For MinRange
        var normed = (to - from).ToVector3().normalized;
        if (minDist > 0)
        {
            var newFrom = from + (normed * minDist).ToIntVec3();
            from = newFrom;
        }
        
        if (distance < Props.range)
        {
            var newTo = from + (normed * travelDistance).ToIntVec3();
            to = newTo;
        }

        var line = new ShootLine(from, to);
        foreach (var cell in line.Points())
        {
            //if (cell.DistanceTo(from) <= Props.minRange) continue;
            var line2 = new ShootLine(from, cell);
            AdjustedTarget(cell, ref line2, out var flags);
            CastProjectile(from, caster, CurrentStartPos, cell, currentTarget, flags, false, null, null);
        }

        return true;*/
    }
}
using Verse;

namespace TiberiumRim
{
    public class Verb_BurstToTarget : Verb_TR
    {
        protected override bool TryCastShot()
        {
            if (!currentTarget.IsValid) return false;
            var from = DrawPos.ToIntVec3();
            var to = currentTarget.Cell;
            var distance = from.DistanceTo(to);
            if (distance < Props.range)
            {
                var normed = (to - from).ToVector3().normalized;
                IntVec3 newTo = from + (normed * Props.range).ToIntVec3();
                to = newTo;
            }

            var line = new ShootLine(from, to);
            foreach (IntVec3 cell in line.Points())
            {
                if(cell.DistanceTo(from) <= Props.minRange) continue;
                ShootLine line2 = new ShootLine(from, cell);
                AdjustedTarget(cell, ref line2, out ProjectileHitFlags flags);
                CastProjectile(from, caster, cell, currentTarget, flags);
            }
            return true;
        }
    }
}

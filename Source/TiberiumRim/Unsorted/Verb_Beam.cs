using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class Verb_Beam : Verb_TR
{
    public ThingDef GunDef => CasterIsPawn ? EquipmentSource.def : caster.def.building.turretGunDef;

    public override bool TryCastBeam()
    {
        if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map) return false;
        var shootLine = new ShootLine(caster.Position, currentTarget.Cell);
        if (verbProps.stopBurstWithoutLos &&
            !TryFindShootLineFromTo(caster.Position, currentTarget, out shootLine)) return false;
        var adjustedTarget = AdjustedTarget(currentTarget, ref shootLine, out var flags);
        var Laser = Props.laser;
        var damage = Laser.damageDef ?? DamageDefOf.Burn;
        if (adjustedTarget.HasThing)
            adjustedTarget.Thing.TakeDamage(new DamageInfo(damage, Laser.damageBase, 0f, -1, caster, null, GunDef,
                DamageInfo.SourceCategory.ThingOrUnknown, currentTarget.Thing));
        var targetPos = adjustedTarget.Cell.ToVector3Shifted();
        for (var i = 0; i < 3; i++)
        {
            MoteMaker.ThrowSmoke(targetPos, caster.Map, 2f);
            MoteMaker.ThrowMicroSparks(targetPos, caster.Map);
        }

        var start = ShotOrigin();
        var beam = (Mote_Beam)ThingMaker.MakeThing(TiberiumDefOf.LaserBeam);
        var mat = MaterialPool.MatFrom(Laser.beamPath, ShaderDatabase.MoteGlow);
        beam.SetConnections(start, targetPos, mat, Color.white);
        beam.Attach(caster);
        GenSpawn.Spawn(beam, caster.Position, caster.Map);
        if (Laser.glow != null)
        {
            var glow = (MoteThrown)ThingMaker.MakeThing(
                Laser.glow.glowMote /*DefDatabase<ThingDef>.GetNamed("ObeliskGlow")*/);
            glow.exactPosition = start;
            glow.Scale = Laser.glow.scale;
            glow.exactRotation = Laser.glow.rotation;
            glow.rotationRate = Laser.glow.rotationRate;
            glow.airTimeLeft = 99999;
            glow.SetVelocity(0, 0);
            GenSpawn.Spawn(glow, caster.Position + IntVec3.East, caster.Map);
        }

        Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, !currentTarget.HasThing ? null : currentTarget.Thing,
            EquipmentSource == null ? null : EquipmentSource.def, null, false));
        return true;
    }
}
using RimWorld;
using TeleCore.Rendering;
using TR.Defs;
using UnityEngine;
using Verse;

namespace TR.Weaponry;

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
        var beamProps = Props.beamProps;
        var damage = beamProps.damageDef ?? DamageDefOf.Burn;
        if (adjustedTarget.HasThing)
            adjustedTarget.Thing.TakeDamage(new DamageInfo(damage, beamProps.damageBase, 0f, -1, caster, null, GunDef,
                DamageInfo.SourceCategory.ThingOrUnknown, currentTarget.Thing));
        var targetPos = adjustedTarget.Cell.ToVector3Shifted();
        // for (int i = 0; i < 3; i++)
        // {
        //     MoteMaker.ThrowSmoke(targetPos, caster.Map, 2f);
        //     MoteMaker.ThrowMicroSparks(targetPos, caster.Map);
        // }
        beamProps.hitEffecter?.Spawn(adjustedTarget.Cell, caster.Map);
        var start = ShotOrigin();
        var beam = (Mote_Beam)ThingMaker.MakeThing(TiberiumDefOf.Mote_Beam);
        var mat = MaterialPool.MatFrom(beamProps.beamPath, ShaderDatabase.MoteGlow);
        beam.solidTimeOverride = beamProps.solidTime;
        beam.fadeInTimeOverride = beamProps.fadeInTime;
        beam.fadeOutTimeOverride = beamProps.fadeOutTime;
        beam.SetConnections(start, targetPos, mat, Color.white);
        beam.Attach(caster);
        GenSpawn.Spawn(beam, caster.Position, caster.Map);
        if (beamProps.glow != null)
        {
            //TODO: Replace motes with more fine-tuned settings (eg. fade-in and -out time)
            var glow = (MoteThrown)ThingMaker.MakeThing(
                beamProps.glow.glowMote /*DefDatabase<ThingDef>.GetNamed("ObeliskGlow")*/);
            glow.exactPosition = start;
            glow.Scale = beamProps.glow.scale;
            glow.exactRotation = beamProps.glow.rotation;
            glow.rotationRate = beamProps.glow.rotationRate;
            glow.airTimeLeft = 99999;
            glow.SetVelocity(0, 0);
            GenSpawn.Spawn(glow, caster.Position + IntVec3.East, caster.Map);
        }

        Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, !currentTarget.HasThing ? null : currentTarget.Thing,
            EquipmentSource == null ? null : EquipmentSource.def, null, false));
        return true;
    }
}
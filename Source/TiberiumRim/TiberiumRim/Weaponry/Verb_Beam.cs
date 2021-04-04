using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Verb_Beam : Verb_TR
    {
        public ThingDef GunDef => CasterIsPawn ? EquipmentSource.def : caster.def.building.turretGunDef;

        public override bool TryCastBeam()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }
            ShootLine shootLine = new ShootLine(caster.Position, currentTarget.Cell);
            if (verbProps.stopBurstWithoutLos && !TryFindShootLineFromTo(caster.Position, currentTarget, out shootLine))
            {
                return false;
            }
            LocalTargetInfo adjustedTarget = AdjustedTarget(currentTarget, ref shootLine, out ProjectileHitFlags flags);
            var beamProps = Props.beamProps;
            DamageDef damage = beamProps.damageDef ?? DamageDefOf.Burn;
            if(adjustedTarget.HasThing)
                adjustedTarget.Thing.TakeDamage(new DamageInfo(damage, beamProps.damageBase, 0f, -1, caster, null, GunDef, DamageInfo.SourceCategory.ThingOrUnknown, currentTarget.Thing));
            Vector3 targetPos = adjustedTarget.Cell.ToVector3Shifted();
            // for (int i = 0; i < 3; i++)
            // {
            //     MoteMaker.ThrowSmoke(targetPos, caster.Map, 2f);
            //     MoteMaker.ThrowMicroSparks(targetPos, caster.Map);
            // }
            beamProps.hitEffecter?.Spawn(adjustedTarget.Cell, caster.Map);
            Vector3 start = ShotOrigin();
            Mote_Beam beam = (Mote_Beam)ThingMaker.MakeThing(TiberiumDefOf.Mote_Beam);
            Material mat = MaterialPool.MatFrom(beamProps.beamPath, ShaderDatabase.MoteGlow);
            beam.solidTimeOverride = beamProps.solidTime;
            beam.fadeInTimeOverride = beamProps.fadeInTime;
            beam.fadeOutTimeOverride = beamProps.fadeOutTime;
            beam.SetConnections(start, targetPos, mat, Color.white);
            beam.Attach(caster);
            GenSpawn.Spawn(beam, caster.Position, caster.Map, WipeMode.Vanish);
            if (beamProps.glow != null)
            {
                //TODO: Replace motes with more fine-tuned settings (eg. fade-in and -out time)
                MoteThrown glow = (MoteThrown)ThingMaker.MakeThing(beamProps.glow.glowMote /*DefDatabase<ThingDef>.GetNamed("ObeliskGlow")*/);
                glow.exactPosition = start;
                glow.Scale = beamProps.glow.scale;
                glow.exactRotation = beamProps.glow.rotation;
                glow.rotationRate = beamProps.glow.rotationRate;
                glow.airTimeLeft = 99999;
                glow.SetVelocity(0, 0);
                GenSpawn.Spawn(glow, caster.Position + IntVec3.East, caster.Map);
            }
            Find.BattleLog.Add(new BattleLogEntry_RangedFire(this.caster, (!this.currentTarget.HasThing) ? null : this.currentTarget.Thing, (base.EquipmentSource == null) ? null : base.EquipmentSource.def, null, false));
            return true;
        }
    }
}

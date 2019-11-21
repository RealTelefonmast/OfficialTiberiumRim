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
        public ThingDef GunDef
        {
            get
            {
                if (CasterIsPawn)
                {
                    Log.Message("EquipmentSource: " + EquipmentSource.Label);
                    return EquipmentSource.def;
                }
                return caster.def.building.turretGunDef;
            }
        }

        public override bool TryCastBeam()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }
            ShootLine shootLine;
            if (verbProps.stopBurstWithoutLos && !TryFindShootLineFromTo(caster.Position, currentTarget, out shootLine))
            {
                return false;
            }
            float num = 100;
            currentTarget.Thing.TakeDamage(new DamageInfo(DamageDefOf.Burn, num, 0f, -1, caster, null, GunDef, DamageInfo.SourceCategory.ThingOrUnknown, currentTarget.Thing));
            Vector3 loc = currentTarget.Cell.ToVector3Shifted();
            for (int i = 0; i < 3; i++)
            {
                MoteMaker.ThrowSmoke(loc, caster.Map, 2f);
                MoteMaker.ThrowMicroSparks(loc, caster.Map);
            }
            var Laser = Props.laser;
            Vector3 offset = !Props.originOffsets.NullOrEmpty() ? Props.originOffsets.RandomElement() : Vector3.zero;
            if (castingGun != null)
            {
                offset = castingGun.props.barrelOffset;
            }
            Vector3 start = this.caster.TrueCenter() + offset.RotatedBy(GunRotation);
            Mote_Beam beam = (Mote_Beam)ThingMaker.MakeThing(TiberiumDefOf.LaserBeam);
            Material mat = MaterialPool.MatFrom(Laser.beamPath, ShaderDatabase.MoteGlow);
            beam.SetConnections(start, currentTarget.CenterVector3, mat, Color.white);
            beam.Attach(caster);
            GenSpawn.Spawn(beam, caster.Position, caster.Map, WipeMode.Vanish);
            if (Laser.glow != null)
            {
                MoteThrown glow = (MoteThrown)ThingMaker.MakeThing(Laser.glow.glowMote /*DefDatabase<ThingDef>.GetNamed("ObeliskGlow")*/);
                glow.exactPosition = start;
                glow.Scale = Laser.glow.scale;
                glow.exactRotation = Laser.glow.rotation;
                glow.rotationRate = Laser.glow.rotationRate;
                glow.airTimeLeft = 99999;
                glow.SetVelocity(0, 0);
                GenSpawn.Spawn(glow, caster.Position + IntVec3.East, caster.Map);
            }
            Find.BattleLog.Add(new BattleLogEntry_RangedFire(this.caster, (!this.currentTarget.HasThing) ? null : this.currentTarget.Thing, (base.EquipmentSource == null) ? null : base.EquipmentSource.def, null, false));
            return true;
        }
    }
}

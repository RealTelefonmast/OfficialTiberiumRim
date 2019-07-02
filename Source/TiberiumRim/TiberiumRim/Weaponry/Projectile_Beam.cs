using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class Projectile_Beam : Projectile
    {
        public VerbProperties_TR Props => (VerbProperties_TR)(launcher as Building_TurretGun).gun.def.Verbs.FirstOrDefault();
        public ExtendedGraphicData ExtraData => (def as FXThingDef)?.extraData;

        protected override void Impact(Thing hitThing)
        {
            var start = origin + (ExtraData?.drawOffset ?? Vector3.zero);
            Mote_Beam beam = (Mote_Beam)ThingMaker.MakeThing(TiberiumDefOf.BeamMote);
            MoteThrown glow = (MoteThrown)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("ObeliskGlow"));
            glow.Scale = 3.5f;
            glow.exactPosition = start;
            glow.exactRotation = 0.1f;
            glow.rotationRate = 0.1f;
            glow.airTimeLeft = 99999;
            glow.SetVelocity(0, 0);
            GenSpawn.Spawn(glow, launcher.Position + IntVec3.East, launcher.Map);
            beam.SetConnections(start, destination, Graphic.MatSingle, Color.white);
            beam.Attach(launcher);
            GenSpawn.Spawn(beam, launcher.Position, launcher.Map, WipeMode.Vanish);

            Map map = base.Map;
            base.Impact(hitThing);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, this.equipmentDef, this.def, this.targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null || true)
            {
                DamageDef damageDef = this.def.projectile.damageDef;
                float amount = (float)base.DamageAmount;
                float armorPenetration = base.ArmorPenetration;
                float y = this.ExactRotation.eulerAngles.y;
                Thing launcher = this.launcher;
                ThingDef equipmentDef = this.equipmentDef;
                DamageInfo dinfo = new DamageInfo(damageDef, amount, armorPenetration, y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                intendedTarget.Thing.TakeDamage(dinfo);
                return;
            }
            IntVec3 impactPos = destination.ToIntVec3();
        }

        private void DamageSurroundings(IntVec3 cell)
        {

        }

        public override void Draw()
        {
            return;
        }
    }
}

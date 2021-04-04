using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public enum TeslaArcType : int
    {
        Arc   = 1,
        Jump  = 2,
        Spark = 3
    }

    public class Projectile_TeslaArc : Projectile
    {
        public TeslaArcType ArcType = TeslaArcType.Arc;

        private Material randomMat;

        public Material RandomMaterial
        {
            get
            {
                if (randomMat != null) return randomMat;
                switch (ArcType)
                {
                    case TeslaArcType.Arc:
                        return randomMat ??= MaterialsTesla.Arcs.RandomElement();
                    case TeslaArcType.Jump:
                        return randomMat ??= MaterialsTesla.Jumps.RandomElement();
                    case TeslaArcType.Spark:
                        return randomMat ??= MaterialsTesla.Sparks.RandomElement();
                }
                return null;
            }
            set => randomMat = value;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Tick()
        {
            base.Tick();
        }

        protected float DamageMultiplier
        {
            get
            {
                switch (ArcType)
                {
                    case TeslaArcType.Arc:
                        return 1;
                    case TeslaArcType.Jump:
                        return 0.25f;
                    case TeslaArcType.Spark:
                        return 0.1f;
                }
                return 0;
            }
        }

        public float JumpRadius
        {
            get
            {
                switch (ArcType)
                {
                    case TeslaArcType.Arc:
                        return 8;
                    case TeslaArcType.Jump:
                        return 6;
                    case TeslaArcType.Spark:
                        return 4;
                }
                return 0;
            }
        }

        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            IntVec3 position = base.Position;

            base.Impact(hitThing);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, this.equipmentDef, this.def, this.targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing == null) return;

            DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, (float)base.DamageAmount * DamageMultiplier, base.ArmorPenetration, this.ExactRotation.eulerAngles.y, this.launcher, null, this.equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
            hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);

            Pawn pawn = hitThing as Pawn;
            if (pawn != null && pawn.stances != null && pawn.BodySize <= this.def.projectile.StoppingPower + 0.001f)
            {
                if(pawn.RaceProps.IsMechanoid)
                    pawn.stances.stunner.StunFor_NewTmp(50, this.Launcher, false, true);
                pawn.stances.StaggerFor(95);
            }

            //Arc To Other Things
            if (ArcType == TeslaArcType.Spark) return;
            float arcRadius = 0;
            if (hitThing is Pawn || (hitThing.Stuff != null && hitThing.Stuff.IsMetal))
            {
                var cells = GenRadial.RadialCellsAround(hitThing.Position, 7, false);
                var options = from x in cells
                    where x.GetThingList(hitThing.Map).Any(t => t is Pawn || t.IsMetallic())
                    select x.GetFirstThing<Thing>(hitThing.Map);

                //var pawns = cells.Select(p => p.GetFirstPawn(hitThing.Map));
                //var things = cells.Select(p => p.GetFirstThing<Thing>(hitThing.Map)).Where(t => t.IsMetallic());
                foreach (var thing in options)
                {
                    if (thing == hitThing) continue;
                    if(thing == null) continue;
                    Projectile_TeslaArc newArc = (Projectile_TeslaArc)GenSpawn.Spawn(this.def, hitThing.Position, hitThing.Map);
                    newArc.ArcType = ArcType + 1;
                    var equipment = (launcher as Pawn).equipment.AllEquipmentListForReading.Find(t => t.def == equipmentDef);
                    newArc.Launch(this.launcher, thing, thing, ProjectileHitFlags.All, equipment);
                }
            }
        }

        public override void Draw()
        {
            //base.Draw();
            DrawArc(origin.ToIntVec3(), destination.ToIntVec3());
        }

        private void DrawArc(IntVec3 from, IntVec3 to)
        {
            var start = from.ToVector3Shifted();
            var end = to.ToVector3Shifted();
            Vector3 diff = start - end;
            float alpha = Mathf.InverseLerp(end.magnitude, start.magnitude, ExactPosition.magnitude); //(ExactPosition - end).magnitude;
            Color color = Color.white;
            color.a *= alpha;
            if (color != RandomMaterial.color)
            {
                RandomMaterial = MaterialPool.MatFrom((Texture2D)RandomMaterial.mainTexture, ShaderDatabase.MoteGlow, color);
            }
            float z = (diff).MagnitudeHorizontal();
            float x = (diff).MagnitudeHorizontal();
            Vector3 pos = (start + end) / 2f;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Vector3 scale = new Vector3(z/2f, 1f, z);
            Quaternion quat = Quaternion.LookRotation(diff);
            Matrix4x4 matrix = default;
            matrix.SetTRS(pos, quat, scale);
            Graphics.DrawMesh(MeshPool.plane10, matrix, RandomMaterial, 0);
        }
    }
}

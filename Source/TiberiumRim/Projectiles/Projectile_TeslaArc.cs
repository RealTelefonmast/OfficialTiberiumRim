using System.Linq;
using TR.TextureContent;
using UnityEngine;
using Verse;

namespace TR.Projectiles;

public enum TeslaArcType
{
    Arc = 1,
    Jump = 2,
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

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var map = Map;
        var position = Position;

        base.Impact(hitThing);
        var battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher,
            hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
        Find.BattleLog.Add(battleLogEntry_RangedImpact);
        if (hitThing == null) return;

        var dinfo = new DamageInfo(def.projectile.damageDef, base.DamageAmount * DamageMultiplier,
            base.ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef,
            DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
        hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);

        var pawn = hitThing as Pawn;
        if (pawn != null && pawn.stances != null && pawn.BodySize <= def.projectile.StoppingPower + 0.001f)
        {
            if (pawn.RaceProps.IsMechanoid)
                pawn.stances.stunner.StunFor(50, Launcher, false);
            pawn.stances.StaggerFor(95);
        }

        //Arc To Other Things
        if (ArcType == TeslaArcType.Spark) return;
        if (hitThing is Pawn || (hitThing.Stuff != null && hitThing.Stuff.IsMetal))
        {
            var cells = GenRadial.RadialCellsAround(hitThing.Position, 7, false);
            var options =
                from x in cells
                where x.GetThingList(hitThing.Map).Any(t => t is Pawn || t.IsMetallic())
                select x.GetFirstThing<Thing>(hitThing.Map);

            //var pawns = cells.Select(p => p.GetFirstPawn(hitThing.Map));
            //var things = cells.Select(p => p.GetFirstThing<Thing>(hitThing.Map)).Where(t => t.IsMetallic());
            foreach (var thing in options)
            {
                if (thing == hitThing) continue;
                if (thing == null) continue;
                var newArc =
                    (Projectile_TeslaArc)GenSpawn.Spawn(def, hitThing.Position, hitThing.Map);
                newArc.ArcType = ArcType + 1;
                var equipment =
                    (launcher as Pawn).equipment.AllEquipmentListForReading.Find(t => t.def == equipmentDef);
                newArc.Launch(launcher, thing, thing, ProjectileHitFlags.All, false, equipment);
            }
        }
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        //base.Draw();
        DrawArc(origin.ToIntVec3(), destination.ToIntVec3());
    }

    private void DrawArc(IntVec3 from, IntVec3 to)
    {
        var start = from.ToVector3Shifted();
        var end = to.ToVector3Shifted();
        var diff = start - end;
        var alpha =
            Mathf.InverseLerp(end.magnitude, start.magnitude,
                ExactPosition.magnitude); //(ExactPosition - end).magnitude;
        var color = Color.white;
        color.a *= alpha;
        if (color != RandomMaterial.color)
            RandomMaterial =
                MaterialPool.MatFrom((Texture2D)RandomMaterial.mainTexture, ShaderDatabase.MoteGlow, color);

        var z = diff.MagnitudeHorizontal();
        var x = diff.MagnitudeHorizontal();
        var pos = (start + end) / 2f;
        pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        var scale = new Vector3(z / 2f, 1f, z);
        var quat = Quaternion.LookRotation(diff);
        Matrix4x4 matrix = default;
        matrix.SetTRS(pos, quat, scale);
        Graphics.DrawMesh(MeshPool.plane10, matrix, RandomMaterial, 0);
    }
}
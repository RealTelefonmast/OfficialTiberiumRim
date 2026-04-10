using RimWorld;
using UnityEngine;
using Verse;

namespace TR.SuperWeapon;

[StaticConstructorOnStartup]
public class IonBeam : ThingWithComps
{
    private static readonly MaterialPropertyBlock MatPropertyBlock = new();

    private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow,
        MapMaterialRenderQueues.OrbitalBeam);

    private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow,
        MapMaterialRenderQueues.OrbitalBeam);

    public bool continuousBurn = true;
    public int durationTicks;
    private Vector3 lastRealPos = Vector3.zero;
    public Vector3 realPos;
    private int startTick;
    public float width = 1.5f;

    public override Vector3 DrawPos => realPos;

    private int TicksPassed => Find.TickManager.TicksGame - startTick;

    public IntVec3 CurrentPosition => realPos.ToIntVec3();

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        startTick = Find.TickManager.TicksGame;
    }

    public override void Tick()
    {
        base.Tick();
        if (TicksPassed >= durationTicks)
        {
            Destroy();
            return;
        }

        if (continuousBurn || TicksPassed <= 2)
        {
            TryDamageOrBurn(CurrentPosition);
            BeamBurn(CurrentPosition, Map);
        }

        lastRealPos = realPos;
    }

    public void BeamBurn(IntVec3 cell, Map map)
    {
        //Main Beam Burn
        var mote = (Mote)ThingMaker.MakeThing(ThingDef.Named("IonBeamBurn"));
        mote.exactPosition = realPos;
        mote.Scale = 3 * width;
        mote.rotationRate = 1.2f;
        mote.instanceColor = new ColorInt(70, 90, 175).ToColor;
        GenSpawn.Spawn(mote, cell, map);
    }

    private void TryDamageOrBurn(IntVec3 cell)
    {
        float damage = TRUtils.Range(1, 15);
        var dInfo = new DamageInfo(DamageDefOf.Burn, damage, 5, 0, this);
        var list = cell.GetThingList(Map);
        for (var i = 0; i < list.Count; i++)
        {
            var thing = list[i];
            thing.TakeDamage(dInfo);
        }

        if (FireUtility.TryStartFireIn(cell, Map, TRUtils.Range(0.1f, 0.5f)))
        {
            var moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Smoke);
            moteThrown.Scale = TRUtils.Range(3f, 5.5f);
            moteThrown.rotationRate = TRUtils.Range(-30f, 30f);
            moteThrown.exactPosition = realPos;
            moteThrown.instanceColor = new ColorInt(50, 50, 50).ToColor;
            moteThrown.SetVelocity(TRUtils.Range(25, 75), TRUtils.Range(0.7f, 2.8f));
            GenSpawn.Spawn(moteThrown, cell, Map);
        }
        //if (TRUtils.Chance(0.3f))
        //GenSpawn.Spawn(ThingDef.Named("IonizedAir"), cell, Map);
    }

    public override void Draw()
    {
        var beamHeight = (Map.Size.z - DrawPos.z) * 1.41421354f;

        var angle = Vector3Utility.FromAngleFlat(-90f);
        var angle2 = DrawPos + angle * beamHeight * 0.5f;
        angle2.y = AltitudeLayer.MetaOverlays.AltitudeFor();

        var initialPct = Mathf.Min(TicksPassed / 10f, 1f);
        var initalHeight = angle * ((1f - initialPct) * beamHeight);

        var opacity = 0.975f + Mathf.Sin(TicksPassed * 0.3f) * 0.025f;
        var color = new ColorInt(70, 90, 175).ToColor;
        color.a *= opacity * 2;

        MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
        var matrix = default(Matrix4x4);
        matrix.SetTRS(angle2 + angle * (width / 2f) * 0.5f + initalHeight, Quaternion.Euler(0f, 0, 0f),
            new Vector3(width, 1f, beamHeight));
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0, null, 0, MatPropertyBlock);

        var pos = DrawPos + initalHeight;
        pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
        var matrix2 = default(Matrix4x4);
        matrix2.SetTRS(pos, Quaternion.Euler(0f, 0, 0f), new Vector3(width, 1f, width * 0.5f));
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix2, BeamEndMat, 0, null, 0, MatPropertyBlock);
    }
}
using RimWorld;
using TR.Defs;
using TR.Util;
using UnityEngine;
using Verse;

namespace TR.Data.SuperWeapons.IonCannon;

[StaticConstructorOnStartup]
public class IonBeam : ThingWithComps
{
    private int startTick;
    public int durationTicks;
    public float width = 1.5f;
    public bool continuousBurn = true;
    public Vector3 realPos;
    private Vector3 lastRealPos = Vector3.zero;

    private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();
    private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
    private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);

    public override Vector3 DrawPos => realPos;

    private int TicksPassed => Find.TickManager.TicksGame - this.startTick;

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
        FleckCreationData dataStatic = FleckMaker.GetDataStatic(realPos, map, TRFDefOf.IonBeamBurn, 3 * width);
        dataStatic.instanceColor = new ColorInt(70, 90, 175).ToColor;
        dataStatic.rotationRate = 1.2f;
        map.flecks.CreateFleck(dataStatic);
    }

    private void TryDamageOrBurn(IntVec3 cell)
    {
        float damage = TRandom.Range(1, 15);
        DamageInfo dInfo = new DamageInfo(DamageDefOf.Burn, damage, 5, 0, this);
        var list = cell.GetThingList(Map);
        for (var i = 0; i < list.Count; i++)
        {
            var thing = list[i];
            thing.TakeDamage(dInfo);
        }
        if (FireUtility.TryStartFireIn(cell, Map, TRandom.Range(0.1f, 0.5f)))
        {
            FleckMaker.ThrowSmoke(realPos, Map, 3);
            /*
            FleckThrown moteThrown = (FleckThrown)FleckMaker.ThrowSmoke(realPos, Map, 3);
            moteThrown.Scale = TRandom.Range(3f, 5.5f);
            moteThrown.rotationRate = TRandom.Range(-30f, 30f);
            moteThrown.exactPosition = realPos;
            moteThrown.instanceColor = new ColorInt(50, 50, 50).ToColor;
            moteThrown.SetVelocity((float)TRandom.Range(25, 75), TRandom.Range(0.7f, 2.8f));
            GenSpawn.Spawn(moteThrown, cell, Map, WipeMode.Vanish);
            */
        }
        //if (TRandom.Chance(0.3f))
        //GenSpawn.Spawn(ThingDef.Named("IonizedAir"), cell, Map);
    }

    public IntVec3 CurrentPosition => realPos.ToIntVec3();

    public override void Draw()
    {
        float beamHeight = ((float)Map.Size.z - DrawPos.z) * 1.41421354f;

        Vector3 angle = Vector3Utility.FromAngleFlat(-90f);
        Vector3 angle2 = DrawPos + angle * beamHeight * 0.5f;
        angle2.y = AltitudeLayer.MetaOverlays.AltitudeFor();

        float initialPct = Mathf.Min((float)this.TicksPassed / 10f, 1f);
        Vector3 initalHeight = angle * ((1f - initialPct) * beamHeight);

        float opacity = 0.975f + Mathf.Sin((float)this.TicksPassed * 0.3f) * 0.025f;
        Color color = new ColorInt(70, 90, 175).ToColor;
        color.a *= (opacity *2);

        MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
        Matrix4x4 matrix = default(Matrix4x4);
        matrix.SetTRS(angle2 + angle * (width / 2f) * 0.5f + initalHeight, Quaternion.Euler(0f, 0, 0f), new Vector3(width, 1f, beamHeight));
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0, null, 0, MatPropertyBlock);

        Vector3 pos = DrawPos + initalHeight;
        pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
        Matrix4x4 matrix2 = default(Matrix4x4);
        matrix2.SetTRS(pos, Quaternion.Euler(0f, 0, 0f), new Vector3(this.width, 1f, width*0.5f));
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix2, BeamEndMat, 0, null, 0, MatPropertyBlock);
    }
}
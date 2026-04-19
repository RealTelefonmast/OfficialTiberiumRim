using System;
using System.Collections.Generic;
using LudeonTK;
using TiberiumRim;
using UnityEngine;
using Verse;

namespace TR.SuperWeapon;

public class Building_Temple : TRBuilding, IRocketSilo
{
    [TweakValue("NodNukeOffY", 0f, 1f)] public static float NodNukeOffY = 0;

    [TweakValue("NodNukePosZ", 0f, 15f)] public static float NodNukePosZ = 0f;

    private readonly bool[] bools = new bool[3] { true, true, true };
    private readonly int maxTick = 1000;
    private readonly float nukeIdle = 0.25f;
    private readonly float nukeOffset = 0.55f;
    private readonly int rotation = 45;
    private int tick = 1000;

    public override Vector3[] DrawPositions => new[]
        { base.DrawPos, base.DrawPos, base.DrawPos, base.DrawPos + new Vector3(0, 0, -0.25f) };

    public override Color[] ColorOverrides => new[] { Color.white, Color.white };
    public override float[] OpacityFloats => new[] { 1f, 1f };
    public override float?[] RotationOverrides => new float?[] { null, null };
    public override bool[] DrawBools => new[] { bools[0], bools[1], bools[2], true };

    public override Action<FXGraphic>[] Actions => new[]
    {
        null, null, null, delegate(FXGraphic graphic)
        {
            var blades = (Graphic_NumberedCollection)graphic.Graphic;
            for (var i = 0; i < blades.Count; i++)
            {
                var g = blades.Graphics[i];
                var mesh = g.MeshAt(Rotation);
                var drawPos = DrawPositions[3];
                drawPos += Quaternion.Euler(0, i * (180f / blades.Count), 0) * new Vector3(-2, 0, 0) * CurPct;
                drawPos.y = AltitudeLayer.Building.AltitudeFor();
                Graphics.DrawMesh(mesh, drawPos, CurRot.ToQuat(), g.MatSingle, 0);
            }
        },
        delegate(FXGraphic graphic)
        {
            var nukeMat = graphic.Graphic.MatSingle;
            nukeMat.SetTextureOffset("_MainTex", new Vector2(0.25f, NukeOffset + NodNukeOffY));
            nukeMat.SetTextureScale("_MainTex", new Vector2(0.5f, 0.5f));
            var matrix4x = default(Matrix4x4);
            var pos = new Vector3(DrawPos.x, graphic.altitude, DrawPos.z + 2.55f);
            pos.z += NodNukePosZ;
            matrix4x.SetTRS(pos, Quaternion.Euler(Vector3.up), new Vector3(2f, 1f, 6f));
            Graphics.DrawMesh(MeshPool.plane10, matrix4x, nukeMat, 0);
        }
    };

    public override bool ShouldDoEffecters => true;

    public float NukeOffset => Mathf.Lerp(nukeOffset, nukeIdle, CurPct);

    public float CurPct => (float)tick / maxTick;

    public float CurRot => rotation * CurPct;

    public Vector3 RocketBaseOffset { get; }
    public AltitudeLayer Altitude { get; }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
    }

    public override void Tick()
    {
        base.Tick();
        if (tick < maxTick)
            tick++;
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;
        yield return new Command_Action
        {
            defaultLabel = "origin animation",
            action = delegate { tick = 0; }
        };

        yield return new Command_Action
        {
            defaultLabel = "remove overlays",
            action = delegate
            {
                bools[0] = !bools[0];
                bools[1] = !bools[1];
                bools[2] = !bools[2];
            }
        };
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Rendering;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;

namespace TeleCore.Comps;

public enum FXMode
{
    Static,
    Dynamic,
    Mover,
    Blink,
    Pulse,
    Rotate
}

public class CompFX : ThingComp
{
    //Data for graphics
    public Color[] GraphicColors;
    public List<FXGraphic> Graphics = new();
    private IFXObject iParent;
    private MoteThrower MainThrower;
    private List<Vector3> motePositions = new();
    public List<MoteThrower> moteThrowers = new();
    private int moteTicker = -1;
    private bool spawnedOnce;
    public int startTick;

    public int tickOffset;

    private List<Vector3> MoteOrigins
    {
        get
        {
            if (motePositions.NullOrEmpty() && Props.moteData != null)
            {
                var positions = new List<Vector3>();
                var center = parent.TrueCenter();
                Vector3 newVec;
                var rotation = parent.Rotation;
                if (rotation == Rot4.North || (rotation == Rot4.South && Props.moteData.southVec.NullOrEmpty()))
                    for (var i = 0; i < Props.moteData.northVec.Count; i++)
                    {
                        var v2 = center + Props.moteData.northVec[i];
                        newVec = new Vector3(v2.x, AltitudeLayer.MoteOverhead.AltitudeFor(), v2.z);
                        positions.Add(newVec);
                    }

                if (rotation == Rot4.East || (rotation == Rot4.West && Props.moteData.westVec.NullOrEmpty()))
                    for (var i = 0; i < Props.moteData.eastVec.Count; i++)
                    {
                        var v2 = center + Props.moteData.eastVec[i];
                        newVec = new Vector3(v2.x, AltitudeLayer.MoteOverhead.AltitudeFor(), v2.z);
                        positions.Add(newVec);
                    }

                if (rotation == Rot4.South)
                    for (var i = 0; i < Props.moteData.southVec.Count; i++)
                    {
                        var v2 = center + Props.moteData.southVec[i];
                        newVec = new Vector3(v2.x, AltitudeLayer.MoteOverhead.AltitudeFor(), v2.z);
                        positions.Add(newVec);
                    }

                if (rotation == Rot4.West)
                    for (var i = 0; i < Props.moteData.westVec.Count; i++)
                    {
                        var v2 = center + Props.moteData.westVec[i];
                        newVec = new Vector3(v2.x, AltitudeLayer.MoteOverhead.AltitudeFor(), v2.z);
                        positions.Add(newVec);
                    }

                motePositions = positions;
            }

            return motePositions;
        }
    }

    public CompProperties_FX Props => props as CompProperties_FX;

    public CompPowerTrader CompPower => IParent == null ? parent.TryGetComp<CompPowerTrader>() :
        IParent.ForcedPowerComp == null ? parent.TryGetComp<CompPowerTrader>() :
        (CompPowerTrader)IParent.ForcedPowerComp;

    public CompPowerPlant CompPowerPlant => parent.TryGetComp<CompPowerPlant>();

    public IFXObject IParent
    {
        get
        {
            if (iParent != null) return iParent;
            if (!Props.useParentClass && parent.AllComps.Any(c => c is IFXObject))
            {
                iParent = parent.AllComps.First(x => x is IFXObject) as IFXObject;
                return iParent;
            }

            return parent as IFXObject;
        }
    }

    public Vector2 TextureOffset => (bool)IParent?.TextureOffset.HasValue ? IParent.TextureOffset.Value : Vector2.zero;

    public Vector2 TextureScale =>
        (bool)IParent?.TextureScale.HasValue ? IParent.TextureScale.Value : new Vector2(1, 1);

    public bool ShouldDoEffecters => IParent == null || IParent.ShouldDoEffecters;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref startTick, "startTick");
        Scribe_Values.Look(ref tickOffset, "tickOffset");
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        if (spawnedOnce) return;
        if (Props.moteData?.thrower != null) MainThrower = new MoteThrower(Props.moteData.thrower, parent);
        if (!Props.effecters.NullOrEmpty())
            foreach (var info in Props.effecters)
                moteThrowers.Add(new MoteThrower(info, parent));

        if (!Props.overlays.NullOrEmpty())
            for (var i = 0; i < Props.overlays.Count; i++)
                Graphics.Add(new FXGraphic(this, Props.overlays[i], i));

        InitializeData();
        spawnedOnce = true;
        if (!respawningAfterLoad)
        {
            startTick = Find.TickManager.TicksGame;
            tickOffset = TRUtils.Range(Props.tickOffset);
        }
    }

    private void InitializeData()
    {
        GraphicColors = new Color[Graphics.Count];
        for (var i = 0; i < Graphics.Count; i++) GraphicColors[0] = Color.white;
    }

    public override void CompTick()
    {
        Tick();
    }

    public override void CompTickRare()
    {
        for (var i = 0; i < 750; i++) Tick();
    }

    private void Tick()
    {
        TargetInfo A = parent;
        if (ShouldDoEffecters)
        {
            if (Props.moteData != null)
            {
                if (moteTicker <= 0)
                {
                    MoteThrowTick();
                    moteTicker = TRUtils.Range(Props.moteData.tickRange);
                }

                moteTicker--;
            }

            foreach (var t in moteThrowers) t.ThrowerTick(parent.DrawPos, parent.Map);
        }

        foreach (var g in Graphics) g.Tick();
    }

    private void MoteThrowTick()
    {
        foreach (var v in MoteOrigins) MainThrower.ThrowerTick(v, parent.Map);
    }

    public override void ReceiveCompSignal(string signal)
    {
        if (!parent.Spawned) return;
        if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff" || signal == "FlickedOn" ||
            signal == "FlickedOff" || signal == "Refueled" || signal == "RanOutOfFuel" || signal == "ScheduledOn" ||
            signal == "ScheduledOff") parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
    }

    private bool CanDraw(int index)
    {
        if (!DrawBool(index) || OpacityFloat(index) <= 0)
            return false;
        if (Graphics[index].data.skip)
            return false;
        if (!HasPower(index))
            return false;
        return true;
    }

    private bool HasPower(int index)
    {
        if (Graphics[index].data.needsPower)
        {
            if (CompPowerPlant != null)
                return CompPowerPlant.PowerOutput > 0;
            if (CompPower != null)
                return CompPower.PowerOn;
        }

        return true;
    }

    public bool DrawBool(int index)
    {
        if (IParent == null || IParent.DrawBools.Count() < index + 1) return true;
        return IParent.DrawBools[index];
    }

    public float OpacityFloat(int index)
    {
        if (IParent == null || IParent.OpacityFloats.Count() < index + 1) return 1f;
        return IParent.OpacityFloats[index];
    }

    public float? RotationOverride(int index)
    {
        if (IParent == null || IParent.RotationOverrides.Count() < index + 1) return null;
        return IParent.RotationOverrides[index];
    }

    public ref Color ColorOverride(int index)
    {
        if (IParent == null || IParent.ColorOverrides.Count() < index + 1)
            GraphicColors[index] = Color.white;
        else
            GraphicColors[index] = IParent.ColorOverrides[index];
        return ref GraphicColors[index];
    }

    public Vector3 DrawPosition(int index)
    {
        if (IParent == null || IParent.DrawPositions.Count() < index + 1) return parent.DrawPos;
        return IParent.DrawPositions[index];
    }

    public Action<FXGraphic> Action(int index)
    {
        if (IParent?.Actions == null || IParent.Actions.Count() < index + 1) return null;
        return IParent.Actions[index];
    }

    //TODO: Replace motes finally - Add advanced way of rendering effects instead of using "Parent Motes" which need to be spawned
    public override void PostDraw()
    {
        base.PostDraw();
        for (var i = 0; i < Graphics.Count; i++)
            if (Graphics[i].data.mode != FXMode.Static && CanDraw(i))
                Graphics[i].Draw(DrawPosition(i), parent.Rotation, RotationOverride(i), Action(i), i);
    }

    public override void PostPrintOnto(SectionLayer layer)
    {
        base.PostPrintOnto(layer);
        for (var i = 0; i < Graphics.Count; i++)
            if (Graphics[i].data.mode == FXMode.Static && CanDraw(i))
                Graphics[i].Print(layer, DrawPosition(i), parent.Rotation, RotationOverride(i), parent);
    }
}

public class CompProperties_FX : CompProperties
{
    public List<MoteThrowerInfo> effecters;
    public FXMode mode = FXMode.Static;
    public MoteThrowerData moteData;
    public List<FXGraphicData> overlays = new();
    public IntRange tickOffset = new(0, 333);
    public bool useParentClass = false;

    public CompProperties_FX()
    {
        compClass = typeof(CompFX);
    }
}

public enum MoteMakerType
{
    TiberiumSmoke,
    TiberiumFog
}

public class MoteThrowerData
{
    public List<Vector3> eastVec;
    public List<Vector3> northVec;
    public List<Vector3> southVec;
    public MoteThrowerInfo thrower;
    public IntRange tickRange = new(1, 1);
    public List<Vector3> westVec;
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
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
        private IFXObject iParent;
        public List<MoteThrower> moteThrowers = new List<MoteThrower>();
        public List<FXGraphic> Graphics = new List<FXGraphic>();
        private List<Vector3> motePositions = new List<Vector3>();
        private MoteThrower MainThrower;

        public int tickOffset = 0;
        public int startTick = 0;
        private int moteTicker = -1;
        private bool spawnedOnce = false;

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
            if(Props.moteData?.thrower != null)
            {
                MainThrower = new MoteThrower(Props.moteData.thrower, parent);
            }
            if (!Props.effecters.NullOrEmpty())
            {
                foreach (MoteThrowerInfo info in Props.effecters)
                {
                    moteThrowers.Add(new MoteThrower(info, parent));
                }
            }
            if (!Props.overlays.NullOrEmpty())
            {
                for (int i = 0; i < Props.overlays.Count; i++)
                {
                    Graphics.Add(new FXGraphic(this, Props.overlays[i], i));
                }
            }
            spawnedOnce = true;
            if (!respawningAfterLoad)
            {
                startTick = Find.TickManager.TicksGame;
                tickOffset = TRUtils.Range(Props.tickOffset);
            }
        }

        private List<Vector3> MotePositions
        {
            get
            {              
                if (motePositions.NullOrEmpty() && Props.moteData != null)
                {
                    List<Vector3> positions = new List<Vector3>();
                    Vector3 center = parent.TrueCenter();
                    Vector3 newVec;
                    Rot4 rotation = parent.Rotation;
                    if(rotation == Rot4.North || (rotation == Rot4.South && Props.moteData.southVec.NullOrEmpty()))
                    {
                        for (int i = 0; i < Props.moteData.northVec.Count; i++)
                        {
                            Vector3 v2 = center + Props.moteData.northVec[i];
                            newVec = new Vector3(v2.x, AltitudeLayer.MoteOverhead.AltitudeFor(), v2.z);
                            positions.Add(newVec);
                        }
                    }
                    if(rotation == Rot4.East || (rotation == Rot4.West && Props.moteData.westVec.NullOrEmpty()))
                    {
                        for (int i = 0; i < Props.moteData.eastVec.Count; i++)
                        {
                            Vector3 v2 = center + Props.moteData.eastVec[i];
                            newVec = new Vector3(v2.x, AltitudeLayer.MoteOverhead.AltitudeFor(), v2.z);
                            positions.Add(newVec);
                        }
                    }
                    if(rotation == Rot4.South)
                    {
                        for (int i = 0; i < Props.moteData.southVec.Count; i++)
                        {
                            Vector3 v2 = center + Props.moteData.southVec[i];
                            newVec = new Vector3(v2.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z);
                            positions.Add(newVec);
                        }
                    }
                    if (rotation == Rot4.West)
                    {
                        for (int i = 0; i < Props.moteData.westVec.Count; i++)
                        {
                            Vector3 v2 = center + Props.moteData.westVec[i];
                            newVec = new Vector3(v2.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z);
                            positions.Add(newVec);
                        }
                    }                  
                    motePositions = positions;
                }
                return motePositions;
            }
        }

        public CompProperties_FX Props => base.props as CompProperties_FX;

        public CompPowerTrader CompPower => IParent == null ? parent.TryGetComp<CompPowerTrader>() : (IParent.ForcedPowerComp == null ? parent.TryGetComp<CompPowerTrader>() : (CompPowerTrader)IParent.ForcedPowerComp );
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

        public override void CompTick()
        {
            Tick();
        }

        public override void CompTickRare()
        {
            for (int i = 0; i < 750; i++)
            {
                Tick();
            }
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
                foreach (var t in moteThrowers)
                {
                    t.ThrowerTick(parent.DrawPos, parent.Map);
                }
            }
            foreach (var g in Graphics)
            {
                g.Tick();
            }
        }

        private void MoteThrowTick()
        {
            foreach(Vector3 v in MotePositions)
            {
                MainThrower.ThrowerTick(v, parent.Map);
            }
            /*
            switch (Props.moteData.moteType)
            {
                case MoteMakerType.TiberiumSmoke:
                    foreach (Vector3 v in MotePositions)
                    {
                        TRUtils.ThrowTiberiumSmoke(v, parent);
                    }
                    break;
                case MoteMakerType.TiberiumFog:
                    foreach (Vector3 v in MotePositions)
                    {
                        TRUtils.ThrowTiberiumContainerFog(v, parent);
                    }
                    break;
            }
            */
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (!parent.Spawned) return;
            if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff" || signal == "FlickedOn" || signal == "FlickedOff" || signal == "Refueled" || signal == "RanOutOfFuel" || signal == "ScheduledOn" || signal == "ScheduledOff")
            {
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
            }
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
                else
                if (CompPower != null)
                    return CompPower.PowerOn;
            }
            return true;
        }

        public bool DrawBool(int index)
        {
            if(IParent == null || IParent.DrawBools.Count() < (index + 1))
            {
                return true;
            }
            return IParent.DrawBools[index];
        }

        public float OpacityFloat(int index)
        {
            if (IParent == null || IParent.OpacityFloats.Count() < (index + 1))
            {
                return 1f;
            }
            return IParent.OpacityFloats[index];
        }

        public float? RotationOverride(int index)
        {
            if (IParent == null || IParent.RotationOverrides.Count() < (index + 1))
            {
                return null;
            }
            return IParent.RotationOverrides[index];
        }

        public Color ColorOverride(int index)
        {
            if (IParent == null || IParent.ColorOverrides.Count() < (index + 1))
            {
                return Color.white;
            }
            return IParent.ColorOverrides[index];
        }

        public Vector3 DrawPosition(int index)
        {
            if (IParent == null || IParent.DrawPositions.Count() < (index + 1))
            {
                return parent.DrawPos;
            }
            return IParent.DrawPositions[index];
        }

        public Action<FXGraphic> Action(int index)
        {
            if(IParent?.Actions == null || IParent.Actions.Count() < (index + 1))
            {
                return null;
            }
            return IParent.Actions[index];
        }

        public Vector2 TextureOffset => (bool)IParent?.TextureOffset.HasValue ? IParent.TextureOffset.Value : Vector2.zero;

        public  Vector2 TextureScale => (bool)IParent?.TextureScale.HasValue ? IParent.TextureScale.Value : new Vector2(1, 1);

        public bool ShouldDoEffecters => IParent == null || IParent.ShouldDoEffecters;

        //TODO: Replace motes finally - Add advanced way of rendering effects instead of using "Parent Motes" which need to be spawned
        public override void PostDraw()
        {
            base.PostDraw();
            for (int i = 0; i < Graphics.Count; i++)
            {
                if (Graphics[i].data.mode != FXMode.Static && CanDraw(i))
                {
                    Graphics[i].Draw(DrawPosition(i), parent.Rotation, RotationOverride(i), Action(i), i);
                }
            }
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            base.PostPrintOnto(layer);
            for (int i = 0; i < Graphics.Count; i++)
            {
                if (Graphics[i].data.mode == FXMode.Static && CanDraw(i))
                {
                    Graphics[i].Print(layer, DrawPosition(i), parent.Rotation, RotationOverride(i), parent);
                }
            }
        }
    }

    public class CompProperties_FX : CompProperties
    {
        public CompProperties_FX()
        {
            this.compClass = typeof(CompFX);
        }
        public bool useParentClass = false;
        public IntRange tickOffset = new IntRange(0, 333);
        public FXMode mode = FXMode.Static;
        public MoteThrowerData moteData;
        public List<MoteThrowerInfo> effecters;
        public List<FXGraphicData> overlays = new List<FXGraphicData>();
    }

    public enum MoteMakerType
    {
        TiberiumSmoke,
        TiberiumFog
    }

    public class MoteThrowerData
    {
        public List<Vector3> northVec;
        public List<Vector3> eastVec;
        public List<Vector3> westVec;
        public List<Vector3> southVec;
        public IntRange tickRange = new IntRange(1,1);
        public MoteThrowerInfo thrower;
    }
}

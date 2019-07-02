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
            if (!spawnedOnce)
            {
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
                            newVec = new Vector3(v2.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z);
                            positions.Add(newVec);
                        }
                    }
                    if(rotation == Rot4.East || (rotation == Rot4.West && Props.moteData.westVec.NullOrEmpty()))
                    {
                        for (int i = 0; i < Props.moteData.eastVec.Count; i++)
                        {
                            Vector3 v2 = center + Props.moteData.eastVec[i];
                            newVec = new Vector3(v2.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z);
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

        public CompProperties_FX Props
        {
            get
            {
                return base.props as CompProperties_FX;
            }
        }

        public CompPowerTrader CompPower
        {
            get
            {
                return parent.TryGetComp<CompPowerTrader>();
            }
        }

        public IFXObject IParent
        {
            get
            {
                if (iParent == null)
                {
                    if (parent is IFXObject fxObject)
                    {
                        iParent = fxObject;
                        return iParent;
                    }
                    iParent = parent.AllComps.Find(x => x is IFXObject) as IFXObject;
                }
                return iParent;
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
                for (int i = 0; i < moteThrowers.Count; i++)
                {
                    moteThrowers[i].ThrowerTick(parent.DrawPos, parent.Map);
                }
            }
            for (int i = 0; i < Graphics.Count; i++)
            {
                Graphics[i].Tick();
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
            if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff" || signal == "FlickedOn" || signal == "FlickedOff" || signal == "Refueled" || signal == "RanOutOfFuel" || signal == "ScheduledOn" || signal == "ScheduledOff")
            {
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Buildings);
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
            }
        }

        private bool CanDraw(int index)
        {
            if (!DrawBool(index) || OpacityFloat(index) <= 0)
            {
                return false;
            }
            if (Graphics[index].data.needsPower && !(CompPower?.PowerOn ?? false))
            {
                return false;
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

        public float? RotationOverrides(int index)
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

        public bool ShouldDoEffecters
        {
            get
            {
                if (IParent != null)
                {
                    return IParent.ShouldDoEffecters;
                }
                return true;
             }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            for (int i = 0; i < Graphics.Count; i++)
            {
                FXGraphic graphic = Graphics[i];
                if (CanDraw(i) && graphic.data.mode != FXMode.Static)
                {
                    graphic.Draw(DrawPosition(i), parent.Rotation, RotationOverrides(i), i);
                }
            }
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            base.PostPrintOnto(layer);
            for (int i = 0; i < Graphics.Count; i++)
            {
                FXGraphic graphic = Graphics[i];
                if (CanDraw(i) && graphic.data.mode == FXMode.Static)
                {
                    graphic.Print(layer, DrawPosition(i), parent.Rotation, RotationOverrides(i), parent);
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

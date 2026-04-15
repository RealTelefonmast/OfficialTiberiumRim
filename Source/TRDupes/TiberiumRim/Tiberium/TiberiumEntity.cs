using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumEntity : Entity, IExposable, ISelectable, ILoadReferenceable
    {
        public TiberiumDef def;
        public int ID = -1;
        private IntVec3 position;

        public string GetUniqueLoadID()
        {
            return def.defName + ID;
        }

        public virtual void ExposeData()
        {

        }

        public override void SpawnSetup(Map map, bool respawnAfterload)
        {
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            
        }

        public override void Tick()
        {

        }

        public override void TickRare() => Tick();
        public override void TickLong() => Tick();

        public override string Label => "";
        public override string LabelCap => "";
        public IntVec3 Position { get => position; set => position = value; }

        public Vector3 TrueCenter
        {
            get
            {
                return Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Pawn);
            }
        }

        public Graphic Graphic
        {
            get
            {
                return GraphicDatabase.Get(typeof(Graphic_Random), "Tiberium/Green", ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white);
            }
        }

        public virtual string GetInspectString()
        {
            return "";
        }

        public virtual IEnumerable<InspectTabBase> GetInspectTabs()
        {
            return null;
        }

        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            return null;
        }

        public void Print(SectionLayer layer)
        {
            Log.Message("Printing " + ID);
            Log.Message("Graphic " + Graphic.path);
            Printer_Plane.PrintPlane(layer, TrueCenter, Vector2.one, Graphic.MatSingle);
        }
    }
}

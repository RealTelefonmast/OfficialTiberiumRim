using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class TiberiumEntity : Entity, IExposable, ISelectable, ILoadReferenceable
{
    public TiberiumDef def;
    public int ID = -1;

    public override string Label => "";
    public override string LabelCap => "";
    public IntVec3 Position { get; set; }

    public Vector3 TrueCenter => Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Pawn);

    public Graphic Graphic => GraphicDatabase.Get(typeof(Graphic_Random), "Tiberium/Green", ShaderDatabase.Cutout,
        Vector2.one, Color.white, Color.white);

    public virtual void ExposeData()
    {
    }

    public string GetUniqueLoadID()
    {
        return def.defName + ID;
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

    public override void SpawnSetup(Map map, bool respawnAfterload)
    {
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
    }

    public override void Tick()
    {
    }

    public override void TickRare()
    {
        Tick();
    }

    public override void TickLong()
    {
        Tick();
    }

    public void Print(SectionLayer layer)
    {
        Log.Message("Printing " + ID);
        Log.Message("Graphic " + Graphic.path);
        Printer_Plane.PrintPlane(layer, TrueCenter, Vector2.one, Graphic.MatSingle);
    }
}
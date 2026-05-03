using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiberiumRim;
using TR.BeamHub;
using UnityEngine;
using Verse;

namespace TR;

public class BeamSegment
{
    private readonly Map map;

    public List<IntVec3> Cells = new();

    private TRThingDef parentDef;
    public Building_BeamHub[] parents;
    public List<Building_BeamSegmentPart> segments = new();

    public BeamSegment(Building_BeamHub[] parents, List<IntVec3> cells)
    {
        this.parents = parents;
        Cells = cells;
        map = parents[0].Map;
        parentDef = parents[0].def;
        MakeSegments();
    }

    public bool IsActive { get; private set; }

    public bool IsPowered => IsValid && parents[0].IsPowered && parents[1].IsPowered;
    public bool IsValid => parents[0].Spawned && parents[1].Spawned;
    public bool IsGate => false;

    public Building_BeamHub OppositeHubFor(Building_BeamHub hub)
    {
        return parents[0] == hub ? parents[1] : parents[0];
    }

    public void MakeSegments()
    {
        foreach (var cell in Cells)
        {
            Building_BeamSegmentPart s = null;
            if (cell.GetFirstThing(map, parentDef.beamHub.segmentDef) is Building_BeamSegmentPart part)
            {
                s = part;
                s.AddSegment(this);
            }
            else
            {
                s = (Building_BeamSegmentPart)GenSpawn.Spawn(parentDef.beamHub.segmentDef, cell, map);
                s.Setup(this, parentDef);
            }

            segments.Add(s);
        }
    }

    public void Destroy()
    {
        for (var i = 0; i < parents.Length; i++)
        {
            var hub = parents[i];
            hub.RemoveConnection(this);
        }

        foreach (var segment in segments)
        {
            if (!segment.Spawned) continue;
            segment.Remove(this);
        }
    }

    public void Toggle(bool on)
    {
        IsActive = on;
        if (IsActive)
            Activate();
        else
            Deactivate();
    }

    private void Activate()
    {
        if (!IsPowered) return;
        IsActive = true;
        segments.ForEach(s => s.Activate());
    }

    private void Deactivate()
    {
        IsActive = false;
        segments.ForEach(s => s.Deactivate());
    }
}

public class Building_BeamSegmentPart : Building
{
    private readonly BeamSegment[] segments = new BeamSegment[4];
    private bool active;
    private TRThingDef parentDef;

    public bool AnyParentActive => segments.Any(s => s != null && s.IsActive);
    public bool AnySegmentValid => segments.Any(s => s != null && s.IsValid);

    public void Setup(BeamSegment parent, TRThingDef parentDef)
    {
        AddSegment(parent);
        this.parentDef = parentDef;
    }

    public void Remove(BeamSegment parent)
    {
        if (AnySegmentValid)
        {
            UpdateSegments();
        }
        else
        {
            Deactivate(true);
            DeSpawn();
        }
    }

    public void UpdateSegments()
    {
        segments.InsertionSort((s1, s2) => s2?.IsValid.CompareTo(s1?.IsValid) ?? 0);
        for (var i = segments.Length - 1; i > 0; i--)
        {
            var segment = segments[i];
            if (segment != null && !segment.IsValid) segments[i] = null;
        }
    }

    public void AddSegment(BeamSegment newParent)
    {
        for (var i = 0; i < segments.Length; i++)
            if (segments[i] == null)
            {
                segments[i] = newParent;
                return;
            }
    }

    public void Activate()
    {
        if (AnyParentActive && !active && Spawned)
        {
            active = true;
            GenSpawn.Spawn(parentDef.beamHub.beamDef, Position, Map);
        }
    }

    public void Deactivate(bool force = false)
    {
        if ((active && force) || (!AnyParentActive && active && Spawned))
        {
            active = false;
            Position.GetFirstThing(Map, parentDef.beamHub.beamDef).DeSpawn();
        }
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.AppendFormat(base.GetInspectString());
        sb.AppendLine("\nActive: " + active);
        sb.AppendLine("Beam Segments: " + segments.Count(s => s != null));
        sb.AppendLine("AnyParentActive: " + AnyParentActive);
        sb.AppendLine("AnySegmentValid: " + AnySegmentValid);
        sb.AppendLine("Segments:\n" + (segments[0] != null) + "\n"
                      + (segments[1] != null) + "\n"
                      + (segments[2] != null) + "\n"
                      + (segments[3] != null) + "\n");
        return sb.ToString().TrimEndNewlines();
    }

    public override void Draw()
    {
        base.Draw();
        if (Find.Selector.IsSelected(this))
        {
            var cells = new List<IntVec3>();
            foreach (var segment in segments)
                if (segment != null)
                    foreach (var hub in segment.parents)
                        if (hub != null)
                            cells.Add(hub.Position);

            GenDraw.DrawFieldEdges(cells, Color.magenta);
        }
    }
}
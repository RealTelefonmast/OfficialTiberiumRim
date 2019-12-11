using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class BeamSegment
    {
        public List<Building_BeamSegmentPart> segments = new List<Building_BeamSegmentPart>();
        public Building_BeamHub[] parents;

        public List<IntVec3> Cells = new List<IntVec3>();
        private bool active = false;
        private Map map;

        private TRThingDef parentDef;

        public BeamSegment(Building_BeamHub[] parents, List<IntVec3> cells)
        {
            this.parents = parents;
            this.Cells = cells;
            this.map = parents[0].Map;
            parentDef = parents[0].def;
            MakeSegments();
        }

        public Building_BeamHub OppositeHubFor(Building_BeamHub hub)
        {
            return parents[0] == hub ? parents[1] : parents[0];
        }

        public void MakeSegments()
        {
            foreach(IntVec3 cell in Cells)
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
            for (int i = 0; i < parents.Length; i++)
            {
                Building_BeamHub hub = parents[i];
                hub.RemoveConnection(this);
            }
            foreach (Building_BeamSegmentPart segment in segments)
            {
                if (!segment.Spawned) continue;
                segment.Remove(this);
            }
        }

        public void Toggle(bool on)
        {
            active = on;
            if (active)
                Activate();
            else
                Deactivate();
        }

        private void Activate()
        {
            if (!IsPowered) return;
            active = true;
            segments.ForEach(s => s.Activate());
        }

        private void Deactivate()
        {
            active = false;
            segments.ForEach(s => s.Deactivate());
        }

        public bool IsActive => active;
        public bool IsPowered => IsValid && parents[0].IsPowered && parents[1].IsPowered;
        public bool IsValid => parents[0].Spawned && parents[1].Spawned;
        public bool IsGate => false;
    }

    public class Building_BeamSegmentPart : Building
    {
        private BeamSegment[] segments = new BeamSegment[4];
        private bool active = false;
        private TRThingDef parentDef;

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
            segments.InsertionSort((BeamSegment s1, BeamSegment s2) => s2?.IsValid.CompareTo(s1?.IsValid) ?? 0);
            for (int i = segments.Length - 1; i > 0; i--)
            {
                BeamSegment segment = segments[i];
                if(segment != null && !segment.IsValid)
                {
                    segments[i] = null;
                }
            }
        }

        public void AddSegment(BeamSegment newParent)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                if(segments[i] == null)
                {
                    segments[i] = newParent;
                    return;
                }
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

        public bool AnyParentActive => segments.Any(s => s != null && s.IsActive); 
        public bool AnySegmentValid => segments.Any(s => s != null && s.IsValid); 

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
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
                List<IntVec3> cells = new List<IntVec3>();
                foreach(BeamSegment segment in segments)
                {
                    if(segment != null)
                    {
                        foreach(Building_BeamHub hub in segment.parents)
                        {
                            if (hub != null)
                                cells.Add(hub.Position);
                        }
                    }
                }
                GenDraw.DrawFieldEdges(cells, Color.magenta);
            }
        }
    }
}

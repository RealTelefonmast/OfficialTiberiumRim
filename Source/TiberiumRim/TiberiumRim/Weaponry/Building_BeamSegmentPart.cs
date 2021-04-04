using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    /*
    public class Building_BeamSegmentPart2 : Building
    {
        private BeamSegment2[] segments = new BeamSegment2[4];
        private bool active = false;
        private TRThingDef parentDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref parentDef, "beamSegmentParent");
            Scribe_Values.Look(ref active, "active");
        }

        public void Setup(BeamSegment2 parent, TRThingDef parentDef)
        {
            AddSegment(parent);
            this.parentDef = parentDef;
        }

        public void Remove(BeamSegment2 parent)
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
                if (segment != null && !segment.IsValid)
                {
                    segments[i] = null;
                }
            }
        }

        public void AddSegment(BeamSegment newParent)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] == null)
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
                foreach (BeamSegment segment in segments)
                {
                    if (segment != null)
                    {
                        foreach (Building_BeamHub hub in segment.parents)
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
    */
}

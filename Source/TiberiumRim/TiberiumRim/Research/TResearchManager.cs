using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TResearchManager : WorldComponent, IExposable
    {
        public TResearchDef currentProj;

        private Dictionary<TResearchDef, float> progress = new Dictionary<TResearchDef, float>();
        private Dictionary<TResearchDef, bool> completed = new Dictionary<TResearchDef, bool>();
        public TResearchManager(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref currentProj, "currentProj");
            Scribe_Collections.Look(ref progress, "progress", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref completed, "completed", LookMode.Def, LookMode.Value);
        }

        public void CheckResearch()
        {

        }

        public void Complete(TResearchDef def)
        {
            if(!completed.ContainsKey(def))
                completed.Add(def, true);
        }

        public bool IsCompleted(TResearchDef def)
        {
            return completed.TryGetValue(def, out bool value) && value;
            //return progress.TryGetValue(def, out float value) && value >= def.baseCost;
        }

        public float GetProgress(TResearchDef def)
        {
            if (progress.TryGetValue(def, out var result))
            {
                return result;
            }
            progress.Add(def, 0f);
            return 0f;
        }

        public void AddProgress(TResearchDef def, float f)
        {
            if (progress.ContainsKey(def))
            {
                progress[def] = Mathf.Min(progress[def] + f, def.baseCost);
            }
            progress.Add(def, f);
        }
    }
}

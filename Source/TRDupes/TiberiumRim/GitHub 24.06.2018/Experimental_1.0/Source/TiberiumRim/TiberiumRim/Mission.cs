using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Mission : IExposable
    {
        public MissionDef def;

        public string date;

        public bool failed = false;

        public bool seen = false;

        public Dictionary<MissionObjectiveDef, float> objectiveProgress = new Dictionary<MissionObjectiveDef, float>();

        public Dictionary<MissionObjectiveDef, int> objectiveTimer = new Dictionary<MissionObjectiveDef, int>();

        public Mission() : base()
        {
        }

        public Mission(MissionDef def)
        {
            this.def = def;
            this.date = TRUtils.GetCurrentDate();
            foreach(MissionObjectiveDef o in def.objectives)
            {               
                GetProgress(o);
                this.objectiveTimer[o] = o.TimerTicks;
            }
        }       

        public void ExposeData()
        {
            Scribe_Values.Look(ref failed, "failed");
            Scribe_Values.Look(ref seen, "seen");
            Scribe_Values.Look(ref date, "date");
            Scribe_Collections.Look(ref objectiveProgress, "objectiveProgress");
            Scribe_Collections.Look(ref objectiveTimer, "objectiveTimer");
            Scribe_Defs.Look(ref def, "def");
        }

        public void TimePassed(MissionObjectiveDef objective, int ticks)
        {
            if(objective != null)
            {
                float time = this.GetTimer(objective);
                this.objectiveTimer[objective] = (int)Mathf.Max(time - ticks, 0f);
            }
        }

        public void WorkPerformed(MissionObjectiveDef objective, float amount, Pawn researcher = null)
        {
            amount *= MainTCD.MainTiberiumControlDef.workFloat;
            if (DebugSettings.fastResearch)
            {
                amount *= 500f;
            }
            if (researcher != null)
            {
                /*
                this.AddContributor(this.GetParentProject(step), researcher);
                researcher.records.AddTo(RecordDefOf.ResearchPointsResearched, amount);
                */
            }
            if (objective != null)
            {
                float progress = this.GetProgress(objective);
                this.objectiveProgress[objective] = Mathf.Min(progress + amount, objective.workCost);
            }
        }

        public float GetProgress(MissionObjectiveDef objective)
        {
            float result;
            if (this.objectiveProgress.TryGetValue(objective, out result))
            {
                return result;
            }
            this.objectiveProgress.Add(objective, 0f);
            return 0f;
        }

        public int GetTimer(MissionObjectiveDef objective)
        {
            return objectiveTimer[objective];
        }
    }
}

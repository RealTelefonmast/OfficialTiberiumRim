using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TResearchDef : Def
    {
        public float baseCost = 100;
        public Requisites requisites;
        public TechLevel techLevel;
        public TRThingDef researchThing;

        public List<EventDef> events;

        public bool IsFinished => TRUtils.ResearchManager().IsCompleted(this);

        public float ProgressReal => TRUtils.ResearchManager().GetProgress(this);

        public virtual bool RequisitesComplete => requisites == null || requisites.Completed;

        public virtual bool CanStartNow => !IsFinished && RequisitesComplete;
    }
}

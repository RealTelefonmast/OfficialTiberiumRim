using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class IncidentProperties
    {
        [Unsaved]
        private IncidentWorker workerInt;

        public Type workerClass;

        public List<ThingDef> randomThing;

        public ThingDef spawnThing;

        public ThingDef skyfallerDef;

        public IncidentProperties()
        {
        }

        public IncidentWorker Worker
        {
            get
            {
                if (this.workerInt == null)
                {
                    this.workerInt = (IncidentWorker)Activator.CreateInstance(this.workerClass);
                }
                return this.workerInt;
            }
        }

        public virtual void ResolveReferences(ThingDef parentDef)
        {
        }
    }

    public enum IncidentType
    {
        Skyfaller,
        Appear,
        None
    }
}

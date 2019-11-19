using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class Comp_MechStation : Comp_Upgradable, IMechSpawner, IThingHolder
    {
        private CurrentMech mech;
        protected ThingOwner container;
        protected List<MechanicalPawn> storedMechs = new List<MechanicalPawn>();

        public bool HasMechInProgress => mech != null;

        public float MechProgress => mech?.Progress ?? 0;

        public new CompProperties_MechStation Props => (CompProperties_MechStation)base.props;

        public Comp_MechStation()
        {
            container = new ThingOwner<MechanicalPawn>(this, true);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref storedMechs, "storedMechs", LookMode.Reference);
            Scribe_Deep.Look(ref container, "container", new object[]
            {
                this,
                false,
                LookMode.Deep
            });
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            RemoveMechs();
            base.PostDestroy(mode, previousMap);
        }

        public void RemoveMechs()
        {
            storedMechs.ForEach(m => RemoveMech(m));
        }

        public void RemoveMech(MechanicalPawn mech)
        {
            storedMechs.Remove(mech);
            container.Remove(mech);
            //TODO: Add Job
            return;
            storedMechs.ForEach(d =>
            {
                d.jobs.ClearQueuedJobs();
                d.jobs.EndCurrentJob(JobCondition.InterruptForced);
                d.jobs.StartJob(new Job(DefDatabase<JobDef>.GetNamed("RepairMechanicalPawn"), parent.Position));
            });
        }

        protected MechanicalPawn MakeMech(MechanicalPawnKindDef mechDef)
        {
            if (storedMechs.Count >= Props.maxStored) return null;
            MechanicalPawn mech = (MechanicalPawn)PawnGenerator.GeneratePawn(mechDef, parent.Faction);
            mech.ageTracker.AgeBiologicalTicks = 0;
            mech.ageTracker.AgeChronologicalTicks = 0;
            mech.Rotation = Rot4.Random;
            mech.parent = this.parent as Building;
            mech.Drawer.renderer.graphics.ResolveAllGraphics();

            storedMechs.Add(mech);
            container.TryAdd(mech);
            return mech;
        }

        private sealed class CurrentMech : IExposable
        {
            public PawnKindDef mechDef;
            public Dictionary<MechRecipePart, List<ThingDefCount>> resources = new Dictionary<MechRecipePart, List<ThingDefCount>>();

            public float Progress
            {
                get
                {
                    return (float)resources.Sum(s => s.Value.Sum(c => c.Count)) / TotalParts;
                }
            }

            private int TotalParts => resources.Sum(s => s.Key.ingredients.Sum(c => c.count));

            public void AddResource()
            {

            }

            public void ExposeData()
            {
            }
        }

        public ThingOwner Container { get; set; }
        public bool HoldsMech { get; set; }

        public void SpawnMech()
        {

        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return container;
        }
    }

    public class CompProperties_MechStation : CompProperties_Upgrade
    {
        public List<MechRecipeDef> mechRecipes = new List<MechRecipeDef>();
        public bool hasStorage = false;
        public int maxStored = 1;

        public CompProperties_MechStation()
        {
            this.compClass = typeof(Comp_MechStation);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class HediffComp_RangedVerb : HediffComp_Gizmo, IVerbOwner
    {
        protected VerbTracker verbTracker;
        public Verb mainVerb;

        //
        private int autoAttackTick = 0;
        private bool canAutoAttack = true;
        private bool canAttack = false;


        public VerbTracker VerbTracker => verbTracker;
        public List<Verb> AllVerbs => verbTracker.AllVerbs;
        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Hediff;
        public HediffCompProperties_RangedVerb Props => (HediffCompProperties_RangedVerb) props;
        public List<VerbProperties> VerbProperties => Props.VerbsBase.ToList();
        public List<Tool> Tools => null;
        public Thing ConstantCaster => Pawn;

        public bool CanAttack { 
            get => canAttack || canAutoAttack;
            set => canAttack = value;
        }

        public HediffComp_RangedVerb()
        {
            this.verbTracker = new VerbTracker(this);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
            Scribe_Values.Look(ref canAutoAttack, "canAutoAttack");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if(verbTracker == null)
                    verbTracker = new VerbTracker(this);
                if(mainVerb == null)
                    InitVerb();
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            this.verbTracker.VerbsTick();

            if (canAttack) return;
            if (autoAttackTick <= 0)
            {
                canAttack = true;
                autoAttackTick = 100;
            }
            autoAttackTick--;
        }

        public void InitVerb()
        {
            mainVerb = AllVerbs.FirstOrDefault(v => !v.IsMeleeAttack);
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            InitVerb();
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                if(gizmo != null)
                    yield return gizmo;
            }
            if (Pawn.drafter?.Drafted ?? false)
            {
                yield return new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.Misc6,
                    isActive = (() => this.canAutoAttack),
                    toggleAction = delegate ()
                    {
                        this.canAutoAttack = !this.canAutoAttack;
                    },
                    icon = TexCommand.FireAtWill,
                    defaultLabel = "Hediff Auto Attack",//"CommandFireAtWillLabel".Translate(),
                    defaultDesc = "CommandFireAtWillDesc".Translate(),
                    tutorTag = "FireAtWillToggle"
                };
            }

            foreach (var command in verbTracker.GetVerbsCommands())
            {
                yield return command;
            }

            foreach (var verb in AllVerbs)
            {
                yield return CreateVerbTargetCommand(verb);
            }
            //return base.GetGizmos();
        }

        private Command_VerbTarget CreateVerbTargetCommand(Verb verb)
        {
            Command_VerbTarget target = new Command_VerbTarget();
            target.defaultDesc = "Test Verb";
            target.icon = TiberiumContent.IonCannonIcon;
            target.tutorTag = "VerbTarget";
            target.verb = verb;
            if (verb.caster.Faction != Faction.OfPlayer)
            {
                target.Disable("CannotOrderNonControlled".Translate());
            }
            else if (verb.CasterIsPawn)
            {
                if (verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
                {
                    target.Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
                else if (!verb.CasterPawn.drafter.Drafted)
                {
                    target.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
            }

            return target;
        }

        public string UniqueVerbOwnerID()
        {
            return parent.GetUniqueLoadID() + "_" + parent.comps.IndexOf(this);
        }

        public bool VerbsStillUsableBy(Pawn p)
        {
            return p.health.hediffSet.hediffs.Contains(parent);
        }
    }

    public class HediffCompProperties_RangedVerb : HediffCompProperties
    {
        public HediffCompProperties_RangedVerb()
        {
            compClass = typeof(HediffComp_RangedVerb);
        }

        public IEnumerable<VerbProperties> VerbsBase => verbs.Select(v => v as VerbProperties);

        public List<VerbProperties_TR> verbs;


    }
}

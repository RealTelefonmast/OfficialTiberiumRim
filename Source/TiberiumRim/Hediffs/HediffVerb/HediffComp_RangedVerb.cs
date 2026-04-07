using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TR.Rendering.TextureContent;
using TR.Weaponry;
using Verse;
using Command_VerbTarget = TR.Weaponry.Command_VerbTarget;
using IVerbOwner = Verse.IVerbOwner;
using Verb = Verse.Verb;

namespace TR.Hediffs.HediffVerb;

public class HediffComp_RangedVerb : HediffComp_Gizmo, IVerbOwner
{
    //
    private int autoAttackTick;
    private bool canAttack;
    private bool canAutoAttack = true;
    public Verb mainVerb;
    protected VerbTracker verbTracker;

    public HediffComp_RangedVerb()
    {
        verbTracker = new VerbTracker(this);
    }

    public List<Verb> AllVerbs => verbTracker.AllVerbs;
    public HediffCompProperties_RangedVerb Props => (HediffCompProperties_RangedVerb)props;

    public bool CanAttack
    {
        get => canAttack || canAutoAttack;
        set => canAttack = value;
    }


    public VerbTracker VerbTracker => verbTracker;
    public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Hediff;
    public List<VerbProperties> VerbProperties => Props.VerbsBase.ToList();
    public List<Tool> Tools => null;
    public Thing ConstantCaster => Pawn;

    public string UniqueVerbOwnerID()
    {
        return parent.GetUniqueLoadID() + "_" + parent.comps.IndexOf(this);
    }

    public bool VerbsStillUsableBy(Pawn p)
    {
        return p.health.hediffSet.hediffs.Contains(parent);
    }

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
        Scribe_Values.Look(ref canAutoAttack, "canAutoAttack");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (verbTracker == null)
                verbTracker = new VerbTracker(this);
            if (mainVerb == null)
                InitVerb();
        }
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        verbTracker.VerbsTick();

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
        mainVerb = Enumerable.FirstOrDefault(AllVerbs, v => !v.IsMeleeAttack);
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
            if (gizmo != null)
                yield return gizmo;
        if (Pawn.drafter?.Drafted ?? false)
            yield return new Command_Toggle
            {
                hotKey = KeyBindingDefOf.Misc6,
                isActive = () => canAutoAttack,
                toggleAction = delegate { canAutoAttack = !canAutoAttack; },
                icon = TexCommand.FireAtWill,
                defaultLabel = "Hediff Auto Attack", //"CommandFireAtWillLabel".Translate(),
                defaultDesc = "CommandFireAtWillDesc".Translate(),
                tutorTag = "FireAtWillToggle"
            };

        foreach (var command in verbTracker.GetVerbsCommands()) yield return command;

        foreach (var verb in AllVerbs) yield return CreateVerbTargetCommand(verb);
        //return base.GetGizmos();
    }

    private Command_VerbTarget CreateVerbTargetCommand(Verb verb)
    {
        var target = new Command_VerbTarget();
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
                target.Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
            else if (!verb.CasterPawn.drafter.Drafted)
                target.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
        }

        return target;
    }
}

public class HediffCompProperties_RangedVerb : HediffCompProperties
{
    public List<VerbProperties_TR> verbs;

    public HediffCompProperties_RangedVerb()
    {
        compClass = typeof(HediffComp_RangedVerb);
    }

    public IEnumerable<VerbProperties> VerbsBase => verbs.Select(v => v as VerbProperties);
}
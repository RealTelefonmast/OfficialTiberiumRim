using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TeleCore.Hediffs;

public class Command_HediffVerbTarget : Command
{
    public bool drawRadius = true;
    private List<Verb> groupedVerbs;

    public HediffComp_RangedVerb RangedHediff;
    public Verb verb;

    public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
    {
        get
        {
            foreach (var verb in RangedHediff.AllVerbs)
                yield return new FloatMenuOption("place link", null);
        }
    }

    public override void GizmoUpdateOnMouseover()
    {
        if (!drawRadius) return;
        verb.verbProps.DrawRadiusRing(verb.caster.Position);
        if (groupedVerbs.NullOrEmpty()) return;
        foreach (var verb in groupedVerbs) verb.verbProps.DrawRadiusRing(verb.caster.Position);
    }

    public override void MergeWith(Gizmo other)
    {
        base.MergeWith(other);
        var command = other as Command_HediffVerbTarget;
        if (command == null) return;
        if (groupedVerbs == null) 
            groupedVerbs = new List<Verb>();
        groupedVerbs.Add(command.verb);
        if (command.groupedVerbs != null) 
            groupedVerbs.AddRange(command.groupedVerbs);
    }

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        var targeter = Find.Targeter;

        var casterPawn = verb.CasterPawn;
        if (!targeter.IsPawnTargeting(casterPawn)) targeter.targetingSourceAdditionalPawns.Add(casterPawn);
        //targeter.BeginTargeting(verb, null);
    }
}
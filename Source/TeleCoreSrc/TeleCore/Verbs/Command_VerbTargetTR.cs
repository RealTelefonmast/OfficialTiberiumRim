using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TeleCore.Verbs;

public class Command_VerbTargetTR : Command
{
    private List<Verb> _groupedVerbs;
    
    public Verb verb;
    public bool drawRadius = true;

    public override Color IconDrawColor
    {
        get
        {
            if (verb.EquipmentSource != null) return verb.EquipmentSource.DrawColor;
            return base.IconDrawColor;
        }
    }

    public override void GizmoUpdateOnMouseover()
    {
        if (!drawRadius) return;
        this.verb.verbProps.DrawRadiusRing(this.verb.caster.Position);
        if (!_groupedVerbs.NullOrEmpty())
            foreach (var verb in _groupedVerbs)
                verb.verbProps.DrawRadiusRing(verb.caster.Position);
    }

    public override void MergeWith(Gizmo other)
    {
        base.MergeWith(other);
        var command_VerbTarget = other as Command_VerbTarget;
        if (command_VerbTarget == null)
        {
            Log.ErrorOnce($"Tried to merge Command_VerbTarget with unexpected type: {other}", 83400264);
            return;
        }

        if (_groupedVerbs == null) 
            _groupedVerbs = new List<Verb>();
        _groupedVerbs.Add(command_VerbTarget.verb);
        if (command_VerbTarget.groupedVerbs != null) _groupedVerbs.AddRange(command_VerbTarget.groupedVerbs);
    }

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        var targeter = Find.Targeter;
        if (verb.CasterIsPawn && targeter.targetingSource != null &&
            targeter.targetingSource.GetVerb.verbProps == verb.verbProps)
        {
            var casterPawn = verb.CasterPawn;
            if (!targeter.IsPawnTargeting(casterPawn)) targeter.targetingSourceAdditionalPawns.Add(casterPawn);
        }
        else
        {
            Find.Targeter.BeginTargeting(verb);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class Command_HediffVerbTarget : Command
    {
        public Verb verb;
        private List<Verb> groupedVerbs;
        public bool drawRadius = true;

        public HediffComp_RangedVerb RangedHediff;

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                foreach (var verb in RangedHediff.AllVerbs)
                {
                    yield return new FloatMenuOption("place link", null);
                }
            }
        }

        public override void GizmoUpdateOnMouseover()
        {
            if (!drawRadius)
            {
                return;
            }
            verb.verbProps.DrawRadiusRing(verb.caster.Position);
            if (groupedVerbs.NullOrEmpty()) return;
            foreach (Verb verb in groupedVerbs)
            {
                verb.verbProps.DrawRadiusRing(verb.caster.Position);
            }
        }

        public override void MergeWith(Gizmo other)
        {
            base.MergeWith(other);
            Command_HediffVerbTarget command = other as Command_HediffVerbTarget;
            if (command == null) return;
            if (groupedVerbs == null)
            {
                groupedVerbs = new List<Verb>();
            }
            groupedVerbs.Add(command.verb);
            if (command.groupedVerbs != null)
            {
                groupedVerbs.AddRange(command.groupedVerbs);
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
            Targeter targeter = Find.Targeter;

            Pawn casterPawn = this.verb.CasterPawn;
            if (!targeter.IsPawnTargeting(casterPawn))
            {
                targeter.targetingSourceAdditionalPawns.Add(casterPawn);
            }
            //targeter.BeginTargeting(verb, null);
        }
    }
}

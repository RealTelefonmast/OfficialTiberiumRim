using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ITab_ContainedPawn_Health : ITab
    {
        public const float Width = 630f;
        private const int HideBloodLossTicksThreshold = 60000;

        public ITab_ContainedPawn_Health()
        {
            this.size = new Vector2(630f, 430f);
            this.labelKey = "TabHealth";
            this.tutorTag = "Health";
        }

        private Pawn PawnForHealth
        {
            get
            {
                ThingOwner owner = ((IThingHolder) SelThing).GetDirectlyHeldThings();
                if (owner.NullOrEmpty()) return null;
                return owner.First() as Pawn;

            }
        }

        protected override void FillTab()
        {
            Pawn pawnForHealth = this.PawnForHealth;
            if (pawnForHealth == null) return;
            Corpse corpse = base.SelThing as Corpse;
            bool showBloodLoss = corpse == null || corpse.Age < 60000;
            Rect outRect = new Rect(0f, 20f, this.size.x, this.size.y - 20f);
            HealthCardUtility.DrawPawnHealthCard(outRect, pawnForHealth, this.ShouldAllowOperations(), showBloodLoss, base.SelThing);
        }

        private bool ShouldAllowOperations()
        {
            Pawn pawnForHealth = this.PawnForHealth;
            if (pawnForHealth.Dead)
            {
                return false;
            }
            return base.SelThing.def.AllRecipes.Any((RecipeDef x) => x.AvailableNow) && (pawnForHealth.Faction == Faction.OfPlayer || (pawnForHealth.IsPrisonerOfColony || (pawnForHealth.HostFaction == Faction.OfPlayer && !pawnForHealth.health.capacities.CapableOf(PawnCapacityDefOf.Moving))) || ((!pawnForHealth.RaceProps.IsFlesh || pawnForHealth.Faction == null || !pawnForHealth.Faction.HostileTo(Faction.OfPlayer)) && (!pawnForHealth.RaceProps.Humanlike && pawnForHealth.Downed)));
        }

    }
}

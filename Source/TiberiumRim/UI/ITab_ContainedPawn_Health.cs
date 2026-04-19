using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TR;

public class ITab_ContainedPawn_Health : ITab
{
    public const float Width = 630f;
    private const int HideBloodLossTicksThreshold = 60000;

    public ITab_ContainedPawn_Health()
    {
        size = new Vector2(630f, 430f);
        labelKey = "TabHealth";
        tutorTag = "Health";
    }

    private Pawn PawnForHealth
    {
        get
        {
            var owner = ((IThingHolder)SelThing).GetDirectlyHeldThings();
            if (owner.NullOrEmpty()) return null;
            return owner.First() as Pawn;
        }
    }

    public override void FillTab()
    {
        var pawnForHealth = PawnForHealth;
        if (pawnForHealth == null) return;
        var corpse = SelThing as Corpse;
        var showBloodLoss = corpse == null || corpse.Age < 60000;
        var outRect = new Rect(0f, 20f, size.x, size.y - 20f);
        HealthCardUtility.DrawPawnHealthCard(outRect, pawnForHealth, ShouldAllowOperations(), showBloodLoss, SelThing);
    }

    private bool ShouldAllowOperations()
    {
        var pawnForHealth = PawnForHealth;
        if (pawnForHealth.Dead) return false;
        return SelThing.def.AllRecipes.Any(x => x.AvailableNow) && (pawnForHealth.Faction == Faction.OfPlayer ||
                                                                    pawnForHealth.IsPrisonerOfColony ||
                                                                    (pawnForHealth.HostFaction == Faction.OfPlayer &&
                                                                     !pawnForHealth.health.capacities.CapableOf(
                                                                         PawnCapacityDefOf.Moving)) ||
                                                                    ((!pawnForHealth.RaceProps.IsFlesh ||
                                                                      pawnForHealth.Faction == null ||
                                                                      !pawnForHealth.Faction
                                                                          .HostileTo(Faction.OfPlayer)) &&
                                                                     !pawnForHealth.RaceProps.Humanlike &&
                                                                     pawnForHealth.Downed));
    }
}
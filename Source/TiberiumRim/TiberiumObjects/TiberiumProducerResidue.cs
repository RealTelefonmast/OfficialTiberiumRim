using System.Collections.Generic;
using System.Text;
using TR.DefOf;
using TR.GameParts.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace TR.TiberiumObjects;

public class TiberiumProducerResidue : FXBuilding, IResearchCraneTarget
{
    private float deterioration;

    private Building researchCrane;

    public float Deterioration => Mathf.Clamp01(deterioration);

    public float DeteriorationRate
    {
        get
        {
            var rate = 1f;
            rate += Map.weatherManager.curWeather.rainRate;

            return rate * 0.0001f;
        }
    }

    public Building ResearchCrane =>
        researchCrane ??= (Building)Position.GetFirstThing(Map, TiberiumDefOf.TiberiumResearchCrane);

    public bool ResearchBound => !ResearchCrane.DestroyedOrNull();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref deterioration, "deterioration");
    }

    public override void TickRare()
    {
        base.TickRare();
        deterioration += DeteriorationRate;
        if (Deterioration >= 1f)
            DeSpawn();
    }

    public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        var damage = dinfo.Amount;
        deterioration += damage * 0.00001f;
        base.PreApplyDamage(ref dinfo, out absorbed);
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder(base.GetInspectString());
        sb.AppendLine("TiberiumResidueDeterioration".Translate() + ": " + Deterioration.ToStringPercent());
        return sb.ToString().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;
        if (Prefs.DevMode)
            yield return new Command_Action
            {
                defaultLabel = "REMOVE",
                action = delegate { DeSpawn(); }
            };
    }
}
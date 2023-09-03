﻿using System.Collections.Generic;
using System.Text;
using TeleCore;
using UnityEngine;
using Verse;

namespace TR
{
    public class TiberiumProducerResidue : FXBuilding, IResearchCraneTarget
    {
        private float deterioration = 0f;

        private Building researchCrane;
        public Building ResearchCrane => researchCrane ??= (Building)Position.GetFirstThing(Map, TiberiumDefOf.TiberiumResearchCrane);
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

        public float Deterioration => Mathf.Clamp01(deterioration);

        public float DeteriorationRate
        {
            get
            {
                float rate = 1f;
                rate += Map.weatherManager.curWeather.rainRate;

                return rate * 0.0001f;
            }
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            float damage = dinfo.Amount;
            deterioration += damage * 0.00001f;
            base.PreApplyDamage(ref dinfo, out absorbed);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.AppendLine("TiberiumResidueDeterioration".Translate() + ": " + Deterioration.ToStringPercent());
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "REMOVE",
                    action = delegate
                    {
                        this.DeSpawn();
                    }
                };
            }
        }
    }
}

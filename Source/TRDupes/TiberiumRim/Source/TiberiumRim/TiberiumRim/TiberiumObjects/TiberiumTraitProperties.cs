using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class TiberiumTraitProperties : IExposable
    {
        //Growth speed, conflicts: harvestYield, damage, infectivity
        private float growth = 1f;
        //Spread radius, conflicts: damage, infectivity
        private float spread = 1f;
        //Harvest Yield, conflicts: harvestTime
        private float harvestYield = 1f;
        //Harvest Time, conflicts: | connections: harvestYield
        private float harvestTime = 1f;
        //Radiation Intensity, conflicts: spread
        private float radiation = 1f;
        //Infectivity, conflicts: growth
        private float infectivity = 1f;
        //Evolution factor, conflicts: 
        private float evolution = 1f;
        private float damage = 1f;

        public TiberiumTraitProperties()
        {          
        }

        public TiberiumTraitProperties(TiberiumTraitProperties props)
        {
            growth = props.growth;
            spread = props.spread;
            harvestYield = props.harvestYield;
            harvestTime = props.harvestTime;
            radiation = props.radiation;
            evolution = props.evolution;
            damage = props.damage;
        }

        public void Evolve(TiberiumCrystal crystal)
        {
            float[] offsets = new float[8];
            float totalPoints = TotalPoints(crystal);
            float general = totalPoints / (float)offsets.Length;
            for (int i = 0; i < offsets.Length; i++)
            {
                float offset = totalPoints > 0f ? general : 0f;
                totalPoints -= offset;
                if (TRUtils.RandValue > 0.4646469f)
                {
                    offset = -offset;
                }
                offsets[i] = TRUtils.Chance(0.68f) ? offset : 0f;
                i++;
            }
            GrowthAdd(offsets[0]);
            SpreadAdd(offsets[1]);
            HarvestYieldAdd(offsets[2]);
            HarvestTimeAdd(offsets[3]);
            RadiationAdd(offsets[4]);
            EvolutionAdd(offsets[5]);
            InfectivityAdd(offsets[6]);
            DamageAdd(offsets[7]);
        }

        private float TotalPoints(TiberiumCrystal crystal)
        {
            float total = TRUtils.Range(0.7f, 1.4f);
           // total *= Mathf.Lerp(1f, 0f, -crystal.Map.mapTemperature.OutdoorTemp);
            //total *= crystal.HitPoints / crystal.MaxHitPoints;
            //total *= crystal.HarvestValue / crystal.def.tiberium.harvestValue;
            return total;
        }

        public float Growth
        {
            get
            {
                return Mathf.Clamp(growth, 0.001f, 2f);
            }
        }

        public float Spread
        {
            get
            {
                return Mathf.Clamp(spread, 0f, 2f);
            }
        }

        public float HarvestYield
        {
            get
            {
                return Mathf.Clamp(harvestYield, 0.1f, 2f);
            }
        }

        public float HarvestTime
        {
            get
            {
                return Mathf.Clamp(harvestTime, 0.25f, 2f);
            }
        }

        public float Radiation
        {
            get
            {
                return Mathf.Clamp(radiation, 0f, 2f);
            }
        }

        public float Infectivity
        {
            get
            {
                return Mathf.Clamp(infectivity, 0f, 2f);
            }
        }

        public float Evolution
        {
            get
            {
                return Mathf.Clamp(evolution, 0.01f, 2f);
            }
        }

        public float Damage
        {
            get
            {
                return Mathf.Clamp(damage, 0f, 2f);
            }
        }

        private void GrowthAdd(float value)
        {
            growth += value;
            harvestYield -= value * 0.45f;
            damage -= value * 0.35f;
            infectivity -= value * 0.2f;
        }

        private void SpreadAdd(float value)
        {
            spread += value;
            infectivity -= value * 0.2f;
            damage -= value * 0.35f;
            growth -= value * 0.45f;
        }

        private void HarvestYieldAdd(float value)
        {
            harvestYield += value;
            growth -= value * 0.45f;
            spread -= value * 0.2f;
            harvestTime += value * 0.35f;
        }

        private void HarvestTimeAdd(float value)
        {
            Math.Ceiling(4d);
            harvestTime += value;
            harvestYield += value * 0.66f;
        }

        private void RadiationAdd(float value)
        {
            radiation += value;
            infectivity += value * 0.375f;
            damage += value * 0.375f;
            harvestYield += value * 0.25f;
        }

        private void InfectivityAdd(float value)
        {
            infectivity += value;
            spread -= value * 0.3f;
            harvestYield += value * 0.5f;
        }

        private void EvolutionAdd(float value)
        {
            evolution += value;
            growth += value * 0.66f;
        }

        private void DamageAdd(float value)
        {
            damage += value;
            harvestYield += value * 0.5f;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref growth, "growth");
            Scribe_Values.Look(ref spread, "spread");
            Scribe_Values.Look(ref harvestYield, "yield");
            Scribe_Values.Look(ref harvestTime, "harvestTime");
            Scribe_Values.Look(ref radiation, "radiation");
            Scribe_Values.Look(ref evolution, "evolution");
            Scribe_Values.Look(ref damage, "damage");
        }

    }
}

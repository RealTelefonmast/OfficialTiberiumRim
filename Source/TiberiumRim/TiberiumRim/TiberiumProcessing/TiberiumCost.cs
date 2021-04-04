using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    /// <summary>
    /// The TiberiumCost can define a general cost of any tiberium type, or specify a range of specific tiberium type values
    /// </summary>
    public class TiberiumCost
    {
        public bool useDirectStorage = false;
        //GeneralValue
        public float generalCost;
        //Specified Types (and potential specific costs)
        public List<TiberiumTypeCost> specificTypes;

        public bool HasSpecifics => SpecificCosts.Any();
        public float SpecificCost => HasSpecifics ? SpecificCosts.Sum(t => t.cost) : 0;
        public float TotalCost => generalCost + SpecificCost;

        public IEnumerable<TiberiumTypeCost> SpecificCosts => specificTypes.Where(s => s.HasValue);
        public IEnumerable<TiberiumValueType> AllowedTypes => specificTypes.Select(c => c.valueType);

        private float ValueForTypesWithoutSpecifics(CompTNW compTNW)
        {
            float totalValue = 0;
            //Go through all allowed types by this cost
            foreach (var type in AllowedTypes)
            {
                var valueForType = useDirectStorage ? compTNW.Container.ValueForType(type) : compTNW.Network.NetworkValueFor(type);
                //Adjust the available value by the previously "taken" amount from the specific cost
                var specType = SpecificCosts.First(t => t.valueType == type);
                if (specType != null)
                {
                    totalValue += Mathf.Clamp(valueForType - specType.cost, 0, float.PositiveInfinity);
                }
            }
            return totalValue;
        }

        public bool CanPayWith(CompTNW compTNW)
        {
            return useDirectStorage ? CanPayWith(compTNW.Container, compTNW) : CanPayWith(compTNW.Network, compTNW);
        }

        //
        private bool CanPayWith(TiberiumContainer container, CompTNW compTNW)
        {
            if (container.TotalStorage < TotalCost) return false;
            float totalNeeded = TotalCost;
            //Check For Specifics
            if (HasSpecifics)
            {
                foreach (var typeCost in SpecificCosts)
                {
                    var specCost = typeCost.cost;
                    if (container.ValueForType(typeCost.valueType) >= specCost)
                        totalNeeded -= specCost;
                }
            }
            //Check For Generic Cost Value
            if (generalCost > 0)
            {
                if (ValueForTypesWithoutSpecifics(compTNW) >= generalCost)
                {
                    totalNeeded -= generalCost;
                }
            }
            return totalNeeded == 0;
        }

        private bool CanPayWith(TiberiumNetwork network, CompTNW compTNW)
        {
            var totalNetworkValue = network.TotalNetworkValue;
            if (totalNetworkValue < TotalCost) return false;
            float totalNeeded = TotalCost;
            //Check For Specifics
            if (HasSpecifics)
            {
                foreach (var typeCost in SpecificCosts)
                {
                    var specCost = typeCost.cost;
                    if (network.NetworkValueFor(typeCost.valueType) >= specCost)
                        totalNeeded -= specCost;
                }
            }
            //Check For Generic Cost Value
            if (generalCost > 0)
            {
                if (ValueForTypesWithoutSpecifics(compTNW) >= generalCost)
                {
                    totalNeeded -= generalCost;
                }
            }
            return totalNeeded == 0;
        }

        private void PayWith(TiberiumContainer container, CompTNW compTNW)
        {
            var totalCost = TotalCost;
            if (totalCost <= 0) return;

            foreach (var typeCost in SpecificCosts)
            {
                if (container.TryConsume(typeCost.valueType, typeCost.cost))
                    totalCost -= typeCost.cost;
            }

            foreach (var type in AllowedTypes)
            {
                if (container.TryRemoveValue(type, totalCost, out float actualVal))
                {
                    totalCost -= actualVal;
                }
            }
            if (totalCost > 0)
                Log.Warning("Paying " + this + " for " + container.parent + " had leftOver: " + totalCost);
        }

        public void PayWith(CompTNW compTNW)
        {
            if (useDirectStorage)
                PayWith(compTNW.Container, compTNW);
            else
                PayWith(compTNW.Network, compTNW);
        }

        private void PayWith(TiberiumNetwork network, CompTNW compTNW)
        {
            var totalCost = TotalCost;
            if (totalCost <= 0) return;
            var storages = network.NetworkSet.Storages;
            foreach (var storage in storages.TakeWhile(storage => !(totalCost <= 0)))
            {
                foreach (var typeCost in SpecificCosts)
                {
                    if (storage.Container.TryConsume(typeCost.valueType, typeCost.cost))
                        totalCost -= typeCost.cost;
                }

                foreach (var type in AllowedTypes)
                {
                    if (storage.Container.TryRemoveValue(type, totalCost, out float actualVal))
                    {
                        totalCost -= actualVal;
                    }
                }
            }

            if(totalCost > 0)
                Log.Warning("Paying " + this + " for " + network + " had leftOver: " + totalCost);
        }

        public override string ToString()
        {
            string retString = "TiberiumCost([" + generalCost + "]";
            foreach (var specificCost in SpecificCosts)
            {
                retString += "|" + specificCost.valueType.ShortLabel() + ": " + specificCost.cost;
            }

            retString += "[";
            foreach (var type in AllowedTypes)
            {
                retString += "|" + type.ShortLabel();
            }
            return retString + "])";
        }
    }
}

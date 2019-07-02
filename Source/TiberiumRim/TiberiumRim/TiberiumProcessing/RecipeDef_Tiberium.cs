using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using System.Xml;

namespace TiberiumRim
{
    public class RecipeDef_Tiberium : RecipeDef
    {
        public TiberiumCost tiberiumCost;
    }

    public class TiberiumCost
    {
        public List<TiberiumTypeCost> costs = new List<TiberiumTypeCost>();
        public float cost;
        public List<ThingDefCountClass> products = new List<ThingDefCountClass>();

        private Dictionary<TiberiumValueType, float> specifics = new Dictionary<TiberiumValueType, float>();

        public bool HasSpecificCost => costs.Any(c => c.cost > 0);
        public float TotalCost => cost + costs.Sum(c => c.cost);
        public float AnyCost => cost;

        public Dictionary<TiberiumValueType, float> SpecificCosts
        {
            get
            {
                if (!specifics.Any())
                {
                    foreach(var cost in costs)
                    {
                        if(cost.cost > 0)
                        {
                            specifics.Add(cost.TiberiumValueType, cost.cost);
                        }
                    }
                }
                return specifics;
            }
        }
        public IEnumerable<TiberiumValueType> SpecificTypes => costs.Where(c => c.cost > 0).Select(c => c.TiberiumValueType);
        public IEnumerable<TiberiumValueType> AcceptedTypes => costs.Select(c => c.TiberiumValueType);

        public bool CanPay(TiberiumContainer container)
        {
            var anyCost = cost;
            var totalCost = TotalCost;
            var types = AcceptedTypes;
            var specTypes = SpecificCosts;
            if (specTypes.Any())
            {
                foreach (var pair in specTypes)
                {
                    if (container.ValueForType(pair.Key) >= pair.Value)
                    {
                        totalCost -= pair.Value;
                    }
                }
                if ((totalCost - anyCost) != 0)
                {
                    return false;
                }
            }
            if (anyCost > 0)
            {
                if (container.ValueForTypes(types.ToList()) >= anyCost)
                {
                    totalCost -= anyCost;
                }
            }
            return totalCost == 0;
        }

        public void Pay(TiberiumContainer container)
        {
            var totalCost = TotalCost;
            if (totalCost <= 0)
            { return; }
            foreach (var spec in SpecificCosts)
            {
                if (container.TryConsume(spec.Key, spec.Value))
                {
                    totalCost -= spec.Value;
                }
            }
            foreach (var type in AcceptedTypes)
            {
                if (container.TryRemoveValue(type, totalCost, out float leftOver))
                {
                    totalCost = leftOver;
                }
            }
        }
    }

    public class TiberiumTypeCost
    {
        public TiberiumValueType TiberiumValueType = TiberiumValueType.None;
        public float cost;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if(xmlRoot.Name == "li")
            {
                TiberiumValueType = (TiberiumValueType)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(TiberiumValueType));
            }
            else
            {
                TiberiumValueType = (TiberiumValueType)ParseHelper.FromString(xmlRoot.Name, typeof(TiberiumValueType));
                cost = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
            }
        }
    }
}

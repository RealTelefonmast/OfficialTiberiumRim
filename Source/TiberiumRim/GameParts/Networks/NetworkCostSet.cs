using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TiberiumRim
{
    public class NetworkCostSet
    {
        //Cache
        private float? totalCost;
        private float? totalSpecificCost;

        private List<NetworkValueDef> acceptedTypes;
        private List<NetworkCostValue> specificCostsWithValues;

        //
        public float mainCost;
        public List<NetworkCostValue> specificCosts;

        public bool HasSpecifics => SpecificCosts.Any();

        public float TotalSpecificCost
        {
            get
            {
                totalSpecificCost ??= HasSpecifics ? SpecificCosts.Sum(t => t.value) : 0;
                return totalSpecificCost.Value;
            }
        }

        public List<NetworkValueDef> AcceptedValueTypes
        {
            get
            {
                acceptedTypes ??= specificCosts.Select(t => t.valueDef).ToList();
                return acceptedTypes;
            }
        }

        public List<NetworkCostValue> SpecificCosts
        {
            get
            {
                specificCostsWithValues ??= specificCosts.Where(t => t.HasValue).ToList();
                return specificCostsWithValues;
            }
        }

        public float TotalCost
        {
            get
            {
                totalCost ??= mainCost + TotalSpecificCost;
                return totalCost.Value;
            }
        }

        public override string ToString()
        {
            return $"[Total: {TotalCost}|AT: {AcceptedValueTypes.Count}|SC: {SpecificCosts.Count}]";
            /*
            string retString = $"NetworkCost([{mainCost}]";
            foreach (var specificCost in SpecificCosts)
            {
                retString = string.Join(String.Empty, retString, $"|{specificCost.valueType.ShortLabel()}: {specificCost.value}");
            }

            retString += "[";
            foreach (var type in AcceptedValueTypes)
            {
                retString += "|" + type.ShortLabel();
            }
            return retString + "])";
            */
        }
    }
}

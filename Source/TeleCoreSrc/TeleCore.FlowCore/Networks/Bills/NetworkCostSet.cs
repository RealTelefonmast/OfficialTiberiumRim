using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TeleCore.FlowCore.Bills;

public class NetworkCostSet
{
    private List<NetworkValueDef> acceptedTypes;

    //
    public float mainCost;
    public List<NetworkCostValue> specificCosts;

    private List<NetworkCostValue> specificCostsWithValues;

    //Cache
    private float? totalCost;
    private float? totalSpecificCost;

    public bool HasSpecifics => Enumerable.Any(SpecificCosts);

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

    public bool Valid =>
        (mainCost > 0 && !GenList.NullOrEmpty(specificCosts)) || !specificCostsWithValues.NullOrEmpty();

    public override string ToString()
    {
        return $"[Total: {TotalCost}|AT: {AcceptedValueTypes.Count}|SC: {SpecificCosts.Count}]";
    }
}
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class ITab_TiberiumRefinerySettings : ITab
{
    private readonly float marketPriceTiberiumFactor = 1.9f;
    public Dictionary<ThingDef, int> MetalAmount = new();

    public ITab_TiberiumRefinerySettings()
    {
        size = new Vector2(800f, 400f);
        labelKey = "TR_TibResourceRefiner";
    }

    public int TotalCost => MetalAmount.Sum(m => (int)(m.Key.BaseMarketValue * m.Value * marketPriceTiberiumFactor));

    public TiberiumCost MainCost
    {
        get
        {
            var cost = new TiberiumCost();
            foreach (var i in MetalAmount) cost.costs.Add(new TiberiumTypeCost());
            return cost;
        }
    }

    private IEnumerable<ThingDef> Metals => DefDatabase<ThingDef>.AllDefs.Where(t => t.IsMetal);

    public override void OnOpen()
    {
        base.OnOpen();
        foreach (var resource in DefDatabase<ThingDef>.AllDefs.Where(t => t.IsMetal)) MetalAmount.Add(resource, 0);
    }

    protected override void FillTab()
    {
        var mainRect = new Rect(default, size).ContractedBy(5f);
        var leftPart = mainRect.LeftHalf();
        var rightPart = mainRect.RightHalf();

        float curY = 0;
        foreach (var metal in Metals)
        {
            ResourceRow(new Rect(0, curY, leftPart.width, 40f), metal);
            curY += 42f;
        }

        Widgets.Label(rightPart, "Current Cost: " + TotalCost);
    }

    private void ResourceRow(Rect rect, ThingDef resource)
    {
        var iconRect = rect.LeftPartPixels(40);
        var sliderRect = new Rect(iconRect.xMax, rect.y, rect.width - 40, 40);
        Widgets.ThingIcon(iconRect, resource);
        MetalAmount[resource] = (int)Widgets.HorizontalSlider(sliderRect, MetalAmount[resource], 0, 100, false, default,
            default, default, 1);
    }
}
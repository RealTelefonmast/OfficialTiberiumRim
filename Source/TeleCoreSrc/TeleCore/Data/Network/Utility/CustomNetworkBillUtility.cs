using System.Collections.Generic;
using RimWorld;
using TeleCore.Network.Bills;
using UnityEngine;
using Verse;

namespace TeleCore.Network.Utility;

internal static class CustomNetworkBillUtility
{
    private static string repeatCountEditBuffer;
    private static string targetCountEditBuffer;

    public static void DrawDetails(Rect rect, CustomNetworkBill bill)
    {
        if (bill == null) return;

        var listing = new Listing_Standard();
        listing.Begin(rect);
        {
            var listing2 = listing.BeginSection(200);
            {
                if (listing2.ButtonText(bill.RepeatMode.LabelCap))
                {
                    //TODO: Move into utility class taking bill as param
                    bill.DoRepeatModeConfig();
                }

                if (bill.RepeatMode == BillRepeatModeDefOf.RepeatCount)
                {
                    listing2.Label("RepeatCount".Translate(bill.repeatCount));
                    listing2.IntEntry(ref bill.repeatCount, ref repeatCountEditBuffer);
                }
                else if (bill.RepeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    string text = "CurrentlyHave".Translate() + ": ";
                    text += bill.CurrentCount;
                    text += " / ";
                    text += bill.targetCount < 999999
                        ? bill.targetCount.ToString()
                        : "Infinite".Translate().ToLower().ToString();

                    /*
                    string str = bill.recipe.WorkerCounter.ProductsDescription(this.bill);
                    if (!str.NullOrEmpty())
                    {
                        text += "\n" + "CountingProducts".Translate() + ": " + str.CapitalizeFirst();
                    }
                    */
                    listing2.Label(text);
                    var targetCount = bill.targetCount;
                    listing2.IntEntry(ref bill.targetCount, ref targetCountEditBuffer);

                    Widgets.Dropdown(listing2.GetRect(30f), bill, b => b.includeFromZone,
                        b => GenerateStockpileInclusion(bill),
                        bill.includeFromZone == null
                            ? "IncludeFromAll".Translate()
                            : "IncludeSpecific".Translate(bill.includeFromZone.label));
                }
            }
            listing.EndSection(listing2);
            listing.Gap(5);

            //StockPile
            var listing3 = listing.BeginSection(30);
            {
                var text2 = string.Format(bill.StoreMode.LabelCap,
                    bill.StoreZone != null ? bill.StoreZone.SlotYielderLabel() : "");
                if (bill.StoreZone != null && !bill.CanPossiblyStoreInStockpile(bill.StoreZone))
                {
                    text2 += $" ({"IncompatibleLower".Translate()})";
                    Text.Font = GameFont.Tiny;
                }

                if (listing3.ButtonText(text2))
                {
                    //TODO: Move into utility class taking bill as param
                    bill.DoStoreModeConfig();
                }

                Text.Font = GameFont.Small;
            }
            listing.EndSection(listing3);
            listing.Gap(5);
        }
        listing.End();
    }

    //
    private static IEnumerable<Widgets.DropdownMenuElement<Zone_Stockpile>> GenerateStockpileInclusion(
        CustomNetworkBill bill)
    {
        yield return new Widgets.DropdownMenuElement<Zone_Stockpile>
        {
            option = new FloatMenuOption("IncludeFromAll".Translate(), delegate { bill.includeFromZone = null; }),
            payload = null
        };
        List<SlotGroup> groupList =
            bill.Stack.ParentBuilding.Map.haulDestinationManager.AllGroupsListInPriorityOrder;
        var groupCount = groupList.Count;
        int num;
        for (var i = 0; i < groupCount; i = num)
        {
            var slotGroup = groupList[i];
            if (slotGroup.parent is Zone_Stockpile stockpile)
            {
                if (!bill.CanPossiblyStoreInStockpile(stockpile))
                    yield return new Widgets.DropdownMenuElement<Zone_Stockpile>
                    {
                        option = new FloatMenuOption(
                            $"{"IncludeSpecific".Translate(slotGroup.parent.SlotYielderLabel())} ({"IncompatibleLower".Translate()})",
                            null),
                        payload = stockpile
                    };
                else
                    yield return new Widgets.DropdownMenuElement<Zone_Stockpile>
                    {
                        option = new FloatMenuOption(
                            "IncludeSpecific".Translate(slotGroup.parent.SlotYielderLabel()),
                            delegate { bill.includeFromZone = stockpile; }),
                        payload = stockpile
                    };
            }

            num = i + 1;
        }
    }

    //
    public static int CountProducts(CustomNetworkBill bill)
    {
        ThingDefCountClass thingDefCountClass = bill.results[0];
        var thingDef = thingDefCountClass.thingDef;
        if (thingDefCountClass.thingDef.CountAsResource && bill.includeFromZone == null)
            return bill.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) + GetCarriedCount(bill, thingDef);
        var num = 0;
        if (bill.includeFromZone == null)
        {
            num = CountValidThings(bill.Map.listerThings.ThingsOfDef(thingDefCountClass.thingDef), bill, thingDef);
            if (thingDefCountClass.thingDef.Minifiable)
            {
                List<Thing> list = bill.Map.listerThings.ThingsInGroup(ThingRequestGroup.MinifiedThing);
                for (var i = 0; i < list.Count; i++)
                {
                    var minifiedThing = (MinifiedThing) list[i];
                    if (CountValidThing(minifiedThing.InnerThing, bill, thingDef))
                        num += minifiedThing.stackCount * minifiedThing.InnerThing.stackCount;
                }
            }

            num += GetCarriedCount(bill, thingDef);
        }
        else
        {
            foreach (var outerThing in bill.includeFromZone.AllContainedThings)
            {
                var innerIfMinified = outerThing.GetInnerIfMinified();
                if (CountValidThing(innerIfMinified, bill, thingDef)) num += innerIfMinified.stackCount;
            }
        }

        return num;
    }

    private static int GetCarriedCount(CustomNetworkBill bill, ThingDef prodDef)
    {
        var num = 0;
        foreach (var pawn in bill.Map.mapPawns.FreeColonistsSpawned)
        {
            var thing = pawn.carryTracker.CarriedThing;
            if (thing != null)
            {
                var stackCount = thing.stackCount;
                thing = thing.GetInnerIfMinified();
                if (CountValidThing(thing, bill, prodDef)) num += stackCount;
            }
        }

        return num;
    }

    public static int CountValidThings(List<Thing> things, CustomNetworkBill bill, ThingDef def)
    {
        var num = 0;
        for (var i = 0; i < things.Count; i++)
            if (CountValidThing(things[i], bill, def))
                num++;
        return num;
    }

    public static bool CountValidThing(Thing thing, CustomNetworkBill bill, ThingDef def)
    {
        var def2 = thing.def;
        if (def2 != def) return false;
        if (def2.IsApparel && ((Apparel) thing).WornByCorpse) return false;
        var compQuality = thing.TryGetComp<CompQuality>();
        return compQuality == null;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Defs;
using TeleCore.MapComponents;
using TeleCore.Types.Structs;
using UnityEngine;
using Verse;

namespace TeleCore.Atmospheres.Types.Utils;

public static class AtmosphericUtility
{
    private static readonly int SampleNumCells = GenRadial.NumCellsInRadius(8.9f);
    private static readonly List<IntVec3> relevantCells = new();
    private static readonly List<Room> visibleRooms = new();

    public static void DrawPassPercentCells()
    {
        if (!false) return;

        //
        var root = UI.MouseCell();
        var map = Find.CurrentMap;
        //FillAtmosphereRelevantCells(UI.MouseCell(), Find.CurrentMap);

        var cacheInfo = Find.CurrentMap.GetMapInfo<DynamicAtmosphericDataMapInfo>();
        for (var i = 0; i < SampleNumCells; i++)
        {
            var intVec = root + GenRadial.RadialPattern[i];
            if (intVec.InBounds(map) && !intVec.Fogged(map))
            {
                var value = cacheInfo.AtmosphericPassGrid[intVec];
                DebugCellRenderer.RenderCell(intVec, Color.black, Color.green, value);
            }
        }
    }

    public static void DrawPassPercentCellsGUI()
    {
        if (!false) return;

        //
        var root = UI.MouseCell();
        var map = Find.CurrentMap;
        //FillAtmosphereRelevantCells(UI.MouseCell(), Find.CurrentMap);

        var cacheInfo = Find.CurrentMap.GetMapInfo<DynamicAtmosphericDataMapInfo>();
        for (var i = 0; i < SampleNumCells; i++)
        {
            var intVec = root + GenRadial.RadialPattern[i];
            if (intVec.InBounds(map) && !intVec.Fogged(map))
            {
                var value = cacheInfo.AtmosphericPassGrid[intVec];
                GenMapUI.DrawThingLabel(GenMapUI.LabelDrawPosFor(intVec), value.ToStringPercent(), Color.white);
            }
        }
    }

    public static void DrawSpreadingGasAroundMouse()
    {
        if (!false) return;

        /*
        FillAtmosphereRelevantCells(UI.MouseCell(), Find.CurrentMap);
        for (int i = 0; i < relevantCells.Count; i++)
        {
            IntVec3 intVec = relevantCells[i];
            var gasGrid =  Find.CurrentMap.GetMapInfo<SpreadingGasGrid>();
            var gasStack = gasGrid.CellStackAt(intVec.Index(gasGrid.Map));

            string stackLabel = "";
            for (int s = 0; s < gasStack.Length; s++)
            {
                stackLabel += $"[{gasStack[s].value}]\n[{gasStack[s].overflow}]";
            }

            GenMapUI.DrawThingLabel(GenMapUI.LabelDrawPosFor(intVec), stackLabel, Color.white);
        }
        */
    }

    private static void MenuOnGUI()
    {
        if (!UI.MouseCell().InBounds(Find.CurrentMap)) return;

        var rect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 336f,
            CellInspectorDrawer.numLines * 24f + 24f);
        CellInspectorDrawer.numLines = 0;

        rect.x += 26f;
        rect.y += 26f;
        if (rect.xMax > UI.screenWidth) rect.x -= rect.width + 52f;
        if (rect.yMax > UI.screenHeight) rect.y -= rect.height + 52f;

        Find.WindowStack.ImmediateWindow(733348, rect, WindowLayer.Super, FillWindow);
    }

    private static void FillWindow()
    {
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;
        Text.WordWrap = false;

        var intVec = UI.MouseCell();
        if (!intVec.InBounds(Find.CurrentMap)) return;
        var gasGrid = Find.CurrentMap.GetMapInfo<SpreadingGasGrid>();
        var gasStack = gasGrid.CellStackAtUnsafe(intVec.Index(gasGrid.Map));

        CellInspectorDrawer.DrawHeader("Stack Inspection");
        CellInspectorDrawer.DrawRow("Gas Grid: ", gasGrid.Length.ToString());
        CellInspectorDrawer.DrawRow("Has Any Gas: ", gasGrid.HasAnyGas.ToString());
        CellInspectorDrawer.DrawRow("Total Gas Count: ", gasGrid.TotalGasCount.ToString());
        CellInspectorDrawer.DrawRow("Total Gas Value: ", gasGrid.TotalGasValue.ToString());
        CellInspectorDrawer.DrawDivider();
        CellInspectorDrawer.DrawRow("Stack Types: ", gasStack.Length.ToString());
        CellInspectorDrawer.DrawRow("Any Gas In Stack: ", gasStack.HasAnyGas.ToString());
        CellInspectorDrawer.DrawRow("Total Stack Value: ", gasStack.totalValue.ToString());
        CellInspectorDrawer.DrawDivider();
        for (var i = 0; i < gasStack.Length; i++)
        {
            var value = gasStack[i];
            CellInspectorDrawer.DrawRow($"{(SpreadingGasTypeDef)value.defID}:", string.Empty);
            CellInspectorDrawer.DrawRow("Total Gas Count:", gasGrid.TotalSubGasCount[value.defID].ToString());
            CellInspectorDrawer.DrawRow("Total Gas Value:", gasGrid.TotalSubGasValue[value.defID].ToString());
            CellInspectorDrawer.DrawRow($"{nameof(GasCellValue.value)}:", value.value.ToString());
            CellInspectorDrawer.DrawRow($"{nameof(GasCellValue.overflow)}:", value.overflow.ToString());
            CellInspectorDrawer.DrawRow($"{nameof(GasCellValue.TotalBitVal)}:", value.TotalBitVal.ToString());
        }

        Text.WordWrap = true;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    private static void FillAtmosphereRelevantCells(IntVec3 root, Map map)
    {
        relevantCells.Clear();
        var room = root.GetRoom(map);
        if (room == null) return;

        visibleRooms.Clear();
        visibleRooms.Add(room);
        if (room.IsDoorway)
            foreach (var region in room.FirstRegion.Neighbors)
                if (!visibleRooms.Contains(region.Room))
                    visibleRooms.Add(region.Room);

        for (var i = 0; i < SampleNumCells; i++)
        {
            var intVec = root + GenRadial.RadialPattern[i];
            if (intVec.InBounds(map) && !intVec.Fogged(map))
            {
                var room2 = intVec.GetRoom(map);
                if (!visibleRooms.Contains(room2))
                {
                    var flag = false;
                    for (var j = 0; j < 8; j++)
                    {
                        var loc = intVec + GenAdj.AdjacentCells[j];
                        if (visibleRooms.Contains(loc.GetRoom(map)))
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (!flag) continue;
                }

                relevantCells.Add(intVec);
            }
        }

        visibleRooms.Clear();
    }

    //
    private static float PassiveFlowPctForDoor(Building_Door door, float fillage)
    {
        var categories = door.Stuff?.stuffProps?.categories;
        if (categories is null || categories.Count == 0) return 1f - fillage;

        //TODO:Add Stat for gas permeability
        var permeability = categories.Sum(c => AtmosphericData.PassPercentByStuff.GetValueOrDefault(c, 0)) /
                           categories.Count;
        return (float)Math.Round(1f - fillage * (1f - permeability), 4);
    }

    public static float DefaultAtmosphericPassPercentAtCell(IntVec3 pos, Map map)
    {
        var thingList = pos.GetThingList(map);
        if (thingList.NullOrEmpty()) return 1f;

        //Min PassPercent Getter
        var passPct = 1f;
        for (var i = 0; i < thingList.Count; i++)
        {
            var thing = thingList[i];
            if (thing is not Building b) continue;
            var buildingPassPct = DefaultAtmosphericPassPercent(b);
            //Set maximum passage to minimum possible
            if (buildingPassPct < passPct)
                passPct = buildingPassPct;
        }

        return passPct;
    }

    public static float DefaultAtmosphericPassPercent(Thing forThing)
    {
        if (forThing == null)
            return 1f;

        var flowPct = GetFlowPct(forThing);
        return forThing switch
        {
            Building_Door door => GetDoorPassPct(door),
            Building_Vent vent => GetVentPassPct(vent),
            Building_Cooler cooler => GetCoolerPassPct(cooler),
            _ => flowPct
        };
    }

    private static float GetFlowPct(Thing forThing)
    {
        var isFullFillage = forThing.def.Fillage == FillCategory.Full;
        var fillage = forThing.def.fillPercent;
        return isFullFillage ? 0f : 1f - fillage;
    }

    private static float GetDoorPassPct(Building_Door door)
    {
        return door.Open ? 1f : PassiveFlowPctForDoor(door, door.def.fillPercent);
    }

    private static float GetVentPassPct(Building_Vent vent)
    {
        return FlickUtility.WantsToBeOn(vent) ? 1f : 0f;
    }

    private static float GetCoolerPassPct(Building_Cooler cooler)
    {
        return cooler.IsPoweredOn() ? 2f : 0f;
    }

    //TODO: needs def definition
    public static bool IsAtmosphericLink(Thing thing)
    {
        if (thing == null) return false;

        return thing switch
        {
            Building_Door => true,
            Building_Vent => true,
            Building_Cooler => true,
            _ => false
        };
    }

    #region Extensions

    public static AtmosphericMapInfo Atmosphere(this Map map)
    {
        return map.GetMapInfo<AtmosphericMapInfo>();
    }

    #endregion
}
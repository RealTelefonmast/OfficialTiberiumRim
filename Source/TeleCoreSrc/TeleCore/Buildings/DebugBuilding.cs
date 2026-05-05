using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Types.Utils;
using TeleCore.UI;
using UnityEngine;
using Verse;
using GridLayout = Verse.GridLayout;

namespace TeleCore.Buildings;

public class ITab_TAEDebug : ITab
{
    //
    private static readonly Vector2 WinSize = new(480f, 480f);
    private readonly List<TabRecord> cachedTabs;
    private Vector2 neighborListScrollPos = Vector2.zero;

    public ITab_TAEDebug()
    {
        size = WinSize;
        labelKey = "Debug";

        cachedTabs = new List<TabRecord>
        {
            new("Room", delegate { SelTab = DebugTabs.Room; },
                () => SelTab == DebugTabs.Room),
            new("Neighbours", delegate { SelTab = DebugTabs.Neighbours; },
                () => SelTab == DebugTabs.Neighbours),
            new("Border", delegate { SelTab = DebugTabs.Border; },
                () => SelTab == DebugTabs.Border)
        };
    }

    public DebugBuilding SelBuilding => (DebugBuilding)SelThing;
    public RoomComponent_Atmospheric Atmos => SelBuilding.Atmos;

    private DebugTabs SelTab { get; set; }

    public override void FillTab()
    {
        var rect = new Rect(0f, 0f, WinSize.x, WinSize.y);
        rect = rect.BottomPartPixels(rect.height - TabDrawer.TabHeight);
        TabDrawer.DrawTabs(rect, cachedTabs);
        rect = rect.ContractedBy(10f);

        //
        switch (SelTab)
        {
            case DebugTabs.Room:
                DrawRoomData(rect);
                break;
            case DebugTabs.Neighbours:
                DrawNeighbourData(rect);
                break;
            case DebugTabs.Border:
                DrawBorderData(rect);
                break;
        }
    }

    private void DrawRoomData(Rect inRect)
    {
        var layout = new GridLayout(inRect, 3, 2);
        DrawLayout(layout, 3, 2);

        var roomContainerArea = layout.GetCellRect(0, 0);
        var roomContainerLabel = roomContainerArea.TopPart(0.15f);
        var roomContainer = roomContainerArea.BottomPart(0.85f);

        var mapContainerArea = layout.GetCellRect(0, 1);
        var mapContainerLabel = mapContainerArea.TopPart(0.15f);
        var mapContainer = mapContainerArea.BottomPart(0.85f);

        Widgets.Label(roomContainerLabel, "Room Container");
        TWidgets.DrawValueContainerReadout(roomContainer, Atmos.Container);
        TWidgets.HoverContainerReadout(roomContainer, Atmos.Container);

        Widgets.Label(mapContainerLabel, "Map Container");
        TWidgets.DrawValueContainerReadout(mapContainer, Atmos.OutsideContainer);
    }

    public void DrawLayout(GridLayout layout, int cols, int rows)
    {
        for (var col = 0; col < cols; col++)
        for (var row = 0; row < rows; row++)
        {
            var cell = layout.GetCellRect(col, row);
            Widgets.DrawBoxSolid(cell, TColor.White005);
        }
    }

    private void DrawNeighbourData(Rect inRect)
    {
        var layout = new GridLayout(inRect, 3, 2);
        DrawLayout(layout, 3, 2);

        var nghbListView = layout.GetCellRect(0, 0, 1, 2);
        var nghbListLabel = nghbListView.TopPartPixels(30);
        var nghbList = nghbListView.BottomPartPixels(nghbListView.height - 30);
        var nghbListScrollView = new Rect(nghbList.x, nghbList.y, nghbList.width, Atmos.AdjacentComps.Count * 30);

        //
        Widgets.Label(nghbListLabel, "Neighbour Rooms");
        Widgets.BeginScrollView(nghbList, ref neighborListScrollPos, nghbListScrollView);

        var i = 0;
        foreach (var roomComp in Atmos.AdjacentComps)
        {
            var nghbRect = new Rect(nghbList.x, nghbListScrollView.y + i * 30, nghbListScrollView.width, 30);
            var checkBoxRect = new Rect(nghbList.xMax - 24, nghbListScrollView.y + i * 30, 24, 30);
            if (i % 2 == 0)
                Widgets.DrawHighlight(nghbRect);
            Widgets.Label(nghbRect, roomComp.ToString());
            var portal = (roomComp as RoomComponent_Atmospheric)?.Portal;
            var hasItem = SelBuilding.ActivePortals.Contains(portal);
            var previous = hasItem;
            Widgets.Checkbox(checkBoxRect.position, ref hasItem, disabled: portal == null);
            if (previous != hasItem && portal != null)
            {
                if (hasItem)
                    SelBuilding.ActivePortals.Add(portal);
                else
                    SelBuilding.ActivePortals.Remove(portal);
            }

            i++;
        }

        Widgets.EndScrollView();
    }

    private void DrawBorderData(Rect rect)
    {
        var settingsRect = rect.RightPartPixels(200).ContractedBy(10);

        //Settings
        var standard = new Listing_Standard();
        standard.Begin(settingsRect);
        standard.CheckboxLabeled("Show AtmosPortals", ref SelBuilding.ShowAtmosPortals);
        standard.CheckboxLabeled("Show All AtmosComps", ref SelBuilding.ShowAtmosComps);

        standard.End();
    }

    private void DrawAtmosContainerReadout(Rect rect, AtmosphericContainer container,
        AtmosphericContainer outside)
    {
        float height = 5;
        Widgets.BeginGroup(rect);
        {
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            foreach (var type in container.StoredDefs)
            {
                var label =
                    $"{type.labelShort}: {container.StoredValueOf(type)}({container.StoredPercentOf(type).ToStringPercent()}) | {outside.StoredValueOf(type)}({outside.StoredPercentOf(type).ToStringPercent()})";

                var typeRect = new Rect(5, height, 10, 10);
                var typeSize = Text.CalcSize(label);
                var typeLabelRect = new Rect(20, height - 2, typeSize.x, typeSize.y);
                Widgets.DrawBoxSolid(typeRect, type.valueColor);
                Widgets.Label(typeLabelRect, label);

                height += 10 + 2;
            }

            Text.Font = default;
            Text.Anchor = default;
        }
        Widgets.EndGroup();
    }

    private enum DebugTabs
    {
        Room,
        Neighbours,
        Border
    }
}

public class DebugBuilding : Building
{
    public HashSet<AtmosphericPortal> ActivePortals;

    //
    public RoomComponent_Atmospheric Atmos;
    public bool ShowAllBorderThings = true;
    public bool ShowAtmosComps;

    //
    public bool ShowAtmosPortals = true;


    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        Atmos = this.GetRoom().GetRoomComp<RoomComponent_Atmospheric>();
        ActivePortals = new HashSet<AtmosphericPortal>();
    }

    public void Notify_ActivatePortal(AtmosphericPortal portal)
    {
        ActivePortals.Add(portal);
    }

    public void Notify_DeactivatePortal(AtmosphericPortal portal)
    {
        ActivePortals.Remove(portal);
    }

    public override void Tick()
    {
        base.Tick();
        if (Atmos.Disbanded)
        {
            Atmos = this.GetRoom().GetRoomComp<RoomComponent_Atmospheric>();
            ;
        }
    }

    public override void DrawGUIOverlay()
    {
        GenMapUI.DrawThingLabel(GenMapUI.LabelDrawPosFor(Position), $"[{Atmos.Room.ID}]", Color.white);

        if (ShowAtmosComps)
        {
            var mouse = UI.MouseCell();
            if (!mouse.InBounds(Map)) return;
            var room = GenData.GetRoomFast(mouse, Find.CurrentMap);
            var tracker = room?.RoomTracker();
            var comp = room?.GetRoomComp<RoomComponent_Atmospheric>();

            var rect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 350, 200);
            Widgets.DrawMenuSection(rect);
            WidgetStackPanel.Begin(rect);
            WidgetStackPanel.DrawHeader("Atmospheric");
            WidgetStackPanel.DrawRow("Room:", $"[{room?.ID}]: {room?.CellCount}");
            WidgetStackPanel.DrawRow("Tracker:", $"{tracker}");
            WidgetStackPanel.DrawRow("Comp:", $"{comp}");
            WidgetStackPanel.End();
        }
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        if (ShowAtmosComps)
        {
            var room = Verse.UI.MouseCell().GetRoomFast(Map);
            if (room != null)
                GenDraw.DrawFieldEdges(room.Cells.ToList());
        }

        if (Find.Selector.IsSelected(this))
            foreach (var thing in Atmos.Parent.BorderListerThings.AllThings)
                DebugCellRenderer.RenderCell(thing.Position, Color.clear, Color.cyan, 1);

        foreach (var portal in ActivePortals)
            if (portal?.IsValid ?? false)
            {
                GenDraw.DrawTargetingHighlight_Cell(portal.Thing.Position);
                if (Find.Selector.IsSelected(portal.Thing)) portal.DrawDebug();
            }
    }
}
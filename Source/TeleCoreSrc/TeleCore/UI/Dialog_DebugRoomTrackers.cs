using System.Linq;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;
using GridLayout = Verse.GridLayout;

namespace TeleCore.UI;

public class Dialog_DebugRoomTrackers : Window
{
    private Vector2 _selScrollPos;
    private Vector2 _selScrollPos2;
    private RoomTracker _selTracker;

    public Dialog_DebugRoomTrackers()
    {
        doCloseX = true;
        draggable = true;
    }

    private Map Map => Find.CurrentMap;
    private MapInformation_Rooms Rooms => Map?.GetMapInfo<MapInformation_Rooms>();

    private RoomTracker SelTracker
    {
        get => _selTracker;
        set
        {
            _selTracker?.Debug_Deselect();
            SelComponent = null;
            _selTracker = value;
            _selTracker.Debug_Select();
        }
    }

    private RoomComponent SelComponent { get; set; }

    public override Vector2 InitialSize => new(1200, 512);

    public override void DoWindowContents(Rect inRect)
    {
        var original = inRect;
        inRect = inRect.LeftPartPixels(800).ContractedBy(5);

        var layout = new GridLayout(inRect, 6, 6);
        var metaRect = layout.GetCellRect(0, 0, 2, 2);
        var selRect = layout.GetCellRect(0, 2, 2, 3);

        var trackerSelection = layout.GetCellRect(0, 2, 1, 3);
        var compSelection = layout.GetCellRect(1, 2, 1, 3);

        var dataRect = layout.GetCellRect(2, 0, 4, 5);
        var debugRect = layout.GetCellRect(0, 5, 6);

        var compDebugRect = new Rect(dataRect.xMax, dataRect.y, original.width - dataRect.xMax, original.height)
            .ContractedBy(5);

        TWidgets.DrawHighlightColor(metaRect, Color.blue);
        TWidgets.DrawHighlightColor(selRect, Color.red);
        TWidgets.DrawHighlightColor(trackerSelection, Color.magenta);
        TWidgets.DrawHighlightColor(compSelection, Color.yellow);
        TWidgets.DrawHighlightColor(dataRect, Color.green);
        TWidgets.DrawHighlightColor(debugRect, Color.cyan);

        //Write Data
        {
            metaRect = metaRect.ContractedBy(2);
            Widgets.DrawMenuSection(metaRect);
            metaRect = metaRect.ContractedBy(5);
            var metaListing = new Listing_Standard();
            metaListing.Begin(metaRect);

            metaListing.Label($"Rooms: {Map.regionGrid.allRooms.Count}");
            metaListing.Label($"RoomsTrackers: {Rooms.AllTrackers.Count()}");
            metaListing.Label($"RoomsTrackersList: {Rooms.AllList.Count()}");

            metaListing.End();
        }

        //Draw Selections
        selRect = selRect.ContractedBy(2);
        Widgets.DrawMenuSection(selRect);

        //Tracker
        var curY = trackerSelection.y;
        var scrollArea = new Rect(trackerSelection.x, trackerSelection.y, trackerSelection.width,
            Rooms.AllTrackers.Count * 24);

        var i = 0;
        Widgets.BeginScrollView(trackerSelection, ref _selScrollPos, scrollArea, false);
        {
            foreach (var tracker in Rooms.AllTrackers)
            {
                var trackerRect = new Rect(trackerSelection.x, curY, trackerSelection.width, 24);
                if (i % 2 == 0)
                    Widgets.DrawAltRect(trackerRect);
                if (SelTracker == tracker.Value)
                    Widgets.DrawHighlightSelected(trackerRect);
                Widgets.DrawHighlightIfMouseover(trackerRect);

                Widgets.Label(trackerRect.ContractedBy(5, 0), $"[{tracker.Value.Room.ID}]");
                if (Widgets.ButtonInvisible(trackerRect)) SelTracker = tracker.Value;

                curY += 24;
                i++;
            }
        }
        Widgets.EndScrollView();

        if (SelTracker != null)
        {
            //Comp
            curY = compSelection.y;
            var scrollAreaComp = new Rect(compSelection.x, compSelection.y, compSelection.width,
                Rooms.AllTrackers.Count * 24);
            i = 0;
            Widgets.BeginScrollView(compSelection, ref _selScrollPos2, scrollAreaComp, false);
            {
                foreach (var component in SelTracker.Comps)
                {
                    var compRect = new Rect(compSelection.x, curY, compSelection.width, 24);
                    if (i % 2 == 0)
                        Widgets.DrawAltRect(compRect);
                    if (SelComponent == component)
                        Widgets.DrawHighlightSelected(compRect);
                    Widgets.DrawHighlightIfMouseover(compRect);

                    Widgets.Label(compRect.ContractedBy(5, 0), $"[{component.GetType().Name}]");
                    if (Widgets.ButtonInvisible(compRect)) SelComponent = component;
                    curY += 24;
                    i++;
                }
            }
            Widgets.EndScrollView();
        }

        //Draw RoomComp Data
        dataRect = dataRect.ContractedBy(2);
        Widgets.DrawMenuSection(dataRect);
        dataRect = dataRect.ContractedBy(5);
        if (SelTracker != null)
        {
            var neighbors = SelTracker.RoomNeighbors;
            var list = new Listing_Standard();
            list.Begin(dataRect.LeftHalf());
            list.Label($"ID: {SelTracker.Room.ID}");
            list.Label($"IsDisbanded: {SelTracker.IsDisbanded}");
            list.Label($"IsOutside: {SelTracker.IsOutside}");
            list.Label($"IsProper: {SelTracker.IsProper}");
            list.Label($"Corners: {SelTracker.MinMaxCorners.ToStringSafeEnumerable()}");
            list.Label($"Center: {SelTracker.ActualCenter}");
            list.Label($"Pawns: {SelTracker.ContainedPawns.ToStringSafeEnumerable()}");

            var trueNeighborsSize = neighbors.TrueNeighbors.Count * 24;
            var count = 0;
            list.Label($"True Neighbors: {neighbors.TrueNeighbors.Count}");
            var listTrue = list.BeginSection(trueNeighborsSize);
            {
                foreach (var trueNeighbor in neighbors.TrueNeighbors)
                {
                    listTrue.Label($"[{count}]: {trueNeighbor.Room.ID}");
                    count++;
                }
            }
            list.EndSection(listTrue);

            var nghbSize = neighbors.AttachedNeighbors.Count * 24;
            list.Label($"Attached Neighbors: {neighbors.AttachedNeighbors.Count}");
            var list2 = list.BeginSection(nghbSize);
            {
                count = 0;
                foreach (var attached in neighbors.AttachedNeighbors)
                {
                    list2.Label($"[{count}]: {attached.Room.ID}");
                    count++;
                }
            }
            list.EndSection(list2);

            list.End();
        }

        if (SelComponent != null)
        {
            //Comp Data
            var neighbors = SelComponent.CompNeighbors;

            var list = new Listing_Standard();
            list.Begin(dataRect.RightHalf());
            list.Label($"[{SelComponent.GetType().Name}]");

            var count = 0;
            var nghbSize = neighbors.Neighbors.Count * 24;
            list.Label($"Neighbors: {neighbors.Neighbors.Count}");
            var nghbs = list.BeginSection(nghbSize);
            {
                foreach (var neighbor in neighbors.Neighbors)
                {
                    nghbs.Label($"[{count}]: {neighbor.Room.ID}");
                    count++;
                }
            }
            list.EndSection(nghbs);

            var linkSize = neighbors.Links.Count * 24;
            list.Label($"Links: {neighbors.Links.Count}");
            var links = list.BeginSection(linkSize);
            {
                count = 0;
                foreach (var link in neighbors.Links)
                {
                    links.Label($"[{count}]: {link}");
                    count++;
                }
            }
            list.EndSection(links);
            list.End();

            //
            SelComponent.Draw_DebugExtra(compDebugRect);
        }

        Widgets.DrawMenuSection(debugRect);
        debugRect = debugRect.ContractedBy(5);

        var debugList = new Listing_Standard();
        debugList.ColumnWidth = debugRect.width / 3;
        debugList.Begin(debugRect);
        var drawRoomLabels = TeleCoreDebugViewSettings.DrawRoomLabels;
        debugList.CheckboxLabeled("Show Room Labels", ref drawRoomLabels);
        TeleCoreDebugViewSettings.DrawRoomLabels = drawRoomLabels;
        if (SelTracker != null)
        {
            var drawPortals = SelTracker.DebugPortals;
            debugList.CheckboxLabeled("Show Room Portals", ref drawPortals);
            SelTracker.DebugPortals = drawPortals;
        }

        debugList.End();
    }
}
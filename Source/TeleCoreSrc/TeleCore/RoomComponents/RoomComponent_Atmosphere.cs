using System.Collections.Generic;
using TeleCore.Comps;
using TeleCore.Types;
using TeleCore.Types.Exposables;
using TeleCore.Types.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.RoomComponents;

public class RoomComponent_Atmosphere : RoomComponent
{
    private RoomOverlay_Atmospheric _renderer;

    public AtmosphericMapInfo AtmosphericInfo { get; private set; }

    public override string ShortIdentifier => "Atmos";

    public AtmosphericVolume Volume
    {
        get
        {
            if (AtmosphericInfo.System.Relations.TryGetValue(this, out var volume)) return volume;
            TLog.Warning(
                $"Tried to access volume for room {Room.ID} from {AtmosphericInfo.System.Relations.Count} relations.");
            TLog.Warning($"{AtmosphericInfo.System.Relations.Select(s => s.Key.Room.ID).ToStringSafeEnumerable()}");
            return null;
        }
    }

    public bool IsOutdoors => Parent.IsOutside;

    public override void Init(RoomTracker[] previous = null)
    {
        AtmosphericInfo = Map.GetMapInfo<AtmosphericMapInfo>();
    }

    public override void PostInit(RoomTracker[] previous = null)
    {
        _renderer = new RoomOverlay_Atmospheric();
        AtmosphericInfo.Notify_AddRoomComp(this);
    }

    public override void Disband(RoomTracker parent, Map map)
    {
        if (AtmosphericInfo == null)
        {
            TLog.Warning("Tried to disband roomcomp without atmospheric info.");
            return;
        }

        AtmosphericInfo.Notify_RemoveRoomComp(this);
    }

    public override void Notify_Reused()
    {
        AtmosphericInfo.Notify_UpdateRoomComp(this);
    }

    //Note: Used to check whether a border thing is relevant in setting up a link to another roomcomp of the same type.
    public override bool IsRelevantLink(Thing thing)
    {
        return AtmosphericUtility.IsAtmosphericLink(thing);
    }

    #region Updates

    public override void CompTick()
    {
        base.CompTick();
    }

    #endregion

    #region Data Notifiers

    public override void Notify_PawnEnteredRoom(Pawn pawn)
    {
        var tracker = pawn.TryGetComp<Comp_PawnAtmosphereTracker>();
        if (tracker == null) return;
        tracker.Notify_EnteredAtmosphere(this);
    }

    public override void Notify_PawnLeftRoom(Pawn pawn)
    {
        var tracker = pawn.TryGetComp<Comp_PawnAtmosphereTracker>();
        if (tracker == null) return;
        tracker.Notify_Clear();
    }

    #endregion

    #region Rendering

    private Vector2 scroller;

    public override void Draw_DebugExtra(Rect inRect)
    {
        var system = AtmosphericInfo.System;

        var rect = new Rect(inRect.position, new Vector2(inRect.width, inRect.height * 8));
        Widgets.BeginScrollView(inRect, ref scroller, rect, false);
        var list = new Listing_Standard();
        list.Begin(rect);
        {
            list.LabelDouble("Room ID", Room.ID.ToString());
            list.LabelDouble("Uses MapVolume?", Volume == AtmosphericInfo.MapVolume ? "Yes" : "No");
            list.LabelDouble("System Volumes", system.Volumes.Count.ToString());
            list.LabelDouble("System Relations", system.Relations.Count.ToString());
            list.LabelDouble("System Interfaces", system.Interfaces.Count.ToString());
            list.LabelDouble("System Connections", system.Connections.Count.ToString());
            list.LabelDouble("System InterfaceLookUp", system.InterfaceLookUp.Count.ToString());
            list.GapLine();

            var relations = system.Relations;
            var relCount = relations.Count;
            list.Label("Relation Listing");
            var relationList = list.BeginSection(relCount * 24);
            foreach (var relation in relations)
                relationList.LabelDouble(relation.Key.Room.ID.ToString(), relation.Value.FillPercent.ToString("P2"));
            list.EndSection(relationList);

            var interfaces = system.Interfaces;
            var interfaceCount = interfaces.Count;
            list.Label("Interface Listing");
            //Begin Section for Interfaces
            var interfaceList = list.BeginSection(interfaceCount * 24);
            {
                var i = 0;
                foreach (var iFace in interfaces)
                {
                    interfaceList.Label(
                        $"{iFace.FromPart.Room.ID} -[{iFace.Mode}][{iFace.PassPercent:P2}]-> [{iFace.ToPart.Room.ID}]");
                    i++;
                }
            }
            list.EndSection(interfaceList);

            //
            var connections = system.Connections;
            var connCount = connections.Count;
            var connSum = connections.Sum(x => x.Value.Count);
            list.Label("Connection Listing");
            var connectionList = list.BeginSection(connSum * 24 + connCount * 24);
            foreach (var conn in system.Connections)
            {
                connectionList.LabelDouble($"Room[{system.Relations.First(c => c.Value == conn.Key).Key.Room.ID}]:",
                    $"{conn.Value.Count}");
                foreach (var iFace in conn.Value)
                    connectionList.Label(
                        $"{iFace.FromPart.Room.ID} -[{iFace.Mode}][{iFace.PassPercent:P2}]-> [{iFace.ToPart.Room.ID}]");
            }

            list.EndSection(connectionList);
        }
        list.End();
        Widgets.EndScrollView();

        //system.RenderDebugRelations(inRect);
    }

    private List<Vector2> GetPointsOnCircle(float radius, int totalPoints)
    {
        var points = new List<Vector2>();
        var angleStep = 360f / totalPoints;

        for (var i = 0; i < totalPoints; i++)
        {
            var angleInDegrees = angleStep * i;
            var angleInRadians = angleInDegrees * (Mathf.PI / 180f);

            var x = radius * Mathf.Cos(angleInRadians);
            var y = radius * Mathf.Sin(angleInRadians);

            points.Add(new Vector2(x, y));
        }

        return points;
    }

    public override void OnGUI()
    {
        base.OnGUI();
    }

    public override void Draw()
    {
        base.Draw();
    }

    #endregion
}
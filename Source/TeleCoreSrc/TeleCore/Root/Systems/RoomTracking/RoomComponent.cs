using System.Collections.Generic;
using TeleCore.Utility;
using UnityEngine;
using Verse;

namespace TeleCore.Systems.RoomTracking;

public abstract class RoomComponent
{
    //Component specific neighbors
    private RoomCompNeighborSet _compNeighborSet;

    public RoomTracker Parent { get; private set; }

    public RoomNeighborSet Neighbors => Parent.RoomNeighbors;
    public RoomCompNeighborSet CompNeighbors => _compNeighborSet;

    //
    public Verse.Map Map => Parent.Map;
    public Room Room => Parent.Room;

    public IReadOnlyCollection<Thing> ContainedPawns => Parent.ContainedPawns;
    public virtual string ShortIdentifier => "Base";
    public bool Disbanded => Parent.IsDisbanded;
    public bool IsDoorway => Room.IsDoorway;

    internal void Create(RoomTracker parent)
    {
        Parent = parent;
        _compNeighborSet = new RoomCompNeighborSet();
    }

    #region Room Linking

    public virtual bool IsRelevantLink(Thing thing)
    {
        return false;
    }

    public void Notify_AddLink(RoomComponentLink link)
    {
        _compNeighborSet.Notify_AddLink(link);
    }

    internal void Notify_AddNeighbor<T>(T neighbor) where T : RoomComponent
    {
        _compNeighborSet.Notify_AddNeighbor(neighbor);
    }

    #endregion

    /// <summary>
    ///     Called after all <see cref="RoomComponent" />s on the <see cref="RoomTracker" /> parent have been created.
    /// </summary>
    public virtual void PostCreate(RoomTracker parent)
    {
    }

    /// <summary>
    ///     Called after all map data has been initialized.
    ///     Runs on the main game thread, so it is safe to use Unity methods.
    /// </summary>
    public virtual void FinalizeMapInit()
    {
    }

    /// <summary>
    ///     Called when disbanded by the <see cref="RoomTracker" /> parent.
    /// </summary>
    public virtual void Disband(RoomTracker parent, Verse.Map map)
    {
    }

    /// <summary>
    ///     Triggered when the room is reused after regeneration without deletion in the game.
    /// </summary>
    public virtual void Notify_Reused()
    {
    }

    /// <summary>
    ///     Called when the room's roof has been fully constructed in the game.
    /// </summary>
    public virtual void Notify_RoofClosed()
    {
    }

    /// <summary>
    ///     Called when the room is considered to be outdoors after the roof has been changed.
    /// </summary>
    public virtual void Notify_RoofOpened()
    {
    }

    /// <summary>
    ///     Alerted if there's any change to the roof of the room (constructed or deconstructed)
    /// </summary>
    public virtual void Notify_RoofChanged()
    {
    }

    internal void Notify_HandleBorderThing(Thing thing)
    {
        Notify_BorderThingAdded(thing);
        if (IsRelevantLink(thing))
        {
            //Note: Removing doors from room links, as the rooms that doors occupy already provide transfer
            // if (thing is Building_Door door) //Special edge case
            // {
            //     var doorRoom = door.GetRoom().RoomTracker().GetRoomComp(GetType());
            //     var doorLink = new RoomComponentLink(thing, this, doorRoom);
            //     Notify_AddLink(doorLink);
            //     Notify_AddNeighbor(doorRoom);
            //     return;
            // }

            var roomLink = new RoomComponentLink(thing, this);
            Notify_AddLink(roomLink);
            Notify_AddNeighbor(roomLink.Opposite(this));
        }
    }

    /// <summary>
    ///     Notifies the game when an object has been added to the border of the room.
    /// </summary>
    /// <param name="thing">The object that was added to the border of the room.</param>
    public virtual void Notify_BorderThingAdded(Thing thing)
    {
    }

    /// <summary>
    ///     Notifies the game when an object is added to the room.
    /// </summary>
    /// <param name="thing">The object that was added to the room.</param>
    public virtual void Notify_ThingAdded(Thing thing)
    {
    }

    /// <summary>
    ///     Called when an object is removed from the room in the game.
    /// </summary>
    /// <param name="thing">The object that was removed from the room.</param>
    public virtual void Notify_ThingRemoved(Thing thing)
    {
    }

    /// <summary>
    ///     Triggered when a pawn enters the room in the game.
    /// </summary>
    public virtual void Notify_PawnEnteredRoom(Pawn pawn)
    {
    }

    /// <summary>
    ///     Triggered when a pawn leaves the room in the game.
    /// </summary>
    public virtual void Notify_PawnLeftRoom(Pawn pawn)
    {
    }

    internal void InternalInit(RoomTracker?[]? previous = null)
    {
        //Handle edge case - Door
        if (Parent.Room.IsDoorway)
        {
            var door = Parent.Room.Regions[0].door;
            for (var i = 0; i < 4; i++)
            {
                var cell = GenAdj.CardinalDirections[i] + door.Position;
                var roomAt = cell.GetRoom(Map);
                if (roomAt != null)
                {
                    var otherRoom = roomAt.RoomTracker().GetRoomComp(GetType());
                    var doorLink = new RoomComponentLink(door, this, otherRoom);
                    Notify_AddLink(doorLink);
                    Notify_AddNeighbor(otherRoom);
                }
            }
        }

        Init(previous);
    }

    /// <summary>
    ///     Runs once on initialization.
    /// </summary>
    public virtual void Init(RoomTracker?[]? previous = null)
    {
    }

    /// <summary>
    ///     Runs once after all components have been initialized.
    /// </summary>
    public virtual void PostInit(RoomTracker?[]? previous = null)
    {
    }

    internal void Reset()
    {
        _compNeighborSet.Reset();
    }

    internal void DisbandInternal()
    {
        _compNeighborSet.Reset();
    }

    public virtual void CompTick()
    {
    }

    public virtual void OnGUI()
    {
    }

    public virtual void Draw()
    {
    }

    public virtual void Draw_DebugExtra(Rect inRect)
    {
    }

    internal void DrawDebug()
    {
        _compNeighborSet.DrawDebug(this);
    }

    public override string ToString()
    {
        return $"{ShortIdentifier}[{Room.ID}][{OutOrInside}]";
    }

    private string OutOrInside => Parent.IsOutside ? "Outside" : "Inside";
}
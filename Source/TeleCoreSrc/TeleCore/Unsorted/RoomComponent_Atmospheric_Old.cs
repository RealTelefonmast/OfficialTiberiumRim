// Preserved from TeleCore/RoomComponents/RoomComponent_Atmospheric.cs (old version of RoomComponent_Atmosphere)

using System.Collections.Generic;
using TeleCore.Comps;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

[StaticConstructorOnStartup]
public class RoomComponent_Atmospheric_Old : RoomComponent, IContainerHolderRoom<TAE.AtmosphericDef>,
    IContainerImplementer<TAE.AtmosphericDef, IContainerHolderRoom<TAE.AtmosphericDef>, TAE.AtmosphericContainer>
{
    //
    private static readonly Material FilledMat =
        SolidColorMaterials.NewSolidColorMaterial(Color.green, ShaderDatabase.MetaOverlay);

    private static readonly Material UnFilledMat =
        SolidColorMaterials.NewSolidColorMaterial(TColor.LightBlack, ShaderDatabase.MetaOverlay);

    //
    private TAE.AtmosphericContainer container;

    //
    private int dirtyMarks;
    private List<TAE.AtmosphericPortal> portals;

    private TAE.RoomOverlay_Atmospheric renderer;
    private TAE.AtmosphericPortal selfPortal;

    //
    public bool IsDoorway => Room.IsDoorway;
    public bool IsOutdoors => Parent.IsOutside;
    public bool IsDirty => dirtyMarks > 0;
    public bool IsConnector => selfPortal != null;

    public int ConnectorCount => IsOutdoors ? AtmosphericInfo.ConnectorCount : portals.Count;

    public TAE.AtmosphericPortal Portal => selfPortal;
    public TAE.AtmosphericMapInfo AtmosphericInfo => Map.GetMapInfo<TAE.AtmosphericMapInfo>();
    public TAE.AtmosphericContainer OutsideContainer => AtmosphericInfo.MapContainer;
    public TAE.AtmosphericContainer Container => container;

    public TAE.AtmosphericContainer CurrentContainer =>
        IsOutdoors ? OutsideContainer : IsConnector ? selfPortal[0].CurrentContainer : Container;

    public void Notify_ContainerStateChanged(NotifyContainerChangedArgs<TAE.AtmosphericDef> args)
    {
    }

    public string ContainerTitle => "Atmosphers be here";
    public RoomComponent RoomComponent => this;

    public override void Notify_BorderThingAdded(Thing thing)
    {
        if (thing is not Building b) return;
        ProcessPotentialPortal(b);
    }

    private void ProcessPotentialPortal(Building b)
    {
        var cacheInfo = b.Map.GetMapInfo<TAE.DynamicDataCacheMapInfo>();
        if (cacheInfo.AtmosphericPassGrid[b.Position] <= 0) return;
        if (!TAE.AtmosphereUtility.IsAtmosphericPortal(b)) return;

        var bRoom = b.GetRoom();
        if (bRoom == null)
        {
            var otherRoom = b.NeighborRoomOf(Room);
            if (otherRoom == null) return;
            var portal = new TAE.AtmosphericPortal(b, this, otherRoom.GetRoomComp<RoomComponent_Atmospheric_Old>());
            portals.Add(portal);
            AtmosphericInfo.Notify_NewPortal(portal);
        }
        else
        {
            var bAtmos = bRoom.GetRoomComp<RoomComponent_Atmospheric_Old>();
            if (bAtmos != null)
            {
                if (bAtmos.Portal?.IsValid ?? false)
                {
                    portals.Add(bAtmos.Portal);
                    AtmosphericInfo.Notify_NewPortal(bAtmos.Portal);
                }
                else
                {
                    var otherRoom = b.NeighborRoomOf(Room);
                    if (otherRoom == null) return;
                    var portal =
                        new TAE.AtmosphericPortal(b, this, otherRoom.GetRoomComp<RoomComponent_Atmospheric_Old>());
                    bAtmos.Noitfy_SetSelfPortal(portal);
                    portals.Add(portal);
                    AtmosphericInfo.Notify_NewPortal(portal);
                }
            }
        }
    }

    public override void PostCreate(RoomTracker parent)
    {
        portals = new List<TAE.AtmosphericPortal>();
        AtmosphericInfo.Notify_NewComp(this);
        renderer = new TAE.RoomOverlay_Atmospheric();
    }

    public override void Disband(RoomTracker parent, Map map)
    {
        AtmosphericInfo.Notify_DisbandedComp(this);
        foreach (var portal in portals) portal.MarkInvalid();
    }

    public override void FinalizeMapInit()
    {
    }

    public override void Init(RoomTracker[] previous = null)
    {
        base.Init(previous);
    }

    public override void PostInit(RoomTracker[] previous)
    {
        CreateContainer();
        MarkDirty();
        Regenerate(previous);

        if (Parent.IsProper)
            TeleUpdateManager.Notify_EnqueueNewSingleAction(() =>
                renderer.UpdateMesh(Room.Cells, Parent.MinVec, Parent.Size.x, Parent.Size.z));
    }

    public override void Notify_Reused()
    {
        base.Notify_Reused();
        portals.Clear();
    }

    public override void Notify_RoofClosed()
    {
        AtmosphericInfo.RegenerateMapInfo();
        Data_CaptureOutsideAtmosphere();
    }

    public override void Notify_RoofOpened()
    {
        Container.Clear();
    }

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

    private void Noitfy_SetSelfPortal(TAE.AtmosphericPortal portal)
    {
        selfPortal = portal;
    }

    private void MarkDirty()
    {
        dirtyMarks = Mathf.Clamp(dirtyMarks + 1, 0, int.MaxValue);
    }

    public bool TryAddValueToRoom(TAE.AtmosphericDef def, float amount, out ValueResult<TAE.AtmosphericDef> result)
    {
        if (!CurrentContainer.TryAddValue(def, amount, out result)) return false;
        Notify_AddedContainerValue(def, result.ActualAmount);
        return true;
    }

    public bool TryRemoveValue(TAE.AtmosphericDef def, float amount, out ValueResult<TAE.AtmosphericDef> result)
    {
        return CurrentContainer.TryRemoveValue(def, amount, out result);
    }

    private void Regenerate(RoomTracker[] previous)
    {
        if (!IsDirty) return;
        container.Notify_RoomChanged(this, Parent.CellCount);

        if (previous != null)
            foreach (var oldTracker in previous)
            {
                var comp = oldTracker.GetRoomComp<RoomComponent_Atmospheric_Old>();
                var container = oldTracker.IsOutside ? comp.OutsideContainer : comp.Container;
                foreach (var value in container.ValueStack)
                {
                    var transferPct = oldTracker.CellCount > Parent.CellCount
                        ? oldTracker.CellCount / (float)Parent.CellCount
                        : 1;
                    Container.TryAddValue(value * transferPct);
                    renderer.TryRegisterNewOverlayPart(value.Def);
                }
            }
    }

    private void Data_CaptureOutsideAtmosphere()
    {
        var outside = OutsideContainer.ValueStack;
        var parts = outside.Length;
        var partSize = Container.Capacity / parts;
        foreach (var value in outside) Container.TryAddValue(value.Def, partSize * OutsideContainer.StoredPercent);
    }

    private void CreateContainer()
    {
        container = new TAE.AtmosphericContainer(this, TAE.AtmosResources.DefaultAtmosConfig(Parent.CellCount));
    }

    public override void CompTick()
    {
    }

    public override void Draw()
    {
    }

    public void Notify_ContainerFull()
    {
    }

    public void Notify_ContainerStateChanged()
    {
    }

    public void Notify_AddedContainerValue(TAE.AtmosphericDef def, float value)
    {
        renderer.TryRegisterNewOverlayPart(def);
    }

    public override string ToString()
    {
        return $"[{Room.ID}]";
    }

    public bool Notify_SpradingGasDissipating(TAE.SpreadingGasTypeDef def, int dissipatedAmount,
        out ValueResult<TAE.AtmosphericDef> actual)
    {
        return TryAddValueToRoom(def.dissipateTo, dissipatedAmount, out actual);
    }
}
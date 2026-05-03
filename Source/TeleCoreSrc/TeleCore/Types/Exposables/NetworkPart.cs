using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

[DebuggerDisplay("{ToString()}")]
public class NetworkPart : INetworkPart, IExposable
{
    [Unsaved] private NetworkIOPartSet? _adjacentSet;

    private NetworkVolume? _cachedVolume;

    [Unsaved] private PipeNetwork? _network;

    [Unsaved] private NetworkIO? _networkIO;

    private float _passThrough = 1; //MUST be initalized with 100%

    internal PipeNetwork Network
    {
        get => _network;
        set
        {
            IsReady = value != null;
            _network = value;
            _network?.Notify_AddPart(this);
        }
    }

    internal NetworkVolume? CachedVolume => _cachedVolume;

    public bool HasVolumeConfig => Config.volumeConfig != null;

    public bool IsPureEdge => IsEdge && !IsJunction;

    public void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving) _cachedVolume = Volume;

        Scribe_Values.Look(ref _passThrough, "passThrough");
        Scribe_Deep.Look(ref _cachedVolume, "cachedVolume");
    }

    [field: Unsaved] public NetworkPartConfig? Config { get; set; }

    [field: Unsaved] public INetworkStructure? Parent { get; set; }

    public Thing Thing => Parent.Thing;


    PipeNetwork INetworkPart.Network
    {
        get => Network;
        set => Network = value;
    }

    public NetworkIO PartIO => _networkIO ?? Parent.GeneralIO;

    public NetworkIOPartSet AdjacentSet => _adjacentSet;
    public NetworkVolume Volume => (Network?.System?.Relations?.TryGetValue(this, out var vol) ?? false ? vol : null)!;

    public bool IsController => (Config.roles | NetworkRole.Controller) == NetworkRole.Controller;
    public bool IsEdge => Config.roles == NetworkRole.Transmitter;
    public bool IsNode => !IsEdge || IsJunction;

    public bool IsJunction => Config.roles == NetworkRole.Transmitter
                              && _adjacentSet.Size > 2
                              && _adjacentSet[NetworkRole.Transmitter]?.Count >= 2;

    public bool HasConnection => _adjacentSet[NetworkRole.Transmitter]?.Count > 0;

    public bool IsReady { get; private set; }

    public bool IsWorking => true;
    public bool IsReceiving { get; }
    public bool HasContainer => Volume != null;
    public bool IsLeaking { get; }
    public float PassThrough => _passThrough;

    public void PostDestroy(DestroyMode mode, Map map)
    {
        //Notify adjacent parts
        foreach (var adjPart in _adjacentSet) adjPart.AdjacentSet.RemoveComponent(this);
    }

    public void Tick()
    {
        if (!IsReady) return;
        if (IsEdge)
        {
            TLog.Warning("Edges should not be ticked!");
            return;
        }

        var isPowered = Parent.IsPowered;
        Parent.NetworkPostTick(this, isPowered);
    }

    public IOConnection IOConnectionTo(INetworkPart other)
    {
        if (other == this) return IOConnection.Invalid;
        if (!Config.networkDef.Equals(other.Config.networkDef))
            return IOConnection.Invalid;
        if (!Parent.CanConnectToOther(other.Parent))
            return IOConnection.Invalid;
        return IOConnection.TryCreate(this, (NetworkPart)other);
    }

    public string InspectString()
    {
        var sb = new StringBuilder();
        if (DebugSettings.godMode)
        {
            sb.AppendLine($"NetworkPart: {Config?.networkDef}");
            sb.AppendLine($"IsController: {IsController}");
            sb.AppendLine($"IsNode: {IsNode}");
            sb.AppendLine($"IsEdge: {IsEdge}");
            sb.AppendLine($"IsJunction: {IsJunction}");
            sb.AppendLine($"IsReady: {IsReady}");
            sb.AppendLine($"IsWorking: {IsWorking}");
            sb.AppendLine($"HasContainer: {HasContainer}");
            sb.AppendLine($"HasConnection: {HasConnection}");
            sb.AppendLine($"IsLeaking: {IsLeaking}");
            sb.AppendLine($"PassThrough: {PassThrough}");
        }

        return sb.ToString().TrimEndNewlines();
    }

    public virtual IEnumerable<Gizmo> GetPartGizmos()
    {
        if (DebugSettings.godMode)
            yield return new Command_Action
            {
                defaultLabel = "Adjancy Set",
                defaultDesc = _adjacentSet.ToString()
            };
        /*if (IsController)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Show Entire Network",
                    action = delegate
                    {
                        DebugNetworkCells = !DebugNetworkCells;
                    }
                };
            }

            if (Network == null) yield break;

            yield return new Command_Action
            {
                defaultLabel = $"Draw Graph",
                action = delegate { Network.DrawInternalGraph = !Network.DrawInternalGraph; }
            };

            yield return new Command_Action
            {
                defaultLabel = $"Draw AdjacencyList",
                action = delegate { Network.DrawAdjacencyList = !Network.DrawAdjacencyList; }
            };


            yield return new Command_Action
            {
                defaultLabel = $"Draw FlowDirections",
                action = delegate { Debug_DrawFlowDir = !Debug_DrawFlowDir; }
            };*/
        if (Network != null)
            foreach (var g in Network.GetGizmos())
                yield return g;
    }

    //Ran after ExposeData
    public void PostLoadInit(NetworkPartConfig config)
    {
        Config = config;
        _adjacentSet = new NetworkIOPartSet(config.networkDef);
        if (config.netIOConfig != null)
            _networkIO = new NetworkIO(config.netIOConfig, Parent.Thing.Position, Parent.Thing.Rotation);
        if (_cachedVolume != null)
            _cachedVolume.PostLoadInit(config.volumeConfig);
    }

    public void PartSetup(bool respawningAfterLoad)
    {
        GetDirectlyAdjacentNetworkParts();
    }

    #region Data

    public void SetPassThrough(float f)
    {
        _passThrough = f;
    }

    #endregion

    #region Helpers

    private void GetDirectlyAdjacentNetworkParts()
    {
        for (var c = 0; c < PartIO.Connections.Count; c++)
        {
            IntVec3 connectionCell = PartIO.Connections[c];
            List<Thing> thingList = connectionCell.GetThingList(Thing.Map);
            for (var i = 0; i < thingList.Count; i++)
            {
                var thing = thingList[i];
                if (thing is not ThingWithComps twc) continue;
                if (PipeNetworkFactory.Fits(twc, Config.networkDef, out var subPart))
                {
                    var result = IOConnectionTo(subPart);
                    var resultReverse = subPart.IOConnectionTo(this);
                    if (result && resultReverse)
                    {
                        _adjacentSet.AddComponent(subPart, result);
                        subPart.AdjacentSet.AddComponent(this, resultReverse);
                    }
                }
            }
        }
    }

    #endregion

    #region Constructors

    public NetworkPart()
    {
    }

    public NetworkPart(INetworkStructure parent)
    {
        Parent = parent;
    }

    //Main creation in Comp_Network with Activator.
    public NetworkPart(INetworkStructure parent, NetworkPartConfig config) : this(parent)
    {
        Config = config;
        _adjacentSet = new NetworkIOPartSet(config.networkDef);
        if (config.netIOConfig != null)
            _networkIO = new NetworkIO(config.netIOConfig, parent.Thing.Position, parent.Thing.Rotation);
    }

    internal void CheckNeighborJunctions()
    {
        foreach (var adjPart in _adjacentSet)
            if (adjPart is NetworkPart part)
                part.CheckIsJunction();
    }

    private void CheckIsJunction()
    {
        if (IsJunction) Thing.Map.TeleCore().NetworkInfo[Config.networkDef].Notify_PartBecameJunction(this);
    }

    public override string ToString()
    {
        return $"{Thing}_{Config.networkDef}";
    }

    #endregion

    #region Rendering

    public void Draw()
    {
        if (Volume == null) return;
        var r = default(GenDraw.FillableBarRequest);
        r.center = Parent.Thing.Position.ToVector3() +
                   new Vector3(0.075f, AltitudeLayer.MetaOverlays.AltitudeFor(), 0.75f);
        r.size = new Vector2(1.5f, 0.15f);
        r.fillPercent = Volume?.FillPercent ?? 0f;
        r.filledMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.green);
        r.unfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.grey);
        r.margin = 0f;
        r.rotation = Rot4.East;
        GenDraw.DrawFillableBar(r);
    }

    public void Print(SectionLayer layer)
    {
        Config.networkDef.TransmitterGraphic?.Print(layer, Thing, 0, this);
    }

    #endregion
}
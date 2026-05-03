using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TR.Info;
using Verse;

namespace TR.Networks;

public abstract class Network : IExposable
{
    protected Map map;

    protected List<IntVec3> networkCells;

    public int NetworkID = -1;

    //
    protected NetworkMaster networkParent;
    protected NetworkRank networkRank;

    protected NetworkType networkType;

    protected NetworkStructureSet structureSet;

    public Network(NetworkType type, Map map, NetworkMaster parent)
    {
        networkParent = parent;
        networkType = type;
        this.map = map;
        structureSet = new NetworkStructureSet();
    }

    //
    public virtual bool IsWorking { get; }
    public virtual float TotalNetworkValue { get; }
    public virtual float TotalStorageNetworkValue { get; }

    public List<IntVec3> NetworkCells => networkCells;

    public NetworkMaster NetworkParent => networkParent;
    public NetworkStructureSet NetworkSet => structureSet;

    public virtual void ExposeData()
    {
    }

    public virtual void Tick()
    {
    }

    public virtual void Draw()
    {
    }

    //
    public bool ValidFor(NetworkRole role, out string reason)
    {
        reason = string.Empty;
        switch (role)
        {
            case NetworkRole.Consumer:
                reason = "TR_ConsumerLack";
                return NetworkSet.FullSet.Any(x =>
                    x.NetworkRole == NetworkRole.Storage || x.NetworkRole == NetworkRole.Producer);
            case NetworkRole.Producer:
                reason = "TR_ProducerLack";
                return NetworkSet.FullSet.Any(x =>
                    x.NetworkRole == NetworkRole.Storage || x.NetworkRole == NetworkRole.Consumer);
            case NetworkRole.Transmitter:
                break;
            case NetworkRole.Storage:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }

        return true;
    }

    public void AddStructure(INetworkStructure structure)
    {
        NetworkSet.AddStructure(structure);
        networkCells.AddRange(structure.ConnectionCells);
    }

    public void RemoveStructure(INetworkStructure structure)
    {
        structureSet.RemoveStructure(structure);
        foreach (var cell in structure.ConnectionCells) networkCells.Remove(cell);
    }

    public void NotifyPotentialSplit(INetworkStructure from)
    {
        from.Network = null;
        Network newNet = null;
        foreach (INetworkStructure root in from.StructureSet.FullSet)
            if (root.Network != newNet)
                newNet = root.Network = new Network(networkType, map);
    }

    public Network RegenerateNetwork(INetworkStructure root)
    {
        var newNet = this;
        var closedSet = new HashSet<INetworkStructure>();
        var openSet = new HashSet<INetworkStructure> { root };
        var currentSet = new HashSet<INetworkStructure>();
        while (openSet.Count > 0)
        {
            foreach (var structure in openSet)
            {
                structure.Network = newNet;
                newNet.AddStructure(structure);
                closedSet.Add(structure);
            }

            var hashSet = currentSet;
            currentSet = openSet;
            openSet = hashSet;
            openSet.Clear();
            foreach (var structure in currentSet)
            foreach (var c in structure.ConnectionCells)
            {
                var thingList = c.GetThingList(map);
                foreach (var thing in thingList)
                {
                    if (!Fits(thing, out var newStructure)) continue;
                    if (!closedSet.Contains(newStructure) && newStructure.ConnectsTo(structure))
                    {
                        map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Buildings);
                        structure.StructureSet.AddNewStructure(newStructure);
                        newStructure.StructureSet.AddNewStructure(structure);
                        openSet.Add(newStructure);
                        break;
                    }
                }
            }
        }

        return newNet;
    }

    private bool Fits(Thing thing, out INetworkStructure structure)
    {
        structure = thing as INetworkStructure;
        structure ??= (thing as ThingWithComps).AllComps.Find(t => t is INetworkStructure) as INetworkStructure;
        return structure != null;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Network of {networkType} with Rank: {networkRank}");
        sb.AppendLine();
        return sb.ToString();
    }
}
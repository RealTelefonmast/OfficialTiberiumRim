using System.Collections.Generic;
using TeleCore.Defs;
using TeleCore.Types.Exposables;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Types;

public class NetworkGraphMaster
{
    //Debug
    internal static bool DEBUG_DrawNetwork = false;
    private readonly NetworkGraph[] lookUpGrid;

    private readonly Map map;
    private readonly BoolGrid validationGrid;
    internal int _MasterID;

    private List<NetworkGraph> allNetworks;

    //
    private NetworkComponent[] unfinishedEdges;

    public NetworkGraphMaster(Map map, NetworkDef networkDef)
    {
        this.map = map;
        NetworkDef = networkDef;
        validationGrid = new BoolGrid(map);
        lookUpGrid = new NetworkGraph[map.cellIndices.NumGridCells];
    }

    public NetworkDef NetworkDef { get; }

    public void RegisterComponent(NetworkComponent netPart, Comp_NetworkStructure netComp)
    {
    }

    public void Deregister(NetworkComponent netPart, Comp_NetworkStructure netComp)
    {
    }

    public void Notify_ThingForNetworkSpawned(Thing thing)
    {
        var netComp = GenData.TryGetComp<Comp_NetworkStructure>(thing);
        if (netComp == null) return;

        foreach (var part in netComp.NetworkParts)
        {
            //Add Node
            if (part.IsNode)
                NodeSpawned(netComp, part);

            //Add Edge
            if (part.IsEdge)
                EdgeSpawned(netComp, part);
        }

        //Try connect to existing graph
        foreach (var connection in netComp.ConnectionCells)
        {
            var graphAt = lookUpGrid[connection.Index(map)];
            if (graphAt == null) continue;

            AddNodeToGraph(graphAt, netComp);
        }

        //Otherwise create Graph
        AddNodeToGraph(new NetworkGraph(), netComp);
    }

    private void NodeSpawned(Comp_NetworkStructure netComp, NetworkComponent part)
    {
        //Add to graph

        //Merge Graph
    }

    private void EdgeSpawned(Comp_NetworkStructure netComp, NetworkComponent part)
    {
        //Add to graph

        //Merge Graph
    }

    public void Notify_ThingForNetworkDespawned(Thing thing)
    {
    }

    //Graphs
    private void AddNodeToGraph(NetworkGraph graph, Comp_NetworkStructure netComp)
    {
        graph.AddNodeFrom(netComp);

        //
        foreach (var cell in netComp.InnerConnectionCells)
        {
            lookUpGrid[cell.Index(map)] = graph;
            validationGrid[cell] = true;
        }
    }
}
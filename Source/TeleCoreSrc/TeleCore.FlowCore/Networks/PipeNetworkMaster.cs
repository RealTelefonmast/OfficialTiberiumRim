using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TeleCore.FlowCore.Flow;
using TeleCore.FlowCore.Graph;
using TeleCore.FlowCore.IO;
using Verse;

namespace TeleCore.FlowCore;

public class DataGraph<TNode, TEdge> : IDisposable where TEdge : IEdge<TNode>
{
    public List<TNode> Nodes { get; private set; }

    public List<TEdge> Edges { get; private set; }

    public Dictionary<TNode, List<(TEdge, TNode)>> AdjacencyList { get; private set; }

    public Dictionary<(TNode, TNode), TEdge> EdgeLookUp { get; private set; }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}

public class DynamicNetworkGraph
{
    private BoolGrid _ioConnGrid;
    private BoolGrid _netGrid;
    private readonly NetworkPartSet _totalPartSet;
    private PipeNetwork _network;
    private Map _map;

    public NetworkGraph Graph => _network.Graph;
    public NetworkFlowSystem FlowSystem => _network.System;
    public NetworkPartSet TotalPartSet => _totalPartSet;

    public DynamicNetworkGraph(NetworkDef def, Map map)
    {
        _map = map;
        _totalPartSet = new NetworkPartSet(def);
        _ioConnGrid = new BoolGrid(map);
        _netGrid = new BoolGrid(map);

        _network = new PipeNetwork(def);
        _network.Prepare();
    }

    #region Data Getters

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PipeNetwork? NetworkAt(IntVec3 c, Map map)
    {
        if (_netGrid[c])
            return _network;
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool? HasNetworkConnectionAt(IntVec3 c, Map map)
    {
        return _netGrid[c];
    }

    #endregion

    #region TickUpdates

    public void Tick(bool shouldTick, int tick)
    {
        _network.TickSystem(tick);
        if (shouldTick)
        {
            _network.Tick(tick);
        }
    }

    public void Draw()
    {
        _network.Draw();

        //Debug
        if (!TeleCoreDebugViewSettings.DrawNetwork) return;
        foreach (var cell in _ioConnGrid.ActiveCells)
        {
            CellRenderer.RenderCell(cell, _network.GetHashCode() / 10000f);
        }
    }

    public void DrawOnGUI()
    {
        _network.OnGUI();
    }

    //Graph generation logic
    //Node spawned
    //  Search in all connected direction for next best node
    //Edge Spawned
    //  Search for next best node, then do as above

    //Case: Graph already has nodes and edges
    //  Search hits junction
    //  

    #endregion

    public void Notify_PartBecameJunction(NetworkPart part)
    {
        TLog.Debug($"Part became junction: {part} | Edge: {part.IsEdge} | Junction: {part.IsJunction} | Node: {part.IsNode}");
    }

    public void Notify_PartSpawned(NetworkPart part)
    {
        //Basic data setup for each spawned part
        if (part.IsNode)
        {
            FlowSystem.Notify_NewNode(part);
        }

        part.Network = _network;
        Graph.AddCells(part);

        foreach (var cell in part.Thing.OccupiedRect())
        {
            _netGrid.Set(cell, true);
        }

        foreach (var visualCell in part.PartIO.VisualCells)
        {
            _ioConnGrid.Set(visualCell, true);
        }

        //Graph Gen
        var isEdge = part.IsEdge; //When node, search all connections to other nodes
        var isNode = part.IsNode; //When edge, seek in two directions to find connected nodes, an edge can ALWAYS only find two connecting nodes
        var isJunction = part.IsJunction;   //Junctions are a special case that act like nodes when spawned as one (spawned inbetween 3 different edges)
                                            //Or are created when an edge is spawned next to another edge with two other previously existing edge neighbors
                                            // 'S' = Spawned Part
                                            // '|' = Array split
                                            // '=' = Edge
                                            // 'N' = Node

        //Edge Creation/Detection
        var newEdges = new List<NetEdge>();
        if (isNode)
        {
            //1. Directly adjacent - [S|N]
            //2. Edge traversal - [S|=|=|N]
            var allEdges = GetAllEdgesFor(part);
            newEdges.AddRange(allEdges);
        }
        else if (isEdge)
        {
            if (part.AdjacentSet.Size > 2)
            {
                TLog.Warning("A pure edge cannot have more than 2 connections!");
            }

            //Junctions are a special case, they are created when an edge is spawned next to another edge with two other previously existing edge neighbors
            foreach (var conn in part.AdjacentSet.Connections)
            {
                var netPart = conn.Key;
                if (netPart.IsJunction && netPart.AdjacentSet.Size == 3)
                {
                    var allEdges = GetAllEdgesFor((NetworkPart)netPart);
                    newEdges.AddRange(allEdges);
                }
            }

            INetworkPart firstNode;
            //Directly adjacent
            //For a single edge connecting two direct nodes there is a bit of trickery involved with the cached I/O data of the network structures
            //We need to know the I/O of both nodes to determine whether the edge is valid or not, and to ensure flow is correct between both nodes
            var foundEdge = false;
            var (firstKey, firstConn) = part.AdjacentSet.Connections.FirstOrFallback(p => p.Key.IsNode);
            if (firstKey != null) //Found a directly attached node, lets find its partner
            {
                firstNode = firstKey;
                var (secondKey, secondConn) =
                    part.AdjacentSet.Connections.FirstOrFallback(p => p.Key.IsNode && p.Key != firstNode);
                if (secondKey != null) //Found another directly connected node, lets connect them
                {
                    newEdges.Add(new NetEdge(firstConn, secondConn, 1));
                    foundEdge = true;
                }
                else //Edge is not between two nodes
                {
                    //We simply search from the known adjacent node into the direction of the spawned edge
                    var edge = FindNextNode(part, firstNode, firstNode.IOConnectionTo(part));
                    if (edge.IsValid)
                    {
                        newEdges.Add(edge);
                        foundEdge = true;
                        //Case: [N|=|=|S|N] OR [N|S|=|=|N]
                    }
                }
            }
            //Search for next node along edge
            if (!foundEdge)
            {
                //Case: [N|=|S|=|N]
                //If no adjecent nodes exist, we search for a node
                var anyEdge = part.AdjacentSet.FirstOrFallback(c => c.IsEdge && !c.IsNode);
                if (anyEdge != null)
                {
                    var (lastEdge, node) = FindNextNode(part, anyEdge);
                    if (node != null)
                    {
                        var edge = FindNextNode(lastEdge, node,
                            node.IOConnectionTo(lastEdge)); //node.PartIO.IOModeAt(rootPos)
                        if (edge.IsValid)
                        {
                            newEdges.Add(edge);
                        }
                    }
                }
            }
        }

        //Junction Pruning
        // Node ------ Node => Node --- Junction --- Node
        foreach (var edge in newEdges)
        {
            var junction = edge.From.IsJunction ? edge.From : (edge.To.IsJunction ? edge.To : null);
            if (junction == null) continue;
            var edges = newEdges.Where(e => !e.Equals(edge) && (e.From == junction || e.To == junction)); ;
            foreach (var otherEdge in edges)
            {
                if (otherEdge.IsValid)
                {
                    var fromAnchor = edge.From == junction ? edge.ToAnchor : edge.FromAnchor;
                    var toAnchor = otherEdge.From == junction ? otherEdge.ToAnchor : otherEdge.FromAnchor;
                    //Note: Not anymore. (obsolete)
                    //FromAnchor must be reversed as it is also the ToAnchor of one of the edges of the junction split
                    Graph.TryDissolveEdge(fromAnchor, toAnchor);
                }
            }
        }

        foreach (var edge in newEdges)
        {
            if (!edge.IsValid)
            {
                TLog.Warning($"Tried to add invalid edge: {edge}");
                continue;
            }
            Graph.AddEdge(edge);
            FlowSystem.Notify_Populate(edge);
        }

        // //Note: Hacky quickfix
        // FlowSystem.Reset();
        // FlowSystem.Notify_Populate(Graph);
    }

    private static IEnumerable<NetEdge> GetAllEdgesFor(NetworkPart rootNode)
    {
        //Directly adjacent - [S|N]
        foreach (var conn in rootNode.AdjacentSet.Connections)
        {
            var directPart = conn.Key;
            if (directPart.IsNode)
            {
                //All directly adjacent nodes get added as infinitely small edges
                if (directPart.IsJunction)
                {
                    TLog.Warning("Connected to direct junction");
                }

                //TODO: Anomalies with mono-directional connections (ie. Output to Input)
                var connResult = rootNode.IOConnectionTo(directPart);
                if (connResult)
                {
                    yield return new NetEdge(connResult, connResult, 0);
                }
            }
            //Edge traversal - [S|=|=|N]
            else if (directPart.IsEdge)
            {
                var nextNode = FindNextNode(directPart, rootNode, conn.Value); //rootNode.PartIO.IOModeAt(directPos)
                if (!nextNode.IsValid) continue;
                yield return nextNode;
            }
        }
    }

    public void Notify_PartDespawned(NetworkPart part)
    {
        FlowSystem.Notify_NetNodeRemoved(part);
        if (!Graph.TryDissolveNode(part))
        {
            if (part.IsEdge)
            {
                //TODO: Ensure only biderictional 'duplicate' edges get cleared, unique one-directional edges (while technically not making much sense)
                //TODO: Should not be removed 
                if (part.AdjacentSet.Size > 0)
                {
                    var begin1 = part.AdjacentSet.FullSet.First();
                    var begin2 = part.AdjacentSet.FullSet.Last();
                    var from = (NetworkPart)FindNextNode(part, begin1).node;
                    var to = (NetworkPart)FindNextNode(part, begin2).node;
                    Graph.TryDissolveEdge(from, to);
                }
            }
        }

        foreach (var cell in part.Thing.OccupiedRect())
        {
            _netGrid.Set(cell, false);
            _ioConnGrid.Set(cell, false);
        }
    }

    private static (INetworkPart lastEdge, INetworkPart node) FindNextNode(NetworkPart origin, INetworkPart direction)
    {
        var currentSet = StaticListHolder<INetworkPart>.RequestSet($"CurrentSubSet3_{origin}", true);
        var openSet = StaticListHolder<INetworkPart>.RequestSet($"OpenSubSet3_{origin}", true);
        var closedSet = StaticListHolder<INetworkPart>.RequestSet($"ClosedSubSet3_{origin}", true);

        openSet.Add(direction);
        do
        {
            foreach (var item in openSet)
            {
                closedSet.Add(item);
            }

            (currentSet, openSet) = (openSet, currentSet);
            openSet.Clear();

            foreach (var part in currentSet)
            {
                foreach (var connPair in part.AdjacentSet.Connections)
                {
                    var newPart = connPair.Key;
                    if (closedSet.Contains(newPart) || newPart == origin) continue;
                    if (newPart.IsNode)
                    {
                        return (part, newPart);
                    }
                    openSet.Add(newPart);
                }
            }

        } while (openSet.Count > 0);
        return (null, null)!;
    }

    private static INetworkPart FindNextNode(INetworkPart rootPart)
    {
        var currentSet = StaticListHolder<INetworkPart>.RequestSet($"CurrentSubSet2_{rootPart}", true);
        var openSet = StaticListHolder<INetworkPart>.RequestSet($"OpenSubSet2_{rootPart}", true);
        var closedSet = StaticListHolder<INetworkPart>.RequestSet($"ClosedSubSet2_{rootPart}", true);

        openSet.Add(rootPart);
        do
        {
            foreach (var item in openSet)
            {
                closedSet.Add(item);
            }

            (currentSet, openSet) = (openSet, currentSet);
            openSet.Clear();

            foreach (var part in currentSet)
            {
                foreach (var connPair in part.AdjacentSet.Connections)
                {
                    var newPart = connPair.Key;
                    if (closedSet.Contains(newPart)) continue;
                    if (newPart.IsNode)
                    {
                        return newPart;
                    }
                    openSet.Add(newPart);
                }
            }

        } while (openSet.Count > 0);
        return null;
    }

    // [Node][Edge]=Search-Direction=>[...][...][OtherNode]
    private static NetEdge FindNextNode(INetworkPart edgePart, INetworkPart firstNode, IOConnection rootIO)
    {
        if (!edgePart.IsEdge)
        {
            TLog.Error($"Trying to search new node with edgePart not being an edge: {edgePart}");
            return NetEdge.Invalid;
        }
        if (!firstNode.IsNode)
        {
            TLog.Error($"Trying to search new node with firstNode not being a node: {firstNode}");
            if (firstNode.IsJunction)
                TLog.Warning(" ^ This error should not have happened!");
            return NetEdge.Invalid;
        }

        var currentSet = StaticListHolder<INetworkPart>.RequestSet($"CurrentSubSet_{edgePart}", true);
        var openSet = StaticListHolder<INetworkPart>.RequestSet($"OpenSubSet_{edgePart}", true);
        var closedSet = StaticListHolder<INetworkPart>.RequestSet($"ClosedSubSet_{edgePart}", true);

        var curLength = 1;
        openSet.Add(edgePart);

        do
        {
            foreach (var item in openSet)
            {
                closedSet.Add(item);
            }

            (currentSet, openSet) = (openSet, currentSet);
            openSet.Clear();

            foreach (var curEdgePart in currentSet)
            {
                foreach (var connPair in curEdgePart.AdjacentSet.Connections)
                {
                    var newPart = connPair.Key;
                    var io = connPair.Value;
                    if (closedSet.Contains(newPart) || newPart == firstNode) continue;

                    //Make Edge When Node Found
                    if (newPart.IsNode)
                    {
                        return new NetEdge(rootIO, io, curLength);
                    }

                    //If Edge, continue search
                    curLength++;
                    openSet.Add(newPart);
                    break;
                }
            }
        }
        while (openSet.Count > 0);

        return NetEdge.Invalid;

    }
}

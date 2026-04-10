using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeleCore.Generics;
using TeleCore.Logging;
using TeleCore.Network.IO;
using TeleCore.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using Verse;

namespace TeleCore.Network.Graph;

[DebuggerDisplay("{Nodes.Count} | {Edges.Count}")]
public class NetworkGraph : IDisposable
{
    public NetworkGraph()
    {
        Nodes = new HashSet<NetNode>();
        AdjacencyList = new Dictionary<NetNode, List<(NetEdge, NetNode)>>();
        UniqueEdges = new Dictionary<TwoWayKey<IOConnection>, NetEdge>();
        EdgesByNodes = new Dictionary<TwoWayKey<NetworkPart>, List<NetEdge>>();
        EdgesByIO = new Dictionary<(NetworkPart, IOCell), NetEdge>();
        //Cells = new List<IntVec3>();
    }

    public HashSet<NetNode> Nodes { get; private set; }
    public Dictionary<NetNode, List<(NetEdge Edge, NetNode Node)>> AdjacencyList { get; private set; }
    public Dictionary<TwoWayKey<IOConnection>, NetEdge> UniqueEdges { get; private set; }
    public Dictionary<(NetworkPart, IOCell), NetEdge> EdgesByIO { get; private set; }
    public Dictionary<TwoWayKey<NetworkPart>, List<NetEdge>> EdgesByNodes { get; set; }
    
    private bool RegisterEdge(NetEdge edge)
    {
        var edgeKey = (edge.FromAnchor, edge.ToAnchor);
        if (UniqueEdges.TryAdd(edgeKey, edge))
        {
            if (EdgesByIO.TryAdd((edge.From, edge.FromIOCell), edge))
            {
                EdgesByIO.TryAdd((edge.To, edge.ToIOCell), edge);   
            }
            else
            {
                TLog.Warning($"Edge with key already exists: {(edge.From, edge.FromIOCell)}");
            }
            
            var nodeKey = new TwoWayKey<NetworkPart>(edge.From, edge.To);
            
            //GetOrMakeEdgeBag
            if (!EdgesByNodes.TryGetValue(nodeKey, out var edges))
            {
                edges = new List<NetEdge>();
                EdgesByNodes.Add(nodeKey, edges);
            }
            
            edges.Add(edge);
            return true;
        }
        return false;
    }

    private void DeregisterEdge(NetEdge edge)
    {
        var edgeKey = (edge.FromAnchor, edge.ToAnchor);
        if (UniqueEdges.Remove(edgeKey))
        {
            var nodeKey = new TwoWayKey<NetworkPart>(edge.From, edge.To);
            EdgesByIO.Remove((edge.From, edge.FromIOCell));
            EdgesByIO.Remove((edge.To, edge.ToIOCell));
            EdgesByNodes.Remove(nodeKey);
        }
    }

    private void ValidateAdded(NetEdge edge)
    {
        // Validate that edge's nodes are part of the graph
        if (!Nodes.Contains(edge.From) || !Nodes.Contains(edge.To))
            throw new InvalidOperationException("Nodes of edge have not been added correctly to the Graph.");

        if (!AdjacencyList.ContainsKey(edge.From) || !AdjacencyList.ContainsKey(edge.To))
            throw new InvalidOperationException("Nodes of edge have not been added correctly to the AdjacencyList. (Missing Keys)");

        if (!AdjacencyList[edge.From].Exists(e => e.Item2.Value == edge.To) ||
            !AdjacencyList[edge.To].Exists(e => e.Item2.Value == edge.From))
            throw new InvalidOperationException("Nodes of edge have not been added correctly to the AdjacencyList. (Missing Values)");

        var edgeKey = (edge.FromAnchor, edge.ToAnchor);

        if (!UniqueEdges.ContainsKey(edgeKey))
            throw new Exception("Edge was not added to UniqueEdges.");

        if (!EdgesByIO.TryGetValue((edge.From, edge.FromIOCell), out var edgeFromIOCell) || !edgeFromIOCell.Equals(edge))
            throw new Exception("Edge was not added to EdgesByIO from side");

        if (!EdgesByIO.TryGetValue((edge.To, edge.ToIOCell), out var edgeToIOCell) || !edgeToIOCell.Equals(edge))
            throw new Exception("Edge was not added to EdgesByIO to side");

        var nodeKey = new TwoWayKey<NetworkPart>(edge.From, edge.To);
        if (!EdgesByNodes.TryGetValue(nodeKey, out var edges) || !edges.Contains(edge))
            throw new Exception("Edge was not added to EdgesByNodes.");
    }

    private void ValidateRemoved(NetEdge edge)
    {
        var edgeKey = (edge.FromAnchor, edge.ToAnchor);
        if (UniqueEdges.ContainsKey(edgeKey) || UniqueEdges.ContainsValue(edge))
            throw new Exception("Edge was not removed from UniqueEdges.");
        
        if (EdgesByIO.TryGetValue((edge.From, edge.FromIOCell), out var edgeFromIOCell) && edgeFromIOCell.Equals(edge))
            throw new Exception("Edge was not removed from EdgesByIO from side");

        if (EdgesByIO.TryGetValue((edge.To, edge.ToIOCell), out var edgeToIOCell) && !edgeToIOCell.Equals(edge))
            throw new Exception("Edge was not removed from to side");
        
        var nodeKey = new TwoWayKey<NetworkPart>(edge.From, edge.To);
        if(EdgesByNodes.ContainsKey(nodeKey) || EdgesByNodes.Values.Any(x => x.Contains(edge)))
            throw new Exception("Edge was not removed from EdgesByNodes.");
        
        if(AdjacencyList.TryGetValue(edge.From, out var list) && list.Exists(e => e.Item2.Value == edge.To))
            throw new Exception("Nodes between edge still exist in AdjacencyList. (From-To)");
        
        if(AdjacencyList.TryGetValue(edge.To, out list) && list.Exists(e => e.Item2.Value == edge.From))
            throw new Exception("Nodes between edge still exist in AdjacencyList. (To-From)");
    }

    public void Dispose()
    {
        //Cells.Clear();
        Nodes.Clear();
        AdjacencyList.Clear();
        EdgesByNodes.Clear();
        UniqueEdges.Clear();

        //Cells = null;
        Nodes = null;
        AdjacencyList = null;
        EdgesByNodes = null;
        UniqueEdges = null;
    }

    public NetEdge GetEdgeOnCell(NetworkPart part, IOCell cell)
    {
        return EdgesByIO.TryGetValue((part, cell), out var edge) ? edge : NetEdge.Invalid;
    }

    public NetEdge GetBestEdgeFor(TwoWayKey<NetworkPart> nodes)
    {
        return EdgesByNodes.TryGetValue(nodes, out var edges) ? edges.FirstOrFallback() : NetEdge.Invalid;
    }

    private static bool EdgeFits(NetEdge edge, IOConnection fromAnchor, IOConnection toAnchor)
    {
        var from = edge.FromAnchor;
        var to = edge.ToAnchor;
        var preResult1 = from.Equals(fromAnchor) && to.Equals(toAnchor);
        var preResult2 = from.Equals(toAnchor) && to.Equals(fromAnchor);
        return preResult1 || preResult2;
    }
    
    public void TryDissolveEdge(IOConnection fromAnchor, IOConnection toAnchor)
    {
        var key = new TwoWayKey<IOConnection>(fromAnchor, toAnchor);
        if (UniqueEdges.TryGetValue(key, out var edge))
        {
            if (EdgeFits(edge, fromAnchor, toAnchor))
            {
                DeregisterEdge(edge);
                if (AdjacencyList.TryGetValue(edge.From, out var fromList))
                {
                    fromList.RemoveAll(e => e.Item2.Value == edge.To);
                }

                if (AdjacencyList.TryGetValue(edge.To, out var toList))
                {
                    toList.RemoveAll(e => e.Item2.Value == edge.From);
                }
                
                //
                ValidateRemoved(edge);
            }
        }
    }
    
    public void TryDissolveEdge(NetworkPart from, NetworkPart to)
    {
        var key = new TwoWayKey<NetworkPart>(from, to);
        if (EdgesByNodes.TryGetValue(key, out var edges))
        {
            foreach (var edge in edges)
            {
                DeregisterEdge(edge);
                if (AdjacencyList.TryGetValue(edge.From, out var fromList))
                {
                    fromList.RemoveAll(e => e.Item2.Value == edge.To);
                }

                if (AdjacencyList.TryGetValue(edge.To, out var toList))
                {
                    toList.RemoveAll(e => e.Item2.Value == edge.From);
                }

                //
                ValidateRemoved(edge);
            }
        }
    }

    public bool TryDissolveNode(NetworkPart node)
    {
        if (!Nodes.Contains(node)) return false;
        
        Nodes.Remove(node);
        if (AdjacencyList.TryGetValue(node, out var list))
        {
            foreach (var (edge, _) in list)
            {
                DeregisterEdge(edge);
            }

            AdjacencyList.Remove(node);
        }

        return true;
    }

    /// <summary>
    /// Returns a list of data tuples, containing the adjacent part and connecting edge.
    /// Warning: The edges are not guaranteed to have the same order as the request. ie: From being the part having adjacency searched for.
    /// </summary>
    public bool TryGetAdjacencyList(INetworkPart forPart, out List<(NetEdge Edge, NetNode Node)> result)
    {
        result = null!;
        if (forPart is NetworkPart part)
        {
            if (AdjacencyList.TryGetValue(part, out result))
                return true;
        }
        return false;
    }
    
    internal void AddCells(INetworkPart netPart)
    {
        foreach (var cell in netPart.Thing.OccupiedRect())
        {
            //Cells.Add(cell);
        }
    }
    
    internal bool AddEdge(NetEdge edge)
    {
        //Ignore invalid edges
        if (!edge.IsValid) return false;

        if (RegisterEdge(edge))
        {
            Nodes.Add(edge.From);
            Nodes.Add(edge.To);

            TryAddAdjacency(edge.From, edge.To, edge);
            TryAddAdjacency(edge.To, edge.From, edge);
            
            //
            ValidateAdded(edge);
        }

        return true;
    }

    private void TryAddAdjacency(NetNode nodeFrom, NetNode nodeTo, NetEdge edge)
    {
        if (!AdjacencyList.TryGetValue(nodeFrom, out var listSource))
        {
            listSource = new List<(NetEdge, NetNode)>()
            {
                (edge, nodeTo)
            };
            AdjacencyList.Add(nodeFrom, listSource);
        }
        else
        {
            listSource.Add((edge, nodeTo));
        }
    }

    internal void Draw()
    {
        //GenDraw.DrawFieldEdges(Cells, Color.cyan);
    }

    internal void OnGUI()
    {
        if (TeleCoreDebugViewSettings.DrawGraphOnGUI)
        {
            DebugTools.Debug_DrawGraphOnUI(this);
        }
    }
}
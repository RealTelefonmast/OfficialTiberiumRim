using System;
using System.Collections.Generic;
using DelaunatorSharp;

namespace TeleCore.Unsorted;

public class Node : INode
{
}

public class Edge : IEdge
{
}

public class Graph : IGraph<Node, Edge>
{
    private readonly HashSet<Node> _nodes;

    private
        public Graph()
    {
        _nodes = new HashSet<Node>();
    }

    public IReadOnlyCollection<Node> Nodes => _nodes;


    public Edge[] GetEdges(Node node)
    {
        throw new NotImplementedException();
    }

    public Node[] GetNeighbors(Node node)
    {
        throw new NotImplementedException();
    }

    public (Edge, Node)[] GetSurrounding(Node node)
    {
        throw new NotImplementedException();
    }
}
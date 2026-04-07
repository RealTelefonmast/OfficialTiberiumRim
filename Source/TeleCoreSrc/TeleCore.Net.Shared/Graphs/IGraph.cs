using System.Collections.Generic;

namespace TeleCore.Net.Shared.Graphs;

public interface IGraph<TVertex, TEdge>
    where TEdge : IEdge<TVertex>
    where TVertex : IVertex
{
    IEnumerable<TVertex> Vertices { get; }
    IEnumerable<TEdge> Edges { get; }

    bool AddVertex(TVertex vertex);
    bool RemoveVertex(TVertex vertex);

    bool AddEdge(TEdge edge);
    bool RemoveEdge(TEdge edge);

    IEnumerable<TVertex> GetNeighbors(TVertex vertex);
    IEnumerable<TEdge> GetEdges(TVertex vertex);
}
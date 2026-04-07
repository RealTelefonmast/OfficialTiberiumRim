namespace TeleCore.Net.Shared.Graphs;

public interface IEdge<out TVertex> where TVertex : IVertex
{
    TVertex Source { get; }
    TVertex Target { get; }
}
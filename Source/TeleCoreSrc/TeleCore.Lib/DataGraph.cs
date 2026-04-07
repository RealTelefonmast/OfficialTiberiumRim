using System.Collections.Generic;

namespace TeleCore.Lib;

public interface INodeEdgeDictionary<TNode, TEdge> : IDictionary<TNode, IList<TEdge>> where TEdge : IDataEdge<TNode>
{
    
}

public interface IDataNode
{
    
}

public interface IDataEdge<TNode>
{
    public TNode NodeA { get; }
    public TNode NodeB { get; }
}

public struct DataNode
{
    
}

public struct DataEdge<TNode> : IDataEdge<TNode>
{
    private TNode _nodeA;
    private TNode _nodeB;
    
    public TNode NodeA { get; }
    public TNode NodeB { get; }
}

public class DataGraph<TNode, TEdge> where TEdge : IDataEdge<TNode>
{
    private Dictionary<TNode, List<TEdge>> _data;
}
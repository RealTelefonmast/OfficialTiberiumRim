using System.Collections.Generic;

namespace TeleCore.Lib.Serialization;

public partial class DataNode
{
    public DataNode? this[int index] => Children[index];

    public DataNode? this[string name]
    {
        get
        {
            for (var i = 0; i < Children.Count; i++)
            {
                var node = Children[i];
                if (node.Name == name) return node;
            }

            return null;
        }
    }

    public BaseNode? this[string name, bool attribute = false]
    {
        get
        {
            if (attribute)
            {
                for (var i = 0; i < Attributes.Count; i++)
                {
                    var subAtt = Attributes[i];
                    if (subAtt.Name == name) return subAtt;
                }
            }
            else
            {
                for (var i = 0; i < Children.Count; i++)
                {
                    var node = Children[i];
                    if (node.Name == name) return node;
                }
            }

            return null;
        }
    }
    
    public IEnumerable<DataNode> GetAllNodes(string tag)
    {
        foreach (var child in Children)
        {
            if (child.Name == tag)
                yield return child;
        }
    }

    public bool HasNode(string tag)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            if (child.Name == tag) return true;
        }
        return false;
    }
    
    public DataNode(string name, string value) : base(name, value)
    {
        Attributes = new List<BaseNode>();
        Children = new List<DataNode>();
    }

    public void AddAttribute(string name, string value)
    {
        Attributes.Add(new BaseNode(name, value));
    }

    public void AddSubnode(DataNode node)
    {
        Children.Add(node);
    }

    public override bool Equals(object? obj)
    {
        if (obj is BaseNode other)
        {
            return Name == other.Name && Value == other.Value;
        }
        return false;
    }

    public bool EqualsByValue(DataNode? other)
    {
        return Value == other?.Value;
    }
    
    public override int GetHashCode()
    {
        //TODO:
        return Attributes.GetHashCode();
        //return HashCode.Combine(Attributes, Children, Name, Value);
    }
}
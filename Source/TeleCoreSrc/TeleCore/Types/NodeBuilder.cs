using System;
using System.Collections.Generic;

namespace TeleCore.Types;

public class NodeBuilder
{
    private readonly List<BaseNode> _attributes;
    private readonly List<DataNode> _children;
    private string _name;
    private string? _value;

    private NodeBuilder()
    {
        _attributes = new List<BaseNode>();
        _children = new List<DataNode>();
    }

    public static NodeBuilder Begin(string rootName, string? value = null)
    {
        var builder = new NodeBuilder();
        builder._name = rootName;
        builder._value = value;
        return builder;
    }

    public NodeBuilder SetName(string name)
    {
        _name = name;
        return this;
    }

    public NodeBuilder SetValue(string value)
    {
        _value = value;
        return this;
    }

    public NodeBuilder AddAttribute(string name, string value)
    {
        _attributes.Add(new BaseNode(name, value));
        return this;
    }

    public NodeBuilder AddSubnode(string name, string value)
    {
        var node = new DataNode(name, value);
        _children.Add(node);
        return this;
    }

    public NodeBuilder AddSubnode(string name, Action<NodeBuilder> node)
    {
        var builder = new NodeBuilder();
        builder._name = name;
        node.Invoke(builder);
        var result = builder.Build();
        _children.Add(result);
        return this;
    }

    public DataNode Build()
    {
        var node = new DataNode(_name, _value);
        foreach (var attribute in _attributes) node.AddAttribute(attribute.Name, attribute.Value);

        foreach (var child in _children) node.AddSubnode(child);

        return node;
    }
}
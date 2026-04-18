using System;

namespace TeleCore.Unsorted;

public enum XmlTagType
{
    Value,
    Object,
    List
}

public enum XmlTagNameSource
{
    Self,
    ParentProperty,
}

[AttributeUsage(AttributeTargets.Class)]
public class XmlParentAttribute : Attribute
{
    private XmlNaming[] _namings;
    
    public XmlParentAttribute(params string[] names)
    {
        _namings = names.Factory(name => new XmlNaming(name));
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class XmlTagAttribute : Attribute
{
    public XmlTagType Type { get; set; }

    public XmlTagAttribute(string name = null, XmlTagType type = XmlTagType.Value, XmlTagNameSource nameSource = XmlTagNameSource.Self)
    {
        Type = type;
    }
}
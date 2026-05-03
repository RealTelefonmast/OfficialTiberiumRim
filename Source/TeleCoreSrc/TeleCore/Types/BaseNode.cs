using System.Diagnostics;
using System.Runtime.Serialization;

namespace TeleCore.Unsorted;

[DebuggerDisplay("{Name}:'{Value}'")]
[DataContract]
public class BaseNode
{
    public BaseNode(string name, string value)
    {
        Name = name;
        Value = value;
    }

    [DataMember] public string Name { get; set; }

    [DataMember] public string Value { get; set; }

    public static implicit operator string(BaseNode node)
    {
        return node.Value;
    }

    //
    public T As<T>()
    {
        return ParseUtility.GeneralParse<T>(Value);
    }
}
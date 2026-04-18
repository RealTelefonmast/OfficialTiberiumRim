using System.Diagnostics;
using System.Runtime.Serialization;

namespace TeleCore.Unsorted;

[DebuggerDisplay("{Name}:'{Value}'")]
[DataContract]
public class BaseNode
{
    [DataMember] 
    public string Name { get; set; }

    [DataMember] 
    public string Value { get; set; }

    public static implicit operator string(BaseNode node) => node.Value;
    
    public BaseNode(string name, string value)
    {
        Name = name;
        Value = value;
    }
    
    //
    public T As<T>()
    {
        return ParseUtility.GeneralParse<T>(Value);
    }
}
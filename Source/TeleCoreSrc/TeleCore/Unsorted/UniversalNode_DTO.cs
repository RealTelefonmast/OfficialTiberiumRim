using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TeleCore.Unsorted;

[DataContract]
public partial class DataNode : BaseNode
{
    [DataMember] 
    public List<BaseNode> Attributes { get; set; }

    [DataMember] 
    public List<DataNode> Children { get; set; }
}
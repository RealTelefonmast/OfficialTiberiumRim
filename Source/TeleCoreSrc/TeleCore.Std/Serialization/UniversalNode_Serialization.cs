using System;
using TeleCore.Lib.Serialization.XML;

namespace TeleCore.Lib.Serialization;

public partial class DataNode
{
    public T ToObject<T>() where T : class
    {
        try
        {
            var xml = UniversalSerializer.GetXmlFromNode(this);
            return XmlBuilder.Deserialize<T>(xml)!;
        }
        catch (Exception ex)
        {
            throw new Exception($"{typeof(T)} cannot be parsed from xml!", ex);
        }
    }
}
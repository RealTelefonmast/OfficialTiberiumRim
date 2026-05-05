using System;
using TeleCore.Types;
using TeleCore.Types.Utils;

namespace TeleCore.UI;

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
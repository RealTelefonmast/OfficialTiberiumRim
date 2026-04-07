using System.Collections.Generic;

namespace TeleCore.Lib.Serialization.XML.CustomTest;

[XmlParent("DataList:StringList")]
public class StringProperty : UnnamedProperty<string>
{
    
}

public abstract class UnnamedProperty<T>
{
    [XmlTag(nameSource: XmlTagNameSource.ParentProperty)]
    public List<T> DataList { get; set; }
}
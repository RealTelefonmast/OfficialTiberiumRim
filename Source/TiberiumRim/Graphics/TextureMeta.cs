using System.Xml.Serialization;
using UnityEngine;

namespace TR;

[XmlRoot("MetaData")]
public class TextureMeta
{
    [XmlElement("WrapMode")] public TextureWrapMode WrapMode { get; set; }
}
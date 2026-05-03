using System.Xml.Serialization;
using UnityEngine;

namespace TeleCore.Unsorted;

[XmlRoot("MetaData")]
public class TextureMeta
{
    [XmlElement("WrapMode")] public TextureWrapMode WrapMode { get; set; }
}
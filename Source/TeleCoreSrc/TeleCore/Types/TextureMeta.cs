using System.Xml.Serialization;
using UnityEngine;

namespace TeleCore.Types;

[XmlRoot("MetaData")]
public class TextureMeta
{
    [XmlElement("WrapMode")] public TextureWrapMode WrapMode { get; set; }
}
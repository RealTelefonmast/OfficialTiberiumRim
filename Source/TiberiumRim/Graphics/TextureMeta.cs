using System.Xml.Serialization;
using UnityEngine;

namespace TR.Graphics;

[XmlRoot("MetaData")]
public class TextureMeta
{
    [XmlElement("WrapMode")] public TextureWrapMode WrapMode { get; set; }
}
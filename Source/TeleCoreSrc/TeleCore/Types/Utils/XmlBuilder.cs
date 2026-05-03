using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TeleCore.Unsorted;

public static class XmlBuilder
{
    public static string SerializeObject<T>(T obj)
    {
        var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = false
        };

        using var stream = new StringWriter();
        using var writer = XmlWriter.Create(stream, settings);
        var serializerMain = new System.Xml.Serialization.XmlSerializer(obj.GetType());
        serializerMain.Serialize(writer, obj, emptyNamespaces);
        return stream.ToString();
    }

    public static TData? Deserialize<TData>(string xmlData, XmlAttributeOverrides? overrides = null)
    {
        try
        {
            var xmlReaderSettings = new XmlReaderSettings
            {
                //NameTable = new NameTable(),
                //ConformanceLevel = ConformanceLevel.Document,
                IgnoreWhitespace = true,
                IgnoreComments = true
            };

            using var stringReader = new StringReader(xmlData);
            using var xmlReader = XmlReader.Create(stringReader, xmlReaderSettings);
            var serial = new System.Xml.Serialization.XmlSerializer(typeof(TData), overrides);
            return (TData)serial.Deserialize(xmlReader);
        }
        catch (Exception e)
        {
            throw new Exception($"Exception: {e.Message}\nCould not serialize xml to {typeof(TData)}!", e);
        }
    }
}
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TeleCore.Unsorted;

public class UniversalSerializer
{
    public static DataNode GetFromXml(string xml)
    {
        var doc = XDocument.Parse(xml);
        var root = doc.Root!;
        var node = NodeFrom(root);
        return node;
    }

    public static DataNode NodeFrom(XElement element)
    {
        var data = new DataNode(element.Name.LocalName, null);

        foreach (var attribute in element.Attributes()) data.AddAttribute(attribute.Name.LocalName, attribute.Value);

        foreach (var node in element.Nodes())
        {
            if (node is XText text) data.Value = text.Value;
            if (node is XElement subElement)
                data.AddSubnode(NodeFrom(subElement));
        }

        return data;
    }

    private static XElement GetElementFromNode(DataNode node)
    {
        var element = new XElement(node.Name);
        if (node.Value != null)
            element.Value = node.Value;

        if (node.Attributes.Count > 0)
            foreach (var attribute in node.Attributes)
                element.SetAttributeValue(attribute.Name, attribute.Value);

        if (node.Children.Count > 0)
            foreach (var child in node.Children)
                element.Add(GetElementFromNode(child));

        return element;
    }

    public static string GetXmlFromNode(DataNode request)
    {
        var doc = new XDocument();
        var root = GetElementFromNode(request);
        doc.Add(root);
        return doc.ToString(SaveOptions.DisableFormatting);
    }

    public static List<T> DeserializeList<T>(DataNode node)
    {
        var xml = GetElementFromNode(node);
        var rootName = (xml.FirstNode as XElement)!.Name.LocalName;

        var list = new List<T>();
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T), new XmlRootAttribute(rootName));
        foreach (var element in xml.Elements())
        {
            using var reader = element.CreateReader();
            var value = (T)serializer.Deserialize(reader)!;
            list.Add(value);
        }

        return list;
    }

    public static T DeserializeNode<T>(DataNode node)
    {
        var xml = GetElementFromNode(node);
        var rootName = xml!.Name.LocalName;

        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T), new XmlRootAttribute(rootName));
        using var reader = xml.CreateReader();
        return (T)serializer.Deserialize(reader)!;
    }
}
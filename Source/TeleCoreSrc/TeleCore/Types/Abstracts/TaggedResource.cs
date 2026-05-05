using System.Xml;
using UnityEngine;
using Verse;

namespace TeleCore.Types.Abstracts;

public abstract class TaggedResource<T>
{
    protected T resource;
    public string resourceData;

    public string resourceTag;

    public T Resource => resource;

    protected virtual T GetResource()
    {
        return default;
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        resourceTag = xmlRoot.Name;
        resourceData = xmlRoot.FirstChild.Value;
        GetResource();
    }
}

public class TextureResource : TaggedResource<Texture2D>
{
    protected override Texture2D GetResource()
    {
        return resource ??= ContentFinder<Texture2D>.Get(resourceData);
    }
}
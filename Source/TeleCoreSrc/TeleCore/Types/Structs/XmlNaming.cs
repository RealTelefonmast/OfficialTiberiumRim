namespace TeleCore.Unsorted;

public readonly struct XmlNaming
{
    private const string XmlTagNamingFormat = "{0}:{1}";

    public string PropertyName { get; }
    public string NameToUse { get; }

    public XmlNaming(string propertyName, string nameToUse)
    {
        PropertyName = propertyName;
        NameToUse = nameToUse;
    }

    public XmlNaming(string formattedNaming)
    {
        var split = formattedNaming.Split(':');
        PropertyName = split[0];
        NameToUse = split[1];
    }

    public static implicit operator string(XmlNaming naming)
    {
        return string.Format(XmlTagNamingFormat, naming.PropertyName, naming.NameToUse);
    }

    public static implicit operator XmlNaming(string naming)
    {
        var split = naming.Split(':');
        return new XmlNaming(split[0], split[1]);
    }
}
using System.Xml;
using Verse;
using OneOf;

namespace TeleCore.RWLib;

/// <summary>
///     Allows loading a simple <see cref="OneOf" /> for int and float.
/// </summary>
public struct OneOfLoadable : ICustomXmlLoadable
{
    public OneOf<int, float> Value { get; set; }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var val = xmlRoot.InnerText;
        var isF = xmlRoot.InnerText.EndsWith("f");
        var valTxt = isF ? val.Substring(0, val.Length - 1) : val;

        if (isF)
            Value = ParseHelper.FromString<float>(valTxt);
        else
            Value = ParseHelper.FromString<int>(valTxt);
    }
}
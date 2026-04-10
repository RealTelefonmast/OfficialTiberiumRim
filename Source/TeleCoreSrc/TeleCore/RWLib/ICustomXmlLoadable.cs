using System.Xml;

namespace TeleCore.RWLib;

/// <summary>
/// A helper interface to ensure the method is correctly implemented.
/// </summary>
public interface ICustomXmlLoadable
{
    public void LoadDataFromXmlCustom(XmlNode xmlRoot);
}
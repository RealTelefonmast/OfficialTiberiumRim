using System.Xml;

namespace TeleCore.Types.Interfaces;

/// <summary>
///     A helper interface to ensure the method is correctly implemented.
/// </summary>
public interface ICustomXmlLoadable
{
    public void LoadDataFromXmlCustom(XmlNode xmlRoot);
}
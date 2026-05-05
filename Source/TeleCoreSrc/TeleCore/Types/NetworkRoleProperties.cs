using System.Collections.Generic;
using System.Xml;
using TeleCore.Defs;
using TeleCore.Types.Enums;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Types;

public class NetworkRoleProperties
{
    public NetworkRole role = NetworkRole.Transmitter;
    public List<NetworkValueDef> subValues;

    public NetworkRoleProperties()
    {
    }

    public NetworkRoleProperties(NetworkRole networkRole)
    {
        role = networkRole;
    }

    public bool HasSubValues => subValues != null;

    public static implicit operator NetworkRole(NetworkRoleProperties props)
    {
        return props.role;
    }

    public static implicit operator NetworkRoleProperties(NetworkRole role)
    {
        return new NetworkRoleProperties(role);
    }

    public bool IsRole(NetworkRole role)
    {
        return this == role;
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        //
        if (xmlRoot.Name != "li")
        {
            TLog.Error("Definition for networkRole not a listing.");
            return;
        }

        if (xmlRoot.ChildNodes.Count == 1)
        {
            role = ParseHelper.FromString<NetworkRole>(xmlRoot.InnerText);
            return;
        }

        role = DirectXmlToObject.ObjectFromXml<NetworkRole>(xmlRoot.SelectSingleNode(nameof(role)), false);
        subValues = DirectXmlToObject.ObjectFromXml<List<NetworkValueDef>>(xmlRoot.SelectSingleNode(nameof(subValues)),
            true);
    }

    public override string ToString()
    {
        return role.ToString();
    }
}
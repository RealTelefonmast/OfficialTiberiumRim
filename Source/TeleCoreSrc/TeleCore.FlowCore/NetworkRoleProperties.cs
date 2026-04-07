using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace TeleCore
{
    public class NetworkRoleProperties
    {
        public NetworkRole role = NetworkRole.Transmitter;
        public List<NetworkValueDef> subValues;

        public bool HasSubValues => subValues != null;

        public NetworkRoleProperties(){}

        public NetworkRoleProperties(NetworkRole networkRole)
        {
            this.role = networkRole;
        }

        public static implicit operator NetworkRole(NetworkRoleProperties props) => props.role;
        public static implicit operator NetworkRoleProperties(NetworkRole role) => new NetworkRoleProperties(role);

        public bool IsRole(NetworkRole role)
        {
            return this == role;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            //
            if (xmlRoot.Name != "li")
            {
                TLog.Error($"Definition for networkRole not a listing.");
                return;
            }

            if (xmlRoot.ChildNodes.Count == 1)
            {
                role = ParseHelper.FromString<NetworkRole>(xmlRoot.InnerText);
                return;
            }

            role = DirectXmlToObject.ObjectFromXml<NetworkRole>(xmlRoot.SelectSingleNode(nameof(role)), false);
            subValues = DirectXmlToObject.ObjectFromXml<List<NetworkValueDef>>(xmlRoot.SelectSingleNode(nameof(subValues)), true);
        }

        public override string ToString()
        {
            return role.ToString();
        }
    }
}

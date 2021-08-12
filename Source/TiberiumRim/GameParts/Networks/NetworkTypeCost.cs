using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class NetworkTypeCost
    {
        public Type enumType;
        public Enum valueType;
        public float cost;

        public bool HasValue => cost > 0;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.Name == "li")
            {
                var parts = xmlRoot.FirstChild.Value.Split('.');
                enumType = (Type) ParseHelper.FromString($"TiberiumRim.{parts[0]}", typeof(Type));
                valueType = (Enum) ParseHelper.FromString(parts[1], enumType);
                
            }
            else
            {
                var parts = xmlRoot.Name.Split('.');
                enumType = (Type)ParseHelper.FromString($"TiberiumRim.{parts[0]}", typeof(Type));
                valueType = (Enum)ParseHelper.FromString(parts[1], enumType);

                cost = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
            }
        }
    }
}

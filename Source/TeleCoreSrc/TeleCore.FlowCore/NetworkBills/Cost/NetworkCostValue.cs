using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace TeleCore
{
    public class NetworkCostValue
    {
        public NetworkValueDef valueDef;
        public float value;

        public bool HasValue => value > 0;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.Name == "li")
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "valueDef", xmlRoot.FirstChild.Value, null, null);
            }
            else
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "valueDef", xmlRoot.Name, null, null);
                value = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
            }
        }
    }
}

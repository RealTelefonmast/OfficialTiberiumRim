using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class TiberiumTypeCost
    {
        public TiberiumValueType valueType = TiberiumValueType.None;
        public float cost;

        public TiberiumTypeCost()
        {
        }

        public bool HasValue => cost > 0;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.Name == "li")
            {
                valueType = (TiberiumValueType)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(TiberiumValueType));
            }
            else
            {
                valueType = (TiberiumValueType)ParseHelper.FromString(xmlRoot.Name, typeof(TiberiumValueType));
                cost = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
            }
        }
    }
}

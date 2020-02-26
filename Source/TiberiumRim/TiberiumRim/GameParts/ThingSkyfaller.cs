using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class ThingSkyfaller
    {
        public ThingDef innerThing;
        public ThingDef skyfallerDef;
        public int amount = 1;
        public float chance = 1f;

        public ThingSkyfaller()
        {
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skyfallerDef", xmlRoot.Name);
            string Child = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = Child.Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "innerThing", array[0]);
            if (array.Count() == 2)
            {
                amount = (int)ParseHelper.FromString(array[1], typeof(int));
            }
            if (array.Count() == 3)
            {
                chance = (float)ParseHelper.FromString(array[2], typeof(float));
            }
        }
    }
}

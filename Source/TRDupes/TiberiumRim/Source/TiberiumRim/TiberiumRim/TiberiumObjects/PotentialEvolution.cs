using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class PotentialEvolution
    {
        public TiberiumProducerDef evolvedDef;
        public float chance = 0f;
        public int days = 1;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.Name == "li")
            {

                return;
            }
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "evolvedDef", xmlRoot.Name);
            string[] parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
            chance = ParseHelper.FromString<float>(parts[0]);
            if (parts.Length == 3)
                days = ParseHelper.FromString<int>(parts[1]);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using Verse;
using  UnityEngine;

namespace TiberiumRim
{
    public class TiberiumTerrainDef : TerrainDef
    {
        public bool isDead = false;
        public float plantChanceFactor = 1f;
        public float daysToNaturalCleanse = 20f;
        public TerrainDef decrystallized;

        public List<Color> colorSpectrum = new List<Color>();
        public List<TerrainSupport> terrainSupport = new List<TerrainSupport>();
        public List<ThingProbability> plantSupport = new List<ThingProbability>();

        public TerrainSupport TerrainSupportFor(TerrainDef def)
        {
            return terrainSupport.Find(s => s.TerrainTag.SupportsDef(def));
        }

        public bool SupportsPlant(ThingDef plant)
        {
            return plantSupport.Any(p => p.thing == plant);
        }

        public ThingDef TiberiumPlantFor()
        {
            foreach (var ps in plantSupport)
            {
                if (TRUtils.Chance(ps.probability))
                    return ps.thing;
            }
            return null;
        }
    }

    public class ThingProbability
    {
        public ThingDef thing;
        public float probability;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string[] parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", parts[0]);
            probability = ParseHelper.ParseFloat(parts[1]);
        }
    }
}

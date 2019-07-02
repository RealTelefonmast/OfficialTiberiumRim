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
        public List<TiberiumPlantRecord> plantSupport = new List<TiberiumPlantRecord>();

        public TerrainSupport TerrainSupportFor(TerrainDef def)
        {
            return terrainSupport.Find(s => s.TerrainTag.SupportsDef(def));
        }

        public bool SupportsPlant(ThingDef plant)
        {
            return plantSupport.Any(p => p.plant == plant);
        }

        public ThingDef TiberiumPlantFor()
        {
            foreach (var ps in plantSupport)
            {
                if (TRUtils.Chance(ps.chance))
                    return ps.plant;
            }
            return null;
        }
    }

    public class TiberiumPlantRecord
    {
        public ThingDef plant;
        public float chance;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string[] parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "plant", parts[0]);
            chance = ParseHelper.ParseFloat(parts[1]);
        }
    }
}

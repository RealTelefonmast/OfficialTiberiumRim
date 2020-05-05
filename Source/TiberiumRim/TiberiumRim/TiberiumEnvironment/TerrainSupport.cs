using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumConversion
    {
        public string fromTerrainDefName;
        public TerrainDef toTerrainDef;
        public TiberiumCrystalDef toCrystalDef;

        public TerrainDef FromTerrain => DefDatabase<TerrainDef>.GetNamedSilentFail(fromTerrainDefName);
        public TerrainFilterDef FromTerrainGroup => DefDatabase<TerrainFilterDef>.GetNamedSilentFail(fromTerrainDefName);

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string[] array = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "").Split(',');
            fromTerrainDefName = array[0];
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "toTerrainDef", array[1], null, null);
            if(array.Length > 2)
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "toCrystalDef", array[2], null, null);
        }

        public bool TerrainContained(TerrainDef def)
        {
            if (FromTerrain != null)
                return FromTerrain == def;

            return FromTerrainGroup.AllowsTerrainDef(def);
        }
    }

    public class FloraConversion
    {
        public string fromPlantDefName;
        public ThingDef fromPlant => DefDatabase<ThingDef>.GetNamedSilentFail(fromPlantDefName);
        public ThingFilterDef fromPlantGroup => DefDatabase<ThingFilterDef>.GetNamedSilentFail(fromPlantDefName);

        public ThingOptionDef toPlantOption;
        public TerrainOptionDef toTerrainOption;

        public bool PlantContained(ThingDef def)
        {
            if (fromPlant != null)
                return fromPlant == def;
            return fromPlantGroup.Allows(def);
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string[] array = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "").Split(',');
            fromPlantDefName = array[0];
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "toPlantOption",array[1], null, null);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "toTerrainOption", array[2], null, null);
        }
    }

    public class TerrainOptionDef
    {
        public List<TerrainFloat> options;

        public TerrainDef SelectRandomOptionByChance()
        {
            return options.First(t => TRUtils.Chance(t.value)).terrainDef;
        }

        public TerrainDef SelectRandomOptionByWeight()
        {
            return options.RandomElementByWeight(t => t.value).terrainDef;
        }
    }

    public class TerrainFilterDef : Def
    {
        public List<TerrainDef> acceptedTerrain;
        public List<string> acceptedTags;

        public bool AllowsTerrainDef(TerrainDef def)
        {
            string defName = def.defName.ToLower();
            if (!acceptedTags.NullOrEmpty() && !acceptedTags.Any(t => defName.Contains(t)))
                return false;
            if (!acceptedTerrain.NullOrEmpty() && !acceptedTerrain.Contains(def))
                return false;
            return true;
        }
    }
}

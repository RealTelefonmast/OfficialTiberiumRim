using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using Verse;

namespace TR
{
    public class TiberiumTerrainDef : TerrainDef
    {
        public bool isDead = false;
        public float plantChanceFactor = 1f;
        public float daysToNaturalCleanse = 20f;
        public TerrainDef decrystallized;
        public TiberiumCrystalDef mainType;

        public List<Color> colorSpectrum = new List<Color>();
        //public List<TerrainSupport> terrainSupport = new List<TerrainSupport>();
        public List<TerrainFilterDef> allowedTerrain;
        public List<DefFloat<ThingDef>> plantSupport = new ();
        
        //TODO: Re-Add AllowedTerrain filter to directly spawn terrain instead of using spreadoutcome
        public bool TryCreateOn(IntVec3 pos, Map map, out TiberiumTerrainDef outTerrain)
        {
            TerrainDef topTerrain = null;
            TerrainDef underTerrain = null;
            mainType?.GetTiberiumOutcomesAt(pos, map, out topTerrain, out underTerrain, out TiberiumCrystalDef _);
            outTerrain = (TiberiumTerrainDef)topTerrain;
            if (!(topTerrain != null || underTerrain != null)) return false;
            GenTiberium.SetTerrain(pos, map, topTerrain, underTerrain);
            return true;
        }
        
        public bool AllowedOn(TerrainDef terrain)
        {
            return allowedTerrain.Any(t => t.Allows(terrain));
        }

        public bool SupportsTerrain(TerrainDef terrain)
        {
            return mainType.HasOutcomesFor(terrain);
        }

        public bool SupportsTerrainAt(IntVec3 pos, Map map)
        {
            return mainType.HasOutcomesAt(pos, map);
        }

        public bool SupportsPlant(ThingDef plant)
        {
            return plantSupport.Any(p => p.def == plant);
        }

        public ThingDef TiberiumPlantFor()
        {
            foreach (var ps in plantSupport)
            {
                if (TRandom.Chance(ps.value))
                    return ps.def;
            }
            return null;
        }
    }

    /*
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
    */
}

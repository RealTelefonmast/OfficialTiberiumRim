using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public class TiberiumConversion
    {
        [NoTranslate]
        private string fromTerrain;

        private TerrainDef terrainDefInt;
        private TerrainFilterDef filterDefInt;

        public bool isTopLayer = false;
        public TerrainDef toTerrain;
        public List<DefFloat<TiberiumCrystalDef>> toCrystal = new();

        public TerrainDef FromTerrain => terrainDefInt ??= DefDatabase<TerrainDef>.GetNamedSilentFail(fromTerrain);
        public TerrainFilterDef FromTerrainGroup => filterDefInt ??= DefDatabase<TerrainFilterDef>.GetNamedSilentFail(fromTerrain);

        public void GetOutcomes(out TiberiumCrystalDef crystalDef, out TerrainDef terrainDef, out bool isTopLayer)
        {
            crystalDef = toCrystal.RandomElementByWeight(t => t.value).def;
            terrainDef = toTerrain;
            isTopLayer = this.isTopLayer;
        }

        public bool HasOutcomesFor(TerrainDef def)
        {
            if (def == FromTerrain) return true;
            return FromTerrainGroup?.Allows(def) ?? false;
        }


        // TerrainDef -> TerrainDef , TiberiumCrystalDef1 : weight | TiberiumCrystalDef2 : weight |...
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string[] arr1 = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "").Split(new[] { "->" }, StringSplitOptions.None); //Split(',');
            string[] arr2 = arr1[1].Split(',');
            string[] arr3 = arr2[1].Split('|');

            fromTerrain = arr1[0];

            //Get Terrain Outcome
            string[] terrainArr = arr2[0].Split(':');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "toTerrain", terrainArr[0], null, null);
            if (terrainArr.Length > 1)
                isTopLayer = ParseHelper.ParseBool(terrainArr[1]);

            //Get Crystal Outcomes
            foreach (var s in arr3)
            {
                string[] parts = s.Split(':');
                float val = parts.Length > 1 ? ParseHelper.ParseFloat(parts[1]) : 1f;
                toCrystal.Add(new DefFloat<TiberiumCrystalDef>(parts[0], val));
            }
        }
        
    }
}

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
    public class TiberiumConversion
    {
        [NoTranslate]
        private string fromTerrain;

        public bool isTopLayer = false;
        public TerrainDef toTerrain;
        public List<WeightedThing> toCrystal = new List<WeightedThing>();

        public TerrainDef FromTerrain => DefDatabase<TerrainDef>.GetNamedSilentFail(fromTerrain);
        public TerrainFilterDef FromTerrainGroup => DefDatabase<TerrainFilterDef>.GetNamedSilentFail(fromTerrain);

        public void GetOutcomes(out TiberiumCrystalDef crystalDef, out TerrainDef terrainDef, out bool isTopLayer)
        {
            crystalDef = (TiberiumCrystalDef)toCrystal.RandomElementByWeight(t => t.weight).thing;
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
                toCrystal.Add(new WeightedThing(parts[0], val));
            }
        }
        
    }
}

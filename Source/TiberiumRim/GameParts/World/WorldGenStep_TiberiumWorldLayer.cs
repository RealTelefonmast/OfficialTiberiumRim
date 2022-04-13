using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace TiberiumRim
{
    public class WorldGenStep_TiberiumWorldLayer : WorldGenStep
    {
        public override int SeedPart => 7809234;

        private World World => Find.World;
        private WorldComponent_TR WorldComp => TRUtils.Tiberium();
        private TiberiumWorldInfo Tiberium => TRUtils.Tiberium().TiberiumInfo;

        public override void GenerateFresh(string seed)
        {
            float coverage = TiberiumSettings.Settings.tiberiumCoverage;
            if (coverage <= 0) return;
            ModuleBase noiseTest = new Perlin((double)(0.035f * coverage), 2.0, 0.4, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            //var test = 
            
            int tilesCount = Find.WorldGrid.TilesCount;
            for (int i = 0; i < tilesCount; i++)
            {
                var pos = Find.WorldGrid.GetTileCenter(i);
                var level = noiseTest.GetValue(pos);
                int value = (int)(level * 1000);
                Tiberium.AdjustTiberiumLevelAt(i, value);
            }
        }


    }
}

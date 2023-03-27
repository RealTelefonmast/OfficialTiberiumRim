using RimWorld.Planet;
using TeleCore;
using UnityEngine;
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
            float min = 1 - coverage;
            ModuleBase noise = new Perlin(0.035f, 1.6, 0.6, 6, seed.GetHashCode(), QualityMode.High);
            RidgedMultifractal fractal = new RidgedMultifractal(0.01f, 2, 6, seed.GetHashCode(), QualityMode.High);
            Add addResult = new Add(noise, fractal);
            var result = new Subtract(new Clamp(min, 1, addResult), new Const(min));
            //var test = 
            
            int tilesCount = Find.WorldGrid.TilesCount;
            for (int i = 0; i < tilesCount; i++)
            {
                var pos = Find.WorldGrid.GetTileCenter(i);
                var level = result.GetValue(pos);
                int value = (int)(level * 1000);
                Tiberium.AdjustTiberiumLevelAt(i, value);
            }
        }


    }
}

using Verse;
using Verse.Noise;

namespace TiberiumRim;

public class MapGenStep_Tiberium : GenStep
{
    public override int SeedPart { get; }
    
    public override void Generate(Map map, GenStepParams parms)
    {
        var tibCoverage = TRUtils.Tiberium().TiberiumInfo.CoverageAt(map.Tile);
        if (tibCoverage <= 0) return;
        var perlin = new Perlin(0.5, 2, 1, 6, map.ConstantRandSeed, QualityMode.High);

    }
}
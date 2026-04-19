using System.Collections.Generic;
using TiberiumRim;
using Verse;

namespace TR;

public class TerrainData
{
    public List<TiberiumCrystalDef> supportedCrystals;

    public bool supportsFlora = false;

    //Full Info for a terrainOptions's use
    public TerrainDef terrain;
}
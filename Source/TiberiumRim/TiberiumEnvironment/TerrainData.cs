using System.Collections.Generic;
using TR.TiberiumObjects;
using Verse;

namespace TR.TiberiumEnvironment;

public class TerrainData
{
    public List<TiberiumCrystalDef> supportedCrystals;

    public bool supportsFlora = false;

    //Full Info for a terrainOptions's use
    public TerrainDef terrain;
}
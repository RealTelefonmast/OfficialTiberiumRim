namespace TR;

public struct TiberiumTerrain
{
    public int crystalDefID;
    public TerrainType type;

    public TiberiumTerrain(TiberiumCrystalDef crystalDef, TerrainType terrainType)
    {
        crystalDefID = crystalDef.IDReference;
        type = terrainType;
    }
}
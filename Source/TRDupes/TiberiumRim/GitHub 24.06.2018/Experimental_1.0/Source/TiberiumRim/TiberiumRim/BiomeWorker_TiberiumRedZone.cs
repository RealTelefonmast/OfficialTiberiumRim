using RimWorld.Planet;
using RimWorld;

namespace TiberiumRim
{
    public class BiomeWorker_TiberiumRedZone : BiomeWorker
    {
        public override float GetScore(Tile tile)
        {
            if (tile.WaterCovered)
            {
                return -100f;
            }
            if (tile.temperature < 15f)
            {
                return 0f;
            }
            if (tile.rainfall < 2000f)
            {
                return 0f;
            }
            return 0;
        }
    }
}

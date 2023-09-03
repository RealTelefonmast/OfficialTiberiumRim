using RimWorld.Planet;
using Verse;

namespace TR
{
    public class GroundZero : TiberiumTile, IGroundZero
    {
        protected TiberiumCrater mainCrater;

        public int TileInt => this.Tile;
        public Thing GZThing => mainCrater;
        public bool IsGroundZero { get; }
        public LocalTargetInfo LocalTarget { get; set; }
        public GlobalTargetInfo GlobalTarget { get; set; }

        public void PassOnGZTitle()
        {
            return;
        }
    }
}

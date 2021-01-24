using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;

namespace TiberiumRim
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

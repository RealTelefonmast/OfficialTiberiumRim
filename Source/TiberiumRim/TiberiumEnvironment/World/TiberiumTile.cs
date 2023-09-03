using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace TR
{
    public class TiberiumTile : WorldObject
    {
        protected float coverageInt = 0;

        public Map Map => Find.World.worldObjects.WorldObjectAt<MapParent>(Tile)?.Map;
        public bool HasMap => Map != null;

        public float Coverage
        {
            get { return HasMap ? Map.Tiberium().TiberiumInfo.InfestationPercent : coverageInt; }
            set => coverageInt = value;
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Coverage: " + Coverage);
            return sb.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref coverageInt, "tiberiumCoverage");

        }

        public bool shouldSpawnNeighour = true;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            //Find.World.renderer

            Log.Message("World Tile: " + this.Tile);
            if (shouldSpawnNeighour)
            {
                List<int> tempNeighbors = new List<int>();
                Find.WorldGrid.GetTileNeighbors(Tile, tempNeighbors);
                for (int i = 0; i < tempNeighbors.Count; i++)
                {
                    TiberiumTile tibTile = (TiberiumTile)WorldObjectMaker.MakeWorldObject(TiberiumDefOf.TiberiumTile);
                    tibTile.Tile = tempNeighbors[i];
                    tibTile.shouldSpawnNeighour = false;
                    tibTile.coverageInt = Rand.Range(0.25f, 1);
                    Find.World.worldObjects.Add(tibTile);
                }
            }

        }

        private float InfestationPerDay()
        {
            return 0;
        }

        public override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(250))
            {
                Find.World.renderer.SetDirty<WorldLayer_Tiberium>();
            }
            //Update Infestation
            if (!HasMap && this.IsHashIntervalTick(10))
            {
                if(Coverage < 1f)
                    Coverage += 0.01f;
                if (Coverage >= 0.45f && !hasSpread)
                    Spread();
            }
        }

        private bool hasSpread = false;
        private void Spread()
        {
            List<int> tempNeighbors = new List<int>();
            Find.WorldGrid.GetTileNeighbors(Tile, tempNeighbors);
            for (int i = 0; i < tempNeighbors.Count; i++)
            {
                if (Find.World.worldObjects.WorldObjectAt<TiberiumTile>(tempNeighbors[i]) != null) continue;
                TiberiumTile tibTile = (TiberiumTile)WorldObjectMaker.MakeWorldObject(TiberiumDefOf.TiberiumTile);
                tibTile.Tile = tempNeighbors[i];
                tibTile.shouldSpawnNeighour = false;
                tibTile.coverageInt = 0.01f;
                Find.World.worldObjects.Add(tibTile);
            }

            hasSpread = true;
        }
    }
}

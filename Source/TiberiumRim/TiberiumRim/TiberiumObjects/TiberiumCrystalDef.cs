using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TiberiumCrystalDef : TRThingDef
    {
        public TiberiumCrystalProperties tiberium;
        //Corruptions
        public ThingDef monolith;
        public ThingDef rock;
        public ThingDef wall;
        public ThingDef chunk;
        //Terrain
        public TerrainDef dead;
        public List<TerrainSupport> supportsTerrain = new List<TerrainSupport>();

        public TiberiumCrystalDef() : base()
        {
        }

        public TerrainSupport TerrainSupportFor(TerrainDef def)
        {
            return supportsTerrain.Find(s => s.TerrainTag.SupportsDef(def));
        }

        public TiberiumValueType TiberiumValueType => tiberium.type;

        public HarvestType HarvestType
        {
            get
            {
                if (TiberiumValueType == TiberiumValueType.Unharvestable)
                {
                    return HarvestType.Unharvestable;
                }
                if (TiberiumValueType == TiberiumValueType.Sludge)
                {
                    return HarvestType.Unvaluable;
                }
                return HarvestType.Valuable;
            }
        }

        public bool IsInfective => tiberium.infects;
        public bool IsMoss => HarvestType == HarvestType.Unvaluable;
    }
}

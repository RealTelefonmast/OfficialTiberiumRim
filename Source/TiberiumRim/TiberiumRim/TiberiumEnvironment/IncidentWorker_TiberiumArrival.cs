using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class IncidentWorker_TiberiumArrival : IncidentWorker
    {
        public TiberiumIncidentDef Def
        {
            get
            {
                return base.def as TiberiumIncidentDef;
            }
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (CanFireNowSub(parms))
            {               
                Map map = (Map)parms.target;
                if (TryFindCell(out IntVec3 cell, map))
                {
                    TiberiumProducer producer = ThingMaker.MakeThing(Def.producerDef) as TiberiumProducer;
                    Skyfaller skyfaller = SkyfallerMaker.SpawnSkyfaller(Def.skyfallerDef, producer, cell, map);
                    //Skyfaller faller = SkyfallerMaker.SpawnSkyfaller(Def.skyfallerDef, Def.producerDef, cell, map);
                    SendStandardLetter(producer);
                    return true;
                }            
            }
            return false;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms);
        }

        private bool TryFindCell(out IntVec3 cell, Map map)
        {
            return CellFinderLoose.TryFindSkyfallerCell(Def.producerDef, map, out cell, 20, default(IntVec3), -1, true, true, false, false, false, false, delegate (IntVec3 x)
                    {
                        if (x.InBounds(map))
                        {
                            TiberiumCrystal crystal = x.GetTiberium(map);
                            TerrainDef terrain = x.GetTerrain(map);
                            bool tryRiver = !map.TileInfo.Rivers.NullOrEmpty();
                            List<Thing> things = map.listerThings.ThingsOfDef(Def.producerDef);
                            float min = 99999f;
                            if (!things.NullOrEmpty())
                            {
                                min = map.listerThings.ThingsOfDef(Def.producerDef).Min(d => d.Position.DistanceTo(x));
                            }

                            if(min < 46f)
                            {
                                return false;
                            }
                            if (crystal != null)
                            {
                                return false;
                            }
                            if (terrain?.affordances.Contains(DefDatabase<TerrainAffordanceDef>.GetNamed("Bridgeable")) ?? false)
                            {
                                return false;
                            }
                            if (x.Fogged(map))
                            {
                                return false;
                            }
                            if (GenAdj.CellsOccupiedBy(x, Rot4.North, new IntVec2(4, 4)).Any(c => !c.Standable(map)))
                            {
                                return false;
                            }
                            MapComponent_TiberiumWater river = map.GetComponent<MapComponent_TiberiumWater>();
                            if (tryRiver && (!river.RiverCells.Any(c => c.InHorDistOf(x, 10f)) || river.RiverCells.Any(c => c.DistanceTo(x) < 5f)))
                            {
                                return false;
                            }
                            return true;
                        }
                        return false;
                    });
        }
    }
}

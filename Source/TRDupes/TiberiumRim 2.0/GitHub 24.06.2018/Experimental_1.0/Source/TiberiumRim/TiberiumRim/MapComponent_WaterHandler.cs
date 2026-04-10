using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class MapComponent_WaterHandler : MapComponent
    {
        private Dictionary<IntVec3, bool> MarshList = new Dictionary<IntVec3, bool>();
        private Dictionary<IntVec3, bool> ShallowList = new Dictionary<IntVec3, bool>();
        private Dictionary<IntVec3, bool> OceanShallowList = new Dictionary<IntVec3, bool>();
        public HashSet<IntVec3> affectedCells = new HashSet<IntVec3>();

        public MapComponent_WaterHandler(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref MarshList, "MarshList");
            Scribe_Collections.Look(ref ShallowList, "ShallowList");
            Scribe_Collections.Look(ref OceanShallowList, "OceanShallowList");
            base.ExposeData();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % GenTicks.TickLongInterval == 0)
            {
                if (TotalList?.Count > 0)
                {
                    foreach (IntVec3 c in TotalList)
                    {
                        if (!affectedCells.Contains(c))
                        {
                            Room r = c.GetRoom(map);
                            if (r.Temperature > 0)
                            {
                                if (Rand.Chance(0.01f * r.Temperature))
                                {
                                    if (MarshList.ContainsKey(c))
                                    {
                                        if (!MarshList[c])
                                        {
                                            map.terrainGrid.SetTerrain(c, DefDatabase<TerrainDef>.GetNamed("Marsh"));
                                            MarshList.Remove(c);
                                        }
                                    }
                                    if (ShallowList.ContainsKey(c))
                                    {
                                        if (!ShallowList[c])
                                        {
                                            map.terrainGrid.SetTerrain(c, TerrainDefOf.WaterShallow);
                                            ShallowList.Remove(c);
                                        }
                                    }
                                    if (OceanShallowList.ContainsKey(c))
                                    {
                                        if (!OceanShallowList[c])
                                        {
                                            map.terrainGrid.SetTerrain(c, TerrainDefOf.WaterOceanShallow);
                                            OceanShallowList.Remove(c);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public HashSet<IntVec3> TotalList
        {
            get
            {
                HashSet<IntVec3> tmp = new HashSet<IntVec3>();
                foreach (IntVec3 c in MarshList.Keys)
                {
                    if (c != null)
                    {
                        tmp.Add(c);
                    }
                }
                foreach (IntVec3 c in ShallowList.Keys)
                {
                    if (c != null)
                    {
                        tmp.Add(c);
                    }
                }
                foreach (IntVec3 c in OceanShallowList.Keys)
                {
                    if (c != null)
                    {
                        tmp.Add(c);
                    }
                }
                return tmp;
            }
        }

        public void SetFalse(HashSet<IntVec3> set)
        {
            List<IntVec3> m = MarshList.Keys.ToList();
            for (int i = 0; i < m.Count(); i++)
            {
                IntVec3 c = m[i];
                if (set.Contains(c))
                {
                    MarshList[c] = false;
                }
            }
            List<IntVec3> s = ShallowList.Keys.ToList();
            for (int i = 0; i < s.Count(); i++)
            {
                IntVec3 c = s[i];
                if (set.Contains(c))
                {
                    ShallowList[c] = false;
                }
            }
            List<IntVec3> os = OceanShallowList.Keys.ToList();
            for (int i = 0; i < os.Count(); i++)
            {
                IntVec3 c = os[i];
                if (set.Contains(c))
                {
                    OceanShallowList[c] = false;
                }
            }
        }

        public bool IsAffected(IntVec3 c)
        {
            if (TotalList.Contains(c))
            {
                return true;
            }
            return false;
        }

        //--
        public void AddMarsh(IntVec3 c)
        {
            if (!MarshList.Keys.Contains(c))
            {
                MarshList.Add(c, true);
            }
            else if (!MarshList[c])
            {
                MarshList[c] = true;
            }
        }

        public void AddShallow(IntVec3 c)
        {
            if (!ShallowList.Keys.Contains(c))
            {
                ShallowList.Add(c, true);
            }
            else if (!ShallowList[c])
            {
                ShallowList[c] = true;
            }
        }

        public void AddOceanShallow(IntVec3 c)
        {
            if (!OceanShallowList.Keys.Contains(c))
            {
                OceanShallowList.Add(c, true);
            }
            else if (!OceanShallowList[c])
            {
                OceanShallowList[c] = true;
            }
        }
    }
}

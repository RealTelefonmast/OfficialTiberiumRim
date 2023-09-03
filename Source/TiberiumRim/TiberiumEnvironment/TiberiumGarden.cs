using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TR
{
    public class TiberiumGarden
    {
        private Map map;
        private CellArea cells;
        private TiberiumBlossom blossomParent;
        private List<TiberiumPlant> tiberiumFlora = new List<TiberiumPlant>();

        public TiberiumGarden(Map map)
        {
            this.map = map;
            cells = new CellArea(map);
        }

        public TiberiumGarden(Map map, List<IntVec3> cells)
        {
            this.map = map;
            this.cells = new CellArea(map);
            this.cells.AddRange(cells);
        }

        private int cycleIndex;
        private bool hasGardenNumDesiredPlantsCalculated;

        private float calculatedGardenNumDesiredPlants;
        private float calculatedGardenNumDesiredPlantsTmp;
        private int calculatedGardenNumNonZeroFertilityCells;
        private int calculatedGardenNumNonZeroFertilityCellsTmp;
        private float? cachedCavePlantsCommonalitiesSum;

        public float CachedChanceFromDensity
        {
            get
            {
                CacheGardenNumDesiredPlants();
                return calculatedGardenNumDesiredPlants / (float)calculatedGardenNumNonZeroFertilityCells;
            }
        }

        public float CurrentPlantDensity => map.Biome.plantDensity * map.gameConditionManager.AggregatePlantDensityFactor(map);

        public float CurrentWholeMapNumDesiredPlants
        {
            get
            {
                CellRect cellRect = CellRect.WholeMap(map);
                float currentPlantDensity = CurrentPlantDensity;
                float num = 0f;
                foreach (IntVec3 item in cellRect)
                {
                    num += GetDesiredPlantsCountAt(item, item, currentPlantDensity);
                }
                return num;
            }
        }

        public int CurrentWholeMapNumNonZeroFertilityCells
        {
            get
            {
                CellRect cellRect = CellRect.WholeMap(map);
                int num = 0;
                foreach (IntVec3 item in cellRect)
                {
                    if (item.GetTerrain(map).fertility > 0f)
                    {
                        num++;
                    }
                }
                return num;
            }
        }

        private void CacheGardenNumDesiredPlants()
        {
            if (!hasGardenNumDesiredPlantsCalculated)
            {
                calculatedGardenNumDesiredPlants = CurrentWholeMapNumDesiredPlants;
                calculatedGardenNumNonZeroFertilityCells = CurrentWholeMapNumNonZeroFertilityCells;
                hasGardenNumDesiredPlantsCalculated = true;
            }
        }

        public void GardenTick()
        {
            int area = cells.Count;
            int num = Mathf.CeilToInt((float)area * 0.0001f);
            float currentPlantDensity = CurrentPlantDensity;
            CacheGardenNumDesiredPlants();
            int num2 = Mathf.CeilToInt(10000f);
            float cachedChanceFromDensity = CachedChanceFromDensity;
            for (int i = 0; i < num; i++)
            {
                if (cycleIndex >= area)
                {
                    calculatedGardenNumDesiredPlants = calculatedGardenNumDesiredPlantsTmp;
                    calculatedGardenNumDesiredPlantsTmp = 0f;
                    calculatedGardenNumNonZeroFertilityCells = calculatedGardenNumNonZeroFertilityCellsTmp;
                    calculatedGardenNumNonZeroFertilityCellsTmp = 0;
                    cycleIndex = 0;
                }
                IntVec3 intVec = map.cellsInRandomOrder.Get(cycleIndex);
                calculatedGardenNumDesiredPlantsTmp += GetDesiredPlantsCountAt(intVec, intVec, currentPlantDensity);
                if (intVec.GetTerrain(map).fertility > 0f)
                {
                    calculatedGardenNumNonZeroFertilityCellsTmp++;
                }
                float mtb = (GoodRoofForCavePlant(intVec) ? 130f : map.Biome.wildPlantRegrowDays);
                if (Rand.Chance(cachedChanceFromDensity) && Rand.MTBEventOccurs(mtb, 60000f, num2) && CanRegrowAt(intVec))
                {
                    //CheckSpawnWildPlantAt(intVec, currentPlantDensity, calculatedWholeMapNumDesiredPlants);
                }
                cycleIndex++;
            }
        }


        private bool CanRegrowAt(IntVec3 c)
        {
            if (c.GetTemperature(map) > 0f)
            {
                if (c.Roofed(map))
                {
                    return GoodRoofForCavePlant(c);
                }
                return true;
            }
            return false;
        }

        public float GetDesiredPlantsCountAt(IntVec3 c, IntVec3 forCell, float plantDensity)
        {
            return Mathf.Min(GetBaseDesiredPlantsCountAt(c) * plantDensity * forCell.GetTerrain(map).fertility, 1f);
        }

        public float GetBaseDesiredPlantsCountAt(IntVec3 c)
        {
            float num = c.GetTerrain(map).fertility;
            if (GoodRoofForCavePlant(c))
            {
                num *= 0.5f;
            }
            return num;
        }
        private bool GoodRoofForCavePlant(IntVec3 c)
        {
            return c.GetRoof(map)?.isNatural ?? false;
        }

        public void AddCell(IntVec3 cell)
        {
            cells.Add(cell);
        }

        public void RegisterFlora(TiberiumPlant plant)
        {
            tiberiumFlora.Add(plant);
        }

        public void DeregisterFlora(TiberiumPlant plant)
        {
            tiberiumFlora.Remove(plant);
        }

        public void GrowFlora(float pct)
        {
            tiberiumFlora.ForEach(f => f.Growth += pct);
        }
    }
}

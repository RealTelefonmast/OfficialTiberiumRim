using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class TiberiumProperties
    {
        // 
        public IntRange entityDamage = new IntRange(0, 1);

        public IntRange buildingDamage = new IntRange(0, 1);

        public bool isFlesh = false;

        // Values
        public const int MaxMaxMeshCount = 25;

        public int maxMeshCount = 1;

        public float reproduceMtbDays = 1f;

        public float reproduceRadius = 1f;

        public float growDays = 2f;

        public FloatRange visualSizeRange = new FloatRange(0.9f, 1.1f);

        public float maxHarvestValue = 0f;

        public float topWindExposure = 0f;

        public float harvestTimeSec = 3;

        public bool canBeInhibited = true;

        public bool corruptsWater = false;

        public bool corruptsSoil = false;

        public bool corruptsSand = false;

        public bool corruptsStone = false;

        public bool corruptsIce = false;

        // Strings
        public IEnumerable<string> ConfigErrors()
        {
            if (this.maxMeshCount > 25)
            {
                yield return "maxMeshCount > MaxMaxMeshCount";
            }
            yield break;
        }

    }
}

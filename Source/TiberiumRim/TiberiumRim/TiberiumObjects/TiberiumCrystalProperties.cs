using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class TiberiumCrystalProperties
    {
        //These are the main properties for Tiberium Crystals
        public TiberiumValueType type = TiberiumValueType.None;
        public TiberiumConsistence consistence = TiberiumConsistence.Plantlike;
        public IntRange entityDamage = new IntRange(0, 5);
        public IntRange buildingDamage = new IntRange(0, 5);
        public FloatRange sizeRange = new FloatRange(1f, 1f);
        public float plantMutationChance = 0.5f;
        public float minTemperature = -30f;
        public float harvestValue = 0f;
        public float harvestTime = 10f;
        public float spreadRadius = 1f;
        public float reproduceDays = 1f;
        public float growDays = 1f;
        public bool dependsOnProducer = false;
        public bool canBeInhibited = true;
        public bool infects = true;
        public bool radiates = true;

        public int MeshCount = 1;
    }


}

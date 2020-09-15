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
        public IntRange deteriorationDamage = new IntRange(0, 5);
        public float plantMutationChance = 0.5f;
        public float rootNodeChance = 0.06f;
        public float minTemperature = -30f;
        public float harvestValue = 0f;


        public float spreadRadius = 1f;
        public float reproduceDays = 1f;
        public float growDays = 1f;
        public bool needsParent = true;
        public bool dependsOnProducer = false;
        public bool canBeInhibited = true;
        public bool infects = true;
        public bool radiates = true;

        public FloatRange sizeRange = new FloatRange(1f, 1f);
        public int MeshCount = 1;
    }


}

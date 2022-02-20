using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Comp_ANS_Filter : Comp_AtmosphericNetworkStructure
    {
        public NetworkComponent AtmosphericComp => this[TiberiumDefOf.AtmosphericNetwork];
        public NetworkComponent ProcessingComp => this[TiberiumDefOf.TiberiumNetwork];

        public override float[] OpacityFloats => new float[] { Alpha };

        public SimpleCurve FlickerCurve = new SimpleCurve()
        {
            new (0, 0.4f),
            new (0.6f, 0.6f),
            new (0.75f, 1),
            new (0.8f, 0.6f),
            new (1, 0.4f)
        };

        private bool ShouldProcess => !AtmosphericComp.Container.Empty && !ProcessingComp.Container.CapacityFull;
        private float Alpha => AtmosphericComp.IsReceiving ? FlickerCurve.Evaluate((curAnimTick / (float) curAnimLength)) : 0.4f;

        private int curAnimLength;
        private int curAnimTick;
        private IntRange animationRange = new IntRange(25, 45);
        
        public override void CompTick()
        {
            base.CompTick();
            if (curAnimTick < curAnimLength)
            {
                curAnimTick++;
            }

            if (curAnimTick == curAnimLength)
            {
                curAnimTick = 0;
                curAnimLength = animationRange.RandomInRange;
            }
        }

        public override bool AcceptsValue(NetworkValueDef value)
        {
            return !ProcessingComp.Container.CapacityFull;
        }

        protected override void NetworkTickCustom(bool isPowered)
        {
            if (!ShouldProcess) return;
            if (AtmosphericComp.Container.TryRemoveValue(TiberiumDefOf.TibPollution, 10, out float actualValue))
            {
                ProcessingComp.Container.TryAddValue(TiberiumDefOf.TibSludge, actualValue * 0.125f, out _);
            }
        }
    }
}

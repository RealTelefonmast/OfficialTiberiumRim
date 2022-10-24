﻿using System;
using TAE;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Comp_ANS_TiberiumFilter : Comp_AtmosphericNetworkStructure
    {
        private int curAnimLength;
        private int ticksLeft;
        private IntRange animationRange = new(15, 45);

        public NetworkSubPart AtmosphericComp => this[TiberiumDefOf.AtmosphericNetwork];
        public NetworkSubPart ProcessingComp => this[TiberiumDefOf.TiberiumNetwork];

        public SimpleCurve FlickerCurve = new SimpleCurve()
        {
            new (0, 0.4f),
            new (0.6f, 0.6f),
            new (0.75f, 1),
            new (0.8f, 0.6f),
            new (1, 0.4f)
        };

        private bool ShouldProcess => !AtmosphericComp.Container.Empty && !ProcessingComp.Container.Full;
        private float Alpha => ticksLeft > 0 ? FlickerCurve.Evaluate(((curAnimLength - ticksLeft) / (float)curAnimLength)) : 0.4f;

        public override Vector3? FX_GetDrawPositionAt(int index)
        {
            return index switch
            {
                0 => parent.DrawPos,
                _ => null
            };
        }

        public override Color? FX_GetColorAt(int index)
        {
            return index switch
            {
                0 => Color.white,
                _ => null
            };
        }

        public override float FX_GetOpacityAt(int index)
        {
            return index switch
            {
                0 => Alpha,
                _ => 1
            };
        }

        //
        public override void CompTick()
        {
            base.CompTick();
            if (ticksLeft > 0)
                ticksLeft--;
        }

        public override void Notify_ReceivedValue()
        {
            if (ticksLeft > 0) return;
            ticksLeft = curAnimLength = animationRange.RandomInRange;
        }

        public override bool AcceptsValue(NetworkValueDef value)
        {
            return !AtmosphericComp.Container.Full;
        }

        public override void NetworkPostTick(bool isPowered)
        {
            if (!ShouldProcess) return;
            if (AtmosphericComp.Container.TryRemoveValue(TiberiumDefOf.TibPollution, 10, out float actualValue))
            {
                ProcessingComp.Container.TryAddValue(TiberiumDefOf.TibSludge, actualValue * 0.125f, out _);
            }
        }

        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra();
        }
    }
}
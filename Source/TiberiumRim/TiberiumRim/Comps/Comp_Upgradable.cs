using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using StoryFramework;

namespace TiberiumRim
{
    public class Comp_Upgradable : ThingComp
    {
        public CompProperties_Upgrade Props
        {
            get
            {
                return this.props as CompProperties_Upgrade;
            }
        }

        public override void CompTick()
        {
            if (Props.requisites != null)
            {
                if (!Props.requisites.IsFulfilled())
                {
                    return;
                }
            }
            base.CompTick();
        }

        public override void CompTickRare()
        {
            if (Props.requisites != null)
            {
                if (!Props.requisites.IsFulfilled())
                {
                    return;
                }
            }
            base.CompTickRare();
        }
    }

    public class CompProperties_Upgrade : CompProperties
    {
        public Requisites requisites;
    }
}
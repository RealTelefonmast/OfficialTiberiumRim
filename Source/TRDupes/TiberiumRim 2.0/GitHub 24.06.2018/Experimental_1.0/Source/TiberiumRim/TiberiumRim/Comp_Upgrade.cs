using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Comp_Upgrade : ThingComp
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
            if (Props.preRequisite != null)
            {
                if (!Props.preRequisite.IsFinished)
                {
                    return;
                }
            }
            base.CompTick();
        }

        public override void CompTickRare()
        {
            if (Props.preRequisite != null)
            {
                if (!Props.preRequisite.IsFinished)
                {
                    return;
                }
            }
            base.CompTickRare();
        }
    }

    public class CompProperties_Upgrade : CompProperties
    {
        public MissionObjectiveDef preRequisite;
    }
}
